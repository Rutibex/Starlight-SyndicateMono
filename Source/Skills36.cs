using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record SkillRequirement(int Skill,int Level);
public sealed class SkillDefinition
{
 public int Id{get;set;} public string Name{get;set;}="";public string SourceName{get;set;}="";public string Group{get;set;}="";public string Description{get;set;}="";
 public double Rank{get;set;}=1;public int Primary{get;set;}public int Secondary{get;set;}public decimal Price{get;set;}
 public SkillRequirement[] Requirements{get;set;}=[];public Dictionary<string,double> Dogma{get;set;}=[];public JsonElement[] Effects{get;set;}=[];
 public long Threshold(int level)=>level<=0?0:(long)Math.Ceiling(250*Rank*Math.Pow(2,2.5*(Math.Clamp(level,1,5)-1)));
 public decimal BookCost=>Math.Max(30,Math.Ceiling(Price/1000));
}
public sealed class SkillTypeRequirements {public int TypeId{get;set;}public string Name{get;set;}="";public int Category{get;set;}public SkillRequirement[] Requirements{get;set;}=[];}
public sealed class SkillDatabase
{
 public SkillModifier36[] Modifiers{get;set;}=[];public Dictionary<string,int> TypeGroups{get;set;}=[];public int Build{get;set;}public string Release{get;set;}="";public string Source{get;set;}="";public string Sha256{get;set;}="";
 public SkillDefinition[] Skills{get;set;}=[];public Dictionary<string,SkillTypeRequirements> Types{get;set;}=[];public Dictionary<string,int> GameTypes{get;set;}=[];
}
public static class SkillCatalog
{
 public static readonly SkillDatabase Database=Retheme41.Apply(JsonSerializer.Deserialize<SkillDatabase>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Skills36.json")))!);
 public static readonly SkillDefinition[] All=Database.Skills;public static readonly Dictionary<int,SkillDefinition> ById=All.ToDictionary(s=>s.Id);
 public static readonly string[] Groups=All.Select(s=>s.Group).Distinct().Order().ToArray();
 public static readonly Dictionary<int,string> AttributeNames=new(){[164]="Charisma",[165]="Intelligence",[166]="Memory",[167]="Perception",[168]="Willpower"};
 public static int Id(string source)=>All.Single(s=>s.SourceName==source).Id;
 public static int TypeId(string id)=>id.StartsWith("skill_")&&int.TryParse(id[6..],out int skill)?skill:Database.GameTypes.GetValueOrDefault(id);
 public static SkillRequirement[] Required(string gameId)=>Required(TypeId(gameId));
 public static SkillRequirement[] Required(int type)=>Database.Types.GetValueOrDefault(type.ToString())?.Requirements??[];
 // Only old subsystem verification commands use this explicit all-V fixture; never set during gameplay.
 public static readonly Dictionary<string,SkillModifier36[]> ModifiersByAttribute=Database.Modifiers.GroupBy(m=>m.Attribute).ToDictionary(g=>g.Key,g=>g.ToArray());
 public static bool VerificationAllV;
}
public sealed record SkillQueueEntry(int Skill,int Level);
public sealed class SkillPlan
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Name{get;set;}="New skill plan";public string Description{get;set;}="";
 public List<SkillQueueEntry> Entries{get;set;}=[];public List<string> Milestones{get;set;}=[];public bool Certified{get;set;}
}
public sealed class ExpertLease {public string Name{get;set;}="";public DateTime ExpiresUtc{get;set;}public Dictionary<int,int> Levels{get;set;}=[];}
public sealed class PilotSkills
{
 public Dictionary<int,double> Points{get;set;}=[];public HashSet<int> Injected{get;set;}=[];public List<SkillQueueEntry> Queue{get;set;}=[];
 public Dictionary<int,int> Attributes{get;set;}=new(){[164]=19,[165]=20,[166]=20,[167]=20,[168]=20};public Dictionary<int,int> Implants{get;set;}=[];
 [System.Text.Json.Serialization.JsonIgnore] public Func<DateTime>? Clock;
 public DateTime Now=>Clock?.Invoke()??DateTime.UtcNow;
 public double Unallocated{get;set;}public bool Paused{get;set;}public DateTime ProcessedUtc{get;set;}=DateTime.UtcNow;
 public DateTime NextRemapUtc{get;set;}=DateTime.MinValue;public int BonusRemaps{get;set;}=2;
 public int AcceleratorBonus{get;set;}public DateTime AcceleratorExpiresUtc{get;set;}
 public List<SkillPlan> Plans{get;set;}=[];public string TrackedPlan{get;set;}="";public List<ExpertLease> Leases{get;set;}=[];public List<string> History{get;set;}=[];
 [JsonIgnore] public long TotalPoints=>(long)Points.Values.Sum();
 public int Trained(int id){if(!SkillCatalog.ById.TryGetValue(id,out var s))return 0;double p=Points.GetValueOrDefault(id);for(int level=5;level>0;level--)if(p>=s.Threshold(level))return level;return 0;}
 public int Effective(int id,DateTime? now=null)=>SkillCatalog.VerificationAllV?5:Math.Max(Trained(id),Leases.Where(l=>l.ExpiresUtc>(now??Now)).Select(l=>l.Levels.GetValueOrDefault(id)).DefaultIfEmpty().Max());
 public double Rate(int id,DateTime at){var s=SkillCatalog.ById[id];double Attr(int a)=>Attributes.GetValueOrDefault(a,20)+Implants.GetValueOrDefault(a)+(at<AcceleratorExpiresUtc?AcceleratorBonus:0);return Attr(s.Primary)+Attr(s.Secondary)/2;}
 public bool Prerequisites(int id,Dictionary<int,int>? projected=null)=>SkillCatalog.ById[id].Requirements.All(r=>(projected?.GetValueOrDefault(r.Skill)??Trained(r.Skill))>=r.Level);
 public List<SkillQueueEntry> Expand(IEnumerable<SkillRequirement> goals,bool useQueue=true)
 {
  var levels=SkillCatalog.All.ToDictionary(s=>s.Id,s=>Trained(s.Id));if(useQueue)foreach(var q in Queue)levels[q.Skill]=Math.Max(levels[q.Skill],q.Level);
  var result=new List<SkillQueueEntry>();var visiting=new HashSet<int>();
  void Add(int id,int level){if(!SkillCatalog.ById.ContainsKey(id)||level is <1 or >5)throw new InvalidDataException("Unknown skill or level");if(levels[id]>=level)return;if(!visiting.Add(id))throw new InvalidDataException("Cyclic prerequisites");foreach(var r in SkillCatalog.ById[id].Requirements)Add(r.Skill,r.Level);for(int l=levels[id]+1;l<=level;l++)result.Add(new(id,l));levels[id]=level;visiting.Remove(id);}
  foreach(var g in goals)Add(g.Skill,g.Level);return result;
 }
 public string Add(IEnumerable<SkillRequirement> goals){Advance(Now);var add=Expand(goals);if(Queue.Count+add.Count>150)return "Training queue is limited to 150 skill levels. No entries added.";Queue.AddRange(add);return "";}
 public string Validate(IEnumerable<SkillQueueEntry> entries){var levels=SkillCatalog.All.ToDictionary(s=>s.Id,s=>Trained(s.Id));foreach(var q in entries){if(!SkillCatalog.ById.ContainsKey(q.Skill)||q.Level is <1 or >5)return "Unknown skill or level.";if(q.Level<=levels[q.Skill])continue;if(q.Level!=levels[q.Skill]+1||!Prerequisites(q.Skill,levels))return "This order would train a skill before its prerequisites.";levels[q.Skill]=q.Level;}return "";}
 public bool Move(int from,int to){Advance(Now);if(from<0||from>=Queue.Count||to<0||to>=Queue.Count)return false;var copy=Queue.ToList();var q=copy[from];copy.RemoveAt(from);copy.Insert(to,q);if(Validate(copy).Length>0)return false;Queue=copy;return true;}
 public void Remove(int index){Advance(Now);if(index<0||index>=Queue.Count)return;Queue.RemoveAt(index);while(Validate(Queue).Length>0){var levels=SkillCatalog.All.ToDictionary(s=>s.Id,s=>Trained(s.Id));int bad=-1;for(int i=0;i<Queue.Count;i++){var q=Queue[i];if(q.Level>levels[q.Skill]&&(q.Level!=levels[q.Skill]+1||!Prerequisites(q.Skill,levels))){bad=i;break;}levels[q.Skill]=Math.Max(levels[q.Skill],q.Level);}if(bad<0)break;Queue.RemoveAt(bad);}}
 public void Advance(DateTime now)
 {
  if(now<=ProcessedUtc)return;DateTime at=ProcessedUtc;ProcessedUtc=now;if(Paused)return;
  while(Queue.Count>0&&at<now){var q=Queue[0];if(!SkillCatalog.ById.TryGetValue(q.Skill,out var s)||!Injected.Contains(q.Skill)||!Prerequisites(q.Skill))break;
   double need=Math.Max(0,s.Threshold(q.Level)-Points.GetValueOrDefault(q.Skill));double perSecond=Rate(q.Skill,at)/60;
   DateTime boundary=AcceleratorExpiresUtc>at&&AcceleratorExpiresUtc<now?AcceleratorExpiresUtc:now;double available=(boundary-at).TotalSeconds;
   if(need>available*perSecond){Points[q.Skill]=Points.GetValueOrDefault(q.Skill)+available*perSecond;at=boundary;continue;}
   Points[q.Skill]=s.Threshold(q.Level);at=at.AddSeconds(need/perSecond);Queue.RemoveAt(0);History.Add(at.ToString("u")+" - "+s.Name+" "+q.Level);if(History.Count>100)History.RemoveAt(0);
  }
 }
 public double Seconds(IEnumerable<SkillQueueEntry> entries){var points=new Dictionary<int,double>(Points);double seconds=0;DateTime at=Now;foreach(var q in entries){var s=SkillCatalog.ById[q.Skill];double need=Math.Max(0,s.Threshold(q.Level)-points.GetValueOrDefault(q.Skill));if(at<AcceleratorExpiresUtc){double boosted=Rate(q.Skill,at),capacity=(AcceleratorExpiresUtc-at).TotalMinutes*boosted;double used=Math.Min(need,capacity);double sec=used/boosted*60;seconds+=sec;at=at.AddSeconds(sec);need-=used;}double tail=need/Rate(q.Skill,at)*60;seconds+=tail;at=at.AddSeconds(tail);points[q.Skill]=Math.Max(points.GetValueOrDefault(q.Skill),s.Threshold(q.Level));}return seconds;}
 public string Apply(int id,int level){Advance(Now);if(!Injected.Contains(id)||!Prerequisites(id))return "Inject this skill and train its prerequisites first.";var s=SkillCatalog.ById[id];double amount=Math.Min(Unallocated,Math.Max(0,s.Threshold(level)-Points.GetValueOrDefault(id)));if(amount<=0)return "No unallocated points available.";Unallocated-=amount;Points[id]=Points.GetValueOrDefault(id)+amount;Queue.RemoveAll(q=>q.Skill==id&&q.Level<=Trained(id));return "";}
 public string Remap(Dictionary<int,int> values,DateTime now){Advance(now);if(values.Count!=5||SkillCatalog.AttributeNames.Keys.Any(k=>!values.ContainsKey(k))||values.Values.Any(v=>v<17||v>27)||values.Values.Sum()!=99)return "Allocate 99 points across five attributes (17 to 27 each).";if(now<NextRemapUtc&&BonusRemaps<=0)return "Annual remap unavailable until "+NextRemapUtc.ToString("u");if(now<NextRemapUtc)BonusRemaps--;else NextRemapUtc=now.AddDays(365);Attributes=new(values);return "";}
 public void Grant(IEnumerable<SkillRequirement> goals){foreach(var q in Expand(goals,false)){Injected.Add(q.Skill);Points[q.Skill]=Math.Max(Points.GetValueOrDefault(q.Skill),SkillCatalog.ById[q.Skill].Threshold(q.Level));}}
}
public sealed partial class Ship
{
 public Dictionary<int,int> SkillLevels{get;set;}=[];
 public int SkillLevel(string name)=>SkillCatalog.VerificationAllV?5:SkillLevels.GetValueOrDefault(SkillCatalog.Id(name));
 public double SkillShipStat(string key,double value)=>SkillShipValue(key,value);
 public float SkillWeapon(WeaponDefinition w,string key)=>SkillWeaponFactor(w,key);
}
public sealed partial class DemoWorld
{
 public PilotSkills Skills{get;set;}=new();public bool IgnoreSkillRequirements{get;set;}
 public string SkillError(string item){if(Commander.Capsule&&!FleetCatalog.ById.ContainsKey(item)&&!CombatHulls.All.Any(h=>h.Id==item))return "Board a ship before fitting equipment.";if(IgnoreSkillRequirements||SkillCatalog.VerificationAllV)return "";var missing=SkillCatalog.Required(item).Where(r=>Skills.Effective(r.Skill)<r.Level).ToArray();return missing.Length==0?"":"Requires "+string.Join(", ",missing.Select(r=>SkillCatalog.ById[r.Skill].Name+" "+r.Level));}
 public bool SkillAllowed(string item){string e=SkillError(item);if(e.Length==0)return true;Say(e);return false;}
 public void SyncSkills(){Player.CapsuleD=Commander.Capsule;Player.PilotShieldD=Commander.Capsule?1:ImplantShieldD;Skills.Clock=()=>Now;if(SkillCatalog.VerificationAllV)return;Skills.Advance(Now);EnforceQualificationsD();Astrometrics=Skills.Effective(SkillCatalog.Id("Astrometrics"));AstrometricRangefinding=Skills.Effective(SkillCatalog.Id("Astrometric Rangefinding"));AstrometricPinpointing=Skills.Effective(SkillCatalog.Id("Astrometric Pinpointing"));AstrometricAcquisition=Skills.Effective(SkillCatalog.Id("Astrometric Acquisition"));var levels=SkillCatalog.All.ToDictionary(s=>s.Id,s=>Skills.Effective(s.Id));foreach(var ship in FleetBerths.Select(b=>b.Ship).Concat(ParkedShips.Select(b=>b.Ship)).Append(Player)){if(!ship.SkillLevels.OrderBy(k=>k.Key).SequenceEqual(levels.OrderBy(k=>k.Key)))ship.ClearSkillCache();ship.SkillLevels=levels;ship.HullProficiency=new(levels);foreach(string group in new[]{"Armor","Shield","Drone","Hybrid","Energy","Projectile","Launcher","Astronautic","Electronic","Energy Weapon"}){var skill=SkillCatalog.All.FirstOrDefault(s=>s.SourceName==group+" Rigging");if(skill!=null)ship.RiggingSkills[group]=levels[skill.Id];}}}
 public void InitializeSkills(bool migration=false){Skills=new(){ProcessedUtc=Now,Clock=()=>Now};var goals=new List<SkillRequirement>();foreach(string name in new[]{"Spaceship Command","CPU Management","Power Grid Management","Gunnery","Missile Launcher Operation","Navigation","Drones","Science","Astrometrics","Mining","Mechanics","Hull Upgrades","Shield Operation"})goals.Add(new(SkillCatalog.Id(name),name=="Drones"?3:2));foreach(var ship in (migration?FleetBerths.Select(b=>b.Ship).Concat(ParkedShips.Select(b=>b.Ship)).Append(Player):new[]{Player})){goals.AddRange(SkillCatalog.Required(ship.CombatHull));foreach(string item in FittingDesignStore.Equipment(ship).Keys)goals.AddRange(SkillCatalog.Required(item));}Skills.Grant(goals);if(migration){foreach(var (name,level) in new[]{("Astrometrics",Astrometrics),("Astrometric Rangefinding",AstrometricRangefinding),("Astrometric Pinpointing",AstrometricPinpointing),("Astrometric Acquisition",AstrometricAcquisition)})if(level>0)Skills.Grant([new(SkillCatalog.Id(name),Math.Clamp(level,1,5))]);foreach(var (group,level) in Player.RiggingSkills){var rigSkill=SkillCatalog.All.FirstOrDefault(s=>s.SourceName==group+" Rigging");if(rigSkill!=null&&level>0)Skills.Grant([new(rigSkill.Id,Math.Clamp(level,1,5))]);}foreach(var (id,level) in Player.HullProficiency)if(SkillCatalog.ById.ContainsKey(id))Skills.Grant([new(id,Math.Clamp(level,1,5))]);Skills.History.Add("Pass 35 migration: existing hulls, fittings and proficiency preserved.");}SyncSkills();}
 public bool BuySkillBooks(IEnumerable<int> ids){Skills.Advance(Now);var missing=ids.Distinct().Where(i=>SkillCatalog.ById.ContainsKey(i)&&!Skills.Injected.Contains(i)).ToArray();decimal cost=missing.Where(i=>ItemCount("skill_"+i,true)<1&&ItemCount("skill_"+i,false)<1).Sum(i=>SkillCatalog.ById[i].BookCost);if(Balance<cost){Say("Skillbooks cost "+cost.ToString("N0")+" CR.");return false;}WalletChange(-cost,"Skillbook direct purchase / "+missing.Length);foreach(int id in missing){string item="skill_"+id;if(ItemCount(item,true)>=1)ChangeStack(item,true,-(decimal)(1));else if(ItemCount(item,false)>=1)ChangeStack(item,false,-(decimal)(1));Skills.Injected.Add(id);}Say("Purchased and injected "+missing.Length+" skillbooks.");return true;}
 public string QueueSkill(int id,int level){string error=Skills.Add([new(id,level)]);if(error.Length==0){Say("Added skill and missing prerequisites to training queue.");Save();}return error;}
 public string QueueRequirements(string item){string error=Skills.Add(SkillCatalog.Required(item));if(error.Length==0)Save();return error;}
 public string BuyInjector()=>BuyConsumableD("d_injector");
 public string Extract(int id)=>ExtractD(id);
 bool PointsLocked(int id)=>SkillCatalog.All.Any(s=>Skills.Trained(s.Id)>0&&s.Requirements.Any(r=>r.Skill==id&&Skills.Points.GetValueOrDefault(id)-500000<SkillCatalog.ById[id].Threshold(r.Level)));
}
public sealed partial class DemoWorld
{
 public bool InjectOwnedSkillBook(string item,bool hangar){if(!item.StartsWith("skill_")||!int.TryParse(item[6..],out int id)||!SkillCatalog.ById.ContainsKey(id)||Skills.Injected.Contains(id)||ItemCount(item,hangar)<1)return false;Skills.Advance(Now);ChangeStack(item,hangar,-(decimal)(1));Skills.Injected.Add(id);Say("Injected "+SkillCatalog.ById[id].Name);return true;}
}
