using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record StrategicSubsystem(string Id,string ShipId,int ReferenceTypeId,string Category,string Name,decimal Price,Dictionary<string,double> Additions,HullEffect[] Effects,HullTrait[] Traits);
public static class ShipBatch28
{
 public static readonly string[] Categories=["Core","Defensive","Offensive","Propulsion"];
 public static readonly StrategicSubsystem[] Subsystems=JsonSerializer.Deserialize<StrategicSubsystem[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Subsystems28.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,StrategicSubsystem> ById=Subsystems.ToDictionary(s=>s.Id);
 public static StrategicSubsystem[] For(string hull,string category)=>Subsystems.Where(s=>s.ShipId==hull&&s.Category==category).ToArray();
 public static Dictionary<string,string> Defaults(string hull)=>Categories.ToDictionary(c=>c,c=>For(hull,c).First(s=>c=="Core"?s.Name.Contains("Reactor"):c=="Defensive"?s.Name is "Nanobot Injector" or "Amplification Node" or "Adaptive Defense Node":c=="Offensive"?s.Name is "Hybrid Encoding Platform" or "Accelerated Ejection Bay" or "Liquid Crystal Magnifiers" or "Projectile Scoping Array":s.Name is "Intercalated Nanofibers" or "Chassis Optimization" or "Hyperspatial Optimization").Id);
 public static IEnumerable<ItemDefinition> Items=>Subsystems.Select(s=>new ItemDefinition(s.Id,FleetCatalog.ById[s.ShipId].Name+" / "+s.Category+" / "+s.Name,"Strategic subsystems / "+s.Category,"Docked strategic refit. Changes base slots, stats, capabilities and proficiency bonuses. Stored in the local hangar.",40,s.Price,"subsystem"));
}
public sealed partial class Ship
{
 public Dictionary<string,string> StrategicSubsystems{get;set;}=[];
 [JsonIgnore]public bool StrategicCruiser=>ShipBatch21.ById.TryGetValue(CombatHull,out var p)&&p.Role=="StrategicCruiser";
 [JsonIgnore]public StrategicSubsystem[] InstalledSubsystems=>StrategicCruiser?StrategicSubsystems.Where(p=>ShipBatch28.ById.TryGetValue(p.Value,out var s)&&s.ShipId==CombatHull&&s.Category==p.Key).Select(p=>ShipBatch28.ById[p.Value]).ToArray():[];
 [JsonIgnore]public bool SubsystemsComplete=>!StrategicCruiser||ShipBatch28.Categories.All(c=>InstalledSubsystems.Count(s=>s.Category==c)==1)&&StrategicSubsystems.Count==4;
 [JsonIgnore]public HullEffect[] ActiveHullEffects=>ShipBatch21.ById.TryGetValue(CombatHull,out var h)?StrategicCruiser?h.Effects.Concat(InstalledSubsystems.SelectMany(s=>s.Effects)).ToArray():h.Effects:[];
 [JsonIgnore]public HullTrait[] ActiveHullTraits=>(ShipBatch21.ById.TryGetValue(CombatHull,out var h)?h.Traits:[]).Concat(InstalledSubsystems.SelectMany(s=>s.Traits).Where(t=>!t.Text.StartsWith('+')&&!t.Text.StartsWith("-20km")&&t.Text!="Additional Base Stats")).ToArray();
 public double SubsystemBase(string key,double value)=>value+InstalledSubsystems.Sum(s=>s.Additions.GetValueOrDefault(key));
 public float ScopedOverheat(string system,string filter)=>HullFactor("hull","overheat")*HullFactor(system,"overheat",filter);
}
public sealed partial class DemoWorld
{
 public string StrategicFitError()
 {
  if(!Player.SubsystemsComplete)return "Install one subsystem in each of the four categories before undocking.";
  foreach(string bank in new[]{"high","medium","low"})if(FittingLayoutModel.Installed(Player).Count(m=>m.Bank==bank)>FittingLayoutModel.Capacity(Player,bank))return "Subsystem change would exceed shared "+bank+" slots. Unfit modules first.";
  if(Player.Weapons.Count(s=>s.ModuleId.Length>0&&s.Definition.UsesTurret)>Player.Stat("turretSlotsLeft")||Player.Weapons.Count(s=>s.ModuleId.Length>0&&s.Definition.UsesLauncher)>Player.Stat("launcherSlotsLeft"))return "Subsystem change would exceed weapon hardpoints. Unfit weapons first.";
  if(TotalFitCpu>Player.Stat("cpuOutput")+.001||TotalFitPower>Player.Stat("powerOutput")+.001)return "Subsystem change would exceed CPU or power grid.";
  if(CargoUsed>Player.CargoMax+.001||Player.DroneBayUsed>Player.Stat("droneCapacity")+.001)return "Unload cargo or drones before reducing capacity.";
  if(Player.Support.Count(s=>s.Definition?.Kind=="Burst")>BurstLimit)return "Remove command bursts before changing the offensive subsystem.";
  if(!Player.HullCapability("can fit covert ops")&&Player.Special.Any(s=>s.Definition?.Kind is "CovertCloak" or "CovertCyno"))return "Remove covert modules before changing the defensive subsystem.";
  return "";
 }
 public string SubsystemFitError(string id)
 {
  if(!Docked)return "Dock before changing strategic subsystems.";
  if(!ShipBatch28.ById.TryGetValue(id,out var s)||s.ShipId!=Player.CombatHull)return "Subsystem belongs to another hull.";
  string old=Player.StrategicSubsystems.GetValueOrDefault(s.Category,"");Player.StrategicSubsystems[s.Category]=id;
  try{return StrategicFitError();}finally{if(old.Length>0)Player.StrategicSubsystems[s.Category]=old;else Player.StrategicSubsystems.Remove(s.Category);}
 }
 public bool FitStrategicSubsystem(string id,bool simulation=false)
 {if(!RequireService44(17))return false;
  string error=SubsystemFitError(id);if(error.Length>0){Say(error);return false;}
  var s=ShipBatch28.ById[id];string old=Player.StrategicSubsystems.GetValueOrDefault(s.Category,"");if(old==id)return true;
  bool owned=ItemCount(id,true)>=1;if(!simulation&&!owned&&(SystemId!=ShipBatch21.ById[s.ShipId].HomeSystem||Credits<(float)s.Price)){Say("Buy replacement subsystems at the faction home station / "+s.Price+" CR.");return false;}
  var fractions=DefenseFractions();float cap=Player.Cap/Player.MaxCap;
  if(!simulation){if(owned)ChangeStack(id,true,-(decimal)(1));else WalletChange(-s.Price,"Strategic subsystem / "+s.Name);if(old.Length>0)ChangeStack(old,true,(decimal)(1));}
  Player.StrategicSubsystems[s.Category]=id;Player.Cap=cap*Player.MaxCap;RestoreDefenseFractions(fractions);RecomputeWeaponFit();RefreshRigDroneHealth();Say("Installed "+s.Category+" / "+s.Name+". Previous subsystem returned to this station's hangar.");return true;
 }
 void EquipStrategicCommand(FleetHull h,BatchHull p)
 {
  Player.Defense=Enumerable.Range(0,12).Select(_=>new DefenseState()).ToArray();Player.Ewar=[new(),new(),new(),new()];
  // Reserve both source command sockets before fitting weapons.
  if(p.Role=="Command")foreach(string family in p.Effects.Where(e=>e.System=="burst"&&e.Key=="strength").Select(e=>e.Filter).Distinct().Take(2)){
   var d=SupportCatalog.All.Where(d=>d.Kind=="Burst"&&d.Family==family).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu).First();int i=Array.FindIndex(Player.Support,s=>s.Definition==null);if(SupportFitError(d,i).Length==0){Player.Support[i]=SupportState.Create(d.Id);var payload=SupportPayloads.For(d).First();Player.Support[i].PayloadId=payload.Id;Player.Support[i].Loaded=(int)(d.MagazineVolume/payload.Volume);int n=Math.Min(10,(int)Math.Floor(CargoRoom(payload.Id)/ItemVolume(payload.Id)));if(n>0)CargoItems[payload.Id]=n;}
  }
  EquipAdvancedCruiser(h,p);
 }
}
