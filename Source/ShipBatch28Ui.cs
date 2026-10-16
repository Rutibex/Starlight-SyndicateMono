using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 bool subsystemPanel;string subsystemNotice="";int subsystemCategory,subsystemDetail;
 void DrawSubsystemPanel()
 {
  // Modal surface blocks clicks through to fitting sockets beneath it.
  ui.Box(new(430,188,680,537),new Color(8,18,28,255));ui.Hits.Add(new(new(430,188,680,537),()=>{},"Strategic subsystem panel"));
  ui.Text("STRATEGIC SUBSYSTEMS",448,205,Gold,19);ui.Button(new(1016,201,75,25),"CLOSE",()=>subsystemPanel=false,"Close strategic subsystems");
  var ship=FittingWorld.Player;
  for(int i=0;i<4;i++){int cat=i;ui.Button(new(448+i*161,243,155,29),ShipBatch28.Categories[i],()=>{subsystemCategory=cat;subsystemDetail=0;},"Select subsystem category / "+ShipBatch28.Categories[i],i==subsystemCategory);}
  string category=ShipBatch28.Categories[subsystemCategory];var choices=ShipBatch28.For(ship.CombatHull,category);
  for(int i=0;i<choices.Length;i++){
   var s=choices[i];int index=i,y=288+i*65;bool installed=ship.StrategicSubsystems.GetValueOrDefault(category)==s.Id;string error=FittingWorld.SubsystemFitError(s.Id);
   ui.Button(new(448,y,443,28),(installed?"INSTALLED / ":"")+s.Name,()=>subsystemDetail=index,"Inspect subsystem / "+s.Id,subsystemDetail==i);
   ui.Button(new(905,y,185,28),installed?"INSTALLED":fittingSimulation!=null?"SIMULATE":FittingWorld.ItemCount(s.Id,true)>0?"INSTALL OWNED":"BUY / 900 CR",()=>{if(FittingWorld.FitStrategicSubsystem(s.Id,fittingSimulation!=null)){subsystemNotice="Installed "+s.Name+"; old part returned to local hangar.";fitNotice=subsystemNotice;}else subsystemNotice=error.Length>0?error:"Requires home station, credits or local owned stock.";},"Install subsystem / "+s.Id+(error.Length>0?" / "+error:""),installed);
   ui.Text(ShortInfo(string.Join(" / ",s.Additions.Where(x=>x.Value!=0).Select(x=>x.Key+" "+x.Value.ToString("+0.##;-0.##;0"))),87),450,y+38,Muted,10);
  }
  var selected=choices[Math.Clamp(subsystemDetail,0,2)];int ty=493;
  foreach(var t in selected.Traits.Where(t=>t.Bonus!=0).Take(5)){InfoParagraph((t.Skill==0?"ROLE ":"PER LEVEL ")+t.Bonus.ToString("0.##")+(t.Unit==105?"% ":" ")+t.Text,448,ty,640,2,Cyan);ty+=34;}
  ui.Text("Installed bonuses and I–V training: Ship Info > Traits. Source stats: offline catalogue.",448,668,Muted,10);
  ui.Text(ShortInfo(subsystemNotice.Length>0?subsystemNotice:"Docked changes only. Illegal fits are rejected before payment or inventory changes.",92),448,697,Gold,10);
 }
}
