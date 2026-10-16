using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class RetainedLock{public string Id{get;set;}="";public bool Silent{get;set;}}
public sealed class TargetAcquisition{public string Id{get;set;}="";public float Left{get;set;}public bool Silent{get;set;}}
public sealed partial class DemoWorld
{
 public List<RetainedLock> RetainedLocks{get;set;}=[];public List<TargetAcquisition> Acquisitions{get;set;}=[];
 public bool TargetingUpgraded=>Player.Enhancements.Any(s=>s.Online&&!s.Burnt&&s.Definition?.Kind is "Sensor" or "SensorPassive" or "Auto" or "Passive");
 public int TargetCapacity=>Math.Clamp((int)Player.Stat("maxLockedTargets"),0,12);
 public float AcquisitionSeconds(SpaceTarget t)=>Math.Clamp(40000/Math.Max(1,(float)Player.Stat("scanResolution")*EwarFor("player").ScanResolution)/MathF.Pow(MathF.Asinh(Math.Max(1,EffectiveSignature(t))),2),.1f,120);
 public bool BeginAcquisition(string id,bool silent=false)
 {
  var t=Targets.FirstOrDefault(t=>t.Id==id&&t.Alive);if(Docked||t==null||silent&&t.Kind!=TargetKind.Raider||!CanLock("player",id))return false;
  if(RetainedLocks.Any(l=>l.Id==id)){LockedId=id;return true;}if(Acquisitions.Any(l=>l.Id==id))return true;
  if(RetainedLocks.Count+Acquisitions.Count>=TargetCapacity){Say("Target limit reached; unlock a target.");return false;}
  if(!silent)t.PlayerLockNotified=true;
  Acquisitions.Add(new(){Id=id,Left=AcquisitionSeconds(t),Silent=silent});Say((silent?"Silent":"Sensor")+" acquisition: "+t.Name+".");return true;
 }
 public void UnlockEnhancedTarget(string id){RetainedLocks.RemoveAll(l=>l.Id==id);Acquisitions.RemoveAll(l=>l.Id==id);if(LockedId==id)LockedId=RetainedLocks.FirstOrDefault()?.Id??"";}
 public void RevealEnhancedLock(string id)
 {var l=RetainedLocks.FirstOrDefault(l=>l.Id==id);if(l==null||!l.Silent)return;l.Silent=false;var t=Targets.FirstOrDefault(t=>t.Id==id);if(t!=null)t.PlayerLockNotified=true;Say("Offensive action revealed lock on "+(Targets.FirstOrDefault(t=>t.Id==id)?.Name??id)+".");}
 public void ClearEnhancedLocks(){RetainedLocks.Clear();Acquisitions.Clear();}
 public void StopEnhancement(EnhancementState s){s.Active=false;s.CycleLeft=0;s.StopRequested=false;s.CycleHeat=1;}
 public bool ToggleEnhancement(int slot)
 {
  if(slot<0||slot>=14)return false;var s=Player.Enhancements[slot];var d=s.Definition;if(d==null||!d.Active)return false;
  if(s.Active){if(d.Kind is "Auto" or "Passive")StopEnhancement(s);else s.StopRequested=true;return true;}
  if(Docked||!s.Online||s.Burnt||Player.Cap<d.Capacitor){Say("Activation requires undocked, online, intact equipment and capacitor.");return false;}
  s.Active=true;s.StopRequested=false;StartEnhancementCycle(s);return true;
 }
 void StartEnhancementCycle(EnhancementState s)
 {
  var d=s.Definition!;if(Player.Cap<d.Capacitor){StopEnhancement(s);return;}Player.Cap-=d.Capacitor;s.CycleLeft=d.Cycle;s.CycleHeat=s.Overheat&&d.HeatDamage>0?1+d.Overload:1;
  if(d.Kind=="Auto")foreach(var t in Targets.Where(t=>t.Alive&&!t.Friendly&&t.Kind==TargetKind.Raider&&Vector3.Distance(Player.Position,t.Position)<=d.Range).OrderBy(t=>Vector3.DistanceSquared(Player.Position,t.Position)))BeginAcquisition(t.Id);
  if(s.Overheat&&d.HeatDamage>0){ApplyModuleHeat(s,d.HeatDamage*Player.HullFactor("hull","heatDamage")/Math.Max(1,d.ModuleHp));if(s.Damage>=1){s.Burnt=true;StopEnhancement(s);Say(d.Name+" burnt out.");}}
 }
 public void UpdateEnhancements(float dt)
 {
  if(dt<=0)return;
  foreach(var s in Player.Enhancements.Where(s=>s.Definition?.Active==true))
  {
   if(Docked||!s.Online||s.Burnt){StopEnhancement(s);continue;}if(!s.Active)continue;
   float left=dt;while(left>0&&s.Active){float step=Math.Min(left,Math.Max(.0001f,s.CycleLeft));s.CycleLeft=Math.Max(0,s.CycleLeft-step);left-=step;if(s.CycleLeft>0)continue;
    
    if(s.StopRequested||!s.Repeat||s.Definition!.Kind=="Passive")StopEnhancement(s);else StartEnhancementCycle(s);
   }
  }
  // Legacy single-focus locks are promoted without destroying previous save behavior.
  if(Locked!=null&&CanLock("player",LockedId)&&!RetainedLocks.Any(l=>l.Id==LockedId)&&RetainedLocks.Count<TargetCapacity)RetainedLocks.Add(new(){Id=LockedId});
  RetainedLocks.RemoveAll(l=>!CanLock("player",l.Id));Acquisitions.RemoveAll(l=>!CanLock("player",l.Id));
  while(RetainedLocks.Count>TargetCapacity)UnlockEnhancedTarget(RetainedLocks.Last().Id);
  while(RetainedLocks.Count+Acquisitions.Count>TargetCapacity)Acquisitions.RemoveAt(Acquisitions.Count-1);
  foreach(var a in Acquisitions.ToArray())
  {a.Left-=dt;if(a.Left<=0){Acquisitions.Remove(a);RetainedLocks.Add(new(){Id=a.Id,Silent=a.Silent});LockedId=a.Id;}}
  if(LockedId.Length>0&&Locked==null)LockedId=RetainedLocks.FirstOrDefault()?.Id??"";
 }
 public bool SilentLockArmed=>Player.Enhancements.Any(s=>s.Active&&s.Definition?.Kind=="Passive");
 public bool TryEnhancedManualLock()
 {if(!TargetingUpgraded)return false;if(Selected==null)return true;bool silent=SilentLockArmed;if(BeginAcquisition(Selected.Id,silent)&&silent)foreach(var s in Player.Enhancements.Where(s=>s.Definition?.Kind=="Passive"&&s.Active))StopEnhancement(s);return true;}
 public DroneDefinition EffectiveDrone(DroneUnit u)
 {
  var d=u.Definition;if(u.Owner!="player"||u.Location is "Abandoned" or "Hangar" or "Destroyed")return d;return d.Enhanced(Player);
 }
 public void RefreshEnhancedFighters(bool normalize=false)
 {
  foreach(var u in Player.Drones.Where(u=>u.Owner=="player"&&u.Definition.Fighter&&u.Location!="Destroyed"))
  {float factor=!normalize&&u.Location is "Bay" or "Reserve" or "Space"?Player.EnhancementFactor("fighterShield"):1,old=u.AppliedShieldFactor;if(old<=0)old=1;float ratio=factor/old;u.Shield*=ratio;foreach(var h in u.ReserveHealth)h.Shield*=ratio;u.AppliedShieldFactor=factor;var b=Targets.FirstOrDefault(t=>t.Id==u.Id);if(b!=null){b.Shield*=ratio;b.Profile?.SetStat("shieldCapacity",u.Definition.Shield*factor);}}
 }
}
public sealed partial class DroneDefinition
{
 public DroneDefinition Enhanced(Ship owner)
 {
  var d=(DroneDefinition)MemberwiseClone();d.Packet=Packet.Scale(owner.DroneDamageFactor(Kind=="Sentry")*owner.LightCombatFactor(this,"damage")*owner.SpecificDroneFactor(this,"damage")*owner.SpecialFactor("droneDamage"));d.Speed=Speed*(Fighter?owner.NanoswarmFighterO:1)*owner.HullFactor("drone","speed",Effect)*owner.SpecificDroneFactor(this,"speed")*owner.SpecialFactor("droneVelocity")*owner.EnhancementFactor("droneSpeed",!Fighter)*(Fighter?owner.EnhancementFactor("fighterSpeed"):1);d.Tracking=Tracking*owner.HullFactor("drone","tracking",Effect)*owner.SpecificDroneFactor(this,"tracking")*owner.EnhancementFactor("droneTracking");d.Optimal=Optimal*owner.HullFactor("drone","optimal",Effect)*owner.SpecificDroneFactor(this,"optimal")*owner.EnhancementFactor("droneOptimal",!Fighter);d.Falloff=Falloff*owner.EnhancementFactor("droneFalloff");
  if(Fighter){d.Cycle=Cycle*owner.EnhancementFactor("fighterCycle");d.Shield=Shield*owner.EnhancementFactor("fighterShield");d.ShieldRecharge=ShieldRecharge*owner.EnhancementFactor("fighterRecharge");d.AttackExplosionRadius=AttackExplosionRadius*owner.EnhancementFactor("fighterExplosionRadius");d.AttackExplosionVelocity=AttackExplosionVelocity*owner.EnhancementFactor("fighterExplosionVelocity");d.Abilities=Abilities.Select(a=>new DroneAbility{Kind=a.Kind,Name=a.Name,Cooldown=a.Cooldown*(a.Kind=="Salvo"?owner.EnhancementFactor("fighterCycle"):1),Charges=a.Charges,Rearm=a.Rearm,Range=a.Range*(a.Kind=="Salvo"?owner.EnhancementFactor("droneOptimal",!Fighter):1),Packet=a.Packet.Scale(a.Kind=="Salvo"?owner.EnhancementFactor("droneDamage")*owner.SpecialFactor("droneDamage"):1),ExplosionRadius=a.ExplosionRadius*(a.Kind=="Salvo"?owner.EnhancementFactor("fighterExplosionRadius"):1),ExplosionVelocity=a.ExplosionVelocity*(a.Kind=="Salvo"?owner.EnhancementFactor("fighterExplosionVelocity"):1)}).ToArray();}
  d.Repair=Repair*(Fighter?1:owner.RigFactor("droneRepair"))*owner.HullFactor("drone","repair",Effect);if(!Fighter&&Effect=="Web")d.Strength=Strength*owner.HullFactor("drone","strength",Effect)*owner.RigFactor("droneWeb");d.Shield*=(Fighter?1:owner.RigFactor("droneHP",false)*owner.HullFactor("drone","hp",Effect)*owner.LightCombatFactor(this,"hp")*owner.SpecificDroneFactor(this,"hp"))*owner.SpecialFactor("droneHP");d.Armor=Armor*(Fighter?1:owner.RigFactor("droneHP",false)*owner.HullFactor("drone","hp",Effect)*owner.LightCombatFactor(this,"hp")*owner.SpecificDroneFactor(this,"hp"))*owner.SpecialFactor("droneHP");d.Hull=Hull*(Fighter?1:owner.RigFactor("droneHP",false)*owner.HullFactor("drone","hp",Effect)*owner.LightCombatFactor(this,"hp")*owner.SpecificDroneFactor(this,"hp"))*owner.SpecialFactor("droneHP");d.Yield=Yield*owner.HullFactor("drone","mining",Effect)*owner.RigFactor("droneMining",false)*owner.SpecialFactor("droneMining");if(Kind=="Ice")d.MiningCycle=MiningCycle*owner.HullFactor("drone","iceCycle")*owner.RigFactor("droneIceCycle",false)*owner.SpecialFactor("droneIceCycle");return d;
 }
}
