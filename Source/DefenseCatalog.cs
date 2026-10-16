using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record DefenseDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Family{get;init;}="";public string Slot{get;init;}="";public string Role{get;init;}="";public string UniqueGroup{get;init;}="";
 public string[] AllowedCells{get;init;}=[];
 public int Size{get;init;} public int ReferenceTypeId{get;init;} public bool Active{get;init;}
 public float HeatDamage{get;init;}public float ModuleHp{get;init;}=40;public float OverloadCycle{get;init;}public float OverloadAmount{get;init;}
 public float Amount{get;init;} public float Capacitor{get;init;} public float Cycle{get;init;} public float Reload{get;init;} public float Reactivation{get;init;} public float Cpu{get;init;} public float Power{get;init;} public float Volume{get;init;} public decimal Price{get;init;}
 public float MagazineVolume{get;init;} public int ChargeUse{get;init;}=1;public float PasteMultiplier{get;init;}=3;public float BurstResonance{get;init;}=.25f;public float ReactiveShift{get;init;}=.054f;public float WarpStrength{get;init;}
 public Dictionary<string,float> Bonuses{get;init;}=[];public Dictionary<string,float> Resonances{get;init;}=[];
 public bool Burst=>Kind is "AssaultControl" or "Emergency" or "Nullifier" or "Stabilizer" or "Suppressor";
 public bool DelayedRepair=>Kind is "ArmorRepair" or "AncillaryArmor" or "HullRepair";
 public string Mechanic=>Kind switch
 {"AncillaryShield"=>"Cells remove capacitor cost; empty cycles use capacitor. Long reload. Shield repair applies at cycle start.","AncillaryArmor"=>"Always consumes capacitor. Paste boosts repair; partial paste scales bonus. Repair applies at cycle end. One per ship.","ArmorRepair" or "HullRepair"=>"Consumes capacitor at cycle start; restores the damaged layer at cycle end.","ShieldBoost"=>"Consumes capacitor and restores shield at cycle start.","Reactive"=>"Adapts its finite resistance pool to actual armor-layer damage. Resets when restarted.","FlexArmor" or "FlexShield"=>"Active capital defense. Reusable scripts focus the resistance pool into one damage type.","AssaultControl"=>"Passive layer defenses with a short active resistance burst. Assault hulls only. Shares damage-control fitting limit.","Emergency"=>"Capital structure protection. Burns out after one activation; repair the module at a station. Shares damage-control limit.","CapBooster"=>"Consumes a real cell from the loaded magazine to inject capacitor. Cell volume determines magazine capacity.","Battery"=>"Adds capacitor capacity and reduces hostile neutralizer/siphon effects.","Nullifier"=>"Temporary bubble immunity; targeted tackle still applies. Fitted sensor/bandwidth penalties. Burst jammers disable nullification.","Stabilizer"=>"Temporary warp strength against targeted tackle; bubbles and scram micro-drive shutdown still apply. Fitted sensor/bandwidth penalties.","Suppressor"=>"Battleship-only passive signature reduction plus short active suppression; long cooldown.","ShieldFlux"=>"Faster shield recharge at the cost of shield capacity.","ShieldRelay"=>"Faster shield recharge at the cost of capacitor recharge.","CapFlux"=>"Faster capacitor recharge at the cost of capacitor capacity.","CapRelay"=>"Faster capacitor recharge at the cost of shield boosting.","Bulkhead"=>"More structure hitpoints, with cargo and maneuvering drawbacks.","Plate"=>"Adds armor hitpoints and mass, increasing alignment time.","Extender"=>"Adds shield hitpoints and signature radius.",_=>Active?"Active defense consumes capacitor each cycle.":"Passive fitted effect; no activation capacitor."};
 public string Description=>$"{Family} / {Slot} power / class {Size}. CPU {Cpu:0.#}; grid {Power:0.#}; volume {Volume:0.#} m3. Repair {Amount:0.#}; capacitor {Capacitor:0.#} GJ; cycle {Cycle:0.#}s; reload {Reload:0.#}s; reactivation {Reactivation:0.#}s. "+(Role.Length>0?$"Requires {Role}. ":"")+Mechanic+" "+string.Join("; ",Bonuses.Select(p=>$"{p.Key} {p.Value:0.###}"))+" "+string.Join("; ",Resonances.Where(p=>p.Value<1).Select(p=>$"{p.Key.Replace("DamageResonance","")} resist {(1-p.Value):P0}"));
}
public static class DefenseCatalog
{
 public static readonly DefenseDefinition[] All=JsonSerializer.Deserialize<DefenseDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Defense.json")),new JsonSerializerOptions(WeaponCatalog.JsonOptions){PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,DefenseDefinition> ById=Build();
 static Dictionary<string,DefenseDefinition> Build(){var d=All.ToDictionary(d=>d.Id);d["booster"]=All.First(d=>d.Kind=="ShieldBoost"&&d.Size==1) with{Id="booster",Name="Shield pulse booster",Amount=34,Capacitor=21,Cycle=4,Cpu=0,Power=0};return d;}
}
public sealed record DefensePayload
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Kind{get;init;}="";public string Damage{get;init;}="";public float Amount{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public int ReferenceTypeId{get;init;}
 public bool Consumable=>Kind is "Cell" or "Paste";
 public bool Fits(DefenseDefinition d)=>Kind=="Paste"&&d.Kind=="AncillaryArmor"||Kind=="ArmorScript"&&d.Kind=="FlexArmor"||Kind=="ShieldScript"&&d.Kind=="FlexShield"||Kind=="Cell"&&d.Kind is "CapBooster" or "AncillaryShield"&&Volume<=d.MagazineVolume&&(d.Kind!="AncillaryShield"||d.AllowedCells.Contains(Id));
}
public static class DefensePayloads
{
 public static readonly DefensePayload[] All=JsonSerializer.Deserialize<DefensePayload[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","DefensePayloads.json")),new JsonSerializerOptions(WeaponCatalog.JsonOptions){PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,DefensePayload> ById=All.ToDictionary(p=>p.Id);
 public static DefensePayload[] For(DefenseDefinition d)=>All.Where(p=>p.Fits(d)).ToArray();
}
public sealed partial class DefenseState
{ public bool MutatedE{get;set;}public float MutationAmountE{get;set;}=1;public float MutationCapE{get;set;}=1;public float MutationCpuE{get;set;}=1;public float MutationPowerE{get;set;}=1;public bool Online{get;set;}=true; public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";public string PayloadId{get;set;}="";public int Loaded{get;set;}public bool Active{get;set;}public bool Repeat{get;set;}=true;public bool AutoReload{get;set;}=true;
 public bool Overheat{get;set;}public bool Burnt{get;set;}public float Damage{get;set;}public float CycleLeft{get;set;}public float ReloadLeft{get;set;}public float ReactivationLeft{get;set;}public float PendingRepair{get;set;}public float EffectLeft{get;set;}public bool ChargedCycle{get;set;}
 public float[] ReactiveResists{get;set;}=[.135f,.135f,.135f,.135f];public float[] Incoming{get;set;}=[0,0,0,0];
 [JsonIgnore]public DefenseDefinition? Definition=>MutationRulesQ.Apply(DefenseCatalog.ById.GetValueOrDefault(ModuleId),MutationQ);
 [JsonIgnore]public DefensePayload? Payload=>DefensePayloads.ById.GetValueOrDefault(PayloadId);
 [JsonIgnore]public bool Running=>Online&&EffectLeft>0&&!Burnt;
 public static DefenseState Create(string id)=>new(){ModuleId=id};
}
public sealed partial class Ship
{
 public DefenseState[] Defense{get;set;}=Enumerable.Range(0,12).Select(i=>i==1?DefenseState.Create("booster"):new DefenseState()).ToArray();
 public double DefenseCpu=>Defense.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigCpu(s.Definition.Id,s.Definition.Cpu)*s.MutationCpuE));public double DefensePower=>Defense.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:RigPower(s.Definition.Id,s.Definition.Power)*s.MutationPowerE));
 public static string DefenseSlot(int index)=>index==0?"high":index<6?"medium":"low";
 public double DefenseBoostMultiplier
 {get{var a=Defense.Where(s=>s.Online&&!s.Burnt&&s.Definition!=null).Select(s=>s.Definition!.Bonuses.GetValueOrDefault("shieldBoostMultiplier")).ToArray();return DemoWorld.StackMultiplier(a.Where(x=>x>0),true)*DemoWorld.StackMultiplier(a.Where(x=>x<0).Select(Math.Abs));}}
 public (float Capacitor,float Duration,float Amount) DefenseQuoteB(DefenseState s)
 {
  var d=s.Definition;if(d==null)return (0,1,0);bool hot=s.Overheat&&d.HeatDamage>0;
  float cap=d.Kind=="AncillaryShield"&&s.Loaded>0?0:d.Capacitor*s.MutationCapE*RigLocalCap(d.Kind,d.Id)/RigFactor(d.Kind is "ArmorRepair" or "AncillaryArmor"?"armorCycle":d.Kind is "ShieldBoost" or "AncillaryShield"?"shieldCycle":"none",false)*RepairCadence(d.Kind,d.Size)/LocalModeFactor("localCycle",d.Kind,d.Size)*LocalModeFactor("localCap",d.Kind,d.Size);
  float duration=d.Cycle*RepairCadence(d.Kind,d.Size)*(hot?1+d.OverloadCycle*ScopedOverheat("local",d.Kind is "ShieldBoost" or "AncillaryShield"?"Shield":"Armor"):1)*(d.Kind=="Nullifier"?NullifierFactor("duration"):1);
  float amount=d.Amount*s.MutationAmountE*RigLocalAmount(d.Kind)*LocalModeFactor("localRepair",d.Kind,d.Size)*(hot?1+d.OverloadAmount*ScopedOverheat("local",d.Kind is "ShieldBoost" or "AncillaryShield"?"Shield":"Armor"):1);
  if(d.Kind=="AncillaryArmor")amount*=1+(d.PasteMultiplier-1)*(float)Math.Min(s.Loaded,d.ChargeUse)/Math.Max(1,d.ChargeUse);
  if(d.Kind is "ShieldBoost" or "AncillaryShield")amount*=(float)DefenseBoostMultiplier;
  return (cap,Math.Max(.001f,duration),amount);
 }
 public double DefenseCapUse=>Defense.Where(s=>s.Online&&s.Active&&!s.Burnt&&s.Definition?.Active==true).Sum(s=>{var q=DefenseQuoteB(s);return q.Capacitor/q.Duration;});
 public double DefenseRepairRate(string layer)=>Defense.Where(s=>s.Online&&!s.Burnt&&s.Definition!=null).Sum(s=>{var d=s.Definition!;bool match=layer=="shield"?d.Kind is "ShieldBoost" or "AncillaryShield":layer=="armor"?d.Kind is "ArmorRepair" or "AncillaryArmor":d.Kind=="HullRepair";var q=DefenseQuoteB(s);return match?q.Amount/q.Duration:0;});
 public double DefenseStat(string key,double value)
 {
  var states=Defense.Where(s=>s.Online&&s.Definition!=null&&!s.Burnt).ToArray();
  if(key.Contains("DamageResonance"))
  {
   var normal=SupportStrengths(key.StartsWith("shield")?"shieldResist":key.StartsWith("armor")?"armorResist":"none").Select(v=>-v).ToList();normal.AddRange(RigStrengths(key).Select(v=>-v));var exceptional=new List<float>();
   foreach(var s in states)
   {
    var d=s.Definition!;float r=d.Resonances.GetValueOrDefault(key,1);
    if(d.Kind=="Reactive") {if(!s.Running||!key.StartsWith("armor"))continue;int index=key.Contains("Em")?0:key.Contains("Thermal")?1:key.Contains("Kinetic")?2:3;r=1-s.ReactiveResists[index];}
    else if(d.Kind=="AssaultControl") {if(s.Running)r*=d.BurstResonance;}
    else if(d.Active&&!s.Running)continue;
    if(d.Kind is "FlexArmor" or "FlexShield") {if(s.Payload!=null){string damage=s.Payload.Damage=="EM"?"Em":s.Payload.Damage;r=key.Contains(damage)?Math.Max(.01f,1-(1-r)*3):1;}}
    if(s.Overheat&&s.Running&&d.Kind is "ShieldHardener" or "ArmorHardener" or "FlexArmor" or "FlexShield")r=Math.Max(.01f,1-(1-r)*(1+.15f*ScopedOverheat("local",d.Kind is "ShieldHardener" or "FlexShield"?"ShieldHardener":"ArmorHardener")));
    if(d.Kind is "Reactive" or "DamageControl" or "AssaultControl")exceptional.Add(1-r);else if(d.Kind=="Emergency")value*=r;else normal.Add(1-r);
   }
   return Math.Clamp(value*DemoWorld.StackMultiplier(normal)*DemoWorld.StackMultiplier(exceptional),0,1);
  }
  string add=key switch{"shieldCapacity"=>"shieldBonus","armorHP"=>"armorHPBonusAdd","capacitorCapacity"=>"capacitorBonus","mass"=>"massAddition","signatureRadius"=>"signatureRadiusAdd",_=>""};
  if(add.Length>0)value+=states.Sum(s=>s.Definition!.Bonuses.GetValueOrDefault(add)*(add=="massAddition"&&s.Definition.Kind=="Plate"?HullFactor("plate","mass"):add=="armorHPBonusAdd"&&s.Definition.Kind=="Plate"?HullFactor("tank","plate"):add=="signatureRadiusAdd"&&s.Definition.Kind=="Extender"?HullFactor("tank","extenderSignature"):add=="shieldBonus"&&s.Definition.Kind=="Extender"?HullFactor("tank","extender"):1));
  string mult=key switch{"shieldCapacity"=>"shieldCapacityMultiplier","armorHP"=>"armorHPMultiplier","hp"=>"structureHPMultiplier","capacitorCapacity"=>"capacitorCapacityMultiplier","rechargeRate"=>"capacitorRechargeRateMultiplier","shieldRechargeRate"=>"shieldRechargeRateMultiplier","capacity"=>"cargoCapacityMultiplier","agility"=>"agilityMultiplier","maxVelocity"=>"maxVelocityMultiplier","maxTargetRange"=>"maxTargetRangeMultiplier","scanResolution"=>"scanResolutionMultiplier","droneBandwidth"=>"droneBandwidthMultiplier",_=>""};
  if(mult.Length>0)foreach(var s in states){double m=s.Definition!.Bonuses.GetValueOrDefault(mult,1);if(mult=="structureHPMultiplier"&&s.Definition.Kind=="Bulkhead")m+=HullFlat("tank","bulkhead");if(s.Definition.Kind=="Nullifier"&&mult is "maxTargetRangeMultiplier" or "scanResolutionMultiplier")m=1-(1-m)*NullifierFactor(mult=="maxTargetRangeMultiplier"?"rangePenalty":"scanPenalty");value*=m;}
  if(key=="energyWarfareResistance")value*=DemoWorld.StackMultiplier(states.Select(s=>-s.Definition!.Bonuses.GetValueOrDefault("energyWarfareResistanceBonus")));
  if(key=="signatureRadius")foreach(var s in states.Where(s=>s.Definition!.Kind=="Suppressor"))value*=s.Definition!.Bonuses.GetValueOrDefault(s.Running?"signatureActive":"signaturePassive",1);
  if(key=="warpCoreStrength")value+=states.Where(s=>s.Running&&s.Definition!.Kind=="Stabilizer").Sum(s=>s.Definition!.WarpStrength);
  if(key=="warpBubbleImmune"&&states.Any(s=>s.Running&&s.Definition!.Kind=="Nullifier")&&!Ewar.Any(e=>e.Definition?.Kind==EwarKind.BurstJam))value=1;
  if(key=="shieldBoostAmount")value*=DefenseBoostMultiplier;
  return value;
 }
}
