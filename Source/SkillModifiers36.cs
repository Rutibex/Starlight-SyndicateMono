using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed class SkillModifier36
{
 public int Skill{get;set;}public string Attribute{get;set;}="";public double Value{get;set;}public bool Scaled{get;set;}public int Operation{get;set;}public string Domain{get;set;}="";public string Function{get;set;}="";public int RequiredSkill{get;set;}public int Group{get;set;}
}
public sealed partial class Ship
{
 [JsonIgnore] readonly Dictionary<(string,string),double> skillFactorCache=[];
 public void ClearSkillCache()=>skillFactorCache.Clear();
 public double SkillItemValue(string item,string attribute,double value,bool character=false)
 {
  if(SkillCatalog.VerificationAllV)return value;
  bool ship=item==CombatHull&&!character;int type=SkillCatalog.TypeId(item);int group=SkillCatalog.Database.TypeGroups.GetValueOrDefault(type.ToString());var req=SkillCatalog.Required(type);
  foreach(var m in (SkillCatalog.ModifiersByAttribute.GetValueOrDefault(attribute)??[]).OrderBy(m=>m.Operation).ThenBy(m=>m.Skill)){int level=SkillLevels.GetValueOrDefault(m.Skill);if(level==0)continue;
   bool match=m.Function switch{"ItemModifier"=>ship&&m.Domain=="shipID"||character&&m.Domain=="charID","LocationRequiredSkillModifier" or "OwnerRequiredSkillModifier"=>!ship&&req.Any(r=>r.Skill==m.RequiredSkill),"LocationGroupModifier"=>!ship&&group==m.Group,"LocationModifier"=>!ship,_=>false};if(!match)continue;
   double amount=m.Value*(m.Scaled?level:1);var applied=Dogma45.Operation(value,amount,m.Operation);if(applied.Supported)value=applied.Value!.Value;
  }return value;
 }
 public float SkillItemFactor(string item,string attribute){if(SkillCatalog.VerificationAllV)return 1;var key=(item,attribute);if(!skillFactorCache.TryGetValue(key,out double factor))skillFactorCache[key]=factor=SkillItemValue(item,attribute,1);return (float)factor;}
 public float SkillWeaponFactor(WeaponDefinition w,string key)=>(w.Kind==WeaponKind.Missile&&key=="damage"?MissileSkillFactor("damage"):1)*SkillItemFactor(w.Id,key switch{"cpu"=>"cpu","power"=>"power","damage"=>"damageMultiplier","cycle"=>"speed","optimal"=>"maxRange","falloff"=>"falloff","tracking"=>"trackingSpeed","cap"=>"capacitorNeed",_=>key});
 public double SkillShipValue(string key,double value){if(SkillCatalog.VerificationAllV)return value;value=SkillItemValue(CombatHull,key,value);return key switch{
 "maxLockedTargets"=>Math.Min(value,SkillItemValue("",key,2,true)),"maxActiveDrones"=>Math.Min(value,SkillItemValue("","maxActiveDrones",0,true)),"droneControlDistance"=>value+SkillItemValue("","droneControlDistance",0,true),"miningYield"=>value*SkillItemFactor("miner","miningAmount"),"turretCycle"=>value*SkillItemFactor("rail","speed"),"turretCap"=>value*SkillItemFactor("rail","capacitorNeed"),"missileCycle"=>value*SkillItemFactor("missile","speed"),"webCap"=>value*SkillItemFactor("web","capacitorNeed"),_=>value};}
}
public sealed partial class Ship
{
 public float MissileSkillFactor(string key)=>SkillCatalog.VerificationAllV?1:key switch{"damage"=>1+.02f*SkillLevel("Warhead Upgrades"),"velocity"=>1+.1f*SkillLevel("Missile Projection"),"flightTime"=>1+.1f*SkillLevel("Missile Bombardment"),"explosionRadius"=>1-.05f*SkillLevel("Guided Missile Precision"),"explosionVelocity"=>1+.1f*SkillLevel("Target Navigation Prediction"),_=>1};
 public float SkillEwarFactor(EwarDefinition d,string key)=>SkillItemFactor(d.Id,key switch{"cap"=>"capacitorNeed","optimal"=>"maxRange","falloff"=>"falloff","duration"=>"duration","strength"=>d.Kind switch{EwarKind.ECM=>"scanRadarStrengthBonus",EwarKind.Damp=>"maxTargetRangeBonus",EwarKind.Paint=>"signatureRadiusBonus",EwarKind.Neutralizer=>"energyNeutralizerAmount",_=>"trackingSpeedBonus"},_=>key});
}
