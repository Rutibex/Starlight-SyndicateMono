using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record SupportDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Family{get;init;}="";public string Slot{get;init;}="high";public string Role{get;init;}="";
 public int Size{get;init;}public int ReferenceTypeId{get;init;}public int MaxGroup{get;init;}public int ChargeUse{get;init;}=1;
 public float Amount{get;init;}public float Capacitor{get;init;}public float Cycle{get;init;}public float Optimal{get;init;}public float Falloff{get;init;}public float Cpu{get;init;}public float Power{get;init;}public float Volume{get;init;}public decimal Price{get;init;}
 public float MagazineVolume{get;init;}public float Reload{get;init;}public float PasteMultiplier{get;init;}=3;public float RampStep{get;init;}public float RampMax{get;init;}public float HeatDamage{get;init;}public float ModuleHp{get;init;}=40;public float OverloadCycle{get;init;}public float OverloadAmount{get;init;}public float BurstDuration{get;init;}
 public string[] AllowedCells{get;init;}=[];public Dictionary<string,float> Bonuses{get;init;}=[];
 public bool Delayed=>Kind is "Armor" or "AncArmor" or "Hull" or "Ramp";
 public bool Repair=>Kind is "Armor" or "AncArmor" or "Shield" or "AncShield" or "Hull" or "Ramp";
 public string Description=>$"{Family} / {Slot} power / class {Size}. CPU {Cpu:0.#}, grid {Power:0.#}, volume {Volume:0.#} m3. Output {Amount:0.#}, capacitor {Capacitor:0.#} GJ, cycle {Cycle:0.##}s. Optimal {Optimal/1000:0.#} km + falloff {Falloff/1000:0.#} km. Reload {Reload:0.#}s. "+(Kind=="Burst"?$"Fleet-only area pulse. Duration {BurstDuration:0.#}s; charge selects bonus. Same-charge pulses use strongest bonus. ":Delayed?"Repair lands at cycle end; shield and energy transfers land at cycle start. ":"")+(Kind=="AncArmor"?"Paste increases repair; dry cycles still cost capacitor. ":Kind=="AncShield"?"Compatible cells remove capacitor cost; dry cycles consume capacitor. ":Kind=="Ramp"?$"Adaptive logistics hull only; +{RampStep:P0} per uninterrupted cycle, up to +{RampMax:P0}. ":"")+string.Join("; ",Bonuses.Select(x=>$"{x.Key} +{x.Value:P0}"));
}
public static class SupportCatalog
{
 public static readonly SupportDefinition[] All=JsonSerializer.Deserialize<SupportDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Support.json")),new JsonSerializerOptions(WeaponCatalog.JsonOptions){PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,SupportDefinition> ById=All.ToDictionary(x=>x.Id);
}
public sealed record SupportPayload
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Family{get;init;}="";public string Mode{get;init;}="";public bool Consumable{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public int ReferenceTypeId{get;init;}public Dictionary<string,float> Bonuses{get;init;}=[];
 public bool Fits(SupportDefinition d)=>Family==d.Kind&&d.Kind is "Sensor" or "Tracking"||d.Kind=="AncArmor"&&Family=="Paste"||d.Kind=="AncShield"&&d.AllowedCells.Contains(Id)||d.Kind=="Burst"&&Family==d.Family;
 public string Description=>Consumable?"Command pulse charge. "+string.Join("; ",Bonuses.Select(x=>x.Key=="miningSurvey"?$"Integrated survey tier {x.Value:0}":$"{x.Key} {x.Value:+0.0%;-0.0%;0%}")):"Reusable targeted support focus script. Doubles selected bonus and removes the other bonuses.";
}
public static class SupportPayloads
{
 public static readonly SupportPayload[] New=JsonSerializer.Deserialize<SupportPayload[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","SupportPayloads.json")),new JsonSerializerOptions(WeaponCatalog.JsonOptions){PropertyNameCaseInsensitive=true})!;
 public static readonly SupportPayload[] All=New.Concat(DefensePayloads.All.Where(x=>x.Kind is "Paste" or "Cell").Select(x=>new SupportPayload{Id=x.Id,Name=x.Name,Family=x.Kind,Consumable=true,Volume=x.Volume,Price=x.Price})).ToArray();
 public static readonly Dictionary<string,SupportPayload> ById=All.ToDictionary(x=>x.Id);
 public static SupportPayload[] For(SupportDefinition d)=>All.Where(x=>x.Fits(d)).ToArray();
}
public sealed partial class SupportState
{ public bool Online{get;set;}=true; public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";public string PayloadId{get;set;}="";public string TargetId{get;set;}="";public string PendingTarget{get;set;}="";public int Loaded{get;set;}
 public bool Active{get;set;}public bool Repeat{get;set;}=true;public bool AutoReload{get;set;}=true;public bool Overheat{get;set;}public bool Burnt{get;set;}public float Damage{get;set;}public float CycleLeft{get;set;}public float ReloadLeft{get;set;}public float PendingAmount{get;set;}public float Ramp{get;set;}
 [JsonIgnore]public SupportDefinition? Definition=>MutationRulesQ.Apply(SupportCatalog.ById.GetValueOrDefault(ModuleId),MutationQ);
 [JsonIgnore]public SupportPayload? Payload=>SupportPayloads.ById.GetValueOrDefault(PayloadId);
 public static SupportState Create(string id)=>new(){ModuleId=id};
}
public sealed class SupportEffect
{
 public string Source{get;set;}="";public string Key{get;set;}="";public string Name{get;set;}="";public bool Burst{get;set;}public float Left{get;set;}public Dictionary<string,float> Bonuses{get;set;}=[];
}
public sealed partial class Ship
{
 public SupportState[] Support{get;set;}=Enumerable.Range(0,8).Select(_=>new SupportState()).ToArray();
 public List<SupportEffect> SupportEffects{get;set;}=[];
 public double SupportCpu=>Support.Where(s=>s.Online).Sum(x=>(x.Definition==null?0:RigCpu(x.Definition.Id,x.Definition.Cpu)));public double SupportPower=>Support.Where(s=>s.Online).Sum(x=>(x.Definition==null?0:RigPower(x.Definition.Id,x.Definition.Power)));
 public float SupportBonus(string key)
 {var effects=SupportEffects.Where(e=>e.Left>0&&e.Bonuses.ContainsKey(key)).ToArray();return effects.Where(e=>e.Burst).GroupBy(e=>key).Select(g=>g.OrderByDescending(e=>Math.Abs(e.Bonuses[key])).First().Bonuses[key]).Sum();}
 public float SupportFactor(string key,IEnumerable<float>? extras=null)
 {float special=SpecialFactor(key);if(SupportEffects.Count==0&&!Enhancements.Any(s=>s.Definition!=null)&&!InstalledRigs.Any())return special;var effects=SupportEffects.Where(e=>e.Left>0&&e.Bonuses.ContainsKey(key));var values=Enhancements.SelectMany(s=>s.EffectiveBonuses()).Where(b=>b.Key==key).Select(b=>b.Value).Concat(RigSupportStrengths(key)).Concat(extras??Enumerable.Empty<float>()).Concat(effects.Where(e=>!e.Burst).Select(e=>e.Bonuses[key])).Concat(effects.Where(e=>e.Burst).GroupBy(e=>key).Select(g=>g.OrderByDescending(e=>Math.Abs(e.Bonuses[key])).First().Bonuses[key])).ToArray();return special*DemoWorld.StackMultiplier(values.Where(v=>v>0),true)*DemoWorld.StackMultiplier(values.Where(v=>v<0).Select(Math.Abs));}
 public IEnumerable<float> SupportStrengths(string key)=>SupportEffects.Where(e=>e.Left>0&&e.Bonuses.ContainsKey(key)&&!e.Burst).Select(e=>e.Bonuses[key]).Concat(SupportEffects.Where(e=>e.Left>0&&e.Bonuses.ContainsKey(key)&&e.Burst).GroupBy(e=>key).Select(g=>g.OrderByDescending(e=>Math.Abs(e.Bonuses[key])).First().Bonuses[key]));
 public double SupportStat(string key,double value)
 {if(SupportEffects.Count==0&&!Enhancements.Any(s=>s.Definition!=null)&&!InstalledRigs.Any()||key.Contains("DamageResonance"))return value;if(key.StartsWith("scan")&&key.EndsWith("Strength"))return value*SupportFactor("sensorStrength");return value*SupportFactor(key);}
 public float RepairCadence(string kind,int size=0,bool remote=false)
 {string key=kind is "Armor" or "AncArmor" or "Ramp" or "ArmorRepair" or "AncillaryArmor"?"armorCycle":kind is "Shield" or "AncShield" or "ShieldBoost" or "AncillaryShield"?"shieldCycle":"none";return (remote?RemoteModeFactor("remoteCycle",kind,size):LocalModeFactor("localCycle",kind,size))*(size>=4&&!remote?1:SupportFactor(key))*(remote?1:RigFactor(key,false)*HullFactor("local","cycle",key=="armorCycle"?"Armor":key=="shieldCycle"?"Shield":"none"));}

 public double SupportCapUse=>Support.Where(s=>s.Online&&s.Active&&s.Definition!=null&&!s.Burnt).Sum(s=>s.Definition!.Kind=="AncShield"&&s.Loaded>0?0:HullSupport(s.Definition)!.Capacitor*RigRemoteCap(s.Definition.Kind)/HullSupport(s.Definition)!.Cycle*((s.Definition.Repair||s.Definition.Kind=="Cap")&&!Specialist&&CombatHulls.Get(CombatHull).Role is "Logistics" or "AdaptiveLogistics"?.45:1)/(s.Overheat?1+s.Definition.OverloadCycle:1));
}
