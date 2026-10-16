namespace StarlightSyndicateDemo;
public sealed partial class Ship
{
 public bool UsesDedicatedHolds=>Specialist||ShipBatch21.ById.TryGetValue(CombatHull,out var cargoHull)&&cargoHull.Batch is 26 or 29;
 public float TacticalModeDelay=>HullFlat("hull","modeDelay")>0?HullFlat("hull","modeDelay"):10;
 public string TacticalModeLabel(string mode)=>ShipBatch21.ById.TryGetValue(CombatHull,out var p)&&p.Batch==26?mode switch{"Primary"=>"Defense","Secondary"=>"Propulsion","Tertiary"=>"Sharpshooter",_=>mode}:mode;
}
public sealed partial class DemoWorld
{
 void EquipAdvancedDestroyer(FleetHull h,BatchHull p)
 {
  // Role equipment gets its share of the source hull's shared sockets before guns.
  Player.Defense=Enumerable.Range(0,12).Select(_=>new DefenseState()).ToArray();Player.Ewar=[new(),new(),new(),new()];
  bool Def(string kind){foreach(var d in DefenseCatalog.All.Where(d=>d.Kind==kind&&d.Size<=1).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=Enumerable.Range(0,12).FirstOrDefault(i=>Ship.DefenseSlot(i)==d.Slot&&Player.Defense[i].Definition==null,-1);if(i>=0&&DefenseFitError(d,i).Length==0){Player.Defense[i]=DefenseState.Create(d.Id);return true;}}return false;}
  void Gun(WeaponDefinition d){int i=Array.FindIndex(Player.Weapons,s=>s.ModuleId.Length==0);if(i<0||FitError(d,i).Length>0)return;var s=WeaponState.Create(d.Id);Player.Weapons[i]=s;var charges=ChargeCatalog.All.Where(c=>c.WeaponKey==ChargeCatalog.Key(d)).ToArray();string damage=p.Effects.FirstOrDefault(e=>e.System=="weapon"&&e.Key=="damage"&&e.Filter==(d.Kind==WeaponKind.Bomb?"Bomb":"Torpedo")&&e.Damage.Length>0)?.Damage.Split(',')[0]??"";var c=charges.FirstOrDefault(c=>c.DamageType.ToString()==damage&&c.Payload=="damage")??ChargeCatalog.Default(d);if(c!=null){s.ChargeId=c.Id;s.Loaded=c.Crystal?1:d.Magazine;int q=c.Crystal?1:Math.Min(d.Magazine*2,(int)Math.Floor(CargoRoom(c.Id)/ItemVolume(c.Id)));if(q>0)CargoItems[c.Id]=CargoItems.GetValueOrDefault(c.Id)+q;}}
  if(p.Role=="Interdictor"){
   var d=EwarCatalog.All.Single(d=>d.Kind==EwarKind.Sphere);if(EwarFitError(d,0).Length==0){Player.Ewar[0]=EwarState.Create(d.Id);Player.Ewar[0].PayloadId="ewar_probe";Player.Ewar[0].Loaded=d.Magazine;CargoItems["ewar_probe"]=6;}
  }
  if(Player.HullCapability("micro jump field generators")){
   var field=MobilityCatalog.All.Single(d=>d.Kind=="Field"&&d.Role=="CommandDestroyer");if(MobilityFitError(field,0).Length==0)Player.Mobility[0]=new(){ModuleId=field.Id};
   string family=p.Effects.First(e=>e.System=="burst"&&e.Key=="strength").Filter;
   var burst=SupportCatalog.All.Single(d=>d.Kind=="Burst"&&d.Family==family);if(SupportFitError(burst,0).Length==0){Player.Support[0]=SupportState.Create(burst.Id);var payload=SupportPayloads.For(burst)[0];Player.Support[0].PayloadId=payload.Id;Player.Support[0].Loaded=3;CargoItems[payload.Id]=6;}
  }
  var preferred=WeaponCatalog.All.Where(w=>w.Family==p.WeaponFamily&&w.Size==p.WeaponSize).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu);foreach(var first in preferred){int n=Player.WeaponSocketCount;Gun(first);if(Player.WeaponSocketCount>n)break;}
  if(h.SourceHull=="Outrider"){
   foreach(var d in UtilityCatalog.All.Where(d=>d.Kind=="Miner").OrderBy(d=>d.Power)){if(UtilityFitError(d,0).Length==0){Player.Utilities[0]=new(){ModuleId=d.Id};break;}}
   var tractor=UtilityCatalog.All.First(d=>d.Kind=="Tractor"&&d.Size==1);if(UtilityFitError(tractor,1).Length==0)Player.Utilities[1]=new(){ModuleId=tractor.Id};
  }
  bool shield=h.Faction is "Amur Cooperative Compact" or "Azadi Confederation" or "Kepler Resource Group";
  foreach(var d in MobilityCatalog.All.Where(d=>d.Kind=="MWD"&&d.Size==1).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int i=Array.FindIndex(Player.Mobility,s=>s.Definition==null);if(MobilityFitError(d,i).Length==0){Player.Mobility[i]=new(){ModuleId=d.Id};break;}}
  Def(shield?"ShieldBoost":"ArmorRepair");
  var guns=WeaponCatalog.All.Where(w=>w.Family==p.WeaponFamily&&w.Size==p.WeaponSize).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu).ToArray();
  foreach(var d in guns){int before=Player.WeaponSocketCount;int n=(int)h.Value(d.UsesLauncher?"launcherSlotsLeft":"turretSlotsLeft");for(int i=0;i<n;i++)Gun(d);if(Player.WeaponSocketCount>before)break;}
  if(p.Role=="TacticalDestroyer"){
   foreach(var d in UtilityCatalog.All.Where(d=>d.Kind=="CoreProbe").OrderBy(d=>d.Cpu)){if(UtilityFitError(d,0).Length==0){Player.Utilities[0]=new(){ModuleId=d.Id};Player.Utilities[0].PayloadId="ut_probe_core";Player.Utilities[0].Loaded=8;CargoItems["ut_probe_core"]=8;break;}}
  }
  var drones=DroneCatalog.All.Where(d=>!d.Fighter&&d.Volume<=5&&d.Kind=="Combat").Select(d=>(d,n:Math.Min(5,(int)Math.Min(Player.Stat("droneCapacity")/d.Volume,Player.Stat("droneBandwidth")/d.Bandwidth)))).Where(x=>x.n>0).OrderByDescending(x=>x.d.Packet.Total*x.n).FirstOrDefault();
  if(drones.d!=null)for(int i=0;i<drones.n;i++)Player.Drones.Add(DroneUnit.Create(drones.d.Id));RefreshRigDroneHealth();
  RecomputeWeaponFit();Player.Shield=Player.MaxShield;Player.Armor=Player.MaxArmor;Player.Hull=Player.MaxHull;Player.Cap=Player.MaxCap;
 }
}
