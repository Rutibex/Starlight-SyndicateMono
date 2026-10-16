using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
internal static class FleetTests
{
 public static void Run()
 {
  int n=0;void Check(bool x,string message){if(!x)throw new Exception("FLEET FAIL: "+message);n++;Console.WriteLine("FLEET PASS: "+message);}
  Check(FleetCatalog.All.Length==569&&FleetCatalog.ById.Count==569,"569 unique stable hull records");
  Check(FleetCatalog.All.Sum(h=>h.RawFields.Count)==34776,"All 34776 populated source fields retained");
  Check(FleetCatalog.All.Select(h=>h.Faction).Distinct().Count()==22,"All 22 factions and organizations");
  Check(FleetCatalog.All.All(h=>h.Description.Length>0&&h.Quote.Length>0&&FleetCatalog.ById.ContainsKey(h.BaseHullId)),"Lore, quotes and base-hull links cover every entry");
  var w=new DemoWorld();w.NewGame(0);float wallet=w.Credits;
  foreach(var h in FleetCatalog.Playable.Where(h=>ShipBatch21.ById[h.Id].LegacyCharter))
  {
   Check(w.BoardFleetHull(h.Id),"Board "+h.Name);
   foreach(string key in new[]{"mass","hiSlots","medSlots","lowSlots","cpuOutput","powerOutput","droneCapacity","droneBandwidth","rigSize","upgradeSlotsLeft","upgradeCapacity","maxTargetRange","rechargeRate","shieldRechargeRate","shieldRechargeRate"})
    Check(Math.Abs(w.Player.Stat(key)-h.Value(key))<.001,h.Name+" uses source "+key);
   Check(w.Player.Weapons.Length==(int)h.Value("hiSlots"),h.Name+" entire high rack represented");
   Check(w.Player.Weapons.Count(s=>w.Player.WeaponAt(Array.IndexOf(w.Player.Weapons,s))!=null)==(int)h.Value(h.SourceHull=="Kestrel"?"launcherSlotsLeft":"turretSlotsLeft"),h.Name+" all primary hardpoints fitted");
   Check(w.TotalFitCpu<=h.Value("cpuOutput")&&w.TotalFitPower<=h.Value("powerOutput"),h.Name+" legal starter budget");
   Check(w.Player.WeaponDps>0&&double.IsFinite(w.Player.TotalDps),h.Name+" finite live DPS");
   Check(w.CargoUsed<=w.Player.CargoMax,h.Name+" spare charges fit cargo");
   Check(w.Player.Defense.Any(s=>s.Definition!=null),h.Name+" repair system fitted");
   var forbidden=WeaponCatalog.All.First(s=>s.Size==1&&s.Role.Length==0&&(h.SourceHull=="Kestrel"?s.UsesTurret:s.Kind==WeaponKind.Missile));
   if(h.SourceHull is "Kestrel" or "Punisher" or "Tristan")Check(w.FitError(forbidden,0).Contains("hardpoints"),h.Name+" rejects incompatible hardpoints");
   Check(w.Player.Drones.Sum(d=>d.Definition.Volume)<=h.Value("droneCapacity"),h.Name+" correct starter drone complement");
   var p=w.Player;var magazine=p.Weapons.Sum(s=>s.Loaded);p.Shield*=.6f;
   w.EnterGrid("belt");var t=w.Targets.First(t=>t.Kind==TargetKind.Raider);t.Position=p.Position+new Vector3(1500,0,0);t.Velocity=Vector3.Zero;t.Profile!.SetStat("hp",100000);t.Profile.SetStat("shieldCapacity",100000);t.Profile.SetStat("armorHP",100000);t.Hull=t.Armor=t.Shield=100000;
   w.SelectedId=w.LockedId=t.Id;var starting=t.Shield+t.Armor+t.Hull;w.ToggleFleetRack();
   for(int i=0;i<240;i++)w.Update(1f/30);
   Check(t.Shield+t.Armor+t.Hull<starting,h.Name+" damages a real combat target");
   Check(p.Weapons.All(s=>s.Cooldown>=0),h.Name+" advances weapon state");
   Check(p.Weapons.Where(s=>WeaponCatalog.ById.ContainsKey(s.ModuleId)&&s.Charge?.Crystal!=true).All(s=>s.Loaded<s.Definition.Magazine),h.Name+" every ammunition weapon fires, including extra slots");
   if(h.SourceHull=="Tristan"){
    foreach(var d in p.Drones.ToArray())Check(w.LaunchDrone(d.Id),"Kootenay launches "+d.Definition.Name);
    Check(!w.LoadDrone(p.Drones[0].TypeId,true),"Cannot load drones remotely");
    foreach(var gun in p.Weapons)gun.Stop();float beforeDrones=t.Shield+t.Armor+t.Hull;
    Check(w.DroneCommand("Engage"),"Kootenay drone attack order accepted");for(int i=0;i<180;i++)w.Update(1f/30);
    Check(t.Shield+t.Armor+t.Hull<beforeDrones,"Kootenay drones inflict damage with guns stopped");
   }
   // Return to a station through the existing grid transition, retaining the ship's state.
   w.Screen=ViewMode.Station;w.Grid="station";foreach(var d in p.Drones.Where(d=>d.Location=="Space"))d.Location="Bay";
  }
  Check(w.Credits==wallet,"Starter charter never charges the campaign wallet");
  var active=w.Player;var damage=active.Shield;var fitted=active.Weapons.Select(s=>s.ModuleId).ToArray();
  Check(w.BoardFleetHull(FleetCatalog.Source("Tristan").Id)&&w.BoardFleetHull(active.CombatHull),"Switch between existing hulls without re-granting");
  Check(ReferenceEquals(active,w.Player)&&w.Player.Shield==damage&&w.Player.Weapons.Select(s=>s.ModuleId).SequenceEqual(fitted),"Boarding retains ship identity, damage and fittings");
  Check(w.GrantedFleetHulls.Count==4,"Exactly one starter grant per hull");
  w.EnterGrid("belt");Check(!w.BoardFleetHull(FleetCatalog.Source("Tristan").Id),"Boarding prohibited in space");w.Screen=ViewMode.Station;w.Grid="station";
  string folder=Path.Combine(Path.GetTempPath(),"fleet16-"+Guid.NewGuid().ToString("N"));var old=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
  try{
   Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",folder);w.Save();Check(File.Exists(DemoWorld.SavePath),"Fleet save written");var loaded=new DemoWorld();Check(loaded.Load(),"Fleet save loads");
   Check(loaded.Player.CombatHull==w.Player.CombatHull&&loaded.Player.Weapons.Length==w.Player.Weapons.Length,"Active hull ID and complete weapon array persist");
   Check(loaded.FleetBerths.Count==w.FleetBerths.Count&&loaded.GrantedFleetHulls.Count==4,"Berths and grant ledger persist");
   foreach(var b in w.FleetBerths){var copy=loaded.FleetBerths.First(p=>p.Ship.CombatHull==b.Ship.CombatHull&&p.System==b.System);Check(copy.Ship.HullName==b.Ship.HullName&&copy.Ship.Stat("mass")==b.Ship.Stat("mass")&&copy.Ship.Weapons.Select(s=>s.ModuleId).SequenceEqual(b.Ship.Weapons.Select(s=>s.ModuleId)),"Berthed "+b.Ship.HullName+" retains attributes and fitting");}
   Check(loaded.BoardFleetHull(FleetCatalog.Source("Tristan").Id),"Board persisted hull");
   var gun=loaded.Player.WeaponAt(0)!;string id=gun.ModuleId;float stock=loaded.ItemCount(id,true);Check(loaded.UnfitFleetWeapon(0)&&loaded.ItemCount(id,true)==stock+1,"Unfit conserves module");Check(loaded.FitWeapon(id,true,0)&&loaded.ItemCount(id,true)==stock,"Refit consumes same module");
  }finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",old);if(Directory.Exists(folder))Directory.Delete(folder,true);}
  Console.WriteLine($"{n} fleet checks passed.");
 }
}
