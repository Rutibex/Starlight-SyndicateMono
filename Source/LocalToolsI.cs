using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed class LocalPackI {public int Schema{get;set;}=1;public string Id{get;set;}="local-science";public string Mechanics{get;set;}="Starlight-authored";public string Geography{get;set;}="supplied-galaxy+2019-belts";public int SourceBuild{get;set;}=3569502;public List<CosmeticI> Designs{get;set;}=[];}
public sealed class CompletionRowI {public string Id{get;set;}="";public string Name{get;set;}="";public string Scope{get;set;}="";public string Status{get;set;}="Partial";public string Delivered{get;set;}="";public string Remaining{get;set;}="";public string Evidence{get;set;}="";}
public static class ReleaseCatalogI {public static readonly CompletionRowI[] Rows=JsonSerializer.Deserialize<CompletionRowI[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","CompletionRegisterI.json")))!;}
public sealed partial class DemoWorld
{
 public static string ExportDirectoryI=>Path.Combine(Path.GetDirectoryName(SavePath)!,"exports");
 public object ExportSnapshotI(string kind)=>kind switch
 {
  "fitting"=>new { Schema=1,At=Now,Player.CombatHull,Player.InstanceId,Modules=FittingLayoutModel.Installed(Player).Select(m=>new{m.Id,m.Bank,m.Index,m.Kind,m.Burnt}),Stats=Player.Attributes.Keys.ToDictionary(k=>k,k=>Player.Stat(k)),Cpu=TotalFitCpu,Power=TotalFitPower,Skills=Skills.Points },
  "skills"=>new {Schema=1,At=Now,Skills.Points,Skills.Queue,Skills.Plans,Skills.Unallocated},
  "market"=>new {Schema=1,At=Now,Station=StationKey44,Orders=MarketOrders.Where(o=>LocationRegion(o.Station)==Current.Region).ToArray(),Trades=MarketTrades,Journal=WalletJournal},
  "industry"=>new {Schema=1,At=Now,Industry.Blueprints,Industry.Jobs},
  "assets"=>new {Schema=1,At=Now,Assets=FindAssetsD("","All"),MissionCargo=AgentsState.Missions.Select(m=>new{m.Id,m.Definition,m.Cargo,m.Hangars}),CargoVolume=CargoUsed},
  "combat"=>new {Schema=1,At=Now,Events=Experience.CombatArchive,Losses=Commander.Losses},
  "cosmetics"=>new LocalPackI {Designs=Experience.Cosmetics.Select(d=>new CosmeticI{Name=d.Name,Hull=d.Hull,Primary=d.Primary,Accent=d.Accent,Pattern=d.Pattern}).ToList()},
  "completion"=>new {Schema=1,At=Now,Mechanics="Pinned October 2026 reference / authored deviations listed",Geography="Supplied build 3569502 with restored 2019 belts",Rows=ReleaseCatalogI.Rows},
  _=>throw new ArgumentException("Unsupported export kind.")
 };
 public string ExportLocalI(string kind)
 {try{object data=ExportSnapshotI(kind);Directory.CreateDirectory(ExportDirectoryI);string path=Path.Combine(ExportDirectoryI,kind+".json");File.WriteAllText(path+".tmp",JsonSerializer.Serialize(data,WeaponCatalog.JsonOptions));File.Move(path+".tmp",path,true);return path;}catch(Exception ex){return "Export failed: "+ex.Message;}}
 public string ImportLocalPackI(string path)
 {
  try{
   var file=new FileInfo(path);if(!file.Exists||file.Length>256000)return "Choose a pack file no larger than 256 KB.";
   var options=new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};var pack=JsonSerializer.Deserialize<LocalPackI>(File.ReadAllText(path),options);
   if(pack==null||pack.Schema!=1||pack.Id!="local-science"||pack.Mechanics!="Starlight-authored"||pack.Geography!="supplied-galaxy+2019-belts"||pack.SourceBuild!=SourceBuild||pack.Designs==null||pack.Designs.Count>32||Experience.Cosmetics.Count+pack.Designs.Count>128)return "Pack schema, profiles or design budget do not match this build.";
   var drafts=new List<CosmeticI>();foreach(var d in pack.Designs){if(string.IsNullOrWhiteSpace(d.Name)||d.Name.Length>48||d.Name.Any(char.IsControl)||!(d.Hull is "frigate" or "cruiser" or "battleship"||FleetCatalog.ById.ContainsKey(d.Hull))||!ExperienceCatalogI.Paints.ContainsKey(d.Primary)||!ExperienceCatalogI.Paints.ContainsKey(d.Accent)||!ExperienceCatalogI.Patterns.Contains(d.Pattern))return "Invalid cosmetic draft. Pack was not applied.";drafts.Add(new(){Name=d.Name,Hull=d.Hull,Primary=d.Primary,Accent=d.Accent,Pattern=d.Pattern});}
   Experience.Cosmetics.AddRange(drafts);Experience.Packs.Add(pack.Id);return "";
  }catch(Exception ex){return "Pack rejected: "+ex.Message;}
 }
}
