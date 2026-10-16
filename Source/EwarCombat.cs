using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class EwarEffect
{
 public string SourceId{get;set;}="player";
 public string TargetId{get;set;}="";
 public string Key{get;set;}="";
 public EwarKind Kind{get;set;}
 public string Mode{get;set;}="Combined";
 public float Strength{get;set;}
 public float Remaining{get;set;}
}
public sealed class EwarZone
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");
 public string SourceId{get;set;}="player";
 public string FollowId{get;set;}="";
 public string ModuleId{get;set;}="";
 public int SourceSlot{get;set;}=-1;
 public EwarKind Kind{get;set;}
 public Vector3 Center{get;set;}
 public float Radius{get;set;}
 public float Strength{get;set;}
 public float Remaining{get;set;}
 public string Mode{get;set;}="Combined";
}
public sealed class EwarSnapshot
{
 public float Speed=1,Signature=1,LockRange=1,ScanResolution=1,Tracking=1,TurretRange=1,MissileRange=1,ExplosionRadius=1,ExplosionVelocity=1;
 public float WarpStrength;
 public bool Scrammed,Bubble;
 public string[] Jammers=[];
}
public sealed partial class DemoWorld
{
 public List<EwarEffect> EwarEffects{get;set;}=[];
 public List<EwarZone> EwarZones{get;set;}=[];
 public string PendingLockId{get;set;}="";
 public float LockTimeLeft{get;set;}
 public int SelectedEwarSlot{get;set;}=1;
 float npcEwarTimer=6;
 public static float RangeScale(float distance,float optimal,float falloff)=>distance<=optimal?1:falloff<=0?0:MathF.Pow(.5f,MathF.Pow((distance-optimal)/falloff,2));
 public static float StackMultiplier(IEnumerable<float> strengths,bool increase=false)
 =>TacticalRulesB.Stack(strengths,increase);
 Ship Profile(string id)=>id=="player"?Player:Targets.First(t=>t.Id==id).Profile??Player;
 Vector3 At(string id)=>id=="player"?Player.Position:Targets.FirstOrDefault(t=>t.Id==id)?.Position??Vector3.Zero;
 bool ShipAlive(string id)=>id=="player"?Player.Hull>0:Targets.Any(t=>t.Id==id&&t.Alive&&t.Kind==TargetKind.Raider);
 float CapAt(string id)=>id=="player"?Player.Cap:Targets.First(t=>t.Id==id).Cap;
 void SetCapAt(string id,float value){if(id=="player")Player.Cap=Math.Clamp(value,0,Player.MaxCap);else {var t=Targets.First(t=>t.Id==id);t.Cap=Math.Clamp(value,0,t.Profile?.MaxCap??185);}}
 public EwarSnapshot EwarFor(string id)
 {
  var effects=EwarEffects.Where(e=>e.TargetId==id&&e.Remaining>0).ToArray();var snapshot=new EwarSnapshot();var profile=Profile(id);
  float Mult(EwarKind kind,Func<EwarEffect,bool>? filter=null,bool increase=false)=>StackMultiplier(effects.Where(e=>e.Kind==kind&&(filter==null||filter(e))).Select(e=>e.Strength*(kind is EwarKind.Damp or EwarKind.Tracking or EwarKind.Guidance&&profile.SpecialEcmImmune?.4f:1)),increase);
  snapshot.Speed=StackMultiplier(effects.Where(e=>e.Kind is EwarKind.Web or EwarKind.Grapple).Select(e=>e.Strength));
  snapshot.Signature=Mult(EwarKind.Paint,increase:true);
  snapshot.LockRange=Mult(EwarKind.Damp,e=>e.Mode is "Combined" or "Range");snapshot.ScanResolution=Mult(EwarKind.Damp,e=>e.Mode is "Combined" or "Resolution");
  snapshot.Tracking=Mult(EwarKind.Tracking,e=>e.Mode is "Combined" or "Tracking");snapshot.TurretRange=Mult(EwarKind.Tracking,e=>e.Mode is "Combined" or "Range");
  snapshot.MissileRange=Mult(EwarKind.Guidance,e=>e.Mode is "Combined" or "Range");snapshot.ExplosionRadius=1/Mult(EwarKind.Guidance,e=>e.Mode is "Combined" or "Application");snapshot.ExplosionVelocity=Mult(EwarKind.Guidance,e=>e.Mode is "Combined" or "Application");
  snapshot.WarpStrength=effects.Where(e=>e.Kind is EwarKind.Point or EwarKind.Scram).Sum(e=>e.Strength);snapshot.Scrammed=effects.Any(e=>e.Kind==EwarKind.Scram);
  snapshot.Jammers=effects.Where(e=>!profile.SpecialEcmImmune).Where(e=>e.Kind==EwarKind.ECM).Select(e=>e.SourceId).Distinct().ToArray();
  bool immune=Profile(id).Stat("warpBubbleImmune")>0; // Intrinsic and activated nullification both feed the same travel checks.
  snapshot.Bubble=!immune&&EwarZones.Any(z=>z.Remaining>0&&z.Kind is EwarKind.Sphere or EwarKind.Field or EwarKind.BurstPoint&&Vector3.Distance(z.Center,At(id))<=z.Radius);
  return snapshot;
 }
 public float EffectiveSignature(SpaceTarget target)=>target.Signature*(target.Profile?.SupportFactor("signatureRadius")??1)*EwarFor(target.Id).Signature;
 public bool CanLock(string sourceId,string targetId)
 {
  if(sourceId=="player"&&(Cloaked||SpecialLockRecovery>0)||targetId=="player"&&Cloaked)return false;
  if(!ShipAlive(sourceId)||!ShipAlive(targetId))return false;var effect=EwarFor(sourceId);
  if(effect.Jammers.Length>0&&!effect.Jammers.Contains(targetId))return false;
  if(sourceId=="player"&&Player.SensorJam>0||sourceId!="player"&&Targets.First(t=>t.Id==sourceId).SensorJam>0)return false;
  return Vector3.Distance(At(sourceId),At(targetId))<=Profile(sourceId).Stat("maxTargetRange")*effect.LockRange;
 }
 public bool WarpBlocked(string id)=>Profile(id).Special.Any(s=>s.Active&&s.Definition?.WarpLock==true)||id=="player"&&(SpecialWarpLocked||Cloaked&&!Player.HasSpecial("CovertCloak"))||EwarFor(id).Bubble||EwarFor(id).WarpStrength>Profile(id).Stat("warpCoreStrength");
 float DriveInterlock(string id)=>id=="player"?Math.Max(Player.DriveLockout,IndustrialCore?1:0):Targets.First(t=>t.Id==id).DriveLockout;
 public bool CanUseMicroDrive(string id)=>(id!="player"||!RobbingQ)&&!Profile(id).Special.Any(s=>s.Active&&s.Definition?.Kind is "Siege" or "Triage" or "Bastion" or "Core")&&(id!="player"||!SpecialMicroBlocked)&&!EwarFor(id).Scrammed&&DriveInterlock(id)<=0;
 public bool CanUseGate(string id)=>(id!="player"||!SpecialDockLocked)&&DriveInterlock(id)<=0&&(id!="player"||SupportDockLock<=0)&&(CombatHulls.Get(Profile(id).CombatHull).Size<4||!WarpBlocked(id));
 public bool CanUseJumpDrive(string id)=>(id!="player"||!RobbingQ&&TravelInterlockQ())&&DriveInterlock(id)<=0&&(id!="player"||SupportDockLock<=0)&&!WarpBlocked(id);
 public bool CanDockAtStructure(string id)=>(id!="player"||!SpecialDockLocked)&&DriveInterlock(id)<=0&&(id!="player"||SupportDockLock<=0)&&!WarpBlocked(id);
 public void ToggleMicroWarp()
 {int fitted=Array.FindIndex(Player.Mobility,s=>s.Definition?.Kind=="MWD");if(fitted>=0){ToggleMobility(fitted);return;}if(!CanUseMicroDrive("player")){Say("Micro-warp drive disabled by scrambler/interlock.");return;}Player.MicroWarpOn=!Player.MicroWarpOn;Say("Trial micro-warp drive "+(Player.MicroWarpOn?"active":"off")+".");}
 public bool MicroJump()
 {
  int fitted=Array.FindIndex(Player.Mobility,s=>s.Definition?.Kind=="MJD");if(fitted>=0)return ToggleMobility(fitted);
  if(!WeaponTestMode){Say("Micro-jump exercise is available in trial mode.");return false;}
  if(!CanUseMicroDrive("player")||Player.Cap<25){Say("Micro-jump drive blocked or capacitor low.");return false;}
  Vector3 aim=(Selected?.Position??Player.ManualPoint)-Player.Position;Player.Position+=(aim.LengthSquared()>1?Vector3.Normalize(aim):Vector3.Forward)*10000;Player.Cap-=25;Say("Trial micro-jump complete.");return true;
 }
 public void ToggleEwar(int slot)
 {
  if(slot<0||slot>3)return;var s=Player.Ewar[slot];var d=Player.HullEwar(s.Definition);if(d==null){Say("Empty EWAR socket.");return;}
  if(s.Active){s.Stop();return;}
  string error=EwarActivationError(s);if(error.Length>0){Say(error);return;}
  s.Active=true;s.TargetId=LockedId;Say(d.Name+" active.");
 }
 public string EwarActivationError(EwarState s)
 {
  if(!s.Online)return "Module offline.";
  if(ModuleUnderRepairB("ewar",s))return "Module is being repaired with nanites.";
  var d=Player.HullEwar(s.Definition);if(d==null)return "Empty EWAR socket.";if(s.Burnt)return "Module burnt out; repair at a station.";if(SpecialEwarBlocked)return "Stealth or capital mode blocks hostile EWAR.";if(Docked)return "Undock to activate EWAR.";
  bool needs=d.NeedsLock||d.Kind==EwarKind.Field&&s.Mode!="Bubble";
  if(needs&&(Locked?.Kind!=TargetKind.Raider||!CanLock("player",LockedId)))return "Lock a ship within effective sensor range first.";
  if(needs&&Locked!=null&&!LegalAttackQ("player",Locked,false))return "Safety blocks illegal electronic aggression.";bool bubble=d.Kind is EwarKind.Sphere or EwarKind.BurstPoint||d.Kind==EwarKind.Field&&s.Mode=="Bubble";
  if((bubble||d.Projected||d.Kind==EwarKind.Displacement)&&Current.Security>0&&!WeaponTestMode)return "This area system requires unregulated space (0.0 or below).";
  if(WeaponSafety&&(needs&&Locked?.Friendly==true||d.Area&&Targets.Any(t=>t.Alive&&t.Friendly&&Vector3.Distance(t.Position,d.Projected||d.Kind==EwarKind.Displacement?Selected?.Position??Player.ManualPoint:Player.Position)<d.Radius)))return "Safety blocks hostile EWAR near friendly ships.";
  return "";
 }
 void UpdateEwar(float dt)
 {
  foreach(var e in EwarEffects)e.Remaining-=dt;EwarEffects.RemoveAll(e=>e.Remaining<=0||!ShipAlive(e.TargetId)||!ShipAlive(e.SourceId));
  foreach(var z in EwarZones)
  {
   z.Remaining-=dt;if(z.FollowId.Length>0)
   {var state=Player.Ewar.ElementAtOrDefault(z.SourceSlot);if(!ShipAlive(z.FollowId)||state?.Active!=true||state.Mode!="Bubble"){z.Remaining=0;continue;}z.Center=At(z.FollowId);}
   if(z.Remaining>0&&z.Kind is not (EwarKind.Sphere or EwarKind.Field or EwarKind.BurstPoint))
   foreach(var id in ShipIdsIn(z.Center,z.Radius))ApplyAreaEffect(z,id);
  }
  EwarZones.RemoveAll(z=>z.Remaining<=0);
  for(int slot=0;slot<4;slot++)
  {
   var s=Player.Ewar[slot];var d=Player.HullEwar(s.Definition);if(d==null)continue;if(s.Burnt){s.Stop();continue;}
   s.Cooldown=Math.Max(0,s.Cooldown-dt);s.ReloadLeft=Math.Max(0,s.ReloadLeft-dt);
   if(s.WarmupLeft>0){s.WarmupLeft=Math.Max(0,s.WarmupLeft-dt);if(s.WarmupLeft==0)ResolveEwarArea(s,slot);continue;}
   if(!s.Active||s.Cooldown>0||s.ReloadLeft>0)continue;
   string error=EwarActivationError(s);if(error.Length>0){s.Stop();Say(error);continue;}
   bool targeted=d.NeedsLock||d.Kind==EwarKind.Field&&s.Mode!="Bubble";
   if(targeted)
   {s.TargetId=LockedId;if(Vector3.Distance(Player.Position,Locked!.Position)>(d.Optimal*Player.RigEwarRange(d)*(d.Kind==EwarKind.ECM?Player.EnhancementFactor("ecmOptimal"):1)+d.Falloff*3)*Player.SupportFactor(d.Kind is EwarKind.Web or EwarKind.Point or EwarKind.Scram?"tackleRange":"ewar")){s.Stop();continue;}}
   if(Player.Cap<d.Capacitor*Player.RigEwarCap(d)){s.Stop();Say("EWAR stopped: insufficient capacitor.");continue;}
   if(d.Magazine>0&&s.Loaded==0)
   {bool active=s.Active;string payload=s.PayloadId.Length>0?s.PayloadId:(d.Kind==EwarKind.Sphere?"ewar_probe":"ewar_fuel");if(LoadEwarPayload(slot,payload))s.Active=active;else s.Stop();continue;}
   if(targeted&&!LegalAttackQ("player",Locked!)){s.Stop();continue;}if(d.Magazine>0)s.Loaded--;
   Player.Cap-=d.Capacitor*Player.RigEwarCap(d);s.Cooldown=d.Cycle;
   if(s.Overheat&&StrategicHeat.Ewar.TryGetValue(d.Id,out var heat)){ApplyModuleHeat(s,heat.Damage/Math.Max(1,heat.Hp)*Player.HullFactor("hull","heatDamage"));if(s.Damage>=1){s.Burnt=true;s.Stop();Say(d.Name+" burnt out.");continue;}}
   if(d.Warmup>0){s.AimX=(Selected?.Position??Player.ManualPoint).X;s.AimY=(Selected?.Position??Player.ManualPoint).Y;s.AimZ=(Selected?.Position??Player.ManualPoint).Z;s.WarmupLeft=d.Warmup;s.Active=false;Say(d.Name+" warning field placed; windup "+d.Warmup+" s.");continue;}
   if(d.Kind==EwarKind.Sphere){EwarZones.Add(new(){ModuleId=d.Id,Kind=EwarKind.Sphere,Center=Player.Position,Radius=d.Radius,Remaining=d.Duration});s.Active=false;}
   else if(d.Kind==EwarKind.Field&&s.Mode=="Bubble")
   {
    var zone=EwarZones.FirstOrDefault(z=>z.FollowId=="player"&&z.SourceSlot==slot);
    if(zone==null)EwarZones.Add(new(){ModuleId=d.Id,Kind=EwarKind.Field,FollowId="player",SourceSlot=slot,Center=Player.Position,Radius=d.Radius,Remaining=d.Cycle+.1f});else zone.Remaining=d.Cycle+.1f;
   }
   else if(d.Kind==EwarKind.BurstJam)
   {foreach(var id in ShipIdsIn(Player.Position,d.Radius*Player.RigEwarRange(d)).Where(id=>id!="player"))if(CombatRoll()<JamChance(d with{Strength=d.Strength*Player.RigFactor("ecmStrength")},Profile(id),0))BreakLocks(id);Say("Flashblind lock-breaking burst fired.");}
   else ApplyEwarCycle(d,"player",s.TargetId,"slot_"+slot,s.Mode);
  }
  // Locks, travel and micro-drive status consume derived effects; base hull stats remain unchanged.
  if(EwarFor("player").Scrammed)Player.MicroWarpOn=false;
  if(Locked?.Kind==TargetKind.Raider&&!CanLock("player",LockedId))LockedId="";
  if(PendingLockId.Length>0)
  {
   if(!CanLock("player",PendingLockId)){PendingLockId="";LockTimeLeft=0;}
   else {LockTimeLeft-=dt;if(LockTimeLeft<=0){LockedId=PendingLockId;PendingLockId="";Say("Suppressed sensor lock completed.");}}
  }
  foreach(var t in Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&MissionNpcActiveL(t)))
  {
   if(t.LockId.Length>0&&!CanLock(t.Id,t.LockId))t.LockId="";
   if(t.LockId.Length==0){var aim=EwarFor(t.Id).Jammers.FirstOrDefault()??MissionAggroL(t)??ChooseNpcSupportVictim(t);if(CanLock(t.Id,aim)){t.LockProgress+=dt*EwarFor(t.Id).ScanResolution;if(t.LockProgress>=DefenseNpcLockSeconds(t,aim)){t.LockId=aim;t.LockProgress=0;}}else t.LockProgress=0;}
  }
  if(WeaponTestMode&&Grid=="ewarrange")
  {
   npcEwarTimer-=dt;if(npcEwarTimer<=0){npcEwarTimer=12;var npc=Targets.FirstOrDefault(t=>t.Id=="ew_tackler"&&t.Alive);if(npc!=null&&CanLock(npc.Id,"player")&&npc.Cap>=12){npc.Cap-=12;ApplyEwarCycle(EwarCatalog.All.First(e=>e.Kind==EwarKind.Web),npc.Id,"player","npc_web","Combined");ApplyEwarCycle(EwarCatalog.All.First(e=>e.Kind==EwarKind.Point),npc.Id,"player","npc_point","Combined");Say("Range tackler applied web and warp disruption. Drain, jam or outrange it.");}}
  }
 }
 public static float JamChance(EwarDefinition d,Ship target,float range)
 {
  string sensor=target.NativeSensor;float strength=(float)target.Stat("scan"+sensor+"Strength");if(strength<=0)strength=new[]{"Radar","Ladar","Magnetometric","Gravimetric"}.Max(s=>(float)target.Stat("scan"+s+"Strength"));
  float matched=d.Sensor.Length==0||d.Sensor=="All"||d.Sensor==sensor?1:.28f;
  float resistance=(float)Math.Clamp(target.Stat("ECMResistance"),0,1);return Math.Clamp(d.Strength*matched/Math.Max(1,strength)*RangeScale(range,d.Optimal,d.Falloff)*resistance,0,1);
 }
 public void ApplyEwarCycle(EwarDefinition d,string source,string target,string key,string mode="Combined")
 {
  if(!ShipAlive(source)||!ShipAlive(target))return;if(source=="player")RevealEnhancedLock(target);if(d.Kind==EwarKind.ECM&&Profile(source).Ewar.Any(s=>s.Definition?.Id==d.Id))d=d with{Strength=d.Strength*Profile(source).EnhancementFactor("ecmStrength"),Optimal=d.Optimal*Profile(source).EnhancementFactor("ecmOptimal")};if(source=="player"&&Player.Ewar.Any(s=>s.Definition?.Id==d.Id))d=d with{Optimal=d.Optimal*Player.RigEwarRange(d),Strength=d.Strength*Player.RigFactor(d.Kind==EwarKind.Damp?"dampStrength":d.Kind is EwarKind.Tracking or EwarKind.Guidance?"disruptStrength":d.Kind==EwarKind.Paint?"paintStrength":"none")};float distance=Vector3.Distance(At(source),At(target)),range=RangeScale(distance,d.Optimal,d.Falloff);
  var sourceProfile=Profile(source);float ewBonus=sourceProfile.SupportFactor("ewar"),tackle=sourceProfile.SupportFactor("tackleRange");
  if(d.Kind is EwarKind.ECM or EwarKind.Damp or EwarKind.Tracking or EwarKind.Guidance or EwarKind.Paint){d=d with{Optimal=d.Optimal*ewBonus,Falloff=d.Falloff*ewBonus,Strength=d.Strength*ewBonus};range=RangeScale(distance,d.Optimal,d.Falloff);}
  if(d.Kind is EwarKind.Point or EwarKind.Scram or EwarKind.Web){d=d with{Optimal=d.Optimal*tackle};range=RangeScale(distance,d.Optimal,d.Falloff);}
  if(range<=0)return;var victim=Profile(target);
  if(d.Kind is EwarKind.Neutralizer or EwarKind.Siphon)
  {
   float resistance=(float)Math.Clamp(victim.Stat("energyWarfareResistance"),0,1);float amount=d.Amount*range*resistance;
   if(d.Kind==EwarKind.Siphon)
   {if(CapAt(source)/Math.Max(1,Profile(source).MaxCap)>=CapAt(target)/Math.Max(1,victim.MaxCap)&&!Profile(source).HullCapability("regardless of your own capacitor"))return;amount=Math.Min(amount,Profile(source).HullCapability("regardless of your own capacitor")?CapAt(target):Math.Min(CapAt(target),Profile(source).MaxCap-CapAt(source)));SetCapAt(source,CapAt(source)+amount);}
   amount=Math.Min(amount,CapAt(target));SetCapAt(target,CapAt(target)-amount);CreditP(source,amount);Say(d.Name+" / "+amount.ToString("0.#")+" GJ "+(d.Kind==EwarKind.Siphon?"transferred":"neutralized"));return;
  }
  EwarKind kind=d.Kind;float strength=d.Strength;
  if(kind==EwarKind.Field){kind=mode=="Scram"?EwarKind.Scram:EwarKind.Point;strength=10000;}
  if(kind==EwarKind.ECM){if(CombatRoll()>=JamChance(d,victim,distance)){Say("Mirage jam failed.");return;}strength=1;}
  else if(kind is EwarKind.Damp or EwarKind.Tracking or EwarKind.Guidance or EwarKind.Paint)
  {if(CombatRoll()>range)return;float resist=(float)victim.Stat(kind==EwarKind.Damp?"sensorDampenerResistance":kind==EwarKind.Paint?"targetPainterResistance":"weaponDisruptionResistance");strength*=resist;if(mode!="Combined"&&kind!=EwarKind.Paint)strength=Math.Min(.95f,strength*2);}
  else if(kind is EwarKind.Web or EwarKind.Grapple)strength*=range*(float)victim.Stat("stasisWebifierResistance");
  Upsert(new(){SourceId=source,TargetId=target,Kind=kind,Strength=strength,Remaining=d.Duration,Key=key,Mode=mode});
  if(kind==EwarKind.ECM)BreakLocks(target,true);
  if(kind==EwarKind.Scram&&target=="player")Player.MicroWarpOn=false;
  WeaponVisuals.Add(new(){Start=At(source),End=At(target),Kind=WeaponKind.Arc,DamageType=kind is EwarKind.Point or EwarKind.Scram?DamageType.Explosive:DamageType.EM,Remaining=.4f});
 }
 void Upsert(EwarEffect effect){EwarEffects.RemoveAll(e=>e.SourceId==effect.SourceId&&e.TargetId==effect.TargetId&&e.Key==effect.Key&&e.Kind==effect.Kind);EwarEffects.Add(effect);}
 void BreakLocks(string id,bool keepJammers=false)
 {
  var allowed=keepJammers?EwarFor(id).Jammers:[];
  if(id=="player"){RetainedLocks.RemoveAll(l=>!allowed.Contains(l.Id));Acquisitions.Clear();if(!allowed.Contains(LockedId))LockedId="";PendingLockId="";LockTimeLeft=0;}
  else {var t=Targets.First(t=>t.Id==id);if(!allowed.Contains(t.LockId))t.LockId="";t.LockProgress=0;}
 }
 IEnumerable<string> ShipIdsIn(Vector3 center,float radius)=>Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&Vector3.Distance(t.Position,center)<=radius).Select(t=>t.Id).Concat(Vector3.Distance(Player.Position,center)<=radius?["player"]:[]).ToArray();
 void ResolveEwarArea(EwarState state,int slot)
 {
  var d=state.Definition!;var aim=new Vector3(state.AimX,state.AimY,state.AimZ);if(Vector3.Distance(Player.Position,aim)>d.Optimal){Say("Projected EWAR fizzled: committed point outside range.");return;}
  if(d.Kind==EwarKind.Displacement)
  {
   foreach(var id in ShipIdsIn(aim,d.Radius).ToArray())
   {
    if(CombatHulls.Get(Profile(id).CombatHull).Size>=4||EwarFor(id).Scrammed)continue;
    Vector3 destination=aim+new Vector3((float)(CombatRoll()-.5)*90000,15000,(float)(CombatRoll()-.5)*90000);
    if(id=="player"){Player.Position=destination;Player.Velocity=Vector3.Zero;Player.Order="stop";}else {var t=Targets.First(t=>t.Id==id);t.Position=destination;t.Velocity=Vector3.Zero;}
    BreakLocks(id);
   }
   Say("Exile displacement field relocated eligible ships within the system.");return;
  }
  var zone=new EwarZone{ModuleId=d.Id,SourceSlot=slot,Center=aim,Radius=d.Radius,Remaining=d.Duration,Kind=d.Kind,Strength=d.Strength};
  if(d.Kind==EwarKind.BurstNeutral)
  {foreach(var id in ShipIdsIn(aim,d.Radius))SetCapAt(id,Math.Max(0,CapAt(id)-d.Amount*(float)Profile(id).Stat("energyWarfareResistance")));}
  else if(d.Kind==EwarKind.BurstECM)
  {foreach(var id in ShipIdsIn(aim,d.Radius))if(CombatRoll()<JamChance(d,Profile(id),0))BreakLocks(id);}
  else EwarZones.Add(zone);
  WeaponVisuals.Add(new(){Start=aim,End=aim,Kind=WeaponKind.Smartbomb,Radius=d.Radius,DamageType=DamageType.EM,Remaining=.7f});Say(d.Name+" field resolved.");
 }
 void ApplyAreaEffect(EwarZone zone,string id)
 {
  float strength=zone.Strength;var target=Profile(id);EwarKind kind=zone.Kind switch{EwarKind.BurstDamp=>EwarKind.Damp,EwarKind.BurstWeb=>EwarKind.Web,EwarKind.BurstPaint=>EwarKind.Paint,_=>EwarKind.Tracking};
  strength*=(float)target.Stat(kind==EwarKind.Damp?"sensorDampenerResistance":kind==EwarKind.Paint?"targetPainterResistance":kind==EwarKind.Web?"stasisWebifierResistance":"weaponDisruptionResistance");
  Upsert(new(){SourceId=zone.SourceId,TargetId=id,Key=zone.Id,Kind=kind,Strength=strength,Remaining=.2f});
  if(zone.Kind==EwarKind.BurstWeapon)Upsert(new(){SourceId=zone.SourceId,TargetId=id,Key=zone.Id,Kind=EwarKind.Guidance,Strength=strength,Remaining=.2f});
 }
 public bool TryNpcWarp(string id)
 {
  var t=Targets.FirstOrDefault(t=>t.Id==id&&t.Alive);if(t==null||t.DriveLockout>0||WarpBlocked(id))return false;t.Position+=new Vector3(50000,0,0);t.Velocity=Vector3.Zero;BreakLocks(id);return true;
 }
 public void EnterEwarRange()
 {
  EnterWeaponRange();if(Grid!="range")return;Grid="ewarrange";
  var sensors=new[]{"Radar","Ladar","Magnetometric","Gravimetric"};
  for(int i=0;i<Targets.Count;i++){var t=Targets[i];t.Profile!.NativeSensor=sensors[i%4];foreach(string sensor in sensors)t.Profile.SetStat("scan"+sensor+"Strength",sensor==t.Profile.NativeSensor?8:0);t.Cap=t.Profile.MaxCap;t.LockId="player";t.Position=new(3500+i*1200,0,i*300);}
  Targets[4].Id="ew_tackler";Targets[4].Name="Range tackle drone";npcEwarTimer=6;
  Say("EWAR range: four sensor profiles, a live-capacitor tackler and a friendly vessel. F4/F5/F6/F7 activate EWAR. Trial micro drives are available in fitting.");
 }
}
