using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace StarlightSyndicateDemo;

// This registry owns source identity and relationships. Gameplay support remains explicit in ItemCatalog.
public sealed class SourceType40
{
 public int Id{get;set;} public string Name{get;set;}="";public int GroupId{get;set;}
 public int? MarketGroupId{get;set;}public int? MetaGroupId{get;set;}public int? ParentTypeId{get;set;}public int? MetaLevel{get;set;}public int? TechLevel{get;set;}
 public bool Published{get;set;}public double Volume{get;set;}public double? PackagedVolume{get;set;}public double? Mass{get;set;}public double? Capacity{get;set;}
}
public sealed class SourceNode40
{
 public int Id{get;set;}public string Name{get;set;}="";public int? ParentId{get;set;}public bool Published{get;set;}
}
public sealed class SourceIndex40
{
 public int Build{get;set;}public SourceType40[] Types{get;set;}=[];public SourceNode40[] Categories{get;set;}=[];public SourceNode40[] Groups{get;set;}=[];public SourceNode40[] Markets{get;set;}=[];public SourceNode40[] MetaGroups{get;set;}=[];
 public Dictionary<string,int> Aliases{get;set;}=[];public Dictionary<string,string> AliasOrigins{get;set;}=[];
 public Dictionary<int,JsonElement> TypeLists{get;set;}=[];public Dictionary<int,JsonElement> MetaRecords{get;set;}=[];
}
public static class UnifiedCatalog40
{
 static string Asset(string file)=>Path.Combine(AppContext.BaseDirectory,"Assets","Sde40",file);
 public static readonly SourceIndex40 Index=JsonSerializer.Deserialize<SourceIndex40>(File.ReadAllText(Asset("index.json")))??throw new InvalidDataException("Missing Pass 40 item index");
 public static readonly IReadOnlyDictionary<int,SourceType40> Types=Index.Types.ToDictionary(t=>t.Id);
 public static readonly IReadOnlyDictionary<int,SourceNode40> Groups=Index.Groups.ToDictionary(n=>n.Id),Categories=Index.Categories.ToDictionary(n=>n.Id),Markets=Index.Markets.ToDictionary(n=>n.Id),MetaGroups=Index.MetaGroups.ToDictionary(n=>n.Id);
 public static readonly IReadOnlyDictionary<int,string> MarketPaths=Index.Markets.ToDictionary(n=>n.Id,n=>MarketPath(n.Id));
 public static readonly IReadOnlyDictionary<int,string[]> GameAliases=Index.Aliases.Concat(CatalogQ.Data.Items.Select(i=>new KeyValuePair<string,int>(i.Id,i.Type))).Concat(FilamentsP.All.Select(f=>new KeyValuePair<string,int>("sde_"+f.Id,f.Id))).Concat(Index.Types.Where(t=>new[]{"Tranquil","Calm","Agitated","Fierce","Raging","Chaotic","Cataclysmic"}.Any(g=>new[]{"Dark","Electrical","Exotic","Firestorm","Gamma"}.Any(w=>t.Name==g+" "+w+" Filament"))).Select(t=>new KeyValuePair<string,int>("sde_"+t.Id,t.Id))).GroupBy(p=>p.Value).ToDictionary(g=>g.Key,g=>g.Select(p=>p.Key).Order(StringComparer.Ordinal).ToArray());
 static readonly IReadOnlyDictionary<int,int[]> Families=Index.Types.GroupBy(t=>FamilyRoot(t.Id)).ToDictionary(g=>g.Key,g=>g.Select(t=>t.Id).Order().ToArray());
 static readonly Dictionary<int,Dictionary<int,JsonElement>> shards=[];static readonly Queue<int> shardOrder=[];
 static readonly Dictionary<int,InformationEntry> information=[];static readonly Queue<int> informationOrder=[];
 static readonly Lazy<Dictionary<int,JsonElement>> attributes=new(()=>JsonSerializer.Deserialize<Dictionary<int,JsonElement>>(File.ReadAllText(Asset("attributes.json")))!);
 public static string ReferenceId(int id)=>"sde_"+id;
 public static int SourceId(string id)=>FindSourceId(id)??0;
 public static int? FindSourceId(string id)
 {
  if(Index.Aliases.TryGetValue(id,out int mapped))return mapped;
  foreach(string prefix in new[]{"sde_","type:","ind_","skill_"})if(id.StartsWith(prefix,StringComparison.Ordinal)&&int.TryParse(id[prefix.Length..],out int parsed)&&Types.ContainsKey(parsed))return parsed;
  return null;
 }
 public static SourceType40? Source(string id)=>FindSourceId(id) is int sourceId?Types.GetValueOrDefault(sourceId):null;
 public static int CategoryId(SourceType40 t)=>Groups[t.GroupId].ParentId??0;
 public static string Classification(SourceType40 t)=>Retheme41.Taxonomy("category",CategoryId(t))+" / "+Retheme41.Taxonomy("group",t.GroupId);
 public static string PathFor(SourceType40 t)=>t.MarketGroupId is int market?MarketPaths[market]:Classification(t);
 public static string MarketPath(int id)
 {
  var names=new List<string>();var seen=new HashSet<int>();int? current=id;
  while(current is int k){if(!seen.Add(k)||!Markets.TryGetValue(k,out var n))throw new InvalidDataException("Invalid market ancestry: "+id);names.Add(Retheme41.Taxonomy("market",n.Id));current=n.ParentId;}
  names.Reverse();return string.Join(" / ",names);
 }
 public static int FamilyRoot(int id){var seen=new HashSet<int>();while(Types[id].ParentTypeId is int parent&&parent!=id){if(!seen.Add(id)||!Types.ContainsKey(parent))throw new InvalidDataException("Invalid type family: "+id);id=parent;}return id;}
 public static IReadOnlyList<int> Variations(int id)=>Families.GetValueOrDefault(FamilyRoot(id),[]);
 public static bool InTypeList(int listId,int typeId)
 {
  if(!Index.TypeLists.TryGetValue(listId,out var list)||!Types.TryGetValue(typeId,out var t))return false;
  bool Has(string key,int value)=>list.TryGetProperty(key,out var ids)&&ids.EnumerateArray().Any(n=>n.GetInt32()==value);
  if(Has("excludedTypeIDs",typeId)||Has("excludedGroupIDs",t.GroupId)||Has("excludedCategoryIDs",CategoryId(t)))return false;
  return Has("includedTypeIDs",typeId)||Has("includedGroupIDs",t.GroupId)||Has("includedCategoryIDs",CategoryId(t));
 }
 public static ItemDefinition ReferenceItem(string id)
 {
  var t=Source(id)??throw new KeyNotFoundException("Unknown item identity: "+id);
  return new(id,Retheme41.TypeName(t.Id),"Pattern archive / "+PathFor(t),Retheme41.Type(t.Id).Description??"Archived production pattern.",(float)t.Volume,0,"");
 }
 public static InformationEntry Summary(SourceType40 t)=>new(){Key=InformationCatalog.ItemKey(ReferenceId(t.Id)),Id=ReferenceId(t.Id),Name=Retheme41.TypeName(t.Id),Kind="Reference type",Group="Pattern archive / "+PathFor(t),Description=(Retheme41.Type(t.Id).Description??"")+" Type "+t.Id+" / "+Classification(t)+" / "+(t.Published?"Published":"Unpublished")+" / "+(t.MetaGroupId is int m?Retheme41.Taxonomy("meta",m):"")};
 public static IEnumerable<InformationEntry> Summaries()=>Index.Types.Select(Summary);
 static JsonElement Detail(int id)
 {
  int shard=id/1000;
  if(!shards.TryGetValue(shard,out var records)){
   records=JsonSerializer.Deserialize<Dictionary<int,JsonElement>>(File.ReadAllText(Asset("details-"+shard+".json")))!;
   if(shards.Count>=8)shards.Remove(shardOrder.Dequeue());shards.Add(shard,records);shardOrder.Enqueue(shard);
  }
  return records[id];
 }
 static string English(JsonElement e)=>e.ValueKind==JsonValueKind.Object&&e.TryGetProperty("en",out var en)?en.GetString()??"":e.ValueKind==JsonValueKind.String?e.GetString()??"":e.ToString();
 public static bool TryInformation(string key,out InformationEntry entry)
 {
  entry=null!;if(!key.StartsWith("item/sde_",StringComparison.Ordinal)&&!key.StartsWith("item/type:",StringComparison.Ordinal))return false;
  if(FindSourceId(key[5..]) is not int id)return false;
  entry=Information(id);return true;
 }
 public static InformationEntry Information(int id)
 {
  if(information.TryGetValue(id,out var cached))return cached;
  var t=Types[id];var e=Summary(t);var detail=Detail(id);var raw=detail.GetProperty("Type");
  e.Description=Retheme41.Type(id).Description??"Archived production pattern.";
  e.Add("Source audit","EVE SOURCE RECORD / DELIBERATE REFERENCE VIEW",heading:true);e.Add("Source audit","Source name",t.Name);if(raw.TryGetProperty("description",out var sourceDescription))e.Add("Source audit","Original description",System.Net.WebUtility.HtmlDecode(Regex.Replace(English(sourceDescription),"<[^>]*>","")));
  e.Add("Description",e.Description);e.Add("Source audit","REFERENCE DATABASE / GAMEPLAY SUPPORT IS EXPLICIT",heading:true);
  e.Add("Source audit","Build",Index.Build.ToString());e.Add("Source audit","Canonical identity","type:"+id);e.Add("Source audit","Source name",t.Name);e.Add("Source audit","Category",Categories[CategoryId(t)].Name+" ("+CategoryId(t)+")");e.Add("Source audit","Group",Groups[t.GroupId].Name+" ("+t.GroupId+")");e.Add("Source audit","Market hierarchy",t.MarketGroupId is int m?MarketPaths[m]+" ("+m+")":"No market group");e.Add("Source audit","Published",t.Published.ToString());e.Add("Source audit","Meta tier",t.MetaGroupId is int meta?MetaGroups[meta].Name+" ("+meta+")":"Unspecified");e.Add("Source audit","Meta level",t.MetaLevel?.ToString()??"Unspecified");e.Add("Source audit","Tech level",t.TechLevel?.ToString()??"Unspecified");
  if(GameAliases.TryGetValue(id,out var aliases))foreach(string alias in aliases){bool supported=ItemCatalog.IsSupported(alias);e.Add("Game aliases",supported?ItemCatalog.Get(alias).Name:alias,alias+" / "+(supported?"Supported":"Registry only"),supported||FleetCatalog.ById.ContainsKey(alias)?InformationCatalog.ItemKey(alias):"");}
  e.Add("Source audit","Purchase and fitting","Use the supported game alias; reference identities cannot be traded or fitted.");
  foreach(var property in raw.EnumerateObject().Where(p=>p.Value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False))e.Add("Source fields",property.Name,property.Value.ToString());
  e.Add("Attributes","Source volume",t.Volume.ToString("0.###",CultureInfo.InvariantCulture)+" m3");e.Stats["Source volume"]=t.Volume.ToString(CultureInfo.InvariantCulture);
  if(detail.GetProperty("Dogma").TryGetProperty("dogmaAttributes",out var values))foreach(var a in values.EnumerateArray()){
   int attribute=a.GetProperty("attributeID").GetInt32();string label=attributes.Value.TryGetValue(attribute,out var def)&&def.TryGetProperty("name",out var name)?name.GetString()!:"Attribute "+attribute;string value=a.GetProperty("value").ToString();e.Add("Source dogma",label+" ("+attribute+")",value);e.Stats["Source / "+label]=value;
  }
  foreach(int variation in Variations(id)){var related=Types[variation];string k=InformationCatalog.ItemKey(ReferenceId(variation));e.Related.Add(k);e.Add("Variations",Retheme41.TypeName(variation),related.MetaGroupId is int relatedMeta?MetaGroups[relatedMeta].Name:"Base",k);}
  foreach(var list in Index.TypeLists.Where(l=>InTypeList(l.Key,id)))e.Add("Source type lists",list.Value.GetProperty("name").GetString()??"",list.Key.ToString());
  if(information.Count>=128)information.Remove(informationOrder.Dequeue());information.Add(id,e);informationOrder.Enqueue(id);return e;
 }
 public static void AttachSource(InformationEntry e)
 {
  var t=Source(e.Id);if(t==null){e.Add("Source audit","Identity","Authored Starlight item; no source type alias.");return;}
  e.Add("Source audit","Canonical identity","type:"+t.Id,InformationCatalog.ItemKey(ReferenceId(t.Id)));e.Add("Source audit","Source name",t.Name);e.Add("Source audit","Classification",Classification(t));e.Add("Source audit","Market hierarchy",PathFor(t));e.Add("Source audit","Meta tier",t.MetaGroupId is int m?MetaGroups[m].Name:"Unspecified");e.Add("Source audit","Build",Index.Build.ToString());e.Add("Source audit","Alias provenance",Index.AliasOrigins.GetValueOrDefault(e.Id,"identity prefix"));
 }
}
