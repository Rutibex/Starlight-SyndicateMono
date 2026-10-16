using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed class FleetHull
{
 public string Id{get;set;}="";public string Name{get;set;}="";public string Faction{get;set;}="";public string Class{get;set;}="";
 public string SourceHull{get;set;}="";public string SourceLineage{get;set;}="";public string BaseHullId{get;set;}="";
 public string Description{get;set;}="";public string Quote{get;set;}="";public string TraitsStatus{get;set;}="";
 public bool Playable{get;set;}public Dictionary<string,string> RawFields{get;set;}=[];public Dictionary<string,double> Attributes{get;set;}=[];
 public int SourceTypeId=>UnifiedCatalog40.SourceId(Id);
 public SourceType40? SourceType=>UnifiedCatalog40.Source(Id);
 public double Value(string key)=>Attributes.GetValueOrDefault(key);
 public CombatHullDefinition CombatDefinition=>new(Id,Name,(int)Math.Max(1,Value("rigSize")),ShipBatch24.Role(this),(float)Value("shieldCapacity"),(float)Value("armorHP"),(float)Value("hp"),(float)Value("capacitorCapacity"),(float)Value("cpuOutput"),(float)Value("powerOutput"),(float)Value("maxVelocity"),(float)Value("capacity"),(int)Value("turretSlotsLeft"),(int)Value("launcherSlotsLeft"));
}
public static class FleetCatalog
{
 public static readonly FleetHull[] All=(JsonSerializer.Deserialize<FleetHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Fleet.json")))??throw new InvalidDataException("Missing fleet registry")).Select(Retheme41.Apply).ToArray();
 public static readonly Dictionary<string,FleetHull> ById=All.ToDictionary(h=>h.Id);
 public static readonly FleetHull[] Playable=All.Where(h=>h.Playable).ToArray();
 public static FleetHull Source(string name)=>All.Single(h=>h.SourceHull==name);
}
public sealed partial class Ship
{
 public void ApplyFleetHull(FleetHull h,bool simulationOnly=false)
 {
  if(!h.Playable&&!simulationOnly)throw new InvalidOperationException("This registry hull is awaiting integration.");
  // Initialize simulation-only attributes, then apply every mapped raw source value.
  // Template values are never written back into the authoritative registry.
  ConfigureHull(0);CombatHull=h.Id;HullName=h.Name;
  foreach(var (key,value) in h.Attributes)if(ShipAttributeCatalog.ByKey.ContainsKey(key))SetStat(key,value);
  foreach(string key in new[]{"upgradeSlotsLeft","upgradeCapacity","droneCapacity","droneBandwidth","turretSlotsLeft","launcherSlotsLeft"})if(!h.Attributes.ContainsKey(key))SetStat(key,0);
  SetStat("maxActiveDrones",5);SetStat("droneControlDistance",48000);
  SetStat("fighterCapacity",0);SetStat("fighterTubes",0);SetStat("fighterLightSlots",0);SetStat("fighterHeavySlots",0);SetStat("fighterSupportSlots",0);
  NativeSensor=h.Value("scanRadarStrength")>0?"Radar":h.Value("scanGravimetricStrength")>0?"Gravimetric":h.Value("scanLadarStrength")>0?"Ladar":"Magnetometric";
  StrategicSubsystems=StrategicCruiser?ShipBatch28.Defaults(h.Id):[];
  Weapons=Enumerable.Range(0,Math.Max(2,(int)Stat("hiSlots"))).Select(_=>new WeaponState{ModuleId=""}).ToArray();
  UtilityFit="";Shield=MaxShield;Armor=MaxArmor;Hull=MaxHull;Cap=MaxCap;
 }
}
