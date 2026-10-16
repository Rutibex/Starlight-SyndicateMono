namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int batch25Skill,batch25Level,batch25Berths;float batch25Cash;string batch25Selection="";
 void RunShipBatch25UiChecks(int stage){if(stage==0){fleetElite=false;fleetStrategic=false;fleetCruisers=false;}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());fleetDestroyers=false;switch(stage){
 case 0:UiCheck(shipInfoTab==7&&ShipBatch21.ById[world.Player.CombatHull].Batch==25,"Advanced frigate traits displayed");batch25Skill=ShipBatch21.ById[world.Player.CombatHull].Traits.Last(t=>t.Skill!=0).Skill;batch25Level=world.Player.HullProficiency.GetValueOrDefault(batch25Skill,1);batch25Cash=world.Credits;Click("Dock and board this hull to train; bonuses apply per level.");break;
 case 1:UiCheck(world.Player.HullProficiency[batch25Skill]==batch25Level+1&&world.Credits==batch25Cash-250*(batch25Level+1)*(batch25Level+1),"Training click pays and changes proficiency");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetAdvanced=fleetCurrentBatch=fleetBrowser=true;fleetSpecialists=false;fleetFaction=fleetClass=fleetPage=fleetFactionPage=0;fleetSelected=world.Player.CombatHull;EnsureWindow(ViewMode.Fitting);break;
 case 2:UiCheck(ui.Hits.Count(h=>h.Tip.StartsWith("Show batch25 faction / "))==4,"Four main faction buttons");Click("MORE FACTIONS >");break;
 case 3:UiCheck(ui.Hits.Any(h=>h.Tip=="Show batch25 faction / Pale Orchard"),"Rare factions accessible on second page");Click("Show batch25 faction / Pale Orchard");break;
 case 4:UiCheck(FleetCatalog.ById[fleetSelected].Faction=="Pale Orchard","Faction click selects its hull");batch25Selection=fleetSelected;world.SystemId=ShipBatch21.ById[fleetSelected].HomeSystem;batch25Cash=world.Credits;batch25Berths=world.FleetBerths.Count;Click("Purchase another fitted hull at its home station; board it through Local berths.");break;
 case 5:UiCheck(world.FleetBerths.Count==batch25Berths+1&&world.Credits==batch25Cash-(float)ShipBatch21.ById[batch25Selection].Price,"Fitted rare hull purchased through UI");Click("SHOW SHIP INFORMATION");break;
 case 6:UiCheck(shipInfoId==batch25Selection&&desktop.Last().Kind==ViewMode.ShipInfo,"Selected hull lore and model opens");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);break;
 case 7:Click("Switch active ship batch");break;
 case 8:UiCheck(!fleetAdvanced&&fleetSpecialists&&ShipBatch21.ById[fleetSelected].Batch==24,"Previous specialists accessible");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.Player.HullProficiency[batch25Skill]==batch25Level+1&&loaded.FleetBerths.Count==world.FleetBerths.Count,"UI training and acquisitions saved");desktop.Clear();utilityTab=5;world.Hangar["booster25_focus"]=1;Open(ViewMode.Utilities);break;
 case 9:Click("Consume one Focus dose");break;
 case 10:UiCheck(world.Player.BoosterTime>0&&world.ItemCount("booster25_focus",true)==0,"Combat booster UI consumes inventory and starts timer");Console.WriteLine("10 batch25 UI checks passed.");break;
 }}
}
