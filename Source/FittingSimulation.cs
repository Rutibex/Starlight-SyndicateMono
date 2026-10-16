using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
// This world is never updated, saved, or exposed to campaign module-control windows.
internal sealed class FittingSimulation
{
 static readonly JsonSerializerOptions Options=new(){IncludeFields=true,IgnoreReadOnlyProperties=true,NumberHandling=JsonNumberHandling.AllowNamedFloatingPointLiterals};
 public DemoWorld World {get;private set;}=new();
 public static Ship Copy(Ship ship){var copy=JsonSerializer.Deserialize<Ship>(JsonSerializer.Serialize(ship,Options),Options)!;copy.RestoreStats(new(ship.Attributes));return copy;}
 public FittingSimulation(Ship ship){Reset(ship);}
 public void Reset(Ship ship,bool assumptions=false){World=new DemoWorld{Player=Copy(ship),Screen=ViewMode.Station,IgnoreSkillRequirements=true};StopCycles();Stock();if(assumptions)RestoreAssumptions(ship);}
 public void Stock(){foreach(var item in ItemCatalog.All)World.Hangar[item.Id]=100000;World.Player.Cap=World.Player.MaxCap;}
 object? ModuleState(FittingModule m)=>m.Kind switch{"weapon"=>World.Player.Weapons[m.Index],"defense"=>World.Player.Defense[m.Index],"ewar"=>World.Player.Ewar[m.Index],"support"=>World.Player.Support[m.Index],"enhancement"=>World.Player.Enhancements[m.Index],"mobility"=>World.Player.Mobility[m.Index],"utility"=>World.Player.Utilities[m.Index],"special"=>World.Player.Special[m.Index],_=>null};
 void StopCycles(){foreach(var w in World.Player.Weapons)w.Stop();foreach(var m in FittingLayoutModel.Installed(World.Player)){var state=ModuleState(m);if(state==null)continue;state.GetType().GetProperty("Active")?.SetValue(state,false);foreach(string name in new[]{"CycleLeft","EffectLeft","ReloadLeft","ReactivationLeft","WarmupLeft","Cooldown"})if(state.GetType().GetProperty(name) is {} prop&&prop.PropertyType==typeof(float))prop.SetValue(state,0f);}World.Player.TurretOn=World.Player.MissileOn=World.Player.BoosterOn=World.Player.WebOn=World.Player.MinerOn=false;}
 public void Hull(string id){var s=new Ship();s.ApplyFleetHull(FleetCatalog.ById[id],true);s.HullProficiency=new(World.Player.HullProficiency);s.SkillLevels=new(World.Player.SkillLevels);Reset(s);}
 public void RestoreAssumptions(Ship ship){foreach(var m in FittingLayoutModel.Installed(ship))if(m.Active)SetActive(m,true);}
 public static void ClearEquipment(Ship s){s.Weapons=s.Weapons.Select(_=>new WeaponState{ModuleId=""}).ToArray();s.Ewar=s.Ewar.Select(_=>new EwarState()).ToArray();s.Defense=s.Defense.Select(_=>new DefenseState()).ToArray();s.Support=s.Support.Select(_=>new SupportState()).ToArray();s.Enhancements=s.Enhancements.Select(_=>new EnhancementState()).ToArray();s.Mobility=s.Mobility.Select(_=>new MobilityState()).ToArray();s.Utilities=s.Utilities.Select(_=>new UtilityState()).ToArray();s.Special=s.Special.Select(_=>new SpecialState()).ToArray();s.Rigs=s.Rigs.Select(_=>"").ToArray();s.UtilityFit="";s.Drones=[];}
 public string Error(FittingSpec spec,int slot){var attrs=new Dictionary<string,double?>(World.Player.Attributes);try{World.Player.SetStat("cpuOutput",1000000000);World.Player.SetStat("powerOutput",1000000000);return FittingLayoutModel.Error(World,spec,slot);}finally{World.Player.RestoreStats(attrs);}}
 public bool Fit(FittingSpec spec,int slot){Stock();var attrs=new Dictionary<string,double?>(World.Player.Attributes);try{World.Player.SetStat("cpuOutput",1000000000);World.Player.SetStat("powerOutput",1000000000);return FittingLayoutModel.Fit(World,spec,true,slot);}finally{World.Player.RestoreStats(attrs);World.RefreshSavedFitting();}}
 public bool SetActive(FittingModule m,bool active)
 {
  object? state=m.Kind switch {"weapon"=>World.Player.Weapons[m.Index],"defense"=>World.Player.Defense[m.Index],"ewar"=>World.Player.Ewar[m.Index],"support"=>World.Player.Support[m.Index],"enhancement"=>World.Player.Enhancements[m.Index],"mobility"=>World.Player.Mobility[m.Index],"utility"=>World.Player.Utilities[m.Index],"special"=>World.Player.Special[m.Index],_=>null};
  if(state==null||m.Online==false||m.Burnt)return false;
  var def=state.GetType().GetProperty("Definition")?.GetValue(state);if(def==null)return false;
  if(def.GetType().GetProperty("Active")?.GetValue(def) is false)return false;
  var prop=state.GetType().GetProperty("Active");if(prop==null)return false;prop.SetValue(state,active);
  if(m.Kind=="defense"){var defense=(DefenseState)state;defense.EffectLeft=active?defense.Definition!.Cycle:0;defense.CycleLeft=defense.EffectLeft;}
  // Utility capacitor projection needs its nominal cycle rather than a running timer.
  if(m.Kind=="utility"&&def.GetType().GetProperty("Cycle")?.GetValue(def) is float cycle){var u=(UtilityState)state;var d=u.Definition!;state.GetType().GetProperty("CycleDuration")?.SetValue(state,cycle*World.Player.HarvestFactor(d,"cycle")*World.Player.HullFactor("utility","cycle",d.Kind)*(d.Family=="Ice"?World.Player.UtilityFactor("iceCycle"):1));}
  return true;
 }
}
public sealed partial class DemoGame
{
 string simUiBaseline="";
 void RunSimulationUiChecks(int stage){
  string State()=>JsonSerializer.Serialize(new{world.Player,world.Credits,world.Day,world.CargoItems,world.StationItems},WeaponCatalog.JsonOptions);
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  if(stage==0){simUiBaseline=State();Click("Toggle fitting simulation");UiCheck(fittingSimulation!=null,"Simulation button enters detached fitting");}
  if(stage==1){var slot=fitSockets.First(s=>s.Module?.Kind=="weapon");SelectFitSocket(slot);Click("Unfit selected fitting module");UiCheck(State()==simUiBaseline,"UI unfit leaves campaign unchanged");}
  if(stage==2){fitBrowseTab=0;UiCheck(fittingSimulation!=null&&!fittingWorkshop,"Simulation retains circular layout");}
  if(stage==3){var h=FleetCatalog.Playable.Take(8).Last();TestClick(ui.Hits.Last(x=>x.Tip==ShortInfo(h.Name,33)).Rect.Center.ToVector2());UiCheck(FittingWorld.Player.CombatHull==h.Id,"UI hull selection changes simulated ship");}
  if(stage==4){ResetFittingSimulation();UiCheck(FittingWorld.Player.CombatHull==world.Player.CombatHull,"Reset returns to live hull copy");}
  if(stage==5){Click("Toggle fitting simulation");UiCheck(fittingSimulation==null&&ReferenceEquals(FittingWorld,world),"Return to live discards simulation");UiCheck(State()==simUiBaseline,"Simulation UI leaves complete campaign fit and stocks unchanged");}
 }

 FittingSimulation? fittingSimulation;
 DemoWorld FittingWorld=>fittingSimulation?.World??world;
 void ToggleFittingSimulation(){if(fittingSimulation==null){fittingSimulation=new(world.Player);FitHistoryCapture();fitNotice="Simulation: free catalogue. Save designs to keep them; campaign changes remain separate.";}else{fittingSimulation=null;fitNotice="Live fitting restored. Simulation discarded.";}fitSelectedSocket="";fitOwnedOnly=false;fitSearchFocus=false;fitBrowsePage=0;}
 void ResetFittingSimulation(){fittingSimulation?.Reset(world.Player);fitSelectedSocket="";fitBrowsePage=0;fitNotice="Simulation reset from the current live ship; cycles stopped.";}
}
