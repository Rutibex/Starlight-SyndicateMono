namespace StarlightSyndicateDemo;
public sealed partial class Ship
{
 public float NullifierFactor(string key)=>IsFleetHull?HullFactor("nullifier",key):CombatHulls.Get(CombatHull).Role=="Interceptor"?key=="duration"?2:key=="rangePenalty"||key=="reactivation"?.25f:1:1;
 public float LightCombatFactor(DroneDefinition d,string key)=>!d.Fighter&&d.Kind=="Combat"&&d.Volume<=5?HullFactor("drone",key,"LightCombat")/HullFactor("drone",key):1;
 public string CombatBooster{get;set;}="";public float BoosterTime{get;set;}
 public float BoosterWeapon(string key)=>BoosterTime>0&&CombatBooster=="booster25_focus"&&key=="damage"?1.1f:1;
 public double BoosterStat(string key,double value)=>BoosterTime>0?value*(CombatBooster=="booster25_surge"&&key=="maxVelocity"?1.1:CombatBooster=="booster25_focus"&&key=="rechargeRate"?1+.1*HullFactor("booster","sideEffect"):CombatBooster=="booster25_surge"&&key=="signatureRadius"?1+.1*HullFactor("booster","sideEffect"):1):value;
}
public sealed partial class DemoWorld
{
 public bool ConsumeCombatBooster(string id)
 {
  if(id is not ("booster25_focus" or "booster25_surge")||Player.BoosterTime>0||Cloaked||ItemCount(id,Docked)<1)return false;
  ChangeStack(id,Docked,-(decimal)(1));Player.CombatBooster=id;Player.BoosterTime=300;Say("Combat booster active for 300 seconds; adverse effect scaled by hull tolerance.");return true;
 }
 void EquipAdvancedFrigate(FleetHull h,BatchHull p)
 {
  // Role equipment gets its share of the source hull's shared sockets before guns.
  Player.Defense=Enumerable.Range(0,12).Select(_=>new DefenseState()).ToArray();Player.Ewar=[new(),new(),new(),new()];
  bool EW(EwarKind kind){foreach(var d in EwarCatalog.All.Where(d=>d.Kind==kind&&d.Size<=1&&d.Role.Length==0).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=d.Slot=="high"?0:Enumerable.Range(1,3).FirstOrDefault(i=>Player.Ewar[i].Definition==null,-1);if(i<0||EwarFitError(d,i).Length>0)continue;Player.Ewar[i]=EwarState.Create(d.Id);return true;}return false;}
  bool Def(string kind){foreach(var d in DefenseCatalog.All.Where(d=>d.Kind==kind&&d.Size<=1).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=Enumerable.Range(0,12).FirstOrDefault(i=>Ship.DefenseSlot(i)==d.Slot&&Player.Defense[i].Definition==null,-1);if(i>=0&&DefenseFitError(d,i).Length==0){Player.Defense[i]=DefenseState.Create(d.Id);return true;}}return false;}
  void Gun(WeaponDefinition d){int i=Array.FindIndex(Player.Weapons,s=>s.ModuleId.Length==0);if(i<0||FitError(d,i).Length>0)return;var s=WeaponState.Create(d.Id);Player.Weapons[i]=s;var charges=ChargeCatalog.All.Where(c=>c.WeaponKey==ChargeCatalog.Key(d)).ToArray();string damage=p.Effects.FirstOrDefault(e=>e.System=="weapon"&&e.Key=="damage"&&e.Filter==(d.Kind==WeaponKind.Bomb?"Bomb":"Torpedo")&&e.Damage.Length>0)?.Damage.Split(',')[0]??"";var c=charges.FirstOrDefault(c=>c.DamageType.ToString()==damage&&c.Payload=="damage")??ChargeCatalog.Default(d);if(c!=null){s.ChargeId=c.Id;s.Loaded=c.Crystal?1:d.Magazine;int q=c.Crystal?1:Math.Min(d.Magazine*2,(int)Math.Floor(CargoRoom(c.Id)/ItemVolume(c.Id)));if(q>0)CargoItems[c.Id]=CargoItems.GetValueOrDefault(c.Id)+q;}}
  if(p.Role=="Bomber"){var cloak=SpecialCatalog.ById["sp_ghost"];if(SpecialFitError(cloak,0).Length==0)Player.Special[0]=new(){ModuleId=cloak.Id};foreach(var d in WeaponCatalog.All.Where(w=>w.Kind==WeaponKind.Bomb))Gun(d);}
  if(p.Role=="Assault")Def("AssaultControl");
  if(p.Role=="Interceptor"){if(p.Effects.Any(e=>e.System=="nullifier"))Def("Nullifier");EW(EwarKind.Point);}
  if(p.Role=="Electronic warfare"){
   var filter=p.Effects.FirstOrDefault(e=>e.System=="ewar"&&e.Key=="strength"&&e.Filter is "Damp" or "ECM" or "Disrupt" or "Paint")?.Filter;
   EW(filter switch{"Damp"=>EwarKind.Damp,"Disrupt"=>EwarKind.Tracking,"Paint"=>EwarKind.Paint,_=>EwarKind.ECM});
   if(p.Effects.Any(e=>e.Filter=="Neutralizer"))EW(EwarKind.Neutralizer);
   if(p.Effects.Any(e=>e.Filter=="Web"))EW(EwarKind.Web);else if(p.Effects.Any(e=>e.Filter=="Point"))EW(EwarKind.Point);
  }else if(p.Effects.Any(e=>e.Filter=="Neutralizer"))EW(EwarKind.Neutralizer);
  var preferred=WeaponCatalog.All.Where(w=>w.Family==p.WeaponFamily&&w.Size==p.WeaponSize).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu);foreach(var first in preferred){int n=Player.WeaponSocketCount;Gun(first);if(Player.WeaponSocketCount>n)break;}
  bool shield=h.Faction is "Amur Cooperative Compact" or "Iron Lantern Cooperative"||h.SourceHull is "Jaguar" or "Geri";
  foreach(var d in MobilityCatalog.All.Where(d=>d.Kind=="MWD"&&d.Size==1).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=Array.FindIndex(Player.Mobility,s=>s.Definition==null);if(MobilityFitError(d,i).Length==0){Player.Mobility[i]=new(){ModuleId=d.Id};break;}}
  Def(shield?"ShieldBoost":"ArmorRepair");
  var guns=WeaponCatalog.All.Where(w=>w.Family==p.WeaponFamily&&w.Size==p.WeaponSize).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu).ToArray();
  foreach(var d in guns){int before=Player.WeaponSocketCount;int n=(int)h.Value(d.UsesLauncher?"launcherSlotsLeft":"turretSlotsLeft");for(int i=0;i<n;i++)Gun(d);if(Player.WeaponSocketCount>before)break;}
  if(Player.Weapons.All(s=>s.ModuleId.Length==0||s.Definition.Kind==WeaponKind.Bomb)){
   // Small-gun fallback keeps legacy prototypes useful when their unbonused layout favors another family.
   foreach(var d in WeaponCatalog.All.Where(w=>w.Size==1&&w.Kind is WeaponKind.Turret or WeaponKind.Missile).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu)){int before=Player.WeaponSocketCount;Gun(d);if(Player.WeaponSocketCount>before)break;}
  }
  var drones=DroneCatalog.All.Where(d=>!d.Fighter&&d.Volume<=5&&(h.SourceHull=="Geri"?d.Effect=="Web":d.Kind=="Combat")).Select(d=>(d,n:Math.Min(5,(int)Math.Min(Player.Stat("droneCapacity")/d.Volume,Player.Stat("droneBandwidth")/d.Bandwidth)))).Where(x=>x.n>0).OrderByDescending(x=>x.d.Packet.Total*x.n).FirstOrDefault();
  if(drones.d!=null)for(int i=0;i<drones.n;i++)Player.Drones.Add(DroneUnit.Create(drones.d.Id));RefreshRigDroneHealth();
  RecomputeWeaponFit();Player.Shield=Player.MaxShield;Player.Armor=Player.MaxArmor;Player.Hull=Player.MaxHull;Player.Cap=Player.MaxCap;
 }
}
