using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class Wingmate
{
 public string System{get;set;}="";public Dictionary<string,float> UtilityCargo{get;set;}=[]; public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Name{get;set;}="";public string HullId{get;set;}="cruiser";public string Role{get;set;}="Escort";public string Order{get;set;}="Follow";public string TargetId{get;set;}="";public string Broadcast{get;set;}="";public bool Fleet{get;set;}=true;
 public float Shield{get;set;}public float Armor{get;set;}public float Hull{get;set;}public float Cap{get;set;}public float ActionLeft{get;set;}public List<SupportEffect> Effects{get;set;}=[];
}
public sealed partial class DemoWorld
{
 public List<Wingmate> Wingmates{get;set;}=[];
 public float SupportDockLock{get;set;}
 public float LogisticsRangeFactor(SupportDefinition d)=>Player.RemoteModeFactor("remoteRange",d.Kind,d.Size)*(Player.Specialist?1:(d.Repair||d.Kind=="Cap"?CombatHulls.Get(Player.CombatHull).Role is "Logistics"?4:CombatHulls.Get(Player.CombatHull).Role=="AdaptiveLogistics"?2.2f:1:1));
 public float LogisticsCapFactor(SupportDefinition d)=>Player.Specialist?1:d.Repair||d.Kind=="Cap"?CombatHulls.Get(Player.CombatHull).Role is "Logistics" or "AdaptiveLogistics"?.45f:1:1;
 public string FleetSelection{get;set;}="";
 public bool HireWingmate(string role)
 {
  if(!Docked||Wingmates.Count>=4||role is not ("Escort" or "Medic" or "Miner"))return false;
  float cost=WeaponTestMode?0:350;if(Credits<cost){Say("Wingmate contract costs 350 CR.");return false;}WalletChange(-(decimal)(cost),"Gameplay transaction");
  var profile=new Ship();profile.SetCombatHull("cruiser");var w=new Wingmate{Role=role,Name=role+" / "+new[]{"Lysa","Tern","Miro","Oriel"}[Wingmates.Count],Shield=profile.MaxShield,Armor=profile.MaxArmor,Hull=profile.MaxHull,Cap=profile.MaxCap};Wingmates.Add(w);FleetSelection=w.Id;Say(w.Name+" joins your flight.");return true;
 }
 public bool DismissWingmate(string id)
 {if(!Docked)return false;return Wingmates.RemoveAll(w=>w.Id==id)>0;}
 public bool RebuildWingmate(string id)
 {var w=Wingmates.FirstOrDefault(x=>x.Id==id);if(!Docked||w==null)return false;float cost=WeaponTestMode?0:120;if(Credits<cost)return false;WalletChange(-(decimal)(cost),"Gameplay transaction");var p=new Ship();p.SetCombatHull(w.HullId);w.Shield=p.MaxShield;w.Armor=p.MaxArmor;w.Hull=p.MaxHull;w.Cap=p.MaxCap;w.Order="Follow";w.Effects.Clear();return true;}
 public bool OrderWingmate(string id,string order)
 {
  var w=Wingmates.FirstOrDefault(x=>x.Id==id);if(w==null||w.Hull<=0)return false;
  if(order is "Attack" or "Mine"){if(Selected==null||order=="Attack"&&(Selected.Kind!=TargetKind.Raider||Selected.Friendly&&WeaponSafety)||order=="Mine"&&Selected.Kind!=TargetKind.Asteroid)return false;w.TargetId=Selected.Id;}
  w.Order=order;return true;
 }
 void SyncWingmates()
 {foreach(var w in Wingmates){var t=Targets.FirstOrDefault(t=>t.Id==w.Id);if(t==null)continue;w.Shield=t.Shield;w.Armor=t.Armor;w.Hull=t.Hull;w.Cap=t.Cap;if(t.Profile!=null)w.Effects=t.Profile.SupportEffects;}}
 void RestoreWingmates()
 {
  if(Docked||ActiveEncounterF is {FilamentO:>0,State:"Active"})return;int i=0;foreach(var w in Wingmates.Where(w=>w.Hull>0&&(w.System.Length==0||w.System==SystemId)))
  {if(Targets.Any(t=>t.Id==w.Id))continue;var p=new Ship();p.SetCombatHull(w.HullId);p.SupportEffects=w.Effects;p.Shield=w.Shield;p.Armor=w.Armor;p.Hull=w.Hull;p.Cap=w.Cap;Targets.Add(new(){Id=w.Id,Name=w.Name,Kind=TargetKind.Raider,Profile=p,Friendly=true,Training=false,LockId="",Position=Player.Position+new Vector3(350+i++*300,80,350),Signature=p.Signature,Shield=w.Shield,Armor=w.Armor,Hull=w.Hull,Cap=w.Cap});}
 }
 public bool FleetMember(string id)=>id=="player"||Population.Pilots.Any(p=>p.Id==id&&p.Contracted&&!p.Suspended&&p.Fit.Hull>0)||Wingmates.Any(w=>w.Id==id&&w.Fleet&&w.Hull>0);
 public void EnterSupportRange()
 {EnterWeaponRange();if(Grid!="range")return;Grid="supportrange";Targets.RemoveAll(t=>t.Friendly&&!Wingmates.Any(w=>w.Id==t.Id));if(Wingmates.Count==0){Screen=ViewMode.Station;HireWingmate("Escort");HireWingmate("Medic");Screen=ViewMode.Space;RestoreWingmates();}RestoreWingmates();foreach(var t in Targets)t.LockId="";Say("Fleet range: lock an ally for remote support, or pulse fleet commands. Simulate incoming damage from the Logistics window.");}
 public void SupportPressure(string id)
 {if(!WeaponTestMode||Docked)return;var t=Targets.FirstOrDefault(t=>t.Id==id&&t.Alive);if(t==null)return;t.Shield=Math.Max(1,t.Shield*.2f);t.Armor=Math.Max(1,t.Armor*.2f);t.Hull=Math.Max(1,t.Hull*.5f);t.Cap=Math.Max(1,t.Cap*.1f);SyncWingmates();}
 public string SupportActivationError(int slot)
 {
  if(slot<0||slot>=8)return "Invalid support socket.";var s=Player.Support[slot];var d=Player.HullSupport(s.Definition);if(d==null)return "Empty support socket.";
  if(!s.Online)return "Module offline.";
  if(ModuleUnderRepairB("support",s))return "Module is being repaired with nanites.";
  if(Cloaked)return "Decloak before activation.";if(Docked)return "Undock before activation.";if(s.Burnt)return "Repair this burnt module at a station.";if(s.ReloadLeft>0)return "Magazine reloading.";
  if(d.Kind=="Burst")return s.Loaded==0||s.Payload==null?"Load a command charge first.":Player.Cap<d.Capacitor?"Insufficient capacitor.":"";
  var t=Targets.FirstOrDefault(t=>t.Id==s.TargetId&&t.Alive);if(t==null||t.Kind!=TargetKind.Raider||!CanLock("player",t.Id)||LockedId!=t.Id)return "Lock another ship first.";
  if(!LegalAssistQ("player",t.Id,false))return "Safety blocks assistance with criminal consequences.";if(WeaponSafety&&!t.Friendly)return "Safety blocks assistance to hostile ships.";
  if(Vector3.Distance(Player.Position,t.Position)>(d.Optimal+d.Falloff*3)*LogisticsRangeFactor(d))return "Target outside support range.";
  if((t.Profile?.Stat("remoteAssistanceImpedance")??1)<=0)return "Target cannot receive remote assistance.";
  if(d.Kind=="Cap"&&(t.Profile?.Stat("remoteCapacitorImpedance")??1)<=0)return "Target cannot receive capacitor transfer.";
  if(d.Repair&&(t.Profile?.Stat("remoteRepairImpedance")??1)<=0)return "Target cannot receive remote repairs.";
  float cost=d.Kind=="AncShield"&&s.Loaded>0?0:d.Capacitor*Player.RigRemoteCap(d.Kind)*LogisticsCapFactor(d)*Player.RepairCadence(d.Kind,d.Size,true)/Player.RemoteModeFactor("remoteCycle",d.Kind,d.Size);
  return Player.Cap<cost?"Insufficient capacitor.":"";
 }
 public bool ToggleSupport(int slot)
 {
  if(slot<0||slot>=8)return false;var s=Player.Support[slot];if(s.Definition==null)return false;
  if(s.Active){s.Active=false;if(s.CycleLeft<=0){s.Ramp=0;if(s.AutoReload&&s.Loaded==0&&s.Payload?.Consumable==true)ReloadSupport(slot);}return true;}
  if(s.CycleLeft>0&&s.TargetId!=LockedId&&s.Definition.Kind!="Burst"){Say("Finish this cycle before retargeting.");return false;}
  if(s.TargetId!=LockedId)s.Ramp=0;s.TargetId=LockedId;var error=SupportActivationError(slot);if(error.Length>0){Say(error);return false;}
  s.Active=true;if(s.CycleLeft<=0)StartSupportCycle(slot);return true;
 }
 Dictionary<string,float> SupportBonuses(SupportState s,float scale)
 {
  var d=Player.HullSupport(s.Definition)!;var bonuses=new Dictionary<string,float>();string mode=s.Payload?.Mode??"Combined";
  foreach(var (key,amount) in d.Bonuses)
  {
   bool yes=mode=="Combined"||mode=="Range"&&key is "maxTargetRange" or "optimal" or "falloff"||mode=="Resolution"&&key=="scanResolution"||mode=="Strength"&&key=="sensorStrength"||mode=="Tracking"&&key=="tracking";
   if(yes)bonuses[key]=amount*scale*(mode=="Combined"?1:2);
  }return bonuses;
 }
 void StartSupportCycle(int slot)
 {
  var s=Player.Support[slot];var d=Player.HullSupport(s.Definition)!;string error=SupportActivationError(slot);if(error.Length>0){s.Active=false;s.Ramp=0;Say(error);return;}
  if(d.Kind!="Burst"&&!LegalAssistQ("player",s.TargetId)){s.Active=false;return;}float cadence=Player.RepairCadence(d.Kind,d.Size,true);float cost=d.Kind=="AncShield"&&s.Loaded>0?0:d.Capacitor*Player.RigRemoteCap(d.Kind)*LogisticsCapFactor(d)*cadence/Player.RemoteModeFactor("remoteCycle",d.Kind,d.Size);
  Player.Cap-=cost;s.Overheat&=d.HeatDamage>0;s.CycleLeft=d.Cycle*cadence*(s.Overheat?1+d.OverloadCycle*Player.ScopedOverheat("support",Ship.SupportFilter(d)):1);
  float amount=d.Amount*((d.Repair||d.Kind=="Cap")?Player.RemoteModeFactor("remoteAmount",d.Kind,d.Size)*Player.SpecialFactor(d.Kind is "Shield" or "AncShield"?"remoteShield":"remoteArmor"):1)*(1+s.Ramp)*(s.Overheat?1+d.OverloadAmount*Player.ScopedOverheat("support",Ship.SupportFilter(d)):1);
  if(d.Kind=="AncArmor"){int use=Math.Min(s.Loaded,d.ChargeUse);s.Loaded-=use;amount*=1+(d.PasteMultiplier-1)*use/Math.Max(1f,d.ChargeUse);}
  if(d.Kind=="AncShield"&&s.Loaded>0)s.Loaded--;
  if(d.Kind=="Burst")
  {
   s.Loaded--;SupportDockLock=Math.Max(SupportDockLock,60);var pulse=s.Payload!;
   foreach(var id in new[]{"player"}.Concat(Targets.Where(t=>t.Alive&&t.Kind==TargetKind.Raider&&t.DroneId.Length==0&&FleetMember(t.Id)&&Vector3.Distance(Player.Position,t.Position)<=d.Optimal*Player.HullFactor("burst","range")*(d.Family=="Expedition"?Player.SpecialFactor("burstRange"):1)).Select(t=>t.Id)))
   {
    var p=Profile(id);var values=new Dictionary<string,float>(pulse.Bonuses);float bonus=CombatHulls.Get(Player.CombatHull).Role=="Command"&&!(ShipBatch21.ById.TryGetValue(Player.CombatHull,out var bp)&&bp.Batch==28)?1.18f:CombatHulls.Get(Player.CombatHull).Role=="ExpeditionCommand"&&d.Family=="Expedition"?1.25f:1;
    foreach(var key in values.Keys.ToArray())values[key]*=Player.HullFactor("burst","strength",d.Family)*(IndustrialCore?Player.HullFactor("burst","coreStrength",d.Family):1)*bonus*(d.Family=="Expedition"?Player.SpecialFactor("miningBurst"):1);
    ApplySupportEffect(p,new(){Source="player",Key=pulse.Id,Name=pulse.Name,Burst=true,Left=d.BurstDuration*Player.HullFactor("burst","duration",d.Family)*(d.Family=="Expedition"&&CombatHulls.Get(Player.CombatHull).Role=="ExpeditionCommand"?2:1),Bonuses=values},id);
   }
   WeaponVisuals.Add(new(){Start=Player.Position,End=Player.Position+Vector3.UnitY*d.Optimal,Kind=WeaponKind.Smartbomb,Radius=d.Optimal,Remaining=.7f});Say(d.Name+" pulse / "+pulse.Name);
  }
  else
  {
   var t=Targets.First(t=>t.Id==s.TargetId);float scale=RangeScale(Vector3.Distance(Player.Position,t.Position),d.Optimal*LogisticsRangeFactor(d),d.Falloff*LogisticsRangeFactor(d));if(!d.Delayed)amount*=scale;
   if(d.Delayed){s.PendingAmount=amount;s.PendingTarget=t.Id;}
   else if(d.Repair)RestoreSupportLayer(t,d,amount);
   else if(d.Kind=="Cap"){float beforeP=t.Cap;SetCapAt(t.Id,t.Cap+amount*(float)(t.Profile?.Stat("remoteCapacitorImpedance")??1));CreditP("player",t.Cap-beforeP);}
   else ApplySupportEffect(t.Profile??=Ship.RaiderProfile(),new(){Source="player",Key="support_"+slot,Name=d.Name,Left=s.CycleLeft+.001f,Bonuses=SupportBonuses(s,scale*(s.Overheat?1+d.OverloadAmount*Player.ScopedOverheat("support",Ship.SupportFilter(d)):1))},t.Id);
   WeaponVisuals.Add(new(){Start=Player.Position,End=t.Position,Kind=WeaponKind.Turret,Remaining=.35f});
  }
  if(s.Overheat){ApplyModuleHeat(s,d.HeatDamage*Player.HullFactor("hull","heatDamage")/Math.Max(1,d.ModuleHp));if(s.Damage>=1){s.Burnt=true;s.Active=false;s.PendingAmount=0;s.Ramp=0;Say(d.Name+" burnt out.");}}
 }
 void ApplySupportEffect(Ship profile,SupportEffect effect,string id)
 {
  float shield=profile.MaxShield,armor=profile.MaxArmor;
  profile.SupportEffects.RemoveAll(e=>e.Source==effect.Source&&e.Key==effect.Key);profile.SupportEffects.Add(effect);
  if(id=="player"){Player.Shield=Player.Shield/Math.Max(1,shield)*Player.MaxShield;Player.Armor=Player.Armor/Math.Max(1,armor)*Player.MaxArmor;}
  else if(Targets.FirstOrDefault(t=>t.Id==id) is {} t){t.Shield=t.Shield/Math.Max(1,shield)*profile.MaxShield;t.Armor=t.Armor/Math.Max(1,armor)*profile.MaxArmor;}
 }
 void RestoreSupportLayer(SpaceTarget t,SupportDefinition d,float amount,string actorP="player")
 {
  float beforeP=t.Shield+t.Armor+t.Hull;float impedance=(float)(t.Profile?.Stat("remoteRepairImpedance")??1)*t.RemoteRepairMultiplier;amount*=impedance;
  if(d.Kind is "Shield" or "AncShield")t.Shield=Math.Min(t.Profile?.MaxShield??100,t.Shield+amount);
  else if(d.Kind=="Hull")t.Hull=Math.Min(t.Profile?.MaxHull??65,t.Hull+amount);
  else t.Armor=Math.Min(t.Profile?.MaxArmor??70,t.Armor+amount);
  CreditP(actorP,t.Shield+t.Armor+t.Hull-beforeP);if(t.DroneId.Length>0&&Player.Drones.FirstOrDefault(u=>u.Id==t.DroneId) is {} drone)SyncDrone(drone);
 }
 void TickSupportEffects(Ship p,string id,float dt)
 {
  if(p.SupportEffects.Count==0)return;float shield=p.MaxShield,armor=p.MaxArmor;foreach(var e in p.SupportEffects)e.Left=Math.Max(0,e.Left-dt);p.SupportEffects.RemoveAll(e=>e.Left<=0);
  if(id=="player"){Player.Shield=Player.Shield/Math.Max(1,shield)*p.MaxShield;Player.Armor=Player.Armor/Math.Max(1,armor)*p.MaxArmor;}
  else if(Targets.FirstOrDefault(t=>t.Id==id) is {} t){t.Shield=t.Shield/Math.Max(1,shield)*p.MaxShield;t.Armor=t.Armor/Math.Max(1,armor)*p.MaxArmor;}
 }
 public void UpdateSupport(float dt)
 {
  SupportDockLock=Math.Max(0,SupportDockLock-dt);float left=dt;
  while(left>0)
  {
   float step=Math.Min(.05f,left);left-=step;TickSupportEffects(Player,"player",step);
   foreach(var t in Targets.Where(t=>t.Profile!=null))TickSupportEffects(t.Profile!,t.Id,step);
   foreach(var s in Player.Support)
   {
    s.ReloadLeft=Math.Max(0,s.ReloadLeft-step);bool ending=s.CycleLeft>0;s.CycleLeft=Math.Max(0,s.CycleLeft-step);
    if(ending&&s.CycleLeft==0&&Player.HullSupport(s.Definition) is {} d)
    {
     var t=Targets.FirstOrDefault(t=>t.Id==s.PendingTarget&&t.Alive);if(s.PendingAmount>0&&t!=null&&LockedId==t.Id&&CanLock("player",t.Id)&&Vector3.Distance(Player.Position,t.Position)<=(d.Optimal+d.Falloff*3)*LogisticsRangeFactor(d))RestoreSupportLayer(t,d,s.PendingAmount*RangeScale(Vector3.Distance(Player.Position,t.Position),d.Optimal*LogisticsRangeFactor(d),d.Falloff*LogisticsRangeFactor(d)));
     s.PendingAmount=0;s.PendingTarget="";
     if(!s.Repeat)s.Active=false;
     if(d.Kind=="Ramp")s.Ramp=s.Active&&t!=null?Math.Min(d.RampMax,s.Ramp+d.RampStep):0;
     if(s.AutoReload&&s.Loaded==0&&s.Payload?.Consumable==true&&(d.Kind=="Burst"||!s.Active)&&ItemCount(s.PayloadId,Docked)>0){bool resume=s.Active;ReloadSupport(Array.IndexOf(Player.Support,s));s.Active=resume;}
    }
    if(!Docked&&s.Active&&s.CycleLeft==0&&s.ReloadLeft==0&&!s.Burnt)StartSupportCycle(Array.IndexOf(Player.Support,s));
   }
  }SyncWingmates();
 }
 // Pending output stores the amount before distance attenuation; completion uses current distance.

