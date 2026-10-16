namespace StarlightSyndicateDemo;
public sealed record CombatHullDefinition(string Id,string Name,int Size,string Role,float Shield,float Armor,float Hull,float Cap,float Cpu,float Power,float Speed,float Cargo,int Turrets,int Launchers);
public static class CombatHulls
{
 public static readonly CombatHullDefinition[] All=[
 new("covert","Night Finch",1,"CovertOps",340,260,220,850,550,700,580,1000,2,2),
 new("recon","Mirage",2,"ForceRecon",1900,1800,1400,3000,950,4500,350,1800,2,2),
 new("blackops","Night Sovereign",3,"BlackOps",6000,6500,4500,8200,1500,18000,190,6000,2,2),
 new("marauder","Fortitude",3,"Marauder",8500,8500,6500,8800,1500,18000,150,4000,2,2),
 new("forceaux","Resurrection",4,"ForceAux",60000,65000,55000,65000,3800,240000,65,18000,2,2),
 new("hauler","Caravan",2,"Industrial",1000,1000,800,2600,850,3000,210,24000,2,2),
 new("blockade","Night Caravan",2,"BlockadeRunner",1500,1200,1000,3200,950,4500,350,16000,2,2),
 new("deeptransport","Iron Caravan",2,"DeepTransport",2200,2000,1700,4200,1100,6000,230,30000,2,2),
 new("jumpfreighter","Star Caravan",4,"JumpFreighter",25000,26000,24000,65000,2600,140000,65,120000,2,2),
 new("mining_barge","Stonewright",2,"MiningBarge",2200,1800,1200,2600,1000,4200,220,18000,2,2),
 new("expedition_frigate","Ice Finch",1,"ExpeditionFrigate",350,280,220,850,550,700,560,3500,2,2),
 new("mining_tender","Claimkeeper",2,"MiningTender",1900,1800,1400,3400,1000,6000,230,14000,2,2),
 new("command_destroyer","Rally",1,"CommandDestroyer",420,300,250,650,280,180,560,600,2,2),
 new("battlecruiser","Vanguard",2,"Battlecruiser",3800,3600,2600,3500,750,5500,250,1800,2,2),
 new("logistics","Mercy",2,"Logistics",1400,1600,1000,2600,750,3400,360,1300,2,2),
 new("adaptive_logistics","Chorus",2,"AdaptiveLogistics",1300,2100,1000,3300,780,4200,330,1400,2,2),
 new("command","Standard Bearer",3,"Command",4500,4800,3800,5500,1300,14500,240,2500,2,2),
 new("expedition_command","Longview",3,"ExpeditionCommand",3800,4300,3000,4800,1150,12000,270,3000,2,2),
 new("frigate","Wayfarer",1,"",170,130,110,185,112,57,760,160,1,1),
 new("destroyer","Gatekeeper",1,"Destroyer",330,240,190,360,210,110,610,310,2,2),
 new("bomber","Shade",1,"Bomber",240,180,160,280,190,95,580,450,1,2),
 new("parasite_s","JoyOS Wisp",1,"Parasite",400,220,180,420,230,125,640,340,2,2),
 new("cruiser","Envoy",2,"",1100,950,720,1300,480,1300,380,900,2,2),
 new("parasite_m","JoyOS Revenant",2,"Parasite",1900,1300,1100,2100,650,2500,300,1500,2,2),
 new("battleship","Bulwark",3,"",6500,5900,4800,7200,1200,14500,160,2800,2,2),
 new("dread","Resolute",4,"Dread",55000,50000,40000,35000,2600,110000,85,12000,2,2),
 new("lancer","Anchor",4,"Lancer",48000,46000,38000,42000,3200,160000,70,15000,2,2),
 new("titan","Sovereign",5,"Titan",220000,200000,170000,90000,5200,350000,45,50000,2,2),
 new("interdictor","Cagekeeper",1,"Interdictor",400,280,220,550,250,140,580,550,2,2),
 new("hic","Lockwarden",2,"HeavyInterdictor",1600,1350,1050,2000,700,2400,280,1300,2,2),
 new("carrier","Starwarden",4,"Carrier",40000,35000,32000,30000,2400,90000,90,10000,2,2),
 new("industrial_cap","Worldforger",4,"CapitalIndustrial",60000,55000,45000,42000,2800,130000,65,16000,2,2),
 new("assault","Iron Finch",1,"Assault",420,380,310,600,260,130,610,300,2,2),
 new("interceptor","Swift Courier",1,"Interceptor",270,220,200,380,200,90,900,240,1,1),
 new("supercarrier","Storm Regent",4,"Supercarrier",120000,115000,90000,65000,4000,210000,60,30000,2,2)];
 public static CombatHullDefinition Get(string id)=>FleetCatalog.ById.TryGetValue(id,out var h)?h.CombatDefinition:All.FirstOrDefault(h=>h.Id==id)??All[0];
}
public sealed partial class Ship
{
 public void SetCombatHull(string id)
 {
  if(FleetCatalog.ById.TryGetValue(id,out var fleet)){ApplyFleetHull(fleet);return;}
  var h=CombatHulls.Get(id);if(CombatHull!=id)Rigs=["","",""];SetStat("rigSize",Math.Min(4,h.Size));SetStat("upgradeSlotsLeft",h.Role is "CovertOps" or "ForceRecon" or "BlackOps" or "Marauder" or "BlockadeRunner" or "DeepTransport" or "Bomber" or "Command" or "Interceptor"?2:3);SetStat("upgradeCapacity",400);CombatHull=id;HullName=h.Name;
  SetStat("shieldCapacity",h.Shield);SetStat("armorHP",h.Armor);SetStat("hp",h.Hull);SetStat("capacitorCapacity",h.Cap);
  SetStat("cpuOutput",h.Cpu);SetStat("powerOutput",h.Power);SetStat("maxVelocity",h.Speed);SetStat("capacity",h.Cargo);
  SetStat("signatureRadius",new[]{30,82,240,520,2600,10000}[h.Size]);SetStat("maxTargetRange",h.Size<4?18000*h.Size:180000);
  SetStat("maxGangModules",h.Role switch{"Command"=>3,"ExpeditionCommand"=>2,"CapitalIndustrial"=>3,"Carrier" or "Supercarrier" or "Titan"=>2,_=>0});
  SetStat("turretSlotsLeft",h.Turrets);SetStat("launcherSlotsLeft",h.Launchers);SetStat("mass",1080000*Math.Pow(8,h.Size-1));SetStat("agility",new double[]{1,2.35,.64,.115,.045,.013}[h.Size]);
  SetStat("medSlots",h.Size==1?3:5);SetStat("hiSlots",3);SetStat("lowSlots",h.Size==1?3:h.Size==2?4:6);
  if(h.Size>=4||h.Role is "CovertOps" or "ForceRecon" or "BlackOps" or "Marauder" or "Industrial" or "BlockadeRunner" or "DeepTransport" or "HeavyInterdictor" or "Bomber" or "MiningBarge" or "ExpeditionFrigate" or "MiningTender" or "CapitalIndustrial" or "Logistics" or "AdaptiveLogistics" or "Command" or "ExpeditionCommand"){SetStat("hiSlots",8);SetStat("medSlots",6);}
  if(h.Role is "Battlecruiser" or "Command"){SetStat("mass",18000000);SetStat("agility",.55);}SetStat("canJump",h.Size>=4?1:0);SetStat("jumpDriveRange",h.Size>=4?5:0);SetStat("jumpDriveConsumptionAmount",h.Size>=4?240:0);SetStat("jumpDriveCapacitorNeed",h.Size>=4?.7:0);
  if(h.Role=="BlackOps"){SetStat("canJump",1);SetStat("jumpDriveRange",7);SetStat("jumpDriveConsumptionAmount",140);SetStat("jumpDriveCapacitorNeed",.7);}
  ConfigureDroneHull();Shield=MaxShield;Armor=MaxArmor;Hull=MaxHull;Cap=MaxCap;
 }
}
public sealed partial class DemoWorld
{
 public bool WeaponTestMode{get;set;}
 public bool WeaponSafety{get;set;}=true;
 public int FittingWeaponSlot{get;set;}
 public void WeaponTrial(string hull)
 {
  if(ImportedWorld43){Say("Loan-hull trials belong to the separate campaign demonstration.");return;}
  if(!Docked){Say("Dock before selecting a trial hull.");return;}
  // Explicit demo service, isolated from the campaign economy; it remains labelled in the HUD and save.
  if(Player.IsFleetHull){FleetBerths.Add(new(){System=StationKey44,Ship=Player,Cargo=new(CargoItems)});Player=new Ship();CargoItems=[];}
  WeaponTestMode=true;
  foreach(int slot in Enumerable.Range(0,Player.Weapons.Length)){ReturnMagazine(Player.Weapons[slot],true);string old=slot==1&&Player.LegacyUtilityOccupied?Player.UtilityFit:Player.Weapons[slot].ModuleId;if(old.Length>0)Hangar[old]=Hangar.GetValueOrDefault(old)+1;}
  foreach(var w in WeaponCatalog.All)Hangar[w.Id]=Math.Max(1,Hangar.GetValueOrDefault(w.Id));
  foreach(var c in ChargeCatalog.All)Hangar[c.Id]=Math.Max(c.Crystal?2:200,Hangar.GetValueOrDefault(c.Id));
  ProvisionEwarTrial();ProvisionDroneTrial();ProvisionDefenseTrial();ProvisionSupportTrial();ProvisionEnhancementTrial();ProvisionMobilityTrial();ProvisionUtilityTrial();ProvisionSpecialTrial();
  Player.Weapons=[WeaponState.Create("rail"),WeaponState.Create("missile")];Player.Rigs=["","",""];Player.SetCombatHull(hull);Player.Weapons[0]=WeaponState.Create("rail");Player.Weapons[1]=WeaponState.Create("missile");Player.UtilityFit="missile";
  RecomputeWeaponFit();Say("WEAPON TRIAL: "+Player.HullName+" loaned; all weapons and charges stocked at this anchorage. Sandbox wallet unchanged.");
 }
 public void EnterWeaponRange()
 {
  if(!Docked){Say("Dock before entering the weapon range.");return;}if(!WeaponTestMode){Say("Select a trial hull in fitting first.");return;}
  EnterGrid("range");
  Targets.Clear();Player.Position=Microsoft.Xna.Framework.Vector3.Zero;
  int size=CombatHulls.Get(Player.CombatHull).Size;
  for(int i=0;i<6;i++)
  {
   var p=Ship.RaiderProfile();var h=CombatHulls.All.First(h=>h.Size==Math.Min(5,Math.Max(1,size)));p.SetCombatHull(h.Id);
   p.SetStat("hp",p.MaxHull*6);p.SetStat("armorHP",p.MaxArmor*6);p.SetStat("shieldCapacity",p.MaxShield*6);p.Shield=p.MaxShield;p.Armor=p.MaxArmor;p.Hull=p.MaxHull;
   Targets.Add(new(){Id="range_"+i,Name=i==5?"Syndicate friendly test vessel":"Instrumented target "+(i+1),Kind=TargetKind.Raider,Profile=p,Position=new(3500+i*1500,0,(i-2)*450),Signature=p.Signature,Shield=p.Shield,Armor=p.Armor,Hull=p.Hull,Friendly=i==5,Training=true});
  }
  Say("Weapon range: select/lock a target, F1/F2 fire; Fitting selects charge and reload. Incoming bombs spawn for interception. Dock via D to return.");
 }
 public bool FitWeapon(string id,bool fromHangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked){Say("Dock before changing weapons.");return false;}
  if(slot<0||slot>=Player.Weapons.Length||!WeaponCatalog.ById.ContainsKey(id)||ItemCount(id,fromHangar)<1)return false;
  var def=WeaponCatalog.Get(id);var hull=CombatHulls.Get(Player.CombatHull);
  string error=FitError(def,slot);if(error.Length>0){Say(error);return false;}
  string old=slot==1&&Player.LegacyUtilityOccupied?Player.UtilityFit:Player.Weapons[slot].ModuleId;
  // Returning a magazine to hangar avoids a cargo-overflow swap. Fitting never creates ammunition.
  ReturnMagazine(Player.Weapons[slot],true);
  StoreWeapon(Player.Weapons[slot]);ChangeStack(id,fromHangar,-(decimal)(1));if(old.Length>0)ChangeStack(old,true,(decimal)(1));
  Player.Weapons[slot]=TakeWeapon(id,fromHangar);if(slot==1)Player.UtilityFit=id;
  Player.TurretOn=Player.MissileOn=Player.MinerOn=false;RecomputeWeaponFit();AdvanceHours(.25);
  Say("Fitted "+def.Name+" to high slot "+(slot+1)+". Load charges before firing.");return true;
 }
 public string FitError(WeaponDefinition def,int slot)
 {
  if(SkillError(def.Id) is {Length:>0} skillError)return skillError;
  var h=CombatHulls.Get(Player.CombatHull);
  if(slot<0||slot>=Player.Weapons.Length||slot>=Player.Stat("hiSlots"))return "Invalid high socket.";
  int occupied=Player.WeaponSocketCount-(Player.WeaponAt(slot)!=null||slot==1&&Player.LegacyUtilityOccupied?1:0)+1;
  if(SpecialCount("high")+UtilityCount("high")+occupied+EnhancementCount("high")+SupportHighCount+DefenseHighCount+(Player.Ewar[0].Definition!=null?1:0)>Player.Stat("hiSlots"))return "Insufficient shared high sockets.";
  if(def.Size>h.Size&&!(Player.IsFleetHull&&FleetCatalog.ById[Player.CombatHull].Class=="Attack Battlecruiser"&&def.Size==3&&def.Role.Length==0)&&!(h.Role=="Bomber"&&def.Family=="Missile Launcher Torpedo"))return "Weapon size exceeds this hull's fitting class.";
  if(def.Role.Length>0&&def.Role!=h.Role&&!(def.Kind==WeaponKind.Defender&&h.Role is "Interdictor" or "CommandDestroyer" or "TacticalDestroyer"))return "Requires a "+def.Role+" hull.";
  var others=Enumerable.Range(0,Player.Weapons.Length).Where(i=>i!=slot).Select(Player.WeaponAt).Where(w=>w!=null&&w.Online).Select(w=>w!.Definition).ToArray();
  if(def.Kind==WeaponKind.Bomb&&others.Any(w=>w.Kind==WeaponKind.Bomb))return "Only one bomb launcher per ship.";
  if(def.Kind==WeaponKind.Breacher&&others.Any(w=>w.Kind==WeaponKind.Breacher))return "Only one parasite launcher per ship.";
  if(def.Superweapon&&others.Any(w=>w.Superweapon))return "Only one superweapon per ship.";
  if((def.UsesTurret?1:0)+others.Count(w=>w.UsesTurret)>Player.Stat("turretSlotsLeft"))return "Insufficient turret hardpoints.";
  if((def.UsesLauncher?1:0)+others.Count(w=>w.UsesLauncher)>Player.Stat("launcherSlotsLeft"))return "Insufficient launcher hardpoints.";
  if(Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.EnhancementCpu+Player.SupportCpu+Player.DefenseCpu+Player.Ewar.Where(s=>s.Online).Sum(e=>e.Definition==null?0:Player.RigCpu(e.Definition.Id,e.Definition.Cpu))+Player.RigCpu(def.Id,def.Cpu)+others.Sum(w=>Player.RigCpu(w.Id,w.Cpu))>Player.Stat("cpuOutput")||Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+Player.EnhancementPower+SupportFittedPower+Player.DefensePower+Player.Ewar.Where(s=>s.Online).Sum(e=>e.Definition==null?0:Player.RigPower(e.Definition.Id,e.Definition.Power))+Player.RigPower(def.Id,def.Power)+others.Sum(w=>Player.RigPower(w.Id,w.Power))>Player.Stat("powerOutput"))return "CPU or power grid exceeded.";
  return "";
 }
 void RecomputeWeaponFit()
 {
  var a=Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null&&w.Online).Select(w=>w!.Definition).ToArray();
  Player.SetStat("cpuLoad",Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.EnhancementCpu+Player.SupportCpu+Player.DefenseCpu+a.Sum(w=>Player.RigCpu(w.Id,w.Cpu))+Player.Ewar.Where(s=>s.Online).Sum(e=>(e.Definition==null?0:Player.RigCpu(e.Definition.Id,e.Definition.Cpu))));Player.SetStat("powerLoad",Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+Player.EnhancementPower+SupportFittedPower+Player.DefensePower+a.Sum(w=>Player.RigPower(w.Id,w.Power))+Player.Ewar.Where(s=>s.Online).Sum(e=>(e.Definition==null?0:Player.RigPower(e.Definition.Id,e.Definition.Power))));
  Player.SetStat("volleyDamage",a.Sum(w=>w.Damage*Player.WeaponEnhancement(w,"damage")));
 }
 void ReturnMagazine(WeaponState state,bool hangar)
 {
  if(state.Loaded>0&&state.Charge!=null)ChangeStack(state.ChargeId,hangar,(decimal)(state.Loaded));
  state.Loaded=0;state.Stop();
 }
 public bool LoadWeapon(int slot,string chargeId)
 {
  var state=Player.WeaponAt(slot);if(state==null||!ChargeCatalog.ById.TryGetValue(chargeId,out var c)||c.WeaponKey!=ChargeCatalog.Key(state.Definition)){Say("Incompatible charge.");return false;}
  if(state.WarmupLeft>0||state.EffectLeft>0){Say("Cannot change charge during a firing sequence.");return false;}
  if(state.ReloadLeft>0)return false;
  bool hangar=Docked&&ItemCount(chargeId,true)>0;
  if(ItemCount(chargeId,hangar)<1){Say("No compatible charges in "+(hangar?"station hangar":"ship cargo")+".");return false;}
  if(state.Loaded>0&&!hangar&&CargoUsed+state.Loaded*ItemVolume(state.ChargeId)>Player.CargoMax+.001f){Say("Cargo cannot accept the old magazine.");return false;}
  ReturnMagazine(state,hangar);state.PendingCharge=chargeId;state.ReloadLeft=state.Definition.Reload*Player.HullWeapon(state.Definition,"reload");state.Stop();
  // Reserve the new magazine immediately and serialize it, so inventory movement cannot duplicate it during reload.
  int count=c.Crystal?1:Math.Min(state.Definition.Magazine,(int)ItemCount(chargeId,hangar));
  ChangeStack(chargeId,hangar,-(decimal)(count));state.ChargeId=chargeId;state.Loaded=count;
  if(Docked){state.ReloadLeft=0;state.PendingCharge="";}
  Say("Loaded "+count+" "+c.Name+"; "+(Docked?"ready":"reload "+state.ReloadLeft+" s"));return true;
 }
 public void ProvisionWeapon(int slot)
 {
  if(!Docked){Say("Dock to provision spare charges.");return;}var w=Player.WeaponAt(slot);if(w?.Charge==null)return;
  int room=(int)Math.Floor(Math.Max(0,Player.CargoMax-CargoUsed)/ItemVolume(w.ChargeId));
  int quantity=Math.Min(room,Math.Min((int)ItemCount(w.ChargeId,true),Math.Max(1,w.Definition.Magazine*4)));
  if(quantity>0)TransferItem(w.ChargeId,true,quantity);else Say("No spare charges or cargo capacity.");
 }
 public void ReloadWeapon(int slot){var w=Player.WeaponAt(slot);if(w!=null&&w.ChargeId.Length>0)LoadWeapon(slot,w.ChargeId);}
}
