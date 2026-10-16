namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 float batch27Cash;int batch27Berths;
 void RunShipBatch27UiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:fleetElite=false;fleetStrategic=false;UiCheck(shipInfoTab==7&&ShipBatch21.ById[world.Player.CombatHull].Batch==27,"Cruiser traits displayed");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetCruisers=fleetCurrentBatch=fleetBrowser=true;fleetDestroyers=fleetAdvanced=fleetSpecialists=false;fleetFaction=fleetClass=fleetPage=fleetFactionPage=0;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);break;
 case 1:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch27 faction / "))==4,"Four major factions shown");Click("MORE FACTIONS >");break;
 case 2:UiCheck(ui.Hits.Any(h=>h.Tip.StartsWith("Show batch27 faction / ")),"Rare faction page accessible");Click("MORE FACTIONS >");break;
 case 3:UiCheck(ui.Hits.Any(h=>h.Tip.StartsWith("Show batch27 faction / ")),"Final faction page accessible");Click("Show batch27 faction / ");break;
 case 4:UiCheck(ShipBatch21.ById[fleetSelected].Batch==27,"Rare cruiser selected");world.SystemId=ShipBatch21.ById[fleetSelected].HomeSystem;batch27Cash=world.Credits;batch27Berths=world.FleetBerths.Count;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 5:UiCheck(world.FleetBerths.Count==batch27Berths+1&&world.Credits==batch27Cash-(float)ShipBatch21.ById[fleetSelected].Price,"Cruiser purchase and wallet debit");Click("SHOW SHIP INFORMATION");break;
 case 6:UiCheck(shipInfoId==fleetSelected&&desktop.Last().Kind==ViewMode.ShipInfo,"Cruiser lore opens");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 7:Click("Switch active ship batch");break;
 case 8:UiCheck(!fleetCruisers&&fleetDestroyers&&ShipBatch21.ById[fleetSelected].Batch==26,"Previous destroyers accessible");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.Player.CombatHull==world.Player.CombatHull&&loaded.FleetBerths.Count==world.FleetBerths.Count,"Cruiser saves and loads");Console.WriteLine("9 batch27 UI checks passed.");break;
 }}
 void RunShipBatch27ScanUiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());switch(stage){
 case 0:UiCheck(directionalScanner,"Native directional scanner opens");world.Player.Position=Microsoft.Xna.Framework.Vector3.Zero;world.Player.Facing=Microsoft.Xna.Framework.Vector3.UnitZ;world.Targets=[new(){Id="scannercontact",Name="Scanner contact",Kind=TargetKind.Raider,Training=true,Profile=Ship.RaiderProfile(),Position=new(1000,0,0),Shield=10000,Armor=10000,Hull=10000}];world.SelectedId="scannercontact";Click("Directional angle / 30");break;
 case 1:UiCheck(directionalAngle==30,"Angle control clicked");Click("Cycle directional range");break;
 case 2:UiCheck(directionalRange==100000,"Range control clicked");Click("Run directional scan");break;
 case 3:UiCheck(!directionalResults.Any(c=>c.Id=="scannercontact"),"Narrow facing cone excludes side contact");Click("Aim directional scan toward selected contact");break;
 case 4:UiCheck(world.Player.Facing.X>.9,"Face selected control changes heading");Click("Run directional scan");break;
 case 5:UiCheck(directionalResults.Any(c=>c.Id=="scannercontact"),"Native scan lists actual contact");Click("Open probe scanner");break;
 case 6:UiCheck(!directionalScanner,"Probe scanner remains accessible");Console.WriteLine("7 batch27 scanner UI checks passed.");break;
 }}
}
