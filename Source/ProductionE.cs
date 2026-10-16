namespace StarlightSyndicateDemo;
// Authored Starlight production profile. Durations, deposits and recipes are explicit,
// finite and saved; this profile does not assert complete source-data parity.
public sealed class ProductionStateE
{
 public Dictionary<string,int> Supplies{get;set;}=[];public List<PhaseFieldE> Phases{get;set;}=[];public uint Random{get;set;}=81273;public List<FacilityE> Facilities{get;set;}=[];public List<ProductionJobE> Jobs{get;set;}=[];public List<ColonyE> Colonies{get;set;}=[];public List<MoonE> Moons{get;set;}=[];public List<string> Ledger{get;set;}=[];
}
public sealed class FacilityE {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Station{get;set;}="";public string Kind{get;set;}="Refinery";public bool Online{get;set;}=true;public bool Destroyed{get;set;}public Dictionary<string,decimal> Stock{get;set;}=[];}
public sealed class ProductionJobE {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Recipe{get;set;}="";public string Facility{get;set;}="";public string Station{get;set;}="";public string State{get;set;}="Active";public int Runs{get;set;}=1;public DateTime Finish{get;set;}public bool Success{get;set;}public Dictionary<string,decimal> Inputs{get;set;}=[];public Dictionary<string,decimal> Outputs{get;set;}=[];public string BlueprintItem{get;set;}="";public int BlueprintRuns{get;set;}}
public sealed class MoonE {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Station{get;set;}="";public string Name{get;set;}="";public string Facility{get;set;}="";public string State{get;set;}="Surveyed";public bool Automated{get;set;}public DateTime Due{get;set;}public DateTime LastTick{get;set;}public decimal Deposit{get;set;}=10000;public decimal Rocks{get;set;}public decimal Extracted{get;set;}public Dictionary<string,decimal> Stock{get;set;}=[];}
public sealed class ColonyE {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Station{get;set;}="";public string Name{get;set;}="";public bool Active{get;set;}=true;public DateTime NextCycle{get;set;}public decimal Deposit{get;set;}=100000;public decimal Raw{get;set;}public decimal Product{get;set;}public decimal Capacity{get;set;}=500;public decimal LinkRate{get;set;}=50;public string Status{get;set;}="Running";}
public sealed record RecipeE(string Id,string Name,string Kind,int Seconds,string Output,int Quantity,Dictionary<string,decimal> Inputs);
public static class ProductionCatalogE
{
 public static readonly ItemDefinition[] Items=new[]{
 new ItemDefinition("e_moon","Surveyed moon material","Production / Resources","Authored finite moon feedstock.",1,8,""),new("e_gas","Atmospheric feedstock","Production / Planetary","Extracted and physically exported from colonies.",.5f,2,""),new("e_polymer","Composite polymer","Production / Intermediates","Composite reactor output.",.5f,25,""),new("e_component","Advanced component","Production / Components","Polymer and planetary chain product.",1,60,""),new("e_coolant","Colony coolant","Production / Planetary","Processor output; exported through customs.",1,12,""),new("e_relic","Ancient engineering relic","Production / Research","Authored advanced-research input; acquisition through specialist contracts.",1,400,""),new("e_decryptor","Optimisation decryptor","Production / Research","Consumes one item to multiply invention chance by 1.2 and add two output runs.",.1f,200,""),new("e_formula","Composite reaction formula","Production / Licences","Reusable reaction licence; no research, copy or invention.",.1f,500,""),new("e_prism","Erratic prism ore","Production / Resources","Authored phased deposit; erratic refining selects a mineral using the saved seed.",1,20,""),new("e_compressed_prism","Compressed prism ore","Production / Resources","One-to-one compression preserves units.",.01f,20,""),new("e_mutator","Repairer mutator","Production / Mutators","Consumes one eligible pristine repairer identity to roll unique amount, capacitor, CPU and power modifiers.",.1f,300,""),new("e_t2","Advanced survey instrument","Production / Equipment","Authored chain output for commerce; fitting coverage remains tracked separately.",5,500,""),new("e_t3","Relic navigation core","Production / Equipment","Authored advanced production output.",5,800,"")};
 public static readonly RecipeE[] Recipes=[
 new("polymer","Composite polymer","Reaction",3600,"e_polymer",20,new(){{"e_moon",40},{"e_formula",1},{"fuel",5}}),
 new("component","Advanced component","Manufacturing",1800,"e_component",5,new(){{"e_polymer",10},{"e_coolant",10},{"ind_34",50}}),
 new("advanced","Advanced survey instrument","Manufacturing",3600,"e_t2",1,new(){{"e_component",5},{"analyzer",1}}),
 new("relic-core","Relic navigation core","Manufacturing",7200,"e_t3",1,new(){{"e_component",10},{"e_polymer",20}}),
 new("booster","Focus booster","Reaction",1800,"booster25_focus",2,new(){{"e_gas",40},{"e_coolant",5},{"fuel",5},{"e_formula",1}})];
 public static bool Restricted(string id)=>CatalogQ.Restricted(id)||(id.StartsWith("e_")||id.StartsWith("f_"));
}
public sealed partial class DemoWorld
{
 public ProductionStateE Production{get;set;}=new();
 public void LogProductionE(string message){Production.Ledger.Add(Now.ToString("u")+" / "+message);if(Production.Ledger.Count>300)Production.Ledger.RemoveAt(0);}
 public double RollProductionE(){uint x=Production.Random;x^=x<<13;x^=x>>17;x^=x<<5;Production.Random=x==0?81273:x;return Production.Random/(double)uint.MaxValue;}
 public FacilityE? DeployFacilityE(string kind)
 {
  if(!Docked||kind is not("Refinery" or "Laboratory")||Balance<2000||Production.Facilities.Any(f=>f.Station==StationKey44&&f.Kind==kind&&!f.Destroyed))return null;
  if(kind=="Refinery"&&ResolveSystem43(SystemId).Security>.4)return null;
  WalletChange(-2000,"Production facility / "+kind);var f=new FacilityE{Station=StationKey44,Kind=kind};Production.Facilities.Add(f);LogProductionE("Deployed "+kind);return f;
 }
 public string SupplyFacilityE(string id,string item,int amount)
 {
  var f=Production.Facilities.FirstOrDefault(f=>f.Id==id);if(f==null||f.Destroyed||!Docked||f.Station!=StationKey44||amount<=0||Hangar.GetValueOrDefault(item)<amount)return "Local facility and sufficient stock required.";
  Hangar[item]-=amount;f.Stock[item]=f.Stock.GetValueOrDefault(item)+amount;return "";
 }
 public string SetFacilityE(string id,bool online)
 {var f=Production.Facilities.FirstOrDefault(f=>f.Id==id);if(f==null||f.Destroyed||!Docked||f.Station!=StationKey44)return "Dock at the facility.";AdvanceProductionE();f.Online=online;return "";}
 public void DestroyFacilityE(string id)
 {var f=Production.Facilities.FirstOrDefault(f=>f.Id==id);if(f==null||f.Destroyed)return;AdvanceProductionE();f.Destroyed=true;f.Online=false;f.Stock.Clear();foreach(var j in Production.Jobs.Where(j=>j.Facility==id&&j.State is "Active" or "Ready"))j.State="Lost";foreach(var m in Production.Moons.Where(m=>m.Facility==id)){m.State="Lost";m.Rocks=0;m.Stock.Clear();}LogProductionE("Facility destroyed / jobs and storage lost / "+id);}
 public string InstallProductionE(string recipeId,string facilityId,int runs=1)
 {
  AdvanceProductionE();var r=ProductionCatalogE.Recipes.FirstOrDefault(r=>r.Id==recipeId);var f=Production.Facilities.FirstOrDefault(f=>f.Id==facilityId);
  if(r==null||f==null||f.Destroyed||!f.Online||!Docked||f.Station!=StationKey44||runs<1||runs>100)return "Choose an online local facility and 1–100 runs.";
  if(r.Kind=="Reaction"&&(f.Kind!="Refinery"||ResolveSystem43(SystemId).Security>.4))return "An online refinery in security 0.4 or lower is required.";
  if(r.Kind=="Manufacturing"&&f.Kind!="Laboratory")return "This authored component line requires a laboratory.";
  if(Production.Jobs.Count(j=>j.Facility==f.Id&&j.State=="Active")>=2)return "Facility has two occupied job slots.";
  var bp=Industry.Blueprints.FirstOrDefault(b=>b.Item==r.Output&&b.Station==StationKey44&&!b.Original&&b.Runs>=runs&&b.LockedJob.Length==0);
  if(r.Output is "e_t2" or "e_t3"&&bp==null)return "A successful invention blueprint copy with sufficient runs is required.";
  var inputs=r.Inputs.ToDictionary(p=>p.Key,p=>p.Key=="e_formula"?p.Value:p.Value*runs);decimal fee=25*runs;
  if(inputs.Any(p=>Hangar.GetValueOrDefault(p.Key)<p.Value)||Balance<fee)return "Missing declared materials, formula or installation credits.";
  foreach(var p in inputs.Where(p=>p.Key!="e_formula"))Hangar[p.Key]-=p.Value;
  WalletChange(-fee,"Production installation / "+r.Name);if(bp!=null)bp.Runs-=runs;
  Production.Jobs.Add(new(){Recipe=r.Id,Facility=f.Id,Station=f.Station,Runs=runs,Finish=Now.AddSeconds(r.Seconds*runs),Success=true,Inputs=inputs,Outputs=new(){{r.Output,r.Quantity*runs}}});LogProductionE("Installed "+r.Name+" x"+runs);return "";
 }
 public string InventProductionE(string facilityId,string blueprintId,bool decryptor=false,bool relic=false)
 {
  var f=Production.Facilities.FirstOrDefault(f=>f.Id==facilityId);var bp=Industry.Blueprints.FirstOrDefault(b=>b.Id==blueprintId);
  if(f==null||f.Destroyed||!f.Online||f.Kind!="Laboratory"||!Docked||f.Station!=StationKey44)return "An online local laboratory is required.";
  if(Production.Jobs.Count(j=>j.Facility==f.Id&&j.State=="Active")>=2)return "Facility job slots occupied.";
  if(!relic&&(bp==null||bp.Original||bp.Item!="analyzer"||bp.Runs<1||bp.Station!=StationKey44||bp.LockedJob.Length>0))return "One unused local analyzer blueprint-copy run required.";
  if(Hangar.GetValueOrDefault("d_datacore")<4||relic&&Hangar.GetValueOrDefault("e_relic")<1||decryptor&&Hangar.GetValueOrDefault("e_decryptor")<1||Balance<100)return "Four datacores, credits and selected optional/relic inputs required.";
  Hangar["d_datacore"]-=4;if(relic)Hangar["e_relic"]--;else bp!.Runs--;if(decryptor)Hangar["e_decryptor"]--;WalletChange(-100,"Invention installation");
  double chance=Math.Min(.95,(relic?.3:.4)*(1+IndustryLevel("Science")*.02)*(decryptor?1.2:1));bool success=RollProductionE()<chance;
  Production.Jobs.Add(new(){Recipe=relic?"Relic invention":"Invention",Facility=f.Id,Station=f.Station,Finish=Now.AddHours(1),Success=success,BlueprintItem=relic?"e_t3":"e_t2",BlueprintRuns=10+(decryptor?2:0),Inputs=new(){{"d_datacore",4},{relic?"e_relic":"BPC run",1}},Outputs=[]});LogProductionE("Invention installed / chance "+chance.ToString("P1")+" / outcome sealed until delivery");return "";
 }
 public string DeliverProductionE(string id)
 {
  AdvanceProductionE();var j=Production.Jobs.FirstOrDefault(j=>j.Id==id);var f=Production.Facilities.FirstOrDefault(f=>f.Id==j?.Facility);
  if(j==null||j.State!="Ready"||f==null||f.Destroyed||!f.Online||!Docked||StationKey44!=j.Station)return "Ready job and online local facility required.";
  foreach(var p in j.Outputs)Hangar[p.Key]=Hangar.GetValueOrDefault(p.Key)+p.Value;
  if(j.BlueprintItem.Length>0&&j.Success)Industry.Blueprints.Add(new(){Item=j.BlueprintItem,Original=false,Runs=j.BlueprintRuns,ME=2,TE=4,Station=j.Station});j.State="Delivered";LogProductionE("Delivered "+j.Recipe+" / "+(j.Success?"success":"failed; inputs consumed"));return "";
 }
 public string CancelProductionE(string id){var j=Production.Jobs.FirstOrDefault(j=>j.Id==id);if(j==null||j.State is not("Active" or "Ready"))return "Job cannot be cancelled.";j.State="Cancelled";LogProductionE("Cancelled job; inputs and fees not refunded");return "";}
 public MoonE? SurveyMoonE(string name)
 {if(!Docked||name.Length<1||name.Length>64||Production.Moons.Any(m=>m.Station==StationKey44))return null;var m=new MoonE{Station=StationKey44,Name=name};Production.Moons.Add(m);LogProductionE("Surveyed authored deposit "+name+" / 10,000 units");return m;}
 public string ScheduleMoonE(string id,string facilityId,bool automated)
 {
  var m=Production.Moons.FirstOrDefault(m=>m.Id==id);var f=Production.Facilities.FirstOrDefault(f=>f.Id==facilityId);
  if(m==null||f==null||f.Kind!="Refinery"||!f.Online||f.Destroyed||m.State!="Surveyed"||!Docked||m.Station!=StationKey44||f.Station!=m.Station)return "Surveyed deposit and online local refinery required.";
  if(f.Stock.GetValueOrDefault("fuel")<10)return "Supply ten fuel units to the refinery.";f.Stock["fuel"]-=10;m.Facility=f.Id;m.Automated=automated;m.State=automated?"Automated":"Extracting";m.Due=Now.AddHours(24);m.LastTick=Now;return "";
 }
 public string FractureMoonE(string id)
 {AdvanceProductionE();var m=Production.Moons.FirstOrDefault(m=>m.Id==id);if(m==null||m.State!="Chunk ready"||!Docked||m.Station!=StationKey44)return "A ready local chunk is required.";m.Rocks=Math.Min(1000,m.Deposit);m.Deposit-=m.Rocks;m.State="Fractured";LogProductionE("Chunk fractured / "+m.Rocks+" mineable units");return "";}
 public string MineMoonE(string id,int units)
 {var m=Production.Moons.FirstOrDefault(m=>m.Id==id);if(m==null||m.State!="Fractured"||!Docked||m.Station!=StationKey44||units<=0)return "Local fractured field required.";decimal take=Math.Min(units,m.Rocks);if(take<=0)return "Field exhausted.";m.Rocks-=take;m.Extracted+=take;Hangar["e_moon"]=Hangar.GetValueOrDefault("e_moon")+take;LogProductionE("Mining service collected "+take+" / "+m.Name);if(m.Rocks==0)m.State=m.Deposit>0?"Surveyed":"Depleted";return "";}
 public string CollectMoonE(string id)
 {AdvanceProductionE();var m=Production.Moons.FirstOrDefault(m=>m.Id==id);if(m==null||!Docked||m.Station!=StationKey44)return "Dock locally to collect drill output.";foreach(var p in m.Stock)Hangar[p.Key]=Hangar.GetValueOrDefault(p.Key)+p.Value;m.Stock.Clear();return "";}
 public decimal RaidMoonE(string id,decimal units)
 {AdvanceProductionE();var m=Production.Moons.FirstOrDefault(m=>m.Id==id);if(m==null||units<=0)return 0;decimal take=Math.Min(units,m.Stock.GetValueOrDefault("e_moon"));m.Stock["e_moon"]=m.Stock.GetValueOrDefault("e_moon")-take;LogProductionE("Raid removed "+take+" units / "+m.Name);return take;}
 public ColonyE? DeployColonyE(string name)
 {if(!Docked||Balance<500||name.Length<1||name.Length>64||Production.Colonies.Count(c=>c.Active)>=6||Production.Colonies.Any(c=>c.Station==StationKey44))return null;WalletChange(-500,"Planetary command centre");var c=new ColonyE{Station=StationKey44,Name=name,NextCycle=Now.AddHours(1)};Production.Colonies.Add(c);return c;}
 public string UpgradeColonyE(string id)
 {var c=Production.Colonies.FirstOrDefault(c=>c.Id==id);if(c==null||!Docked||c.Station!=StationKey44||Balance<250||c.LinkRate>=500)return "Dock locally with 250 credits; maximum throughput 500.";AdvanceProductionE();WalletChange(-250,"Planetary link/storage upgrade");c.LinkRate=Math.Min(500,c.LinkRate*2);c.Capacity*=2;return "";}
 public string ExportColonyE(string id,bool raw=false)
 {AdvanceProductionE();var c=Production.Colonies.FirstOrDefault(c=>c.Id==id);if(c==null||!Docked||c.Station!=StationKey44)return "Physical collection requires docking in the colony system.";decimal amount=raw?c.Raw:c.Product;decimal tax=amount*(raw?.2m:1.2m);if(amount<=0||Balance<tax)return "No output or insufficient customs tax.";WalletChange(-tax,"Planetary customs export");string item=raw?"e_gas":"e_coolant";Hangar[item]=Hangar.GetValueOrDefault(item)+amount;if(raw)c.Raw=0;else c.Product=0;return "";}
 public void AdvanceProductionE()
 {
  foreach(var j in Production.Jobs.Where(j=>j.State=="Active"&&j.Finish<=Now))j.State="Ready";
  foreach(var m in Production.Moons){var f=Production.Facilities.FirstOrDefault(f=>f.Id==m.Facility);if(f==null||f.Destroyed)continue;if(m.State=="Extracting"&&m.Due<=Now&&f.Online)m.State="Chunk ready";
   if(m.State!="Automated")continue;long ticks=Math.Max(0,(long)(Now-m.LastTick).TotalHours);if(ticks==0)continue;m.LastTick=m.LastTick.AddHours(ticks);if(!f.Online)continue;decimal possible=Math.Min(ticks,Math.Floor(f.Stock.GetValueOrDefault("fuel")));decimal take=Math.Min(m.Deposit,possible*50);decimal cycles=take>0?Math.Ceiling(take/50):0;f.Stock["fuel"]=f.Stock.GetValueOrDefault("fuel")-cycles;m.Deposit-=take;m.Extracted+=take*.4m;m.Stock["e_moon"]=m.Stock.GetValueOrDefault("e_moon")+take*.4m;if(m.Deposit==0)m.State="Depleted";}
  foreach(var c in Production.Colonies.Where(c=>c.Active&&c.NextCycle<=Now)){long cycles=1+(long)(Now-c.NextCycle).TotalHours;c.NextCycle=c.NextCycle.AddHours(cycles); // Loop only while finite deposits/buffers can change.
   for(long i=0;i<cycles;i++){decimal extracted=Math.Min(c.Deposit,Math.Min(100,Math.Max(0,c.Capacity-c.Raw)));c.Deposit-=extracted;c.Raw+=extracted;decimal process=Math.Min(c.Raw,Math.Min(c.LinkRate,Math.Max(0,c.Capacity-c.Product)*10));process=Math.Floor(process/10)*10;c.Raw-=process;c.Product+=process/10;if(extracted==0&&process==0)break;}
   c.Status=c.Product>=c.Capacity?"Output buffer full":c.Raw>=c.Capacity?"Link bottleneck":c.Deposit==0?"Deposit exhausted":"Running";}
 }
 public List<string> ProductionTreeE(string item,int units)
 {var available=new Dictionary<string,decimal>(StorageAt(StationKey44));var rows=new List<string>();void Walk(string id,decimal need,int depth,HashSet<string> path){decimal own=available.GetValueOrDefault(id);decimal shortage=Math.Max(0,need-own);if(id!="e_formula")available[id]=Math.Max(0,own-need);rows.Add(new string(' ',depth*2)+AssetNameD(id)+" / need "+need+" / local "+own+" / short "+Math.Max(0,need-own));if(shortage<=0)return;if(depth>=8||!path.Add(id)){rows.Add("Cycle/depth limit; inspect source recipe.");return;}var r=ProductionCatalogE.Recipes.FirstOrDefault(r=>r.Output==id);if(r!=null){decimal batches=Math.Ceiling(Math.Max(0,need-own)/r.Quantity);foreach(var p in r.Inputs)Walk(p.Key,p.Key=="e_formula"?p.Value:p.Value*batches,depth+1,new(path));}else if(IndustryCatalog37.Recipes.TryGetValue(id,out var native)&&native.Imported){foreach(var p in native.Native)Walk(p.Key,p.Value*Math.Ceiling(Math.Max(0,need-own)/native.Definition.Output),depth+1,new(path));}}Walk(item,units,0,[]);return rows;}
 public static void ValidateProductionE(ProductionStateE? p)
 {if(p==null)return;if(p.Supplies.Values.Any(v=>v<0||v>5)||p.Phases.Any(f=>f.Remaining<0||f.Harvested<0||f.Energy<0)||p.Random==0||p.Facilities.Select(f=>f.Id).Distinct().Count()!=p.Facilities.Count||p.Jobs.Select(j=>j.Id).Distinct().Count()!=p.Jobs.Count||p.Moons.Select(m=>m.Id).Distinct().Count()!=p.Moons.Count||p.Colonies.Select(c=>c.Id).Distinct().Count()!=p.Colonies.Count||p.Facilities.Any(f=>f.Stock.Values.Any(v=>v<0))||p.Moons.Any(m=>m.Deposit<0||m.Rocks<0||m.Stock.Values.Any(v=>v<0))||p.Colonies.Any(c=>c.Deposit<0||c.Raw<0||c.Product<0||c.Capacity<=0||c.LinkRate<=0)||p.Jobs.Any(j=>j.Inputs.Values.Any(v=>v<0)||j.Outputs.Values.Any(v=>v<0)||!p.Facilities.Any(f=>f.Id==j.Facility)))throw new InvalidDataException("Invalid production state.");}
}
