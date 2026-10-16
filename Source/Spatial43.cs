using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class SpatialBody43
{
 public string Id{get;set;}="";public string System{get;set;}="";public string Name{get;set;}="";public CelestialKind Kind{get;set;}
 public SpacePosition Position{get;set;}public double Radius{get;set;}public bool RadiusKnown{get;set;}public string Parent{get;set;}="";public long ParentSourceId{get;set;}public int SourceId{get;set;}public int TypeId{get;set;}public string SourceEra{get;set;}="";public string Destination{get;set;}="";public int DestinationGateId{get;set;}public string Type{get;set;}="";public JsonElement Statistics{get;set;}public JsonElement Attributes{get;set;}public int? EffectBeaconTypeId{get;set;}public int[]? SourceChildMoonIds{get;set;}public int[]? SourceChildBeltIds{get;set;}
 public Celestial Celestial=>new(Id,System,Name,Kind,Position,Radius,Parent,Type,Destination);
}
public sealed class SpatialPackage43
{
 public int SystemId{get;set;}public SpatialBody43[] Bodies{get;set;}=[];
 [JsonIgnore] public Celestial[] Celestials{get;set;}=[];
 [JsonIgnore] public Dictionary<string,SpatialBody43> ById{get;set;}=[];
}
public sealed class Landmark43 {public int Id{get;set;}public string Name{get;set;}="";public string Description{get;set;}="";public double[] GalacticPosition{get;set;}=[];public int? SystemId{get;set;}public int? SourceLocationId{get;set;}public int? SourceIconId{get;set;}public JsonElement SourceName{get;set;}public JsonElement SourceDescription{get;set;}}
public static class Spatial43
{
 public const string WorldId="sde-3569502";
 static readonly Dictionary<int,SpatialPackage43> cache=[];static readonly LinkedList<int> order=[];
 static readonly JsonSerializerOptions options=new(){Converters={new JsonStringEnumConverter()}};
 public const int CacheLimit=4;public static int CachedSystems=>cache.Count;public static int Loads{get;private set;}
 public static string Root=>Path.Combine(AppContext.BaseDirectory,"Assets","Spatial43");
 public static bool IsSystem(string id)=>id.StartsWith("system:",StringComparison.Ordinal)&&int.TryParse(id[7..],out int n)&&Galaxy42.ById.ContainsKey(n);
 public static SpatialPackage43 Get(string id)
 {
  if(!IsSystem(id))throw new InvalidDataException("Unknown imported system "+id);int n=int.Parse(id[7..]);
  if(cache.TryGetValue(n,out var loaded)){order.Remove(n);order.AddLast(n);return loaded;}
  using var file=File.OpenRead(Path.Combine(Root,n+".json.gz"));using var zip=new GZipStream(file,CompressionMode.Decompress);
  var p=JsonSerializer.Deserialize<SpatialPackage43>(zip,options)??throw new InvalidDataException("Empty spatial package");
  if(p.SystemId!=n||p.Bodies.Any(b=>b.System!=id||!b.Position.Finite||!double.IsFinite(b.Radius)||b.Radius<0))throw new InvalidDataException("Invalid spatial package");
  p.ById=p.Bodies.ToDictionary(b=>b.Id);foreach(var b in p.Bodies)if(b.Parent.Length>0&&!p.ById.ContainsKey(b.Parent))throw new InvalidDataException("Invalid local orbit parent");
  p.Celestials=p.Bodies.Select(b=>b.Celestial).ToArray();if(cache.Count==CacheLimit){cache.Remove(order.First!.Value);order.RemoveFirst();}cache[n]=p;order.AddLast(n);Loads++;return p;
 }
 public static readonly Lazy<Landmark43[]> LandmarkData=new(()=>JsonSerializer.Deserialize<Landmark43[]>(File.ReadAllText(Path.Combine(Root,"landmarks.json")))!);
 public static string GateTo(string system,string destination)=>Get(system).Bodies.FirstOrDefault(b=>b.Kind==CelestialKind.Stargate&&b.Destination==destination)?.Id??"";
 public static InformationEntry? Information(string key,string current)
 {
  if(Stations44.Information(key) is {} stationInfo)return stationInfo;
  if(key.StartsWith("celestial/system:")&&key.Contains("/station:"))return Stations44.Information("station44/"+key[(key.LastIndexOf('/')+1)..]);
  if(key.StartsWith("system/system:")&&IsSystem(key[7..])&&Retheme41.TryInformation("lore/"+key[7..],out var s)){GalaxyInformation42.Attach(s);s.System=key[7..];if(!s.Tabs.ContainsKey("Local space")){var p=Get(s.System);foreach(var group in p.Bodies.GroupBy(b=>b.Kind))s.Add("Local space",group.Key.ToString(),group.Count().ToString());foreach(var st in Stations44.BySystem.GetValueOrDefault(s.System)??[])s.Add("Local space",st.Name,"Station","station44/"+st.Id);foreach(var body in p.Bodies.Where(b=>b.Kind is CelestialKind.Sun or CelestialKind.Planet or CelestialKind.Beacon or CelestialKind.Stargate))s.Add("Local space",body.Name,body.Type,"celestial/"+s.System+"/"+body.Id);}return s;}
  if(key.StartsWith("landmark43/")&&int.TryParse(key[11..],out int l)&&LandmarkData.Value.FirstOrDefault(x=>x.Id==l) is {} landmark){var e=new InformationEntry{Key=key,Id=l.ToString(),Name=landmark.Name,Kind="Galactic landmark",Group="Charted universe / Landmarks",Description=landmark.Description};e.Add("Description",e.Description);e.Add("Attributes","Galactic position (metres)",string.Join(", ",landmark.GalacticPosition.Select(x=>x.ToString("R"))));e.Add("Attributes","Local warp position","Not supplied by source; no local arrival is fabricated.");if(landmark.SystemId is {} sys)e.Add("Relationships","System",Galaxy42.ById[sys].Name,"lore/system:"+sys);e.Add("Source audit","Source name",landmark.SourceName.ToString());e.Add("Source audit","Source description",landmark.SourceDescription.ToString());return e;}
  string bodyKey=key.StartsWith("celestial/")?key[10..]:"";if(bodyKey.StartsWith("system:")&&bodyKey.Contains('/')){int split=bodyKey.IndexOf('/');current=bodyKey[..split];bodyKey=bodyKey[(split+1)..];}
  if(bodyKey.Length==0||!IsSystem(current)||!Get(current).ById.TryGetValue(bodyKey,out var b))return null;
  var entry=new InformationEntry{Key=key,Id=b.Id,Name=b.Name,Kind=b.Type,Group="Charted universe / Local celestials",System=current,Description=b.Kind==CelestialKind.AsteroidBelt?"Original 2019 belt anchor. Asteroid formations and resources are authored game content; the export supplies the belt position and radius.":b.Type+" at its exported local coordinate."};
  entry.Add("Description",entry.Description);entry.Add("Attributes","Local metres",$"{b.Position.X:R}, {b.Position.Y:R}, {b.Position.Z:R}");entry.Add("Attributes","Radius",b.RadiusKnown?SolarCatalog.Distance(b.Radius):"Not supplied");
  if(b.Parent.Length>0)entry.Add("Relationships","Orbit parent",Get(current).ById[b.Parent].Name,"celestial/"+current+"/"+b.Parent);
  foreach(var child in Get(current).Bodies.Where(child=>child.Parent==b.Id))entry.Add("Relationships",child.Name,child.Type,"celestial/"+current+"/"+child.Id);
  entry.Add("Source audit","Source ID",b.SourceId.ToString());entry.Add("Source audit","Type ID",b.TypeId.ToString());entry.Add("Source audit","Era",b.SourceEra);entry.Add("Source audit","Orbit ID",b.ParentSourceId.ToString());entry.Add("Source audit","Modern build","3569502");if(b.Statistics.ValueKind==JsonValueKind.Object)foreach(var field in b.Statistics.EnumerateObject())entry.Add("Source audit","statistics."+field.Name,field.Value.ToString());if(b.Attributes.ValueKind==JsonValueKind.Object)foreach(var field in b.Attributes.EnumerateObject())entry.Add("Source audit","attributes."+field.Name,field.Value.ToString());if(b.EffectBeaconTypeId is {} effect)entry.Add("Source audit","Effect beacon type ID",effect.ToString()+" / metadata retained; environment modifiers are not active in this pass");return entry;
 }
}
public sealed partial class DemoWorld
{
 public bool ImportedWorld43=>WorldDefinitionId==Spatial43.WorldId;
 public static string ImportedSavePath43=>Path.Combine(Path.GetDirectoryName(SavePath)!,"save-universe43.json");
 public string ActiveSavePath43=>ImportedWorld43?ImportedSavePath43:SavePath;
 public static StarSystem ResolveSystem43(string id){if(Spatial43.IsSystem(id)){var s=Galaxy42.ById[int.Parse(id[7..])];return new(id,s.Name,Galaxy42.Regions[s.RegionId].Name,(float)s.Security,s.Affiliation,s.Neighbours.Select(x=>"system:"+x).ToArray(),"Imported exploration system");}return Systems.First(s=>s.Id==id);}
 public void NewExploration43(int system)
 {
  if(!Galaxy42.ById.ContainsKey(system))throw new ArgumentOutOfRangeException(nameof(system));
  NewGame(1);MarketOrders.Clear();MarketTrades.Clear();StationItems.Clear();Industry.Blueprints.Clear();WorldDefinitionId=Spatial43.WorldId;JumpBeacons.Clear();RelayNodes.Clear();Wormholes.Clear();JumpClones.Clear();ParkedShips.Clear();StationId="";PendingStation44="";SystemId="system:"+system;SolarCelestial="";SolarGrids.Clear();SolarOrigin=default;SolarGridKey="";
  var first=Celestials.FirstOrDefault(c=>c.Kind==CelestialKind.AsteroidBelt)??Celestials.FirstOrDefault(c=>c.Kind==CelestialKind.Stargate)??Celestials.FirstOrDefault();
  Log.Clear();EnterGrid(first?.Id??"deep_space",false);SolarGridKey=SolarCatalog.GridKey(SystemId,SystemPosition);RebaseSpatial43(true);Say("Imported exploration / original 2019 belts. Stations and basic mission adaptations are available.");
 }
 public void RebaseSpatial43(bool force=false)
 {
  if(InAbyssF||!ImportedWorld43||InWarp||Transit!=null||(!force&&Player.Position.Length()<32768))return;
  var shift=Player.Position;SolarOrigin+=SpacePosition.From(shift);Player.Position=Vector3.Zero;Player.ManualPoint-=shift;
  foreach(var t in Targets)t.Position-=shift;foreach(var m in MissilesInFlight)m.Position-=shift;foreach(var z in EwarZones)z.Center-=shift;foreach(var v in WeaponVisuals){v.Start-=shift;v.End-=shift;}foreach(var d in Player.Drones)d.MovePoint-=shift;
 }
}
public sealed partial class DemoWorld
{
 void PopulateOriginalBelt43(Celestial c)
 {
  // Stable authored formation, not an assertion about historical individual rocks.
  uint seed=2166136261;foreach(char letter in c.Id)seed=(seed^letter)*16777619;
  var random=new Random(unchecked((int)seed));int count=18+random.Next(12);float radius=(float)Math.Clamp(c.Radius,6000,30000);
  for(int i=0;i<count;i++){double a=random.NextDouble()*Math.Tau;float d=1500+(float)random.NextDouble()*radius;Targets.Add(new(){Id=c.Id+":asteroid:"+i,Name=(i%4==0?"Deep alloy asteroid ":"Vesperite asteroid ")+(i+1),Kind=TargetKind.Asteroid,Resource=i%4==0?"deepore":"ore",Ore=140+random.Next(240),Position=(c.Position-SolarOrigin).Local+new Vector3((float)Math.Cos(a)*d,(float)(random.NextDouble()-.5)*900,(float)Math.Sin(a)*d),Signature=200});}
 }
}
