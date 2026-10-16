using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public enum WeaponKind { Turret, Missile, Smartbomb, Defender, Bomb, Entropic, Arc, Breacher, Doomsday, Lance, Reaper, Cone, DisruptiveLance }
public sealed record WeaponDefinition
{
 public string Id{get;init;}=""; public string Name{get;init;}=""; public string Family{get;init;}=""; public string Role{get;init;}=""; public string ReferenceName{get;init;}="";
 public int ReferenceTypeId{get;init;} public WeaponKind Kind{get;init;} public DamageType DamageType{get;init;}
 public int Size{get;init;} public int Magazine{get;init;}
 public float Damage{get;init;} public float Cycle{get;init;} public float Optimal{get;init;} public float Falloff{get;init;} public float Tracking{get;init;} public float SignatureResolution{get;init;} public float Capacitor{get;init;} public float Velocity{get;init;} public float FlightTime{get;init;} public float ExplosionRadius{get;init;} public float ExplosionVelocity{get;init;} public float DamageReduction{get;init;} public float Reload{get;init;} public float Warmup{get;init;} public float Duration{get;init;} public float Radius{get;init;} public float Cpu{get;init;} public float Power{get;init;} public float Volume{get;init;}
 public decimal Price{get;init;} public bool HighAngle{get;init;}
 public string DisplayFamily=>Family switch {"Precursor Weapon"=>"Entropy drivers","Vorton Projector"=>"Arc projectors","Breacher Pod Launchers"=>"Parasite launchers","Bosonic area weapon"=>"Cone superweapons","Doomsday lances"=>"Titan lances","Doomsday reapers"=>"Titan sweep emitters",_=>Family};
 public bool Laser=>Family is "Beam lasers" or "Pulse lasers";
 public bool UsesTurret=>Kind is WeaponKind.Turret or WeaponKind.Entropic or WeaponKind.Arc;
 public bool UsesLauncher=>Kind is WeaponKind.Missile or WeaponKind.Defender;
 public bool Superweapon=>Kind is WeaponKind.Doomsday or WeaponKind.Lance or WeaponKind.Reaper or WeaponKind.Cone or WeaponKind.DisruptiveLance;
 public bool NeedsLock=>Kind is not (WeaponKind.Smartbomb or WeaponKind.Defender or WeaponKind.Bomb or WeaponKind.Lance or WeaponKind.Reaper or WeaponKind.Cone or WeaponKind.DisruptiveLance);
 public string Description=>$"{Kind} / {SizeLabel}. {Mechanic} CPU {Cpu:0.#}, power {Power:0.#}. Volley {Damage:0.#}; cycle {Cycle:0.#} s; capacitor {Capacitor:0.#} GJ. "+(Optimal>0?$"Optimal {Optimal/1000:0.#} km; falloff {Falloff/1000:0.#} km. ":"")+(Velocity>0&&Kind is WeaponKind.Missile or WeaponKind.Bomb or WeaponKind.Breacher?$"Flight {Velocity:0} m/s / {FlightTime:0.#} s. ":"")+(Magazine>0?$"Magazine {Magazine}; reload {Reload:0.#} s. ":"No ammunition. ")+(Role.Length>0?$"Hull role: {Role}. ":"")+(HighAngle?"High-angle anti-subcapital mounting.":"");
 public string SizeLabel=>new[]{"Micro","Small","Medium","Large","Capital","Titan"}[Size];
 public string Mechanic=>Kind switch {WeaponKind.Turret=>"Tracking, signature and optimal/falloff hit solution.",WeaponKind.Missile=>"Guided flight; signature and velocity damage application.",WeaponKind.Smartbomb=>"Untargeted radial damage; damages nearby ships and ordnance.",WeaponKind.Defender=>"Intercepts hostile bombs only; does not attack ships or ordinary missiles.",WeaponKind.Bomb=>"Unguided timed blast; can hit your own ship. Damage, capacitor and sensor payloads.",WeaponKind.Entropic=>"Damage ramps on an uninterrupted target; no falloff.",WeaponKind.Arc=>"Arcs to up to five nearby ships; missile-style damage application.",WeaponKind.Breacher=>"Interceptable parasite pod; strongest active effect damages raw HP each second without resistance.",WeaponKind.Doomsday=>"Fuelled capital-only targeted strike after a windup.",WeaponKind.Lance=>"Fuelled directional beam after a windup; signature-scaled damage.",WeaponKind.Reaper=>"Fuelled beam sweeps across the aimed direction.",WeaponKind.Cone=>"Fuelled forward cone pulses over time.",_=>"Fuelled beam applies drive/docking disruption and nearby capacitor drain."};
}
public static class WeaponCatalog
{
 public static readonly JsonSerializerOptions JsonOptions=new(){Converters={new JsonStringEnumConverter()},IncludeFields=true,NumberHandling=JsonNumberHandling.AllowNamedFloatingPointLiterals};
 public static readonly WeaponDefinition[] All=JsonSerializer.Deserialize<WeaponDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Weapons.json")),JsonOptions)!;
 public static readonly Dictionary<string,WeaponDefinition> ById=Build();
 static Dictionary<string,WeaponDefinition> Build()
 {
  var d=All.ToDictionary(w=>w.Id);var rail=All.First(w=>w.ReferenceName=="150mm Railgun");
  d["rail"]=rail with {Id="rail",Name="Coil rail turret",Damage=23,Cycle=2.1f,Optimal=4200,Falloff=5200,Tracking=40f/210,Capacitor=6,Cpu=14,Power=9};
  d["missile"]=All.First(w=>w.Family=="Missile Launcher Light") with {Id="missile",Name="Needle launcher",Damage=45,Cycle=5.2f,Velocity=1600,FlightTime=9,ExplosionRadius=110,ExplosionVelocity=120,Cpu=10,Power=5};return d;
 }
 public static WeaponDefinition Get(string id)=>ById[id];
}
public sealed record ChargeDefinition(string Id,string Name,string WeaponKey,DamageType DamageType,float DamageMultiplier,float RangeMultiplier,float TrackingMultiplier,float CapMultiplier,string Payload="damage",bool Crystal=false);
public static class ChargeCatalog
{
 public static readonly ChargeDefinition[] All=Build();
 public static readonly Dictionary<string,ChargeDefinition> ById=All.ToDictionary(c=>c.Id);
 public static string Key(WeaponDefinition w)=>w.Family is "Artillery" or "Autocannons"?"projectile_"+w.Size:w.Family is "Railguns" or "Blasters"?"hybrid_"+w.Size:w.Laser?"laser_"+w.Size:w.Family=="Missile Launcher Rapid Light"?"Missile_Launcher_Light_1":w.Family=="Missile Launcher Rapid Heavy"?"Missile_Launcher_Heavy_2":w.Family=="Missile Launcher Rapid Torpedo"?"Missile_Launcher_Torpedo_3":w.Id=="missile"?"legacy":w.Superweapon?"capital_fuel":w.Kind==WeaponKind.Smartbomb?"none":w.Family.Replace(" ","_")+"_"+w.Size;
 public static ChargeDefinition[] For(WeaponDefinition w)=>All.Where(c=>c.WeaponKey==Key(w)).ToArray();
 static ChargeDefinition[] Build()
 {
  var a=new List<ChargeDefinition>{new("ammo","Needle missile","legacy",DamageType.Explosive,1,1,1,1)};
  foreach(var w in WeaponCatalog.All.DistinctBy(Key))
  {
   string k=Key(w);if(k=="none")continue;
   if(k=="capital_fuel"){a.Add(new("fuel","Catalytic superweapon fuel",k,DamageType.EM,1,1,1,1));continue;}
   if(w.Kind is WeaponKind.Missile or WeaponKind.Bomb)
   {
    foreach(var d in Enum.GetValues<DamageType>())a.Add(new("charge_"+k+"_"+d,w.SizeLabel+" "+d+" "+(w.Kind==WeaponKind.Bomb?"bomb":"warhead"),k,d,1,1,1,1));
    if(w.Kind==WeaponKind.Bomb){a.Add(new("charge_"+k+"_void","Blackout capacitor bomb",k,DamageType.EM,1,1,1,1,"neutral"));a.Add(new("charge_"+k+"_jam","Static sensor bomb",k,DamageType.EM,1,1,1,1,"jam"));a.Add(new("charge_"+k+"_focused","Focused blackout bomb",k,DamageType.EM,2,1,1,1,"focusedneutral"));}
   }
   else if(w.Kind is WeaponKind.Defender or WeaponKind.Breacher)a.Add(new("charge_"+k,"Sentinel / "+w.SizeLabel+" "+(w.Kind==WeaponKind.Defender?"interceptor":"parasite pod"),k,w.DamageType,1,1,1,1));
   else
   {
    foreach(var mode in new[]{("close",1.25f,.65f,1f,1.1f),("standard",1f,1f,1f,1f),("reach",.72f,1.55f,.85f,.85f)})
     foreach(var d in w.Family is "Artillery" or "Autocannons"?Enum.GetValues<DamageType>():new[]{w.DamageType})
     a.Add(new("charge_"+k+"_"+mode.Item1+(w.Family is "Artillery" or "Autocannons"?"_"+d:""),w.SizeLabel+" "+mode.Item1+" "+(w.Family is "Artillery" or "Autocannons"?d+" ":"")+(w.Laser?"focus crystal":w.Kind==WeaponKind.Arc?"arc cell":w.Kind==WeaponKind.Entropic?"entropy charge":w.Family is "Artillery" or "Autocannons"?"cannon shell":"coil charge"),k,d,mode.Item2,mode.Item3,mode.Item4,mode.Item5,"damage",w.Laser));
   }
  }
  // Legacy rail uses the standard small railgun charges; smartbombs do not use any.
  return a.ToArray();
 }
 public static ChargeDefinition? Default(WeaponDefinition w)=>For(w).FirstOrDefault(c=>c.Id.Contains("_standard"))??For(w).FirstOrDefault();
}
public sealed partial class WeaponState
{ public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="rail"; public string ChargeId{get;set;}=""; public string PendingCharge{get;set;}=""; public string RampTarget{get;set;}=""; public string WarmupTarget{get;set;}="";
 public int Loaded{get;set;} public int RampCycles{get;set;}
 public bool Active{get;set;} public bool Online{get;set;}=true; public bool Overheat{get;set;} public bool Burnt{get;set;} public float Damage{get;set;}
 public float Cooldown{get;set;} public float ReloadLeft{get;set;} public float WarmupLeft{get;set;} public float EffectLeft{get;set;}
 public float AimX{get;set;} public float AimY{get;set;} public float AimZ{get;set;}
 [JsonIgnore] public WeaponDefinition Definition=>MutationRulesQ.Apply(WeaponCatalog.Get(ModuleId),MutationQ)!;
 [JsonIgnore] public ChargeDefinition? Charge=>ChargeCatalog.ById.GetValueOrDefault(ChargeId);
 public static WeaponState Create(string id){var w=WeaponCatalog.Get(id);var c=ChargeCatalog.Default(w);return new(){ModuleId=id,ChargeId=c?.Id??"",Loaded=0};}
 public void Stop(){Active=false;RampCycles=0;RampTarget="";}
}
public sealed partial class Ship
{
 public WeaponState[] Weapons{get;set;}=[WeaponState.Create("rail"),WeaponState.Create("missile")];
 public string CombatHull{get;set;}="frigate";
 public float WeaponLockout{get;set;} public float DriveLockout{get;set;} public float SensorJam{get;set;}
 public bool IsFleetHull=>FleetCatalog.ById.ContainsKey(CombatHull);
 public bool LegacyUtilityOccupied=>UtilityFit is "miner" or "analyzer" or "salvager";
 public int WeaponSocketCount=>Enumerable.Range(0,Weapons.Length).Count(i=>WeaponAt(i)!=null)+(LegacyUtilityOccupied?1:0);
 public double FittingBaseCpu=>IsFleetHull?0:20;
 public double FittingBasePower=>IsFleetHull?0:10;
 public WeaponState? WeaponAt(int slot)=>slot<0||slot>=Weapons.Length||slot==1&&LegacyUtilityOccupied||!WeaponCatalog.ById.ContainsKey(Weapons[slot].ModuleId)?null:Weapons[slot];
 public double WeaponDps=>Enumerable.Range(0,Weapons.Length).Select(WeaponAt).Where(w=>w!=null&&w.Definition.Kind!=WeaponKind.Defender).Sum(w=>EnhancedWeaponDps(w!));
}
