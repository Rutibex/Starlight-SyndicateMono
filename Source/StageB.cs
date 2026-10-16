using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record CombatEventB(DateTime Time,string Source,string Target,string Action,float Raw,float Applied,float Probability);
public sealed class WeaponStoredB{public WeaponState State{get;set;}=new();public string Station{get;set;}="";public bool Hangar{get;set;}=true;}
public sealed partial class Ship
{
 public float[] RackHeat{get;set;}=new float[3];
 public object? TacticalModule(FittingModule m)=>m.Kind switch{"weapon"=>Weapons[m.Index],"defense"=>Defense[m.Index],"ewar"=>Ewar[m.Index],"support"=>Support[m.Index],"enhancement"=>Enhancements[m.Index],"mobility"=>Mobility[m.Index],"utility"=>Utilities[m.Index],"special"=>Special[m.Index],_=>null};
}
public sealed partial class DemoWorld
{
 public List<CombatEventB> CombatEvents{get;set;}=[];
 public List<WeaponStoredB> WeaponStorage{get;set;}=[];
 public ulong TacticalRandomState{get;set;}=21977;
 public double TacticalRemainder{get;set;}
 public long TacticalTick{get;set;}
 public double CombatRoll(){ulong x=TacticalRandomState==0?21977:TacticalRandomState;x^=x<<13;x^=x>>7;x^=x<<17;TacticalRandomState=x;return (x>>11)*(1.0/9007199254740992.0);}
 public bool ModuleUnderRepairB(string kind,object state)=>Player.NaniteJobs.Any(j=>j.Kind==kind&&(j.InstanceId.Length>0?j.InstanceId==(string?)state.GetType().GetProperty("InstanceId")?.GetValue(state):j.ModuleId==(string?)state.GetType().GetProperty("ModuleId")?.GetValue(state)));
 void CombatRecord(string source,string target,string action,float raw,float applied,float probability=1){var entry=new CombatEventB(Now,source,target,action,raw,applied,probability);CombatEvents.Add(entry);ArchiveCombatI(entry);if(CombatEvents.Count>200)CombatEvents.RemoveRange(0,CombatEvents.Count-200);}
 public WeaponSolution WeaponSolution(WeaponState state)=>TacticalRulesB.Weapon(Player,state,EwarFor("player"));
 public void TacticalUpdate(float dt)
 {
  if(SimulationPaused||Screen==ViewMode.Title||!float.IsFinite(dt)||dt<=0)return;
  // UI windows do not suspend active-grid combat. Every command shares a 50-ms simulation step.
  if(Docked){UpdateStepB(dt*SimulationSpeed);return;}
  TacticalRemainder+=dt*SimulationSpeed;
  while(TacticalRemainder>=.05-1e-9){TacticalRemainder-=.05;TacticalTick++;UpdateStepB(.05f);}
 }
 public void ApplyModuleHeat(object state,float normalizedDamage,Ship? ship=null)
 {
  ship??=Player;if(normalizedDamage<=0)return;var installed=FittingLayoutModel.Installed(ship);var source=installed.FirstOrDefault(m=>ReferenceEquals(ship.TacticalModule(m),state));if(source==null)return;
  int rack=source.Bank=="high"?0:source.Bank=="medium"?1:2;var modules=installed.Where(m=>m.Bank==source.Bank&&m.Kind!="rig").ToArray();int origin=Array.IndexOf(modules,source);
  string suffix=rack==0?"Hi":rack==1?"Med":"Low";float capacity=(float)Math.Max(1,ship.Stat("heatCapacity"+suffix));
  ship.RackHeat[rack]=Math.Clamp(ship.RackHeat[rack]+normalizedDamage*40/capacity*(float)Math.Max(0,ship.Stat("heatGenerationMultiplier")),0,1);
  float attenuation=(float)Math.Clamp(ship.Stat("heatAttenuation"+suffix),0,1);
  for(int i=0;i<modules.Length;i++){
   var module=ship.TacticalModule(modules[i]);if(module==null)continue;var type=module.GetType();var damage=type.GetProperty("Damage");if(damage==null||type.GetProperty("Burnt")==null)continue;
   bool own=ReferenceEquals(module,state);float spread=own?1:ship.RackHeat[rack]*.35f*MathF.Pow(attenuation,Math.Abs(i-origin));if(spread<=0)continue;
   float n=Math.Clamp((float)damage.GetValue(module)!+normalizedDamage*spread,0,1);damage.SetValue(module,n);
   if(n>=1){type.GetProperty("Burnt")!.SetValue(module,true);type.GetProperty("Active")?.SetValue(module,false);type.GetProperty("Overheat")?.SetValue(module,false);foreach(string timer in new[]{"PendingRepair","PendingAmount","EffectLeft","WarmupLeft"})type.GetProperty(timer)?.SetValue(module,0f);CombatRecord(ship.InstanceId,modules[i].Id,"Heat burnout",0,n);}
  }
 }
 void CoolRacks(Ship ship,float dt){for(int i=0;i<3;i++){string suffix=i==0?"Hi":i==1?"Med":"Low";ship.RackHeat[i]*=MathF.Exp(-(float)Math.Max(0,ship.Stat("heatDissipationRate"+suffix))*dt);}}
 public bool SetModuleOverheat(FittingModule module,bool enabled)
 {
  var state=Player.TacticalModule(module);if(state==null||module.Burnt)return false;var t=state.GetType();if(t.GetProperty("Overheat") is not {} heat)return false;if(enabled&&Player.NaniteJobs.Any(j=>j.Kind==module.Kind&&j.Index==module.Index))return false;
  heat.SetValue(state,enabled);return true;
 }
 public bool RepairTacticalModule(FittingModule module)
 {
  if(!Docked||!RequireService44(13))return false;var state=Player.TacticalModule(module);if(state==null)return false;var t=state.GetType();if(t.GetProperty("Damage") is not {} damage)return false;
  float n=(float)damage.GetValue(state)!;if(t.GetProperty("Burnt")?.GetValue(state) is true)n=1;decimal cost=ItemCatalog.Get(module.Id).Price*.12m*(decimal)n;if(WalletFunds<cost)return false;
  WalletChange(-cost,"Module repair: "+module.Id);damage.SetValue(state,0f);t.GetProperty("Burnt")?.SetValue(state,false);t.GetProperty("Overheat")?.SetValue(state,false);return true;
 }
 void StoreWeapon(WeaponState state){if(WeaponCatalog.ById.ContainsKey(state.ModuleId)){state.Stop();state.Overheat=false;WeaponStorage.Add(new(){State=state,Station=StationKey44});}}
 WeaponState TakeWeapon(string id,bool hangar){var stored=WeaponStorage.FirstOrDefault(s=>s.State.ModuleId==id&&s.Hangar==hangar&&(!hangar||s.Station==StationKey44));if(stored==null)return WeaponState.Create(id);WeaponStorage.Remove(stored);stored.State.Online=true;return stored.State;}
 void TransferWeaponCondition(string id,bool hangar,int q){foreach(var x in WeaponStorage.Where(s=>s.State.ModuleId==id&&s.Hangar==hangar&&(!hangar||s.Station==StationKey44)).Take(q)){x.Hangar=!hangar;x.Station=StationKey44;}}
 public bool SetBasicModuleOnlineB(FittingModule module,bool online){var state=Player.TacticalModule(module);if(state==null||online&&SkillError(module.Id).Length>0)return false;var t=state.GetType();if(t.GetProperty("Online") is not {} prop||t.GetProperty("Active")?.GetValue(state) is true)return false;if(online&&module.Burnt)return false;if(prop.GetValue(state) is bool current&&current==online)return true;if(online&&!Docked&&Player.Cap<Player.MaxCap*.95f)return false;prop.SetValue(state,online);if(TotalFitCpu>Player.Stat("cpuOutput")+.001||TotalFitPower>Player.Stat("powerOutput")+.001){prop.SetValue(state,!online);return false;}if(online&&!Docked)Player.Cap=Math.Max(0,Player.Cap-Player.MaxCap*.66f);RecomputeWeaponFit();return true;}
 public bool SetWeaponOnline(int index,bool online){var s=Player.WeaponAt(index);if(s==null||s.Active||s.WarmupLeft>0||s.EffectLeft>0)return false;var m=FittingLayoutModel.Installed(Player).FirstOrDefault(m=>m.Kind=="weapon"&&m.Index==index);return m!=null&&SetBasicModuleOnlineB(m,online);}

