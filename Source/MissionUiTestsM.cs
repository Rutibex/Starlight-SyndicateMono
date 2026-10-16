using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int checksUiM;string fixtureMissionM="";
 void SetupCaptureM(){world.NewGame(0);world.SimulationPaused=true;world.WalletFunds=2000000;world.Player.CargoMax=100000;desktop.Clear();EnsureWindow(ViewMode.Opportunities);opportunitiesFilterL=captureScene=="stage-m-chains"?"Chains":captureScene=="stage-m-coverage"?"Coverage":"Missions";if(captureScene=="stage-m-space"){world.RequestReviewedM("m_cargo4");if(world.AgentsState.Missions.Count==0){foreach(var a in world.WorldAgents44.Where(a=>a.Station==world.StationKey44))world.AgentsState.Standings[a.Id]=8;world.RequestReviewedM("m_cargo4");}var m=world.AgentsState.Missions.Last();world.AcceptMission38(m.Id);world.Undock();world.EnterGrid(m.WarpId,false);world.AgentsState.TrackedMissionL=m.Id;desktop.Clear();OpenJournalL(m.Id);} }
 void MUiCheck(bool ok,string label){UiCheck(ok,"M / "+label);checksUiM++;}
 void RunUiChecksM(int frame)
 {
  if(captureScene!="stage-m-ui")return;
  switch(frame){
   case 1:MUiCheck(ui.Hits.Any(h=>h.Tip=="M / request / m_cargo1"),"mission catalogue offers explicit reviewed variant");ClickL("M / level / 2");MUiCheck(missionLevelM==2,"level filter uses pointer control");break;
   case 2:ClickL("M / level / 1");ClickL("M / division / Security");break;
   case 3:ClickL("M / select / m_cargo1");break;
   case 4:ClickL("M / request / m_cargo1");MUiCheck(desktop.Last().Kind==ViewMode.ConversationL,"catalogue request opens real agent conversation");fixtureMissionM=world.AgentsState.Missions.Last().Id;break;
   case 5:ClickL("L / accept");MUiCheck(world.AgentsState.Missions.Last().State=="Accepted","reviewed contract accepted by pointer");ClickL("L / open mission journal");break;
   case 6:ClickL("L / track");MUiCheck(world.AgentsState.TrackedMissionL==fixtureMissionM,"tracker stores reviewed contract identity");world.Undock();break;
   case 7:ClickL("L / travel");MUiCheck(world.Transit!=null||world.InWarp,"reviewed journal starts real travel");world.Transit=null;world.EnterGrid("mission:"+fixtureMissionM,false);break;
   case 8:MUiCheck(!world.Targets.Any(t=>t.Kind==TargetKind.Raider)&&world.Targets.Any(t=>t.Id.EndsWith(":objective")),"warehouse present before combat");ClickL("L / objective actions");break;
   case 9:ClickL("Collect dispatch");MUiCheck(world.AgentsState.Missions.Last().Cargo==0,"remote collection rejected through UI");world.Player.Position=world.Targets.First(t=>t.Id.EndsWith(":objective")).Position;ClickL("L / objective actions");break;
   case 10:ClickL("Collect dispatch");var m=world.AgentsState.Missions.Last();MUiCheck(m.ObjectiveDone&&m.Cargo==1&&world.Targets.Any(t=>t.Kind==TargetKind.Raider),"physical pickup completes objective and spawns enemies");OpenInventoryLocationK("cargo");break;
   case 11:MUiCheck(itemHits.Any(h=>h.Id=="mission-cargo:"+fixtureMissionM),"retrieved freight visible in unified inventory");Close();DockFixtureL();OpenConversationL(world.AgentsState.Missions.Last().Contact);break;
   case 12:ClickL("L / complete");MUiCheck(world.AgentsState.Missions.Last().State=="Completed","blitz settles through conversation");desktop.Clear();EnsureWindow(ViewMode.Opportunities);opportunitiesFilterL="Chains";break;
   case 13:ClickL("M / chain / horizons");break;
   case 14:ClickL("M / chain start");MUiCheck(world.AgentsState.ChainsM.Count==1&&desktop.Last().Kind==ViewMode.ConversationL,"epic adaptation starts via chain card");break;
   case 15:ClickL("L / accept");break;
   case 16:ClickL("L / quit");break;
   case 17:ClickL("Abandon");MUiCheck(world.AgentsState.ChainsM.Last().State=="Failed","abandonment seals chain failure");desktop.Clear();EnsureWindow(ViewMode.Opportunities);opportunitiesFilterL="Chains";break;
   case 18:ClickL("M / chain start");MUiCheck(world.AgentsState.ChainsM.Count==1&&agentNotice38.Contains("locked"),"repeat lock blocks second chain start through UI");ClickL("L / activity filter / Coverage");break;
   case 19:MUiCheck(ui.Hits.Any(h=>h.Tip=="M / level / 5"),"coverage catalogue exposes explicit level five filter");ClickL("M / level / 5");break;
   case 20:MUiCheck(missionLevelM==5,"level five reference gap remains explicit");ClickL("M / level / 0");ClickL("Search / Ctrl+A, Ctrl+C, Ctrl+V, Home, End");JTextInput('c');JTextInput('a');editJ.Focus=false;editCommitK=null;break;
   case 21:MUiCheck(desktop.Last().QueryJ=="ca","catalogue uses shared search editing");Console.WriteLine($"{checksUiM} Stage M UI assertions passed.");break;
  }
 }
}
