using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record SpecialDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Roles{get;init;}="";public int ReferenceTypeId{get;init;}public float Cpu{get;init;}public float Power{get;init;}public float Cycle{get;init;}public float Capacitor{get;init;}public float Range{get;init;}public string Fuel{get;init;}="";public int FuelQuantity{get;init;}public float Cooldown{get;init;}public float TargetDelay{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public Dictionary<string,float> Bonuses{get;init;}=[];
 public bool Cloak=>Kind is "Cloak" or "CovertCloak";public bool Beacon=>Kind is "Cyno" or "CovertCyno" or "IndustrialCyno";public bool Portal=>Kind is "Portal" or "CovertPortal" or "IndustrialPortal";public bool Mode=>Kind is "Siege" or "Triage" or "Bastion" or "Core";
 public string Group=>Cloak?"Cloak":Beacon?"Cyno":Portal?"Portal":Mode?"Deployment":Kind;
 public bool Immobile=>Beacon||Portal||Mode||Kind is "Vat";public bool WarpLock=>Immobile||Kind is "Recloner" or "NSA" or "ISA" or "Entosis" or "Mass" or "Panic";public bool DockLock=>WarpLock||Kind=="Phenomena";
 public bool NoAssist=>Mode||Portal||Kind=="Entosis";public bool OffensiveBlock=>Portal||Kind is "Triage" or "Panic";public bool NoRepeat=>Beacon||Portal||Kind is "Panic";public bool Committed=>!Cloak&&Kind!="Mass";
 public string Description=>Hint+" "+$"High socket / {(Roles.Length==0?"all hulls":Roles)}. CPU {Cpu:0.#}, grid {Power:0.#}, volume {Volume:0.#} m3. {Capacitor:0.#} GJ / {Cycle:0.#}s. "+(Fuel.Length>0?$"Consumes {FuelQuantity} {FuelName(Fuel)} each cycle. ":"")+(Range>0?$"Range {Range/1000:0.#} km. ":"")+(Committed?"Cycle cannot be interrupted. ":"")+(WarpLock?"Warp locked while active. ":"")+(Immobile?"Immobilizes ship. ":"")+string.Join("; ",Bonuses.Select(x=>$"{BonusName(x.Key)} {x.Value:+0.0%;-0.0%;0%}"));
 public string Hint=>Kind switch{"Cloak"=>$"Persistent cloak: blocks warp; reduces velocity and online scan resolution. {TargetDelay:0.#}s targeting recovery, {Cooldown:0.#}s reactivation. Keep 2 km clear of objects.","CovertCloak"=>$"Persistent covert cloak: permits warp. {TargetDelay:0.#}s targeting recovery, {Cooldown:0.#}s reactivation. Keep 2 km clear of objects.","Panic"=>"Lock an asteroid to protect your mining fleet. Damage immunity; offensive actions and travel disabled.","Entosis"=>"Lock a vulnerable relay. One warmup cycle, then capture progress; opposition pauses capture.","Mass"=>"Reduces real transit mass for small fractures. Does not create a warp-disruption bubble.","Vat"=>"Activate to install mobile jump clones for other fleet pilots.","Recloner"=>"Requires a mapped fleet pilot, ten mnemonic fuel blocks and a fitted reserve vessel.","NSA"=>"Supercarrier sensor network. Blocks micro-jump systems and hostile EWAR.","ISA"=>"Carrier sensor array. Micro-jump systems remain available.",_=>""};
 static string FuelName(string id)=>id switch{"sp_ozone"=>"phase catalyst","sp_catalyst"=>"siege catalyst","ut_coolant"=>"industrial coolant","sp_isotope_a"=>"Aurora isotope","sp_isotope_b"=>"Boreal isotope","sp_isotope_c"=>"Verdant isotope","sp_isotope_d"=>"Ember isotope",_=>id};
 static string BonusName(string key)=>key switch{"localRepair"=>"Local repair amount","localCycle"=>"Local repair cycle","localCap"=>"Local repair capacitor","remoteAmount"=>"Remote support amount","remoteCycle"=>"Remote support cycle","remoteRange"=>"Remote support range","droneHP"=>"Drone health","droneMining"=>"Drone mining yield","droneIceCycle"=>"Ice drone cycle","miningBurst"=>"Mining burst strength","burstRange"=>"Command burst range","turretDamage"=>"Capital turret damage","missileDamage"=>"Capital missile damage","missileCycle"=>"Capital missile cycle","weaponCycle"=>"Large weapon cycle","scanResolution"=>"Scan resolution","sensorStrength"=>"Sensor strength","maxTargetRange"=>"Target range","mass"=>"Mass","resonance"=>"Damage received","maxVelocity"=>"Maximum velocity","propSpeed"=>"Propulsion boost","optimal"=>"Optimal range","falloff"=>"Falloff range",_=>key};

}
public static class SpecialCatalog
{
 public static readonly SpecialDefinition[] All=JsonSerializer.Deserialize<SpecialDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Special.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,SpecialDefinition> ById=All.ToDictionary(d=>d.Id);
 public static readonly ItemDefinition[] Items=All.Select(d=>new ItemDefinition(d.Id,d.Name,"Modules / Cloaking, jump and capital",d.Description,d.Volume,d.Price,"high")).Concat(new[]{new ItemDefinition("sp_ozone","Phase catalyst","Materials / Capital fuel","Beacon and clone nursery fuel.",.35f,7,""),new("sp_catalyst","Siege catalyst","Materials / Capital fuel","Deployment and relay cycle fuel.",1,11,""),new("sp_clonefuel","Mnemonic fuel block","Materials / Capital fuel","Ten blocks per tactical reconstruction.",5,35,""),new("sp_mapper","Neural continuity mapper","Materials / Cloning","Maps a fleet pilot for six hours.",1,90,"")}).Concat(new[]{"a","b","c","d"}.Select(x=>new ItemDefinition("sp_isotope_"+x,new Dictionary<string,string>{{"a","Aurora"},{"b","Boreal"},{"c","Verdant"},{"d","Ember"}}[x]+" isotope","Materials / Capital fuel","Phenomena projector fuel.",.15f,4,""))).ToArray();
}
public sealed class SpecialState
{ public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";public string TargetId{get;set;}="";public bool Online{get;set;}=true;public bool Active{get;set;}public bool Repeat{get;set;}=true;public bool StopRequested{get;set;}public float CycleLeft{get;set;}public float Cooldown{get;set;}public float ActiveSeconds{get;set;}public bool Warmed{get;set;}
 [JsonIgnore]public SpecialDefinition? Definition=>SpecialCatalog.ById.GetValueOrDefault(ModuleId);[JsonIgnore]public bool Effective=>Online&&Definition!=null;
}
public sealed partial class Ship
{
 public SpecialState[] Special{get;set;}=Enumerable.Range(0,8).Select(_=>new SpecialState()).ToArray();public Dictionary<string,float> SpecialEnvironment{get;set;}=[];public float NexusLeft{get;set;}
 public bool HasSpecial(string kind)=>Special.Any(s=>s.Active&&s.Effective&&s.Definition!.Kind==kind);
 public float SpecialFactor(string key)=>Special.Where(s=>s.Effective&&s.Definition!.Kind!="Phenomena"&&(s.Active||s.Definition!.Kind=="Cloak"&&key=="scanResolution")).Aggregate(1f,(n,s)=>n*(1+s.Definition!.Bonuses.GetValueOrDefault(key)))*(1+SpecialEnvironment.GetValueOrDefault(key));
 public bool SpecialNoAssist=>InterdictionAssistanceBlocked||Special.Any(s=>s.Active&&s.Definition!.NoAssist);public bool SpecialEcmImmune=>Special.Any(s=>s.Active&&(s.Definition!.Kind is "Siege" or "Triage" or "Core"));
 public double SpecialCpu=>Special.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigCpu(s.Definition.Id,s.Definition.Cpu)));public double SpecialPower=>Special.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigPower(s.Definition.Id,s.Definition.Power)));public double SpecialCapUse=>Special.Where(s=>s.Active&&s.Effective&&!s.Definition!.Cloak).Sum(s=>s.Definition!.Capacitor/Math.Max(.1,s.Definition.Cycle));
 public double SpecialStat(string key,double value)
 {
  if(key is "remoteRepairImpedance" or "remoteCapacitorImpedance"&&SpecialNoAssist)return 0;
  if(key=="remoteAssistanceImpedance"&&SpecialNoAssist)return value*(HasSpecial("Bastion")?.05:.2);
  if(key=="maxLockedTargets"&&HasSpecial("Triage"))return value+4;
  if(key.StartsWith("scan")&&key.EndsWith("Strength"))return value*SpecialFactor("sensorStrength");
  if(key.Contains("DamageResonance"))return value*SpecialFactor("resonance")*SpecialFactor(key);
  if(key=="maxVelocity"&&Special.Any(s=>s.Active&&s.Definition!.Cloak))return value*(HasSpecial("Cloak")?(HullCapability("no penalty to ship max velocity")?1:CombatHulls.Get(CombatHull).Role=="BlackOps"&&!EliteBattleship?.75:.12):1)*HullFactor("special","velocity","Cloak");
  return value*SpecialFactor(key);
 }
 public float SpecialWeapon(WeaponDefinition w,string key)=>HasSpecial("Siege")&&w.Size<4||HasSpecial("Bastion")&&w.Size!=3?1:key=="damage"?w.Kind==WeaponKind.Missile?SpecialFactor("missileDamage"):w.UsesTurret?SpecialFactor("turretDamage"):1:key=="cycle"?SpecialFactor("weaponCycle")*(w.Kind==WeaponKind.Missile?SpecialFactor("missileCycle"):1):1;
 public float LocalModeFactor(string key,string kind,int size){if(kind is "HullRepair" or "CapBooster" or "Reactive" or "Nullifier")return 1;if((HasSpecial("Siege")||HasSpecial("Triage")||HasSpecial("ISA")||Special.Any(s=>s.Active&&s.ModuleId=="sp_core_c"))&&size<4)return 1;if(HasSpecial("Core")&&kind is "ArmorRepair" or "AncillaryArmor" or "Armor" or "AncArmor")return 1;return SpecialFactor(key);}
 public float RemoteModeFactor(string key,string kind,int size){if(HasSpecial("Triage")&&size<4)return 1;if(HasSpecial("Core")&&(size<4||kind is not ("Shield" or "AncShield")))return 1;return SpecialFactor(key);}
 public float SpecialRange(WeaponDefinition w,string key)=>HasSpecial("Bastion")&&w.UsesTurret&&w.Size!=3?1:SpecialFactor(key);
 public float SpecialMissileVelocity(WeaponDefinition w)=>(HasSpecial("Bastion")&&w.Size!=3||HasSpecial("Siege")&&(w.Size!=4||!w.Family.Contains("Torpedo")))?1:SpecialFactor("missileVelocity");

}
