using Microsoft.Xna.Framework;
using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 bool restoringGridL;
 public const string RelayDefinitionL="l_relay";
 public Mission38? ActiveMissionL=>AgentsState.Missions.FirstOrDefault(m=>m.WarpId==Grid&&m.SiteSystem==SystemId&&m.State=="Accepted");
 public GraphRunL EnsureMissionL(Mission38 m,bool captureLegacy=false)
 {
  if(m.GraphL!=null)return m.GraphL;
  var run=GraphCompilerL.Start(GraphCompilerL.Mission(m),m.Accepted==default?m.Offered:m.Accepted,m.Expires,StableC(m.Id));m.GraphL=run;run.Pocket=m.Pocket;run.Entered[m.Pocket]=m.Accepted==default?m.Offered:m.Accepted;
  foreach(string id in m.Kills)run.Emit("kill:"+id);if(m.Harvested>=m.Spec.Volume&&m.Spec.Division==AgentDivision38.Mining)run.Emit("harvest");
  if(m.ObjectiveDone){run.Fired.Add("done");run.State="Completed";}
  if(m.State is "Failed" or "Expired" or "Declined")run.State=m.State=="Declined"?"Failed":m.State;
  if(m.State=="Completed"){run.State="Completed";run.Settlement=new(){Id="legacy/"+m.Id,Time=m.Accepted,Corporation=m.Contact.Corporation};}
  if(captureLegacy&&Grid==m.WarpId&&Targets.Any(t=>t.Id.StartsWith(m.WarpId+":"))){
   foreach(var t in Targets.Where(t=>t.Id.StartsWith(m.WarpId+":")&&t.Kind==TargetKind.Raider)){if(!run.Npcs.ContainsKey(t.Id))run.Npcs[t.Id]=new(){Group=m.Pocket+"/"+m.Wave};PrepareNpcL(t,run.Npcs[t.Id],false);}
   run.Snapshots[m.Pocket]=CopyTargets(Targets.Where(t=>t.Id.StartsWith(m.WarpId+":")||t.Id.StartsWith("wreck_"+m.WarpId+":")));
   foreach(string id in m.Kills)run.Generated.Add(id);
  }
  return run;
 }
 public void RestoreMissionsL()
 {
  foreach(var m in AgentsState.Missions){var run=EnsureMissionL(m,true);if(GraphCompilerL.Validate(run.Definition).Length>0||run.Digest!=GraphCompilerL.Digest(run.Definition)||run.Pocket<0||run.Pocket>=run.Definition.Pockets)throw new InvalidDataException("Invalid saved encounter graph.");}
  // Loading keeps the save's live grid, including partially damaged NPCs. It does not enter a new pocket.
 }
 public void SyncMissionL()
 {
  if(restoringGridL)return;var m=AgentsState.Missions.FirstOrDefault(m=>m.WarpId==Grid);if(m==null)return;var run=EnsureMissionL(m,true);
  run.Snapshots[m.Pocket]=CopyTargets(Targets.Where(t=>t.Id.StartsWith(m.WarpId+":")||t.Id.StartsWith("wreck_"+m.WarpId+":")));
  foreach(var drop in run.Drops.Values.Where(d=>d.Pocket==m.Pocket)){var t=Targets.FirstOrDefault(t=>t.Id=="wreck_"+drop.Target);if(t!=null){drop.Loot=new(t.Contents);drop.Salvage=t.Salvage;}}
 }
 public bool EnterMissionL(Mission38 m)
 {
  var run=EnsureMissionL(m);m.Pocket=run.Pocket;Grid=m.WarpId;SolarCelestial="";SolarGridKey=m.WarpId+":"+m.Pocket;MissionWarpTarget38(m.WarpId,out SolarOrigin);SelectedId=LockedId="";Targets.Clear();MissilesInFlight.Clear();Player.Position=new(-3400,180,400);Player.Velocity=Vector3.Zero;
  if(!run.Entered.ContainsKey(m.Pocket))run.Entered[m.Pocket]=Now;
  if(run.Snapshots.TryGetValue(m.Pocket,out var saved)){Targets.AddRange(CopyTargets(saved));}
  PumpMissionL(m);
  if(m.ProfileM==null&&m.Spec.Division==AgentDivision38.Mining&&m.Harvested<m.Spec.Volume&&!Targets.Any(t=>t.Id==m.WarpId+":resource"))Targets.Add(new(){Id=m.WarpId+":resource",Name=m.Spec.Name+" / contract "+m.Spec.Resource,Kind=TargetKind.Asteroid,Resource=m.Spec.Resource,Ore=m.Spec.Volume-m.Harvested,Position=new(1800,0,0),Signature=300});
  if(m.Spec.Objective=="Retrieve"&&m.ObjectiveDone&&m.Cargo+m.Hangars.Values.Sum()<m.Spec.Volume&&!run.Events.Contains("collect")&&!Targets.Any(t=>t.Id==m.WarpId+":objective"))AddObjectiveL(m);
  foreach(var gate in run.Definition.Gates.Where(g=>g.From==m.Pocket)){string id=gate.Id.StartsWith("back")?m.WarpId+":backgate":m.WarpId+":gate";if(!Targets.Any(t=>t.Id==id))Targets.Add(new(){Id=id,Name=gate.Id.StartsWith("back")?"Return acceleration gate":"Acceleration gate",Kind=TargetKind.Celestial,Position=new(9000,0,gate.Id.StartsWith("back")?2500:0),Signature=2000});}
  SyncMissionL();return true;
 }
 void AddObjectiveL(Mission38 m){Targets.Add(new(){Id=m.WarpId+":objective",Name="Mission dispatch / "+m.Spec.Name,Kind=TargetKind.Container,Position=new(2000,0,0),OwnerId="mission:"+m.Id,Signature=100});}
 public void PumpMissionL(Mission38 m)
 {
  var run=EnsureMissionL(m);if(m.State!="Accepted")return;run.Expires=m.Expires;
  if(Grid==m.WarpId&&m.Definition==RelayDefinitionL&&Vector3.Distance(Player.Position,new Vector3(9000,0,0))<=2000)run.Emit("near:relay");
  run.Pump(Now);
  if(Grid!=m.WarpId)return;
  foreach(string action in run.Effects.ToArray()){
   if(action.StartsWith("legacy/")){var bits=action.Split('/');int p=int.Parse(bits[1]),w=int.Parse(bits[2]);if(Grid==m.WarpId&&m.Pocket==p)SpawnLegacyWaveL(m,p,w);}
   else if(action.StartsWith("m/")){SpawnContentM(m,action);}
   else if(action.StartsWith("relay/")){if(Grid==m.WarpId)SpawnRelayL(m,action);}
  }run.Effects.Clear();
  if(run.Fired.Contains("done"))m.ObjectiveDone=true;
  if(run.State=="Expired"&&m.State=="Accepted"){UpdateMissions38(Now);}
 }
 void SpawnLegacyWaveL(Mission38 m,int pocket,int wave)
 {
  var run=m.GraphL!;m.Wave=Math.Max(m.Wave,wave);int count=m.Spec.Waves[wave];var sources=m.Spec.EnemyWaves.ElementAtOrDefault(wave)?.SelectMany(e=>Enumerable.Repeat(e,e.Count)).ToArray()??[];
  for(int i=0;i<count;i++){
   string id=$"{m.WarpId}:{pocket}:{wave}:{i}";if(m.Kills.Contains(id)||run.Generated.Contains(id)||Targets.Any(t=>t.Id==id))continue;
   var source=sources.ElementAtOrDefault(i);string hull=(source?.Hull??"Frigate").ToLowerInvariant();if(!new[]{"frigate","destroyer","cruiser","battlecruiser","battleship"}.Contains(hull))hull="frigate";
   var t=MakeRaider(new(3000+i*1400,300*(i%2),i*850));t.Id=id;t.Name=source?.Name.Length>0?source.Name:"Contract hostile "+(i+1);t.Profile=new Ship();t.Profile.SetCombatHull(hull);float hp=Math.Max(60*MathF.Pow(3,m.Spec.Level-1),source!=null?t.Profile.MaxHull*.5f:0);t.Shield=hp;t.Armor=hp*.6f;t.Hull=hp*.5f;t.Profile.SetStat("shieldCapacity",t.Shield);t.Profile.SetStat("armorHP",t.Armor);t.Profile.SetStat("hp",t.Hull);t.Signature=(float)t.Profile.Stat("signatureRadius");
   string ew=i==0?InferEwarL(m.Spec.SourceEwar):"";var state=new NpcStateL{Role=ew.Length>0?"ewar":"gunner",Ewar=ew,Hull=hull,Group=pocket+"/"+wave};
   if(m.Spec.Objective=="Structure"&&i==0){t.Name="Hostile installation";t.Signature=3000;state.Role="structure";}
   run.Npcs[id]=state;PrepareNpcL(t,state);run.Generated.Add(id);Targets.Add(t);
  }
 }
 static string InferEwarL(string source){string s=source.ToLowerInvariant();return s.Contains("scram")?"Scram":s.Contains("web")?"Web":s.Contains("neut")?"Neutralizer":s.Contains("jam")||s.Contains("ecm")?"ECM":s.Contains("damp")?"Damp":s.Contains("tracking")?"Tracking":"";}
 void SpawnRelayL(Mission38 m,string action)
 {
  var run=m.GraphL!;if(action=="relay/1"){if(!run.Events.Contains("collect")&&!Targets.Any(t=>t.Id==m.WarpId+":objective"))AddObjectiveL(m);return;}
  int first=action.EndsWith("alarm")?4:0,last=action.EndsWith("alarm")?5:4;
  string[] names=["Relay commander","Relay tackle escort","Relay armor tender","Relay missile escort","Relay capacitor hunter"];
  for(int i=first;i<last;i++){string id=m.WarpId+":0:0:"+i;if(run.Generated.Contains(id)||m.Kills.Contains(id)||Targets.Any(t=>t.Id==id))continue;var t=MakeRaider(new(3500+i*750,150*i,300*i));t.Id=id;t.Name=names[i];var s=new NpcStateL{Role=i==2?"logistics":i==3?"missile":i==1||i==4?"ewar":"gunner",Ewar=i==1?"Scram":i==4?"Neutralizer":"",Group="relay",Bounty=i==0?90:35};run.Npcs[id]=s;PrepareNpcL(t,s);run.Generated.Add(id);Targets.Add(t);}
 }
 public void PrepareNpcL(SpaceTarget target,NpcStateL role,bool reset=true)
 {
  var ship=target.Profile??=Ship.RaiderProfile();target.TacticalProvisioned=true;target.Training=false;
  if(!reset&&ship.Weapons.Length>0&&target.NpcAmmunition.Count>0)return;
  if(role.Role is "logistics" or "structure")ship.Weapons=[];else{
   var weapon=role.Role=="missile"?WeaponCatalog.All.Where(w=>w.Kind==WeaponKind.Missile&&w.Size<=1).OrderBy(w=>w.Id).First():WeaponCatalog.All.First(w=>w.ReferenceName=="150mm Railgun");
   ship.Weapons=[WeaponState.Create(weapon.Id)];var module=ship.Weapons[0];module.Loaded=module.Definition.Magazine;target.NpcAmmunition[module.ChargeId]=120;
  }
  ship.Ewar=role.Ewar.Length>0&&Enum.TryParse<EwarKind>(role.Ewar,out var kind)?[EwarState.Create(EwarCatalog.All.Where(e=>e.Kind==kind).OrderBy(e=>e.Size).First().Id)]:[];
  if(role.Role=="logistics"){var d=SupportCatalog.All.Where(s=>s.Kind=="Armor").OrderBy(s=>s.Size).ThenBy(s=>s.Id).First();ship.Support=[new SupportState{ModuleId=d.Id}];}else ship.Support=[];
  ship.UtilityFit="";target.Cap=Math.Min(target.Cap,ship.MaxCap);target.LockId="";target.LockProgress=0;
 }
 public string AdvanceMissionGateL(string id,bool back=false)
 {
  var m=AgentsState.Missions.FirstOrDefault(m=>m.Id==id);if(m==null||m.State!="Accepted"||Grid!=m.WarpId)return "Enter the accepted mission site first.";var run=EnsureMissionL(m);PumpMissionL(m);
  var gate=run.Definition.Gates.FirstOrDefault(g=>g.From==run.Pocket&&(back?g.Id.StartsWith("back"):!g.Id.StartsWith("back")));if(gate==null)return "No gate in that direction.";
  if(!run.GateReady(gate))return "Gate locked: "+gate.Text;if(Commander.Capsule||CombatHulls.Get(Player.CombatHull).Size>gate.MaxSize)return "This hull is not admitted by the gate.";
  var target=Targets.FirstOrDefault(t=>t.Id==m.WarpId+(back?":backgate":":gate"));if(target==null||Vector3.Distance(Player.Position,target.Position)>gate.Range)return "Approach the acceleration gate within "+gate.Range.ToString("N0")+" m.";
  if(!CanUseGate("player"))return "Propulsion or docking interlock prevents gate activation.";string physicalKey=gate.Key.StartsWith("item:")?gate.Key[5..]:"";if(physicalKey.Length>0&&StackCount(physicalKey,false)<1)return "Gate key unavailable in cargo.";
  SyncMissionL();if(gate.ConsumeKey){if(physicalKey.Length>0)ChangeStack(physicalKey,false,-1);else run.Events.Remove("key:"+gate.Key);}run.Pocket=gate.To;m.Pocket=gate.To;m.Wave=0;if(!run.Entered.ContainsKey(gate.To))run.Entered[gate.To]=Now;EnterMissionL(m);return "";
 }
 public bool MissionKillL(SpaceTarget target)
 {
  var m=ActiveMissionL;if(m==null||!target.Id.StartsWith(m.WarpId+":"))return false;var run=EnsureMissionL(m);if(!m.Kills.Add(target.Id))return true;run.Emit("kill:"+target.Id);MissionEventsM(m,target.Id);PumpMissionL(m);SyncMissionL();return true;
 }
 public bool SealMissionKillL(SpaceTarget target)
 {
  var m=AgentsState.Missions.FirstOrDefault(m=>target.Id.StartsWith(m.WarpId+":"));if(m==null)return false;var run=EnsureMissionL(m);if(m.State!="Accepted")return true;
  if(!run.Drops.TryGetValue(target.Id,out var drop)){drop=new(){Target=target.Id,Pocket=m.Pocket,Position=target.Position,Salvage=2+(int)(run.Roll()*4),Loot=new(){{"ammo",1+(int)(run.Roll()*8)}}};run.Drops[target.Id]=drop;}
  if(!Targets.Any(t=>t.Id=="wreck_"+target.Id))Targets.Add(new(){Id="wreck_"+target.Id,Name=target.Name+" wreck",Kind=TargetKind.Wreck,Position=drop.Position,OwnerId=drop.Owner,Contents=new(drop.Loot),Salvage=drop.Salvage});
  SettleBountyL(run,drop,target,run.Npcs.GetValueOrDefault(target.Id)?.Bounty??70);return true;
 }
 public string CollectMissionL(Mission38 m)
 {
  var run=EnsureMissionL(m);PumpMissionL(m);bool eligible=m.ProfileM?.Mode=="Retrieve"|| (m.Definition==RelayDefinitionL?run.Pocket==1:m.ObjectiveDone);
  if(m.State!="Accepted"||Grid!=m.WarpId||m.Spec.Objective!="Retrieve"||!eligible)return "Secure the mission objective first.";
  var target=Targets.FirstOrDefault(t=>t.Id==m.WarpId+":objective"&&t.Alive);if(target==null||run.Events.Contains("collect")||Vector3.Distance(Player.Position,target.Position)>2500)return "Approach the mission objective container within 2,500 m.";
  if(CargoRoom("mission-freight")+0.001f<m.Spec.Volume)return "Cargo hold full.";m.Cargo+=m.Spec.Volume;target.Destroyed=true;run.Emit("collect");PumpMissionL(m);SyncMissionL();return "";
 }
 public string RequestRelayL()
 {
  if(!Docked)return "Dock at a station with an available security agent.";var agent=WorldAgents44.FirstOrDefault(a=>a.Station==StationKey44&&a.Level==1&&a.Division==AgentDivision38.Security);if(agent==null||!CanUseAgent38(agent))return "No eligible security agent here.";
  if(AgentsState.Missions.Any(m=>m.Agent==agent.Id&&m.State is "Offered" or "Accepted"))return "This agent has an outstanding offer or mission.";
  var m=new Mission38{Agent=agent.Id,Definition=RelayDefinitionL,Pickup=StationKey44,Destination=StationKey44,SiteSystem=SystemId,Offered=Now,Expires=Now.AddDays(7),Reward=240,Bonus=120,LP=40};AgentsState.Missions.Add(m);EnsureMissionL(m);return "";
 }
 public string SetMissionSharingL(string id,bool enabled)
 {
  var m=AgentsState.Missions.FirstOrDefault(m=>m.Id==id);if(m==null||m.State is not("Offered" or "Accepted")||!Docked)return "Set reward sharing while docked with an open contract.";EnsurePopulationC();var run=EnsureMissionL(m);run.ShareRewards=enabled;run.SharePilots=enabled?Population.Pilots.Where(p=>p.Contracted&&!p.Suspended&&p.Status!="Lost"&&p.System==m.SiteSystem).Select(p=>p.Id).Distinct().OrderBy(s=>s).ToList():[];return "";
 }
 void SettleMissionL(Mission38 m,DateTime now)
 {
  var run=EnsureMissionL(m);if(run.Settlement!=null)return;decimal total=m.Reward+(now<=m.BonusUntil?m.Bonus:0);var crew=run.ShareRewards?Population.Pilots.Where(p=>run.SharePilots.Contains(p.Id)&&p.Contracted&&!p.Suspended&&p.Status!="Lost"&&p.System==m.SiteSystem).OrderBy(p=>p.Id).ToArray():[];
  var settlement=new SettlementL{Id="mission/"+m.Id,Time=now,Total=total,LP=m.LP,Corporation=m.Contact.Corporation};run.Settlement=settlement;
  decimal each=Math.Floor(total*100/(crew.Length+1))/100;int lp=m.LP/(crew.Length+1);double agent=.02*m.Spec.Level/(crew.Length+1),corp=.01*m.Spec.Level/(crew.Length+1);
  foreach(var p in crew){p.Wallet+=each;Population.ExternalNet+=each;settlement.Money[p.Id]=each;settlement.Loyalty[p.Id]=lp;p.MissionLoyaltyL[m.Contact.Corporation]=p.MissionLoyaltyL.GetValueOrDefault(m.Contact.Corporation)+lp;foreach(var pair in new[]{(m.Agent,agent),(m.Contact.Corporation,corp)}){double before=p.MissionStandingsL.GetValueOrDefault(pair.Item1);p.MissionStandingsL[pair.Item1]=Math.Clamp(before+(10-before)*pair.Item2,-10,10);}}
  settlement.Money["player"]=total-each*crew.Length;settlement.Loyalty["player"]=m.LP-lp*crew.Length;WalletChange(settlement.Money["player"],"Agent mission: "+m.Spec.Name);AgentsState.Loyalty[m.Contact.Corporation]=AgentsState.Loyalty.GetValueOrDefault(m.Contact.Corporation)+settlement.Loyalty["player"];
  ChangeStanding38(m.Agent,agent,"Mission completed");ChangeStanding38(m.Contact.Corporation,corp,"Mission completed");settlement.Standings[m.Agent]=agent;settlement.Standings[m.Contact.Corporation]=corp;
 }
 void RecordEncounterContributionL(SpaceTarget target,string source,float amount)
 {
  if(amount<=0)return;var run=ActiveMissionL?.GraphL??(ActiveEncounterF is {State:"Active"} e?EnsureExplorationGraphL(e):null);if(run==null)return;
  if(!run.Contributions.TryGetValue(target.Id,out var entries))run.Contributions[target.Id]=entries=[];entries[source]=entries.GetValueOrDefault(source)+amount;
 }
 void SettleBountyL(GraphRunL run,DropSealL drop,SpaceTarget target,decimal total)
 {
  if(drop.BountyPaid)return;drop.BountyPaid=true;var damage=run.Contributions.GetValueOrDefault(target.Id);if(damage==null||damage.Count==0){drop.Owner="unclaimed";return;}
  var eligible=Population.Pilots.Where(p=>run.SharePilots.Contains(p.Id)&&p.Contracted&&!p.Suspended&&p.Status!="Lost"&&p.System==SystemId).OrderBy(p=>p.Id).ToArray();
  bool playerCredit=damage.ContainsKey("player")||eligible.Any(p=>damage.ContainsKey(p.Id)||damage.ContainsKey("npc:"+p.Id));drop.Owner=playerCredit?"player":damage.OrderByDescending(p=>p.Value).First().Key;
  var wreck=Targets.FirstOrDefault(t=>t.Id=="wreck_"+target.Id);if(wreck!=null)wreck.OwnerId=drop.Owner;if(!playerCredit)return;
  var crew=run.ShareRewards?eligible:[];decimal each=Math.Floor(total*100/(crew.Length+1))/100;foreach(var p in crew){p.Wallet+=each;Population.ExternalNet+=each;drop.BountyShares[p.Id]=each;}drop.BountyShares["player"]=total-each*crew.Length;WalletChange(drop.BountyShares["player"],"NPC bounty: "+target.Name);
 }
 public bool MissionNpcActiveL(SpaceTarget t)=>!t.Id.StartsWith("structure-q:")&&!t.Id.StartsWith("den-p:")&&!(ActiveEncounterF?.RunP!=null&&ActiveEncounterF.GraphL?.Npcs.GetValueOrDefault(t.Id)?.Role=="structure")&&!(ActiveEncounterF?.ProfileO!=null&&ActiveEncounterF.GraphL?.Npcs.GetValueOrDefault(t.Id)?.Role=="structure")&&!SafeResidentHullO()&&(ActiveMissionL?.GraphL?.Npcs.GetValueOrDefault(t.Id)?.Aggro??true);
 void WakeMissionGroupL(SpaceTarget t){var run=ActiveMissionL?.GraphL;if(run==null||!run.Npcs.TryGetValue(t.Id,out var role))return;foreach(var other in run.Npcs.Values.Where(n=>n.Group==role.Group))other.Aggro=true;}
 string? MissionAggroL(SpaceTarget t)=>(ActiveMissionL?.GraphL??(ActiveEncounterF?.RunP!=null?ActiveEncounterF.GraphL:null))?.Npcs.GetValueOrDefault(t.Id)?.AggroTarget is {Length:>0} id?id:null;
 public void UpdateMissionsL(float dt)
 {
  var m=ActiveMissionL;if(m!=null){PumpMissionL(m);UpdateMissionNpcsL(m,dt);}else if(ActiveEncounterF is {State:"Active"} e)UpdateGraphNpcsL(EnsureExplorationGraphL(e),dt);
 }
 public void UpdateMissionNpcsL(Mission38 m,float dt)
 {
  UpdateGraphNpcsL(EnsureMissionL(m),dt);
 }
 public void UpdateGraphNpcsL(GraphRunL run,float dt)
 {
  foreach(var t in Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&run.Npcs.ContainsKey(t.Id)).ToArray()){
   var role=run.Npcs[t.Id];if(t.Profile==null||!role.Aggro||role.Role=="structure")continue;role.ThinkLeft=Math.Max(0,role.ThinkLeft-dt);
   if(role.ThinkLeft==0){role.ThinkLeft=2;var threats=CombatEvents.Where(e=>e.Target==t.Id&&e.Applied>0).TakeLast(20).GroupBy(e=>e.Source).OrderByDescending(g=>g.Sum(e=>e.Applied)).Select(g=>g.Key).ToArray();string desired=threats.FirstOrDefault(id=>ShipAlive(id)&&CanLock(t.Id,id))??ChooseOperationsVictimP(t);if(desired!=role.AggroTarget){role.AggroTarget=desired;t.LockId="";t.LockProgress=0;}}
   role.EwarLeft=Math.Max(0,role.EwarLeft-dt);foreach(var state in t.Profile.Ewar){var d=state.Definition;if(d==null||!state.Online||state.Burnt||role.EwarLeft>0||t.LockId.Length==0||!CanLock(t.Id,t.LockId))continue;float range=Vector3.Distance(t.Position,At(t.LockId));if(RangeScale(range,d.Optimal,d.Falloff)<=0||t.Cap<d.Capacitor)continue;t.Cap-=d.Capacitor;ApplyEwarCycle(d,t.Id,t.LockId,"mission-l/"+state.ModuleId);role.EwarLeft=d.Cycle;CombatRecord(t.Id,t.LockId,d.Name,d.Strength,d.Strength);}
   if(role.Role=="logistics")UpdateMissionLogisticsL(t,role,dt,run);
  }
 }
 void UpdateMissionLogisticsL(SpaceTarget tender,NpcStateL role,float dt,GraphRunL run)
 {
  var state=tender.Profile!.Support.FirstOrDefault();var d=state?.Definition;if(d==null||!state!.Online||state.Burnt)return;
  if(role.PendingRepair>0){role.RepairLeft=Math.Max(0,role.RepairLeft-dt);if(role.RepairLeft>0)return;var recipient=Targets.FirstOrDefault(t=>t.Id==role.RepairTarget&&t.Alive);if(recipient!=null&&CanLock(tender.Id,recipient.Id)&&Vector3.Distance(tender.Position,recipient.Position)<=d.Optimal+d.Falloff*3){float before=recipient.Armor;RestoreSupportLayer(recipient,d,role.PendingRepair);CombatRecord(tender.Id,recipient.Id,"Remote armor repair",role.PendingRepair,recipient.Armor-before);}role.PendingRepair=0;role.RepairTarget="";return;}
  var ally=Targets.Where(t=>t.Id!=tender.Id&&t.Alive&&t.Kind==TargetKind.Raider&&run.Npcs.TryGetValue(t.Id,out var other)&&other.Group==role.Group&&t.Armor<(t.Profile?.MaxArmor??70)&&CanLock(tender.Id,t.Id)&&Vector3.Distance(tender.Position,t.Position)<=d.Optimal+d.Falloff*3).OrderBy(t=>t.Armor/Math.Max(1,t.Profile?.MaxArmor??70)).FirstOrDefault();
  if(ally==null||tender.Cap<d.Capacitor)return;tender.Cap-=d.Capacitor;role.PendingRepair=d.Amount*RangeScale(Vector3.Distance(tender.Position,ally.Position),d.Optimal,d.Falloff);role.RepairTarget=ally.Id;role.RepairLeft=d.Cycle;
 }
}
