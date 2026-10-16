namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int batch24Skill,batch24Level,batch24Berths;float batch24Cash;string batch24Selection="";
 void RunShipBatch24UiChecks(int stage){if(stage==0){fleetElite=false;fleetStrategic=false;fleetCruisers=false;}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());fleetDestroyers=false;switch(stage){
 case 0:UiCheck(shipInfoTab==7&&ShipBatch21.ById[world.Player.CombatHull].Batch==24,"Specialist traits displayed");batch24Skill=ShipBatch21.ById[world.Player.CombatHull].Traits.First(t=>t.Skill!=0).Skill;batch24Level=world.Player.HullProficiency.GetValueOrDefault(batch24Skill,1);batch24Cash=world.Credits;Click("Dock and board this hull to train; bonuses apply per level.");break;
 case 1:UiCheck(world.Player.HullProficiency[batch24Skill]==batch24Level+1&&world.Credits==batch24Cash-250*(batch24Level+1)*(batch24Level+1),"Specialist training click");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetAdvanced=false;fleetBrowser=fleetCurrentBatch=fleetSpecialists=true;fleetFaction=fleetClass=fleetPage=fleetFactionPage=0;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);break;
 case 2:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch24 faction / "))==4,"Four paged faction buttons");Click("MORE FACTIONS >");break;
 case 3:UiCheck(ui.Hits.Any(h=>h.Tip=="Show batch24 faction / Kepler Resource Group"),"Industrial factions visible on next page");Click("Show batch24 faction / Kepler Resource Group");break;
 case 4:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Kepler Resource Group","Faction navigation selects its hulls");batch24Selection=fleetSelected;world.SystemId=ShipBatch21.ById[fleetSelected].HomeSystem;batch24Cash=world.Credits;batch24Berths=world.FleetBerths.Count;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 5:UiCheck(world.FleetBerths.Count==batch24Berths+1&&world.Credits==batch24Cash-(float)ShipBatch21.ById[batch24Selection].Price,"Specialist purchase click");Click("SHOW SHIP INFORMATION");break;
 case 6:UiCheck(shipInfoId==batch24Selection&&desktop.Last().Kind==ViewMode.ShipInfo,"Selected hull information and model opens");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 7:Click("Switch active ship batch");break;
 case 8:UiCheck(!fleetSpecialists&&fleetBattleships&&FleetCatalog.ById[fleetSelected].Class=="Battleship","Previous battleships reachable");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.FleetBerths.Count==world.FleetBerths.Count&&loaded.Player.HullProficiency[batch24Skill]==batch24Level+1,"Specialist UI changes save");Console.WriteLine("9 batch24 UI checks passed.");break;
 }}
}
