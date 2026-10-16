using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed class GalaxySystem42
{
 public int Id{get;set;}public string Name{get;set;}="";public int RegionId{get;set;}public int ConstellationId{get;set;}public double[] Position{get;set;}=[];public double[] Flat{get;set;}=[];public string FlatSource{get;set;}="";public double Security{get;set;}public int? FactionId{get;set;}public string Affiliation{get;set;}="";public int Component{get;set;}public int[] Neighbours{get;set;}=[];public int? WormholeClass{get;set;}
}
public sealed class GalaxyPlace42 {public int Id{get;set;}public string Name{get;set;}="";public double[] Position{get;set;}=[];public double[] Flat{get;set;}=[];public string FlatSource{get;set;}="";public int ParentId{get;set;}public int[] Members{get;set;}=[];}
public sealed class GalaxyGate42 {public int Id{get;set;}public int SystemId{get;set;}public int DestinationSystemId{get;set;}public int DestinationGateId{get;set;}public int TypeId{get;set;}public double[] Position{get;set;}=[];}
public sealed class GalaxyComponent42 {public int Id{get;set;}public int Systems{get;set;}public int Seed{get;set;}public bool Gated{get;set;}}
public sealed class GalaxyData42 {public int Version{get;set;}public int Build{get;set;}public string WorldId{get;set;}="";public GalaxySystem42[] Systems{get;set;}=[];public GalaxyPlace42[] Regions{get;set;}=[];public GalaxyPlace42[] Constellations{get;set;}=[];public GalaxyGate42[] Gates{get;set;}=[];public GalaxyComponent42[] Components{get;set;}=[];}
public enum GalaxyRouteMode42 {Shortest,Safer}
public sealed record GalaxyRoute42(bool Found,int[] Systems,double Cost,string Reason){public int Jumps=>Math.Max(0,Systems.Length-1);}
public sealed class GalaxyPlan42
{
 public string WorldId{get;set;}="sde-3569502";public int Selected{get;set;}=30000001;public int From{get;set;}=30000001;public int To{get;set;}public GalaxyRouteMode42 Mode{get;set;}public int[] Avoid{get;set;}=[];
 public bool Valid()=>WorldId==Galaxy42.Data.WorldId&&Galaxy42.ById.ContainsKey(Selected)&&Galaxy42.ById.ContainsKey(From)&&(To==0||Galaxy42.ById.ContainsKey(To))&&Enum.IsDefined(Mode)&&Avoid!=null&&Avoid.All(Galaxy42.ById.ContainsKey);
}
public readonly record struct GalaxyPoint42(double X,double Y){public double this[int axis]=>axis==0?X:axis==1?Y:throw new ArgumentOutOfRangeException(nameof(axis));public bool IsFinite=>double.IsFinite(X)&&double.IsFinite(Y);}
public static class Galaxy42
{
 static readonly Lazy<GalaxyData42> data=new(()=>JsonSerializer.Deserialize<GalaxyData42>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Galaxy42","galaxy.json")))??throw new InvalidDataException("Missing galaxy graph"));
 public static GalaxyData42 Data=>data.Value;
 static readonly Lazy<IReadOnlyDictionary<int,GalaxySystem42>> systems=new(()=>Data.Systems.ToDictionary(s=>s.Id));public static IReadOnlyDictionary<int,GalaxySystem42> ById=>systems.Value;
 static readonly Lazy<IReadOnlyDictionary<int,GalaxyPlace42>> regions=new(()=>Data.Regions.ToDictionary(s=>s.Id));public static IReadOnlyDictionary<int,GalaxyPlace42> Regions=>regions.Value;
 static readonly Lazy<IReadOnlyDictionary<int,GalaxyPlace42>> constellations=new(()=>Data.Constellations.ToDictionary(s=>s.Id));public static IReadOnlyDictionary<int,GalaxyPlace42> Constellations=>constellations.Value;
 public const double LightYear=9.4607304725808e15;
 public static double StepCost(GalaxySystem42 destination,GalaxyRouteMode42 mode)=>mode==GalaxyRouteMode42.Shortest?1:1+(destination.Security<.45?50:0)+(destination.Security<=0?100:0);
 public static GalaxyRoute42 Route(int start,int end,GalaxyRouteMode42 mode,IEnumerable<int>? avoid=null)
 {
  if(!ById.ContainsKey(start)||!ById.ContainsKey(end)||!Enum.IsDefined(mode))return new(false,[],0,"Choose valid route endpoints.");
  var blocked=avoid?.ToHashSet()??[];if(blocked.Contains(start)||blocked.Contains(end))return new(false,[],0,"An endpoint is on the avoidance list.");
  if(start==end)return new(true,[start],0,"");
  if(ById[start].Component!=ById[end].Component)return new(false,[],0,"No static gate route: these systems belong to separate graph components. Temporary connections are not in this graph.");
  var distance=new Dictionary<int,double>{{start,0}};var previous=new Dictionary<int,int>();var queue=new PriorityQueue<int,(double Cost,int Id)>();queue.Enqueue(start,(0,start));
  while(queue.TryDequeue(out int current,out var priority))
  {
   if(priority.Cost!=distance[current])continue;if(current==end)break;
   foreach(int next in ById[current].Neighbours){if(blocked.Contains(next))continue;double cost=priority.Cost+StepCost(ById[next],mode);if(distance.TryGetValue(next,out var old)&&old<=cost)continue;distance[next]=cost;previous[next]=current;queue.Enqueue(next,(cost,next));}
  }
  if(!distance.TryGetValue(end,out var total))return new(false,[],0,"No gate route remains with the current avoidance list.");
  var path=new List<int>{end};while(path[^1]!=start)path.Add(previous[path[^1]]);path.Reverse();return new(true,path.ToArray(),total,"");
 }
 public static bool Visible(GalaxySystem42 s,int connection,int security,int region,int constellation=0)=> (connection==0||connection==1&&s.Neighbours.Length>0||connection==2&&s.Neighbours.Length==0)&&(security==0||security==1&&s.Security>=.45||security==2&&s.Security>0&&s.Security<.45||security==3&&s.Security<=0)&&(region==0||s.RegionId==region)&&(constellation==0||s.ConstellationId==constellation);
 public static GalaxyPoint42 Project(GalaxySystem42 s,bool threeD,double yaw,double pitch)
 {
  if(!threeD)return new(s.Flat[0]/LightYear,s.Flat[1]/LightYear);double x=s.Position[0]/LightYear,y=s.Position[1]/LightYear,z=s.Position[2]/LightYear;double a=x*Math.Cos(yaw)+z*Math.Sin(yaw),b=-x*Math.Sin(yaw)+z*Math.Cos(yaw);return new(a,y*Math.Cos(pitch)+b*Math.Sin(pitch));
 }
}
public sealed partial class DemoWorld
{
 public const string LegacyWorld42="legacy-nine-system";public string WorldDefinitionId{get;set;}=LegacyWorld42;public int SaveSchemaVersion{get;set;}=54;public int SourceBuild{get;set;}=3569502;public GalaxyPlan42? GalaxyPlan{get;set;}
}
