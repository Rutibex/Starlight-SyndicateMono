using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int stationUiChecks44;string stationFirst44="",stationSecond44="",stationAgent44="";
 void SetupStationCapture44()
 {
  var group=Stations44.Stations.Where(s=>s.Services.Contains(22)&&s.Faction!="faction:500026").GroupBy(s=>s.System).First(g=>g.Count()>1&&Stations44.Playable.Any(a=>a.Station==g.First().Id&&a.Level==1&&a.Division==AgentDivision38.Distribution));
  stationFirst44=group.First().Id;stationSecond44=group.Skip(1).First().Id;stationAgent44=Stations44.Playable.First(a=>a.Station==stationFirst44&&a.Level==1&&a.Division==AgentDivision38.Distribution).Id;
  world.NewExploration43(int.Parse(group.Key[7..]));world.EnterGrid(stationFirst44,false);world.Player.Position=Vector3.Zero;world.SelectedId=stationFirst44;world.Dock();desktop.Clear();
  if(captureScene=="stations44-info")ShowInformation("station44/"+stationFirst44);
  if(captureScene=="stations44-agents"){AgentOpen38(Stations44.Contacts[stationAgent44]);agentDivision38=2;}
  if(captureScene=="stations44-services"){ShowInformation("station44/"+stationFirst44);desktop.Last().InfoTab="Services";}
  if(captureScene=="stations44-space"){world.Undock();overviewTab=4;}
  if(captureScene=="stations44-inspace"){ShowInformation("agent44/"+Stations44.Agents.First(a=>a.Space.ValueKind==System.Text.Json.JsonValueKind.Object).Id);desktop.Last().InfoTab="Attributes";}
 }
 void RunStationUiChecks44(int stage)
 {
  void Check(bool b,string why){UiCheck(b,"Pass44: "+why);stationUiChecks44++;}
  void Click(string tip){var h=ui.Hits.Last(h=>h.Tip==tip);if(h.Owner.Length==0)h.Action();else TestClick(h.Rect.Center.ToVector2());}
  switch(stage){
   case 0:Check(world.Docked&&world.StationId==stationFirst44,"source station interior");Check(ui.Hits.Any(h=>h.Tip=="Pass44 independent hangar"),"independent hangar control rendered");Click("Pass44 station information");break;
   case 1:Check(desktop.Last().InfoKey=="station44/"+stationFirst44,"station Show Info opens");Click("Info tab / Services");break;
   case 2:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs["Services"].Count>0,"services and dispositions visible");Click("Info tab / Source audit");break;
   case 3:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs["Source audit"].Any(r=>r.Label=="reprocessingEfficiency"),"source property audit retained");Close();Click("Pass44 independent hangar");break;
   case 4:Check(desktop.Last().Kind==ViewMode.Inventory&&showHangar,"source item hangar opens");world.Hangar["miner"]=3;Close();Click("Pass44 contact / "+stationAgent44);break;
   case 5:Check(desktop.Last().Kind==ViewMode.ConversationL&&agentSelected38==stationAgent44,"station-local agent selection");var a=world.WorldAgents44.First(a=>a.Id==stationAgent44);Check(world.RequestMission38(a.Id)=="","mission adapter works from selected source station");Check(world.AgentsState.Missions.Last().Pickup==stationFirst44,"mission pickup uses station ID");Close();Click("Pass44 undock");break;
   case 6:Check(!world.Docked&&world.SolarCelestial==stationFirst44,"undock uses actual source station");world.EnterGrid(stationSecond44,false);world.Player.Position=Vector3.Zero;world.SelectedId=stationSecond44;world.Dock();break;
   case 7:Check(world.Docked&&world.StationId==stationSecond44,"same-system station B reached");Check(!world.Hangar.ContainsKey("miner"),"station B hangar independent");Check(world.RequestMission38(stationAgent44).Contains("Dock"),"agent A unavailable at station B");world.Save();var restored=new DemoWorld();Check(restored.Load(DemoWorld.ImportedSavePath43)&&restored.StationId==stationSecond44,"station B docking persists");world.Undock();world.EnterGrid(stationFirst44,false);world.Player.Position=Vector3.Zero;world.SelectedId=stationFirst44;world.Dock();break;
   case 8:Check(world.Hangar["miner"]==3,"station A stock remains");Click("Pass44 fitting service");break;
   case 9:Check(desktop.Last().Kind==ViewMode.Fitting,"imported fitting window usable");Close();Click("Pass44 agent finder");break;
   case 10:Check(desktop.Last().Kind==ViewMode.Agents,"agent finder usable");Console.WriteLine($"Pass44 station UI: {stationUiChecks44} checks passed.");break;
  }
 }
}
