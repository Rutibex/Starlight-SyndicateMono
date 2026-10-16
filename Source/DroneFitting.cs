using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public string DroneSelection{get;set;}="";public string DroneGroupFilter{get;set;}="All";public int SelectedFighterTube{get;set;}
 public int ActiveDroneCount=>Player.Drones.Count(u=>u.Owner=="player"&&u.Location=="Space"&&!u.Definition.Fighter);
 public float UsedDroneBandwidth=>Player.Drones.Where(u=>u.Owner=="player"&&u.Location=="Space"&&!u.Definition.Fighter).Sum(u=>u.Definition.Bandwidth);
 public float ReservedDroneVolume(bool fighter)=>Player.Drones.Where(u=>u.Owner=="player"&&u.Definition.Fighter==fighter&&u.Location is "Bay" or "Space" or "Reserve").Sum(u=>u.Count*u.Definition.Volume);
 List<DroneIntegrity> DroneIntegrities(DroneUnit u)=>new[]{new DroneIntegrity{Shield=u.Shield,Armor=u.Armor,Hull=u.Hull}}.Concat(u.ReserveHealth).Take(u.Count).ToList();
 void SetDroneIntegrities(DroneUnit u,List<DroneIntegrity> health){u.Count=health.Count;if(health.Count==0)return;u.Shield=health[0].Shield;u.Armor=health[0].Armor;u.Hull=health[0].Hull;u.ReserveHealth=health.Skip(1).ToList();}
 bool FighterRoleAllowed(DroneDefinition d)=>CombatHulls.Get(Player.CombatHull).Role is "Carrier" or "Supercarrier"&&(d.Role!="Supercarrier"||CombatHulls.Get(Player.CombatHull).Role=="Supercarrier");
 public bool LoadDrone(string type,bool fromHangar,int count=1,int tube=-1)
 {
  if(!Docked||count<1||!DroneCatalog.ById.TryGetValue(type,out var d)||ItemCount(type,fromHangar)<count)return false;
  if(d.Role=="CapitalIndustrial"&&CombatHulls.Get(Player.CombatHull).Role!="CapitalIndustrial"){Say("Requires capital industrial hull.");return false;}
  if(d.Fighter&&( !FighterRoleAllowed(d)||tube>=Player.Stat("fighterTubes"))){Say("Fighter role incompatible with hull.");return false;}if(tube<0)tube=SelectedFighterTube;
  if(ReservedDroneVolume(d.Fighter)+count*d.Volume>Player.Stat(d.Fighter?"fighterCapacity":"droneCapacity")){Say("Bay volume exceeded.");return false;}
  var loaded=d.Fighter?Player.Drones.FirstOrDefault(u=>u.Owner=="player"&&u.Tube==tube&&u.Location is "Bay" or "Space"):null;
  if(d.Fighter&&(tube>=Player.Stat("fighterTubes")||loaded?.Location=="Space"||loaded!=null&&loaded.TypeId!=type||(loaded?.Count??0)+count>d.SquadronSize)){Say("Tube incompatible, deployed or full.");return false;}
  RefreshEnhancedFighters(true);ChangeStack(type,fromHangar,-(decimal)(count));var health=new List<DroneIntegrity>();int left=count;
  while(left>0){var old=fromHangar?Player.Drones.FirstOrDefault(u=>u.TypeId==type&&u.Location=="Hangar"&&u.System==StationKey44):null;int take=Math.Min(left,old?.Count??left);health.AddRange(old==null?Enumerable.Range(0,take).Select(_=>new DroneIntegrity{Shield=d.Shield,Armor=d.Armor,Hull=d.Hull}):DroneIntegrities(old).Take(take));if(old!=null){var rest=DroneIntegrities(old).Skip(take).ToList();if(rest.Count==0)Player.Drones.Remove(old);else SetDroneIntegrities(old,rest);}left-=take;}
  if(d.Fighter){var u=loaded??DroneUnit.Create(type,0,tube);if(loaded==null)Player.Drones.Add(u);SetDroneIntegrities(u,(loaded==null?new List<DroneIntegrity>():DroneIntegrities(u)).Concat(health).ToList());DroneSelection=u.Id;}
  else foreach(var h in health){var u=DroneUnit.Create(type);u.Shield=h.Shield;u.Armor=h.Armor;u.Hull=h.Hull;Player.Drones.Add(u);DroneSelection=u.Id;}
  RefreshRigDroneHealth();RefreshEnhancedFighters();Say($"Loaded {count} {d.Name}.");return true;
 }
 public bool UnloadDrone(string id)
 {
  var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Owner=="player"&&u.Location is "Bay" or "Reserve");if(u==null)return false;
  if(!Docked){if(!u.Definition.Fighter||u.Location!="Bay")return false;u.Location="Reserve";u.Tube=-1;u.Order="Idle";return true;}
  NormalizeRigDrone(u);ChangeStack(u.TypeId,true,(decimal)(u.Count));u.Location="Hangar";u.System=StationKey44;u.Tube=-1;u.Order="Idle";RefreshEnhancedFighters();return true;
 }
 public bool StoreFighterReserve(string type,int count)
 {
  if(!Docked||count<1||!DroneCatalog.ById.TryGetValue(type,out var d)||!d.Fighter||!FighterRoleAllowed(d)||ItemCount(type,true)<count||ReservedDroneVolume(true)+count*d.Volume>Player.Stat("fighterCapacity"))return false;
  // Loading via a temporary free tube would incorrectly require spare tubes; transfer integrity directly.
  RefreshEnhancedFighters(true);ChangeStack(type,true,-(decimal)(count));var u=DroneUnit.Create(type,0);u.Location="Reserve";var health=new List<DroneIntegrity>();int left=count;
  while(left>0){var old=Player.Drones.FirstOrDefault(x=>x.TypeId==type&&x.Location=="Hangar"&&x.System==StationKey44);int take=Math.Min(left,old?.Count??left);health.AddRange(old==null?Enumerable.Range(0,take).Select(_=>new DroneIntegrity{Shield=d.Shield,Armor=d.Armor,Hull=d.Hull}):DroneIntegrities(old).Take(take));if(old!=null){var remaining=DroneIntegrities(old).Skip(take).ToList();if(remaining.Count==0)Player.Drones.Remove(old);else SetDroneIntegrities(old,remaining);}left-=take;}SetDroneIntegrities(u,health);Player.Drones.Add(u);DroneSelection=u.Id;RefreshEnhancedFighters();Say("Spare fighters stored aboard.");return true;
 }
 public bool ReplenishFighter(string id,int tube=-1)
 {
  if(tube<0)tube=SelectedFighterTube;var r=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Location=="Reserve"&&u.Owner=="player");if(r==null||!FighterRoleAllowed(r.Definition)||tube<0||tube>=Player.Stat("fighterTubes"))return false;
  var loaded=Player.Drones.FirstOrDefault(u=>u.Owner=="player"&&u.Tube==tube&&u.Location is "Bay" or "Space");if(loaded?.Location=="Space"||loaded!=null&&loaded.TypeId!=r.TypeId)return false;int take=Math.Min(r.Count,r.Definition.SquadronSize-(loaded?.Count??0));if(take<=0)return false;
  RefreshEnhancedFighters(true);var spare=DroneIntegrities(r);var health=(loaded==null?new List<DroneIntegrity>():DroneIntegrities(loaded)).Concat(spare.Take(take)).ToList();if(loaded==null){loaded=DroneUnit.Create(r.TypeId,0,tube);Player.Drones.Add(loaded);}SetDroneIntegrities(loaded,health);if(take==r.Count)Player.Drones.Remove(r);else SetDroneIntegrities(r,spare.Skip(take).ToList());DroneSelection=loaded.Id;RefreshEnhancedFighters();return true;
 }
 public string DroneLaunchError(DroneUnit u)
 {
  if(Docked||u.Location!="Bay")return "Launch requires a ready bay craft in space.";var d=u.Definition;
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(!d.Fighter)return ActiveDroneCount>=Player.Stat("maxActiveDrones")?"Control count exceeded.":UsedDroneBandwidth+d.Bandwidth>Player.Stat("droneBandwidth")?"Bandwidth exceeded.":"";
  if(!FighterRoleAllowed(d))return "Wrong fighter hull role.";if(u.Tube<0||u.Tube>=Player.Stat("fighterTubes")||Player.Drones.Any(x=>x.Id!=u.Id&&x.Owner=="player"&&x.Tube==u.Tube&&x.Location is "Bay" or "Space"))return "Launch tube already occupied or invalid.";int active=Player.Drones.Count(x=>x.Owner=="player"&&x.Location=="Space"&&x.Definition.Fighter);if(active>=Player.Stat("fighterTubes"))return "No launch tubes.";
  string key=d.Kind=="Support"?"fighterSupportSlots":d.Role=="Supercarrier"?"fighterHeavySlots":"fighterLightSlots";if(Player.Drones.Count(x=>x.Owner=="player"&&x.Location=="Space"&&x.Definition.Fighter&&(x.Definition.Kind=="Support"?"fighterSupportSlots":x.Definition.Role=="Supercarrier"?"fighterHeavySlots":"fighterLightSlots")==key)>=Player.Stat(key))return "Category launch limit exceeded.";return "";
 }
 public bool LaunchDrone(string id)
 {
  if(Cloaked||Player.HasSpecial("Triage")||Player.NexusLeft>0)return false;
  var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Owner=="player");if(u==null)return false;string error=DroneLaunchError(u);if(error.Length>0){Say(error);return false;}RefreshEnhancedFighters();u.Location="Space";u.System=SystemId;u.Grid=DroneGridIdentity;u.Position=Player.Position+new Vector3(100+ActiveDroneCount*40,0,150);u.Order="Orbit";u.RearmLeft=u.Definition.Fighter?2:0;Targets.Add(DroneBody(u));DroneSelection=u.Id;return true;
 }
 SpaceTarget DroneBody(DroneUnit u)
 {
  var d=EffectiveDrone(u);var p=new Ship();p.NativeSensor=d.Sensor;p.SetStat("shieldCapacity",Math.Max(1,d.Shield));p.SetStat("armorHP",Math.Max(1,d.Armor));p.SetStat("hp",Math.Max(1,d.Hull));p.SetStat("maxTargetRange",240000);p.SetStat("scanResolution",d.ScanResolution);p.SetStat("remoteRepairImpedance",d.Fighter?0:1);p.SetStat("signatureRadius",d.Signature);p.SetStat("maxVelocity",d.Speed);p.SetStat("capacitorCapacity",100);p.SetStat("scan"+d.Sensor+"Strength",d.SensorStrength);foreach(var (k,v) in d.Resonances)if(ShipAttributeCatalog.ByKey.ContainsKey(k))p.SetStat(k,v);return new(){Id=u.Id,DroneId=u.Id,Name=d.Name,Kind=TargetKind.Raider,Position=u.Position,Profile=p,Signature=d.Signature,Shield=u.Shield,Armor=u.Armor,Hull=u.Hull,Training=true,Friendly=u.Owner=="player",Cap=100,LockId=""};
 }
 void SyncDrone(DroneUnit u){var b=Targets.FirstOrDefault(t=>t.Id==u.Id);if(b!=null){u.Position=b.Position;u.Shield=b.Shield;u.Armor=b.Armor;u.Hull=b.Hull;}}
 void LeaveDroneGrid(){foreach(var u in Player.Drones.Where(u=>u.Owner=="player"&&u.Location=="Space")){SyncDrone(u);u.Location="Abandoned";u.Order="Idle";u.TargetId="";}Player.Drones.RemoveAll(u=>u.Owner!="player");}
 void RestoreDroneGrid(){foreach(var u in Player.Drones.Where(u=>u.Location=="Abandoned"&&u.System==SystemId&&u.Grid==DroneGridIdentity))if(!Targets.Any(t=>t.Id==u.Id))Targets.Add(DroneBody(u));}
 public void ReconnectDrones(){if(Docked)return;foreach(var u in Player.Drones.Where(u=>u.Location=="Abandoned"&&u.System==SystemId&&u.Grid==DroneGridIdentity).ToArray()){u.Location="Bay";if(DroneLaunchError(u).Length==0&&ReservedDroneVolume(u.Definition.Fighter)<=Player.Stat(u.Definition.Fighter?"fighterCapacity":"droneCapacity")){u.Location="Space";u.Order="Orbit";}else u.Location="Abandoned";}}
 public bool ScoopDrone(string id){var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Owner=="player"&&u.Location is "Space" or "Abandoned");var b=Targets.FirstOrDefault(t=>t.Id==id);if(u==null||b==null||Vector3.Distance(Player.Position,b.Position)>2500||u.Location=="Abandoned"&&ReservedDroneVolume(u.Definition.Fighter)+u.Count*u.Definition.Volume>Player.Stat(u.Definition.Fighter?"fighterCapacity":"droneCapacity"))return false;SyncDrone(u);if(u.Definition.Fighter&&u.Location=="Abandoned"&&Player.Drones.Any(x=>x.Id!=u.Id&&x.Tube==u.Tube&&x.Location is "Bay" or "Space")){u.Location="Reserve";u.Tube=-1;}else u.Location="Bay";u.Order="Idle";Targets.Remove(b);return true;}
 public bool ScoopDroneCargo(string id){var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Owner=="player"&&u.Location=="Abandoned");var b=Targets.FirstOrDefault(t=>t.Id==id);if(u==null||b==null||Vector3.Distance(Player.Position,b.Position)>2500||CargoUsed+u.Count*u.Definition.Volume>Player.CargoMax)return false;SyncDrone(u);if(u.Armor<u.Definition.Armor||u.Hull<u.Definition.Hull){Say("Recover damaged drones to the drone bay.");return false;}CargoItems[u.TypeId]=CargoItems.GetValueOrDefault(u.TypeId)+u.Count;Player.Drones.Remove(u);Targets.Remove(b);return true;}
 public void RepairBayDrones(){if(!Docked)return;var units=Player.Drones.Where(u=>u.Location is "Bay" or "Reserve").ToArray();float missing=0;foreach(var u in units)foreach(var h in DroneIntegrities(u))missing+=EffectiveDrone(u).Shield-h.Shield+u.Definition.Armor-h.Armor+u.Definition.Hull-h.Hull;int cost=(int)Math.Ceiling(Math.Max(0,missing)*.03);if(Credits<cost)return;WalletChange(-(decimal)(cost),"Gameplay transaction");foreach(var u in units)SetDroneIntegrities(u,Enumerable.Range(0,u.Count).Select(_=>new DroneIntegrity{Shield=EffectiveDrone(u).Shield,Armor=u.Definition.Armor,Hull=u.Definition.Hull}).ToList());Say("Bay repairs / "+cost+" CR.");}
 void ProvisionDroneTrial(){foreach(var u in Player.Drones.Where(u=>u.Owner=="player"&&u.Location is "Bay" or "Reserve").ToArray())UnloadDrone(u.Id);foreach(var d in DroneCatalog.All)Hangar[d.Id]=Math.Max(d.Fighter?d.SquadronSize*5:10,Hangar.GetValueOrDefault(d.Id));}
 public void EnterDroneRange(){EnterWeaponRange();if(Grid!="range")return;Grid="dronerange";Targets.Add(new(){Id="dr_ore",Name="Drone exercise ore",Kind=TargetKind.Asteroid,Ore=50000,Resource="ore",Position=new(1500,0,0)});Targets.Add(new(){Id="dr_ice",Name="Drone exercise ice",Kind=TargetKind.Asteroid,Ore=50000,Resource="ice",Position=new(0,0,1500)});Targets.Add(new(){Id="dr_wreck",Name="Drone exercise wreck",Kind=TargetKind.Wreck,Salvage=4,Position=new(-1300,0,0)});var friend=Targets.First(t=>t.Friendly);friend.Shield*=.4f;friend.Armor*=.4f;friend.Hull*=.4f;var enemy=DroneUnit.Create(DroneCatalog.All.First(d=>d.Kind=="Combat").Id);enemy.Owner="range";enemy.Location="Space";enemy.Position=new(1800,0,-1000);Player.Drones.Add(enemy);Targets.Add(DroneBody(enemy));Say("Drone range: combat, enemy drone, friendly repairs, ore, ice and salvage.");}
}
