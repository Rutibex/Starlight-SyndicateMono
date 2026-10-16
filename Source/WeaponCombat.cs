using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public readonly record struct DamagePacket(float EM,float Thermal,float Kinetic,float Explosive)
{
 public float Total=>EM+Thermal+Kinetic+Explosive;
 public static DamagePacket Single(float amount,DamageType type)=>type switch{DamageType.EM=>new(amount,0,0,0),DamageType.Thermal=>new(0,amount,0,0),DamageType.Kinetic=>new(0,0,amount,0),_=>new(0,0,0,amount)};
 public DamagePacket Scale(float n)=>new(EM*n,Thermal*n,Kinetic*n,Explosive*n);
 public float EffectiveResonance(Ship? profile,string layer)
 {if(Total<=0)return 1;float[] fallback=layer=="shield"?[1,.8f,.6f,.5f]:layer=="armor"?[.5f,.65f,.75f,.9f]:[1,1,1,1];float[] a=[EM,Thermal,Kinetic,Explosive];return Enumerable.Range(0,4).Sum(i=>a[i]*(profile==null?fallback[i]:1-profile.Resistance(layer,(DamageType)i)))/Total;}
}
public sealed class ParasiteEffect
{
 public string TargetId{get;set;}="";public float Remaining{get;set;} public float Dps{get;set;} public float Tick{get;set;}
}
public sealed class WeaponVisual
{
 public Vector3 Start{get;set;} public Vector3 End{get;set;}public WeaponKind Kind{get;set;}public DamageType DamageType{get;set;}public float Remaining{get;set;}=.3f;public float Radius{get;set;}
}
public sealed partial class DemoWorld
{
 public List<ParasiteEffect> Parasites{get;set;}=[];
 public List<WeaponVisual> WeaponVisuals{get;set;}=[];
 float bombSpawnTimer=9;
 public static float TurretChance(WeaponDefinition w,ChargeDefinition? c,float range,float angular,float signature)
 {
  return TacticalRulesB.TurretProbability(w.Optimal*(c?.RangeMultiplier??1),w.Falloff*(c?.RangeMultiplier??1),w.Tracking*(c?.TrackingMultiplier??1),w.SignatureResolution,range,angular,signature);
 }
 public static float MissileApplication(float signature,float speed,float radius,float velocity,float reduction)=>TacticalRulesB.MissileFraction(signature,speed,radius,velocity,reduction);

