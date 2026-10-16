using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class DroneAbility
{
 public string Kind{get;set;}="";public string Name{get;set;}="";public float Cooldown{get;set;} public int Charges{get;set;} public float Rearm{get;set;} public float Range{get;set;} public DamagePacket Packet{get;set;}public float ExplosionRadius{get;set;}=180;public float ExplosionVelocity{get;set;}=100;
}
public sealed partial class DroneDefinition
{
 public string Id{get;set;}="";public string Name{get;set;}="";public string Kind{get;set;}="";public string Family{get;set;}="";public string Role{get;set;}="";public string Effect{get;set;}="";public string Sensor{get;set;}="Radar";
 public int Size{get;set;} public int SquadronSize{get;set;}=1;public int Price{get;set;}
 public float Volume{get;set;}public float Bandwidth{get;set;}public float Speed{get;set;}public float Orbit{get;set;}public float Optimal{get;set;}public float Falloff{get;set;}public float Tracking{get;set;}public float SignatureResolution{get;set;}public float Signature{get;set;}public float Shield{get;set;}public float Armor{get;set;}public float Hull{get;set;}public float Cycle{get;set;}public float Strength{get;set;}public float Repair{get;set;}public float Yield{get;set;}public float MiningCycle{get;set;}public float ResidueChance{get;set;}public float ResidueMultiplier{get;set;}public float SensorStrength{get;set;}public float ScanResolution{get;set;}public float ShieldRecharge{get;set;}public float Agility{get;set;}public float Mass{get;set;}public float WarpSpeed{get;set;}public float AttackExplosionRadius{get;set;}public float AttackExplosionVelocity{get;set;}
 public DamagePacket Packet{get;set;}public DroneAbility[] Abilities{get;set;}=[];public Dictionary<string,float> Resonances{get;set;}=[];
 [JsonIgnore]public bool Fighter=>Abilities.Length>0;
 [JsonIgnore]public bool Harvest=>Kind is "Mine" or "Ice";
 [JsonIgnore]public bool RepairDrone=>Kind.EndsWith("Repair");
 [JsonIgnore]public string Description=>$"{Family}. Volume {Volume:0} m3; bandwidth {Bandwidth:0}; squadron {SquadronSize}. Speed {Speed:0} m/s, orbit {Orbit/1000:0.0} km; optimal {Optimal/1000:0.0} / falloff {Falloff/1000:0.0} km; tracking {Tracking:0.000}; signature {Signature:0}, resolution {SignatureResolution:0}. Shield {Shield:0}, armor {Armor:0}, hull {Hull:0}. Volley EM {Packet.EM:0.#} / thermal {Packet.Thermal:0.#} / kinetic {Packet.Kinetic:0.#} / explosive {Packet.Explosive:0.#}; cycle {Cycle:0.#} s. {Effect} strength {Strength:0.###}. Repair {Repair:0.#}/cycle; yield {Yield:0.#} m3/{MiningCycle:0.#} s; residue {ResidueChance:P0} x{ResidueMultiplier:0.#}. Sensor {Sensor} {SensorStrength:0.#}; scan {ScanResolution:0}; shield recharge {ShieldRecharge:0}s. Mass {Mass:0} kg / agility {Agility:0.###} / warp {WarpSpeed:0.##}. Shield resist EM {1-Resonances.GetValueOrDefault("shieldEmDamageResonance",1):P0} / thermal {1-Resonances.GetValueOrDefault("shieldThermalDamageResonance",1):P0} / kinetic {1-Resonances.GetValueOrDefault("shieldKineticDamageResonance",1):P0} / explosive {1-Resonances.GetValueOrDefault("shieldExplosiveDamageResonance",1):P0}. Armor resist EM {1-Resonances.GetValueOrDefault("armorEmDamageResonance",1):P0} / thermal {1-Resonances.GetValueOrDefault("armorThermalDamageResonance",1):P0} / kinetic {1-Resonances.GetValueOrDefault("armorKineticDamageResonance",1):P0} / explosive {1-Resonances.GetValueOrDefault("armorExplosiveDamageResonance",1):P0}. Role: {(Role.Length==0?"ordinary":Role)}. "+string.Join("; ",Abilities.Select(a=>$"{a.Name}: {a.Cooldown:0}s CD, {a.Charges} charges, {a.Rearm:0}s rearm"));
}
public static class DroneCatalog
{
 public static readonly DroneDefinition[] All=JsonSerializer.Deserialize<DroneDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Drones.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??[];
 public static readonly Dictionary<string,DroneDefinition> ById=All.ToDictionary(d=>d.Id);
}
public sealed class DroneIntegrity {public float Shield{get;set;}public float Armor{get;set;}public float Hull{get;set;}}
public sealed class DroneUnit
{
 public string MissionPayload38{get;set;}="";
 public string Id{get;set;}="drone_"+Guid.NewGuid().ToString("N");public string TypeId{get;set;}="";public string Group{get;set;}="Alpha";public string Location{get;set;}="Bay";public string Owner{get;set;}="player";public string System{get;set;}="";public string Grid{get;set;}="";public string Order{get;set;}="Idle";public string TargetId{get;set;}="";public string AssistId{get;set;}="";public string AssistMode{get;set;}="";
 public int Count{get;set;}=1;public int Tube{get;set;}=-1;public Vector3 Position{get;set;}public Vector3 MovePoint{get;set;}public float Shield{get;set;}public float Armor{get;set;}public float Hull{get;set;}public float CycleLeft{get;set;}public float Payload{get;set;}public float BoostLeft{get;set;}public float EvasionLeft{get;set;}public float JumpLeft{get;set;}public float ReturnWarpLeft{get;set;}public float RearmLeft{get;set;}public bool AttackOn{get;set;}=true;public bool MineOnce{get;set;}public bool HarvestCycling{get;set;}public bool SalvageCycling{get;set;}
 public float AppliedRigHpFactor{get;set;}=1;
 public float AppliedShieldFactor{get;set;}=1;
 public float[] AbilityCooldowns{get;set;}=new float[3];public int[] AbilityCharges{get;set;}=new int[3];public float[] AbilityReloads{get;set;}=new float[3];public List<DroneIntegrity> ReserveHealth{get;set;}=[];
 [JsonIgnore]public DroneDefinition Definition=>DroneCatalog.ById[TypeId];
 public static DroneUnit Create(string type,int count=1,int tube=-1){var d=DroneCatalog.ById[type];var u=new DroneUnit{TypeId=type,Count=count,Tube=tube,Shield=d.Shield,Armor=d.Armor,Hull=d.Hull};u.ReserveHealth=Enumerable.Range(0,Math.Max(0,count-1)).Select(_=>new DroneIntegrity{Shield=d.Shield,Armor=d.Armor,Hull=d.Hull}).ToList();for(int i=0;i<d.Abilities.Length;i++)u.AbilityCharges[i]=d.Abilities[i].Charges;return u;}
}
public sealed partial class Ship
{
 public List<DroneUnit> Drones{get;set;}=[];public bool DroneAggressive{get;set;}public bool DroneFocusFire{get;set;}=true;
 public float FighterBayUsed=>Drones.Where(u=>u.Owner=="player"&&u.Definition.Fighter&&u.Location is "Bay" or "Reserve").Sum(u=>u.Count*u.Definition.Volume);
 public float DroneBayUsed=>Drones.Where(u=>u.Owner=="player"&&!u.Definition.Fighter&&u.Location=="Bay").Sum(u=>u.Count*u.Definition.Volume);
 public double ActiveDroneDps=>Drones.Where(u=>u.Owner=="player"&&u.Location=="Space").Sum(u=>EnhancedDroneDps(u.Definition,u.Count));
 public double BayDroneDps
 {
  get {double fighters=Drones.Where(u=>u.Location=="Bay"&&u.Definition.Fighter&&u.Owner=="player").Sum(u=>EnhancedDroneDps(u.Definition,u.Count));var choices=Drones.Where(u=>u.Owner=="player"&&u.Location=="Bay"&&!u.Definition.Fighter).ToArray();int bw=(int)Stat("droneBandwidth"),n=(int)Stat("maxActiveDrones");var scores=new double[n+1,bw+1];foreach(var u in choices){int cost=(int)u.Definition.Bandwidth;for(int k=n;k>=1;k--)for(int b=bw;b>=cost;b--)scores[k,b]=Math.Max(scores[k,b],scores[k-1,b-cost]+EnhancedDroneDps(u.Definition));}return fighters+scores.Cast<double>().Max();}
 }
 public void ConfigureDroneHull()
 {
  var h=CombatHulls.Get(CombatHull);bool f=h.Role is "Carrier" or "Supercarrier";SetStat("droneCapacity",f?0:h.Role=="CapitalIndustrial"?9000:h.Size==1?45:h.Size==2?160:375);SetStat("droneBandwidth",f?0:h.Size==1?25:h.Size==2?60:125);SetStat("maxActiveDrones",f?0:5);SetStat("droneControlDistance",48000);SetStat("fighterCapacity",f?h.Role=="Carrier"?120000:240000:0);SetStat("fighterTubes",f?h.Role=="Carrier"?3:5:0);SetStat("fighterLightSlots",f?3:0);SetStat("fighterHeavySlots",h.Role=="Supercarrier"?3:0);SetStat("fighterSupportSlots",f?1:0);
 }
}
