namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int batch23Berths,batch23Skill,batch23Level;float batch23Cash;string batch23Selected="";
 void RunShipBatch23UiChecks(int stage){if(stage==0){fleetElite=false;fleetStrategic=false;fleetCruisers=false;}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());if(stage==0)fleetDestroyers=false;switch(stage){
 case 0:UiCheck(shipInfoTab==7&&FleetCatalog.ById[world.Player.CombatHull].Class=="Battleship","Battleship traits opens");batch23Skill=ShipBatch21.ById[world.Player.CombatHull].Traits.First(t=>t.Skill!=0).Skill;batch23Level=world.Player.HullProficiency.GetValueOrDefault(batch23Skill,1);batch23Cash=world.Credits;Click("Dock and board this hull to train; bonuses apply per level.");break;
 case 1:UiCheck(world.Player.HullProficiency[batch23Skill]==batch23Level+1&&world.Credits==batch23Cash-250*(batch23Level+1)*(batch23Level+1),"Battleship training click pays exact price");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetAdvanced=false;fleetBrowser=fleetCurrentBatch=true;fleetSpecialists=false;fleetBattleships=true;fleetFaction=fleetClass=fleetPage=0;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);break;
 case 2:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch23 faction / "))==4,"Four battleship faction groups visible");Click("Show batch23 faction / Amur Cooperative Compact");break;
 case 3:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Amur Cooperative Compact"&&FleetCatalog.ById[fleetSelected].Class=="Battleship","Faction group selects a basic battleship");batch23Selected=fleetSelected;world.SystemId=ShipBatch21.ById[batch23Selected].HomeSystem;batch23Cash=world.Credits;batch23Berths=world.FleetBerths.Count;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 4:UiCheck(world.Credits==batch23Cash-(float)ShipBatch21.ById[batch23Selected].Price&&world.FleetBerths.Count==batch23Berths+1,"Battleship purchase click adds fitted berth and exact wallet debit");Click("SHOW SHIP INFORMATION");break;
 case 5:UiCheck(shipInfoId==batch23Selected&&desktop.Last().Kind==ViewMode.ShipInfo,"Faction registry opens selected battleship lore");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 6:Click("Switch active ship batch");break;
 case 7:UiCheck(!fleetBattleships&&fleetCurrentBatch&&FleetCatalog.ById[fleetSelected].Class=="Cruiser","Previous cruiser batch remains accessible");Click("Switch active ship batch");break;
 case 8:UiCheck(fleetElite&&ShipBatch21.ById[fleetSelected].Batch==29,"Latest elite battleship batch accessible after cycle");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.FleetBerths.Count==world.FleetBerths.Count&&loaded.Player.HullProficiency[batch23Skill]==batch23Level+1,"UI battleship training and purchase survive save/load");Console.WriteLine("9 batch23 UI checks passed.");break;
 }}
}
