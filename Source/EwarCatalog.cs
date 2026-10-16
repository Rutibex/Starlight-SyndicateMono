using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public enum EwarKind { ECM, BurstJam, Neutralizer, Siphon, Displacement, Sphere, Field, Grapple, Web, Paint, Damp, Tracking, Guidance, Point, Scram, BurstECM, BurstNeutral, BurstDamp, BurstWeb, BurstPaint, BurstPoint, BurstWeapon }
public sealed record EwarDefinition
{
 public string Id{get;init;}="";
 public string Name{get;init;}="";
 public string Family{get;init;}="";
 public string ReferenceName{get;init;}="";
 public string Slot{get;init;}="medium";
 public string Role{get;init;}="";
 public string Sensor{get;init;}="";
 public int ReferenceTypeId{get;init;}
 public int Size{get;init;}
 public EwarKind Kind{get;init;}
 public float Strength{get;init;}
 public float Amount{get;init;}
 public float Optimal{get;init;}
 public float Falloff{get;init;}
 public float Cycle{get;init;}
 public float Capacitor{get;init;}
 public float Radius{get;init;}
 public float Warmup{get;init;}
 public float Duration{get;init;}
 public int Magazine{get;init;}
 public float Reload{get;init;}
 public float Cpu{get;init;}
 public float Power{get;init;}
 public float Volume{get;init;}
 public decimal Price{get;init;}
 public bool Projected=>Kind.ToString().StartsWith("Burst")&&Kind!=EwarKind.BurstJam;
 public bool NeedsLock=>!Projected&&Kind is not (EwarKind.BurstJam or EwarKind.Sphere or EwarKind.Displacement or EwarKind.Field);
 public bool Area=>Projected||Kind is EwarKind.BurstJam or EwarKind.Sphere or EwarKind.Displacement or EwarKind.Field;
 public string Description=>$"{Kind} / {Slot} power. Optimal {Optimal/1000:0.#} km; falloff {Falloff/1000:0.#} km; cycle {Cycle:0.#} s; capacitor {Capacitor:0.#} GJ. Strength {Strength:0.##}; drain/transfer {Amount:0} GJ. "+(Radius>0?$"Radius {Radius/1000:0.#} km. ":"")+(Warmup>0?$"Windup {Warmup:0.#} s. ":"")+(Role.Length>0?$"Requires {Role} hull. ":"")+Mechanic;
 public string Mechanic=>Kind switch
 {EwarKind.ECM=>"Sensor-matched chance to jam. Victim can still lock the jammer.",EwarKind.BurstJam=>"Chance-based nearby lock break; no sustained targeted jam.",EwarKind.Neutralizer=>"Consumes your capacitor to destroy target capacitor; falloff and resistance apply.",EwarKind.Siphon=>"Transfers capacitor only while the target has a higher capacitor percentage than you.",EwarKind.Damp=>"Suppresses lock range and scan resolution; selectable scripts.",EwarKind.Tracking=>"Suppresses gun tracking and optimal/falloff; selectable scripts.",EwarKind.Guidance=>"Suppresses missile flight range or application; selectable scripts.",EwarKind.Web=>"Reduces target maximum speed.",EwarKind.Grapple=>"One per battleship/capital; strongest nearby, weakens with distance.",EwarKind.Paint=>"Increases target signature, improving weapon application.",EwarKind.Point=>"Adds warp-disruption strength; does not disable micro drives.",EwarKind.Scram=>"Stronger tackle also disables micro-warp and micro-jump systems.",EwarKind.Sphere=>"Consumes a probe to leave a timed warp-interdiction bubble.",EwarKind.Field=>"Moving bubble; reusable focus scripts switch to infinite-strength targeted tackle.",EwarKind.Displacement=>"Fuelled, telegraphed field relocates non-capital ships within the same system.",_=>"Fuelled, telegraphed area projector; hits friend and foe in the committed area."};
}
public static class EwarCatalog
{
 public static readonly EwarDefinition[] All=JsonSerializer.Deserialize<EwarDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Ewar.json")),WeaponCatalog.JsonOptions)!;
 public static readonly Dictionary<string,EwarDefinition> ById=Build();
 static Dictionary<string,EwarDefinition> Build(){var d=All.ToDictionary(e=>e.Id);d["web"]=All.First(e=>e.Kind==EwarKind.Web) with {Id="web",Optimal=6500,Capacitor=8,Cycle=4,Strength=.58f};return d;}
 public static EwarDefinition Get(string id)=>ById[id];
}
public sealed record EwarPayload(string Id,string Name,string Mode,EwarKind[] Fits,bool Consumable=false);
public static class EwarPayloads
{
 public static readonly EwarPayload[] All=[
 new("ewar_probe","Cage warp-disruption probe","Bubble",[EwarKind.Sphere],true),
 new("ewar_fuel","Stormfront field catalyst","Area",[EwarKind.Displacement,EwarKind.BurstECM,EwarKind.BurstNeutral,EwarKind.BurstDamp,EwarKind.BurstWeb,EwarKind.BurstPaint,EwarKind.BurstPoint,EwarKind.BurstWeapon],true),
 new("ewar_range_script","Fog range-focus script","Range",[EwarKind.Damp]),
 new("ewar_resolution_script","Fog resolution-focus script","Resolution",[EwarKind.Damp]),
 new("ewar_tracking_script","Prism tracking-focus script","Tracking",[EwarKind.Tracking]),
 new("ewar_turret_range_script","Prism range-focus script","Range",[EwarKind.Tracking]),
 new("ewar_application_script","Decoy application-focus script","Application",[EwarKind.Guidance]),
 new("ewar_flight_script","Decoy flight-focus script","Range",[EwarKind.Guidance]),
 new("ewar_point_script","Bastion disruption-focus script","Point",[EwarKind.Field]),
 new("ewar_scram_script","Bastion scrambling-focus script","Scram",[EwarKind.Field])];
 public static readonly Dictionary<string,EwarPayload> ById=All.ToDictionary(p=>p.Id);
 public static EwarPayload[] For(EwarDefinition e)=>All.Where(p=>p.Fits.Contains(e.Kind)).ToArray();
}
public sealed partial class EwarState
{ public bool Online{get;set;}=true; public string InstanceId{get;set;}=Guid.NewGuid().ToString("N");
 public string ModuleId{get;set;}="";
 public string PayloadId{get;set;}="";
 public string TargetId{get;set;}="";
 public bool Overheat{get;set;}public float Damage{get;set;}public bool Burnt{get;set;}
 public bool Active{get;set;}
 public float Cooldown{get;set;}
 public float ReloadLeft{get;set;}
 public float WarmupLeft{get;set;}
 public int Loaded{get;set;}
 public float AimX{get;set;}
 public float AimY{get;set;}
 public float AimZ{get;set;}
 [JsonIgnore] public EwarDefinition? Definition=>MutationRulesQ.Apply(EwarCatalog.ById.GetValueOrDefault(ModuleId),MutationQ);
 [JsonIgnore] public string Mode=>EwarPayloads.ById.GetValueOrDefault(PayloadId)?.Mode??(Definition?.Kind==EwarKind.Field?"Bubble":"Combined");
 public void Stop(){Active=false;TargetId="";}
 public static EwarState Create(string id)=>new(){ModuleId=id};
}
public sealed partial class Ship
{
 public EwarState[] Ewar{get;set;}=[new(),EwarState.Create("web"),new(),new()];
 public bool MicroWarpOn{get;set;}
 public string MicroJumpTarget{get;set;}="";
 public string NativeSensor{get;set;}="Magnetometric";
}
