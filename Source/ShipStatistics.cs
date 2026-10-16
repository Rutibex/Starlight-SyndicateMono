namespace StarlightSyndicateDemo;
public sealed partial class Ship
{
 [System.Text.Json.Serialization.JsonInclude] public Dictionary<string,double?> Attributes {get;private set;} = ShipAttributeCatalog.All.ToDictionary(a=>a.Key,a=>(double?)null);
 [System.Text.Json.Serialization.JsonInclude] public string HullName {get;private set;}="Wayfarer";
 public double Stat(string key)=>CapsuleD?CapsuleStatD(key):key switch
 {"volleyDamage"=>Enumerable.Range(0,Weapons.Length).Select(WeaponAt).Where(w=>w!=null&&w.Online&&!w.Burnt).Sum(w=>TacticalRulesB.Weapon(this,w!).Packet.Total),"droneDps"=>BayDroneDps,"alignTime"=>AlignTime,"effectiveHitpoints"=>EffectiveHitpoints,"capacitorPeakRecharge"=>PeakCapRecharge,"shieldPeakRecharge"=>PeakShieldRecharge,"capacitorUse"=>CapacitorUse,"capacitorStableFraction"=>double.IsNaN(CapacitorStableFraction)?-1:CapacitorStableFraction,"shieldTank"=>DefenseRepairRate("shield"),"totalDps"=>TotalDps,"miningYieldRate"=>(UtilityFit=="miner"?60*Stat("miningYield")/Stat("miningCycle"):0)+UtilityHarvestRate,"missileMaxRange"=>Stat("missileVelocity")*Stat("missileFlightTime"),"currentSpeed"=>Velocity.Length(),"turretDps"=>Enumerable.Range(0,Weapons.Length).Select(WeaponAt).Where(w=>w?.Definition.UsesTurret==true).Sum(w=>EnhancedWeaponDps(w!)),"missileDps"=>Enumerable.Range(0,Weapons.Length).Select(WeaponAt).Where(w=>w?.Definition.Kind==WeaponKind.Missile).Sum(w=>EnhancedWeaponDps(w!)),_=>TacticalAttribute(key)};
 public void SetStat(string key,double value)
 {if(!ShipAttributeCatalog.ByKey.ContainsKey(key))throw new ArgumentException("Unknown ship stat: "+key);if(!double.IsFinite(value)||PositiveKeys.Contains(key)&&value<=0||key.Contains("Resonance")&&(value<0||value>1))throw new ArgumentOutOfRangeException(nameof(value));Attributes[key]=value;}
 static readonly HashSet<string> PositiveKeys=["hp","armorHP","shieldCapacity","capacitorCapacity","rechargeRate","shieldRechargeRate","mass","agility","capacity","turretCycle","turretTracking","turretFalloff","missileCycle","miningCycle","shieldBoostCycle","explosionRadius"];
 public void RestoreStats(Dictionary<string,double?>? saved)
 {if(saved==null)return;foreach(var (key,value) in saved)if(Attributes.ContainsKey(key)){if(value==null){if(!PositiveKeys.Contains(key))Attributes[key]=null;continue;}try{SetStat(key,value.Value);}catch(ArgumentOutOfRangeException){}}}
 public void ConfigureHull(int origin)
 {
  HullName=origin==1?"Claimrunner":origin==2?"Bastion":"Wayfarer";
  foreach(var (key,value) in BaseValues)SetStat(key,value);
  foreach(var attribute in ShipAttributeCatalog.All.Where(a=>a.Group=="Derived"))Attributes[attribute.Key]=0;
  foreach(var attribute in ShipAttributeCatalog.All.Where(a=>a.Key.StartsWith("special")&&a.Key.EndsWith("HoldCapacity")))SetStat(attribute.Key,0);
  foreach(var key in new[]{"canJump","canReceiveCloneJumps","maxSubSystems","maxGangModules","jumpClonesLeft","warpBubbleImmune","ignoreCloakVelocityPenalty","enablePerformConduitJump","conduitJumpPassengerCount","conduitJumpDriveConsumptionAmount","frigateEscapeBayCapacity"})SetStat(key,0);
  SetStat("heatCapacityHi",85);SetStat("heatCapacityMed",70);SetStat("heatCapacityLow",65);
  SetStat("heatDissipationRateHi",.013);SetStat("heatDissipationRateMed",.011);SetStat("heatDissipationRateLow",.01);
  SetStat("heatGenerationMultiplier",1);SetStat("heatAttenuationHi",.65);SetStat("heatAttenuationMed",.62);SetStat("heatAttenuationLow",.6);
  SetStat("warpSpeedMultiplier",1);SetStat("warpCapacitorNeed",.0000045);SetStat("covertOpsAndReconOpsCloakModuleDelay",17000);
  SetStat("shieldCapacity",origin==2?220:170);SetStat("capacity",origin==1?240:160);SetStat("maxVelocity",origin==1?650:760);
  SetStat("mass",origin==1?1220000:origin==2?1150000:1080000);
  Shield=MaxShield;Armor=MaxArmor;Hull=MaxHull;Cap=MaxCap;
 }
 public float MaxShield=>(float)Stat("shieldCapacity");
 public float MaxArmor=>(float)Stat("armorHP");
 public float MaxHull=>(float)Stat("hp");
 public float Resistance(string layer,DamageType type)
 {string name=type switch{DamageType.EM=>"Em",DamageType.Thermal=>"Thermal",DamageType.Kinetic=>"Kinetic",_=>"Explosive"};return (float)Math.Clamp(1-Stat(layer+name+"DamageResonance"),0,1);}
 public double AlignTime=>Math.Log(4)*Stat("mass")*Stat("agility")/1000000;
 public double PeakCapRecharge=>2.5*MaxCap/(Stat("rechargeRate")/1000);
 public double PeakShieldRecharge=>2.5*MaxShield/(Stat("shieldRechargeRate")/1000);
 public double EffectiveHitpoints
 {get{double total=0;foreach(var layer in new[]{"shield","armor","hull"}){double hp=layer=="shield"?MaxShield:layer=="armor"?MaxArmor:MaxHull;double weakest=Enum.GetValues<DamageType>().Min((DamageType damage)=>Resistance(layer,damage));total+=hp/(1-weakest);}return total;}}
 public double TotalDps=>WeaponDps+Stat("droneDps");
 public double CapacitorUse=>Enumerable.Range(0,Weapons.Length).Select(WeaponAt).Where(w=>w?.Active==true&&w.Online&&!w.Burnt).Sum(w=>{var q=TacticalRulesB.Weapon(this,w!);return q.Capacitor/q.Cycle;})+DefenseCapUse+SupportCapUse+EnhancementCapUse+MobilityCapUse+UtilityCapUse+SpecialCapUse+Ewar.Where(e=>e.Online&&e.Active&&e.Definition!=null).Sum(e=>HullEwar(e.Definition)!.Capacitor*RigEwarCap(e.Definition!)/e.Definition!.Cycle)+(MinerOn?8/Stat("miningCycle"):0);
 public double CapacitorStableFraction
 {get{double load=CapacitorUse,scale=10*MaxCap/(Stat("rechargeRate")/1000);if(load>PeakCapRecharge)return double.NaN;return Math.Pow((1+Math.Sqrt(Math.Max(0,1-4*load/scale)))/2,2);}}
 public string SimulationStatus(string key)=>ShipAttributeCatalog.ByKey[key].Group=="Derived"||key is "turretDps" or "missileDps"?"Derived":(LiveKeys.Contains(key)||UsesDedicatedHolds&&(key.EndsWith("HoldCapacity")||key is "generalMiningHoldCapacity" or "specialFuelBayCapacity" or "fleetHangarCapacity" or "warpCoreStrength" or "jumpFatigueMultiplier"))?"Active":Attributes[key]!=null?"Defined / pending system":"Not applicable / reserved";
 public static readonly HashSet<string> LiveKeys=["conduitJumpPassengerCount","conduitJumpDriveConsumptionAmount","dscanImmune","droneControlDistance","rigSize","upgradeSlotsLeft","upgradeCapacity","warpCapacitorNeed","baseWarpSpeed","warpSpeedMultiplier","jumpDriveConsumptionAmount","jumpDriveRange","jumpDriveCapacitorNeed","canJump","cpuOutput","powerOutput","maxGangModules","probeStrength","probeDeviation","virusCoherence","maxDirectionalScanRange","scanResolution","remoteRepairImpedance","remoteAssistanceImpedance","remoteCapacitorImpedance","shieldCapacity","armorHP","hp","capacitorCapacity","rechargeRate","shieldRechargeRate","maxVelocity","agility","mass","capacity","signatureRadius","maxTargetRange","shieldEmDamageResonance","shieldThermalDamageResonance","shieldKineticDamageResonance","shieldExplosiveDamageResonance","armorEmDamageResonance","armorThermalDamageResonance","armorKineticDamageResonance","armorExplosiveDamageResonance","hullEmDamageResonance","hullThermalDamageResonance","hullKineticDamageResonance","hullExplosiveDamageResonance","shieldBoostAmount","shieldBoostCycle","shieldBoostCap","turretTracking","turretOptimal","turretFalloff","turretCycle","turretCap","missileVelocity","missileFlightTime","missileCycle","explosionRadius","miningCycle","miningRange","miningYield","webRange","webCap"];
 static readonly Dictionary<string,double> BaseValues=new()
 {
  ["hp"]=110,["armorHP"]=130,["shieldCapacity"]=170,["capacitorCapacity"]=185,["rechargeRate"]=95000,["shieldRechargeRate"]=180000,
  ["mass"]=1080000,["agility"]=2.5925925926,["maxVelocity"]=760,["signatureRadius"]=82,["capacity"]=160,["volume"]=26500,["packagedVolume"]=2600,["radius"]=32,
  ["hiSlots"]=3,["medSlots"]=2,["lowSlots"]=2,["upgradeSlotsLeft"]=3,["upgradeCapacity"]=400,["rigSize"]=1,["turretSlotsLeft"]=1,["launcherSlotsLeft"]=1,["cpuOutput"]=112,["powerOutput"]=57,["cpuLoad"]=48,["powerLoad"]=29,
  ["maxTargetRange"]=13500,["maxLockedTargets"]=4,["scanResolution"]=525,["scanRadarStrength"]=0,["scanLadarStrength"]=0,["scanMagnetometricStrength"]=13,["scanGravimetricStrength"]=0,["baseWarpSpeed"]=5.4,["warpCoreStrength"]=0,["warpScrambleStatus"]=0,
  ["shieldEmDamageResonance"]=1,["shieldThermalDamageResonance"]=.8,["shieldKineticDamageResonance"]=.6,["shieldExplosiveDamageResonance"]=.5,
  ["armorEmDamageResonance"]=.5,["armorThermalDamageResonance"]=.65,["armorKineticDamageResonance"]=.75,["armorExplosiveDamageResonance"]=.9,
  ["hullEmDamageResonance"]=1,["hullThermalDamageResonance"]=1,["hullKineticDamageResonance"]=1,["hullExplosiveDamageResonance"]=1,
  ["droneCapacity"]=12,["droneBandwidth"]=10,["maxActiveDrones"]=2,["droneControlDistance"]=18000,["fighterCapacity"]=0,["fighterTubes"]=0,["fighterLightSlots"]=0,["fighterHeavySlots"]=0,["fighterSupportSlots"]=0,
  ["energyWarfareResistance"]=1,["sensorDampenerResistance"]=1,["weaponDisruptionResistance"]=1,["targetPainterResistance"]=1,["stasisWebifierResistance"]=1,["ECMResistance"]=1,["remoteRepairImpedance"]=1,["remoteAssistanceImpedance"]=1,["remoteCapacitorImpedance"]=1,
  ["jumpDriveRange"]=0,["jumpDriveConsumptionAmount"]=0,["jumpDriveCapacitorNeed"]=0,["shipMaintenanceBayCapacity"]=0,["fleetHangarCapacity"]=0,["specialFuelBayCapacity"]=0,["generalMiningHoldCapacity"]=0,["maxJumpClones"]=0,["jumpFatigueMultiplier"]=1,["jumpThroughFatigueMultiplier"]=1,["dscanImmune"]=0,["maxDirectionalScanRange"]=2130000000000,["techLevel"]=1,
  ["turretDps"]=23/2.1,["missileDps"]=45/5.2,["droneDps"]=0,["volleyDamage"]=68,["turretOptimal"]=4200,["turretFalloff"]=5200,["turretTracking"]=40.0/210,["turretCycle"]=2.1,["turretCap"]=6,
  ["missileVelocity"]=1600,["missileFlightTime"]=9,["explosionRadius"]=110,["explosionVelocity"]=120,["damageReductionFactor"]=.72,["missileCycle"]=5.2,
  ["miningYield"]=14,["miningCycle"]=4.8,["miningRange"]=6500,["probeStrength"]=28,["probeDeviation"]=1,["probeScanTime"]=8,["virusStrength"]=22,["virusCoherence"]=65,
  ["shieldBoostAmount"]=34,["shieldBoostCycle"]=4,["shieldBoostCap"]=21,["webRange"]=6500,["webStrength"]=58,["webCap"]=2,["estimatedHullValue"]=11800
 };
 public static Ship RaiderProfile()
 {var s=new Ship();s.HullName="Black Horizon interceptor";s.SetStat("shieldCapacity",95);s.SetStat("armorHP",58);s.SetStat("hp",44);s.SetStat("signatureRadius",62);s.SetStat("maxVelocity",290);s.SetStat("mass",980000);s.SetStat("capacity",85);s.Shield=s.MaxShield;s.Armor=s.MaxArmor;s.Hull=s.MaxHull;return s;}
 public Ship(){ConfigureHull(0);}
}
