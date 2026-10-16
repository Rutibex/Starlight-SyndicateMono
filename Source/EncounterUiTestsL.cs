using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int checksUiL;string fixtureMissionL="";decimal fixtureMoneyL;
 void SetupCaptureL()
 {
  SkillCatalog.VerificationAllV=false;world.NewGame(0);world.SimulationPaused=true;world.Screen=ViewMode.Station;world.Grid="station";world.WalletFunds=100000;desktop.Clear();world.RequestRelayL();var m=world.AgentsState.Missions.Last();fixtureMissionL=m.Id;
  if(captureScene=="stage-l-opportunities"){EnsureWindow(ViewMode.Opportunities);return;}
  if(captureScene is "stage-l-journal" or "stage-l-space"){world.AcceptMission38(m.Id);world.AgentsState.TrackedMissionL=m.Id;if(captureScene=="stage-l-space"){world.Undock();world.EnterGrid(m.WarpId,false);}else OpenJournalL(m.Id);return;}
  if(captureScene=="stage-l-rewards"){world.AcceptMission38(m.Id);m.Cargo=1;m.ObjectiveDone=true;world.CompleteMission38(m.Id);journalFilterL="History";OpenJournalL(m.Id);desktop.Last().DetailTabL=2;return;}
  OpenConversationL(m.Contact);fixtureMoneyL=world.Balance;
 }
 void LUiCheck(bool ok,string label){UiCheck(ok,"L / "+label);checksUiL++;}
 void ClickL(string tip){var h=ui.Hits.Last(h=>h.Tip==tip);var at=h.Rect.Center.ToVector2();foreach(var left in new[]{Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Pressed,Microsoft.Xna.Framework.Input.ButtonState.Released}){mouse=at;var state=new Microsoft.Xna.Framework.Input.MouseState((int)at.X,(int)at.Y,0,left,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released);bool consumed=DesktopInput(state,new Microsoft.Xna.Framework.Input.KeyboardState());if(!consumed&&left==Microsoft.Xna.Framework.Input.ButtonState.Released&&oldMouse.LeftButton==Microsoft.Xna.Framework.Input.ButtonState.Pressed)ui.Hits.LastOrDefault(hit=>hit.Rect.Contains(at.ToPoint()))?.Action();oldMouse=state;}}
 void DockFixtureL(){world.Transit=null;world.EnterGrid("station",false);var station=world.Targets.First(t=>t.Kind==TargetKind.Station);world.Player.Position=station.Position;world.SelectedId=station.Id;world.Dock();}
 void RunUiChecksL(int frame)
 {
  if(captureScene!="stage-l-ui")return;var m=world.AgentsState.Missions.First(m=>m.Id==fixtureMissionL);
  switch(frame){
   case 1:LUiCheck(desktop.Last().Kind==ViewMode.ConversationL&&ui.Hits.Any(h=>h.Tip=="L / accept")&&ui.Hits.Any(h=>h.Tip=="L / decline"),"dedicated conversation renders offer actions");ClickL("L / delay");LUiCheck(m.State=="Offered","delay preserves the offer");ClickL("L / accept");LUiCheck(m.State=="Accepted","pointer accepts the actual agent contract");break;
   case 2:ClickL("L / open mission journal");LUiCheck(desktop.Last().Kind==ViewMode.JournalL&&desktop.Last().SubjectL==m.Id,"conversation opens its own mission in the journal");break;
   case 3:ClickL("L / track");LUiCheck(world.AgentsState.TrackedMissionL==m.Id,"track control stores a durable mission identity");world.Waypoints.Add("umbra");ClickL("L / route");LUiCheck(world.Waypoints.Count==0&&m.SiteSystem==world.SystemId,"current-system route clears stale waypoints without adding a redundant jump");break;
   case 4:ClickL("L / detail tab / Briefing");LUiCheck(desktop.Last().DetailTabL==1,"journal detail tab owns window state");var w=desktop.Last();int px=w.X;TestDrag(new(w.X+90,w.Y+12),new(w.X+108,w.Y+22));LUiCheck(w.X==px+18&&w.SubjectL==m.Id,"journal dragging retains selected contract");ClickL("L / conversation");break;
   case 5:ClickL("L / complete");LUiCheck(m.State=="Accepted"&&world.Balance==fixtureMoneyL,"missing objective and freight block UI settlement");ClickL("L / open mission journal");break;
   case 6:ClickL("Station / undock");LUiCheck(!world.Docked,"station sidebar undocks without losing journal");break;
   case 7:ClickL("L / travel");LUiCheck(world.Transit!=null||world.InWarp,"journal travel begins real mission warp");world.Transit=null;world.EnterGrid(m.WarpId,false);break;
   case 8:LUiCheck(ui.Hits.Any(h=>h.Tip=="L / tracker journal"),"in-space objective tracker renders navigation links");ClickL("L / gates");break;
   case 9:ClickL("Activate forward gate");LUiCheck(m.Pocket==0&&agentNotice38.Contains("locked"),"gate popup enforces the commander trigger");world.DamageMissionAudit38(world.Targets.First(t=>t.Id==m.WarpId+":0:0:0"));LUiCheck(!m.ObjectiveDone&&world.Targets.Any(t=>t.Kind==TargetKind.Raider&&t.Alive),"blitz trigger does not silently complete freight objective");ClickL("L / gates");break;
   case 10:ClickL("Approach forward gate");LUiCheck(world.Player.OrderTarget==m.WarpId+":gate","gate approach issues an actual navigation order");world.Player.Position=world.Targets.First(t=>t.Id==m.WarpId+":gate").Position;ClickL("L / gates");break;
   case 11:ClickL("Activate forward gate");LUiCheck(m.Pocket==1,"gate popup advances the real encounter pocket");ClickL("L / objective actions");break;
   case 12:ClickL("Collect dispatch");LUiCheck(m.Cargo==0&&agentNotice38.Contains("Approach"),"objective popup rejects remote collection");world.Player.Position=world.Targets.First(t=>t.Id==m.WarpId+":objective").Position;ClickL("L / objective actions");break;
   case 13:ClickL("Collect dispatch");LUiCheck(m.Cargo==1&&m.ObjectiveDone,"objective popup collects physical contract cargo once");OpenInventoryLocationK("cargo");break;
   case 14:LUiCheck(itemHits.Any(h=>h.Id=="mission-cargo:"+m.Id&&h.QuantityK==1),"collected dispatch is visible in unified cargo inventory");Close();DockFixtureL();OpenConversationL(m.Contact);fixtureMoneyL=world.Balance;break;
   case 15:ClickL("L / complete");LUiCheck(m.State=="Completed"&&world.Balance==fixtureMoneyL+m.Reward+m.Bonus,"agent completion settles actual money and timed bonus");LUiCheck(m.Cargo==0&&m.GraphL!.Settlement!=null,"turn-in consumes dispatch and stores settlement receipt");OpenJournalL(m.Id);break;
   case 16:ClickL("L / journal tab / History");LUiCheck(ui.Hits.Any(h=>h.Tip=="L / detail tab / Rewards"),"history retains completed contract details");ClickL("L / detail tab / Rewards");break;
   case 17:LUiCheck(desktop.Last().DetailTabL==2&&m.GraphL!.Settlement!.Money.ContainsKey("player"),"rewards tab uses authoritative settlement shares");SaveDesktop();desktop.Clear();RestoreLayoutJ();LUiCheck(desktop.Any(w=>w.Kind==ViewMode.JournalL&&w.SubjectL==m.Id&&w.DetailTabL==2),"journal contract and reward tab survive desktop restoration");desktop.Clear();EnsureWindow(ViewMode.Opportunities);break;
   case 18:LUiCheck(ui.Hits.Any(h=>h.Tip=="L / relay offer")&&ui.Hits.Any(h=>h.Tip.StartsWith("L / site details / ")),"Opportunities exposes agent and authored activity cards");ClickL("L / relay offer");LUiCheck(desktop.Last().Kind==ViewMode.ConversationL,"activity offer opens its agent conversation");fixtureMissionL=world.AgentsState.Missions.Last().Id;break;
   case 19:ClickL("L / accept");LUiCheck(world.AgentsState.Missions.Last().State=="Accepted","second contract acceptance creates its own graph instance");break;
   case 20:ClickL("L / quit");LUiCheck(context&&popupActions.Any(a=>a.Label=="Abandon"),"quit explains standings loss before applying it");break;
   case 21:ClickL("Abandon");LUiCheck(m.State=="Failed"&&m.GraphL!.State=="Failed","abandonment seals contract and graph failure together");OpenJournalL(m.Id);break;
   case 22:ClickL("L / journal tab / History");ClickL("Search / Ctrl+A, Ctrl+C, Ctrl+V, Home, End");JTextInput('r');JTextInput('e');LUiCheck(desktop.Last().QueryJ=="re","journal uses shared searchable text editing");editJ.Focus=false;editCommitK=null;break;
   case 23:LUiCheck(ui.Hits.Any(h=>h.Tip=="L / select mission / "+m.Id),"failed mission remains discoverable in history");Console.WriteLine($"{checksUiL} Stage L UI assertions passed.");break;
  }
 }
}
