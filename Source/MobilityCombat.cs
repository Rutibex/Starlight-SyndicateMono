using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public float JumpDriveCooldown{get;set;}public int LastMobilityPassengers{get;set;}
 public void StopMobility(MobilityState s){s.Active=false;s.CycleLeft=0;s.StopRequested=false;s.CycleHeat=1;}
 public void LeaveMobilityGrid(){foreach(var s in Player.Mobility)StopMobility(s);}
 public void FaceSelected(){var v=(Selected?.Position??Player.ManualPoint)-Player.Position;if(v.LengthSquared()>1){Player.Facing=Vector3.Normalize(v);Say("Heading set toward selected point.");}}
 public string MobilityActivationError(MobilityState s)
 {
  if(ModuleUnderRepairB("mobility",s))return "Module is being repaired with nanites.";
  if(Cloaked)return "Decloak before propulsion activation.";var d=s.Definition;if(d==null||!d.Active)return "Passive or empty socket.";if(Docked||!s.Online||s.Burnt)return "Undock with online, intact equipment.";if(s.Cooldown>0)return "Reactivation timer running.";
  if(d.Kind is "MWD" or "MJD" or "Field"&&!CanUseMicroDrive("player"))return "Scramble or drive interlock blocks this drive.";
  if(d.Kind is "AB" or "MWD"&&Player.Mobility.Any(x=>x!=s&&x.Active&&x.Definition?.Kind is "AB" or "MWD"))return "Only one thrust drive can run at a time.";
  if(d.Skip&&Player.MobilitySpooling)return "Another vector drive is spooling.";
  if(d.Kind=="Field"&&Current.Security>=.5f&&!WeaponTestMode)return "Vector fields require low-security space.";
  if(Player.Cap<d.Capacitor*(d.Kind is "AB" or "MWD"?Player.RigFactor("propCap",false)*Player.HullFactor("prop","cap",d.Kind):1))return "Insufficient capacitor.";return "";
 }
 public bool ToggleMobility(int slot)
 {
  if(slot<0||slot>=12)return false;var s=Player.Mobility[slot];if(s.Active){if(s.Definition?.Skip==true){Say("Committed spool cannot be stopped early.");return false;}s.StopRequested=true;return true;}
  string error=MobilityActivationError(s);if(error.Length>0){Say(error);return false;}Player.MicroWarpOn=false;s.Active=true;s.StopRequested=false;StartMobilityCycle(s);return true;
 }
 void StartMobilityCycle(MobilityState s)
 {
  var d=s.Definition!;if(Player.Cap<d.Capacitor*(d.Kind is "AB" or "MWD"?Player.RigFactor("propCap",false)*Player.HullFactor("prop","cap",d.Kind):1)){StopMobility(s);return;}Player.Cap-=d.Capacitor*(d.Kind is "AB" or "MWD"?Player.RigFactor("propCap",false)*Player.HullFactor("prop","cap",d.Kind):1);s.CycleLeft=d.Cycle*Player.HullFactor("prop","cycle",d.Kind)*(d.Kind is "AB" or "MWD"?Player.RigFactor("propCycle",false):1);s.CycleHeat=s.Overheat&&d.HeatDamage>0?1+d.Overload*Player.HullFactor("hull","overheat")*Player.ScopedOverheat("prop",d.Kind):1;
  if(d.Skip){var aim=Player.Facing.LengthSquared()>0?Vector3.Normalize(Player.Facing):Vector3.UnitZ;s.AimX=aim.X;s.AimY=aim.Y;s.AimZ=aim.Z;s.Cooldown=d.Reactivation*Player.HullFactor("prop","reactivation",d.Kind)+s.CycleLeft;Say(d.Name+" spool committed.");}
  if(s.Overheat&&d.HeatDamage>0){var f=DefenseFractions();ApplyModuleHeat(s,d.HeatDamage*Player.HullFactor("hull","heatDamage")/Math.Max(1,d.ModuleHp));if(s.Damage>=1){s.Burnt=true;StopMobility(s);RestoreDefenseFractions(f);Say(d.Name+" burnt out.");}}
 }
 public void UpdateMobility(float dt)
 {
  if(dt<=0)return;JumpDriveCooldown=Math.Max(0,JumpDriveCooldown-dt);foreach(var stored in MobilityStorage)stored.State.Cooldown=Math.Max(0,stored.State.Cooldown-dt);
  foreach(var s in Player.Mobility)
  {
   s.Cooldown=Math.Max(0,s.Cooldown-dt);var d=s.Definition;if(d==null||!s.Active)continue;
   if(Docked||!s.Effective||d.Kind is "MWD" or "MJD" or "Field"&&!CanUseMicroDrive("player")){StopMobility(s);continue;}
   float left=dt;while(left>0&&s.Active){float step=Math.Min(left,Math.Max(.0001f,s.CycleLeft));s.CycleLeft=Math.Max(0,s.CycleLeft-step);left-=step;if(s.CycleLeft>0)continue;
    if(d.Skip){ResolveMobilitySkip(s);StopMobility(s);}
    else if(s.StopRequested||!s.Repeat)StopMobility(s);else StartMobilityCycle(s);
   }
  }
 }
 void ResolveMobilitySkip(MobilityState s)
 {
  var d=s.Definition!;if(!CanUseMicroDrive("player"))return;Vector3 origin=Player.Position;Vector3 displacement=new Vector3(s.AimX,s.AimY,s.AimZ)*d.JumpRange;LastMobilityPassengers=0;
  if(d.Kind=="Field")
  {
   int sub=0,cap=0;foreach(var t in Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&Vector3.Distance(t.Position,origin)<=d.Radius).OrderBy(t=>Vector3.DistanceSquared(t.Position,origin)).ToArray())
   {
    bool craft=t.DroneId.Length>0;if(!craft&&!t.Training&&!Wingmates.Any(w=>w.Id==t.Id)&&!Player.HullCapability("micro jump field generators"))continue;if(EwarFor(t.Id).Scrammed)continue;
    if(!craft){var role=CombatHulls.Get(t.Profile?.CombatHull??"frigate").Role;if(d.CapitalCap==0&&role is "Freighter" or "JumpFreighter" or "ExpeditionCommand" or "CapitalIndustrial")continue;bool capital=CombatHulls.Get(t.Profile?.CombatHull??"frigate").Size>=4;if(capital){if(cap>=d.CapitalCap)continue;cap++;}else{if(sub>=d.ShipCap)continue;sub++;}}
    t.Position+=displacement;LastMobilityPassengers++;if(d.PostScram>0)Upsert(new(){SourceId="player",TargetId=t.Id,Kind=EwarKind.Scram,Key="vector-post",Strength=3,Remaining=d.PostScram});var u=Player.Drones.FirstOrDefault(u=>u.Id==t.DroneId);if(u!=null){u.Position=t.Position;if(u.Order=="Move")u.MovePoint+=displacement;}
   }
   foreach(var probe in ScanProbes.Where(p=>p.System==SystemId&&Vector3.Distance(new(p.X,p.Y,p.Z),origin)<=d.Radius)){probe.X+=displacement.X;probe.Y+=displacement.Y;probe.Z+=displacement.Z;}
   foreach(var bomb in MissilesInFlight.Where(m=>m.Bomb&&Vector3.Distance(m.Position,origin)<=d.Radius))bomb.Position+=displacement;
  }
  Player.Position+=displacement;Player.ManualPoint+=displacement;if(d.PostScram>0){Player.MicroWarpOn=false;Upsert(new(){SourceId="player",TargetId="player",Kind=EwarKind.Scram,Key="vector-post",Strength=3,Remaining=d.PostScram});}
  RetainedLocks.RemoveAll(l=>!CanLock("player",l.Id));Acquisitions.RemoveAll(a=>!CanLock("player",a.Id));if(LockedId.Length>0&&!CanLock("player",LockedId))LockedId="";
  WeaponVisuals.Add(new(){Start=origin,End=Player.Position,Kind=WeaponKind.Arc,Radius=d.Radius,Remaining=.8f,DamageType=DamageType.EM});SyncWingmates();Say(d.Name+" complete / "+(d.JumpRange/1000).ToString("0")+" km / "+LastMobilityPassengers+" passengers.");
 }
 public double WarpRate=>Player.Stat("baseWarpSpeed")*Player.Stat("warpSpeedMultiplier");
 public float WarpPresentationSeconds=>(float)WarpCurve.Calculate(2*SolarCatalog.AU,WarpRate,Player.Speed).Total;
 public double WarpTravelHours=>WarpPresentationSeconds/3600.0;
 public static double BeaconDistance(string from,string to){if(from.StartsWith("system:")&&to.StartsWith("system:")&&int.TryParse(from[7..],out int a)&&int.TryParse(to[7..],out int b)&&Galaxy42.ById.TryGetValue(a,out var x)&&Galaxy42.ById.TryGetValue(b,out var y))return Math.Sqrt(Enumerable.Range(0,3).Sum(i=>Math.Pow(x.Position[i]-y.Position[i],2)))/Galaxy42.LightYear;return Math.Abs(Array.FindIndex(Systems,s=>s.Id==from)-Array.FindIndex(Systems,s=>s.Id==to))*.55+1;}
 public int JumpFuelRequired(string destination)=>(int)Math.Ceiling(BeaconDistance(SystemId,destination)*Player.Stat("jumpDriveConsumptionAmount"));
 public string JumpDriveError(string destination)
 {
  if(InHubH||destination=="system:30100000")return "Jump drives unavailable in the restricted hub.";
  if(ImportedWorld43&&!LegalBeaconDestination(destination))return "Choose a legal imported low/null-security beacon destination outside wormholes and Pochven.";
  string specialError=SpecialJumpError(destination);if(specialError.Length>0)return specialError;var target=ImportedWorld43?ResolveSystem43(destination):Systems.FirstOrDefault(s=>s.Id==destination);if(target==null||destination==SystemId)return "Choose another beacon.";if(Docked||Player.Stat("canJump")<=0)return "Undock a jump-capable hull.";if(!CanUseJumpDrive("player")||Player.MobilitySpooling)return "Tackle or drive interlock blocks transit.";if(JumpDriveCooldown>0)return "Jump transit cooldown.";
  if(target.Security>=.5f)return "No navigation beacon in high-security space.";if(BeaconDistance(SystemId,destination)>Player.Stat("jumpDriveRange"))return "Beacon beyond jump reach.";if(Player.Cap<Player.MaxCap*Player.Stat("jumpDriveCapacitorNeed"))return "Insufficient jump capacitor.";if(ItemCount("jump_fuel",false)<JumpFuelRequired(destination))return "Load transit fuel into cargo.";return "";
 }
 public bool JumpToBeacon(string destination)
 {if(InAbyssF)return false;
  string error=JumpDriveError(destination);if(error.Length>0){Say(error);return false;}double distance=BeaconDistance(SystemId,destination);int fuel=JumpFuelRequired(destination);ChangeStack("jump_fuel",false,-(decimal)(fuel));Player.Cap-=Player.MaxCap*(float)Player.Stat("jumpDriveCapacitorNeed");float remainingCap=Player.Cap;JumpDriveCooldown=60+Math.Min(1740,JumpFatigue*.1f);JumpFatigue=Math.Min(18000,JumpFatigue+300*(float)distance*(float)Player.Stat("jumpFatigueMultiplier"));
  LeaveSupportGrid(true);RecoverProbesForTransit();SystemId=destination;if(ImportedWorld43){StationId="";EnterDeepSpace(default);}else EnterGrid("station",false);Player.Cap=remainingCap;AdvanceHours(distance*.6);Say("Beacon transit / "+fuel+" transit fuel consumed.");return true;
 }
 public void ProvisionTransitFuel(){if(!Docked)return;int q=Math.Min(2000,Math.Min((int)ItemCount("jump_fuel",true),(int)(CargoRoom("jump_fuel")/ItemVolume("jump_fuel"))));if(q>0)TransferItem("jump_fuel",true,q);}
}
