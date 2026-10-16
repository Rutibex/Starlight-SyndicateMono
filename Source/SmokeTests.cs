using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;

internal static class SmokeTests
{
    public static void Run()
    {
        int checks=0;
        void Check(bool ok,string message){if(!ok)throw new Exception("FAIL: "+message);checks++;Console.WriteLine("PASS: "+message);}
        var seen=new HashSet<string>{"aster"};
        for(int i=0;i<8;i++)foreach(var s in DemoWorld.Systems.Where(s=>seen.Contains(s.Id)).ToArray())foreach(var link in s.Links)seen.Add(link);
        Check(seen.Count==DemoWorld.Systems.Length,"All systems including the unregulated rim are connected");
        Check(DemoWorld.Systems.All(s=>s.Links.All(l=>DemoWorld.Systems.Single(t=>t.Id==l).Links.Contains(s.Id))),"Gate links are reciprocal");
        for(int origin=0;origin<3;origin++)
        {
            var start=new DemoWorld();start.NewGame(origin);Check(start.Origin==origin&&DemoWorld.Systems.Any(s=>s.Id==start.SystemId),"Origin "+origin+" initializes");
        }
        var w=new DemoWorld();w.NewGame(0);w.EnterGrid("station");
        var begin=w.Player.Position;w.Player.ManualPoint=begin+new Vector3(1500,2000,-2500);w.Player.Order="manual";
        for(int i=0;i<240;i++)w.Update(1f/60);
        Check(w.Player.Position.Y>begin.Y+100&&w.Player.Position.Z<begin.Z-100,"Flight advances vertically and in depth");
        w.EnterGrid("belt");w.Select("raider");w.LockTarget();w.ToggleModule(1);w.ToggleModule(3);
        for(int i=0;i<3600;i++)w.Update(1f/60);
        Check(!w.Targets.Any(t=>t.Kind==TargetKind.Raider&&t.Alive),"Rail combat destroys hostile");
        Check(w.Player.Hull>0,"Player survives encounter");
        w.Player.UtilityFit="miner";w.Select("rock");w.LockTarget();w.ToggleMining();
        for(int i=0;i<600;i++)w.Update(1f/60);
        Check(w.Player.Ore>0,"Mining cycles transfer ore into cargo");
        float cash=w.Credits;w.Screen=ViewMode.Station;w.SellOre();Check(w.Credits>cash&&w.Player.Ore==0,"Local sale pays for mined cargo");
        w.SystemId="umbra";var probes=new ProbePuzzle();probes.Reset();probes.Analyze(w);
        for(int i=0;i<5&&!w.SignalResolved;i++){probes.Center=probes.Estimate;probes.AdjustRange(true);probes.Analyze(w);}
        Check(w.SignalResolved,"Probe refinement resolves JoyOS signature");
        var hack=new HackPuzzle();hack.Reset();var queue=new Queue<Point>();var parents=new Dictionary<Point,Point>();var first=new Point(0,0);var goal=new Point(5,4);
        queue.Enqueue(first);parents[first]=first;
        while(queue.Count>0){var p=queue.Dequeue();foreach(var n in HackPuzzle.Neighbors(p.X,p.Y))if(!parents.ContainsKey(n)&&hack.Nodes[n.X,n.Y]!=1){parents[n]=p;queue.Enqueue(n);}}
        Check(parents.ContainsKey(goal),"Hacking board has an attainable core");
        var path=new List<Point>();for(var p=goal;p!=first;p=parents[p])path.Add(p);path.Reverse();foreach(var p in path)hack.Click(p.X,p.Y,w);
        Check(hack.Won&&w.SignalHacked,"Hex-node exploration recovers the fragment");
        for(int choice=1;choice<=3;choice++){var story=new DemoWorld();story.NewGame(0);story.SignalHacked=true;story.Decide(choice);Check(story.EndingSet&&story.Ending.Length>0,"Story outcome "+choice+" resolves");}
        var deadline=new DemoWorld();deadline.NewGame(0);deadline.AdvanceHours(200);Check(deadline.Quarantine&&!deadline.EndingSet,"Day-nine deadline triggers quarantine");
        var inv=new DemoWorld();inv.NewGame(0);
        Check(inv.Docked&&inv.ItemCount("miner",true)==1,"Starter module stock is station-local");
        Check(inv.TransferItem("ammo",true,6)&&inv.Player.Missiles==20&&inv.ItemCount("ammo",true)==6,"Stack transfer conserves ammunition");
        Check(!inv.TransferItem("ammo",true,99)&&inv.Player.Missiles==20,"Unavailable quantity is rejected atomically");
        inv.Player.Ore=inv.Player.CargoMax-inv.Player.Missiles*.1f;
        Check(!inv.TransferItem("miner",true,1)&&inv.ItemCount("miner",true)==1,"Full cargo rejects transfer without item loss");
        inv.Player.Ore=0;
        Check(inv.FitItem("miner",true)&&inv.Player.UtilityFit=="miner"&&inv.ItemCount("analyzer",true)==1&&inv.ItemCount("miner",true)==0,"Fitting swaps owned modules without duplication");
        inv.Undock();Check(!inv.Docked&&!inv.TransferItem("ammo",true,1),"Undocking blocks remote hangar transfers");
        inv.Dock();Check(inv.DockPending&&!inv.Docked,"Docking outside perimeter starts an approach");
        for(int i=0;i<1800&&!inv.Docked;i++)inv.Update(1f/60);
        Check(inv.Docked&&!inv.DockPending,"Approach completes docking inside perimeter");
        inv.SystemId="vale";Check(inv.Hangar.Count==0,"Another station has independent storage");inv.SystemId="aster";
        string temp=Path.Combine(Path.GetTempPath(),"starlight-test-"+Guid.NewGuid().ToString("N"));
        string? previous=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
        try
        {
            Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",temp);
            inv.Save();var inventoryLoad=new DemoWorld();Check(inventoryLoad.Load()&&inventoryLoad.ItemCount("analyzer",true)==1&&inventoryLoad.Player.UtilityFit=="miner","Hangar and fitting persist across save/load");
            w.EnterGrid("signal");w.Player.Position=new Vector3(123,456,789);w.Player.Ore=42;double day=w.Day;w.Save();
            var load=new DemoWorld();Check(load.Load(),"Save file loads successfully");
            Check(load.Player.Position==w.Player.Position&&load.Player.Ore==42&&load.SignalHacked&&load.SignalResolved,"3D position, cargo and story survive save/load");
            Check(load.Day==day,"Loading does not advance campaign time");
        }
        finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",previous);if(Directory.Exists(temp))Directory.Delete(temp,true);}
        Console.WriteLine($"{checks} simulation checks passed.");
    }
}
