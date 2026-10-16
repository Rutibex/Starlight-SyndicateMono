using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed record RigModuleModifier {public string Attribute{get;init;}="";public string[] Targets{get;init;}=[];public float Value{get;init;}public bool Drawback{get;init;}}
public sealed record RigDefinition
{
 public string Id{get;init;}="";public string Name{get;init;}="";public string Family{get;init;}="";public string Group{get;init;}="";public int Size{get;init;}public int Calibration{get;init;}public float Volume{get;init;}public decimal Price{get;init;}public int ReferenceTypeId{get;init;}public bool Unique{get;init;}public RigModuleModifier[] Modifiers{get;init;}=[];public Dictionary<string,float> Bonuses{get;init;}=[];public Dictionary<string,float> Drawbacks{get;init;}=[];
 public string Description=>$"{RigCatalog.SizeName(Size)} rig / {Calibration} calibration / {Volume:0.#} m3. "+string.Join("; ",Bonuses.Select(b=>RigCatalog.BonusText(b.Key,b.Value)))+". Drawbacks at skill 0: "+string.Join("; ",Drawbacks.Select(b=>$"{RigCatalog.Label(b.Key)}: {b.Value:+0.0%;-0.0%;0%}"))+". Rigging skill reduces trained drawbacks 10% per level. Installation is permanent: removal destroys the rig. "+(Unique?"One anchor per hull. ":"");
}
public static class RigCatalog
{
 public static string Label(string key)=>key switch{"hp"=>"Structure capacity","capacity"=>"Cargo capacity","agility"=>"Inertia","rechargeRate"=>"Capacitor recharge time",_=>System.Text.RegularExpressions.Regex.Replace(key.Replace("Add",""),"([a-z])([A-Z])","$1 $2")};
 public static string BonusText(string key,float value)=>Label(key)+": "+(key.EndsWith("Add")?value.ToString("+0.##;-0.##;0"):value.ToString("+0.0%;-0.0%;0%"));
 public static string SizeName(int n)=>n switch{1=>"Small",2=>"Medium",3=>"Large",4=>"Capital",_=>"Unknown"};
 public static readonly RigDefinition[] All=JsonSerializer.Deserialize<RigDefinition[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Rigs.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly Dictionary<string,RigDefinition> ById=All.ToDictionary(d=>d.Id);
 public static readonly ItemDefinition[] Items=All.Select(d=>new ItemDefinition(d.Id,d.Name,"Rigs / "+d.Group,d.Description,d.Volume,d.Price,"rig")).ToArray();
}
public sealed partial class Ship
{
 public string[] Rigs{get;set;}=["","",""];public Dictionary<string,int> RiggingSkills{get;set;}=[];
 [System.Text.Json.Serialization.JsonIgnore]public IEnumerable<RigDefinition> InstalledRigs=>Rigs.Select(id=>RigCatalog.ById.GetValueOrDefault(id)).OfType<RigDefinition>();
 public IEnumerable<float> RigStrengths(string key)=>InstalledRigs.SelectMany(d=>d.Bonuses).Where(b=>b.Key==key).Select(b=>b.Value);
 public IEnumerable<float> RigSupportStrengths(string key)=>key is "maxTargetRange" or "scanResolution" or "sensorStrength"?RigStrengths(key):Enumerable.Empty<float>();
 public float RigAdd(string key)=>RigStrengths(key).Sum();
 public float RigFactor(string key,bool stacked=true)
 {var a=RigStrengths(key).ToArray();return stacked?DemoWorld.StackMultiplier(a.Where(v=>v>0),true)*DemoWorld.StackMultiplier(a.Where(v=>v<0).Select(Math.Abs)):a.Aggregate(1f,(n,v)=>n*(1+v));}
 public float RigDrawbackFactor(string key)=>InstalledRigs.Aggregate(1f,(n,d)=>n*(1+d.Drawbacks.GetValueOrDefault(key)*(d.Unique||d.Family=="Processor Overclocking Unit"?1:1-.1f*Math.Clamp(RiggingSkills.GetValueOrDefault(d.Group),0,5))));
 [System.Text.Json.Serialization.JsonIgnore]public int RigCalibration=>InstalledRigs.Sum(d=>d.Calibration);
 public double RigStat(string key,double value)
 {if(key=="upgradeSlotsLeft")return Math.Max(0,value-InstalledRigs.Count());if(key=="maxGangModules")return value+RigAdd("maxGangModulesAdd");if(key is "droneControlDistance" or "probeStrength" or "probeDeviation")return value; // additive handled in EnhancementStat
  if(key.Contains("DamageResonance"))return value; // joins resist stacking pool in DefenseStat
  if(key=="missileCycle")return value*RigFactor("Missilecycle");if(key=="turretCycle")return value*RigWeapon(WeaponAt(0)?.Definition??WeaponCatalog.Get("rail"),"cycle");if(key=="turretCap")return value*RigWeapon(WeaponAt(0)?.Definition??WeaponCatalog.Get("rail"),"cap");
  if(key=="warpBubbleImmune"&&InstalledRigs.Any(d=>d.Unique))return 0;
  if(key is "maxTargetRange" or "scanResolution" or "agility" or "maxVelocity")return value*RigDrawbackFactor(key); // joins SupportFactor
  return value*RigFactor(key,key is not ("armorHP" or "hp" or "shieldCapacity" or "capacitorCapacity" or "rechargeRate" or "shieldRechargeRate" or "capacity" or "cpuOutput" or "powerOutput" or "mass" or "warpCapacitorNeed"))*RigDrawbackFactor(key);
 }
 public static string RigWeaponFamily(WeaponDefinition w)=>w.Kind==WeaponKind.Missile?"Missile":w.Kind==WeaponKind.Turret?w.Laser?"Laser":w.Family is "Blasters" or "Railguns"?"Hybrid":w.Family is "Artillery" or "Autocannons"?"Projectile":"":"";
 public float WeaponProjection(WeaponDefinition w,string key)=>SkillWeapon(w,key)*HullWeapon(w,key)*SupportFactor(key,RigStrengths(RigWeaponFamily(w)+key));
 public float DroneDamageFactor(bool sentry){var values=EnhancementStrengths("droneDamage").Concat(sentry?RigStrengths("sentryDamage"):Enumerable.Empty<float>()).ToArray();return HullFactor("drone","damage")*DemoWorld.StackMultiplier(values.Where(v=>v>0),true)*DemoWorld.StackMultiplier(values.Where(v=>v<0).Select(Math.Abs));}
 public float RigWeapon(WeaponDefinition w,string key)=>(key=="cap"?SkillWeapon(w,key)*HullWeapon(w,key):1)*RigFactor(RigWeaponFamily(w)+key,key is not ("cap" or "cpu" or "power"))*RigDrawbackFactor(RigWeaponFamily(w)+key);
 public float RigModuleFactor(string id,string attribute)
 {var values=InstalledRigs.SelectMany(d=>d.Modifiers.Where(m=>m.Attribute==attribute&&m.Targets.Contains(id)).Select(m=>m.Value*(m.Drawback?1-.1f*Math.Clamp(RiggingSkills.GetValueOrDefault(d.Group),0,5):1))).ToArray();return SkillItemFactor(id,attribute)*values.Aggregate(1f,(n,v)=>n*(1+v));}
 public float RigCostFactor(string id,string cost)=>RigModuleFactor(id,cost);
 public float RigCpu(string id,double value)=>(float)value*HullCpu(id)*RigCostFactor(id,"cpu");public float RigPower(string id,double value)=>(float)value*HullPower(id)*RigCostFactor(id,"power");
 public float RigLocalAmount(string kind)=>SecurityRepairFactor(kind)*HullFactor("local","amount",kind is "ArmorRepair" or "AncillaryArmor"?"Armor":kind is "ShieldBoost" or "AncillaryShield"?"Shield":"none")*(kind is "ArmorRepair" or "AncillaryArmor"?RigFactor("armorRepair"):1);
 public float RigLocalCap(string kind,string id="")=>SkillItemFactor(id,"capacitorNeed")*HullFactor("local","cap",kind is "ShieldBoost" or "AncillaryShield"?"Shield":kind is "ArmorRepair" or "AncillaryArmor"?"Armor":kind=="FlexShield"?"Flex":"none")*(kind is "ShieldBoost" or "AncillaryShield"?RigFactor("shieldCap",false):1);
 public float RigRemoteCap(string kind)=>kind is "Armor" or "AncArmor" or "Ramp"?RigFactor("remoteArmorCap",false):kind=="Cap"?RigFactor("energyCap",false):1;
 public float RigEwarRange(EwarDefinition d)=>d.Kind is EwarKind.ECM or EwarKind.Damp or EwarKind.Tracking or EwarKind.Guidance or EwarKind.Paint or EwarKind.BurstJam?RigFactor("ewarRange"):1;
 public float RigEwarCap(EwarDefinition d)=>RigModuleFactor(d.Id,"capacitorNeed");
 public int RigCoherence(string kind)=>(int)RigAdd(kind=="Relic"?"relicCoherenceAdd":"dataCoherenceAdd");
}
public sealed partial class DemoWorld
{
 public void NormalizeRigDrone(DroneUnit u){if(u.Definition.Fighter)return;float old=Math.Max(.01f,u.AppliedRigHpFactor);u.Shield/=old;u.Armor/=old;u.Hull/=old;u.AppliedRigHpFactor=1;}
 public void RefreshRigDroneHealth(){foreach(var u in Player.Drones.Where(u=>u.Owner=="player"&&!u.Definition.Fighter&&u.Location is "Bay" or "Reserve" or "Space")){float factor=Player.RigFactor("droneHP",false)*Player.HullFactor("drone","hp",u.Definition.Effect)*Player.LightCombatFactor(u.Definition,"hp")*Player.SpecificDroneFactor(u.Definition,"hp"),ratio=factor/Math.Max(.01f,u.AppliedRigHpFactor);u.Shield*=ratio;u.Armor*=ratio;u.Hull*=ratio;u.AppliedRigHpFactor=factor;var body=Targets.FirstOrDefault(t=>t.DroneId==u.Id);if(body?.Profile!=null){var d=EffectiveDrone(u);body.Shield=u.Shield;body.Armor=u.Armor;body.Hull=u.Hull;body.Profile.SetStat("shieldCapacity",d.Shield);body.Profile.SetStat("armorHP",d.Armor);body.Profile.SetStat("hp",d.Hull);}}}
 public int FittingRigSlot;public int PendingRigRemoval=-1;
 public string RigFitError(RigDefinition d,int slot,float cargoConsumption=0)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(!Docked)return "Dock before installing rigs.";if(slot<0||slot>=Player.Rigs.Length||slot>=Player.Attributes.GetValueOrDefault("upgradeSlotsLeft"))return "No rig socket.";
  if(Player.Rigs[slot].Length>0)return "Destroy the installed rig before replacing it.";if(d.Size!=(int)Player.Stat("rigSize"))return "Wrong rig size for this hull.";
  if(Player.RigCalibration+d.Calibration>Player.Stat("upgradeCapacity"))return "Calibration exceeded.";if(d.Unique&&Player.InstalledRigs.Any(x=>x.Unique))return "One anchor per hull.";
  Player.Rigs[slot]=d.Id;try {if(CargoUsed-cargoConsumption>Player.CargoMax+.001)return "Rig would leave cargo over capacity.";if(TotalFitCpu>Player.Stat("cpuOutput")+.001||TotalFitPower>Player.Stat("powerOutput")+.001)return "Rig would exceed CPU or power grid.";}finally{Player.Rigs[slot]="";}return "";
 }
 public bool FitRig(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;if(!RigCatalog.ById.TryGetValue(id,out var d))return false;string err=RigFitError(d,slot,hangar?0:d.Volume);if(err.Length>0||ItemCount(id,hangar)<1){Say(err.Length>0?err:"No rig in selected inventory.");return false;}ChangeStack(id,hangar,-(decimal)(1));Player.Rigs[slot]=id;RefreshRigDroneHealth();RecomputeWeaponFit();Say("Installed "+d.Name+". Removal will destroy it.");return true;}
 public bool RemoveRig(int slot,bool confirmed=false)
 {if(!Docked||slot<0||slot>=Player.Rigs.Length||Player.Rigs[slot].Length==0)return false;if(!confirmed&&!Player.StrategicCruiser){PendingRigRemoval=slot;Say("Confirm destruction in the Rigs window.");return false;}string old=Player.Rigs[slot];Player.Rigs[slot]="";if(CargoUsed>Player.CargoMax+.001||TotalFitCpu>Player.Stat("cpuOutput")+.001||TotalFitPower>Player.Stat("powerOutput")+.001){Player.Rigs[slot]=old;Say("Unload cargo or unfit modules before removing this supporting rig.");return false;}if(Player.StrategicCruiser)ChangeStack(old,true,(decimal)(1));PendingRigRemoval=-1;RefreshRigDroneHealth();RecomputeWeaponFit();Player.Shield=Math.Min(Player.Shield,Player.MaxShield);Player.Armor=Math.Min(Player.Armor,Player.MaxArmor);Player.Hull=Math.Min(Player.Hull,Player.MaxHull);Player.Cap=Math.Min(Player.Cap,Player.MaxCap);Say(Player.StrategicCruiser?"Rig returned to local hangar; socket is now empty.":"Rig destroyed; socket is now empty.");return true;}
 public float WarpEnergyCost(float distanceAu=2)=>Math.Max(1,(float)(Player.Stat("mass")*(Player.Attributes.GetValueOrDefault("warpCapacitorNeed")??.0000045)*Player.HullFactor("hull","warpCapacitorNeed")*distanceAu))*Player.RigFactor("warpCapacitorNeed",false);
 public bool PayWarpEnergy(float distanceAu=2){float cost=WarpEnergyCost(distanceAu);if(Player.Cap<cost){Say("Insufficient capacitor for warp.");return false;}Player.Cap-=cost;return true;}
 public bool TrainRiggingSkill(string group){if(!SkillCatalog.VerificationAllV){var skill=SkillCatalog.All.FirstOrDefault(s=>s.SourceName==group+" Rigging");return skill!=null&&QueueSkill(skill.Id,Math.Min(5,Skills.Trained(skill.Id)+1)).Length==0;}int level=Player.RiggingSkills.GetValueOrDefault(group);float cost=1000*(level+1)*(level+1);if(!Docked||level>=5||Credits<cost||!RigCatalog.All.Any(d=>d.Group==group))return false;WalletChange(-(decimal)(cost),"Gameplay transaction");Player.RiggingSkills[group]=level+1;AdvanceHours(2*(level+1));RecomputeWeaponFit();Say(group+" rigging advanced to "+(level+1));return true;}
 public void RigTrial(string hull="frigate"){WeaponTrial(hull);FittingRigSlot=0;foreach(var d in RigCatalog.All)Hangar[d.Id]=3;PendingRigRemoval=-1;Say("Rig loan hangar stocked. Changing loan hull starts a fresh rig rack.");}
 public bool SetRiggingSkill(string group,int level){if(!Docked||!WeaponTestMode||level<0||level>5)return false;Player.RiggingSkills[group]=level;RecomputeWeaponFit();return true;}
}
