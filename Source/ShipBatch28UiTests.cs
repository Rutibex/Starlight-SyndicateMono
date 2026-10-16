namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 string batch28Live="",batch28Core="",batch28Covert="";float batch28Cash;
 void RunShipBatch28UiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:UiCheck(world.Player.StrategicCruiser,"Strategic cruiser native fit opens");Click("Open strategic subsystems");break;
 case 1:UiCheck(subsystemPanel,"Subsystem panel opens by click");batch28Core=ShipBatch28.For(world.Player.CombatHull,"Core")[0].Id;batch28Cash=world.Credits;Click("Install subsystem / "+batch28Core);break;
 case 2:UiCheck(world.Player.StrategicSubsystems["Core"]==batch28Core&&world.Credits==batch28Cash-900,"Native subsystem purchase debits exact credits");Click("Select subsystem category / Defensive");break;
 case 3:batch28Covert=ShipBatch28.For(world.Player.CombatHull,"Defensive").Single(s=>s.Name.Contains("Covert")).Id;Click("Install subsystem / "+batch28Covert);break;
 case 4:UiCheck(world.Player.HullCapability("can fit covert ops")&&world.Player.Stat("virusStrengthBonus")==10,"UI covert choice grants live capability and analyzer bonus");batch28Live=System.Text.Json.JsonSerializer.Serialize(world.Player.StrategicSubsystems);Click("Close strategic subsystems");Click("Toggle fitting simulation");break;
 case 5:UiCheck(fittingSimulation!=null&&FittingWorld.Player.StrategicSubsystems["Defensive"]==batch28Covert,"Simulation clones installed composition");Click("Open strategic subsystems");break;
 case 6:var normal=ShipBatch28.For(world.Player.CombatHull,"Defensive").First(s=>!s.Name.Contains("Covert"));Click("Install subsystem / "+normal.Id);UiCheck(FittingWorld.Player.StrategicSubsystems["Defensive"]==normal.Id&&System.Text.Json.JsonSerializer.Serialize(world.Player.StrategicSubsystems)==batch28Live,"UI simulation swap leaves live configuration unchanged");break;
 case 7:Click("Close strategic subsystems");Click("Toggle fitting simulation");break;
 case 8:UiCheck(fittingSimulation==null&&world.Player.StrategicSubsystems["Defensive"]==batch28Covert,"Return to live retains purchased covert part");Click("SHIP INFO");break;
 case 9:UiCheck(shipInfoId==world.Player.CombatHull,"Ship information native navigation");Click("Traits");break;
 case 10:Click("Dock and board this hull to train; bonuses apply per level.");break;
 case 11:UiCheck(world.Player.HullProficiency.Any(p=>p.Value>1),"Subsystem proficiency native training");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.Player.StrategicSubsystems.SequenceEqual(world.Player.StrategicSubsystems),"UI strategic purchases and training save/load");Console.WriteLine("10 batch28 subsystem UI checks passed.");break;
 }}
 void RunShipBatch28RegistryUiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:UiCheck(fleetStrategic&&FleetVisibleBatch.Length==12,"New strategic and command registry selected");UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch28 faction / "))==4,"Four new faction groups visible");Click("Show batch28 faction / Boreal Federation");break;
 case 1:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Boreal Federation","Boreal native faction selection");Click("Show batch28 faction / Amur Cooperative Compact");break;
 case 2:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Amur Cooperative Compact","Amur native faction selection");Click("Show batch28 faction / Suryavarta Empire");break;
 case 3:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Suryavarta Empire","Suryavarta native faction selection");Click("Show batch28 faction / Azadi Confederation");break;
 case 4:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Azadi Confederation","Azadi native faction selection");world.SystemId=ShipBatch21.ById[fleetSelected].HomeSystem;batch28Cash=world.Credits;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 5:UiCheck(world.Credits==batch28Cash-(float)ShipBatch21.ById[fleetSelected].Price,"Native fitted hull purchase debits exact price");Click("SHOW SHIP INFORMATION");break;
 case 6:UiCheck(shipInfoId==fleetSelected,"Registry opens hull lore");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 7:Click("Switch active ship batch");break;
 case 8:UiCheck(!fleetStrategic&&fleetCruisers&&ShipBatch21.ById[fleetSelected].Batch==27,"Prior advanced cruiser registry accessible");Console.WriteLine("9 batch28 registry UI checks passed.");break;
 }}
}
