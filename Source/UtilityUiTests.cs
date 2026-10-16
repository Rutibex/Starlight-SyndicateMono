using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int utilityUiChecks;
 void RunUtilityUiChecks(int stage){void Check(bool okay,string name){if(!okay)throw new Exception("UTILITY UI FAIL: "+name);utilityUiChecks++;Console.WriteLine("UI PASS: "+name);}void Click(string text){var hit=ui.Hits.Last(h=>h.Tip.StartsWith(text,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}var w=desktop.First(x=>x.Kind==ViewMode.Utilities);var miner=UtilityCatalog.All.First(d=>d.Kind=="Miner");switch(stage){
 case 0:Check(world.Docked&&UtilityCatalog.All.Length==42,"42 utility profiles in movable window");int x=w.X;TestDrag(new(w.X+90,w.Y+12),new(w.X+110,w.Y+22));Check(w.X==x+20,"Utility title drag");int width=w.Width;TestDrag(new(w.Bounds.Right-5,w.Bounds.Bottom-5),new(w.Bounds.Right+15,w.Bounds.Bottom+5));Check(w.Width==width+20,"Utility resize");utilityPage=1;break;
 case 1:Click("Utility catalog / "+miner.Id);break;
 case 2:Check(utilityType==miner.Id,"Utility catalog selection");Click("FIT SELECTED");break;
 case 3:Check(world.Player.Utilities[0].ModuleId==miner.Id&&world.ItemCount(miner.Id,true)==2,"Utility fitting consumes stock");Click("OFFLINE");break;
 case 4:Check(!world.Player.Utilities[0].Online,"Utility offline button");Click("ONLINE");Click("ACTIVATE");break;
 case 5:Check(world.Player.Utilities[0].Online&&!world.Player.Utilities[0].Active,"Station activation refused");Click("UTILITY RANGE");break;
 case 6:Check(world.Grid=="utility_range"&&!world.Docked,"Utility range button");world.SelectedId=world.LockedId="ut_resource_ore";world.Player.Position=Vector3.Zero;Click("ACTIVATE");break;
 case 7:Check(world.Player.Utilities[0].Active,"Harvest activation button");Click("FINISH TRIAL CYCLE");break;
 case 8:Check(world.Player.Ore>0,"Harvest cargo appears in UI workflow");Click("STOP");Click("PROBE SCANNER");break;
 case 9:Check(utilityTab==1,"Probe scanner tab");float range=world.ProbeRange;Click("NARROW RANGE");Check(world.ProbeRange==range/2,"Probe range control");Click("DEPTH +");Check(world.ProbeCenter.Y>.2f,"Probe 3D depth control");Click("HACKING");break;
 case 10:Check(utilityTab==2,"Hacking board tab");Click("INDUSTRY");break;
 case 11:Check(utilityTab==3,"Industry and compression tab");Click("REPORTS");break;
 case 12:Check(utilityTab==4,"Inspection reports tab");Click("FITTING");break;
 case 13:Click("DOCK");break;
 case 14:Check(world.Docked,"Utility range docking");world.FittingUtilitySlot=1;EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;inventoryScroll=Array.IndexOf(DemoWorld.InventoryIds.Where(id=>world.ItemCount(id,true)>.001f).ToArray(),miner.Id);break;
 case 15:var item=itemHits.First(h=>h.Id==miner.Id);TestDrag(item.Rect.Center.ToVector2(),new(1160,425));break;
 case 16:Check(world.Player.Utilities[1].ModuleId==miner.Id,"Drag inventory to utility socket");desktop.RemoveAll(x=>x.Kind==ViewMode.Inventory);break;
 case 17:TestPointer(new(w.X+w.Width-90,w.Y+405),right:ButtonState.Pressed);TestPointer(new(w.X+w.Width-90,w.Y+405));Check(context&&popupActions.Any(a=>a.Label=="Toggle online"),"Utility context menu");break;
 case 18:Click("Toggle online");break;
 case 19:Check(!world.Player.Utilities[1].Online,"Context online control");world.Save();var saved=new DemoWorld();Check(saved.Load()&&saved.Player.Utilities[1].ModuleId==miner.Id&&!saved.Player.Utilities[1].Online,"UI utility fit save/load");break;
 case 20:world.FitUtility(UtilityCatalog.All.First(d=>d.Kind=="Data").Id,true,6);world.EnterUtilityRange();world.SelectedId=world.LockedId="ut_Data";world.Player.Position=world.Locked!.Position-new Vector3(100,0,0);world.ToggleUtility(6);utilityTab=2;world.UtilityHackState!.Nodes[1]=new(){Kind="Cache"};break;
 case 21:var node=ui.Hits.First(h=>h.Owner==ViewMode.Utilities.ToString()&&h.Tip=="Unexplored node");TestClick(node.Rect.Center.ToVector2());break;
 case 22:Check(world.UtilityHackState!.Nodes[1].Cleared&&world.UtilityHackState.Tools.Count==1,"Graphical hacking frontier cache");world.UtilityHackState.Tools[0]="Repair";world.UtilityHackState.Coherence-=10;break;
 case 23:Click("Repair / use on selected node");break;
 case 24:Check(world.UtilityHackState!.Coherence==world.UtilityHackState.MaxCoherence,"Graphical virus utility use");Click("ABORT LINK");Check(world.UtilityHackState.Complete&&!world.UtilityHackState.Won,"Graphical hacking abort");break;
 case 26:Console.WriteLine($"{utilityUiChecks} exploration graphical interaction checks passed.");break;}
 oldMouse=Mouse.GetState();}
}
