using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record EnhancementDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Slot{get;init;}="";public string Role{get;init;}="";
 public float Cpu{get;init;}public float Power{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public float Capacitor{get;init;}public float Cycle{get;init;}public float Range{get;init;}public float HeatDamage{get;init;}public float ModuleHp{get;init;}public float Overload{get;init;}public bool Active{get;init;}public int ReferenceTypeId{get;init;}public Dictionary<string,float> Bonuses{get;init;}=[];
 public string Description=>$"{Slot} socket / {(Active?"cycled active":"passive online")} / CPU {Cpu:0.#}, grid {Power:0.#}, volume {Volume:0.#} m3. "+(Active?$"{Capacitor:0.#} GJ / {Cycle:0.##}s. ":"")+string.Join("; ",Bonuses.Select(b=>b.Key.EndsWith("Add")?$"{EnhancementCatalog.Label(b.Key)}: +{b.Value:0.#}":$"{EnhancementCatalog.Label(b.Key)}: {b.Value:+0.0%;-0.0%;0%}"))+". "+(Kind=="Fighter"?"Fighter hull only; ordinary drones unaffected. ":Kind=="Auto"?$"Scans hostile ships within {Range/1000:0.#} km and sensor reach. Extra lock slots work while online. ":Kind=="Passive"?"Arm, then lock a ship without notifying it; offensive actions reveal the lock. ":Kind=="Ecm"?"Improves fitted targeted Mirage jammers only, not bursts or drones. ":Kind.EndsWith("Damage")&&Kind!="DroneDamage"?"Affects only the matching weapon family, not bombs or superweapons. ":"")+"Percentage bonuses use diminishing stacking; extra locks and control range add.";
}
public sealed record EnhancementScript
{public string Id{get;init;}="";public string Name{get;init;}="";public string Mode{get;init;}="";public string Family{get;init;}="Guidance";public float Volume{get;init;}public decimal Price{get;init;}public int ReferenceTypeId{get;init;}}
public static class EnhancementCatalog
{
 public static string Label(string key)=>key switch{"damage"=>"Weapon volley","cycle"=>"Weapon cycle time","droneDamage"=>"Drone/fighter damage","droneSpeed"=>"Drone/fighter speed","droneControlDistanceAdd"=>"Drone control reach (m)","maxLockedTargetsAdd"=>"Extra locks","tracking"=>"Turret tracking","optimal"=>"Turret optimal","falloff"=>"Turret falloff","droneTracking"=>"Flight tracking","droneOptimal"=>"Flight optimal","droneFalloff"=>"Flight falloff","fighterExplosionRadius"=>"Fighter explosion radius","fighterExplosionVelocity"=>"Fighter explosion speed","fighterShield"=>"Fighter shield capacity","fighterSpeed"=>"Fighter speed","fighterCycle"=>"Fighter attack cycle","fighterRecharge"=>"Fighter recharge time","ecmStrength"=>"Mirage jam strength","ecmOptimal"=>"Mirage optimal","missileVelocity"=>"Missile speed","missileFlightTime"=>"Missile lifetime","explosionRadius"=>"Explosion radius","explosionVelocity"=>"Explosion speed","maxTargetRange"=>"Sensor reach","scanResolution"=>"Scan resolution","sensorStrength"=>"Sensor strength",_=>key};
 static readonly JsonSerializerOptions Options=new(){PropertyNameCaseInsensitive=true};
 public static readonly EnhancementDefinition[] All=JsonSerializer.Deserialize<EnhancementDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Enhancements.json")),Options)!;
 public static readonly Dictionary<string,EnhancementDefinition> ById=All.ToDictionary(d=>d.Id);
 public static readonly EnhancementScript[] NewScripts=JsonSerializer.Deserialize<EnhancementScript[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","EnhancementScripts.json")),Options)!;
 public static readonly EnhancementScript[] Scripts=NewScripts.Concat(SupportPayloads.New.Where(p=>p.Family is "Sensor" or "Tracking").Select(p=>new EnhancementScript{Id=p.Id,Name=p.Name,Mode=p.Mode,Family=p.Family,Volume=p.Volume,Price=p.Price,ReferenceTypeId=p.ReferenceTypeId})).ToArray();
 public static bool Fits(EnhancementDefinition d,EnhancementScript p)=>d.Kind=="Sensor"&&p.Family=="Sensor"||d.Kind is "Tracking" or "DroneTracking"&&p.Family=="Tracking"||d.Kind=="Guidance"&&p.Family=="Guidance";
}
public sealed partial class EnhancementState
{ public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";public string ScriptId{get;set;}="";public bool Online{get;set;}=true;public bool Active{get;set;}public bool Repeat{get;set;}=true;public bool Overheat{get;set;}public bool Burnt{get;set;}public float Damage{get;set;}public float CycleLeft{get;set;}public float CycleHeat{get;set;}=1;public bool StopRequested{get;set;}
 [JsonIgnore]public EnhancementDefinition? Definition=>MutationRulesQ.Apply(EnhancementCatalog.ById.GetValueOrDefault(ModuleId),MutationQ);
 [JsonIgnore]public EnhancementScript? Script=>EnhancementCatalog.Scripts.FirstOrDefault(p=>p.Id==ScriptId);
 [JsonIgnore]public bool Effective=>Online&&!Burnt&&Definition!=null&&(!Definition.Active||Active);
 public IEnumerable<KeyValuePair<string,float>> EffectiveBonuses()
 {
  if(Online&&!Burnt&&Definition?.Active==true)foreach(var b in Definition.Bonuses.Where(b=>b.Key=="maxLockedTargetsAdd"))yield return b;
  if(!Effective)yield break;foreach(var pair in Definition!.Bonuses)
  {float value=pair.Value;if(pair.Key=="maxLockedTargetsAdd"){if(!Definition.Active)yield return pair;continue;}
   string mode=Script?.Mode??"";bool enabled=mode.Length==0||mode switch{"Resolution"=>pair.Key=="scanResolution","Strength"=>pair.Key=="sensorStrength","Tracking"=>pair.Key is "tracking" or "droneTracking" or "fighterExplosionVelocity" or "fighterExplosionRadius","Range"=>pair.Key is "optimal" or "falloff" or "droneOptimal" or "droneFalloff" or "maxTargetRange","MissileRange"=>pair.Key is "missileVelocity" or "missileFlightTime","MissilePrecision"=>pair.Key is "explosionVelocity" or "explosionRadius",_=>false};
   if(enabled)yield return new(pair.Key,value*(mode.Length>0?2:1)*(Definition.Active?CycleHeat:1));
  }
 }
}
public sealed partial class Ship
{
 public EnhancementState[] Enhancements{get;set;}=Enumerable.Range(0,14).Select(_=>new EnhancementState()).ToArray();
 public static string EnhancementSlot(int i)=>i<2?"high":i<8?"medium":"low";
 public double EnhancementCpu=>Enhancements.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigCpu(s.Definition.Id,s.Definition.Cpu)));public double EnhancementPower=>Enhancements.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigPower(s.Definition.Id,s.Definition.Power)));
 public IEnumerable<float> EnhancementStrengths(string key)=>RigStrengths(key).Concat(Enhancements.SelectMany(s=>s.EffectiveBonuses()).Where(b=>b.Key==key).Select(b=>b.Value));
 public float EnhancementFactor(string key,bool includeRigs=true){var a=(includeRigs?EnhancementStrengths(key):Enhancements.SelectMany(s=>s.EffectiveBonuses()).Where(b=>b.Key==key).Select(b=>b.Value)).ToArray();return DemoWorld.StackMultiplier(a.Where(x=>x>0),true)*DemoWorld.StackMultiplier(a.Where(x=>x<0).Select(Math.Abs));}
 public float WeaponEnhancement(WeaponDefinition w,string key)
 {string kind=w.Kind==WeaponKind.Entropic?"EntropicDamage":w.Kind==WeaponKind.Arc?"ArcDamage":w.Kind==WeaponKind.Missile?"MissileDamage":w.Kind==WeaponKind.Turret?w.Laser?"LaserDamage":w.Family is "Blasters" or "Railguns"?"HybridDamage":w.Family is "Artillery" or "Autocannons"?"ProjectileDamage":"":"";var a=RigStrengths(RigWeaponFamily(w)+key).Concat(Enhancements.Where(s=>s.Definition?.Kind==kind).SelectMany(s=>s.EffectiveBonuses()).Where(b=>b.Key==key).Select(b=>b.Value)).ToArray();return BoosterWeapon(key)*HullWeapon(w,key)*SpecialWeapon(w,key)*DemoWorld.StackMultiplier(a.Where(x=>x>0),true)*DemoWorld.StackMultiplier(a.Where(x=>x<0).Select(Math.Abs));}
 public double EnhancementStat(string key,double value)
 {if(key is "maxLockedTargets" or "droneControlDistance")return value+EnhancementStrengths(key+"Add").Sum();return key switch{"turretTracking"=>value*WeaponProjection(WeaponAt(0)?.Definition??WeaponCatalog.Get("rail"),"tracking"),"turretOptimal"=>value*WeaponProjection(WeaponAt(0)?.Definition??WeaponCatalog.Get("rail"),"optimal"),"turretFalloff"=>value*WeaponProjection(WeaponAt(0)?.Definition??WeaponCatalog.Get("rail"),"falloff"),_=>value};}
 public double EnhancementCapUse=>Enhancements.Where(s=>s.Active&&s.Definition!=null&&!s.Burnt).Sum(s=>s.Definition!.Capacitor/s.Definition.Cycle);
 public double EnhancedWeaponDps(WeaponState s){if(!s.Online||s.Burnt)return 0;var q=TacticalRulesB.Weapon(this,s);return q.Packet.Total/q.Cycle;}
 public double EnhancedDroneDps(DroneDefinition d,int count=1)=>d.Packet.Total*count*DroneDamageFactor(d.Kind=="Sentry")*LightCombatFactor(d,"damage")*SpecificDroneFactor(d,"damage")/(d.Cycle*(d.Fighter?EnhancementFactor("fighterCycle"):1));
}
