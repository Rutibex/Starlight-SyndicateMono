using System.Text.Json;
using System.Text.RegularExpressions;
namespace StarlightSyndicateDemo;
public sealed class LoreRecord41
{
 public string Key{get;set;}="";public string Name{get;set;}="";public string Description{get;set;}="";public string Kind{get;set;}="";public string Parent{get;set;}="";public string Affiliation{get;set;}="";public string Policy{get;set;}="";
 public string[] Links{get;set;}=[];public Dictionary<string,JsonElement> Data{get;set;}=[];
}
public sealed class TypeLore41
{
 public string Name{get;set;}="";public string? Description{get;set;}public string Affiliation{get;set;}="";public int FamilyId{get;set;}public string Policy{get;set;}="";
}
public sealed class LoreIndex41
{
 public int Version{get;set;}public int Build{get;set;}public LoreRecord41[] Records{get;set;}=[];public Dictionary<int,TypeLore41> Types{get;set;}=[];public Dictionary<string,TypeLore41> GameItems{get;set;}=[];public Dictionary<string,string> Replacements{get;set;}=[];public string[] ForbiddenTerms{get;set;}=[];
}
public static class Retheme41
{
 public static readonly LoreIndex41 Index=JsonSerializer.Deserialize<LoreIndex41>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Retheme41","registry.json")))??throw new InvalidDataException("Missing Starlight lore registry");
 public static readonly IReadOnlyDictionary<string,LoreRecord41> Records=Index.Records.ToDictionary(r=>r.Key,StringComparer.Ordinal);
 static readonly Dictionary<string,string> replacements=Index.Replacements.Where(p=>p.Key!="CONCORD"&&p.Key!="ORE").ToDictionary(p=>p.Key,p=>p.Value,StringComparer.OrdinalIgnoreCase);
 static readonly Regex replacementPattern=new(@"(?<!\w)(?:"+string.Join('|',replacements.Keys.OrderByDescending(k=>k.Length).ThenBy(k=>k,StringComparer.Ordinal).Select(Regex.Escape))+@")(?!\w)",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant|RegexOptions.Compiled);
 public static readonly Regex SourceLorePattern=new(@"(?<!\w)(?:"+string.Join('|',Index.ForbiddenTerms.Select(Regex.Escape))+@")(?!\w)",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant|RegexOptions.Compiled);
 static readonly Dictionary<string,InformationEntry> pages=[];static readonly Queue<string> pageOrder=[];
 public static string Text(string text)=>replacementPattern.Replace(Regex.Replace(Regex.Replace(text,@"\bCONCORD\b","Arbitration"),@"\bORE\b","Kepler Resource Group"),m=>replacements[m.Value]);
 public static TypeLore41 Type(int id)=>Index.Types.TryGetValue(id,out var t)?t:throw new KeyNotFoundException("Missing type lore "+id);
 public static string TypeName(int id)=>Type(id).Name;
 public static string Taxonomy(string kind,int id)=>Records[kind+":"+id].Name;
 public static string InformationKey(string key)=>"lore/"+key;
 public static ItemDefinition Apply(ItemDefinition item)=>Index.GameItems.TryGetValue(item.Id,out var lore)?item with{Name=lore.Name,Description=lore.Description??Text(item.Description),Group=Text(item.Group)}:item with{Name=Text(item.Name),Description=Text(item.Description),Group=Text(item.Group)};
 public static FleetHull Apply(FleetHull hull){hull.Name=Text(hull.Name);hull.Description=Text(hull.Description);hull.Quote=Text(hull.Quote);hull.TraitsStatus=Text(hull.TraitsStatus);return hull;}
 public static SkillDatabase Apply(SkillDatabase db){foreach(var s in db.Skills){s.Name=Text(s.Name);s.Description=Text(s.Description);s.Group=Text(s.Group);}return db;}
 public static string Group(LoreRecord41 r)=>r.Kind switch{"faction" or "organization"=>"Organizations / Political societies","corporation"=>"Organizations / Corporations / "+r.Affiliation,"npc"=>"People / Registered contacts / "+r.Affiliation,"region" or "constellation" or "system" or "station"=>"Charted universe / "+(r.Kind=="region"?"Regions":r.Kind=="constellation"?"Constellations":r.Kind=="system"?"Solar systems":"Stations")+" / "+r.Affiliation,"culture" or "heritage" or "background" or "school" or "title" or "attribute"=>"People / Backgrounds / "+(r.Kind=="culture"?"Cultures":r.Kind=="heritage"?"Heritages":r.Kind=="background"?"Personal histories":r.Kind=="school"?"Schools":r.Kind=="title"?"Titles":"Neural aptitudes"),_=>"Organizations / Charter records / "+r.Kind};
 public static InformationEntry Summary(LoreRecord41 r)=>new(){Key=InformationKey(r.Key),Id=r.Key,Name=r.Name,Kind=r.Kind is "system" or "station" or "region" or "constellation"?"Chart record":"Lore record",Group=Group(r),Description=r.Description};
 public static IEnumerable<InformationEntry> Summaries()=>Index.Records.Where(r=>r.Kind is not("category" or "group" or "market" or "meta")).Select(Summary);
 public static bool TryInformation(string key,out InformationEntry e)
 {
  e=null!;if(!key.StartsWith("lore/",StringComparison.Ordinal)||!Records.TryGetValue(key[5..],out var r))return false;
  if(pages.TryGetValue(key,out e!))return true;
  e=Summary(r);e.Add("Description",r.Description);e.Add("Identity","Starlight affiliation",r.Affiliation.Length>0?r.Affiliation:"Independent");e.Add("Identity","Record",r.Key);e.Add("Identity","Display policy",r.Policy);
  if(r.Parent.Length>0){var parent=Records[r.Parent];e.Add("Relationships",parent.Name,parent.Kind,InformationKey(parent.Key));}
  foreach(string link in r.Links.Where(l=>l!=r.Parent)){var related=Records[link];e.Add("Relationships",related.Name,related.Kind,InformationKey(link));}
  if(r.Kind=="npc")e.Add("Identity","Availability","Contact biography; visiting and agent services follow station integration.");
  if(r.Kind is "region" or "constellation" or "system" or "station")e.Add("Identity","Navigation","Chart identity prepared for the imported universe. Imported exploration is separately selectable; existing campaigns retain their authored cluster.");
  foreach(var field in r.Data.Where(p=>p.Key is not("Agent" or "StartingStations" or "CareerAgents")))e.Add("Source audit",field.Key,field.Value.ToString());
  if(pages.Count>=128)pages.Remove(pageOrder.Dequeue());pages.Add(key,e);pageOrder.Enqueue(key);return true;
 }
 public static void Sanitize(InformationEntry e)
 {
  e.Name=Text(e.Name);e.Group=Text(e.Group);e.Description=Text(e.Description);
  foreach(var tab in e.Tabs.Where(t=>!t.Key.StartsWith("Source",StringComparison.Ordinal)))for(int i=0;i<tab.Value.Count;i++){var row=tab.Value[i];tab.Value[i]=row with{Label=Text(row.Label),Value=Text(row.Value)};}
 }
}
public sealed class CharacterOrigin41
{
 public string Heritage{get;set;}="";public string Background{get;set;}="";public string School{get;set;}="";public string Title{get;set;}="";
}
public sealed partial class DemoWorld
{
 public CharacterOrigin41? CharacterOrigin{get;set;}
 public string ChooseCharacterOrigin(string heritage,string background,string school,string title)
 {
  if(!Retheme41.Records.TryGetValue(heritage,out var h)||h.Kind!="heritage"||!Retheme41.Records.TryGetValue(background,out var b)||b.Kind!="background"||b.Parent!=heritage||!Retheme41.Records.TryGetValue(school,out var s)||s.Kind!="school"||!s.Links.Contains(h.Parent)||!Retheme41.Records.TryGetValue(title,out var t)||t.Kind!="title")return "Choose a personal history and school belonging to the selected heritage.";
  CharacterOrigin=new(){Heritage=heritage,Background=background,School=school,Title=title};return "";
 }
 public bool CharacterOriginValid()=>CharacterOrigin==null||Retheme41.Records.TryGetValue(CharacterOrigin.Heritage,out var h)&&h.Kind=="heritage"&&Retheme41.Records.TryGetValue(CharacterOrigin.Background,out var b)&&b.Parent==h.Key&&Retheme41.Records.TryGetValue(CharacterOrigin.School,out var s)&&s.Kind=="school"&&s.Links.Contains(h.Parent)&&Retheme41.Records.TryGetValue(CharacterOrigin.Title,out var t)&&t.Kind=="title";
}
