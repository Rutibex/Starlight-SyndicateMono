using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void ShowTacticalDiagnosticsB()
 {
  var ship=FittingWorld.Player;var entry=new InformationEntry{Key="tactical/stage-b",Id=ship.InstanceId,Name="Tactical mechanics / "+ship.HullName,Kind="Fitted mechanics",Group="Ship / Tactical",Description="Live and simulated fits use the same calculations. Values below are a snapshot; reopen to refresh."};
  entry.Add("Description",entry.Description);
  entry.Add("Attributes","Rules profile",TacticalRulesB.Profile);
  foreach(string key in new[]{"cpuOutput","powerOutput","shieldCapacity","armorHP","capacitorCapacity","rechargeRate","maxVelocity","agility","signatureRadius","maxTargetRange","scanResolution"}){
   var explain=ship.ExplainAttribute(key);entry.Add("Attributes",key,$"{explain.Base:0.#####} base -> {explain.Value:0.#####} fitted");
   entry.Add("Calculation",key+" / base",explain.Base.ToString("0.#####"));foreach(var step in explain.Steps.Where(s=>s.Before!=s.After))entry.Add("Calculation",key+" / "+step.Stage,$"{step.Before:0.#####} -> {step.After:0.#####}");
  }
  foreach(var module in FittingLayoutModel.Installed(ship)){
   var state=ship.TacticalModule(module);float damage=state?.GetType().GetProperty("Damage")?.GetValue(state) is float n?n:0;
   entry.Add("Modules",module.Name,$"{module.Bank} / {(module.Burnt?"burnt":module.Online==false?"offline":module.Active?"active":"online")} / damage {damage:P0}");
   if(module.Kind=="weapon"){
    var weapon=ship.Weapons[module.Index];var q=TacticalRulesB.Weapon(ship,weapon);entry.Add("Weapons",module.Name,$"{q.Packet.Total:0.##} volley / {q.Cycle:0.###}s / {q.Capacitor:0.###} GJ");entry.Add("Weapons",module.Name+" / damage",$"EM {q.Packet.EM:0.##}; TH {q.Packet.Thermal:0.##}; KI {q.Packet.Kinetic:0.##}; EX {q.Packet.Explosive:0.##}");entry.Add("Weapons",module.Name+" / application",$"tracking {q.Gun.Tracking:0.####}; optimal {q.Gun.Optimal:0}m; falloff {q.Gun.Falloff:0}m; flight {q.Velocity*q.FlightTime:0}m");
   }
  }
  entry.Add("Heat","High / medium / low",string.Join(" / ",ship.RackHeat.Select(h=>h.ToString("P0"))));
  entry.Add("Heat","Model","Deterministic Starlight rack propagation; numeric EVE heat parity is not claimed.");
  foreach(var e in world.CombatEvents.TakeLast(35).Reverse())entry.Add("Combat",e.Time.ToString("HH:mm:ss")+" / "+e.Action,$"{e.Source} -> {e.Target}: {e.Applied:0.##} HP / {e.Probability:P0} solution");
  informationSnapshots[entry.Key]=entry;ShowInformation(entry.Key);
 }
 void DrawTacticalFitControlsB()
 {
  ui.Button(new(566,609,320,25),"TACTICAL CALCULATIONS",ShowTacticalDiagnosticsB,"Open tactical calculation trace");
  var heat=FittingWorld.Player.RackHeat;ui.Text($"Rack heat H {heat[0]:P0} / M {heat[1]:P0} / L {heat[2]:P0}",566,638,Gold,10);
  if(SelectedFitModule is {} m&&FittingWorld.Player.TacticalModule(m)?.GetType().GetProperty("Overheat")!=null){
   bool hot=FittingWorld.Player.TacticalModule(m)!.GetType().GetProperty("Overheat")!.GetValue(FittingWorld.Player.TacticalModule(m)) is true;
   ui.Button(new(950,549,135,25),hot?"HEAT ON":"HEAT OFF",()=>{FittingWorld.SetModuleOverheat(m,!hot);},"Toggle selected module heat");
   if(FittingWorld.Docked)ui.Button(new(950,609,135,25),"REPAIR MODULE",()=>FittingWorld.RepairTacticalModule(m),"Repair selected module at station");
  }
 }
}
