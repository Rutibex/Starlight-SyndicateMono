namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int batch22Berths;float batch22Cash;string batch22Selected="";
 void RunShipBatch22UiChecks(int stage){if(stage==0){fleetElite=false;fleetStrategic=false;fleetCruisers=false;}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
 fleetDestroyers=false;switch(stage){
 case 0:UiCheck(shipInfoTab==7&&world.Player.CombatHull==FleetCatalog.Source("Anhinga").Id,"Tactical battlecruiser traits opens");Click("Switch tactical mode / Secondary");break;
 case 1:UiCheck(world.Player.HullTacticalMode=="Secondary"&&world.Player.HullTacticalCooldown>0,"Secondary mode click applies cooldown");Click("Switch tactical mode / Tertiary");break;
 case 2:UiCheck(world.Player.HullTacticalMode=="Secondary","Mode cooldown blocks rapid UI switch");world.Player.HullTacticalCooldown=0;Click("Switch tactical mode / Tertiary");break;
 case 3:UiCheck(world.Player.HullTacticalMode=="Tertiary","Tertiary mode click applies");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetAdvanced=false;fleetBrowser=true;fleetCurrentBatch=true;fleetSpecialists=false;fleetBattleships=false;fleetClass=fleetFaction=fleetPage=0;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);break;
 case 4:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch22 faction / "))==4,"Four faction group buttons visible");Click("Show batch22 faction / Boreal Federation");break;
 case 5:UiCheck(fleetFaction>0&&ShipBatch21.Current.Any(p=>p.ShipId==fleetSelected)&&FleetCatalog.ById[fleetSelected].Faction=="Boreal Federation","Faction click selects its new hulls");batch22Selected=ShipBatch21.Current.First(p=>FleetCatalog.ById[p.ShipId].Faction=="Boreal Federation"&&FleetCatalog.ById[p.ShipId].Class=="Cruiser").ShipId;fleetSelected=batch22Selected;world.SystemId=ShipBatch21.ById[batch22Selected].HomeSystem;batch22Berths=world.FleetBerths.Count;batch22Cash=world.Credits;break;
 case 6:Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 7:UiCheck(world.FleetBerths.Count==batch22Berths+1&&world.Credits==batch22Cash-(float)ShipBatch21.ById[batch22Selected].Price,"Cruiser purchase click conserves exact wallet and adds fitted berth");Click("SHOW SHIP INFORMATION");break;
 case 8:UiCheck(desktop.Last().Kind==ViewMode.ShipInfo&&shipInfoId==batch22Selected,"Grouped registry opens cruiser information");Console.WriteLine("8 batch22 UI checks passed.");break;
 }}
}
