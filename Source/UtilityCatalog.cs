using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed record UtilityDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Family{get;init;}="";public string Slot{get;init;}="";public string Role{get;init;}="";public int Size{get;init;}public int ReferenceTypeId{get;init;}public int MaxGroup{get;init;}public int VirusSlots{get;init;}public int VirusCoherence{get;init;}public int VirusStrength{get;init;}
 public float Cpu{get;init;}public float Power{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public float Capacitor{get;init;}public float Cycle{get;init;}public float Range{get;init;}public float Falloff{get;init;}public float Amount{get;init;}public float SpecialAmount{get;init;}public float TractorSpeed{get;init;}public float CpuPenalty{get;init;}public float CritChance{get;init;}public float CritYield{get;init;}public float ResidueChance{get;init;}public float ResidueVolume{get;init;}public float CloudChance{get;init;}public float SalvageChance{get;init;}public float Reload{get;init;}public Dictionary<string,float> Bonuses{get;init;}=[];
 public bool Harvest=>Kind is "Miner" or "DeepMiner" or "Strip" or "DeepStrip" or "CrystalDeepMiner" or "CrystalDeepStrip" or "CrystalStrip" or "IceHarvest" or "IceLaser" or "GasScoop" or "GasHarvest";
 public bool Crystal=>Kind.StartsWith("Crystal");public bool Analyzer=>Kind is "Data" or "Relic" or "Integrated";public bool Probe=>Kind is "CoreProbe" or "CombatProbe" or "SurveyProbe";public bool Passive=>Kind is "Chipset" or "OreUpgrade" or "IceUpgrade" or "CombinedUpgrade" or "Deviation" or "Strength" or "ScanTime";
 public string Group=>Kind is "CoreProbe" or "CombatProbe"?"ScanLauncher":Kind;
 public string Description=>$"{Family} / {Slot}. CPU {Cpu:0.#}; grid {Power:0.#}; volume {Volume:0.#} m3. "+(!Passive?$"{Capacitor:0.#} GJ / {Cycle:0.##}s; range {Range/1000:0.#} km. ":"")+(Harvest?$"Yield {Amount:0.#} m3/cycle. "+(Crystal?$"Matched crystal uses {SpecialAmount:0.#} specialty yield. ":""):"")+(Analyzer?$"Virus {VirusCoherence} coherence / {VirusStrength} strength / {VirusSlots} utility slots. ":"")+(Kind=="Compress"?"Active fleet service; deployed industrial core required. ":"")+(CpuPenalty>0?$"Matching harvester CPU penalty +{CpuPenalty:P1}. ":"")+string.Join("; ",Bonuses.Select(x=>$"{x.Key} {x.Value:+0.0%;-0.0%;0%}"))+(Role.Length>0?". Hull: "+Role:".");
}
public static class UtilityCatalog
{
 public static readonly UtilityDefinition[] All=JsonSerializer.Deserialize<UtilityDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Utility.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,UtilityDefinition> ById=All.ToDictionary(d=>d.Id);
}
public sealed record UtilityPayload(string Id,string Name,string Kind,string Resource,float Volume,decimal Price,float Yield=1,float Cycle=1,float Cap=1,float WasteChance=0,float WasteVolume=0,float WearChance=.023f,float WearDamage=.046f);
public static class UtilityPayloads
{
 public static readonly UtilityPayload[] All=Build();
 static UtilityPayload[] Build(){var a=new List<UtilityPayload>{new("ut_probe_core","Wayfinder scan probe","Core","",.1f,15),new("ut_probe_combat","Pursuit scan probe","Combat","",1,25),new("ut_probe_survey","Lunar assay probe","Survey","",5,35),new("ut_firework","Prismatic celebration flare","Festival","",.1f,3)};foreach(string resource in new[]{"ore","moonore","deepore"}){a.Add(new("ut_crystal_"+resource+"_A",resource+" conservation prism","Crystal",resource,1,90,1.46f));a.Add(new("ut_crystal_"+resource+"_B",resource+" rapid prism","Crystal",resource,1,110,1.46f,.908f,1.184f,.184f));a.Add(new("ut_crystal_"+resource+"_C",resource+" erosion prism","Crystal",resource,1,80,.27f,1,1.46f,.368f,16.56f));}return a.ToArray();}
 public static bool Fits(UtilityDefinition d,UtilityPayload p)=>p.Kind=="Crystal"?d.Crystal&&(p.Resource!="deepore"||d.Kind.Contains("Deep")):d.Kind=="CoreProbe"?p.Kind=="Core":d.Kind=="CombatProbe"?p.Kind is "Core" or "Combat":d.Kind=="SurveyProbe"?p.Kind=="Survey":d.Kind=="Festival"&&p.Kind=="Festival";
 public static UtilityPayload? Get(string id)=>All.FirstOrDefault(p=>p.Id==id);
}
public sealed partial class UtilityState
{ public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";public string PayloadId{get;set;}="";public string TargetId{get;set;}="";public bool Online{get;set;}=true;public bool Active{get;set;}public bool Repeat{get;set;}=true;public bool StopRequested{get;set;}public float CycleLeft{get;set;}public float CycleDuration{get;set;}public float ReloadLeft{get;set;}public int Loaded{get;set;}public float CrystalWear{get;set;}public float Damage{get;set;}public bool Burnt{get;set;}public float Harvested{get;set;}public float Residue{get;set;}public int Criticals{get;set;}
 [JsonIgnore]public UtilityDefinition? Definition=>MutationRulesQ.Apply(UtilityCatalog.ById.GetValueOrDefault(ModuleId),MutationQ);[JsonIgnore]public UtilityPayload? Payload=>UtilityPayloads.Get(PayloadId);[JsonIgnore]public bool Effective=>Online&&!Burnt&&Definition!=null;
}
public sealed partial class Ship
{
 public UtilityState[] Utilities{get;set;}=Enumerable.Range(0,16).Select(_=>new UtilityState()).ToArray();public static string UtilitySlot(int i)=>i<6?"high":i<12?"medium":"low";
 public float UtilityFactor(string key){var b=Utilities.Where(s=>s.Effective).Select(s=>s.Definition!.Bonuses.GetValueOrDefault(key)).Where(v=>v!=0);b=b.Concat(RigStrengths(key));return HullFactor("utility",key)*(key is "probeStrength" or "probeDeviation"?DemoWorld.StackMultiplier(b.Select(Math.Abs),key=="probeStrength"):b.Aggregate(1f,(n,v)=>n*(1+v)));}
 public float UtilityCpuPenalty(string family)=>Utilities.Where(s=>s.Effective&&s.Definition!.Kind==(family=="Ice"?"IceUpgrade":"OreUpgrade")||s.Effective&&family=="Ore"&&s.Definition!.Kind=="CombinedUpgrade").Aggregate(1f,(n,s)=>n*(1+s.Definition!.CpuPenalty));
 public float UtilityCpuCost(UtilityState s)=>s.Definition is {} d&&s.Online?RigCpu(d.Id,d.Cpu)*(d.Harvest&&d.Family is "Ice" or "Ore"?UtilityCpuPenalty(d.Family):1):0;
 public double UtilityCpu=>Utilities.Sum(UtilityCpuCost)+(UtilityFit=="miner"?20*(UtilityCpuPenalty("Ore")-1):0);public double UtilityPower=>Utilities.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigPower(s.Definition.Id,s.Definition.Power)));
 public double UtilityHarvestRate=>Utilities.Where(s=>s.Effective&&s.Definition!.Harvest).Sum(s=>60*(s.Definition!.Crystal&&s.Loaded>0?s.Definition.SpecialAmount*(s.Payload?.Yield??1):s.Definition.Amount)*HarvestFactor(s.Definition,"yield")*(s.Definition.Family=="Ore"?UtilityFactor("oreYield"):1)/(s.Definition.Cycle*HarvestFactor(s.Definition,"cycle")*(s.Payload?.Cycle??1)*(s.Definition.Family=="Ice"?UtilityFactor("iceCycle"):1)));
 public double UtilityCapUse=>Utilities.Where(s=>s.Active&&s.Effective).Sum(s=>s.Definition!.Capacitor*HarvestFactor(s.Definition,"cap")*(s.Payload?.Cap??1)/Math.Max(.01,s.CycleDuration));
 public double UtilityStat(string key,double value)=>value*UtilityFactor(key=="miningYield"?"oreYield":key);
}
