namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public List<ParkedShip> FleetBerths{get;set;}=[];
 public List<string> GrantedFleetHulls{get;set;}=[];
 public bool BoardFleetHull(string id)
 {
  if(PlayerPackagesVolumeD>0){Say("Deliver or break the sealed package before switching ships.");return false;}
  if(!Docked){Say("Dock before boarding a ship.");return false;}
  if(!FleetCatalog.ById.TryGetValue(id,out var h)||!h.Playable)return false;
  if(!SkillAllowed(id))return false;
  if(Player.CombatHull==id&&!Commander.Capsule)return true;
  var stored=FleetBerths.FirstOrDefault(b=>b.System==StationKey44&&b.Ship.CombatHull==id);
  if(ImportedWorld43&&stored==null){Say("Bring an assembled hull to this station before boarding.");return false;}
  if(stored==null&&GrantedFleetHulls.Contains(id)){Say("That ship is berthed at another station.");return false;}
  var profile=ShipBatch21.ById[id];float price=profile.Starter||profile.LegacyCharter?0:(float)profile.Price;
  if(stored==null&&price>0&&SystemId!=profile.HomeSystem){Say("Available at "+profile.HomeSystem+" station / "+price+" CR including basic fit.");return false;}
  if(stored==null&&Credits<price){Say("Insufficient credits for this fitted hull.");return false;}
  var proficiency=new Dictionary<int,int>(Player.HullProficiency);float security=Player.PilotSecurity;
  if(stored==null&&price>0)WalletChange(-(decimal)price,"Fitted hull purchase / "+h.Name);
  // Keep the complete old ship: fittings, magazines, rigs, damage, drones and cargo.
  FleetBerths.Add(new(){System=StationKey44,Ship=Player,Cargo=new(CargoItems)});
  if(stored!=null){FleetBerths.Remove(stored);Player=stored.Ship;CargoItems=new(stored.Cargo);}
  else
  {
   Player=new Ship();Player.HullProficiency=proficiency;Player.SetCombatHull(id);CargoItems=[];GrantedFleetHulls.Add(id);WeaponTestMode=false;
   EquipFleetStarter(h);
  }
  Commander.Capsule=false;SyncSkills();Player.PilotSecurity=security;FittingWeaponSlot=0;Player.Position=Microsoft.Xna.Framework.Vector3.Zero;Player.Velocity=Microsoft.Xna.Framework.Vector3.Zero;Player.Order="stop";
  RecomputeWeaponFit();SelectedId=LockedId=PendingLockId="";LockTimeLeft=0;Say("Boarded "+h.Name+". Previous fitted ship remains in this station's berth.");return true;
 }
 public bool BuyAdditionalFleetHull(string id)
 {
  if(!Docked||!ShipBatch21.ById.TryGetValue(id,out var profile)||SystemId!=profile.HomeSystem||Credits<(float)profile.Price||profile.Starter)return false;
  var factory=new DemoWorld{Screen=ViewMode.Station,SystemId=SystemId};factory.Player=new Ship{HullProficiency=new(Player.HullProficiency)};factory.Player.SetCombatHull(id);factory.EquipFleetStarter(FleetCatalog.ById[id]);
  WalletChange(-profile.Price,"Additional fitted hull / "+factory.Player.HullName);FleetBerths.Add(new(){System=StationKey44,Ship=factory.Player,Cargo=new(factory.CargoItems)});if(!GrantedFleetHulls.Contains(id))GrantedFleetHulls.Add(id);Say("Purchased fitted "+factory.Player.HullName+"; it is in this station's local berths.");return true;
 }
 public bool BoardFleetBerth(int index)
 {if(PlayerPackagesVolumeD>0)return false;if(!RequireService44(17))return false;
  if(!Docked||index<0||index>=FleetBerths.Count||FleetBerths[index].System!=StationKey44)return false;
  var stored=FleetBerths[index];if(!SkillAllowed(stored.Ship.CombatHull))return false;FleetBerths.RemoveAt(index);FleetBerths.Add(new(){System=StationKey44,Ship=Player,Cargo=new(CargoItems)});
  Player=stored.Ship;Commander.Capsule=false;SyncSkills();CargoItems=new(stored.Cargo);FittingWeaponSlot=0;SelectedId=LockedId=PendingLockId="";RecomputeWeaponFit();Say("Boarded berthed "+Player.HullName+".");return true;
 }
 void EquipFleetStarter(FleetHull h)
 {
  var profile=ShipBatch21.ById[h.Id];
  if(profile.Batch==29){EquipEliteBattleship(h,profile);return;}
  if(profile.Batch==28){EquipStrategicCommand(h,profile);return;}
  if(profile.Batch==27){EquipAdvancedCruiser(h,profile);return;}
  if(profile.Batch==26){EquipAdvancedDestroyer(h,profile);return;}
  if(profile.Batch==25){EquipAdvancedFrigate(h,profile);return;}
  if(profile.Batch==24){EquipSpecialistStarter(h,profile);return;}
  Player.Defense=Enumerable.Range(0,12).Select(_=>new DefenseState()).ToArray();Player.Ewar=[new(),new(),new(),new()];
  // Fit mission equipment first so combat guns cannot consume its required sockets or budget.
  if(profile.Starter){foreach(var miner in UtilityCatalog.All.Where(d=>d.Kind=="Miner"&&d.Size<=1).OrderBy(d=>d.Power)){if(UtilityFitError(miner,0).Length==0){Player.Utilities[0]=new(){ModuleId=miner.Id};break;}}}
  if(profile.Role=="Exploration")
  {
   foreach(string kind in new[]{"CoreProbe","Data","Relic"})
   {
    var candidates=UtilityCatalog.All.Where(d=>d.Kind==kind||(kind=="Data"&&d.Kind=="DataAnalyzer")||(kind=="Relic"&&d.Kind=="RelicAnalyzer")).OrderBy(d=>d.Cpu);
    foreach(var d in candidates){int slot=Enumerable.Range(0,16).FirstOrDefault(i=>Ship.UtilitySlot(i)==d.Slot&&Player.Utilities[i].Definition==null,-1);if(slot<0||UtilityFitError(d,slot).Length>0)continue;Player.Utilities[slot]=new(){ModuleId=d.Id};if(kind=="CoreProbe"){var payload=UtilityPayloads.All.First(x=>x.Kind=="Core");Player.Utilities[slot].PayloadId=payload.Id;Player.Utilities[slot].Loaded=8;CargoItems[payload.Id]=8;}break;}
   }
  }
  if(profile.Role=="Logistics")
  {
   string kind=profile.Effects.Any(e=>e.System=="support"&&e.Filter=="Armor")?"Armor":"Shield";
   var d=SupportCatalog.All.Where(d=>d.Size==(profile.Batch==23?3:Math.Min(2,profile.WeaponSize))&&d.Kind==kind).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu).First();
   for(int i=0;i<2;i++)if(SupportFitError(d,i).Length==0)Player.Support[i]=SupportState.Create(d.Id);
  }
  if(profile.Role=="Electronic warfare")
  {
   string f=profile.Effects.First(e=>e.System=="ewar"&&e.Key=="strength").Filter;var kind=f switch{"Disrupt"=>EwarKind.Tracking,"Damp"=>EwarKind.Damp,"Paint"=>EwarKind.Paint,_=>EwarKind.ECM};
   foreach(var d in EwarCatalog.All.Where(d=>d.Kind==kind&&d.Size<=1&&d.Role.Length==0).OrderBy(d=>d.Cpu)){if(EwarFitError(d,1).Length==0){Player.Ewar[1]=EwarState.Create(d.Id);break;}}
  }
  if(profile.Batch==22&&Player.HullFlat("hull","commandSlots")>0){foreach(var burst in SupportCatalog.All.Where(d=>d.Kind=="Burst").OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Array.FindIndex(Player.Support,x=>x.Definition==null);if(SupportFitError(burst,slot).Length>0)continue;Player.Support[slot]=SupportState.Create(burst.Id);var payload=SupportPayloads.For(burst).First();Player.Support[slot].PayloadId=payload.Id;Player.Support[slot].Loaded=Math.Max(1,(int)(burst.MagazineVolume/payload.Volume));CargoItems[payload.Id]=10;break;}}
  if(profile.Batch==22&&profile.Effects.Any(e=>e.System=="support"&&e.Filter=="Cap")){var cap=SupportCatalog.All.Where(d=>d.Kind=="Cap"&&d.Size==2).OrderBy(d=>d.Power).First();int slot=Array.FindIndex(Player.Support,s=>s.Definition==null);if(SupportFitError(cap,slot).Length==0)Player.Support[slot]=SupportState.Create(cap.Id);}
  if(profile.Batch==23&&profile.Effects.Any(e=>e.System=="ewar"&&e.Filter=="Neutralizer")){foreach(var energy in EwarCatalog.All.Where(d=>d.Kind==EwarKind.Neutralizer&&d.Size==3&&d.Role.Length==0).OrderBy(d=>d.Power)){if(EwarFitError(energy,0).Length==0){Player.Ewar[0]=EwarState.Create(energy.Id);break;}}}
  string tank=h.Faction is "Suryavarta Empire" or "Azadi Confederation"||h.Faction.Contains("Sury")||h.Faction.Contains("Azadi")?"ArmorRepair":"ShieldBoost";
  foreach(var d in DefenseCatalog.All.Where(d=>d.Size==(profile.Batch==23?3:Math.Min(2,profile.WeaponSize))&&d.Kind==tank).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu))
  {int slot=Enumerable.Range(0,Player.Defense.Length).First(i=>Ship.DefenseSlot(i)==d.Slot);if(DefenseFitError(d,slot).Length==0){Player.Defense[slot]=DefenseState.Create(d.Id);break;}}
  if(profile.Batch==23){foreach(var drive in MobilityCatalog.All.Where(d=>!profile.LegacyCharter&&d.Kind=="AB"&&d.Size==(profile.Batch==23?3:Math.Min(2,profile.WeaponSize))).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Enumerable.Range(0,Player.Mobility.Length).FirstOrDefault(i=>Ship.MobilitySlot(i)==drive.Slot&&Player.Mobility[i].Definition==null,-1);if(slot>=0&&MobilityFitError(drive,slot).Length==0){Player.Mobility[slot]=new(){ModuleId=drive.Id};break;}}}
  var gun=WeaponCatalog.All.Where(w=>w.Size==profile.WeaponSize&&w.Family==profile.WeaponFamily).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu).First();
  int count=(int)h.Value(gun.UsesLauncher?"launcherSlotsLeft":"turretSlotsLeft");
  for(int i=0;i<Math.Min(count,Player.Weapons.Length);i++)
  {
   if(FitError(gun,i).Length>0)break;Player.Weapons[i]=WeaponState.Create(gun.Id);var c=ChargeCatalog.Default(gun);
   if(c!=null){Player.Weapons[i].Loaded=c.Crystal?1:gun.Magazine;CargoItems[c.Id]=CargoItems.GetValueOrDefault(c.Id)+(c.Crystal?1:gun.Magazine*2);}
  }
  if(profile.Batch!=23){foreach(var drive in MobilityCatalog.All.Where(d=>!profile.LegacyCharter&&d.Kind=="AB"&&d.Size==(profile.Batch==23?3:Math.Min(2,profile.WeaponSize))).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Enumerable.Range(0,Player.Mobility.Length).FirstOrDefault(i=>Ship.MobilitySlot(i)==drive.Slot&&Player.Mobility[i].Definition==null,-1);if(slot>=0&&MobilityFitError(drive,slot).Length==0){Player.Mobility[slot]=new(){ModuleId=drive.Id};break;}}}
  if(profile.Batch==23){foreach(string passive in new[]{tank=="ArmorRepair"?"Plate":"Extender","Bulkhead"}){foreach(var d in DefenseCatalog.All.Where(d=>d.Kind==passive&&(d.Size==3||d.Size==0)).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Enumerable.Range(0,Player.Defense.Length).FirstOrDefault(i=>Ship.DefenseSlot(i)==d.Slot&&Player.Defense[i].Definition==null,-1);if(slot>=0&&DefenseFitError(d,slot).Length==0){Player.Defense[slot]=DefenseState.Create(d.Id);break;}}}}
  if(profile.Batch==21){var drone=DroneCatalog.All.First(d=>!d.Fighter&&d.Kind=="Combat"&&d.Volume<=5&&d.Bandwidth<=5);int countDrones=Math.Min(5,(int)Math.Min(h.Value("droneCapacity")/drone.Volume,h.Value("droneBandwidth")/drone.Bandwidth));for(int i=0;i<countDrones;i++)Player.Drones.Add(DroneUnit.Create(drone.Id));RefreshRigDroneHealth();}
  else{
  var drones=DroneCatalog.All.Where(d=>!d.Fighter&&d.Kind=="Combat"&&(profile.Batch>=22||d.Volume<=5)).Select(d=>(d,count:Math.Min(5,(int)Math.Min(h.Value("droneCapacity")/d.Volume,h.Value("droneBandwidth")/d.Bandwidth)))).Where(p=>p.count>0).OrderByDescending(p=>p.count*p.d.Packet.Total/p.d.Cycle).FirstOrDefault();
  if(drones.d!=null)for(int i=0;i<drones.count;i++)Player.Drones.Add(DroneUnit.Create(drones.d.Id));RefreshRigDroneHealth();
  }
  RecomputeWeaponFit();Player.Shield=Player.MaxShield;Player.Armor=Player.MaxArmor;Player.Hull=Player.MaxHull;Player.Cap=Player.MaxCap;
 }
 public bool UnfitFleetWeapon(int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||!Player.IsFleetHull||slot<0||slot>=Player.Weapons.Length)return false;var s=Player.WeaponAt(slot);if(s==null)return false;
  ReturnMagazine(s,true);ChangeStack(s.ModuleId,true,(decimal)(1));Player.Weapons[slot]=new(){ModuleId=""};if(slot==1)Player.UtilityFit="";RecomputeWeaponFit();return true;
 }
 public void ToggleFleetRack()
 {
  if(!Player.IsFleetHull)return;
  bool stop=Player.Weapons.Any(w=>w.Active);
  for(int i=0;i<Player.Weapons.Length;i++){var w=Player.WeaponAt(i);if(w==null)continue;if(stop)w.Stop();else if(!w.Active)ToggleWeapon(i);}
 }
}
