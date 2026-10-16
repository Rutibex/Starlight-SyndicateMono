using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public readonly record struct SpacePosition(double X,double Y,double Z)
{
 public static SpacePosition operator +(SpacePosition a,SpacePosition b)=>new(a.X+b.X,a.Y+b.Y,a.Z+b.Z);
 public static SpacePosition operator -(SpacePosition a,SpacePosition b)=>new(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
 public static SpacePosition operator *(SpacePosition a,double k)=>new(a.X*k,a.Y*k,a.Z*k);
 [JsonIgnore] public double Length=>Math.Sqrt(X*X+Y*Y+Z*Z);
 [JsonIgnore] public SpacePosition Unit=>Length>1?this*(1/Length):new(1,0,0);
 [JsonIgnore] public Vector3 Local=>new((float)X,(float)Y,(float)Z);
 public static SpacePosition From(Vector3 v)=>new(v.X,v.Y,v.Z);
 [JsonIgnore] public bool Finite=>double.IsFinite(X)&&double.IsFinite(Y)&&double.IsFinite(Z);
}
public enum CelestialKind { Sun,Planet,Moon,AsteroidBelt,IceField,GasCloud,Station,Stargate,Anomaly,Beacon,Wormhole }
public sealed record Celestial(string Id,string System,string Name,CelestialKind Kind,SpacePosition Position,double Radius,string Parent,string Type,string Destination="")
{
 public SpacePosition WarpPosition=>Id.StartsWith("station:")?Stations44.ById[Id].DockPoint:Kind is CelestialKind.Sun or CelestialKind.Planet or CelestialKind.Moon?Position+new SpacePosition(0,Radius+(Kind==CelestialKind.Sun?100000000:500000),0):Position;
 public SpacePosition Anchor=>WarpPosition+(Kind==CelestialKind.Station&&!Id.StartsWith("station:")?new SpacePosition(1000,0,-800):default);
}
public static class SolarCatalog
{
 public const double AU=149597870700,GridSize=8000000,MinimumWarp=150000;
 public static readonly Dictionary<string,Celestial[]> Systems=DemoWorld.Systems.ToDictionary(s=>s.Id,Build);
 public static readonly Dictionary<string,Celestial> ById=Systems.Values.SelectMany(a=>a).ToDictionary(c=>c.Id);
 public static string GridKey(string system,SpacePosition p)=>$"{system}:{Math.Floor((p.X+GridSize/2)/GridSize)}:{Math.Floor((p.Y+GridSize/2)/GridSize)}:{Math.Floor((p.Z+GridSize/2)/GridSize)}";
 public static SpacePosition GridCenter(SpacePosition p)=>new(Math.Floor((p.X+GridSize/2)/GridSize)*GridSize,Math.Floor((p.Y+GridSize/2)/GridSize)*GridSize,Math.Floor((p.Z+GridSize/2)/GridSize)*GridSize);
 public static string Distance(double meters)=>meters>=AU*.1?$"{meters/AU:0.00} AU":meters>=10000?$"{meters/1000:N0} km":$"{meters:N0} m";
 static Celestial[] Build(StarSystem s)
 {
  int index=Array.FindIndex(DemoWorld.Systems,x=>x.Id==s.Id);var rng=new Random(31031+index*997);var list=new List<Celestial>();
  list.Add(new(s.Id+":sun",s.Id,s.Name+" - Star",CelestialKind.Sun,default,(.65+index*.08)*696340000,"","G"+(index%8)+" main sequence"));
  string[] types=["Barren","Temperate","Oceanic","Lava","Gas giant","Ice","Storm","Gas giant","Ice"];
  string[] roman=["I","II","III","IV","V","VI","VII","VIII","IX"];
  int count=6+index%4;double orbit=.32+index*.02;
  for(int i=0;i<count;i++){
   orbit*=1.55+rng.NextDouble()*.32;double angle=rng.NextDouble()*Math.Tau;var p=new SpacePosition(Math.Cos(angle)*orbit*AU,Math.Sin(angle*.7)*orbit*AU*.018,Math.Sin(angle)*orbit*AU);
   string id=$"{s.Id}:planet:{i+1}",name=s.Name+" "+roman[i];double radius=types[i]=="Gas giant"?(48000000+rng.NextDouble()*20000000):2800000+rng.NextDouble()*5000000;
   list.Add(new(id,s.Id,name,CelestialKind.Planet,p,radius,s.Id+":sun",types[i]));
   int moons=i==0?1:2+rng.Next(i>3?10:4);for(int m=0;m<moons;m++){double a=rng.NextDouble()*Math.Tau,r=(radius*3+70000000)*(1+m*.8);var mp=p+new SpacePosition(Math.Cos(a)*r,Math.Sin(a*.3)*r*.13,Math.Sin(a)*r);list.Add(new(id+":moon:"+(m+1),s.Id,name+" - Moon "+(m+1),CelestialKind.Moon,mp,450000+rng.NextDouble()*1200000,id,"Rocky moon"));}
   if(i>0){var bp=p+new SpacePosition(-radius*8-700000000,0,radius*5);list.Add(new(id+":belt:1",s.Id,name+" - Asteroid Belt 1",CelestialKind.AsteroidBelt,bp,30000,id,"Vesperite / refined ore"));if(i%2==0)list.Add(new(id+":belt:2",s.Id,name+" - Asteroid Belt 2",CelestialKind.AsteroidBelt,bp+new SpacePosition(300000000,25000000,600000000),40000,id,"Deep ore / moon ore"));}
  }
  var home=list.First(c=>c.Kind==CelestialKind.Planet&&c.Id.EndsWith(":3"));list.Add(new(s.Id+":station",s.Id,s.Name+" Anchorage",CelestialKind.Station,home.Position+new SpacePosition(0,home.Radius+250000000,0),1100,home.Id,s.Faction+" station"));
  var outer=list.Last(c=>c.Kind==CelestialKind.Planet);list.Add(new(s.Id+":ice",s.Id,s.Name+" Ice Field",CelestialKind.IceField,outer.Position+new SpacePosition(900000000,10000000,0),60000,outer.Id,"Ice harvesting"));
  list.Add(new(s.Id+":gas",s.Id,s.Name+" Vapor Reservoir",CelestialKind.GasCloud,outer.Position+new SpacePosition(-1200000000,0,500000000),50000,outer.Id,"Gas harvesting"));
  int gate=0;foreach(string dest in s.Links){double a=(index*.7+gate++*2.2);double r=(3+gate*3.6)*AU;list.Add(new(s.Id+":gate:"+dest,s.Id,DemoWorld.Systems.First(x=>x.Id==dest).Name+" Stargate",CelestialKind.Stargate,new(Math.Cos(a)*r,r*.02,Math.Sin(a)*r),1500,"","Stargate",dest));}
  list.Add(new(s.Id+":beacon",s.Id,s.Name+" Navigation Beacon",CelestialKind.Beacon,home.Position+new SpacePosition(AU*.02,0,AU*.01),300,home.Id,"Navigation relay"));
  list.Add(new(s.Id+":anomaly",s.Id,s.Name+" Ruined Relay",CelestialKind.Anomaly,outer.Position+new SpacePosition(AU*.035,0,AU*.02),400,"","Relic archive"));
  if(s.Id=="umbra")list.Add(new("umbra:signal",s.Id,"JoyOS memory vault",CelestialKind.Anomaly,new(AU*17,AU*.03,-AU*12),200,"","Unresolved signature"));
  if(s.Id is "rim" or "blackquota")list.Add(new(s.Id+":wormhole",s.Id,"Unstable Rift",CelestialKind.Wormhole,outer.Position+new SpacePosition(AU*.05,AU*.01,0),1800,"","Unstable wormhole",s.Id=="rim"?"blackquota":"rim"));
  return list.ToArray();
 }
 public static Celestial[] ForSystem(string system)=>Spatial43.IsSystem(system)?Spatial43.Get(system).Celestials.Concat((Stations44.BySystem.GetValueOrDefault(system)??[]).Select(s=>s.Celestial)).ToArray():Systems[system];
 public static Celestial? Resolve(string system,string id)=>Spatial43.IsSystem(system)?ForSystem(system).FirstOrDefault(c=>c.Id==id||(id=="belt"&&c.Kind==CelestialKind.AsteroidBelt)):id switch{"station"=>ById[system+":station"],"belt"=>Systems[system].First(c=>c.Kind==CelestialKind.AsteroidBelt),"signal"=>system=="umbra"?ById["umbra:signal"]:null,_=>ById.GetValueOrDefault(id) is {} c&&c.System==system?c:null};
}
public sealed class WarpCurve
{
 public double Distance,Peak,Acceleration,Deceleration,AccelerationTime,CruiseTime,DecelerationTime,AccelerationDistance,CruiseDistance,ExitSpeed;
 public double Total=>AccelerationTime+CruiseTime+DecelerationTime;
 public static WarpCurve Calculate(double distance,double rate,double sublight,double accelerationBonus=1){var w=new WarpCurve{Distance=distance,Acceleration=Math.Max(.1,rate*accelerationBonus),Deceleration=Math.Min(2,Math.Max(.1,rate*accelerationBonus/3)),ExitSpeed=Math.Max(1,Math.Min(100,sublight*.75))};double max=rate*SolarCatalog.AU;w.Peak=Math.Min(max,(distance+1+w.ExitSpeed/w.Deceleration)/(1/w.Acceleration+1/w.Deceleration));w.Peak=Math.Max(w.ExitSpeed,w.Peak);w.AccelerationTime=Math.Log(w.Peak/w.Acceleration)/w.Acceleration;w.AccelerationDistance=(w.Peak-w.Acceleration)/w.Acceleration;double dd=(w.Peak-w.ExitSpeed)/w.Deceleration;w.CruiseDistance=Math.Max(0,distance-w.AccelerationDistance-dd);w.CruiseTime=w.CruiseDistance/w.Peak;w.DecelerationTime=Math.Log(w.Peak/w.ExitSpeed)/w.Deceleration;return w;}
 public (double Distance,double Velocity,string Phase) Sample(double seconds){if(seconds<=AccelerationTime){double v=Acceleration*Math.Exp(Acceleration*Math.Max(0,seconds));return((v-Acceleration)/Acceleration,v,"Accelerating");}seconds-=AccelerationTime;if(seconds<=CruiseTime)return(AccelerationDistance+Peak*seconds,Peak,"Cruising");seconds-=CruiseTime;double speed=Peak*Math.Exp(-Deceleration*Math.Min(seconds,DecelerationTime));double x=AccelerationDistance+CruiseDistance+(Peak-speed)/Deceleration;return(Math.Min(Distance,x),speed,"Decelerating");}
}
