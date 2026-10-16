namespace StarlightSyndicateDemo;
public static class ShipBatch24
{
 public static bool ContextOnly(string text)=>new[]{"mobile phase anchor","orbital skyhook","sleeper drones","depending on the environment"}.Any(x=>text.Contains(x,StringComparison.OrdinalIgnoreCase));
 public static string Role(FleetHull h)=>h.SourceHull is "Zarmazd" or "Rabisu"?"AdaptiveLogistics":h.Class switch {
 "Marauder"=>"Marauder", "Black Ops"=>"BlackOps", "Command Ship"=>"Command", "Strategic Cruiser"=>"StrategicCruiser", "Interdictor"=>"Interdictor", "Command Destroyer"=>"CommandDestroyer", "Tactical Destroyer"=>"TacticalDestroyer", "Heavy Assault Cruiser"=>"HeavyAssault","Force Recon Ship"=>"ForceRecon","Combat Recon Ship"=>"CombatRecon","Heavy Interdiction Cruiser"=>"HeavyInterdictor","Assault Frigate"=>"Assault", "Interceptor"=>"Interceptor", "Electronic Attack Ship"=>"ElectronicAttack", "Stealth Bomber"=>"Bomber",
 "Mining Barge" or "Exhumer"=>"MiningBarge", "Expedition Frigate"=>"ExpeditionFrigate", "Industrial Command Ship"=>h.SourceHull.StartsWith("Porpoise")?"MiningTender":"ExpeditionCommand", "Capital Industrial Ship"=>"CapitalIndustrial", "Freighter"=>"Freighter", "Jump Freighter"=>"JumpFreighter", "Deep Space Transport"=>"DeepTransport", "Blockade Runner"=>"BlockadeRunner", "Hauler"=>"Industrial", "Covert Ops"=>"CovertOps", "Expedition Command Ship"=>"ExpeditionCommand", "Prototype Exploration Ship"=>"PrototypeExplorer", _=>h.Class.Contains("Battlecruiser")?"Battlecruiser":""};
}
public sealed partial class Ship
{
 public bool HullCapability(string phrase)=>ShipBatch21.ById.TryGetValue(CombatHull,out var h)&&ActiveHullEffects.Any(e=>e.System=="capability"&&e.Key.Contains(phrase));
 public float PilotSecurity{get;set;}
 public float SecurityGainFactor=>ShipBatch21.ById.TryGetValue(CombatHull,out var p)?1+p.Effects.Where(e=>e.System=="capability"&&e.Key.Contains("security status gains")).Sum(e=>e.Value*HullProficiency.GetValueOrDefault(e.Skill,1))/100:1;
 public float SecurityRepairFactor(string kind)=>kind is "ShieldBoost" or "AncillaryShield" or "ArmorRepair" or "AncillaryArmor"&&HullCapability("10x pilot security status")?1+Math.Clamp(PilotSecurity*.1f,0,.5f):1;
 public bool Specialist=>ShipBatch21.ById.TryGetValue(CombatHull,out var h)&&h.Batch==24;
 public float HarvestFactor(UtilityDefinition d,string key)=>d.Harvest?HullFactor("harvest",key,d.Family)*(d.Kind.Contains("Strip")?HullFactor("harvest",key,"Strip"):1):1;
 public SpecialDefinition SpecialHullDefinition(SpecialDefinition d)
 {
  float delay=d.Cooldown,target=d.TargetDelay;
  if(d.Cloak&&(Specialist||ShipBatch21.ById.TryGetValue(CombatHull,out var p)&&p.Batch is 25 or 27 or 28 or 29)){if(HullCapability("reduced to 5 seconds"))delay=5;else if(HullCapability("reduced to 15 seconds"))delay=15;if(HullCapability("targeting delay reduced to 5"))target=5;if(HullCapability("no targeting delay"))target=0;}
  return d with{Cooldown=delay,TargetDelay=target,FuelQuantity=(int)Math.Ceiling(d.FuelQuantity*HullFactor("special","fuel",d.Kind)),Cycle=d.Cycle*HullFactor("special","cycle",d.Kind)};
 }
}
public sealed partial class DemoWorld
{
 void EquipSpecialistStarter(FleetHull h,BatchHull p)
 {
  Player.Defense=Enumerable.Range(0,12).Select(_=>new DefenseState()).ToArray();Player.Ewar=[new(),new(),new(),new()];
  bool Utility(string kind){foreach(var d in UtilityCatalog.All.Where(d=>d.Kind==kind).OrderBy(d=>d.Cpu).ThenBy(d=>d.Power)){int slot=Enumerable.Range(0,16).FirstOrDefault(i=>Ship.UtilitySlot(i)==d.Slot&&Player.Utilities[i].Definition==null,-1);if(slot<0||UtilityFitError(d,slot).Length>0)continue;Player.Utilities[slot]=new(){ModuleId=d.Id};if(d.Kind=="CoreProbe"){Player.Utilities[slot].PayloadId="ut_probe_core";Player.Utilities[slot].Loaded=8;CargoItems["ut_probe_core"]=8;}return true;}return false;}
  bool Special(string kind){foreach(var d in SpecialCatalog.All.Where(d=>d.Kind==kind).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Array.FindIndex(Player.Special,s=>s.Definition==null);if(SpecialFitError(d,slot).Length>0)continue;Player.Special[slot]=new(){ModuleId=d.Id};if(d.Fuel.Length>0)CargoItems[d.Fuel]=Player.SpecialHullDefinition(d).FuelQuantity*3;return true;}return false;}
  bool Support(string kind,int size){foreach(var d in SupportCatalog.All.Where(d=>d.Kind==kind&&d.Size==size&&(kind!="Burst"||d.Family=="Expedition")).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Array.FindIndex(Player.Support,s=>s.Definition==null);if(SupportFitError(d,slot).Length>0)continue;Player.Support[slot]=SupportState.Create(d.Id);if(kind=="Burst"){var payload=SupportPayloads.For(d).First();Player.Support[slot].PayloadId=payload.Id;Player.Support[slot].Loaded=Math.Max(1,(int)(d.MagazineVolume/payload.Volume));CargoItems[payload.Id]=10;}return true;}return false;}
  bool explore=p.Role.Contains("Exploration");
  if(explore){if(h.SourceHull=="Zephyr")Player.Utilities[0]=new(){ModuleId="ut_iris24",PayloadId="ut_probe_core",Loaded=8};else{Utility("CoreProbe");Utility("Data");Utility("Relic");}}
  if(p.Role=="Mining"){
   if(h.Class is "Mining Barge" or "Exhumer"){Utility("Strip");Utility("Strip");}
   else if(h.Class=="Expedition Frigate"||h.SourceHull=="Venture"){Utility(h.SourceHull=="Endurance"?"IceLaser":"Miner");if(h.SourceHull!="Endurance")Utility("GasScoop");}
   else{Special("Core");Support("Burst",0);Utility("Compress");Utility("Tractor");}
  }
  if(p.Role=="Salvage"){Utility("Tractor");for(int i=0;i<3;i++)Utility("Salvage");}
  if(p.Role.Contains("Logistics",StringComparison.OrdinalIgnoreCase)){
   string kind=h.SourceHull is "Zarmazd" or "Rabisu"?"Ramp":p.Effects.Any(e=>e.System=="support"&&e.Filter=="Armor")?"Armor":"Shield";
   int size=h.Class=="Logistics"?3:Math.Min(3,p.WeaponSize);
   if(!Support(kind,size))Support(kind,Math.Min(2,p.WeaponSize));Support(kind,size);
   if(p.Effects.Any(e=>e.System=="support"&&e.Filter=="Cap"))Support("Cap",h.Class=="Logistics"?2:1);
  }
  if(h.Class=="Expedition Command Ship"){Support("Burst",0);Utility("GasScoop");}
  if(h.Class is "Blockade Runner" or "Covert Ops"||h.SourceHull is "Prospect" or "Astero" or "Stratios" or "Odysseus"||h.SourceHull=="Etana")Special("CovertCloak");
  else if(h.SourceHull=="Endurance")Special("Cloak");
  int scale=h.Class=="Capital Industrial Ship"?4:Math.Min(3,p.WeaponSize);bool shield=h.Faction is "Amur Cooperative Compact" or "Kepler Resource Group" or "Palisade Consortium"||h.SourceHull is "Scimitar" or "Scalpel";
  foreach(var d in DefenseCatalog.All.Where(d=>d.Size==scale&&d.Kind==(shield?"ShieldBoost":"ArmorRepair")).OrderBy(d=>d.Power).ThenBy(d=>d.Cpu)){int slot=Enumerable.Range(0,12).First(i=>Ship.DefenseSlot(i)==d.Slot);if(DefenseFitError(d,slot).Length==0){Player.Defense[slot]=DefenseState.Create(d.Id);break;}}
  foreach(var d in MobilityCatalog.All.Where(d=>d.Kind=="AB"&&d.Size==Math.Min(3,p.WeaponSize)).OrderBy(d=>d.Power)){int slot=Array.FindIndex(Player.Mobility,s=>s.Definition==null);if(MobilityFitError(d,slot).Length==0){Player.Mobility[slot]=new(){ModuleId=d.Id};break;}}
  if(p.Role=="Hauling"){foreach(var d in MobilityCatalog.All.Where(d=>d.Kind=="Cargo").OrderBy(d=>d.Cpu)){int slot=Enumerable.Range(0,Player.Mobility.Length).FirstOrDefault(i=>Ship.MobilitySlot(i)==d.Slot&&Player.Mobility[i].Definition==null,-1);if(slot>=0&&MobilityFitError(d,slot).Length==0){Player.Mobility[slot]=new(){ModuleId=d.Id};break;}}}
  var gun=WeaponCatalog.All.Where(w=>w.Size==p.WeaponSize&&w.Family==p.WeaponFamily).OrderBy(w=>w.Power).ThenBy(w=>w.Cpu).FirstOrDefault();
  if(gun!=null){int count=(int)h.Value(gun.UsesLauncher?"launcherSlotsLeft":"turretSlotsLeft");for(int i=0;i<Math.Min(count,Player.Weapons.Length);i++){if(FitError(gun,i).Length>0)break;Player.Weapons[i]=WeaponState.Create(gun.Id);var c=ChargeCatalog.Default(gun);if(c!=null){Player.Weapons[i].Loaded=c.Crystal?1:gun.Magazine;float qty=c.Crystal?1:(float)Math.Min(gun.Magazine*2,Math.Floor(CargoRoom(c.Id)/ItemVolume(c.Id)));if(qty>0)CargoItems[c.Id]=CargoItems.GetValueOrDefault(c.Id)+(decimal)qty;}}}
  string droneKind=p.Role=="Mining"&&h.Class is "Industrial Command Ship" or "Capital Industrial Ship"?"Mine":"Combat";
  var drones=DroneCatalog.All.Where(d=>!d.Fighter&&d.Kind==droneKind).Select(d=>(d,count:Math.Min(5,(int)Math.Min(h.Value("droneCapacity")/d.Volume,h.Value("droneBandwidth")/d.Bandwidth)))).Where(x=>x.count>0).OrderByDescending(x=>x.d.Yield*x.count+x.d.Packet.Total*x.count).FirstOrDefault();
  if(drones.d!=null)for(int i=0;i<drones.count;i++)Player.Drones.Add(DroneUnit.Create(drones.d.Id));RefreshRigDroneHealth();
  if(Player.Stat("canJump")>0)CargoItems["jump_fuel"]=(decimal)Math.Min(60000,(float)Math.Floor(CargoRoom("jump_fuel")/ItemVolume("jump_fuel")));
  RecomputeWeaponFit();Player.Shield=Player.MaxShield;Player.Armor=Player.MaxArmor;Player.Hull=Player.MaxHull;Player.Cap=Player.MaxCap;
 }
}
