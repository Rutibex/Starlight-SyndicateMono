namespace StarlightSyndicateDemo;
public sealed partial class Ship
{
 public float SpecificDroneFactor(DroneDefinition d,string key){if(d.Fighter)return 1;float result=1;foreach(string filter in new[]{d.Kind=="Sentry"?"Sentry":"",d.Kind=="Combat"&&d.Volume==10?"MediumCombat":"",d.Kind=="Combat"&&d.Volume<=25?"CombatSizes":"",d.Kind=="Combat"&&d.Volume==25?"Heavy":""}.Where(f=>f.Length>0))result*=HullFactor("drone",key,filter)/HullFactor("drone",key);return result;}
 public float SecurityTackleRange(string filter)=>filter=="Web"&&HullCapability("stasis webifier optimal range increased")?1+Math.Clamp(-PilotSecurity*.075f,0,.75f):filter is "Point" or "Scram"&&HullCapability("warp scrambler and warp disruptor optimal range increased")?1+Math.Clamp(-PilotSecurity*.0375f,0,.375f):1;
 public bool InterdictionAssistanceBlocked=>ShipBatch21.ById.TryGetValue(CombatHull,out var p)&&p.Batch==27&&Ewar.Any(s=>s.Active&&s.Definition?.Kind==EwarKind.Field&&s.Mode=="Bubble");
 public double FieldStat(string key,double value)=>key=="signatureRadius"&&InterdictionAssistanceBlocked?value*(1+.5*HullFactor("field","signature")):value;
}
public sealed partial class DemoWorld
{
 void EquipAdvancedCruiser(FleetHull h,BatchHull p)
 {
  // Role equipment gets its share of the source hull's shared sockets before guns.
  Player.Defense=Enumerable.Range(0,12).Select(_=>new DefenseState()).ToArray();Player.Ewar=[new(),new(),new(),new()];
  bool EW(EwarKind kind){foreach(var d in EwarCatalog.All.Where(d=>d.Kind==kind&&d.Size<=2&&d.Role.Length==0).OrderBy(d=>d.Size==2?0:1).ThenBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=d.Slot=="high"?0:Enumerable.Range(1,3).FirstOrDefault(i=>Player.Ewar[i].Definition==null,-1);if(i<0||EwarFitError(d,i).Length>0)continue;Player.Ewar[i]=EwarState.Create(d.Id);return true;}return false;}
  bool Def(string kind){foreach(var d in DefenseCatalog.All.Where(d=>d.Kind==kind&&d.Size<=2).OrderBy(d=>d.Size==2?0:1).ThenBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=Enumerable.Range(0,12).FirstOrDefault(i=>Ship.DefenseSlot(i)==d.Slot&&Player.Defense[i].Definition==null,-1);if(i>=0&&DefenseFitError(d,i).Length==0){Player.Defense[i]=DefenseState.Create(d.Id);return true;}}return false;}
  void Gun(WeaponDefinition d){int i=Array.FindIndex(Player.Weapons,s=>s.ModuleId.Length==0);if(i<0||FitError(d,i).Length>0)return;var s=WeaponState.Create(d.Id);Player.Weapons[i]=s;var charges=ChargeCatalog.All.Where(c=>c.WeaponKey==ChargeCatalog.Key(d)).ToArray();string damage=p.Effects.FirstOrDefault(e=>e.System=="weapon"&&e.Key=="damage"&&e.Damage.Length>0)?.Damage.Split(',')[0]??"";var c=charges.FirstOrDefault(c=>c.DamageType.ToString()==damage&&c.Payload=="damage")??ChargeCatalog.Default(d);if(c!=null){s.ChargeId=c.Id;s.Loaded=c.Crystal?1:d.Magazine;int q=Math.Max(0,Math.Min(c.Crystal?1:d.Magazine*2,(int)Math.Floor(CargoRoom(c.Id)/ItemVolume(c.Id))));if(q>0)CargoItems[c.Id]=CargoItems.GetValueOrDefault(c.Id)+q;}}
  void Special(string id){var d=SpecialCatalog.ById[id];int i=Array.FindIndex(Player.Special,s=>s.Definition==null);if(i<0||SpecialFitError(d,i).Length>0)return;Player.Special[i]=new(){ModuleId=id};var effective=Player.SpecialHullDefinition(d);if(d.Fuel.Length>0){int q=Math.Min(effective.FuelQuantity*(p.Role=="HeavyInterdictor"?1:3),(int)Math.Floor(CargoRoom(d.Fuel)/ItemVolume(d.Fuel)));if(q>0)CargoItems[d.Fuel]=CargoItems.GetValueOrDefault(d.Fuel)+q;}}
  if(p.Role=="HeavyAssault")Def("AssaultControl");
  if(p.Role=="HeavyInterdictor"){var field=EwarCatalog.All.Single(d=>d.Kind==EwarKind.Field);if(EwarFitError(field,0).Length==0)Player.Ewar[0]=EwarState.Create(field.Id);Special("sp_beacon");}
  if(p.Role=="ForceRecon"){Special("sp_ghost");Special("sp_covert_beacon");}
  if(p.Role is "ForceRecon" or "CombatRecon"){
   var filter=p.Effects.FirstOrDefault(e=>e.System=="ewar"&&e.Key=="strength"&&e.Filter is "Damp" or "ECM" or "Disrupt" or "Paint" or "Web")?.Filter;
   if(filter!=null)EW(filter switch{"Damp"=>EwarKind.Damp,"Disrupt"=>EwarKind.Tracking,"Paint"=>EwarKind.Paint,"Web"=>EwarKind.Web,_=>EwarKind.ECM});
   if(p.Effects.Any(e=>e.Filter=="Neutralizer"))EW(EwarKind.Neutralizer);
   if((p.Effects.Any(e=>e.Filter=="Web")||Player.HullCapability("stasis webifier optimal range increased"))&&filter!="Web")EW(EwarKind.Web);
   if(p.Effects.Any(e=>e.Filter is "Point" or "Scram")||Player.HullCapability("pilot negative security status"))EW(EwarKind.Point);
  }
  if(Player.HullCapability("medium micro jump drive")){var d=MobilityCatalog.All.Single(d=>d.Kind=="MJD"&&d.Size==2);if(MobilityFitError(d,0).Length==0)Player.Mobility[0]=new(){ModuleId=d.Id};}
  var preferred=WeaponCatalog.All.Where(w=>w.Family==p.WeaponFamily&&w.Size==p.WeaponSize).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu);foreach(var first in preferred){int n=Player.WeaponSocketCount;Gun(first);if(Player.WeaponSocketCount>n)break;}
  bool shield=h.Faction is "Amur Cooperative Compact" or "Azadi Confederation" or "Iron Lantern Cooperative"||h.SourceHull is "Laelaps" or "Fiend";
  foreach(var d in MobilityCatalog.All.Where(d=>d.Kind=="MWD"&&d.Size==2).OrderBy(d=>d.Size==2?0:1).ThenBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=Array.FindIndex(Player.Mobility,s=>s.Definition==null);if(MobilityFitError(d,i).Length==0){Player.Mobility[i]=new(){ModuleId=d.Id};break;}}
  Def(shield?"ShieldBoost":"ArmorRepair");
  var guns=WeaponCatalog.All.Where(w=>w.Family==p.WeaponFamily&&w.Size==p.WeaponSize).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu).ToArray();
  foreach(var d in guns){int before=Player.WeaponSocketCount;int n=(int)Player.Stat(d.UsesLauncher?"launcherSlotsLeft":"turretSlotsLeft");for(int i=0;i<n;i++)Gun(d);if(Player.WeaponSocketCount>before)break;}
  if(Player.Weapons.All(s=>s.ModuleId.Length==0||s.Definition.Kind==WeaponKind.Bomb)){
   // Small-gun fallback keeps legacy prototypes useful when their unbonused layout favors another family.
   foreach(var d in WeaponCatalog.All.Where(w=>w.Size==2&&w.Kind is WeaponKind.Turret or WeaponKind.Missile).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu)){int before=Player.WeaponSocketCount;Gun(d);if(Player.WeaponSocketCount>before)break;}
  }
  var drones=DroneCatalog.All.Where(d=>!d.Fighter&&(h.SourceHull=="Bestla"?d.Effect=="Web":d.Kind=="Combat")).Select(d=>(d,n:Math.Min(5,(int)Math.Min(Player.Stat("droneCapacity")/d.Volume,Player.Stat("droneBandwidth")/d.Bandwidth)))).Where(x=>x.n>0).OrderByDescending(x=>x.d.Enhanced(Player).Packet.Total*x.n).FirstOrDefault();
  if(drones.d!=null)for(int i=0;i<drones.n;i++)Player.Drones.Add(DroneUnit.Create(drones.d.Id));RefreshRigDroneHealth();
  RecomputeWeaponFit();Player.Shield=Player.MaxShield;Player.Armor=Player.MaxArmor;Player.Hull=Player.MaxHull;Player.Cap=Player.MaxCap;
 }
}
