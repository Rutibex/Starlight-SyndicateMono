using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void DrawImportedStation44()
 {
  int x=1260,y=18,w=325;var s=world.CurrentStation44;if(s==null)return;
  ui.Panel(new(x,y,w,826),"STATION SERVICES");DrawCorpLogo38(x+128,y+40,68);ui.Center(ShortInfo(Stations44.Name(world.StationCorporation),31),x+w/2,y+118,White,16);ui.Center(ShortInfo(Stations44.Name(world.StationFaction),38),x+w/2,y+143,Cyan,11);
  ui.Button(new(x+12,y+175,197,32),"Station information",()=>ShowInformation("station44/"+s.Id),"Pass44 station information");ui.Button(new(x+218,y+175,95,32),"LP store",()=>{commanderTabD=5;OpenCommanderD();});
  ui.Button(new(x+12,y+217,145,32),"Item hangar",()=>{showHangar=true;EnsureWindow(ViewMode.Inventory);},"Pass44 independent hangar");ui.Button(new(x+168,y+217,145,32),"Ship fitting",()=>EnsureWindow(ViewMode.Fitting),"Pass44 fitting service");
  ui.Button(new(x+12,y+259,145,32),"Reprocessing",()=>EnsureWindow(ViewMode.Reprocessing),"Pass44 reprocessing service");ui.Button(new(x+168,y+259,145,32),"Repair",RepairStation38,"Pass44 repair service");
  ui.Button(new(x+12,y+303,w-24,43),"UNDOCK [U]",world.Undock,"Pass44 undock");
  ui.Button(new(x+12,y+360,145,29),"Agent finder",()=>EnsureWindow(ViewMode.Agents),"Pass44 agent finder");ui.Button(new(x+168,y+360,145,29),"Standings",()=>EnsureWindow(ViewMode.Standings));
  ui.Button(new(x+12,y+395,145,29),"Market",()=>{world.EnsureLocationMarket(s.Id);EnsureWindow(ViewMode.Market);});ui.Button(new(x+168,y+395,145,29),"Industry",()=>EnsureWindow(ViewMode.Industry));
  ui.Button(new(x+12,y+735,95,26),"Wait 1h",()=>world.WaitDocked(1));ui.Button(new(x+115,y+735,95,26),"Clones",()=>{specialTab=2;EnsureWindow(ViewMode.Special);});ui.Button(new(x+218,y+735,95,26),world.SimulationPaused?"Resume":"Pause",()=>world.SimulationPaused=!world.SimulationPaused);
  ui.Text("LOCAL MISSION CONTACTS",x+13,y+435,Cyan,12);var agents=world.WorldAgents44.Where(a=>a.Station==s.Id).ToArray();stationAgentPage38=Math.Clamp(stationAgentPage38,0,Math.Max(0,agents.Length-4));int yy=y+458;
  foreach(var a in agents.Skip(stationAgentPage38).Take(4)){var id=a.Id;ui.Button(new(x+12,yy,w-24,42),ShortInfo(a.Name,31)+" / L"+a.Level,()=>AgentOpen38(a),"Pass44 contact / "+id);ui.Text(a.Division+" / "+(world.CanUseAgent38(a)?"Available":"Standing required"),x+20,yy+29,Muted,10);yy+=48;}
  ui.Button(new(x+12,y+647,145,27),"Previous",()=>stationAgentPage38=Math.Max(0,stationAgentPage38-4));ui.Button(new(x+168,y+647,145,27),"Next",()=>stationAgentPage38+=4);
  ui.Wrap("Basic L1-L4 contacts offer authored adaptations. Special and in-space agents remain indexed in Station information.",x+15,y+688,w-30,Muted,12);
  ui.Text($"Reprocessing {s.ReprocessingEfficiency:P0} / base take {s.ReprocessingTake:P0}",x+15,y+766,Cyan,12);ui.Text("Local storage / this station only",x+15,y+795,Muted,11);
 }
}
