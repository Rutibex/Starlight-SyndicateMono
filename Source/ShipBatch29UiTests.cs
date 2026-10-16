namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 float batch29Cash;
 void RunShipBatch29RegistryUiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:UiCheck(fleetElite&&FleetVisibleBatch.Length==25,"New elite battleship registry selected");UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch29 faction / "))==4,"Four major factions visible");Click("Show batch29 faction / Boreal Federation");break;
 case 1:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Boreal Federation","Boreal native selection");Click("Show batch29 faction / Amur Cooperative Compact");break;
 case 2:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Amur Cooperative Compact","Amur native selection");Click("Show batch29 faction / Suryavarta Empire");break;
 case 3:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Suryavarta Empire","Suryavarta native selection");Click("Show batch29 faction / Azadi Confederation");break;
 case 4:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Azadi Confederation","Azadi native selection");Click("MORE FACTIONS >");break;
 case 5:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch29 faction / "))==4,"Four rare issuer factions visible");Click("Show batch29 faction / Veyr Assemblage");break;
 case 6:UiCheck(FleetCatalog.ById[fleetSelected].SourceHull=="Babaroga","rare entropic battleship native selection");world.SystemId=ShipBatch21.ById[fleetSelected].HomeSystem;batch29Cash=world.Credits;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 7:UiCheck(world.Credits==batch29Cash-(float)ShipBatch21.ById[fleetSelected].Price,"exact native hull acquisition debit");Click("SHOW SHIP INFORMATION");break;
 case 8:UiCheck(shipInfoId==fleetSelected,"registry opens ship lore");Click("Traits");break;
 case 9:UiCheck(shipInfoTab==7,"native source traits navigation");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 10:Click("Switch active ship batch");break;
 case 11:UiCheck(!fleetElite&&fleetStrategic&&ShipBatch21.ById[fleetSelected].Batch==28,"prior strategic and command registry accessible");Console.WriteLine("12 batch29 registry UI checks passed.");break;
 }}
 void RunShipBatch29JumpUiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:UiCheck(world.Player.EliteBattleship,"native black ops fitting");world.SystemId="rook";world.Hangar["sp_ozone"]=2000;world.HireWingmate("Escort");world.AssignFleetHull(world.Wingmates[0].Id,"covert");specialDestination="umbra";specialBeaconKind="CovertCyno";specialTab=1;EnsureWindow(ViewMode.Special);break;
 case 1:batch29Cash=world.Credits;Click("CONTRACT REMOTE BEACON");break;
 case 2:UiCheck(world.SelectedJumpBeacon()?.Kind=="CovertCyno"&&world.Credits==batch29Cash-650,"native contract consumes exact credits and yields selected beacon");world.Undock();world.Targets.RemoveAll(t=>!world.Wingmates.Any(w=>w.Id==t.Id));specialTab=0;world.FittingSpecialSlot=Array.FindIndex(world.Player.Special,s=>s.Definition?.Kind=="CovertPortal");EnsureWindow(ViewMode.Special);break;
 case 3:Click("ACTIVATE");break;
 case 4:UiCheck(world.Player.HasSpecial("CovertPortal"),"native bridge activation");Click("JUMP NETWORK");break;
 case 5:Click("CONDUIT JUMP");break;
 case 6:UiCheck(world.SystemId=="umbra"&&world.LastBridgePassengers==1&&world.Wingmates[0].System=="umbra","native conduit transports player and fleet ally");UiCheck(world.JumpFatigue>0&&world.JumpDriveCooldown>0,"native travel pays fatigue and cooldown");world.Save();break;
 case 11:var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.SystemId=="umbra"&&loaded.Wingmates[0].System=="umbra","UI-created jump save/load");Console.WriteLine("6 batch29 jump UI checks passed.");break;
 }}
}
