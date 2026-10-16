namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public IEnumerable<StarSystem> BeaconDestinations()=>ImportedWorld43?Galaxy42.Data.Systems.Where(s=>s.Security<.45&&s.Id>=30000000&&s.Id<31000000&&s.WormholeClass==null&&s.RegionId!=10000070&&"system:"+s.Id!=SystemId).OrderBy(s=>BeaconDistance(SystemId,"system:"+s.Id)).Take(8).Select(s=>ResolveSystem43("system:"+s.Id)):Systems.Where(s=>s.Id!=SystemId&&s.Security<.5);
 public IEnumerable<(string Id,string Name)> IndustryLocations()=>ImportedWorld43?Stations44.Stations.Where(s=>s.System==SystemId&&(s.Services.Contains(14)||s.Services.Contains(15))).Take(9).Select(s=>(s.Id,s.Name)):Systems.Select(s=>(s.Id,s.Name));
 public string RecoveryStationId="";
 public DateTime SimulationUtc=DateTime.UtcNow;
 public DateTime Now=>SimulationUtc;
 private double SkillSyncElapsed;
 public bool SimulationPaused;
 public float SimulationSpeed=1;
 public void CycleSimulationSpeed(){SimulationSpeed=SimulationSpeed==1?5:SimulationSpeed==5?20:1;}
 public HashSet<string> SeededMarkets=new();
 public string LocationSystem(string id)=>Stations44.ById.TryGetValue(id,out var station)?station.System:id;
 public string LocationName(string id)=>Stations44.ById.TryGetValue(id,out var station)?station.Name:ResolveSystem43(id).Name;
 public string LocationRegion(string id)=>ResolveSystem43(LocationSystem(id)).Region;
 public bool LocationExists(string id)=>ImportedWorld43?Stations44.ById.ContainsKey(id):Systems.Any(s=>s.Id==id);
 public bool LocationService(string id,int service){if(!ImportedWorld43)return true;if(!Stations44.ById.TryGetValue(id,out var station)||!station.Services.Contains(service))return false;var required=Stations44.Required(station,service);return !required.HasValue||EffectiveStanding38(station.Faction)>=required.Value;}
 public bool WaitDocked(double hours){if(SimulationPaused){Say("Resume simulated time before waiting.");return false;}if(!Docked||!double.IsFinite(hours)||hours<=0||hours>24*365)return false;AdvanceHours(hours);float seconds=(float)(hours*3600);UpdateSpecial(seconds);UpdateMobility(seconds);UpdateNaniteRepairs(seconds);Player.BoosterTime=Math.Max(0,Player.BoosterTime-seconds);Player.HullTacticalCooldown=Math.Max(0,Player.HullTacticalCooldown-seconds);foreach(var defense in Player.Defense){defense.ReloadLeft=Math.Max(0,defense.ReloadLeft-seconds);defense.ReactivationLeft=Math.Max(0,defense.ReactivationLeft-seconds);}Say($"Waited {hours:0.##} simulated hours.");return true;}
 public bool LegalBeaconDestination(string destination){if(!ImportedWorld43)return Systems.Any(s=>s.Id==destination&&s.Security<.5f);if(!Spatial43.IsSystem(destination))return false;var system=Galaxy42.ById[int.Parse(destination[7..])];return system.Security<.45&&system.Id>=30000000&&system.Id<31000000&&system.WormholeClass==null&&system.RegionId!=10000070;}
 public bool LegalCloneDestination(string station){if(!ImportedWorld43)return LocationExists(station);if(!Stations44.ById.TryGetValue(station,out var location))return false;var system=Galaxy42.ById[int.Parse(location.System[7..])];return system.RegionId!=10000070&&(system.Id<31000000||system.Id==31000005);}
 public bool SetRecoveryStation(){if(!Docked||!RequireService44(11))return false;RecoveryStationId=StationKey44;Say("Home recovery station set to "+StationName44+".");return true;}
 public void RecoverImported()
 {
  var home=Stations44.ById.GetValueOrDefault(RecoveryStationId)??Stations44.Stations.FirstOrDefault(s=>s.System==SystemId&&LocationService(s.Id,22))??Stations44.Stations.Where(s=>LocationService(s.Id,22)).OrderBy(s=>BeaconDistance(SystemId,s.System)).FirstOrDefault();
  if(home==null){Say("No legal recovery station is available.");return;}
  LeaveSpecialGrid(true);LeaveSupportGrid(true);LeaveMobilityGrid();CargoItems.Clear();LoseMissionCargo38();Player=new Ship{UtilityFit="miner",Missiles=5};Player.SetCombatHull("frigate");Player.Shield=Player.MaxShield;Player.Armor=Player.MaxArmor;Player.Hull=Player.MaxHull;Player.Cap=Player.MaxCap;
  MissilesInFlight.Clear();Parasites.Clear();WeaponVisuals.Clear();EwarEffects.Clear();EwarZones.Clear();Targets.Clear();LockedId=SelectedId="";Transit=null;Autopilot=false;DockPending=false;SystemId=home.System;StationId=home.Id;RecoveryStationId=home.Id;Grid=home.Id;SolarOrigin=home.DockPoint;SolarCelestial=home.Id;SolarGridKey=SolarCatalog.GridKey(SystemId,SolarOrigin);Screen=ViewMode.Station;SyncSkills();EnsureLocationMarket(home.Id);WalletChange(-Math.Min(Balance,150m),"Emergency recovery");AdvanceHours(8);Say("Ship and onboard cargo lost. Emergency starter vessel issued at "+home.Name+". Stored assets remain at their original stations.");
 }
 public void EnsureLocationMarket(string station)
 {
  if(!ImportedWorld43||!LocationExists(station)||!LocationService(station,7)||!SeededMarkets.Add(station))return;
  foreach(var item in ItemCatalog.All.Where(i=>ItemCatalog.IsSupported(i.Id)&&!ProductionCatalogE.Restricted(i.Id)))
  {if(item.Price<=0)continue;int quantity=item.Id.StartsWith("ind_")?1000000:item.Id is "ore" or "deepore" or "moonore" or "gas" or "ice"?10000:item.Id=="ammo"?1000:item.Slot=="charge"?200:12;
   MarketOrders.Add(new(){Item=item.Id,Station=station,Price=item.Price,Remaining=quantity,Expires=Day+90,Created=Day});
   MarketOrders.Add(new(){Item=item.Id,Station=station,Buy=true,Price=Math.Round(item.Price*.78m,2),Remaining=quantity,Expires=Day+90,Created=Day});}
 }
}

public sealed partial class Ship {public string InstanceId {get;set;}=Guid.NewGuid().ToString("N");}
