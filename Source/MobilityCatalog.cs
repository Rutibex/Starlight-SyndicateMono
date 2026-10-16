using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed record MobilityDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Slot{get;init;}="";public string Role{get;init;}="";public int Size{get;init;}public int ReferenceTypeId{get;init;}public int MaxGroup{get;init;}public int ShipCap{get;init;}public int CapitalCap{get;init;}
 public float Cpu{get;init;}public float Power{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public float Capacitor{get;init;}public float Cycle{get;init;}public float Reactivation{get;init;}public float Thrust{get;init;}public float Speed{get;init;}public float MassAddition{get;init;}public float Signature{get;init;}public float JumpRange{get;init;}public float Radius{get;init;}public float PostScram{get;init;}public float HeatDamage{get;init;}public float ModuleHp{get;init;}public float Overload{get;init;}public Dictionary<string,float> Bonuses{get;init;}=[];
 public bool Active=>Kind is "AB" or "MWD" or "MJD" or "Field";public bool Skip=>Kind is "MJD" or "Field";
 public string Group=>Kind=="MJD"?"skip":Kind=="Field"?"field":Kind;
 public string Description=>$"{Slot} power / {(Active?"active":"passive online")}. CPU {Cpu:0.#}, grid {Power:0.#}, volume {Volume:0.#} m3. "+(Active?$"{Capacitor:0.#} GJ / {Cycle:0.##}s. ":"")+(Kind is "AB" or "MWD"?$"Speed bonus {Speed:P0}; thrust {Thrust:0}; active mass +{MassAddition:0} kg. ":Skip?$"Forward skip {JumpRange/1000:0} km / spool {Cycle:0.#}s / cooldown {Reactivation:0}s. ":"")+(Signature>0?$"Active signature +{Signature:P0}. ":"")+(Kind=="Field"?$"Field radius {Radius/1000:0.#} km; {ShipCap} subcapital and {CapitalCap} capital passengers. ":"")+string.Join("; ",Bonuses.Select(b=>$"{Label(b.Key)} {(b.Key.EndsWith("Add")?$"+{b.Value:0.#}":$"{b.Value:+0.0%;-0.0%;0%}")}"))+(Role.Length>0?". Hull restriction: "+Role:".");
 public static string Label(string k)=>k switch{"cpuOutput"=>"CPU output","powerOutput"=>"Grid output","powerOutputAdd"=>"Grid added","capacity"=>"Cargo capacity","hp"=>"Hull integrity","maxVelocity"=>"Base speed","agility"=>"Inertia","signatureRadius"=>"Signature","baseWarpSpeedAdd"=>"Warp AU/s added","jumpDriveConsumptionAmount"=>"Jump fuel use","shieldCapacity"=>"Shield capacity","capacitorCapacity"=>"Capacitor capacity","rechargeRate"=>"Capacitor recharge time","shieldRechargeRate"=>"Shield recharge time",_=>k};
}
public static class MobilityCatalog
{
 public static readonly MobilityDefinition[] All=JsonSerializer.Deserialize<MobilityDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Mobility.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,MobilityDefinition> ById=All.ToDictionary(d=>d.Id);
}
public sealed partial class MobilityState
{ public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";public bool Online{get;set;}=true;public bool Active{get;set;}public bool Repeat{get;set;}=true;public bool StopRequested{get;set;}public bool Overheat{get;set;}public bool Burnt{get;set;}public float Damage{get;set;}public float CycleLeft{get;set;}public float Cooldown{get;set;}public float CycleHeat{get;set;}=1;public float AimX{get;set;}public float AimY{get;set;}public float AimZ{get;set;}
 [JsonIgnore]public MobilityDefinition? Definition=>MutationRulesQ.Apply(MobilityCatalog.ById.GetValueOrDefault(ModuleId),MutationQ);
 [JsonIgnore]public bool Effective=>Online&&!Burnt&&Definition!=null;
}
public sealed partial class Ship
{
 public MobilityState[] Mobility{get;set;}=Enumerable.Range(0,12).Select(_=>new MobilityState()).ToArray();public Vector3 Facing=Vector3.UnitZ;
 public static string MobilitySlot(int i)=>i<6?"medium":"low";
 public double MobilityCpu=>Mobility.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigCpu(s.Definition.Id,s.Definition.Cpu)));public double MobilityPower=>Mobility.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigPower(s.Definition.Id,s.Definition.Power)));
 public double MobilityCapUse=>Mobility.Where(s=>s.Active&&s.Definition!=null&&s.Effective).Sum(s=>s.Definition!.Capacitor*(s.Definition.Kind is "AB" or "MWD"?RigFactor("propCap",false)*HullFactor("prop","cap",s.Definition.Kind):1)/(s.Definition.Cycle*(s.Definition.Kind is "AB" or "MWD"?RigFactor("propCycle",false):1)));
 public MobilityState? ActivePropulsion=>Mobility.FirstOrDefault(s=>s.Active&&s.Effective&&s.Definition?.Kind is "AB" or "MWD");
 public bool MobilitySpooling=>Mobility.Any(s=>s.Active&&s.Effective&&s.Definition?.Skip==true);
 public double MobilityStat(string key,double value)
 {
  if(!Mobility.Any(s=>s.Definition!=null)&&!InstalledRigs.Any())return value;var states=Mobility.Where(s=>s.Effective).ToArray();value+=states.Sum(s=>s.Definition!.Bonuses.GetValueOrDefault(key+"Add"));var pct=(key is "agility" or "maxVelocity"?RigStrengths(key):Enumerable.Empty<float>()).Concat(states.Select(s=>s.Definition!.Bonuses.GetValueOrDefault(key)).Where(v=>v!=0)).ToArray();
  if(key is "agility" or "jumpDriveConsumptionAmount")value*=DemoWorld.StackMultiplier(pct.Where(v=>v<0).Select(Math.Abs));
  else if(key=="maxVelocity")value*=DemoWorld.StackMultiplier(pct.Where(v=>v>0),true)*pct.Where(v=>v<0).Aggregate(1f,(n,v)=>n*(1+v));
  else foreach(var v in pct)value*=1+v;
  if(key=="mass")value+=states.Where(s=>s.Active).Sum(s=>s.Definition!.MassAddition);
  if(key=="maxVelocity"&&ActivePropulsion is {} prop){var d=prop.Definition!;value*=1+d.Speed*HullFactor("prop","speed",d.Kind)*SpecialFactor("propSpeed")*prop.CycleHeat*SupportFactor("propulsion")*d.Thrust/Math.Max(1,Stat("mass"));}
  if(key=="signatureRadius")foreach(var s in states.Where(s=>s.Active))value*=1+s.Definition!.Signature*(s.Definition.Kind=="MWD"?HullFactor("prop","signature","MWD"):1);
  return value;
 }
}
