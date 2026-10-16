using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;

public enum TacticalStage { Subsystem, Defense, Support, Enhancement, Propulsion, Utility, CapitalMode, Rig, Hull, Booster, Skill, Environment }
public sealed record TacticalContext(string Owner,string Hull,int SourceType,string Attribute);
public sealed record AttributeStep(TacticalStage Stage,double Before,double After);
public sealed record AttributeExplanation(TacticalContext Context,double Base,double Value,AttributeStep[] Steps);
public sealed record WeaponSolution(WeaponDefinition Gun,DamagePacket Packet,float Cycle,float Capacitor,float Velocity,float FlightTime,float ExplosionRadius,float ExplosionVelocity,float Reduction);
public sealed record DamageResult(float Shield,float Armor,float Hull,float RemainingRaw);
public static class TacticalRulesB
{
 public const string Profile="Starlight tactical B / source build 3569502";
 // Separate positive and negative classes before calling. Rank zero is unpenalized.
 public static double StackWeight(int rank)=>Math.Pow(.5,Math.Pow(Math.Max(0,rank)/2.22292081,2));
 public static float Stack(IEnumerable<float> strengths,bool increase=false)
 {
  double value=1;int rank=0;
  foreach(float strength in strengths.OrderByDescending(Math.Abs)){
   if(!float.IsFinite(strength))throw new ArgumentException("Nonfinite modifier");
   value*=1+(increase?1:-1)*Math.Clamp(strength,0,increase?float.MaxValue:1)*StackWeight(rank++);
  }return (float)Math.Max(0,value);
 }
 public static float TurretProbability(float optimal,float falloff,float tracking,float resolution,float range,float angular,float signature)
 {
  if(!float.IsFinite(range)||!float.IsFinite(angular)||signature<=0||range<0)return 0;
  double motion=angular==0?0:tracking<=0?double.PositiveInfinity:Math.Abs(angular)*resolution/(tracking*signature);
  double distance=range<=optimal?0:falloff<=0?double.PositiveInfinity:(range-optimal)/falloff;
  return (float)Math.Pow(.5,motion*motion+distance*distance);
 }
 public static float HitQuality(float probability,double roll)=>roll<0||roll>=Math.Clamp(probability,0,1)?0:roll<.01?3:(float)(.49+roll);
 public static float MissileFraction(float signature,float speed,float radius,float velocity,float exponent)
 {
  if(!float.IsFinite(signature)||!float.IsFinite(speed)||radius<=0||velocity<0||exponent<=0||signature<=0)return 0;
  double size=signature/radius,motion=speed<=0?double.PositiveInfinity:size*velocity/speed;
  return (float)Math.Clamp(Math.Min(1,Math.Min(size,Math.Pow(Math.Max(0,motion),exponent))),0,1);
 }
 public static float RechargeAfter(float amount,float maximum,float seconds,float elapsed)
 {
  if(maximum<=0||seconds<=0||elapsed<=0)return Math.Clamp(amount,0,Math.Max(0,maximum));
  // Exact solution of dq/dt = 10*C/T*(sqrt(q/C)-q/C), avoiding frame-dependent Euler drift.
  double y=Math.Sqrt(Math.Clamp(amount/maximum,0,1));
  if(y==0)return 0;
  y=1-(1-y)*Math.Exp(-5*elapsed/seconds);return (float)(maximum*y*y);
 }
 public static DamageResult Damage(Ship? ship,DamagePacket packet,float shield,float armor,float hull)
 {
  if(packet.Total<=0||!float.IsFinite(packet.Total)||packet.EM<0||packet.Thermal<0||packet.Kinetic<0||packet.Explosive<0)return new(shield,armor,hull,0);
  float raw=packet.Total;
  float Drain(float pool,string layer){if(pool<=0)return 0;float factor=packet.EffectiveResonance(ship,layer);if(factor<=0){raw=0;return pool;}float used=Math.Min(pool,raw*factor);pool-=used;raw=Math.Max(0,raw-used/factor);return pool;}
  shield=Drain(Math.Max(0,shield),"shield");armor=Drain(Math.Max(0,armor),"armor");hull=Drain(Math.Max(0,hull),"hull");return new(shield,armor,hull,raw);
 }
 public static WeaponSolution Weapon(Ship ship,WeaponState state,EwarSnapshot? ew=null)
 {
  ew??=new();var w=state.Definition;var c=state.Charge;
  float cycle=w.Cycle*ship.WeaponEnhancement(w,"cycle")*ship.SkillWeapon(w,"cycle"),cap=w.Capacitor*ship.RigWeapon(w,"cap")*(c?.CapMultiplier??1);
  float damage=w.Damage*(c?.DamageMultiplier??1)*ship.WeaponEnhancement(w,"damage")*ship.SkillWeapon(w,"damage");
  damage*=(float)ship.WormholeModifiersO.GetValueOrDefault("damageMultiplierMultiplier",1);if(w.Size==1)damage*=(float)ship.WormholeModifiersO.GetValueOrDefault("smallWeaponDamageMultiplier",1);
  if(state.Overheat&&!state.Burnt&&state.Online&&!w.Superweapon){damage*=1.15f;cycle*=.85f;}
  DamagePacket packet;
  if(w.Superweapon||w.Kind==WeaponKind.Smartbomb)packet=DamagePacket.Single(damage,w.DamageType);
  else if(w.Laser)packet=new(damage*.7f,damage*.3f,0,0);
  else if(w.Family is "Blasters" or "Railguns")packet=new(0,damage*.35f,damage*.65f,0);
  else if(w.Kind==WeaponKind.Entropic)packet=new(0,damage*.65f,0,damage*.35f);
  else if(w.Kind==WeaponKind.Arc)packet=new(damage*.55f,0,damage*.45f,0);
  else if(w.Family is "Artillery" or "Autocannons"){
   packet=DamagePacket.Single(damage*.7f,c?.DamageType??DamageType.Explosive);
   packet=new(packet.EM,packet.Thermal+damage*.1f,packet.Kinetic+damage*.2f,packet.Explosive);
  }else packet=DamagePacket.Single(damage*ship.HullWeapon(w,"damage",(c?.DamageType??w.DamageType).ToString())/ship.HullWeapon(w,"damage"),c?.DamageType??w.DamageType);
  var model=w.Id=="rail"?w with{Tracking=(float)(ship.Attributes["turretTracking"]??w.Tracking),Optimal=(float)(ship.Attributes["turretOptimal"]??w.Optimal),Falloff=(float)(ship.Attributes["turretFalloff"]??w.Falloff)}:w;
  model=model with{Tracking=model.Tracking*(float)ship.WormholeModifiersO.GetValueOrDefault("trackingSpeedMultiplier",1)*ew.Tracking*ship.WeaponProjection(w,"tracking"),Optimal=model.Optimal*ship.NanoswarmRangeO*ew.TurretRange*ship.WeaponProjection(w,"optimal")/ship.SpecialFactor("optimal")*ship.SpecialRange(w,"optimal")*((w.Laser||w.Family is "Blasters" or "Railguns")?ship.SpecialFactor("energyHybridOptimal"):1),Falloff=model.Falloff*ew.TurretRange*ship.WeaponProjection(w,"falloff")/ship.SpecialFactor("falloff")*ship.SpecialRange(w,"falloff")};
  bool missile=w.Kind==WeaponKind.Missile;
  return new(model,packet,Math.Max(.01f,cycle),Math.Max(0,cap),w.Velocity*(float)ship.WormholeModifiersO.GetValueOrDefault("missileVelocityMultiplier",1)*(missile?ship.EnhancementFactor("missileVelocity")*ship.SpecialMissileVelocity(w)*ship.HullWeapon(w,"missileVelocity")*ship.MissileSkillFactor("velocity"):1),w.FlightTime*ship.NanoswarmFlightO*ew.MissileRange*(missile?ship.EnhancementFactor("missileFlightTime")*ship.HullWeapon(w,"missileFlightTime")*ship.MissileSkillFactor("flightTime"):1),w.ExplosionRadius*(float)ship.WormholeModifiersO.GetValueOrDefault("aoeCloudSizeMultiplier",1)*ew.ExplosionRadius*(missile?ship.EnhancementFactor("explosionRadius")*ship.HullWeapon(w,"explosionRadius")*ship.MissileSkillFactor("explosionRadius"):1),w.ExplosionVelocity*(float)ship.WormholeModifiersO.GetValueOrDefault("aoeVelocityMultiplier",1)*ew.ExplosionVelocity*(missile?ship.EnhancementFactor("explosionVelocity")*ship.HullWeapon(w,"explosionVelocity")*ship.MissileSkillFactor("explosionVelocity"):1),w.DamageReduction);
 }
}
public sealed partial class Ship
{
 public double TacticalAttribute(string key,List<AttributeStep>? trace=null)
 {
  if(!Attributes.TryGetValue(key,out var original))throw new ArgumentException("Unknown ship stat: "+key);
  double value=original??0;
  for(int i=0;i<=11;i++){
   var stage=(TacticalStage)i;double before=value;
   value=stage switch{TacticalStage.Environment=>EnvironmentStatF(key,value),TacticalStage.Subsystem=>SubsystemBase(key,value),TacticalStage.Defense=>DefenseStat(key,value),TacticalStage.Support=>SupportStat(key,value),TacticalStage.Enhancement=>EnhancementStat(key,value),TacticalStage.Propulsion=>MobilityStat(key,value),TacticalStage.Utility=>UtilityStat(key,value),TacticalStage.CapitalMode=>SpecialStat(key,value),TacticalStage.Rig=>RigStat(key,value),TacticalStage.Hull=>HullStat(key,value),TacticalStage.Booster=>BoosterStat(key,value)*(key=="shieldCapacity"?PilotShieldD:1),_=>SkillShipStat(key,value)};
   if(!double.IsFinite(value))throw new InvalidOperationException($"Nonfinite {key} at {stage}");
   trace?.Add(new(stage,before,value));
  }return value;
 }
 public AttributeExplanation ExplainAttribute(string key){var trace=new List<AttributeStep>();double value=TacticalAttribute(key,trace);return new(new(InstanceId,CombatHull,SkillCatalog.TypeId(CombatHull),key),Attributes[key]??0,value,trace.ToArray());}
 public Dictionary<string,double> TacticalSnapshot()=>ShipAttributeCatalog.All.Where(a=>LiveKeys.Contains(a.Key)).ToDictionary(a=>a.Key,a=>Stat(a.Key));
}
