namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int batchUiBerths,batchUiLevel,batchUiSkill;float batchUiCredits;
 void RunShipBatch21UiChecks(int stage){void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
 fleetDestroyers=false;switch(stage){
 case 0:UiCheck(shipInfoTab==7&&world.Player.CombatHull==FleetCatalog.Source("Heron").Id,"Traits tab opens for purchased exploration hull");batchUiSkill=ShipBatch21.ById[world.Player.CombatHull].Traits.First(t=>t.Skill!=0).Skill;batchUiLevel=world.Player.HullProficiency.GetValueOrDefault(batchUiSkill,1);batchUiCredits=world.Credits;Click("Dock and board this hull to train; bonuses apply per level.");break;
 case 1:UiCheck(world.Player.HullProficiency[batchUiSkill]==batchUiLevel+1&&world.Credits==batchUiCredits-250*(batchUiLevel+1)*(batchUiLevel+1),"Training click applies proficiency and exact cost");ShowShipInformation(FleetCatalog.Source("Punisher").Id);shipInfoTab=7;batchUiCredits=world.Credits;break;
 case 2:Click("Dock and board this hull to train; bonuses apply per level.");UiCheck(world.Credits==batchUiCredits,"Training another displayed hull requires boarding it");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetBrowser=true;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);batchUiBerths=world.FleetBerths.Count;break;
 case 3:UiCheck(ui.Hits.Any(h=>h.Tip==FleetCatalog.Playable.Length+" PLAYABLE"),"Playable registry filter is visible");Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 4:UiCheck(world.FleetBerths.Count==batchUiBerths+1&&world.Credits==batchUiCredits-400,"Additional purchase adds a fitted berth and charges once");Click("SHOW SHIP INFORMATION");break;
 case 5:UiCheck(desktop.Last().Kind==ViewMode.ShipInfo&&shipInfoId==world.Player.CombatHull,"Registry opens selected hull information");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.Player.HullProficiency[batchUiSkill]==2&&loaded.FleetBerths.Count==world.FleetBerths.Count,"UI acquisition and proficiency survive save/load");Console.WriteLine("7 batch21 UI checks passed.");break;
 }}
}
