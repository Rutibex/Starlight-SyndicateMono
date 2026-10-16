using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class SpecialSave
{
 public List<SpecialStored> Storage{get;set;}=[];public SpecialState[] Rack{get;set;}=[];public float LockRecovery{get;set;}public float Fatigue{get;set;}public float Observatory{get;set;}=600;public string BeaconSelection{get;set;}="";public string Affinity{get;set;}="Aurora";public float Nexus{get;set;}public List<JumpBeacon> Beacons{get;set;}=[];public List<CloneRecord> Clones{get;set;}=[];public List<RelayRecord> Relays{get;set;}=[];public List<WormholeRecord> Wormholes{get;set;}=[];public Dictionary<string,float> Mappers{get;set;}=[];public Dictionary<string,int> StoredShips{get;set;}=[];public float CloneCooldown{get;set;}public Dictionary<string,float> FleetNexus{get;set;}=[];
}
public sealed partial class DemoWorld
{
 public float ObservatoryTimer=600;public bool PreserveMassTransit;public float SpecialLockRecovery{get;set;}public float JumpFatigue{get;set;}public bool Cloaked=>Player.Special.Any(s=>s.Active&&s.Effective&&s.Definition!.Cloak);
 public bool SpecialImmobile=>Player.Special.Any(s=>s.Active&&s.Definition!.Immobile)||Player.NexusLeft>0;
 public bool SpecialWarpLocked=>Player.Special.Any(s=>s.Active&&s.Definition!.WarpLock)||Player.NexusLeft>0;
 public bool SpecialDockLocked=>Player.Special.Any(s=>s.Active&&s.Definition!.DockLock)||Player.NexusLeft>0||Cloaked;
 public bool SpecialOffenseBlocked=>Cloaked||Player.NexusLeft>0||Player.Special.Any(s=>s.Active&&s.Definition!.OffensiveBlock);
 public bool SpecialEwarBlocked=>SpecialOffenseBlocked||Player.HasSpecial("NSA")||Player.HasSpecial("ISA");
 public bool SpecialMicroBlocked=>Cloaked||Player.NexusLeft>0||Player.Special.Any(s=>s.Active&&s.Definition!.Kind!="ISA"&&s.Definition.WarpLock);
 public bool ActiveSpecialCore=>Player.HasSpecial("Core");
 public Dictionary<string,float> FleetNexus{get;set;}=[];
 public SpecialSave CaptureSpecial()=>new(){Observatory=ObservatoryTimer,Storage=SpecialStorage,Rack=Player.Special,LockRecovery=SpecialLockRecovery,Fatigue=JumpFatigue,BeaconSelection=JumpBeaconSelection,Affinity=TitanAffinity,Nexus=Player.NexusLeft,Beacons=JumpBeacons,Clones=JumpClones,Relays=RelayNodes,Wormholes=Wormholes,Mappers=MappedPilots,StoredShips=RecloneShips,CloneCooldown=CloneJumpCooldown,FleetNexus=FleetNexus};
 public void RestoreSpecial(SpecialSave? d){if(d==null)return;ObservatoryTimer=d.Observatory;SpecialStorage=d.Storage;if(d.Rack.Length==8&&d.Rack.All(s=>s.ModuleId.Length==0||SpecialCatalog.ById.ContainsKey(s.ModuleId)))Player.Special=d.Rack;SpecialLockRecovery=d.LockRecovery;JumpFatigue=d.Fatigue;JumpBeaconSelection=d.BeaconSelection;TitanAffinity=d.Affinity;Player.NexusLeft=d.Nexus;JumpBeacons=d.Beacons;JumpClones=d.Clones;RelayNodes=d.Relays;Wormholes=d.Wormholes;MappedPilots=d.Mappers;RecloneShips=d.StoredShips;CloneJumpCooldown=d.CloneCooldown;FleetNexus=d.FleetNexus;foreach(var t in Targets)if(t.Profile!=null)t.Profile.NexusLeft=FleetNexus.GetValueOrDefault(t.Id);RebuildSpecialEnvironment();IndustrialCore=ActiveSpecialCore;CoreLeft=Player.Special.FirstOrDefault(s=>s.Active&&s.Definition?.Kind=="Core")?.CycleLeft??0;RestoreSpecialGrid();}
 public void ResetSpecial(){ObservatoryTimer=600;SpecialLockRecovery=JumpFatigue=CloneJumpCooldown=0;SpecialStorage.Clear();JumpBeacons.Clear();JumpClones.Clear();RelayNodes.Clear();Wormholes.Clear();MappedPilots.Clear();RecloneShips.Clear();FleetNexus.Clear();ParkedShips.Clear();JumpBeaconSelection="";TitanAffinity="Aurora";InitializeSpecialCluster();}
 public string SpecialActivationError(int slot)
 {
  if(slot<0||slot>=8||!Player.Special[slot].Effective)return "Online special module required.";var s=Player.Special[slot];var d=Player.SpecialHullDefinition(s.Definition!);
  if(RobbingQ&&d.Cloak)return "Reagent robbery prevents cloaking.";if(InHubH&&(d.Beacon||d.Portal))return "Beacons and bridges unavailable in restricted hub.";if(Docked)return "Undock before activation.";if(s.Cooldown>0)return "Reactivation delay.";if(Player.MobilitySpooling)return "Finish drive spool first.";if(Cloaked&&!d.Cloak)return "Decloak first.";
  if(Player.Special.Any(x=>x!=s&&x.Active&&x.Definition!.Group==d.Group)&&d.Kind!="Mass")return "Another system in this group is active.";
  if(Player.Cap<d.Capacitor)return "Insufficient capacitor.";if(d.Fuel.Length>0&&ItemCount(d.Fuel,false)<d.FuelQuantity)return "Load "+d.FuelQuantity+" "+ItemName(d.Fuel)+" into cargo.";
  if(d.Cloak){if(Player.NexusLeft>0||Player.Special.Any(x=>x.Active&&!x.Definition!.Cloak&&x.Definition.Kind!="Mass")||Player.DriveLockout>0)return "Active system interlock prevents cloaking.";if(Enumerable.Range(0,Player.Weapons.Length).Any(i=>Player.WeaponAt(i)?.Definition.Kind==WeaponKind.DisruptiveLance))return "Fitted disruptive lance prevents cloaking.";if(Targets.Any(t=>t.Alive&&t.Kind==TargetKind.Raider&&t.LockId=="player"))return "A ship already has you locked.";if(Targets.Any(t=>t.Alive&&Vector3.Distance(t.Position,Player.Position)<2000))return "Move at least 2 km from nearby objects.";}
  if(d.Beacon){if(Current.Security>=.5&&!WeaponTestMode)return "Beacon deployment is restricted to low or unregulated security.";if(Player.Velocity.Length()>500)return "Reduce speed below 500 m/s.";}
  if(d.Portal){if(Player.NexusLeft>0||Player.Special.Any(x=>x!=s&&x.Active&&x.Definition!.WarpLock))return "Finish other deployment modes before opening a bridge.";var b=SelectedJumpBeacon();if(b==null)return "Select a compatible live remote beacon.";string e=BeaconCompatibility(b,d.Kind);if(e.Length>0)return e;if(BeaconDistance(SystemId,b.System)>(Player.EliteBattleship?Player.Stat("jumpDriveRange"):Math.Max(5,Player.Stat("jumpDriveRange"))))return "Beacon beyond bridge range.";}
  if(d.Kind=="Panic"&&(Locked?.Kind!=TargetKind.Asteroid||!Locked.Alive||Vector3.Distance(Player.Position,Locked.Position)>d.Range))return "Lock an asteroid within nexus range.";
  if(d.Kind=="Entosis"){var node=RelayNodes.FirstOrDefault(n=>n.Id==LockedId&&n.System==SystemId);if(node==null||Locked==null)return "Lock a sovereignty relay.";if(Vector3.Distance(Player.Position,Locked.Position)>d.Range)return "Relay outside link range.";if(!node.Vulnerable)return "Relay vulnerability window is closed.";}
  return "";
 }
 public bool ToggleSpecial(int slot)
 {
  if(slot<0||slot>=8)return false;var s=Player.Special[slot];if(s.Active){if(s.Definition!.Committed){s.StopRequested=true;Say("Stop requested at cycle boundary.");}else StopSpecial(s);return true;}string error=SpecialActivationError(slot);if(error.Length>0){Say(error);return false;}return StartSpecial(s);
 }
 bool StartSpecial(SpecialState s)
 {
  var d=Player.SpecialHullDefinition(s.Definition!);if(Player.Cap<d.Capacitor||d.Fuel.Length>0&&ItemCount(d.Fuel,false)<d.FuelQuantity)return false;
  Player.Cap-=d.Capacitor;if(d.Fuel.Length>0)ChangeStack(d.Fuel,false,-(decimal)(d.FuelQuantity));s.Active=true;s.StopRequested=false;s.CycleLeft=d.Cloak?0:Math.Max(.1f,d.Cycle)*(d.Kind=="Entosis"&&CombatHulls.Get(Player.CombatHull).Size>=4?5:1);s.TargetId=LockedId;s.ActiveSeconds=0;
  if(d.Cloak){ClearEnhancedLocks();LockedId=PendingLockId="";foreach(var w in Player.Weapons)w.Active=false;Player.TurretOn=Player.MissileOn=Player.MinerOn=false;foreach(var u in Player.Utilities)StopUtility(u);foreach(var e in Player.Ewar)e.Stop();StopDefenses();foreach(var u in Player.Support)u.Active=false;}
  if(d.Immobile){Player.Velocity=Vector3.Zero;Player.Order="stop";}if(d.Beacon){JumpBeacons.Add(new(){Id="player-beacon",System=SystemId,Kind=d.Kind,Owner="player",Left=s.CycleLeft});}
  if(d.Kind=="Panic"){Player.NexusLeft=s.CycleLeft;foreach(var t in Targets.Where(t=>t.Alive&&FleetMember(t.Id)&&MiningNexusEligible(t.Profile)&&Vector3.Distance(Player.Position,t.Position)<=d.Range)){FleetNexus[t.Id]=s.CycleLeft;if(t.Profile!=null)t.Profile.NexusLeft=s.CycleLeft;}Player.TurretOn=Player.MissileOn=false;foreach(var w in Player.Weapons)w.Active=false;}
  if(d.Kind=="Core"){IndustrialCore=true;CoreLeft=s.CycleLeft;}RebuildSpecialEnvironment();Say(d.Name+" active.");return true;
 }
 bool MiningNexusEligible(Ship? p)=>p!=null&&CombatHulls.Get(p.CombatHull).Role is "MiningBarge" or "MiningTender" or "ExpeditionCommand" or "CapitalIndustrial" or "ExpeditionFrigate";
 public void StopSpecial(SpecialState s){bool cloak=s.Definition?.Cloak==true;s.Active=false;s.StopRequested=false;s.CycleLeft=0;s.Warmed=false;s.Cooldown=Math.Max(s.Cooldown,s.Definition==null?0:Player.SpecialHullDefinition(s.Definition).Cooldown);if(cloak)SpecialLockRecovery=Math.Max(SpecialLockRecovery,Player.SpecialHullDefinition(s.Definition!).TargetDelay*Player.RigFactor("cloakRecovery"));if(s.Definition?.Beacon==true)JumpBeacons.RemoveAll(b=>b.Owner=="player");if(s.Definition?.Kind=="Core"){IndustrialCore=CoreStop=false;CoreLeft=0;foreach(var u in Player.Utilities.Where(u=>u.Definition?.Kind=="Compress"))StopUtility(u);}RebuildSpecialEnvironment();}
 public void LeaveSpecialGrid(bool force=false){foreach(var s in Player.Special.Where(s=>s.Active).ToArray())if((force||!s.Definition!.Cloak)&&!(PreserveMassTransit&&s.Definition!.Kind=="Mass"))StopSpecial(s);Player.SpecialEnvironment.Clear();}
 public void UpdateSpecial(float dt)
 {
  if(dt<=0)return;foreach(var x in SpecialStorage)x.State.Cooldown=Math.Max(0,x.State.Cooldown-dt);ObservatoryTimer-=dt;if(ObservatoryTimer<=0){if(Current.Security<.5f)CloakObservatoryPulse();ObservatoryTimer+=600;}SpecialLockRecovery=Math.Max(0,SpecialLockRecovery-dt);JumpFatigue=Math.Max(0,JumpFatigue-dt);CloneJumpCooldown=Math.Max(0,CloneJumpCooldown-dt);Player.NexusLeft=Math.Max(0,Player.NexusLeft-dt);
  foreach(var id in FleetNexus.Keys.ToArray()){FleetNexus[id]=Math.Max(0,FleetNexus[id]-dt);var t=Targets.FirstOrDefault(t=>t.Id==id);if(t?.Profile!=null)t.Profile.NexusLeft=FleetNexus[id];if(FleetNexus[id]<=0)FleetNexus.Remove(id);}
  foreach(var id in MappedPilots.Keys.ToArray()){MappedPilots[id]=Math.Max(0,MappedPilots[id]-dt);if(MappedPilots[id]<=0)MappedPilots.Remove(id);}
  foreach(var b in JumpBeacons.Where(b=>b.Owner!="player"))b.Left=Math.Max(0,b.Left-dt);JumpBeacons.RemoveAll(b=>b.Left<=0||b.Owner!="player"&&b.Hull<=0);
  foreach(var h in Wormholes){h.Polarization=Math.Max(0,h.Polarization-dt);if(h.Polarization<=0)h.RecentJumps=0;}
  if(!WeaponTestMode)foreach(var n in RelayNodes){double hour=Day*24%24;n.Vulnerable=hour>=n.WindowStart&&hour<n.WindowEnd;}
  foreach(var s in Player.Special){s.Cooldown=Math.Max(0,s.Cooldown-dt);if(!s.Active||s.Definition==null)continue;var d=s.Definition;s.ActiveSeconds+=dt;
   if(d.Cloak){if(Targets.Any(t=>t.Alive&&Vector3.Distance(Player.Position,t.Position)<2000)){StopSpecial(s);Say("Proximity decloaking.");}continue;}
   if(d.Kind=="Entosis"&&(LockedId!=s.TargetId||Locked==null||Vector3.Distance(Player.Position,Locked.Position)>d.Range)){StopSpecial(s);Say("Relay lock lost; warmup reset.");continue;}
   s.CycleLeft-=dt;if(d.Kind=="Core")CoreLeft=Math.Max(0,s.CycleLeft);if(d.Beacon){var b=JumpBeacons.FirstOrDefault(b=>b.Owner=="player");if(b!=null)b.Left=Math.Max(0,s.CycleLeft);}
   int guard=0;while(s.Active&&s.CycleLeft<=0&&guard++<128){float overshoot=-s.CycleLeft;if(d.Kind=="Entosis"){var n=RelayNodes.FirstOrDefault(n=>n.Id==s.TargetId);if(n!=null&&n.Vulnerable&&!n.Contested){if(s.Warmed){n.Progress=Math.Min(1,n.Progress+.25f);if(n.Progress>=1){n.Owner="Syndicate";Say("Relay secured for the Syndicate.");}}else s.Warmed=true;}}
    if(s.StopRequested||!s.Repeat||d.NoRepeat||!s.Effective||d.Kind=="Entosis"&&RelayNodes.FirstOrDefault(n=>n.Id==s.TargetId)?.Vulnerable!=true){StopSpecial(s);break;}bool warmed=s.Warmed;string target=s.TargetId;if(!StartSpecial(s)){StopSpecial(s);Say("Capital cycle stopped: fuel or capacitor exhausted.");break;}s.Warmed=warmed;s.TargetId=target;s.CycleLeft-=overshoot;
   }
  }
  if(SpecialImmobile){Player.Velocity=Vector3.Zero;Player.Order="stop";}if(Player.HasSpecial("Entosis")&&Player.Velocity.Length()>4000)Player.Velocity=Vector3.Normalize(Player.Velocity)*4000;
  RebuildSpecialEnvironment();RefreshCapitalDrones();UpdateSpecialCloning();
 }
 public void RebuildSpecialEnvironment()
 {
  var aura=Player.Special.FirstOrDefault(s=>s.Active&&s.Definition?.Kind=="Phenomena");var entities=Targets.Where(t=>t.Profile!=null).ToArray();float ps=Player.Shield/Math.Max(1,Player.MaxShield),pa=Player.Armor/Math.Max(1,Player.MaxArmor);Player.SpecialEnvironment=aura?.Definition?.Bonuses.ToDictionary(x=>x.Key,x=>x.Value)??[];Player.Shield=ps*Player.MaxShield;Player.Armor=pa*Player.MaxArmor;
  foreach(var t in entities){var p=t.Profile!;float sh=t.Shield/Math.Max(1,p.MaxShield),ar=t.Armor/Math.Max(1,p.MaxArmor);p.SpecialEnvironment=aura!=null&&Vector3.Distance(Player.Position,t.Position)<=aura.Definition!.Range?aura.Definition.Bonuses.ToDictionary(x=>x.Key,x=>x.Value):[];t.Shield=sh*p.MaxShield;t.Armor=ar*p.MaxArmor;t.Signature=p.Signature;}
 }
 public void FinishSpecialTrialCycle(){if(!WeaponTestMode)return;float dt=Player.Special.Where(s=>s.Active&&!s.Definition!.Cloak).Select(s=>s.CycleLeft).DefaultIfEmpty(0).Max();if(dt>0)UpdateSpecial(dt+.001f);}
 public void EnterSpecialRange(){if(!Docked||!WeaponTestMode)return;EnterGrid("special_range",false);Player.Position=Vector3.Zero;Targets.Clear();Targets.Add(new(){Id="special_rock",Name="Nexus asteroid",Kind=TargetKind.Asteroid,Position=new(4000,0,0),Ore=300000});string relay="relay_"+SystemId;if(!RelayNodes.Any(n=>n.Id==relay))RelayNodes.Add(new(){Id=relay,System=SystemId});if(Wormholes.Count==0)Wormholes.Add(new(){Id="fracture_1",A=SystemId,B=Systems.Last().Id,RemainingMass=150000000,MaxShipMass=5000000});RestoreSpecialGrid();RestoreWingmates();Say("SPECIAL LOAN RANGE: asteroid, sovereignty relay, finite-mass fracture and contracted jump beacons.");}
 public void RestoreSpecialGrid(){if(Docked||Grid=="utility_site"||Grid=="utility_range"||Grid=="range"||Grid.EndsWith("range")&&Grid!="special_range")return;foreach(var n in RelayNodes.Where(n=>n.System==SystemId))if(!Targets.Any(t=>t.Id==n.Id))Targets.Add(new(){Id=n.Id,Name=n.Owner+" sovereignty relay",SiteKind="Relay",Kind=TargetKind.Container,Position=new(6000,0,0)});foreach(var h in Wormholes.Where(h=>h.RemainingMass>0&&(h.A==SystemId||h.B==SystemId)))if(!Targets.Any(t=>t.Id==h.Id))Targets.Add(new(){Id=h.Id,Name="Hyperspace fracture",SiteKind="Wormhole",Kind=TargetKind.Container,Position=new(2400,0,0)});}
}
