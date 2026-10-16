namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 float batch26Cash;int batch26Berths;string batch26Id="";
 void RunShipBatch26UiChecks(int stage){if(stage==0){fleetElite=false;fleetStrategic=false;fleetCruisers=false;}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:UiCheck(shipInfoTab==7&&ShipBatch21.ById[world.Player.CombatHull].Batch==26,"Destroyer traits displayed");Click("Switch tactical mode / Secondary");break;
 case 1:UiCheck(world.Player.HullTacticalMode=="Secondary"&&world.Player.HullTacticalCooldown>0,"Propulsion mode clicked");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetDestroyers=fleetCurrentBatch=fleetBrowser=true;fleetAdvanced=fleetSpecialists=false;fleetFaction=fleetClass=fleetPage=fleetFactionPage=0;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);break;
 case 2:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch26 faction / "))==4,"Four major factions shown");Click("MORE FACTIONS >");break;
 case 3:UiCheck(ui.Hits.Any(h=>h.Tip=="Show batch26 faction / Kepler Resource Group"),"Additional factions accessible");Click("Show batch26 faction / Kepler Resource Group");break;
 case 4:UiCheck(FleetCatalog.ById[fleetSelected].Name=="Tonnage","Faction selects Tonnage");batch26Id=fleetSelected;world.SystemId=ShipBatch21.ById[batch26Id].HomeSystem;batch26Cash=world.Credits;batch26Berths=world.FleetBerths.Count;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 5:UiCheck(world.FleetBerths.Count==batch26Berths+1&&world.Credits==batch26Cash-(float)ShipBatch21.ById[batch26Id].Price,"Fitted destroyer bought");Click("SHOW SHIP INFORMATION");break;
 case 6:UiCheck(shipInfoId==batch26Id&&desktop.Last().Kind==ViewMode.ShipInfo,"Destroyer lore opens");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 7:Click("Switch active ship batch");break;
 case 8:UiCheck(!fleetDestroyers&&fleetAdvanced&&ShipBatch21.ById[fleetSelected].Batch==25,"Previous frigates accessible");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.Player.HullTacticalMode=="Secondary"&&loaded.FleetBerths.Count==world.FleetBerths.Count,"Mode and acquisition saved");Console.WriteLine("9 batch26 UI checks passed.");break;
 }}
}
