using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed record DirectionalContact(string Id,string Name,string Class,float Distance,bool IsShip=false);
public sealed partial class DemoWorld
{
 public DirectionalContact[] DirectionalScan(string observer="player",float angle=360,float range=50000)
 {
  if(InHubH)return [];var watcher=observer=="player"?Player:Targets.FirstOrDefault(t=>t.Id==observer)?.Profile;if(watcher==null||!float.IsFinite(angle)||!float.IsFinite(range))return [];
  var origin=observer=="player"?Player.Position:Targets.First(t=>t.Id==observer).Position;var facing=watcher.Facing.LengthSquared()>0?Vector3.Normalize(watcher.Facing):Vector3.UnitZ;float max=(float)watcher.Stat("maxDirectionalScanRange");range=Math.Clamp(range,0,max>0?max:(float)(14.3*SolarCatalog.AU));angle=Math.Clamp(angle,5,360);
  bool Visible(Ship? p,bool cloaked)=>!cloaked&&(p?.Stat("dscanImmune")??0)<=0;
  bool Inside(Vector3 pos){var v=pos-origin;return v.Length()<=range&&(angle>=360||v.LengthSquared()==0||Vector3.Dot(Vector3.Normalize(v),facing)>=MathF.Cos(angle*MathF.PI/360));}
  var list=Targets.Where(t=>t.Alive&&t.Id!=observer&&Inside(t.Position)&&Visible(t.Profile,t.Profile?.Special.Any(s=>s.Active&&s.Definition?.Cloak==true)==true)).Select(t=>new DirectionalContact(t.Id,t.Name,t.Profile?.HullName??t.Kind.ToString(),Vector3.Distance(origin,t.Position),t.Kind==TargetKind.Raider)).ToList();
  if(observer!="player"&&Inside(Player.Position)&&Visible(Player,Cloaked))list.Add(new("player",Player.HullName,CombatHulls.Get(Player.CombatHull).Role,Vector3.Distance(origin,Player.Position),true));
  if(observer=="player"&&IsSolarGrid){
   bool GlobalInside(SpacePosition p){var delta=p-SystemPosition;double distance=delta.Length;return distance<=range&&(angle>=360||distance<1||Vector3.Dot(delta.Unit.Local,facing)>=MathF.Cos(angle*MathF.PI/360));}
   var ids=list.Select(c=>c.Id).ToHashSet();foreach(var celestial in Celestials.Where(c=>c.Kind is CelestialKind.Station or CelestialKind.Stargate or CelestialKind.Wormhole))if(GlobalInside(celestial.Anchor)&&ids.Add(celestial.Id)&&!(celestial.Kind==CelestialKind.Station&&Grid=="station"))list.Add(new(celestial.Id,celestial.Name,celestial.Type,(float)(celestial.Anchor-SystemPosition).Length));
   foreach(var grid in SolarGrids.Where(g=>g.Key.StartsWith(SystemId+":")&&g.Key!=SolarGridKey))foreach(var target in grid.Value.Targets.Where(t=>t.Alive&&t.Kind is TargetKind.Raider or TargetKind.Container or TargetKind.Wreck)){var global=grid.Value.Origin+SpacePosition.From(target.Position);if(GlobalInside(global)&&Visible(target.Profile,target.Profile?.Special.Any(s=>s.Active&&s.Definition?.Cloak==true)==true))list.Add(new(grid.Key+":"+target.Id,target.Name,target.Profile?.HullName??target.Kind.ToString(),(float)(global-SystemPosition).Length,target.Kind==TargetKind.Raider));}
  }
  return list.OrderBy(c=>c.Distance).ThenBy(c=>c.Id,StringComparer.Ordinal).ToArray();
 }
}
public sealed partial class DemoGame
{
 bool directionalScanner;float directionalAngle=360,directionalRange=50000;DirectionalContact[] directionalResults=[];string directionalRangeTextN="50";bool directionalAuN=false;bool directionalFilterN;
 void DirectionalScanWindow()
 {
  DrawWindow(new(423,130,817,620),"Directional scanner / local space");ui.Text("DIRECTIONAL SCAN",443,177,Gold,18);ui.Button(new(1015,168,205,28),"PROBE SCANNER",()=>directionalScanner=false,"Open probe scanner");
  ui.Wrap("A directional scan reports visible contacts in your facing cone. Combat recons are hidden from this scan; they remain visible on grid, targetable and detectable by combat probes. Cloaked ships remain hidden.",443,215,755,White,12);
  foreach(var pair in new float[]{5,15,30,60,90,180,360}.Select((v,i)=>(v,i))){float angle=pair.v;ui.Button(new(443+pair.i*108,288,100,26),angle+" DEG",()=>directionalAngle=angle,"Directional angle / "+angle,directionalAngle==angle);}
  ui.Button(new(443,333,220,28),"RANGE / "+SolarCatalog.Distance(directionalRange),()=>{directionalRange=directionalRange==5000?50000:directionalRange==50000?100000:directionalRange==100000?(float)SolarCatalog.AU:directionalRange<5*SolarCatalog.AU?(float)(5*SolarCatalog.AU):directionalRange<14*SolarCatalog.AU?(float)(14.3*SolarCatalog.AU):5000;directionalRangeTextN=(directionalRange/(directionalAuN?SolarCatalog.AU:1000)).ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);},"Cycle directional range");ui.Button(new(675,333,220,28),"SCAN",()=>directionalResults=world.DirectionalScan("player",directionalAngle,directionalRange),"Run directional scan");ui.Button(new(909,333,311,28),"FACE SELECTED",world.FaceSelected,"Aim directional scan toward selected contact");
  FieldK(new(443,370,145,27),directionalRangeTextN,value=>{directionalRangeTextN=value;if(double.TryParse(value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var v)&&double.IsFinite(v)&&v>=0)directionalRange=(float)Math.Min(14.3*SolarCatalog.AU,v*(directionalAuN?SolarCatalog.AU:1000));},"N / directional range");ui.Button(new(597,370,90,27),directionalAuN?"AU":"KM",()=>{directionalAuN=!directionalAuN;directionalRangeTextN=(directionalRange/(directionalAuN?SolarCatalog.AU:1000)).ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);},"N / directional units");ui.Button(new(700,370,250,27),directionalFilterN?"SHIPS ONLY":"ALL CONTACTS",()=>directionalFilterN=!directionalFilterN,"N / directional filter");
  ui.Text(directionalResults.Length+" contacts / "+directionalAngle+" degrees / "+SolarCatalog.Distance(directionalRange),443,405,Cyan,13);int y=435;foreach(var contact in directionalResults.Where(c=>!directionalFilterN||c.IsShip).Take(8)){ui.Text(contact.Name+" / "+contact.Class,443,y,White,12);ui.Text(SolarCatalog.Distance(contact.Distance),1124,y,Gold,12);y+=28;}
  ui.Wrap("Scan immunity does not prevent a ship lock, weapon fire or warp disruption. It hides a reconnaissance signature from directional scans only.",443,693,755,Muted,12);
 }
}
