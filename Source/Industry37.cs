using System.Text.Json;
namespace StarlightSyndicateDemo;
public enum IndustryActivity { Manufacturing, MaterialResearch, TimeResearch, Copying }
public sealed class IndustryItem37 {public string Id{get;set;}="";public int TypeId{get;set;}public string Name{get;set;}="";public string Group{get;set;}="";public string Description{get;set;}="";public float Volume{get;set;}public decimal Price{get;set;}public string Slot{get;set;}="";}
public sealed class BlueprintDefinition37
{
 public int TypeId{get;set;}public int BlueprintTypeId{get;set;}public int MaxRuns{get;set;}=600;public int Output{get;set;}=1;
 public double Seconds{get;set;}=300;public double CopySeconds{get;set;}=240;public double MeSeconds{get;set;}=105;public double TeSeconds{get;set;}=105;public bool CanResearch{get;set;}=true;
 public Dictionary<string,double> MineralComposition{get;set;}=[];public Dictionary<string,long> Materials{get;set;}=[];public SkillRequirement[] Skills{get;set;}=[];public Dictionary<string,SkillRequirement[]> ActivitySkills{get;set;}=[];
}
public sealed class IndustryDatabase37
{
 public int Build{get;set;}public string Source{get;set;}="";public string Sha256{get;set;}="";public Dictionary<string,BlueprintDefinition37> Blueprints{get;set;}=[];
 public IndustryItem37[] ExtraItems{get;set;}=[];public Dictionary<string,Dictionary<string,long>> Recovery{get;set;}=[];public Dictionary<string,int> Portions{get;set;}=[];
}
public sealed class IndustryRecipe37
{
 public string Item{get;set;}="";public string Provenance{get;set;}="";public BlueprintDefinition37 Definition{get;set;}=new();public Dictionary<string,long> Native{get;set;}=[];public Dictionary<string,long> Minerals{get;set;}=[];
 public bool Imported=>Definition.BlueprintTypeId!=0;
}
public static class IndustryCatalog37
{
 public static readonly IndustryDatabase37 Database=JsonSerializer.Deserialize<IndustryDatabase37>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Industry37.json")))!;
 public static readonly int[] MineralTypes=[34,35,36,37,38,39,40,11399];
 public static readonly ItemDefinition[] ExtraItems=Database.ExtraItems.Select(i=>new ItemDefinition(i.Id,i.Name,i.Group,i.Description,i.Volume,i.Price,i.Slot)).ToArray();
 static Dictionary<string,IndustryRecipe37>? recipes;static Dictionary<int,string>? gameTypes;
 public static Dictionary<string,IndustryRecipe37> Recipes {get{if(recipes!=null)return recipes;gameTypes=SkillCatalog.Database.GameTypes.Where(p=>ItemCatalog.All.Any(i=>i.Id==p.Key)).GroupBy(p=>p.Value).ToDictionary(g=>g.Key,g=>g.First().Key);foreach(var i in Database.ExtraItems)gameTypes.TryAdd(i.TypeId,i.Id);recipes=[];foreach(var i in ItemCatalog.All)recipes[i.Id]=Build(i);return recipes;}}
 public static int Type(string id)=>id.StartsWith("ind_")?int.Parse(id[4..]):SkillCatalog.TypeId(id);
 public static string GameId(string type)=>gameTypes!.GetValueOrDefault(int.Parse(type),"ind_"+type);
 static IndustryRecipe37 Build(ItemDefinition i)
 {
  int type=Type(i.Id);var source=Database.Blueprints.GetValueOrDefault(type.ToString());var def=source??new BlueprintDefinition37{TypeId=type,Seconds=i.Slot=="ship"?7200:300,CopySeconds=i.Slot=="ship"?5760:240,MaxRuns=i.Slot=="ship"?10:600,Output=i.Slot=="charge"?100:1,Skills=[new(SkillCatalog.Id("Industry"),i.Slot=="ship"?3:1)]};
  var native=source?.Materials.ToDictionary(m=>GameId(m.Key),m=>m.Value)??new();
  // Mineral-only fabrication is a Starlight economic adaptation, not a claim that EVE uses these recipes.
  decimal budget=Math.Max(5,i.Price*.65m)*def.Output;double[] weights=i.Slot=="ship"?[.40,.20,.15,.10,.08,.04,.02,.01]:[.55,.22,.13,.06,.03,.01,0,0];
  if(source?.MineralComposition.Count>0){decimal total=source.MineralComposition.Sum(m=>(decimal)m.Value*ExtraItems.First(e=>e.Id=="ind_"+m.Key).Price);if(total>0)weights=MineralTypes.Select(t=>(double)((decimal)source.MineralComposition.GetValueOrDefault(t.ToString())*ExtraItems.First(e=>e.Id=="ind_"+t).Price/total)).ToArray();}
  var minerals=new Dictionary<string,long>();for(int n=0;n<8;n++)if(weights[n]>0){string id="ind_"+MineralTypes[n];decimal price=ExtraItems.First(m=>m.Id==id).Price;minerals[id]=Math.Max(1,(long)Math.Ceiling(budget*(decimal)weights[n]/price));}
  if(OreContents(i.Id) is {} feedstock&&feedstock.Materials.Keys.All(k=>k.StartsWith("ind_")))minerals=feedstock.Materials.ToDictionary(m=>m.Key,m=>Math.Max(1,(long)Math.Ceiling(m.Value/(double)feedstock.Portion)));
  if(native.Count==0)native=new(minerals);
  return new(){Item=i.Id,Definition=def,Native=native,Minerals=minerals,Provenance=source==null?"Authored Starlight blueprint; no recorded component design":"CCP SDE "+Database.Build+" / blueprint "+def.BlueprintTypeId};
 }
 public sealed record Feedstock(Dictionary<string,long> Materials,int Portion,string Skill);
 public static Feedstock? OreContents(string id)=>id switch
 {
  "ore" or "compressed_ore"=>new(new(){{"ind_34",300},{"ind_35",120},{"ind_36",40}},100,"Simple Ore Processing"),
  "deepore" or "compressed_deepore"=>new(new(){{"ind_37",100},{"ind_38",80},{"ind_39",40},{"ind_40",20},{"ind_11399",4}},100,"Mercoxit Ore Processing"),
  "moonore" or "compressed_moonore"=>new(new(){{"ind_34",400},{"ind_35",160},{"ind_36",120},{"ind_37",40}},100,"Ubiquitous Moon Ore Processing"),
  "ice" or "compressed_ice"=>new(new(){{"jump_fuel",25},{"fuel",50}},1,"Ice Processing"),_=>null
 };
 public static SkillRequirement[] BuildSkills(IndustryRecipe37 recipe,bool native)=>native?recipe.Definition.Skills:recipe.Definition.Skills.Concat(new[]{new SkillRequirement(SkillCatalog.Id("Industry"),ItemCatalog.Get(recipe.Item).Slot=="ship"?3:1)}).GroupBy(r=>r.Skill).Select(g=>new SkillRequirement(g.Key,g.Max(r=>r.Level))).ToArray();
 public static Dictionary<string,long> Materials(IndustryRecipe37 recipe,bool native,int runs,int me,decimal facility)
 {return (native?recipe.Native:recipe.Minerals).ToDictionary(m=>m.Key,m=>Math.Max(runs,checked((long)Math.Ceiling(Math.Round(m.Value*(decimal)runs*(1-me/100m)*facility,2)))));}
 public static readonly double[] ResearchTotals=[0,105,250,595,1414,3360,8000,19000,45255,107700,256000];
}
public sealed class OwnedBlueprint37
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Item{get;set;}="";public string Station{get;set;}="";public bool Original{get;set;}=true;public int Runs{get;set;}public int ME{get;set;}public int TE{get;set;}public string LockedJob{get;set;}="";
}
public sealed class IndustryJob37
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Blueprint{get;set;}="";public string Item{get;set;}="";public string Station{get;set;}="";public IndustryActivity Activity{get;set;}
 public string State{get;set;}="Active";public int Runs{get;set;}public int Copies{get;set;}public int Target{get;set;}public int ME{get;set;}public int TE{get;set;}public bool Native{get;set;}
 public DateTime StartUtc{get;set;}public DateTime FinishUtc{get;set;}public double Seconds{get;set;}public decimal Fee{get;set;}public Dictionary<string,long> Inputs{get;set;}=[];
}
public sealed class IndustryWork37 {public string Station{get;set;}="";public IndustryActivity Activity{get;set;}public DateTime AtUtc{get;set;}public double Hours{get;set;}}
public sealed class IndustryState37 {public List<IndustryWork37> Work{get;set;}=[];public List<OwnedBlueprint37> Blueprints{get;set;}=[];public List<IndustryJob37> Jobs{get;set;}=[];public Dictionary<string,double> WorkHours{get;set;}=[];public Dictionary<string,double> Standings{get;set;}=[];public double Implant{get;set;}public string Facility{get;set;}="Station";}
public sealed record IndustryQuote37(Dictionary<string,long> Inputs,double Seconds,decimal Fee,string Error);
public sealed record ReprocessQuote37(Dictionary<string,long> Inputs,Dictionary<string,long> Outputs,Dictionary<string,long> Gross,decimal Fee,string Error,double Yield,double Tax);
public sealed partial class DemoWorld
{
 public IndustryState37 Industry{get;set;}=new();
 public int IndustryLevel(string name)=>Skills.Trained(SkillCatalog.Id(name));
 public decimal IndustryIndex(string station,IndustryActivity activity){string key=station+"/"+activity;var recent=Industry.Work.Where(w=>w.Activity==activity&&w.AtUtc>Now.AddDays(-28));double local=recent.Where(w=>w.Station==station).Sum(w=>w.Hours);double global=recent.Sum(w=>w.Hours);return (decimal)Math.Clamp(.02+Math.Sqrt(local/(10000+global))*.12,.02,.14);}
 public int IndustrySlots(bool manufacturing)=>1+IndustryLevel(manufacturing?"Mass Production":"Laboratory Operation")+IndustryLevel(manufacturing?"Advanced Mass Production":"Advanced Laboratory Operation");
 public void InitializeIndustry(){Industry=new();foreach(string id in new[]{"miner","ammo","analyzer"})Industry.Blueprints.Add(new(){Item=id,Station=StationKey44});Hangar["ind_34"]=200;Hangar["ind_35"]=80;}
 public void UpdateIndustry(DateTime now){Industry.Work.RemoveAll(w=>w.AtUtc<now.AddDays(-28));foreach(var job in Industry.Jobs.Where(j=>j.State=="Active"&&j.FinishUtc<=now))job.State="Ready";}
 public decimal BlueprintPrice(string item)=>Math.Max(30,ItemCatalog.Get(item).Price*8);
 public string BuyBlueprint(string item)
 {
  if((CharacterItemsD.AcquisitionOnly(item)||ProductionCatalogE.Restricted(item)))return "Acquisition-only item: no purchasable manufacturing blueprint.";if(!Docked)return "Dock to acquire a blueprint.";if(!ItemCatalog.IsSupported(item)||!LocationService(StationKey44,14)&&!LocationService(StationKey44,15))return "Blueprint or industry service unavailable.";if(!IndustryCatalog37.Recipes.ContainsKey(item))return "Unknown blueprint.";
  decimal price=BlueprintPrice(item);if(Balance<price)return "Insufficient credits.";WalletChange(-price,"Blueprint original / "+ItemName(item));Industry.Blueprints.Add(new(){Item=item,Station=StationKey44});return "";
 }
 public string MoveIndustryBlueprint(string id)
 {
  var bp=Industry.Blueprints.FirstOrDefault(b=>b.Id==id);if(bp==null)return "Blueprint no longer exists.";if(!Docked)return "Dock to move a blueprint between ship and station.";if(bp.LockedJob.Length>0)return "Installed blueprints cannot be moved.";
  if(bp.Station=="cargo"){bp.Station=StationKey44;return "";}if(bp.Station!=StationKey44)return "Travel to the blueprint's station first.";if(Player.CargoMax-CargoUsed<.01f)return "No cargo room for the blueprint.";bp.Station="cargo";return "";
 }
 public IndustryQuote37 QuoteIndustry(string bpId,IndustryActivity activity,int runs=1,int copies=1,int target=1,bool native=false)
 {
  UpdateIndustry(Now);var bp=Industry.Blueprints.FirstOrDefault(b=>b.Id==bpId);if(bp==null)return new([],0,0,"Select an owned blueprint.");if(bp.Station=="cargo")return new([],0,0,"Deposit this blueprint in a station hangar before installing a job.");if((CharacterItemsD.AcquisitionOnly(bp.Item)||ProductionCatalogE.Restricted(bp.Item)))return new([],0,0,"Acquisition-only items cannot be manufactured, copied or researched.");var recipe=IndustryCatalog37.Recipes[bp.Item];var def=recipe.Definition;
  string error=LocationService(bp.Station,activity==IndustryActivity.Manufacturing?14:15)?"":"Activity unavailable at this facility.";if(runs<1||runs>100000||copies<1||copies>1000)error="Invalid job quantity.";
  if(bp.LockedJob.Length>0)error="Blueprint is installed in another job.";
  bool manufacturing=activity==IndustryActivity.Manufacturing;bool complex=!ImportedWorld43&&Industry.Facility=="Engineering complex";decimal material=complex?.99m:1m;double facility=complex?.85:1;
  if(bp.Station!=StationKey44&&JumpDistance(SystemId,bp.Station)>5*IndustryLevel(manufacturing?"Supply Chain Management":"Scientific Networking"))error="Facility exceeds your remote job range.";
  if(!manufacturing&&!bp.Original)error="Copies cannot be researched or copied.";
  if(activity is IndustryActivity.MaterialResearch or IndustryActivity.TimeResearch)
  {int current=activity==IndustryActivity.MaterialResearch?bp.ME:bp.TE/2;if(target<=current||target>10)error="Research target must exceed current level and be at most 10.";if(native&&!def.CanResearch)error="This component licence does not support research.";}
  if(manufacturing&&!bp.Original&&runs>bp.Runs)error="Not enough licensed runs on this copy.";
  if(activity==IndustryActivity.Copying&&runs>def.MaxRuns)error="Runs per copy exceed this blueprint's limit.";
  int active=ActiveSlotsQ(manufacturing?"manufacturing":"science");if(active>=IndustrySlots(manufacturing))error="All industry job slots are occupied.";
  var req=manufacturing?IndustryCatalog37.BuildSkills(recipe,native):def.ActivitySkills.GetValueOrDefault(activity switch{IndustryActivity.MaterialResearch=>"research_material",IndustryActivity.TimeResearch=>"research_time",_=>"copying"},[]);
  var missing=req.Where(s=>!SkillCatalog.ById.ContainsKey(s.Skill)||Skills.Trained(s.Skill)<s.Level).ToArray();if(missing.Length>0)error="Build skills: "+string.Join(", ",missing.Select(s=>(SkillCatalog.ById.GetValueOrDefault(s.Skill)?.Name??s.Skill.ToString())+" "+s.Level));
  var inputs=manufacturing&&runs>0&&runs<=100000?IndustryCatalog37.Materials(recipe,native,runs,bp.ME,material):new Dictionary<string,long>();var storage=StorageAt(bp.Station);
  foreach(var m in inputs)if(storage.GetValueOrDefault(m.Key)<m.Value) {error="Missing materials in blueprint's station hangar.";break;}

  double advanced=1-.03*IndustryLevel("Advanced Industry"),seconds=0,costRuns=runs;
  if(manufacturing)seconds=def.Seconds*runs*(1-bp.TE/100d)*(1-.04*IndustryLevel("Industry"))*advanced*facility;
  else if(activity==IndustryActivity.Copying){seconds=def.CopySeconds*runs*copies*(1-.05*IndustryLevel("Science"))*advanced*facility;costRuns=.02*runs*copies;}
  else if(target is >=1 and <=10){int old=activity==IndustryActivity.MaterialResearch?bp.ME:bp.TE/2;double baseTime=activity==IndustryActivity.MaterialResearch?def.MeSeconds:def.TeSeconds;double steps=IndustryCatalog37.ResearchTotals[target]-IndustryCatalog37.ResearchTotals[old];seconds=steps/105*baseTime*(1-.05*IndustryLevel(activity==IndustryActivity.MaterialResearch?"Metallurgy":"Research"))*advanced*facility;costRuns=.02*steps/105;}
  decimal eiv=(native?recipe.Native:recipe.Minerals).Sum(m=>m.Value*ItemCatalog.Get(m.Key).Price);decimal fee=Math.Round(eiv*(decimal)Math.Max(0,costRuns)*(IndustryIndex(bp.Station,activity)+(complex?.01m:.0025m)+.04m),2);if(Balance<fee)error="Insufficient credits for the installation fee.";
  if(manufacturing&&runs>1&&seconds>86400*30)error="Manufacturing jobs with multiple runs are limited to 30 days. Reduce the run count.";
  if(seconds>86400*3650)error="Job exceeds the supported ten-year duration.";
  return new(inputs,Math.Max(1,seconds),fee,error);
 }
 public string BuyIndustryInputs(string id,bool native,int runs)
 {
  if(runs<1||runs>100000)return "Invalid manufacturing run count.";var bp=Industry.Blueprints.FirstOrDefault(b=>b.Id==id);if(bp==null||bp.Station=="cargo")return "Select a blueprint in a station hangar.";
  if(LocationRegion(bp.Station)!=Current.Region)return "Travel to the blueprint's region to buy its materials.";
  var inputs=IndustryCatalog37.Materials(IndustryCatalog37.Recipes[bp.Item],native,runs,bp.ME,Industry.Facility=="Engineering complex"?.99m:1m);var purchases=new List<(string Item,int Quantity,decimal Price)>();decimal cost=0;
  foreach(var m in inputs){long need=Math.Max(0,m.Value-(long)StorageAt(bp.Station).GetValueOrDefault(m.Key));foreach(var offer in Offers(m.Key,bp.Station,false).OrderBy(o=>o.Price)){long amount=Math.Min(need,offer.Remaining);if(offer.Price>1000000)return "A required material exceeds this market's order price limit.";while(amount>0){int q=(int)Math.Min(100000,amount);purchases.Add((m.Key,q,offer.Price));cost+=q*offer.Price;amount-=q;need-=q;}if(need==0)break;}if(need>0)return "Insufficient local sell-order supply for "+ItemName(m.Key)+". Mine or haul additional materials.";}
  if(Balance<cost)return $"Missing materials cost {cost:N2} CR.";if(purchases.Count==0)return "All required materials are already available.";
  foreach(var p in purchases){string result=SubmitMarket(p.Item,bp.Station,true,p.Quantity,p.Price,0,true);if(!result.StartsWith("Trade complete:"))return result;}return "";
 }
 public string InstallIndustry(string bpId,IndustryActivity activity,int runs=1,int copies=1,int target=1,bool native=false)
 {
  var q=QuoteIndustry(bpId,activity,runs,copies,target,native);if(q.Error.Length>0)return q.Error;var bp=Industry.Blueprints.Single(b=>b.Id==bpId);var storage=StorageAt(bp.Station);
  foreach(var m in q.Inputs)storage[m.Key]-=m.Value;WalletChange(-q.Fee,"Industry installation / "+ItemName(bp.Item));var now=Now;var job=new IndustryJob37{Blueprint=bp.Id,Item=bp.Item,Station=bp.Station,Activity=activity,Runs=runs,Copies=copies,Target=target,ME=bp.ME,TE=bp.TE,Native=native,StartUtc=now,FinishUtc=now.AddSeconds(q.Seconds),Seconds=q.Seconds,Fee=q.Fee,Inputs=q.Inputs};
  if(activity==IndustryActivity.Manufacturing&&!bp.Original)bp.Runs-=runs;
  bp.LockedJob=job.Id;Industry.Jobs.Add(job);string key=bp.Station+"/"+activity;Industry.WorkHours[key]=Industry.WorkHours.GetValueOrDefault(key)+q.Seconds/3600;Industry.Work.Add(new(){Station=bp.Station,Activity=activity,AtUtc=now,Hours=q.Seconds/3600});return "";
 }
 public string DeliverIndustry(string id)
 {
  UpdateIndustry(Now);var j=Industry.Jobs.FirstOrDefault(j=>j.Id==id);if(j==null||j.State!="Ready")return "Job is not ready for delivery.";var bp=Industry.Blueprints.Single(b=>b.Id==j.Blueprint);
  if(j.Activity==IndustryActivity.Manufacturing){var storage=StorageAt(j.Station);long amount=checked((long)j.Runs*IndustryCatalog37.Recipes[j.Item].Definition.Output);storage[j.Item]=storage.GetValueOrDefault(j.Item)+amount;}
  else if(j.Activity==IndustryActivity.MaterialResearch)bp.ME=j.Target;else if(j.Activity==IndustryActivity.TimeResearch)bp.TE=j.Target*2;
  else for(int c=0;c<j.Copies;c++)Industry.Blueprints.Add(new(){Item=j.Item,Station=j.Station,Original=false,Runs=j.Runs,ME=j.ME,TE=j.TE});
  if(j.Activity==IndustryActivity.Manufacturing)RecordExperienceI("industry",1);bp.LockedJob="";j.State="Delivered";if(j.Activity==IndustryActivity.Manufacturing)RecordTaskM("Manufacture");if(!bp.Original&&bp.Runs==0)Industry.Blueprints.Remove(bp);return "";
 }
 public string CancelIndustry(string id)
 {var j=Industry.Jobs.FirstOrDefault(j=>j.Id==id);if(j==null||j.State!="Active")return "Only running jobs can be cancelled.";var bp=Industry.Blueprints.Single(b=>b.Id==j.Blueprint);bp.LockedJob="";j.State="Cancelled";if(!bp.Original&&bp.Runs==0)Industry.Blueprints.Remove(bp);return "";}
 public double IndustryStanding=>EffectiveStanding38(ImportedWorld43?CurrentStation44?.Owner??"":StationCorporation);
 public ReprocessQuote37 QuoteReprocess(Dictionary<string,long> selected)
 {
  var used=new Dictionary<string,long>();var gross=new Dictionary<string,long>();var outputs=new Dictionary<string,long>();string error=Docked?"":"Dock to use reprocessing.";decimal fee=0;double baseYield=ImportedWorld43?CurrentStation44?.ReprocessingEfficiency??0:.5;double lastYield=0,tax=Math.Max(0,(ImportedWorld43?CurrentStation44?.ReprocessingTake??0:.05)-.0075*IndustryStanding);if(ImportedWorld43&&!ServiceAllowed44(5)&&!ServiceAllowed44(6))error="Reprocessing unavailable at this station or standing is too low.";
  foreach(var (id,qty) in selected)
  {
   if(qty<1||qty>1000000000||!IndustryCatalog37.Recipes.TryGetValue(id,out var recipe)){error="Select valid quantities.";continue;}
   if(Hangar.GetValueOrDefault(id)<qty){error="Selected input stack is no longer available.";continue;}
   if(id.StartsWith("ind_")&&IndustryCatalog37.MineralTypes.Contains(IndustryCatalog37.Type(id))){error="Refined minerals cannot be reprocessed.";continue;}
   if(HasDamagedUtilityStock(id,true)||HasDamagedMobilityStock(id,true)||HasDamagedEnhancementStock(id,true)||HasDamagedDefenseStock(id,true)||HasDamagedSupportStock(id,true)||EwarStorage.Any(s=>s.Hangar&&s.System==StationKey44&&s.State.ModuleId==id)||SpecialStorage.Any(s=>s.Hangar&&s.System==StationKey44&&s.State.ModuleId==id)||CrystalConditions.Any(c=>c.Id==id&&c.Hangar&&c.System==StationKey44)){error="Repair or remove conditioned modules before reprocessing.";continue;}
   var feedstock=IndustryCatalog37.OreContents(id);bool raw=feedstock!=null;int portion=feedstock?.Portion??recipe.Definition.Output;long batches=qty/portion;if(batches==0){error="Not enough units for one reprocessing batch.";continue;}used[id]=batches*portion;
   var source=feedstock?.Materials??recipe.Minerals;
   // Mineral-only craft recovery uses the same base materials, preventing buy/build/recycle minting loops.
   double yield=raw?baseYield*(1+.03*IndustryLevel("Reprocessing"))*(1+.02*IndustryLevel("Reprocessing Efficiency"))*(1+.02*IndustryLevel(feedstock!.Skill))*(1+Industry.Implant ):baseYield*(1+.02*IndustryLevel("Scrapmetal Processing"));lastYield=yield;
   foreach(var m in source){long full=checked(m.Value*batches),net=(long)Math.Floor(full*Math.Min(.9063,yield)*(1-tax));gross[m.Key]=gross.GetValueOrDefault(m.Key)+full;outputs[m.Key]=outputs.GetValueOrDefault(m.Key)+net;fee+=full*ItemCatalog.Get(m.Key).Price*(decimal)(yield*tax);}
  }

  if(used.Count==0&&error.Length==0)error="Select at least one input stack.";
  return new(used,outputs,gross,Math.Round(fee,2),error,lastYield,tax);
 }
 public bool AssembleIndustryHull(string id)
 {if(!RequireService44(17))return false;
  if(!Docked||Hangar.GetValueOrDefault(id)<1||!FleetCatalog.ById.TryGetValue(id,out var hull)||!hull.Playable||!SkillAllowed(id))return false;
  var ship=new Ship{HullProficiency=new(Player.HullProficiency)};ship.SetCombatHull(id);FittingSimulation.ClearEquipment(ship);ship.Missiles=0;
  Hangar[id]-=1;FleetBerths.Add(new(){System=StationKey44,Ship=ship,Cargo=[]});if(!GrantedFleetHulls.Contains(id))GrantedFleetHulls.Add(id);
  Say("Assembled "+hull.Name+" into a local berth. Fit modules before undocking.");return BoardFleetBerth(FleetBerths.Count-1);
 }
 public string ReprocessIndustry(Dictionary<string,long> selected)
 {var q=QuoteReprocess(selected);if(q.Error.Length>0)return q.Error;foreach(var m in q.Inputs)Hangar[m.Key]-=m.Value;foreach(var m in q.Outputs)Hangar[m.Key]=Hangar.GetValueOrDefault(m.Key)+m.Value;Say("Reprocessing complete. Minerals delivered to this station's hangar; tax withheld from output.");return "";}
}
