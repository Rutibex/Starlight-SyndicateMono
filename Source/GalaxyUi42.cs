using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 GalaxyPlan42 Plan42=>world.GalaxyPlan??=new();
 int galaxyConnection42=1,galaxySecurity42,galaxyRegion42,galaxyConstellation42,galaxyKind42,galaxyResultPage42,galaxyRoutePage42;
 string galaxyQuery42="",galaxySearchStamp42="",galaxyProjectionStamp42="";bool galaxyFocus42,galaxyThreeD42,galaxyDragging42,galaxyRotating42;
 double galaxyYaw42=.45,galaxyPitch42=.65,galaxyCenterX42,galaxyCenterY42,galaxySpan42=1,galaxyZoom42=1;
 Vector2 galaxyDragStart42,galaxyLastMouse42;Rectangle galaxyViewport42,galaxyResultsRect42;
 (string Kind,int Id,string Name)[] galaxyResults42=[];GalaxySystem42[] galaxyVisible42=[];
 readonly Dictionary<int,Vector2> galaxyPoints42=[];GalaxyRoute42 galaxyRoute42=new(false,[],0,"Choose route endpoints.");readonly HashSet<(int,int)> galaxyRouteEdges42=[];
 int galaxyDrawNodes42,galaxyDrawEdges42,galaxyProjectionRebuilds42;double galaxyLastProjectionMs42;
 void OpenGalaxy42(int selected=0){if(selected!=0&&Galaxy42.ById.ContainsKey(selected)){Plan42.Selected=selected;galaxyRegion42=Galaxy42.ById[selected].RegionId;galaxyConstellation42=0;galaxyConnection42=0;galaxySecurity42=0;}EnsureWindow(ViewMode.Galaxy);galaxyFocus42=false;RefreshGalaxyRoute42();FitGalaxy42();}
 void RefreshGalaxyRoute42(){galaxyRoute42=Galaxy42.Route(Plan42.From,Plan42.To,Plan42.Mode,Plan42.Avoid);galaxyRouteEdges42.Clear();for(int i=1;i<galaxyRoute42.Systems.Length;i++)galaxyRouteEdges42.Add((Math.Min(galaxyRoute42.Systems[i-1],galaxyRoute42.Systems[i]),Math.Max(galaxyRoute42.Systems[i-1],galaxyRoute42.Systems[i])));galaxyRoutePage42=0;}
 void SelectGalaxy42(int id){Plan42.Selected=id;}
 void GalaxyText42(char c){if(!galaxyFocus42)return;if(c is '\r' or '\n'){galaxyFocus42=false;return;}if(c=='\b'){if(galaxyQuery42.Length>0)galaxyQuery42=galaxyQuery42[..^1];}else if(!char.IsControl(c)&&galaxyQuery42.Length<64)galaxyQuery42+=c;galaxyResultPage42=0;}
 void RefreshGalaxySearch42()
 {
  string stamp=$"{galaxyQuery42}|{galaxyKind42}|{galaxyConnection42}|{galaxySecurity42}|{galaxyRegion42}|{galaxyConstellation42}";if(stamp==galaxySearchStamp42)return;galaxySearchStamp42=stamp;
  bool Match(int id,string name,string extra="")=>galaxyQuery42.Length==0||(name+" "+id+" "+extra).Contains(galaxyQuery42,StringComparison.OrdinalIgnoreCase);
  galaxyResults42=galaxyKind42==3?Spatial43.LandmarkData.Value.Where(l=>Match(l.Id,l.Name)).Select(l=>("landmark",l.Id,l.Name)).ToArray():galaxyKind42==0?Galaxy42.Data.Systems.Where(s=>Galaxy42.Visible(s,galaxyConnection42,galaxySecurity42,galaxyRegion42,galaxyConstellation42)&&Match(s.Id,s.Name,s.Affiliation)).Select(s=>("system",s.Id,s.Name)).OrderBy(s=>s.Name,StringComparer.Ordinal).ToArray():galaxyKind42==1?Galaxy42.Data.Regions.Where(s=>Match(s.Id,s.Name)).Select(s=>("region",s.Id,s.Name)).OrderBy(s=>s.Name,StringComparer.Ordinal).ToArray():Galaxy42.Data.Constellations.Where(s=>(galaxyRegion42==0||s.ParentId==galaxyRegion42)&&Match(s.Id,s.Name)).Select(s=>("constellation",s.Id,s.Name)).OrderBy(s=>s.Name,StringComparer.Ordinal).ToArray();
 }
 void FitGalaxy42()
 {
  galaxyVisible42=Galaxy42.Data.Systems.Where(s=>Galaxy42.Visible(s,galaxyConnection42,galaxySecurity42,galaxyRegion42,galaxyConstellation42)).ToArray();galaxyZoom42=1;
  if(galaxyVisible42.Length>0){double minx=double.MaxValue,miny=minx,maxx=double.MinValue,maxy=maxx;foreach(var s in galaxyVisible42){var p=Galaxy42.Project(s,galaxyThreeD42,galaxyYaw42,galaxyPitch42);minx=Math.Min(minx,p[0]);maxx=Math.Max(maxx,p[0]);miny=Math.Min(miny,p[1]);maxy=Math.Max(maxy,p[1]);}galaxyCenterX42=(minx+maxx)/2;galaxyCenterY42=(miny+maxy)/2;galaxySpan42=Math.Max(.001,Math.Max(maxx-minx,maxy-miny));}
  galaxyProjectionStamp42="";
 }
 double GalaxyScale42=>Math.Min(galaxyViewport42.Width,galaxyViewport42.Height)*.9/galaxySpan42*galaxyZoom42;
 void ProjectGalaxy42()
 {
  string stamp=$"{galaxyViewport42}|{galaxyCenterX42:R}|{galaxyCenterY42:R}|{galaxyZoom42:R}|{galaxyYaw42:R}|{galaxyPitch42:R}|{galaxyThreeD42}|{galaxyConnection42}|{galaxySecurity42}|{galaxyRegion42}|{galaxyConstellation42}";if(stamp==galaxyProjectionStamp42)return;galaxyProjectionStamp42=stamp;galaxyProjectionRebuilds42++;var sw=System.Diagnostics.Stopwatch.StartNew();galaxyPoints42.Clear();
  foreach(var s in galaxyVisible42){var p=Galaxy42.Project(s,galaxyThreeD42,galaxyYaw42,galaxyPitch42);double x=galaxyViewport42.Center.X+(p[0]-galaxyCenterX42)*GalaxyScale42,y=galaxyViewport42.Center.Y-(p[1]-galaxyCenterY42)*GalaxyScale42;if(double.IsFinite(x)&&double.IsFinite(y)&&x>=galaxyViewport42.X+5&&x<galaxyViewport42.Right-5&&y>=galaxyViewport42.Y+5&&y<galaxyViewport42.Bottom-5)galaxyPoints42[s.Id]=new((float)x,(float)y);}
  galaxyLastProjectionMs42=sw.Elapsed.TotalMilliseconds;
 }
 bool GalaxyInput42(MouseState m,KeyboardState keys)
 {
  var top=WindowAt(mouse);bool leftDown=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released;if(leftDown&&!ui.Hits.Any(h=>h.Tip=="Galaxy search"&&h.Rect.Contains(mouse.ToPoint())))galaxyFocus42=false;if(top?.Kind!=ViewMode.Galaxy||top.Minimized)return false;
  if(galaxyFocus42&&Press(Keys.Escape,keys)){galaxyFocus42=false;return true;}
  int wheel=m.ScrollWheelValue-oldMouse.ScrollWheelValue;
  if(galaxyResultsRect42.Contains(mouse.ToPoint())&&wheel!=0){galaxyResultPage42=Math.Max(0,galaxyResultPage42-wheel/120);return true;}
  bool down=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released,up=m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed;
  bool right=m.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released;
  if(galaxyDragging42||galaxyRotating42){var delta=mouse-galaxyLastMouse42;galaxyLastMouse42=mouse;if(galaxyDragging42){galaxyCenterX42-=delta.X/GalaxyScale42;galaxyCenterY42+=delta.Y/GalaxyScale42;if(up){galaxyDragging42=false;if(Vector2.Distance(mouse,galaxyDragStart42)<6)SelectNearestGalaxy42(mouse);}}else{galaxyYaw42+=delta.X*.008;galaxyPitch42=Math.Clamp(galaxyPitch42+delta.Y*.008,-1.5,1.5);if(m.RightButton==ButtonState.Released)galaxyRotating42=false;}return true;}
  if(!galaxyViewport42.Contains(mouse.ToPoint()))return false;
  if(wheel!=0){galaxyZoom42=Math.Clamp(galaxyZoom42*Math.Pow(1.22,wheel/120.0),.05,10000);return true;}
  if(down){galaxyFocus42=false;galaxyDragging42=true;galaxyDragStart42=galaxyLastMouse42=mouse;return true;}
  if(right){galaxyRotating42=galaxyThreeD42;galaxyLastMouse42=mouse;return true;}
  return false;
 }
 void SelectNearestGalaxy42(Vector2 point){int selected=0;float best=121;foreach(var p in galaxyPoints42){float d=Vector2.DistanceSquared(p.Value,point);if(d<best){selected=p.Key;best=d;}}if(selected!=0)SelectGalaxy42(selected);}
 static Color GalaxySecurityColor42(double security)=>security>=.45?Cyan:security>0?Gold:Red;
 void DrawGalaxy42(DesktopWindow window)
 {
  var sw=System.Diagnostics.Stopwatch.StartNew();if(Environment.GetCommandLineArgs().Contains("--galaxy-profile-motion")){galaxyYaw42+=.005;galaxyCenterX42+=galaxySpan42*.0004;}var r=window.Bounds;ui.Box(new(r.X+1,r.Y+28,r.Width-2,r.Height-29),new Color(8,15,22));int x=r.X+12,y=r.Y+39,left=270,right=285,mapx=x+left+12;
  ui.Button(new(x,y,126,26),"Campaign cluster",()=>{galaxyFocus42=false;EnsureWindow(ViewMode.Map);},"Galaxy legacy campaign");ui.Button(new(x+132,y,138,26),"Imported galaxy",()=>{},"Galaxy imported world",true);
  ui.Button(new(mapx,y,88,26),galaxyThreeD42?"3D":"2D",()=>{galaxyThreeD42=!galaxyThreeD42;FitGalaxy42();},"Galaxy projection");ui.Button(new(mapx+94,y,75,26),"Fit view",FitGalaxy42,"Galaxy fit view");ui.Button(new(mapx+175,y,45,26),"+",()=>galaxyZoom42=Math.Min(10000,galaxyZoom42*1.5),"Galaxy zoom in");ui.Button(new(mapx+226,y,45,26),"-",()=>galaxyZoom42=Math.Max(.05,galaxyZoom42/1.5),"Galaxy zoom out");
  int rx=r.Right-right-12;ui.Text("STATIC GATE ROUTE",rx,y+7,Cyan,12);y+=36;
  ui.Button(new(x,y,left-31,27),galaxyQuery42.Length==0?"Find name, ID or affiliation":ShortInfo(galaxyQuery42,29)+(galaxyFocus42?" |":""),()=>{galaxyFocus42=true;encyclopediaSearchFocus=false;marketTextFocus="";skillFocus=industryFocus="";},"Galaxy search");ui.Button(new(x+left-26,y,26,27),"x",()=>{galaxyQuery42="";galaxyFocus42=false;galaxyResultPage42=0;},"Galaxy clear search");
  string[] kinds=["Systems","Regions","Constellations","Landmarks"];for(int i=0;i<4;i++){int k=i;ui.Button(new(x+i*68,y+34,64,25),ShortInfo(kinds[i],8),()=>{galaxyKind42=k;galaxyResultPage42=0;},"Galaxy kind / "+kinds[i],galaxyKind42==i);}
  string[] con=["All records","With gates","No gates"];for(int i=0;i<3;i++){int k=i;ui.Button(new(mapx+i*94,y,90,25),con[i],()=>{galaxyConnection42=k;galaxyResultPage42=0;FitGalaxy42();},"Galaxy connections / "+con[i],galaxyConnection42==i);}
  string[] sec=["All security","High","Low","Null"];for(int i=0;i<4;i++){int k=i;ui.Button(new(mapx+i*74,y+32,70,25),sec[i],()=>{galaxySecurity42=k;galaxyResultPage42=0;FitGalaxy42();},"Galaxy security / "+sec[i],galaxySecurity42==i);}
  if(galaxyRegion42!=0)ui.Button(new(mapx+302,y+32,Math.Max(72,rx-mapx-314),25),"All regions",()=>{galaxyRegion42=galaxyConstellation42=0;FitGalaxy42();},"Galaxy all regions");
  int top=y+70;galaxyViewport42=new(mapx,top,rx-mapx-13,r.Bottom-top-61);ui.Box(galaxyViewport42,new Color(3,9,15));ui.Border(galaxyViewport42,new Color(39,65,81));for(int gx=galaxyViewport42.X+40;gx<galaxyViewport42.Right;gx+=70)ui.Line(new(gx,top+1),new(gx,galaxyViewport42.Bottom-1),new Color(16,29,41));for(int gy=top+40;gy<galaxyViewport42.Bottom;gy+=70)ui.Line(new(mapx+1,gy),new(galaxyViewport42.Right-1,gy),new Color(16,29,41));
  ProjectGalaxy42();galaxyDrawNodes42=galaxyPoints42.Count;galaxyDrawEdges42=0;
  foreach(var s in galaxyVisible42){if(!galaxyPoints42.TryGetValue(s.Id,out var a))continue;foreach(int n in s.Neighbours){if(n<=s.Id||!galaxyPoints42.TryGetValue(n,out var b))continue;bool path=galaxyRouteEdges42.Contains((s.Id,n));ui.Line(a,b,path?Gold:new Color(35,67,87),path?2:1);galaxyDrawEdges42++;}}
  foreach(var s in galaxyVisible42){if(!galaxyPoints42.TryGetValue(s.Id,out var p))continue;bool selected=s.Id==Plan42.Selected;ui.Box(new((int)p.X-1,(int)p.Y-1,3,3),GalaxySecurityColor42(s.Security));if(selected){ui.Circle(p,8,White,1);ui.Text(s.Name,Math.Clamp(p.X+12,galaxyViewport42.X+8,galaxyViewport42.Right-170),Math.Clamp(p.Y-20,top+8,galaxyViewport42.Bottom-26),White,12);}else if(s.Id==Plan42.From||s.Id==Plan42.To)ui.Circle(p,6,Gold,1);}
  ui.Text($"{galaxyVisible42.Length:N0} systems / {galaxyDrawNodes42:N0} on screen",mapx+10,galaxyViewport42.Bottom-21,Muted,11);
  RefreshGalaxySearch42();ui.Text($"{galaxyResults42.Length:N0} matches",x,top,Cyan,12);galaxyResultsRect42=new(x,top+24,left,Math.Max(50,r.Height-470));int cap=Math.Max(1,galaxyResultsRect42.Height/27),pages=Math.Max(1,(galaxyResults42.Length+cap-1)/cap);galaxyResultPage42=Math.Clamp(galaxyResultPage42,0,pages-1);int yy=galaxyResultsRect42.Y;
  foreach(var result in galaxyResults42.Skip(galaxyResultPage42*cap).Take(cap)){var e=result;ui.Button(new(x,yy,left-31,25),ShortInfo(e.Name,31),()=>{if(e.Kind=="system")SelectGalaxy42(e.Id);else if(e.Kind=="landmark")ShowInformation("landmark43/"+e.Id);else{galaxyRegion42=e.Kind=="region"?e.Id:Galaxy42.Constellations[e.Id].ParentId;galaxyConstellation42=e.Kind=="constellation"?e.Id:0;galaxyKind42=0;galaxyResultPage42=0;FitGalaxy42();}},"Galaxy result / "+e.Kind+":"+e.Id,e.Kind=="system"&&e.Id==Plan42.Selected);ui.Button(new(x+left-26,yy,26,25),"i",()=>ShowInformation(e.Kind=="landmark"?"landmark43/"+e.Id:"lore/"+e.Kind+":"+e.Id),"Galaxy result info / "+e.Kind+":"+e.Id);yy+=27;}
  yy=galaxyResultsRect42.Bottom+6;ui.Button(new(x,yy,88,25),"Previous",()=>galaxyResultPage42=Math.Max(0,galaxyResultPage42-1),"Galaxy results previous");ui.Text($"{galaxyResultPage42+1}/{pages}",x+101,yy+7,Muted,11);ui.Button(new(x+left-88,yy,88,25),"Next",()=>galaxyResultPage42=Math.Min(pages-1,galaxyResultPage42+1),"Galaxy results next");
  var chosen=Galaxy42.ById[Plan42.Selected];yy+=42;ui.Text(ShortInfo(chosen.Name,30),x,yy,Gold,17);ui.Text($"Security {chosen.Security:0.00} / {chosen.Id}",x,yy+26,GalaxySecurityColor42(chosen.Security),12);ui.Text(ShortInfo(Galaxy42.Regions[chosen.RegionId].Name+" / "+Galaxy42.Constellations[chosen.ConstellationId].Name,41),x,yy+48,Muted,11);ui.Wrap(chosen.Affiliation,x,yy+67,left,Muted,11);ui.Text($"Component {chosen.Component} / {chosen.Neighbours.Length} gate links",x,yy+100,Muted,11);
  ui.Button(new(x,yy+124,84,27),"Show info",()=>ShowInformation("lore/system:"+chosen.Id),"Galaxy selected info");ui.Button(new(x+91,yy+124,84,27),"Region",()=>{galaxyRegion42=chosen.RegionId;galaxyConstellation42=0;galaxyConnection42=galaxySecurity42=0;FitGalaxy42();},"Galaxy focus region");ui.Button(new(x+182,yy+124,88,27),"Locate",()=>{var p=Galaxy42.Project(chosen,galaxyThreeD42,galaxyYaw42,galaxyPitch42);galaxyConnection42=galaxySecurity42=0;galaxyRegion42=chosen.RegionId;galaxyConstellation42=0;FitGalaxy42();galaxyCenterX42=p[0];galaxyCenterY42=p[1];},"Galaxy locate selected");
  ui.Button(new(x,yy+158,132,27),"New exploration",()=>{if(world.Screen!=ViewMode.Title)world.Save();world.NewExploration43(chosen.Id);desktop.Clear();},"Pass43 new imported exploration; separate save");
  ui.Button(new(x+138,yy+158,132,27),"Resume exploration",()=>{if(!world.ImportedWorld43&&world.Screen!=ViewMode.Title)world.Save();if(world.Load(DemoWorld.ImportedSavePath43))desktop.Clear();},"Pass43 resume imported exploration");
  int ry=y;ui.Text("FROM",rx,ry,Cyan,10);ui.Text(ShortInfo(Galaxy42.ById[Plan42.From].Name,34),rx,ry+19,White,13);ui.Button(new(rx,ry+43,right,25),"Use selected as start",()=>{Plan42.From=Plan42.Selected;RefreshGalaxyRoute42();},"Galaxy route start");ry+=81;ui.Text("TO",rx,ry,Cyan,10);ui.Text(Plan42.To==0?"Choose a destination":ShortInfo(Galaxy42.ById[Plan42.To].Name,34),rx,ry+19,White,13);ui.Button(new(rx,ry+43,right,25),"Use selected as destination",()=>{Plan42.To=Plan42.Selected;RefreshGalaxyRoute42();},"Galaxy route destination");ry+=79;
  foreach(var mode in Enum.GetValues<GalaxyRouteMode42>()){var m=mode;ui.Button(new(rx+(int)m*146,ry,139,26),m.ToString(),()=>{Plan42.Mode=m;RefreshGalaxyRoute42();},"Galaxy route mode / "+m,Plan42.Mode==m);}ry+=36;
  bool avoided=Plan42.Avoid.Contains(chosen.Id);ui.Button(new(rx,ry,right,26),avoided?"Remove selected from avoidance":"Avoid selected system",()=>{Plan42.Avoid=avoided?Plan42.Avoid.Where(i=>i!=chosen.Id).ToArray():Plan42.Avoid.Append(chosen.Id).Distinct().Order().ToArray();RefreshGalaxyRoute42();},"Galaxy avoid selected");ry+=32;ui.Button(new(rx,ry,139,25),"Clear avoidance",()=>{Plan42.Avoid=[];RefreshGalaxyRoute42();},"Galaxy clear avoidance");ui.Button(new(rx+146,ry,139,25),"Save plan",()=>world.Save(),"Galaxy save plan");ry+=39;
  if(!galaxyRoute42.Found){ui.Wrap(galaxyRoute42.Reason,rx,ry,right,Gold,12);ry+=85;}else{ui.Text($"{galaxyRoute42.Jumps} jumps / cost {galaxyRoute42.Cost:0}",rx,ry,Gold,14);ry+=26;int pathcap=Math.Max(1,(r.Bottom-ry-117)/27),pathpages=Math.Max(1,(galaxyRoute42.Systems.Length+pathcap-1)/pathcap);galaxyRoutePage42=Math.Clamp(galaxyRoutePage42,0,pathpages-1);foreach(int id in galaxyRoute42.Systems.Skip(galaxyRoutePage42*pathcap).Take(pathcap)){int node=id;var s=Galaxy42.ById[id];ui.Button(new(rx,ry,right,25),ShortInfo(s.Name,30)+$"  {s.Security:0.0}",()=>SelectGalaxy42(node),"Galaxy route step / "+id,id==Plan42.Selected);ry+=27;}ui.Button(new(rx,r.Bottom-110,139,25),"Previous steps",()=>galaxyRoutePage42=Math.Max(0,galaxyRoutePage42-1),"Galaxy route previous");ui.Button(new(rx+146,r.Bottom-110,139,25),$"Next {galaxyRoutePage42+1}/{pathpages}",()=>galaxyRoutePage42=Math.Min(pathpages-1,galaxyRoutePage42+1),"Galaxy route next");}
  ui.Wrap("Safer: each low-security arrival adds 50 cost; null adds another 100. Filters affect display, not routing.",rx,r.Bottom-76,right,Muted,10);
  ui.Text("Drag to pan / wheel to zoom / right-drag to orbit in 3D. Temporary connections are not static gates.",mapx,r.Bottom-43,Muted,10);ui.Text("Original 2019 belts / imported exploration uses a separate save. Dock at imported stations for hangars, fitting, repair and local agents.",x,r.Bottom-22,Cyan,10);
  if(world.ImportedWorld43&&Plan42.To!=0)ui.Button(new(rx,r.Bottom-42,right,24),"Autopilot to destination",()=>{world.SetDestination("system:"+Plan42.To);world.Autopilot=true;desktop.Clear();},"Pass43 fly route");
  RecordGalaxyFrame42(sw.Elapsed.TotalMilliseconds);
 }
 readonly List<double> galaxyFrameTimes42=[];readonly List<double> galaxyProjectionTimes42=[];
 void RecordGalaxyFrame42(double milliseconds){if(!Environment.GetCommandLineArgs().Contains("--galaxy-profile"))return;galaxyFrameTimes42.Add(milliseconds);galaxyProjectionTimes42.Add(galaxyLastProjectionMs42);}
 void WriteGalaxyProfile42()
 {
  string? target=Environment.GetCommandLineArgs().SkipWhile(a=>a!="--galaxy-profile").Skip(1).FirstOrDefault();if(target==null)return;
  var sorted=galaxyFrameTimes42.Skip(3).Order().ToArray();var report=new{Systems=Galaxy42.Data.Systems.Length,Frames=sorted.Length,MapCpuMedianMs=sorted.Length==0?0:sorted[sorted.Length/2],MapCpuP95Ms=sorted.Length==0?0:sorted[(int)((sorted.Length-1)*.95)],VisibleNodes=galaxyDrawNodes42,VisibleEdges=galaxyDrawEdges42,ProjectionMs=galaxyLastProjectionMs42,ProjectionRebuilds=galaxyProjectionRebuilds42,Motion=Environment.GetCommandLineArgs().Contains("--galaxy-profile-motion"),ManagedBytes=GC.GetTotalMemory(false),WorkingSetBytes=Environment.WorkingSet==0?(long?)null:Environment.WorkingSet,Measurement="Actual rendered map CPU submission on SDL offscreen; excludes GPU/display latency. Projection cache rebuilt on camera/filter changes."};File.WriteAllText(target,System.Text.Json.JsonSerializer.Serialize(report,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
 }
}
