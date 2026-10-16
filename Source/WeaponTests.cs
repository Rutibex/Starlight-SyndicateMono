using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
internal static class WeaponTests
{
 public static void Run()
 {
  int n=0;void Check(bool ok,string message){if(!ok)throw new Exception("WEAPON FAIL: "+message);n++;Console.WriteLine("WEAPON PASS: "+message);}
  void Step(DemoWorld world,float seconds){for(int i=0;i<(int)Math.Ceiling(seconds/.05f);i++)world.Update(.05f);}
  DemoWorld Trial(WeaponDefinition w)
  {
   var world=new DemoWorld();world.NewGame(0);var h=CombatHulls.All.First(h=>h.Size>=w.Size&&(w.Role.Length==0||h.Role==w.Role)&&(!w.UsesLauncher||h.Launchers>=2));world.WeaponTrial(h.Id);
   Check(world.FitWeapon(w.Id,true,0),"Fits "+w.Name);
   var state=world.Player.Weapons[0];var c=ChargeCatalog.Default(w);if(c!=null)Check(world.LoadWeapon(0,c.Id),"Loads "+w.Name);
   world.EnterWeaponRange();world.WeaponSafety=false;world.SelectedId=world.LockedId="range_0";
   foreach(var t in world.Targets){t.Position=new(500,0,0);t.Velocity=Vector3.Zero;}
   return world;
  }
  Check(WeaponCatalog.All.Length==112&&WeaponCatalog.All.Select(w=>w.Id).Distinct().Count()==112,"112 unique weapon definitions");
  Check(WeaponCatalog.All.Select(w=>w.Name).Distinct().Count()==112,"All weapon names are original and distinct");
  Check(WeaponCatalog.All.All(w=>w.Cycle>0&&w.Damage>0&&w.Cpu>0&&w.Power>0),"Every weapon has positive balance and fitting data");
  foreach(var def in WeaponCatalog.All)
  {
   var w=Trial(def);var target=w.Locked!;float health=target.Shield+target.Armor+target.Hull;var state=w.Player.Weapons[0];
   if(def.Kind==WeaponKind.Bomb){foreach(var t in w.Targets)t.Position=new(def.Velocity*def.FlightTime,0,0);w.SelectedId="range_0";}
   if(def.Kind==WeaponKind.Defender)w.MissilesInFlight.Add(new(){Position=new(1000,0,0),Bomb=true,Enemy=true,Life=40,Speed=0,Hp=70});
   w.ToggleWeapon(0);Step(w,def.Superweapon?def.Warmup+def.Duration+1:def.Kind==WeaponKind.Bomb?def.FlightTime+1:5);
   if(def.Kind==WeaponKind.Defender)Check(!w.MissilesInFlight.Any(m=>m.Bomb&&m.Enemy),"Intercepts bomb / "+def.Name);
   else Check(target.Shield+target.Armor+target.Hull<health,"Applies combat effect / "+def.Name);
  }
  var small=WeaponCatalog.All.First(w=>w.Family=="Railguns"&&w.Size==1);
  Check(DemoWorld.TurretChance(small,null,small.Optimal,0,80)==1,"Stationary turret solution at optimal");
  Check(Math.Abs(DemoWorld.TurretChance(small,null,small.Optimal+small.Falloff,0,80)-.5)<.001,"One falloff halves hit chance");
  Check(DemoWorld.TurretChance(small,null,small.Optimal,.5f,20)<DemoWorld.TurretChance(small,null,small.Optimal,.5f,200),"Tracking punishes small fast targets");
  Check(DemoWorld.MissileApplication(20,900,200,100,.7f)<DemoWorld.MissileApplication(400,20,200,100,.7f),"Missile application accounts for speed and signature");
  var ent=WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Entropic&&w.Size==1);var ramp=Trial(ent);ramp.ToggleWeapon(0);Step(ramp,9);Check(ramp.Player.Weapons[0].RampCycles>=2,"Entropy damage ramps over uninterrupted cycles");ramp.ToggleWeapon(0);Check(ramp.Player.Weapons[0].RampCycles==0,"Entropy ramp resets on stop");
  var arc=Trial(WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Arc&&w.Size==1));arc.ToggleWeapon(0);arc.Update(.05f);Check(arc.Targets.Count(t=>t.Shield<t.Profile!.MaxShield)==5,"Arcing weapon hits exactly five clustered ships");
  var defender=Trial(WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Defender));defender.MissilesInFlight.Add(new(){Enemy=true,Bomb=false,Position=new(100,0,0),TargetId="range_0",Life=100,Speed=0});defender.ToggleWeapon(0);defender.Update(.05f);Check(defender.MissilesInFlight.Count==1&&defender.Player.Weapons[0].Loaded==8,"Defender ignores ordinary missiles and preserves ammo");
  var inv=Trial(small);inv.Screen=ViewMode.Station;var stateInv=inv.Player.Weapons[0];string charge=stateInv.ChargeId;float before=inv.ItemCount(charge,true)+stateInv.Loaded;Check(inv.LoadWeapon(0,charge)&&Math.Abs(inv.ItemCount(charge,true)+stateInv.Loaded-before)<.001,"Reload preserves ammunition quantity");
  var capital=WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Doomsday);inv.Player.SetCombatHull("frigate");inv.Hangar[capital.Id]=1;Check(!inv.FitWeapon(capital.Id,true,0)&&inv.Hangar[capital.Id]==1,"Oversized fitting fails without consuming stock");
  var parasite=Trial(WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Breacher&&w.Size==1));foreach(var d in Enum.GetValues<DamageType>())parasite.Locked!.Profile!.SetStat("shield"+(d==DamageType.EM?"Em":d.ToString())+"DamageResonance",.01);parasite.ToggleWeapon(0);Step(parasite,2);Check(parasite.Parasites.Count>0,"Parasite attachment survives extreme resistance");var victim=parasite.Locked!;float hp=victim.Shield;parasite.Player.Weapons[0].Stop();parasite.Parasites.Clear();parasite.Parasites.Add(new(){TargetId=victim.Id,Dps=2,Remaining=10});parasite.Parasites.Add(new(){TargetId=victim.Id,Dps=5,Remaining=10});Step(parasite,1.1f);Check(Math.Abs(hp-victim.Shield-5)<.02,"Parasites use strongest effect and ignore resistance");
  var smart=Trial(WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Smartbomb&&w.Size==1));smart.MissilesInFlight.Add(new(){Bomb=false,Position=new(100,0,0),Life=30,Hp=5,TargetId="range_0",Speed=0});smart.ToggleWeapon(0);smart.Update(.05f);Check(smart.MissilesInFlight.Count==0,"Smartbomb destroys nearby ordnance");
  Check(DemoWorld.BeamContains(Vector3.Zero,Vector3.UnitX,new(500,10,0),1000,20)&&!DemoWorld.BeamContains(Vector3.Zero,Vector3.UnitX,new(-500,0,0),1000,20)&&!DemoWorld.BeamContains(Vector3.Zero,Vector3.UnitX,new(500,100,0),1000,20),"Beam respects direction, length and radius");
  var lance=Trial(WeaponCatalog.All.First(w=>w.Kind==WeaponKind.DisruptiveLance));lance.ToggleWeapon(0);Step(lance,7);Check(lance.Targets[0].DriveLockout>0&&lance.Targets[0].RemoteRepairMultiplier==.5f,"Disruption lance applies drive and remote-repair penalties");Check(lance.Player.DriveLockout>0,"Superweapon immobilizes firing ship's drives");
  string temp=Path.Combine(Path.GetTempPath(),"starlight-weapon-"+Guid.NewGuid().ToString("N"));string? old=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
  try
  {
   Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",temp);parasite.Player.Weapons[0].ReloadLeft=3;parasite.MissilesInFlight.Add(new(){Bomb=true,Position=new(12,34,56),Life=7,Packet=new(2,3,4,5)});parasite.Save();var load=new DemoWorld();Check(load.Load(),"Weapon save loads");Check(load.Player.Weapons[0].ModuleId==parasite.Player.Weapons[0].ModuleId&&load.Player.Weapons[0].ReloadLeft==3&&load.Parasites.Count==2&&load.MissilesInFlight.Count==1&&load.MissilesInFlight[0].Position==new Vector3(12,34,56)&&load.Targets.Count==6,"Fit, charges, reload, pods, ordnance, range and hull persist");
  }
  finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",old);if(Directory.Exists(temp))Directory.Delete(temp,true);}
  Console.WriteLine($"{n} weapon checks passed.");
 }
}