 public static bool BeamContains(Vector3 origin,Vector3 direction,Vector3 point,float range,float radius)
 {var delta=point-origin;float along=Vector3.Dot(delta,direction);return along>=0&&along<=range&&(delta-direction*along).LengthSquared()<=radius*radius;}
 public static bool ConeContains(Vector3 origin,Vector3 direction,Vector3 point,float range)
 {var delta=point-origin;float d=delta.Length();return d<=range&&d>0&&Vector3.Dot(delta/d,direction)>=.8f;}
 public void ToggleWeapon(int slot)
 {
  var s=Player.WeaponAt(slot);if(s==null)return;
  if(s.Active){s.Stop();Say(s.Definition.Name+" deactivating.");return;}
  string reason=ActivationError(s);
  if(reason.Length>0){Say(reason);return;}
  s.Active=true;Say(s.Definition.Name+" active.");
 }
 public string ActivationError(WeaponState s)
 {
  var d=s.Definition;
  if(!s.Online)return "Module is offline.";if(s.Burnt)return "Module burnt out; repair at a station.";if(Player.NaniteJobs.Any(j=>j.Kind=="weapon"&&Player.Weapons.ElementAtOrDefault(j.Index)==s))return "Module is being repaired with nanites.";
  if(SpecialOffenseBlocked)return "Cloak or capital mode blocks weapons.";if(d.Kind==WeaponKind.DisruptiveLance&&!Player.HasSpecial("Siege")&&!WeaponTestMode)return "Activate siege before firing a disruptive lance.";if(Docked)return "Undock before firing.";
  if((Player.SensorJam>0||Locked!=null&&!CanLock("player",LockedId))&&d.NeedsLock)return "Sensors jammed.";
  if(Player.WeaponLockout>0)return "Superweapon interlock active.";
  if(d.NeedsLock&&(Locked==null||Locked.Kind!=TargetKind.Raider))return "Lock a ship target first.";
  if(d.Kind==WeaponKind.Doomsday&&(Locked?.Profile==null||CombatHulls.Get(Locked.Profile.CombatHull).Size<4))return "Cataclysm strikes require a capital target.";
  if((d.Superweapon||d.Kind==WeaponKind.Bomb)&&Current.Security>=.5f&&!WeaponTestMode)return "This weapon is prohibited in secure systems.";
  if(d.Kind is WeaponKind.Lance or WeaponKind.Reaper or WeaponKind.Cone &&Current.Security>=.4f&&!WeaponTestMode)return "Area superweapons require an unregulated system.";
  if(d.Kind==WeaponKind.Smartbomb&&!WeaponTestMode&&Targets.Any(t=>t.Kind==TargetKind.Station&&Vector3.Distance(t.Position,Player.Position)<d.Radius+500))return "Station exclusion radius blocks this smartbomb.";
  if(WeaponSafety&&(d.Kind is WeaponKind.Smartbomb or WeaponKind.Arc or WeaponKind.Bomb or WeaponKind.Lance or WeaponKind.Reaper or WeaponKind.Cone or WeaponKind.DisruptiveLance))
  {
   float reach=d.Kind==WeaponKind.Smartbomb?d.Radius:d.Kind==WeaponKind.Arc?d.Optimal+d.Radius:d.Kind==WeaponKind.Bomb?d.Velocity*d.FlightTime+d.Radius:d.Optimal;
   if(Targets.Any(t=>t.Alive&&t.Friendly&&Vector3.Distance(t.Position,Player.Position)<reach))return "Safety blocks fire near a friendly vessel. Disable safety deliberately in fitting.";
  }
  if(WeaponSafety&&Locked?.Friendly==true)return "Safety blocks attacking a friendly vessel.";
  return "";
 }
 void UpdateWeaponCombat(float dt)
 {
  float left=dt;
  while(left>0){float step=Math.Min(.05f,left);left-=step;WeaponStep(step);}
 }
 void WeaponStep(float dt)
 {
  UpdateEwar(dt);UpdateDrones(dt);
  Player.WeaponLockout=Math.Max(0,Player.WeaponLockout-dt);Player.DriveLockout=Math.Max(0,Player.DriveLockout-dt);Player.SensorJam=Math.Max(0,Player.SensorJam-dt);
  foreach(var t in Targets){t.DriveLockout=Math.Max(0,t.DriveLockout-dt);t.SensorJam=Math.Max(0,t.SensorJam-dt);if(t.DriveLockout==0)t.RemoteRepairMultiplier=1;}
  foreach(var v in WeaponVisuals)v.Remaining-=dt;WeaponVisuals.RemoveAll(v=>v.Remaining<=0);
  for(int i=0;i<Player.Weapons.Length;i++)
  {
   var s=Player.WeaponAt(i);if(s==null)continue;var w=s.Definition;
   s.Cooldown=Math.Max(0,s.Cooldown-dt);s.ReloadLeft=Math.Max(0,s.ReloadLeft-dt);if(s.ReloadLeft==0)s.PendingCharge="";
   if(s.WarmupLeft>0)
   {
    s.WarmupLeft=Math.Max(0,s.WarmupLeft-dt);
    if(s.WarmupLeft==0)BeginSuperweapon(s);continue;
   }
   if(s.EffectLeft>0)
   {
    float tick=Math.Min(dt,s.EffectLeft);FireBeam(s,tick);s.EffectLeft=Math.Max(0,s.EffectLeft-dt);continue;
   }
   if(!s.Active||s.Cooldown>0||s.ReloadLeft>0)continue;
   string error=ActivationError(s);if(error.Length>0){s.Stop();Say(error);continue;}
   var target=Locked;
   if(w.NeedsLock&&target!=null&&Vector3.Distance(Player.Position,target.Position)>WeaponReach(s)){s.RampCycles=0;s.RampTarget="";continue;}
   if(w.Magazine>0&&s.Loaded==0){bool active=s.Active;ReloadWeapon(i);s.Active=active&&s.ReloadLeft>0;if(!s.Active)Say("Weapon stopped: no ammunition.");continue;}
   float cap=WeaponSolution(s).Capacitor;
   if(Player.Cap<cap){s.Stop();Say("Weapon stopped: insufficient capacitor.");continue;}
   if(w.Kind==WeaponKind.Defender)
   {
    var bomb=MissilesInFlight.Where(m=>m.Bomb&&m.Enemy&&m.Hp>0&&Vector3.Distance(m.Position,Player.Position)<=w.Optimal).OrderBy(m=>Vector3.DistanceSquared(m.Position,Player.Position)).FirstOrDefault();
    if(bomb==null)continue;
    Consume(s);MissilesInFlight.Add(Ordnance(s,bomb.Id,true));Say("Sentinel interceptor launched at incoming bomb.");continue;
   }
   if(w.NeedsLock&&target!=null&&!LegalAttackQ("player",target)){s.Stop();continue;}Consume(s);Player.Cap-=cap;if(target!=null)RevealEnhancedLock(target.Id);
   if(w.Superweapon)
   {
    s.WarmupLeft=w.Warmup;s.WarmupTarget=LockedId;s.Active=false;
    Vector3 delta=(Selected?.Position??Player.ManualPoint)-Player.Position;var aim=delta.LengthSquared()>1?Vector3.Normalize(delta):Vector3.Forward;
    s.AimX=aim.X;s.AimY=aim.Y;s.AimZ=aim.Z;Player.DriveLockout=Math.Max(Player.DriveLockout,w.Warmup+w.Duration+8);Player.WeaponLockout=Math.Max(Player.WeaponLockout,w.Warmup+w.Duration);
    Say(w.Name+" winding up; aim is committed.");continue;
   }
   switch(w.Kind)
   {
    case WeaponKind.Turret:case WeaponKind.Entropic:FireTurret(s,target!);break;
    case WeaponKind.Missile:case WeaponKind.Breacher:MissilesInFlight.Add(Ordnance(s,target!.Id));break;
    case WeaponKind.Arc:FireArc(s,target!);break;
    case WeaponKind.Smartbomb:Radial(s);break;
    case WeaponKind.Bomb:
     var bomb=Ordnance(s,"");var delta=(Selected?.Position??Player.ManualPoint)-Player.Position;
     bomb.Direction=delta.LengthSquared()>1?Vector3.Normalize(delta):Vector3.Forward;MissilesInFlight.Add(bomb);s.Active=false;Say("Bomb released on an unguided timed trajectory.");break;
   }
  }
  Player.TurretOn=Player.Weapons[0].Active;Player.MissileOn=Player.WeaponAt(1)?.Active==true;
  Player.TurretTimer=Player.Weapons[0].Cooldown;Player.MissileTimer=Player.Weapons[1].Cooldown;
  UpdateOrdnance(dt);UpdateParasites(dt);
  if(WeaponTestMode&&Grid=="range")
  {
   bombSpawnTimer-=dt;if(bombSpawnTimer<=0){bombSpawnTimer=15;Vector3 from=Player.Position+new Vector3(0,0,-15000);MissilesInFlight.Add(new(){Position=from,Direction=Vector3.UnitZ,Enemy=true,Bomb=true,Life=10,Speed=1500,Radius=9000,Damage=450,Hp=70,Packet=DamagePacket.Single(450,DamageType.Explosive),ExplosionRadius=350});Say("Range control: incoming practice bomb.");}
  }
 }
 public float WeaponReach(WeaponState s)=>s.Definition.Kind is WeaponKind.Missile or WeaponKind.Breacher?s.Definition.Velocity*s.Definition.FlightTime*(s.Definition.Kind==WeaponKind.Missile?Player.EnhancementFactor("missileVelocity")*Player.SpecialMissileVelocity(s.Definition)*Player.HullWeapon(s.Definition,"missileVelocity")*Player.EnhancementFactor("missileFlightTime")*Player.HullWeapon(s.Definition,"missileFlightTime"):1):s.Definition.Optimal*(s.Charge?.RangeMultiplier??1)*(s.Definition.Kind is WeaponKind.Turret or WeaponKind.Entropic?Player.WeaponProjection(s.Definition,"optimal"):1)+s.Definition.Falloff*3*(s.Charge?.RangeMultiplier??1)*(s.Definition.Kind is WeaponKind.Turret or WeaponKind.Entropic?Player.WeaponProjection(s.Definition,"falloff"):1);
 void Consume(WeaponState s){s.Cooldown=WeaponSolution(s).Cycle;if(s.Overheat&&!s.Definition.Superweapon)ApplyModuleHeat(s,.075f*Player.HullFactor("hull","heatDamage"));if(s.Definition.Magazine>0&&s.Charge?.Crystal!=true)s.Loaded--;}
 DamagePacket Packet(WeaponState s,float scale=1)
 {
  return WeaponSolution(s).Packet.Scale(scale);
 }
 void FireTurret(WeaponState s,SpaceTarget target)
 {
  var w=s.Definition;float range=Vector3.Distance(Player.Position,target.Position);Vector3 dir=range>0?(target.Position-Player.Position)/range:Vector3.UnitX;
  float angular=Vector3.Cross(target.Velocity-Player.Velocity,dir).Length()/Math.Max(1,range);
  var model=WeaponSolution(s).Gun;
  float chance=TurretChance(model,s.Charge,range,angular,EffectiveSignature(target));
  if(w.Kind==WeaponKind.Entropic){if(s.RampTarget!=target.Id)s.RampCycles=0;s.RampTarget=target.Id;}
  float ramp=w.Kind==WeaponKind.Entropic?1+Math.Min(1.5f*Player.HullWeapon(w,"rampMax"),s.RampCycles*(.055f+Player.HullFlat("weapon","ramp")/100)):1;
  double roll=CombatRoll();if(roll<chance)
  {
   float quality=TacticalRulesB.HitQuality(chance,roll);ApplyPacket(target,Packet(s,ramp*quality));Visual(Player.Position,target.Position,w);
   Say(w.Name+" hit / "+(chance*100).ToString("0")+"% solution / ramp "+ramp.ToString("0.00")+"x");
  }
  else Say(w.Name+" misses.");
  if(w.Kind==WeaponKind.Entropic)s.RampCycles++;
 }
 void FireArc(WeaponState s,SpaceTarget first)
 {
  var w=s.Definition;var candidates=Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&t.Id!=first.Id&&Vector3.Distance(t.Position,first.Position)<=w.Radius).OrderBy(t=>Vector3.DistanceSquared(t.Position,first.Position)).Take(4).Prepend(first);
  foreach(var t in candidates.ToArray())
  {float app=MissileApplication(EffectiveSignature(t),t.Velocity.Length(),w.ExplosionRadius*EwarFor("player").ExplosionRadius,w.ExplosionVelocity*EwarFor("player").ExplosionVelocity,w.DamageReduction);ApplyPacket(t,Packet(s,app),weapon:w.Kind);Visual(t==first?Player.Position:first.Position,t.Position,w);}
 }
 Missile Ordnance(WeaponState s,string target,bool interceptor=false)
 {
  var w=s.Definition;var q=WeaponSolution(s);return new(){Position=Player.Position,TargetId=target,Life=q.FlightTime,Speed=q.Velocity,Damage=q.Packet.Total,Packet=q.Packet,Hp=w.Kind==WeaponKind.Bomb?70:w.Kind==WeaponKind.Breacher?18:12,Bomb=w.Kind==WeaponKind.Bomb,Interceptor=interceptor,Breacher=w.Kind==WeaponKind.Breacher,Radius=w.Radius,ExplosionRadius=q.ExplosionRadius,ExplosionVelocity=q.ExplosionVelocity,Reduction=q.Reduction,Duration=w.Duration,Payload=s.Charge?.Payload??"damage",Owner="player"};
 }
 void UpdateOrdnance(float dt)
 {
  foreach(var m in MissilesInFlight.ToArray())
  {
   if(m.Hp<=0){MissilesInFlight.Remove(m);continue;}
   float step=Math.Min(dt,m.Life);m.Life-=dt;
   if(m.Bomb){m.Position+=m.Direction*m.Speed*step;if(m.Life<=0){Detonate(m);MissilesInFlight.Remove(m);}continue;}
   if(m.Interceptor)
   {
    var bomb=MissilesInFlight.FirstOrDefault(b=>b.Id==m.TargetId&&b.Bomb&&b.Enemy&&b.Hp>0);
    if(bomb==null||m.Life<=0){MissilesInFlight.Remove(m);continue;}
    if(AdvanceTo(m,bomb.Position,step)){bomb.Hp-=m.Damage;MissilesInFlight.Remove(m);Say("Incoming bomb intercepted.");}continue;
   }
   var target=Targets.FirstOrDefault(t=>t.Id==m.TargetId&&t.Alive&&t.Kind==TargetKind.Raider);
   if(m.TargetId=="player"){
    if(Player.Hull<=0||Cloaked){MissilesInFlight.Remove(m);continue;}
    if(AdvanceTo(m,Player.Position,step)){var packet=m.Packet.Scale(MissileApplication(Player.Signature*EwarFor("player").Signature,Player.Velocity.Length(),m.ExplosionRadius,m.ExplosionVelocity,m.Reduction));float before=Player.Shield+Player.Armor+Player.Hull;ApplyPlayerPacket(packet);CombatRecord(m.Owner,"player","Missile impact",packet.Total,before-Player.Shield-Player.Armor-Player.Hull);MissilesInFlight.Remove(m);}else if(m.Life<=0)MissilesInFlight.Remove(m);continue;
   }
   if(target==null){MissilesInFlight.Remove(m);continue;}
   if(AdvanceTo(m,target.Position,step))
   {
    if(m.Breacher)
    {float maxhp=target.Profile==null?target.Shield+target.Armor+target.Hull:target.Profile.MaxShield+target.Profile.MaxArmor+target.Profile.MaxHull;Parasites.Add(new(){TargetId=target.Id,Remaining=m.Duration,Dps=Math.Min(m.Damage,maxhp*.007f)});Say("Parasite pod attached to "+target.Name+".");}
    else ApplyPacket(target,m.Packet.Scale(MissileApplication(EffectiveSignature(target),target.Velocity.Length(),m.ExplosionRadius,m.ExplosionVelocity,m.Reduction)),playerAttack:m.Owner=="player",source:m.Owner);
    Visual(m.Position,target.Position,new(){Kind=WeaponKind.Missile,DamageType=m.Packet.EM>0?DamageType.EM:DamageType.Explosive});MissilesInFlight.Remove(m);
   }
   else if(m.Life<=0)MissilesInFlight.Remove(m);
  }
  MissilesInFlight.RemoveAll(m=>m.Hp<=0);
 }
 static bool AdvanceTo(Missile m,Vector3 at,float dt)
 {var delta=at-m.Position;float travel=m.Speed*dt;if(delta.Length()<=travel+20)return true;if(delta.LengthSquared()>0)m.Position+=Vector3.Normalize(delta)*travel;return false;}
 void Detonate(Missile bomb)
 {
  WeaponVisuals.Add(new(){Start=bomb.Position,End=bomb.Position,Kind=WeaponKind.Bomb,Radius=bomb.Radius,Remaining=.7f,DamageType=DamageType.Explosive});
  foreach(var t in Targets.Where(t=>t.Kind==TargetKind.Raider&&t.Alive&&Vector3.Distance(t.Position,bomb.Position)<=bomb.Radius).ToArray())
  {
   if(bomb.Payload=="jam"){t.SensorJam=12;continue;}
   if(bomb.Payload.Contains("neutral")){float apply=bomb.Payload=="focusedneutral"?Math.Min(1,t.Signature/10000):1;t.Cap=Math.Max(0,t.Cap-bomb.Damage*apply);continue;}
   ApplyPacket(t,bomb.Packet.Scale(Math.Min(1,EffectiveSignature(t)/Math.Max(1,bomb.ExplosionRadius))),weapon:WeaponKind.Bomb);
  }
  if(Vector3.Distance(Player.Position,bomb.Position)<=bomb.Radius)
  {
   if(bomb.Payload=="jam"){Player.SensorJam=12;LockedId="";}
   else if(bomb.Payload.Contains("neutral"))Player.Cap=Math.Max(0,Player.Cap-bomb.Damage*(bomb.Payload=="focusedneutral"?Math.Min(1,Player.Signature/10000):1));
   else ApplyPlayerPacket(bomb.Packet.Scale(Math.Min(1,Player.Signature/Math.Max(1,bomb.ExplosionRadius))));
  }
  foreach(var other in MissilesInFlight.Where(m=>m!=bomb&&Vector3.Distance(m.Position,bomb.Position)<=bomb.Radius).ToArray())
   other.Hp-=bomb.Packet.Total*(other.Bomb?.01f:1); // bomb shells strongly resist blast damage; other ordnance is fragile.
  Say("Bomb detonated / "+bomb.Payload+".");
 }
 void Radial(WeaponState s)
 {
  var w=s.Definition;
  foreach(var t in Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&Vector3.Distance(Player.Position,t.Position)<=w.Radius).ToArray())ApplyPacket(t,Packet(s),weapon:w.Kind);
  foreach(var m in MissilesInFlight.Where(m=>Vector3.Distance(m.Position,Player.Position)<=w.Radius))m.Hp-=w.Damage;
  WeaponVisuals.Add(new(){Start=Player.Position,End=Player.Position,Kind=w.Kind,DamageType=w.DamageType,Radius=w.Radius,Remaining=.6f});
 }
 void BeginSuperweapon(WeaponState s)
 {
  if(s.Definition.Kind==WeaponKind.Doomsday)
  {
   var t=Targets.FirstOrDefault(t=>t.Id==s.WarmupTarget&&t.Alive);
   if(t!=null&&Vector3.Distance(t.Position,Player.Position)<=s.Definition.Optimal){ApplyPacket(t,Packet(s));Visual(Player.Position,t.Position,s.Definition);}
   Say("Cataclysm strike completed.");
  }
  else
  {
   s.EffectLeft=s.Definition.Duration;
   if(s.Definition.Kind==WeaponKind.DisruptiveLance)
   {foreach(var t in Targets.Where(t=>t.Alive&&Vector3.Distance(t.Position,Player.Position)<5000))t.Cap=Math.Max(0,t.Cap-700);}
  }
 }
 void FireBeam(WeaponState s,float dt)
 {
  var w=s.Definition;var dir=new Vector3(s.AimX,s.AimY,s.AimZ);
  if(w.Kind==WeaponKind.Reaper){float elapsed=w.Duration-s.EffectLeft;dir=Vector3.Transform(dir,Matrix.CreateRotationY(-.35f+.7f*elapsed/w.Duration));}
  foreach(var t in Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider).ToArray())
  {
   bool hit=w.Kind==WeaponKind.Cone?ConeContains(Player.Position,dir,t.Position,w.Optimal):BeamContains(Player.Position,dir,t.Position,w.Optimal,w.Radius);
   if(!hit)continue;
   float app=Math.Min(1,EffectiveSignature(t)/(w.Kind==WeaponKind.Cone?8000:2200));ApplyPacket(t,Packet(s,app*dt),weapon:w.Kind);
   if(w.Kind==WeaponKind.DisruptiveLance){t.DriveLockout=60;t.RemoteRepairMultiplier=.5f;}
  }
  Visual(Player.Position,Player.Position+dir*w.Optimal,w,.1f);
 }
 void UpdateParasites(float dt)
 {
  // Every effect has its own lifetime. Only the strongest live attachment on a ship applies damage; others expire concurrently.
  foreach(var group in Parasites.GroupBy(p=>p.TargetId).ToArray())
  {
   var target=Targets.FirstOrDefault(t=>t.Id==group.Key&&t.Alive);var strongest=group.OrderByDescending(p=>p.Dps).First();
   if(target!=null){strongest.Tick+=Math.Min(dt,strongest.Remaining);while(strongest.Tick>=1){strongest.Tick-=1;ApplyRaw(target,strongest.Dps);}}
  }
  foreach(var p in Parasites)p.Remaining-=dt;Parasites.RemoveAll(p=>p.Remaining<=0||!Targets.Any(t=>t.Id==p.TargetId&&t.Alive));
 }
 void Visual(Vector3 from,Vector3 to,WeaponDefinition w,float life=.3f)=>WeaponVisuals.Add(new(){Start=from,End=to,Kind=w.Kind,DamageType=w.DamageType,Remaining=life});
 void ApplyRaw(SpaceTarget t,float raw)
 {if(!t.Alive||t.Profile?.NexusLeft>0||ProtectedTargetO(t)||ProtectedP(t)||ProtectedStructureQ(t))return;raw*=StructureScaleQ(t,raw);raw=DrainResisted(ref t.Shield,raw,0);raw=DrainResisted(ref t.Armor,raw,0);DrainResisted(ref t.Hull,raw,0);DamageDenP(t);DamageStructureQ(t);RecordKill(t);}
 void ApplyPacket(SpaceTarget t,DamagePacket packet,bool playerAttack=true,string source="player",WeaponKind? weapon=null)
 {
  if(ProtectedTargetO(t)||ProtectedP(t)||ProtectedStructureQ(t)||!LegalAttackQ(source,t))return;packet=packet.Scale(StructureScaleQ(t,packet.Total));packet=packet.Scale(FabricatorWeaponScaleO(t,weapon));
  if(playerAttack&&ManagedPopulationTarget(t.Id))AggressPopulationC(t);
  if(!t.Alive||packet.Total<=0||t.Profile?.NexusLeft>0)return;
  if(playerAttack)WakeMissionGroupL(t);
  var result=TacticalRulesB.Damage(t.Profile,packet.Scale(Player.Drones.Any(u=>u.Id==t.DroneId&&u.EvasionLeft>0)?.6f:1),t.Shield,t.Armor,t.Hull);float before=t.Shield+t.Armor+t.Hull;t.Shield=result.Shield;t.Armor=result.Armor;t.Hull=result.Hull;
  RecordEncounterContributionL(t,source,before-t.Shield-t.Armor-t.Hull);CreditP(source.StartsWith("npc:")?source[4..]:source,before-t.Shield-t.Armor-t.Hull);DamageDenP(t);DamageStructureQ(t);
  if(playerAttack)CombatRecord("player",t.Id,"Damage",packet.Total,before-t.Shield-t.Armor-t.Hull);
  if(t.Friendly&&playerAttack){SyndicateStanding-=.01f;if(Current.Security>=.5f&&!WeaponTestMode)DealPlayerDamage(30,DamageType.EM);}RecordKill(t);
 }
 void ApplyPlayerPacket(DamagePacket packet)
 {if(Player.NexusLeft>0)return;ObserveDefenseDamage(packet);var result=TacticalRulesB.Damage(Player,packet,Player.Shield,Player.Armor,Player.Hull);Player.Shield=result.Shield;Player.Armor=result.Armor;Player.Hull=result.Hull;}

 void RecordKill(SpaceTarget t)
 {
  if(t.Hull>0||t.KillRecorded)return;if(RecordPopulationLossC(t))return;if(t.DroneId.Length>0){DroneLoss(t);return;}bool missionDrop=SealMissionKillL(t)||SealEncounterKillL(t);if(!missionDrop)Targets.Add(new(){Id="wreck_"+t.Id,Name=t.Name+" wreck",Kind=TargetKind.Wreck,Position=t.Position,Salvage=3});if(ActiveEncounterF?.RunP!=null&&t.Contents.Count>0&&Targets.FirstOrDefault(w=>w.Id=="wreck_"+t.Id) is {} wreckP)foreach(var cargoP in t.Contents)wreckP.Contents[cargoP.Key]=wreckP.Contents.GetValueOrDefault(cargoP.Key)+cargoP.Value;t.KillRecorded=true;EncounterKillF(t);MissionFactionKill38(t);MissionKill38(t);if(!t.Training){float gain=Player.SecurityGainFactor;Player.PilotSecurity=Math.Clamp(Player.PilotSecurity+(t.Friendly?-.2f:.02f*gain),-10,10);foreach(var b in FleetBerths)b.Ship.PilotSecurity=Player.PilotSecurity;}Say(t.Name+" destroyed.");
  if(!t.Training&&!t.Friendly){if(!t.Id.StartsWith("mission:")&&!t.Id.StartsWith("navy:"))_destroyedRaiders[SystemId+(Grid=="signal"?":signal":"")]=true;if(!missionDrop)WalletChange(70,"NPC bounty: "+t.Name);}
  if(LockedId==t.Id)LockedId="";
 }
}