 public void LeaveSupportGrid(bool clearBursts=false)
 {
  float playerShield=Player.Shield/Player.MaxShield,playerArmor=Player.Armor/Player.MaxArmor;
  var fractions=Targets.Where(t=>Wingmates.Any(w=>w.Id==t.Id)&&t.Profile!=null).ToDictionary(t=>t.Id,t=>new[]{t.Shield/t.Profile!.MaxShield,t.Armor/t.Profile!.MaxArmor});
  SyncWingmates();foreach(var s in Player.Support){s.Active=false;s.PendingAmount=0;s.CycleLeft=0;s.Ramp=0;s.TargetId="";}
  Player.SupportEffects.RemoveAll(e=>clearBursts||!e.Burst);
  foreach(var w in Wingmates){w.Effects.RemoveAll(e=>clearBursts||!e.Burst);if(fractions.TryGetValue(w.Id,out var f)){var p=new Ship();p.SetCombatHull(w.HullId);p.SupportEffects=w.Effects;w.Shield=f[0]*p.MaxShield;w.Armor=f[1]*p.MaxArmor;}}
  Player.Shield=playerShield*Player.MaxShield;Player.Armor=playerArmor*Player.MaxArmor;
 }
 string ChooseNpcSupportVictim(SpaceTarget npc)
 {if(npc.Friendly)return "player";return Wingmates.Where(w=>w.Hull>0&&w.Fleet&&(w.Order=="Attack"||w.Role=="Medic")).Select(w=>Targets.FirstOrDefault(t=>t.Id==w.Id&&t.Alive)).Where(t=>t!=null&&CanLock(npc.Id,t.Id)).OrderBy(t=>t!.Armor).FirstOrDefault()?.Id??"player";}
 void UpdateWingmates(float dt)
 {
  foreach(var w in Wingmates.Where(w=>w.Hull>0&&(w.System.Length==0||w.System==SystemId)))
  {
   var t=Targets.FirstOrDefault(t=>t.Id==w.Id&&t.Alive);if(t?.Profile is not {} p)continue;
   t.Signature=(float)(p.Attributes["signatureRadius"]??82);t.Cap=Math.Min(p.MaxCap,t.Cap+Recharge(t.Cap,p.MaxCap,(float)p.Stat("rechargeRate")/1000)*dt);t.Shield=Math.Min(p.MaxShield,t.Shield+Recharge(t.Shield,p.MaxShield,(float)p.Stat("shieldRechargeRate")/1000)*dt);
   var aim=Targets.FirstOrDefault(x=>x.Id==w.TargetId&&x.Alive);Vector3 goal=w.Order=="Hold"?t.Position:w.Order is "Attack" or "Mine"&&aim!=null?aim.Position+new Vector3(1000,0,500):Player.Position+new Vector3(500,0,500);
   var delta=goal-t.Position;t.Velocity=delta.Length()>100?Vector3.Normalize(delta)*Math.Min(p.Speed*EwarFor(t.Id).Speed,delta.Length()):Vector3.Zero;t.Position+=t.Velocity*dt;
   w.ActionLeft=Math.Max(0,w.ActionLeft-dt);if(w.ActionLeft>0)continue;
   if(!Player.SpecialNoAssist&&w.Role=="Medic"&&w.Fleet&&Vector3.Distance(t.Position,Player.Position)<9000&&t.Cap>=25){t.Cap-=25;Player.Shield=Math.Min(Player.MaxShield,Player.Shield+70);Player.Armor=Math.Min(Player.MaxArmor,Player.Armor+45);w.ActionLeft=6;}
   else if(p.NexusLeft<=0&&w.Order=="Attack"&&aim!=null&&!aim.Friendly&&CanLock(t.Id,aim.Id)&&t.Cap>=6){t.LockId=aim.Id;float range=Vector3.Distance(t.Position,aim.Position);var direction=range>0?(aim.Position-t.Position)/range:Vector3.UnitX;float angular=Vector3.Cross(aim.Velocity-t.Velocity,direction).Length()/Math.Max(1,range);var gun=WeaponCatalog.Get("rail") with{Optimal=6000*p.SupportFactor("optimal"),Falloff=6500*p.SupportFactor("falloff"),Tracking=.12f*p.SupportFactor("tracking")};float chance=TurretChance(gun,null,range,angular,EffectiveSignature(aim));if(CombatRoll()<chance)ApplyPacket(aim,DamagePacket.Single(40,DamageType.Kinetic));t.Cap-=6;w.ActionLeft=3;}
   else if(w.Role=="Miner"&&w.Order=="Mine"&&aim?.Kind==TargetKind.Asteroid&&Vector3.Distance(t.Position,aim.Position)<p.Stat("miningRange")&&t.Cap>=8){float q=Math.Min(aim.Ore,Math.Min((float)p.Stat("miningYield"),Math.Max(0,Player.CargoMax-CargoUsed)));aim.Ore-=q;Player.Ore+=q;t.Cap-=8*p.SupportFactor("miningCap");w.ActionLeft=(float)p.Stat("miningCycle");}
   w.Broadcast=t.Shield/p.MaxShield<.35f?"SHIELD":t.Armor/p.MaxArmor<.35f?"ARMOR":t.Cap/p.MaxCap<.25f?"CAPACITOR":"";
  }SyncWingmates();
 }
}
