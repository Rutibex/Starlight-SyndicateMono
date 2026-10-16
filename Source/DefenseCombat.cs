using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public string DefenseActivationError(int index)
 {
  if(index<0||index>=12)return "Invalid defense socket.";var s=Player.Defense[index];var d=s.Definition;
  if(ModuleUnderRepairB("defense",s))return "Module is being repaired with nanites.";
  if(!s.Online)return "Module offline.";
  if(d==null||!d.Active)return "This socket has no active defense.";
  if(Cloaked)return "Decloak before activation.";if(Docked)return "Undock before activation.";if(s.Burnt)return "Module burnt out; station repair required.";
  if(s.ReloadLeft>0)return "Magazine is reloading.";if(s.ReactivationLeft>0)return "Module reactivation delay.";
  if(d.Kind=="CapBooster"&&s.Loaded==0)return "Load capacitor cells first.";
  if(d.Kind=="Nullifier"&&Player.Ewar.Any(e=>e.Definition?.Kind==EwarKind.BurstJam))return "Fitted burst jammer disables nullification.";
  float cap=Player.DefenseQuoteB(s).Capacitor;
  return Player.Cap<cap?"Insufficient capacitor.":"";
 }
 public bool ToggleDefense(int index)
 {
  if(index<0||index>=12)return false;var s=Player.Defense[index];var d=s.Definition;if(d==null)return false;
  if(s.Active){s.Active=false;if(s.AutoReload&&s.CycleLeft<=0&&s.Loaded==0&&s.Payload?.Consumable==true)ReloadDefense(index);Say(d.Name+" stops after its current cycle.");return true;}
  string error=DefenseActivationError(index);if(error.Length>0){Say(error);return false;}
  s.Active=true;if(d.Kind=="Reactive"){s.ReactiveResists=[.135f,.135f,.135f,.135f];s.Incoming=[0,0,0,0];}
  if(s.CycleLeft<=0)StartDefenseCycle(s);Say(d.Name+" active.");return true;
 }
 public bool ReloadDefense(int index)
 {
  if(index<0||index>=12)return false;var s=Player.Defense[index];if(s.CycleLeft>0||s.ReloadLeft>0||s.Burnt||s.PayloadId.Length==0)return false;return LoadDefensePayload(index,s.PayloadId);
 }
 void StartDefenseCycle(DefenseState s)
 {
  var d=s.Definition!;
  if(s.ReloadLeft>0||s.ReactivationLeft>0||s.Burnt){s.Active=false;return;}
  if(d.Kind=="CapBooster"&&s.Loaded==0){s.Active=false;if(s.AutoReload)ReloadDefense(Array.IndexOf(Player.Defense,s));return;}
  float cap=Player.DefenseQuoteB(s).Capacitor;
  if(Player.Cap<cap){s.Active=false;Say(d.Name+" stopped: capacitor exhausted.");return;}Player.Cap-=cap;
  s.Overheat&=d.HeatDamage>0;var quote=Player.DefenseQuoteB(s);s.CycleLeft=quote.Duration;s.EffectLeft=s.CycleLeft;s.ChargedCycle=false;
  float amount=quote.Amount;
  if(d.Kind=="AncillaryArmor") {int use=Math.Min(s.Loaded,d.ChargeUse);s.Loaded-=use;s.ChargedCycle=use>0;}
  if(d.Kind=="AncillaryShield"&&s.Loaded>0){s.Loaded--;s.ChargedCycle=true;}
  if(d.Kind=="CapBooster") {Player.Cap=Math.Min(Player.MaxCap,Player.Cap+(s.Payload?.Amount??0));s.Loaded--;}
  if(d.Kind is "ShieldBoost" or "AncillaryShield")Player.Shield=Math.Min(Player.MaxShield,Player.Shield+amount);
  if(d.DelayedRepair)s.PendingRepair=amount;
  if(d.Burst){s.Active=false;s.ReactivationLeft=s.CycleLeft+d.Reactivation*(d.Kind=="Nullifier"?Player.NullifierFactor("reactivation"):1);}
  if(s.Overheat){ApplyModuleHeat(s,d.HeatDamage*Player.HullFactor("hull","heatDamage")/Math.Max(1,d.ModuleHp)*(d.Kind is "ShieldBoost" or "AncillaryShield"?Player.Defense.Where(a=>!a.Burnt).Aggregate(1f,(heat,a)=>heat*(1+(a.Definition?.Bonuses.GetValueOrDefault("shieldBoostHeatDamage")??0))):1));if(s.Damage>=1){s.Burnt=true;s.Active=false;s.PendingRepair=0;s.EffectLeft=0;Say(d.Name+" burnt out from heat.");}}
 }
 void ShiftReactive(DefenseState s)
 {
  float total=s.Incoming.Sum();if(total<=0)return;float pool=s.ReactiveResists.Sum(),step=s.Definition!.ReactiveShift;
  var desired=s.Incoming.Select(v=>pool*v/total).ToArray();float gained=0;
  for(int i=0;i<4;i++) {float loss=Math.Min(step,Math.Max(0,s.ReactiveResists[i]-desired[i]));s.ReactiveResists[i]-=loss;gained+=loss;}
  float deficit=Enumerable.Range(0,4).Sum(i=>Math.Max(0,desired[i]-s.ReactiveResists[i]));
  if(deficit>0)for(int i=0;i<4;i++)s.ReactiveResists[i]+=gained*Math.Max(0,desired[i]-s.ReactiveResists[i])/deficit;
  s.Incoming=[0,0,0,0];
 }
 public void ObserveDefenseDamage(DamagePacket packet)
 {
  // Only the part of a hit which reaches armor informs reactive membranes.
  float raw=packet.Total;float shieldRes=packet.EffectiveResonance(Player,"shield");float remaining=raw-Math.Min(raw,Player.Shield/Math.Max(.01f,shieldRes));if(remaining<=0||Player.Armor<=0)return;
  foreach(var s in Player.Defense.Where(s=>s.Running&&s.Definition?.Kind=="Reactive"))
  {s.Incoming[0]+=remaining*packet.EM/Math.Max(.001f,raw);s.Incoming[1]+=remaining*packet.Thermal/Math.Max(.001f,raw);s.Incoming[2]+=remaining*packet.Kinetic/Math.Max(.001f,raw);s.Incoming[3]+=remaining*packet.Explosive/Math.Max(.001f,raw);}
 }
 public void UpdateDefense(float dt)
 {
  float left=dt;while(left>0){float step=Math.Min(.05f,left);left-=step;foreach(var s in Player.Defense)
  {
   if(s.Definition is not {} d)continue;s.ReactivationLeft=Math.Max(0,s.ReactivationLeft-step);s.ReloadLeft=Math.Max(0,s.ReloadLeft-step);s.EffectLeft=Math.Max(0,s.EffectLeft-step);
   bool finishing=s.CycleLeft>0;s.CycleLeft=Math.Max(0,s.CycleLeft-step);
   if(finishing&&s.CycleLeft<=0)
   {
    if(!s.Burnt&&s.PendingRepair>0){if(d.Kind=="HullRepair")Player.Hull=Math.Min(Player.MaxHull,Player.Hull+s.PendingRepair);else Player.Armor=Math.Min(Player.MaxArmor,Player.Armor+s.PendingRepair);s.PendingRepair=0;}
    if(d.Kind=="Reactive")ShiftReactive(s);
    if(d.Kind=="Emergency"){s.Burnt=true;s.Damage=1;s.Active=false;Say(d.Name+" spent: repair at station before another activation.");}
    if(!s.Repeat)s.Active=false;
    if(s.AutoReload&&(d.Kind=="CapBooster"||!s.Active)&&s.Loaded==0&&s.Payload?.Consumable==true&&ItemCount(s.PayloadId,false)>0){bool resume=s.Active;s.Active=false;ReloadDefense(Array.IndexOf(Player.Defense,s));s.Active=resume;}
   }
   if(s.Active&&s.CycleLeft<=0&&s.ReloadLeft<=0&&!s.Burnt)StartDefenseCycle(s);
  }}
  Player.BoosterOn=Player.Defense.Any(s=>s.Active&&s.Definition?.Kind is "ShieldBoost" or "AncillaryShield");Player.BoosterTimer=Player.Defense.FirstOrDefault(s=>s.Definition?.Kind is "ShieldBoost" or "AncillaryShield")?.CycleLeft??0;
 }
 public float DefenseIncomingGunChance(SpaceTarget source,SpaceTarget? craft=null)
 {
  var p=source.Profile??Ship.RaiderProfile();var position=craft?.Position??Player.Position;var velocity=craft?.Velocity??Player.Velocity;float distance=Vector3.Distance(source.Position,position);var direction=distance>0?(position-source.Position)/distance:Vector3.UnitX;float angular=Vector3.Cross(velocity-source.Velocity,direction).Length()/Math.Max(1,distance);var ew=EwarFor(source.Id);
  var gun=WeaponCatalog.Get("rail") with{Tracking=(float)p.Stat("turretTracking")*ew.Tracking,Optimal=(float)p.Stat("turretOptimal")*ew.TurretRange,Falloff=(float)p.Stat("turretFalloff")*ew.TurretRange};
  return TurretChance(gun,null,distance,angular,craft==null?Player.Signature*EwarFor("player").Signature:EffectiveSignature(craft));
 }
 public float DefenseNpcLockSeconds(SpaceTarget source,string target)
 {
  float signature=target=="player"?Player.Signature*EwarFor("player").Signature:Targets.FirstOrDefault(t=>t.Id==target) is {} t?EffectiveSignature(t):82;
  float baseline=target=="player"?(float)(Player.Attributes["signatureRadius"]??82):82;
  double sig=Math.Pow(Math.Asinh(Math.Max(1,baseline))/Math.Asinh(Math.Max(1,signature)),2);var p=source.Profile;double scan=p==null?1:p.Stat("scanResolution")/(p.Attributes["scanResolution"]??1);
  return source.LockDelay*(float)sig/(float)Math.Max(.05,scan);
 }
 public void StopDefenses()
 {foreach(var s in Player.Defense){s.Active=false;s.EffectLeft=0;s.CycleLeft=0;s.PendingRepair=0;}Player.BoosterOn=false;}
}