 public void UpdateNpcCombatB(float dt)
 {
  foreach(var t in Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&t.DroneId.Length==0&&MissionNpcActiveL(t)&&PopulationCanFightC(t)&&(!t.Training||t.Id.StartsWith("q-guard:")||t.Id.StartsWith("f:")&&!t.Friendly)&&t.Profile!=null).ToArray()){
   var ship=t.Profile!;
   if(!t.TacticalProvisioned){t.TacticalProvisioned=true;ship.Weapons=[WeaponState.Create("rail")];ship.UtilityFit="";var s=ship.Weapons[0];s.Loaded=s.Definition.Magazine;t.NpcAmmunition[s.ChargeId]=120;}
   CoolRacks(ship,dt);
   t.Cap=TacticalRulesB.RechargeAfter(t.Cap,ship.MaxCap,(float)ship.Stat("rechargeRate")/1000,dt);
   t.Shield=TacticalRulesB.RechargeAfter(t.Shield,ship.MaxShield,(float)ship.Stat("shieldRechargeRate")/1000,dt);
   for(int slot=0;slot<ship.Weapons.Length;slot++){
    var s=ship.WeaponAt(slot);if(s==null)continue;var w=s.Definition;s.Cooldown=Math.Max(0,s.Cooldown-dt);s.ReloadLeft=Math.Max(0,s.ReloadLeft-dt);
    if(!s.Online||s.Burnt||s.Cooldown>0||s.ReloadLeft>0||t.LockId.Length==0||!CanLock(t.Id,t.LockId)||ship.SpecialOffenseBlockedB())continue;
    var target=t.LockId=="player"?null:Targets.FirstOrDefault(x=>x.Id==t.LockId&&x.Alive);if(t.LockId!="player"&&target==null)continue;
    var q=TacticalRulesB.Weapon(ship,s,EwarFor(t.Id));var at=target?.Position??Player.Position;float range=Vector3.Distance(t.Position,at);
    if(w.NeedsLock&&range>q.Gun.Optimal*(s.Charge?.RangeMultiplier??1)+q.Gun.Falloff*3*(s.Charge?.RangeMultiplier??1)&&w.UsesTurret)continue;
    if(w.Kind is not (WeaponKind.Turret or WeaponKind.Entropic or WeaponKind.Missile))continue;
    if(w.Magazine>0&&s.Loaded<=0){int qty=Math.Min(w.Magazine,t.NpcAmmunition.GetValueOrDefault(s.ChargeId));if(qty<=0)continue;t.NpcAmmunition[s.ChargeId]-=qty;s.Loaded=qty;s.ReloadLeft=w.Reload*ship.HullWeapon(w,"reload");continue;}
    if(t.Cap<q.Capacitor)continue;t.Cap-=q.Capacitor;s.Cooldown=q.Cycle;if(w.Magazine>0&&s.Charge?.Crystal!=true)s.Loaded--;if(ManagedPopulationTarget(t.Id)&&w.Magazine>0&&s.Charge?.Crystal!=true)Population.GoodsConsumed++;
    if(w.UsesTurret){var direction=range>0?(at-t.Position)/range:Vector3.UnitX;float angular=Vector3.Cross((target?.Velocity??Player.Velocity)-t.Velocity,direction).Length()/Math.Max(1,range);float signature=target==null?Player.Signature*EwarFor("player").Signature:EffectiveSignature(target);float chance=TurretChance(q.Gun,s.Charge,range,angular,signature),quality=TacticalRulesB.HitQuality(chance,CombatRoll());float before=target==null?Player.Shield+Player.Armor+Player.Hull:target.Shield+target.Armor+target.Hull;
     if(quality>0){if(target==null)ApplyPlayerPacket(q.Packet.Scale(quality));else ApplyPacket(target,q.Packet.Scale(quality),false,t.Id);}float after=target==null?Player.Shield+Player.Armor+Player.Hull:target.Shield+target.Armor+target.Hull;CombatRecord(t.Id,t.LockId,w.Name,q.Packet.Total*quality,before-after,chance);
    }else{var delta=at-t.Position;MissilesInFlight.Add(new(){Owner=t.Id,Enemy=true,TargetId=t.LockId,Position=t.Position,Direction=delta.LengthSquared()>0?Vector3.Normalize(delta):Vector3.UnitX,Packet=q.Packet,Damage=q.Packet.Total,Speed=q.Velocity,Life=q.FlightTime,ExplosionRadius=q.ExplosionRadius,ExplosionVelocity=q.ExplosionVelocity,Reduction=q.Reduction,Hp=12});}
    if(s.Overheat)ApplyModuleHeat(s,.075f*ship.HullFactor("hull","heatDamage"),ship);
   }
  }
 }
}
public sealed partial class Ship{public bool SpecialOffenseBlockedB()=>Special.Any(s=>s.Active&&(s.Definition?.Cloak==true||s.Definition?.Kind=="Triage"));}
