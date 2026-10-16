using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed record HullEffect(string System,string Key,string Filter,float Value,int Skill,string Damage,int Size=1,string Mode="");
public sealed record HullTrait(int Skill,float Bonus,string Text,int Unit);
public sealed record BatchHull(string ShipId,int ReferenceTypeId,string Role,string WeaponFamily,string HomeSystem,decimal Price,bool Starter,bool LegacyCharter,HullEffect[] Effects,HullTrait[] Traits){public int WeaponSize{get;init;}=1;public int Batch{get;init;}=21;}
public static class ShipBatch21
{
 public static readonly BatchHull[] Previous=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch21.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] Current=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch22.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] Battleships=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch23.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] Specialists=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch24.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] AdvancedFrigates=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch25.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] AdvancedDestroyers=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch26.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] AdvancedCruisers=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch27.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] StrategicCommand=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch28.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] EliteBattleships=JsonSerializer.Deserialize<BatchHull[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ShipBatch29.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
 public static readonly BatchHull[] All=Previous.Concat(Current).Concat(Battleships).Concat(Specialists).Concat(AdvancedFrigates).Concat(AdvancedDestroyers).Concat(AdvancedCruisers).Concat(StrategicCommand).Concat(EliteBattleships).ToArray();
 public static readonly Dictionary<string,BatchHull> ById=All.ToDictionary(h=>h.ShipId);
}
public sealed partial class Ship
{
 public string HullTacticalMode{get;set;}="Primary";public float HullTacticalCooldown{get;set;}
 public Dictionary<int,int> HullProficiency{get;set;}=[];
 public float HullFactor(string system,string key,string filter="",string damage="")
 {
  if(!ShipBatch21.ById.TryGetValue(CombatHull,out var h))return 1;
  return ActiveHullEffects.Where(e=>e.System==system&&e.Key==key&&(e.Mode.Length==0||e.Mode==HullTacticalMode)&&(e.Filter.Length==0||e.Filter==filter)&&(e.Damage.Length==0||damage.Length>0&&e.Damage.Split(',').Contains(damage))).Aggregate(1f,(n,e)=>n*Math.Max(key is "cpu" or "power" or "mass"||system=="field"&&key=="signature"?0:.01f,1+e.Value*(e.Skill==0?1:HullProficiency.GetValueOrDefault(e.Skill,1))));
 }
 public float HullFlat(string system,string key)=>ShipBatch21.ById.TryGetValue(CombatHull,out var h)?ActiveHullEffects.Where(e=>e.System==system&&e.Key==key).Sum(e=>e.Value*(e.Skill==0?1:HullProficiency.GetValueOrDefault(e.Skill,1))):0;
 public string HullWeaponFamily(WeaponDefinition w)=>w.Kind==WeaponKind.Defender?"Defender":w.Kind==WeaponKind.Entropic?"Entropic":w.Kind==WeaponKind.Smartbomb?"Smartbomb":w.Kind==WeaponKind.Missile?w.Family.Contains("Rocket",StringComparison.OrdinalIgnoreCase)?"Rocket":"Missile":w.Laser?"Laser":w.Kind==WeaponKind.Turret&&w.Family is "Blasters" or "Railguns"?"Hybrid":w.Kind==WeaponKind.Turret&&w.Family is "Artillery" or "Autocannons"?"Projectile":"none";
 public float HullWeapon(WeaponDefinition w,string key,string damage="")
 {
  if(!ShipBatch21.ById.TryGetValue(CombatHull,out var h))return 1;var filters=new HashSet<string>{HullWeaponFamily(w)};if(w.Kind==WeaponKind.Bomb)filters.Add("Bomb");if(w.Family=="Missile Launcher Torpedo")filters.Add("Torpedo");float security=key=="damage"&&w.Size==1&&HullCapability("damage increased by a percentage equal to -7.5x pilot negative security status")?1+Math.Clamp(-PilotSecurity*.075f,0,.75f):1;
  if(w.Kind==WeaponKind.Missile){if(w.Family=="Missile Launcher Heavy Assault")filters.Add("HeavyAssault");if(w.Family is "Missile Launcher Heavy" or "Missile Launcher Heavy Assault")filters.Add("HeavyAndAssault");if(w.Family is "Missile Launcher Light" or "Missile Launcher Rapid Light" or "Missile Launcher Heavy" or "Missile Launcher Heavy Assault")filters.Add("LightHeavyAssault");filters.Add("Missile");if(w.Family=="Missile Launcher Light")filters.Add("LightMissile");if(w.Family.Contains("Auto"))filters.Add("AutoMissile");if(w.Size==2)filters.Add("MediumMissile");if(w.Family is "Missile Launcher Heavy" or "Missile Launcher Heavy Assault" or "Missile Launcher Rapid Light")filters.Add("HeavyAssaultRapidLight");if(w.Size==3)filters.Add("LargeLauncher");if(w.Family is "Missile Launcher Heavy" or "Missile Launcher Heavy Assault" or "Missile Launcher Rapid Heavy")filters.Add("HeavyMissile");if(w.Family is "Missile Launcher Cruise" or "Missile Launcher Torpedo")filters.Add("CruiseTorpedo");if(w.Family is "Missile Launcher Heavy" or "Missile Launcher Rapid Heavy" or "Missile Launcher Cruise" or "Missile Launcher Torpedo")filters.Add("HeavyCruiseTorpedo");}
  return security*ActiveHullEffects.Where(e=>e.System=="weapon"&&e.Key==key&&(e.Size==-1||e.Size==w.Size)&&(e.Mode.Length==0||e.Mode==HullTacticalMode)&&(e.Filter.Length==0||filters.Contains(e.Filter))&&(e.Damage.Length==0||damage.Length>0&&e.Damage.Split(',').Contains(damage))).Aggregate(1f,(n,e)=>n*Math.Max(key is "cpu" or "power" or "mass"?0:.01f,1+e.Value*(e.Skill==0?1:HullProficiency.GetValueOrDefault(e.Skill,1))));
 }
 public double HullStat(string key,double value)=>FieldStat(key,key=="dscanImmune"?Math.Max(value,HullFlat("hull","dscanImmune")):key=="droneControlDistance"?value+HullFlat("hull","droneControlDistanceAdd"):key=="warpCoreStrength"?value+HullFlat("hull","warpCoreStrengthAdd"):key=="virusStrengthBonus"?Math.Max(value,HullFlat("utility","virusStrength")):key=="maxGangModules"?Math.Max(value,HullFlat("hull","commandSlots")):value*HullFactor("hull",key.Contains("DamageResonance")?key.StartsWith("shield")?"shieldResonance":key.StartsWith("armor")?"armorResonance":key.StartsWith("hull")?"hullResonance":key:key));
 public float HullCpu(string id)=>HullModuleCost(id,"cpu");
 public float HullPower(string id)=>HullModuleCost(id,"power");
 public float HullModuleCost(string id,string key)
 {
  if(MobilityCatalog.ById.TryGetValue(id,out var drive))return HullFactor("prop",key,drive.Kind);
  if(WeaponCatalog.ById.TryGetValue(id,out var w))return HullWeapon(w,key);
  if(SupportCatalog.ById.TryGetValue(id,out var s))return HullFactor("support",key,SupportFilter(s))*(s.Size==2?HullFactor("support",key,SupportFilter(s)+"Medium")/HullFactor("support",key):1);
  if(SpecialCatalog.ById.TryGetValue(id,out var sp))return HullFactor("special",key,sp.Cloak?"Cloak":sp.Kind);
  if(UtilityCatalog.ById.TryGetValue(id,out var u))return u.Harvest?HarvestFactor(u,key):HullFactor("utility",key,u.Probe?"Probe":u.Kind);
  if(EwarCatalog.ById.TryGetValue(id,out var e))return HullFactor("ewar",key,e.Kind is EwarKind.Tracking or EwarKind.Guidance?"Disrupt":e.Kind.ToString());
  if(DefenseCatalog.ById.TryGetValue(id,out var d))return HullFactor("local",key,d.Kind is "ShieldBoost" or "AncillaryShield"?"Shield":d.Kind=="Bulkhead"?"Bulkhead":d.Kind=="FlexShield"?"Flex":d.Kind);
  return 1;
 }
 public static string SupportFilter(SupportDefinition d)=>d.Kind is "Armor" or "AncArmor"?"Armor":d.Kind is "Shield" or "AncShield"?"Shield":d.Kind;
 public SupportDefinition? HullSupport(SupportDefinition? d)
 {if(d==null)return null;string f=SupportFilter(d);return d with{Amount=d.Amount*SkillItemFactor(d.Id,d.Kind=="Shield"?"shieldBonus":"armorDamageAmount")*HullFactor("support","amount",f),Cycle=d.Cycle*SkillItemFactor(d.Id,"duration")*HullFactor("support","cycle",f),Capacitor=d.Capacitor*SkillItemFactor(d.Id,"capacitorNeed")*HullFactor("support","cap",f),Optimal=d.Optimal*SkillItemFactor(d.Id,"maxRange")*HullFactor("support","optimal",f),Falloff=d.Falloff*SkillItemFactor(d.Id,"falloff")*HullFactor("support","falloff",f),Bonuses=d.Bonuses.ToDictionary(b=>b.Key,b=>b.Value*HullFactor("support","strength",f))};}
 public EwarDefinition? HullEwar(EwarDefinition? d)
 {if(d==null)return null;string f=d.Kind is EwarKind.Tracking or EwarKind.Guidance?"Disrupt":d.Kind.ToString();float F(string key)=>SkillEwarFactor(d,key)*HullFactor("ewar",key,f)*(d.Kind is EwarKind.Point or EwarKind.Scram or EwarKind.Web or EwarKind.Grapple?HullFactor("ewar",key,"Tackle"):1);return EwarOverload(d with{Amount=d.Amount*F("strength"),Strength=d.Strength*F("strength")+(d.Kind==EwarKind.Scram?HullFlat("ewar","scramStrengthAdd"):0),Capacitor=d.Capacitor*F("cap"),Optimal=d.Optimal*F("optimal")*SecurityTackleRange(f),Falloff=d.Falloff*F("falloff"),Radius=d.Radius*F("radius")});}
}
public sealed partial class DemoWorld
{
 public bool SetHullTacticalMode(string mode){if(!ShipBatch21.ById.TryGetValue(Player.CombatHull,out var h)||!h.Effects.Any(e=>e.Mode.Length>0)||mode is not ("Primary" or "Secondary" or "Tertiary")||Player.HullTacticalCooldown>0)return false;if(Player.HullTacticalMode==mode)return true;var fractions=DefenseFractions();Player.HullTacticalMode=mode;Player.HullTacticalCooldown=Player.TacticalModeDelay;RestoreDefenseFractions(fractions);RecomputeWeaponFit();Say("Tactical mode: "+Player.TacticalModeLabel(mode)+" / next switch in "+Player.TacticalModeDelay+" seconds.");return true;}
 public bool TrainHullProficiency(int skill)
 {if(!SkillCatalog.VerificationAllV){if(!SkillCatalog.ById.ContainsKey(skill))return false;return QueueSkill(skill,Math.Min(5,Skills.Trained(skill)+1)).Length==0;}if(!Docked||!ShipBatch21.ById.TryGetValue(Player.CombatHull,out var h)||!Player.ActiveHullTraits.Any(t=>t.Skill==skill&&skill!=0))return false;int level=Player.HullProficiency.GetValueOrDefault(skill,1);decimal cost=250*(level+1)*(level+1);if(level>=5||Credits<(float)cost)return false;float armor=Player.Armor/Player.MaxArmor,shield=Player.Shield/Player.MaxShield;WalletChange(-cost,"Hull proficiency training / "+Player.HullName);Player.HullProficiency[skill]=level+1;foreach(var b in FleetBerths)b.Ship.HullProficiency[skill]=level+1;Player.Armor=armor*Player.MaxArmor;Player.Shield=shield*Player.MaxShield;RefreshRigDroneHealth();AdvanceHours(level+1);RecomputeWeaponFit();Say("Hull proficiency trained to "+(level+1));return true;}
}
