using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void SetupStageBCapture(){var station=Stations44.Stations.First(s=>s.Services.Contains(7)&&s.Services.Contains(13)&&s.Services.Contains(17));world.NewExploration43(int.Parse(station.System[7..]));world.StationId=station.Id;world.Grid=station.Id;world.Screen=ViewMode.Station;world.WalletFunds=100000;world.Player.UtilityFit="";world.Player.Weapons=[WeaponState.Create("rail"),WeaponState.Create("missile")];world.Player.Weapons[0].Loaded=10;world.Skills.Grant(SkillCatalog.All.Select(s=>new SkillRequirement(s.Id,5)));world.SyncSkills();desktop.Clear();EnsureWindow(ViewMode.Fitting);fitSelectedSocket="weapon:0";if(captureScene=="stage-b-trace"){ShowTacticalDiagnosticsB();desktop.Last().InfoTab="Calculation";}if(captureScene=="stage-b-weapons"){ShowTacticalDiagnosticsB();desktop.Last().InfoTab="Weapons";}}
 int stageBUiChecks;double stageBLiveDps;string stageBUiShip="";
 void RunStageBUiChecks(int stage){void Check(bool ok,string label){if(!ok)throw new Exception("Stage B UI: "+label);stageBUiChecks++;Console.WriteLine("STAGE B UI PASS: "+label);}void Click(string tip){TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());}
 switch(stage){
 case 0:Check(world.ImportedWorld43,"Imported universe fitting remains available");stageBLiveDps=world.Player.WeaponDps;Click("Toggle selected fitting module online");Check(!world.Player.Weapons[0].Online,"Weapon offline command uses real fitted state");break;
 case 1:Check(world.Player.WeaponDps<stageBLiveDps,"Offline weapon disappears from projected offense");Click("Toggle selected fitting module online");Check(world.Player.Weapons[0].Online,"Weapon online command restores fitted state");break;
 case 2:Click("Toggle selected module heat");Check(world.Player.Weapons[0].Overheat&&world.Player.WeaponDps>stageBLiveDps,"Weapon heat affects fitting projection");world.Player.Weapons[0].Damage=.5f;break;
 case 3:Click("Repair selected module at station");Check(world.Player.Weapons[0].Damage==0&&!world.Player.Weapons[0].Overheat,"Station repair clears heat damage");break;
 case 4:Click("Open tactical calculation trace");Check(desktop.Last().InfoKey=="tactical/stage-b","Calculation button opens actual diagnostics");break;
 case 5:var record=ResolveInformation("tactical/stage-b")!;Check(record.Tabs.ContainsKey("Calculation")&&record.Tabs.ContainsKey("Weapons")&&record.Tabs.ContainsKey("Modules"),"Diagnostics expose graph, packet and module state");desktop.Last().InfoTab="Calculation";break;
 case 6:Check(ui.Hits.Any(),"Calculation panel renders");desktop.Last().InfoTab="Weapons";break;
 case 7:desktop.RemoveAll(w=>w.Kind==ViewMode.Information);stageBUiShip=world.Player.InstanceId;break;
 case 8:Click("Toggle fitting simulation");Check(fittingSimulation!=null&&!ReferenceEquals(FittingWorld.Player,world.Player),"Simulator owns a detached ship");Check(Math.Abs(FittingWorld.Player.WeaponDps-world.Player.WeaponDps)<.0001,"Simulator and live offense agree");break;
 case 9:fitSelectedSocket="weapon:0";break;
 case 10:Click("Toggle selected module heat");Check(FittingWorld.Player.Weapons[0].Overheat&&!world.Player.Weapons[0].Overheat,"Simulated heat does not change real modules");break;
 case 11:Click("Toggle fitting simulation");Check(fittingSimulation==null&&world.Player.InstanceId==stageBUiShip,"Return to live preserves owned ship identity");break;
 case 12:ShowTacticalDiagnosticsB();desktop.Last().InfoTab="Weapons";Console.WriteLine(stageBUiChecks+" Stage B UI checks passed.");break;
 }
 }
}
