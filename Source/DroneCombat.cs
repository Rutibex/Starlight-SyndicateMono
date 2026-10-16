using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 IEnumerable<DroneUnit> DroneFlight(string id="")=>Player.Drones.Where(u=>u.Owner=="player"&&u.Location=="Space"&&(id.Length==0||id=="All"||u.Id==id||u.Group==id));
 public bool DroneCommand(string order,string id="")
 {
  if(Cloaked||Player.HasSpecial("Triage")||Player.NexusLeft>0&&order is not ("Return" or "Orbit"))return false;
  bool done=false;foreach(var u in DroneFlight(id).ToArray())
  {
   var d=EffectiveDrone(u);var b=Targets.FirstOrDefault(t=>t.Id==u.Id);if(b==null)continue;
   if(order is "Return" or "Orbit"){u.Order=order;u.AssistId="";done=true;continue;}
   if(order=="Abandon"){SyncDrone(u);u.Location="Abandoned";u.Order="Idle";u.AssistId="";done=true;continue;}
   if(order=="Move"&&d.Fighter){u.MovePoint=Player.ManualPoint;u.Order="Move";done=true;continue;}
   if(Vector3.Distance(b.Position,Player.Position)>(d.Fighter?240000:Player.Stat("droneControlDistance")))continue;
   var target=Selected;
   if(order is "Assist" or "Guard"){if(target?.Friendly==true&&target.DroneId.Length==0&&!d.Fighter&&d.Packet.Total>0){u.AssistId=target.Id;u.AssistMode=order;u.Order="Orbit";done=true;}continue;}
   if(d.Kind=="Salvage"&&target?.Kind!=TargetKind.Wreck)target=Targets.FirstOrDefault(t=>t.Kind==TargetKind.Wreck&&t.Alive);
   bool valid=target!=null&&(d.Harvest?target.Kind==TargetKind.Asteroid&&target.Resource==(d.Kind=="Ice"?"ice":"ore"):d.Kind=="Salvage"?target.Kind==TargetKind.Wreck:d.RepairDrone?target.Kind==TargetKind.Raider&&target.Id!="player"&&target.DroneId!=u.Id:target.Kind==TargetKind.Raider&&target.Id!=u.Id&&(!WeaponSafety||!target.Friendly));
   if(!valid||target==null||Vector3.Distance(target.Position,Player.Position)>(d.Fighter?240000:Player.Stat("droneControlDistance"))||(target.Kind==TargetKind.Raider&&LockedId!=target.Id))continue;
   u.TargetId=target.Id;u.Order=d.Harvest?"Mine":d.Kind=="Salvage"?"Salvage":d.RepairDrone?"Repair":"Attack";u.MineOnce=order=="MineOnce";u.AssistId="";u.AttackOn=true;RevealEnhancedLock(target.Id);u.CycleLeft=0;u.HarvestCycling=false;u.SalvageCycling=false;done=true;
  }if(!done)Say("No compatible craft/locked target in control range.");return done;
 }
 public bool MoveFighter(string id,Vector3 point){var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Owner=="player"&&u.Location=="Space"&&u.Definition.Fighter);if(u==null||!float.IsFinite(point.X)||!float.IsFinite(point.Y)||!float.IsFinite(point.Z))return false;u.MovePoint=point;u.Order="Move";u.AssistId="";return true;}
 public bool FighterAbility(string id,int slot)
 {
  var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Owner=="player"&&u.Location=="Space");if(u==null||!u.Definition.Fighter||slot<0||slot>=u.Definition.Abilities.Length||u.AbilityCooldowns[slot]>0||u.AbilityReloads[slot]>0||u.RearmLeft>0)return false;
  var a=EffectiveDrone(u).Abilities[slot];if(a.Charges>0&&u.AbilityCharges[slot]==0)return false;var b=Targets.First(t=>t.Id==id);
  if(slot==0){u.AttackOn=!u.AttackOn;if(u.AttackOn&&u.Order!="Attack")DroneCommand("Engage",id);return true;}
  if(a.Kind=="MWD"&&!CanUseMicroDrive(id))return false;
  if(a.Kind is "Salvo" or "Bomb" or "Sacrifice" or "Tackle")
  {
   var t=Locked;if(t==null||t.Kind!=TargetKind.Raider||t.Id==u.Id||WeaponSafety&&t.Friendly||Vector3.Distance(b.Position,t.Position)>a.Range||!CanLock(id,t.Id))return false;
   if(a.Kind=="Tackle") {if(t.DroneId.Length==0)return false;ApplyEwarCycle(new(){Id="drone_tackle",Name=a.Name,Kind=EwarKind.Point,Strength=1,Optimal=a.Range,Duration=10},id,t.Id,id+"_point");ApplyEwarCycle(new(){Id="drone_tackle_web",Name=a.Name,Kind=EwarKind.Web,Strength=.45f,Optimal=a.Range,Duration=10},id,t.Id,id+"_web");}
   else if(a.Kind=="Sacrifice"){if(Vector3.Distance(b.Position,t.Position)>600)return false;ApplyPacket(t,a.Packet.Scale(u.Count));u.Count=0;u.Location="Destroyed";Targets.Remove(b);}
   else {var aim=t.Position-b.Position;MissilesInFlight.Add(new(){Owner=id,Position=b.Position,TargetId=t.Id,Direction=aim.LengthSquared()>0?Vector3.Normalize(aim):Vector3.UnitX,Bomb=a.Kind=="Bomb",Life=a.Kind=="Bomb"?6:Math.Max(3,a.Range/1800),Speed=1800,Radius=a.Kind=="Bomb"?6000:0,Packet=a.Packet.Scale(u.Count),Damage=a.Packet.Total*u.Count,ExplosionRadius=a.ExplosionRadius,ExplosionVelocity=a.ExplosionVelocity,Enemy=false,Hp=15});}
  }
  else if(a.Kind=="Evade")u.EvasionLeft=20;
  else if(a.Kind=="MJD"){if(!CanUseMicroDrive(id))return false;u.JumpLeft=8;u.MovePoint=Player.ManualPoint;}
  else u.BoostLeft=20;
  u.AbilityCooldowns[slot]=a.Cooldown;if(a.Charges>0)u.AbilityCharges[slot]--;return true;
 }
 public bool RearmFighter(string id)
 {
  var u=Player.Drones.FirstOrDefault(u=>u.Id==id&&u.Definition.Fighter&&u.Location is "Bay" or "Space");if(u==null)return false;bool done=false;for(int i=0;i<u.Definition.Abilities.Length;i++){var a=u.Definition.Abilities[i];if(a.Charges>0&&u.AbilityCharges[i]<a.Charges&&u.AbilityReloads[i]==0){u.AbilityReloads[i]=Math.Max(1,a.Rearm);done=true;}}return done;
 }
 void DroneLoss(SpaceTarget b)
 {
  var u=Player.Drones.FirstOrDefault(u=>u.Id==b.DroneId);if(u==null){b.KillRecorded=true;return;}
  if(u.Count>1){var health=DroneIntegrities(u).Skip(1).ToList();SetDroneIntegrities(u,health);b.Shield=u.Shield;b.Armor=u.Armor;b.Hull=u.Hull;b.KillRecorded=false;Say(u.Definition.Name+" lost one fighter / "+u.Count+" remain.");}
  else {u.Count=0;u.Location="Destroyed";u.Hull=0;b.KillRecorded=true;Say(u.Definition.Name+" lost.");}
 }
 void DroneEwar(DroneUnit u,SpaceTarget t)
 {
  var d=EffectiveDrone(u);if(d.Effect.Length==0)return;var kind=Enum.Parse<EwarKind>(d.Effect);ApplyEwarCycle(new(){Id=d.Id,Name=d.Name,Kind=kind,Optimal=d.Optimal,Falloff=d.Falloff,Duration=Math.Max(d.Cycle,5)*Player.HullFactor("drone","duration",d.Effect),Strength=d.Strength*u.Count,Amount=d.Effect=="Neutralizer"?d.Strength*u.Count:0,Sensor="All"},u.Id,t.Id,u.Id);
 }
 void UpdateDrones(float dt)
 {
  foreach(var u in Player.Drones.Where(u=>u.Location=="Bay"&&u.Definition.Fighter)){for(int i=0;i<u.Definition.Abilities.Length;i++){u.AbilityCooldowns[i]=Math.Max(0,u.AbilityCooldowns[i]-dt);if(u.AbilityReloads[i]>0){u.AbilityReloads[i]=Math.Max(0,u.AbilityReloads[i]-dt);if(u.AbilityReloads[i]==0)u.AbilityCharges[i]=u.Definition.Abilities[i].Charges;}}}

  foreach(var u in Player.Drones.Where(u=>u.Location is "Space" or "Abandoned"&&u.System==SystemId&&u.Grid==DroneGridIdentity||u.Owner!="player"&&u.Location=="Space").ToArray())
  {
   var b=Targets.FirstOrDefault(t=>t.Id==u.Id&&t.Alive);if(b==null)continue;var d=EffectiveDrone(u);
   u.RearmLeft=Math.Max(0,u.RearmLeft-dt);u.BoostLeft=Math.Max(0,u.BoostLeft-dt);u.EvasionLeft=Math.Max(0,u.EvasionLeft-dt);if(EwarFor(u.Id).Scrammed)u.BoostLeft=0;
   b.Signature=d.Signature*(u.EvasionLeft>0?.5f:u.BoostLeft>0?3:1);
   for(int i=0;i<3;i++){u.AbilityCooldowns[i]=Math.Max(0,u.AbilityCooldowns[i]-dt);if(u.AbilityReloads[i]>0){u.AbilityReloads[i]=Math.Max(0,u.AbilityReloads[i]-dt);if(u.AbilityReloads[i]==0)u.AbilityCharges[i]=d.Abilities[i].Charges;}}
   if(u.JumpLeft>0){u.JumpLeft=Math.Max(0,u.JumpLeft-dt);if(u.JumpLeft==0&&CanUseMicroDrive(u.Id)){var delta=u.MovePoint-b.Position;if(delta.LengthSquared()>1)b.Position+=Vector3.Normalize(delta)*10000;}}
   b.Shield=Math.Min(d.Shield,TacticalRulesB.RechargeAfter(b.Shield,d.Shield,d.ShieldRecharge,dt));
   if(u.Location=="Abandoned"||u.Owner!="player"){SyncDrone(u);continue;}
   var ew=EwarFor(u.Id);var t=Targets.FirstOrDefault(t=>t.Id==u.TargetId&&t.Alive);u.CycleLeft=Math.Max(0,u.CycleLeft-dt);
   if(u.AssistId.Length>0){var friend=Targets.FirstOrDefault(x=>x.Id==u.AssistId&&x.Alive);var enemy=friend==null?null:u.AssistMode=="Assist"?Targets.FirstOrDefault(x=>x.Id==friend.LockId&&x.Alive&&!x.Friendly):Targets.FirstOrDefault(x=>x.LockId==friend.Id&&x.Alive&&!x.Friendly);if(enemy!=null){u.TargetId=enemy.Id;u.Order="Attack";t=enemy;}}
   if(Player.DroneAggressive&&u.Order=="Orbit"&&d.Packet.Total>0){var enemies=Targets.Where(x=>x.Alive&&!x.Friendly&&x.Kind==TargetKind.Raider&&x.LockId=="player"&&Vector3.Distance(x.Position,Player.Position)<Player.Stat("droneControlDistance"));var enemy=Player.DroneFocusFire?enemies.FirstOrDefault():enemies.OrderBy(x=>Player.Drones.Count(other=>other.Order=="Attack"&&other.TargetId==x.Id)).FirstOrDefault();if(enemy!=null){u.TargetId=enemy.Id;u.Order="Attack";t=enemy;}}
   if(t==null&&u.Order=="Salvage"){t=Targets.FirstOrDefault(x=>x.Kind==TargetKind.Wreck&&x.Alive);if(t!=null){u.TargetId=t.Id;u.SalvageCycling=false;u.CycleLeft=0;}}
   if(t==null&&u.Order is "Attack" or "Mine" or "Repair" or "Salvage")u.Order="Orbit";
   Vector3 goal=Player.Position;float speed=d.Speed*ew.Speed*(u.BoostLeft>0?4:u.EvasionLeft>0?1.6f:1);
   if(u.Order=="Move")goal=u.MovePoint;
   else if(u.Order is "Attack" or "Mine" or "Repair" or "Salvage"&&t!=null){var direction=b.Position-t.Position;goal=t.Position+(direction.LengthSquared()>1?Vector3.Normalize(direction):Vector3.UnitX)*Math.Min(d.Orbit,d.Optimal*.6f);if(u.Order=="Attack"&&d.Packet.Total>0)goal+=new Vector3(0,0,Math.Min(d.Orbit,1000));}
   else if(u.Order=="Orbit")goal=Player.Position+new Vector3(MathF.Cos((float)Day*100+u.Id.GetHashCode()%360)*500,80,MathF.Sin((float)Day*100+u.Id.GetHashCode()%360)*500);
   if(u.Order=="Return"&&d.Fighter&&Vector3.Distance(b.Position,Player.Position)>50000&&!WarpBlocked(u.Id)){if(u.ReturnWarpLeft==0)u.ReturnWarpLeft=5;u.ReturnWarpLeft=Math.Max(.0001f,u.ReturnWarpLeft-dt);if(u.ReturnWarpLeft<=.0001f){b.Position=Player.Position+new Vector3(200,0,0);u.ReturnWarpLeft=0;}}
   var deltaMove=goal-b.Position;Vector3 desired=deltaMove.Length()>80?Vector3.Normalize(deltaMove)*Math.Min(speed,deltaMove.Length()/Math.Max(dt,.01f)):Vector3.Zero;
   if(d.Kind!="Sentry"){b.Velocity=Vector3.Lerp(b.Velocity,desired,Math.Min(1,dt/Math.Max(.1f,d.Mass*d.Agility/1000000)));b.Position+=b.Velocity*dt;}
   if(u.Order is "Return" or "Deliver"&&Vector3.Distance(b.Position,Player.Position)<500)
   {
    if(u.Payload>0&&u.MissionPayload38.Length>0){var m=AgentsState.Missions.FirstOrDefault(m=>m.Id==u.MissionPayload38&&m.State=="Accepted");if(m==null){u.Payload=0;u.MissionPayload38="";}else{float moved=Math.Min(u.Payload,CargoRoom(m.Spec.Resource));m.Cargo+=moved;u.Payload-=moved;if(u.Payload>0){u.Order="Orbit";continue;}u.MissionPayload38="";}}
    if(u.Payload>0){float room=CargoRoom(d.Kind=="Ice"?"ice":"ore");float delivered=d.Kind=="Ice"?Math.Min(u.Payload,MathF.Floor(room/1000)*1000):Math.Min(u.Payload,room);if(delivered>0){if(d.Kind=="Ice")CargoItems["ice"]=CargoItems.GetValueOrDefault("ice")+(decimal)(delivered/1000);else Player.Ore+=delivered;u.Payload-=delivered;}if(u.Payload>0){u.Order="Orbit";Say("Cargo full: drone retains harvested payload.");continue;}}
    if(u.Order=="Return"){SyncDrone(u);u.Location="Bay";u.Order="Idle";Targets.Remove(b);continue;}u.Order=u.MineOnce?"Orbit":"Mine";
   }
   if(t!=null&&u.CycleLeft==0&&u.RearmLeft==0&&u.Order is "Attack" or "Repair" or "Mine" or "Salvage")
   {
    float range=Vector3.Distance(b.Position,t.Position);float reach=d.Optimal+d.Falloff*3;if(range<=reach&&(t.Kind!=TargetKind.Raider||CanLock(u.Id,t.Id)))
    {
     if(u.Order=="Mine")
     {
      // First arrival starts a full harvest cycle. Completion extracts only available resource.
      if(u.TargetId.EndsWith("#cycle")){u.TargetId=u.TargetId[..^6];}
      if(u.CycleLeft==0&&!u.HarvestCycling){u.HarvestCycling=true;u.CycleLeft=d.MiningCycle;continue;}
      u.HarvestCycling=false;float yield=d.Kind=="Ice"?t.Ore>=1000?1000:0:Math.Min(d.Yield,t.Ore);t.Ore-=yield;u.Payload+=yield;var contract=AgentsState.Missions.FirstOrDefault(m=>m.State=="Accepted"&&(t.Id==m.WarpId+":resource"||t.Id.StartsWith(m.WarpId+":resource:")));if(contract!=null){contract.Harvested+=yield;u.MissionPayload38=contract.Id;if(contract.Harvested+.001f>=contract.Spec.Volume)EnsureMissionL(contract).Emit("harvest");PumpMissionL(contract);SyncMissionL();}if(contract==null&&CombatRoll()<d.ResidueChance)t.Ore=Math.Max(0,t.Ore-yield*d.ResidueMultiplier);u.Order="Deliver";u.CycleLeft=0;
     }
     else if(u.Order=="Salvage"){if(!u.SalvageCycling){u.SalvageCycling=true;u.CycleLeft=d.Cycle;continue;}u.SalvageCycling=false;u.CycleLeft=0;if(CombatRoll()<Math.Clamp(d.Strength-t.SalvageDifficulty,.0f,1)){int contents=(int)t.Salvage;float room=Math.Max(0,Player.CargoMax-CargoUsed);int q=Math.Min(contents,(int)(room/.2f));CargoItems["salvage"]=CargoItems.GetValueOrDefault("salvage")+q;t.Salvage-=q;if(q==0)u.Order="Orbit";}}
     else if(u.Order=="Repair"){u.CycleLeft=d.Cycle;float amount=d.Repair*t.RemoteRepairMultiplier*(float)(t.Profile?.Stat("remoteRepairImpedance")??1);if(d.Kind=="ShieldRepair")t.Shield=Math.Min(t.Profile?.MaxShield??100,t.Shield+amount);else if(d.Kind=="ArmorRepair")t.Armor=Math.Min(t.Profile?.MaxArmor??70,t.Armor+amount);else t.Hull=Math.Min(t.Profile?.MaxHull??65,t.Hull+amount);}
     else if(u.AttackOn)
     {
      u.CycleLeft=d.Cycle;DroneEwar(u,t);if(d.Packet.Total>0){float application;if(d.Fighter&&d.AttackExplosionRadius>0)application=MissileApplication(EffectiveSignature(t),t.Velocity.Length(),d.AttackExplosionRadius*ew.ExplosionRadius,d.AttackExplosionVelocity*ew.ExplosionVelocity,.7f);else {var direction=range>1?(t.Position-b.Position)/range:Vector3.UnitX;float angular=Vector3.Cross(t.Velocity-b.Velocity,direction).Length()/Math.Max(1,range);float chance=TurretChance(new(){Optimal=d.Optimal*ew.TurretRange,Falloff=d.Falloff*ew.TurretRange,Tracking=d.Tracking*ew.Tracking,SignatureResolution=d.SignatureResolution},null,range,angular,EffectiveSignature(t));double roll=CombatRoll();application=roll<chance?(float)(roll<.01?3:.5+roll):0;}ApplyPacket(t,d.Packet.Scale(application*u.Count*(d.Kind=="Superiority"&&t.DroneId.Length==0?.1f:1)));WeaponVisuals.Add(new(){Start=b.Position,End=t.Position,Kind=WeaponKind.Turret,Remaining=.2f});}
     }
    }
   }
   SyncDrone(u);
  }
 }

 public void ApplyDroneTestDamage(string id,float raw){var t=Targets.First(x=>x.Id==id);ApplyPacket(t,DamagePacket.Single(raw,DamageType.Thermal),false);}
}
