using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 SoundEffect? noticeToneI;bool audioFailedI;string lastNoticeI="";readonly Queue<double> drawSamplesI=[];System.Diagnostics.Stopwatch drawWatchI=new();
 void BeginPresentationI(){drawWatchI.Restart();ui.LargeText=world.Experience.Settings.LargeText;ui.HighContrast=world.Experience.Settings.HighContrast;}
 void EndPresentationI()
 {
  drawWatchI.Stop();drawSamplesI.Enqueue(drawWatchI.Elapsed.TotalMilliseconds);while(drawSamplesI.Count>120)drawSamplesI.Dequeue();
  string current=world.Experience.Notifications.LastOrDefault()??"";if(current==lastNoticeI)return;lastNoticeI=current;
  if(current==""||!world.Experience.Settings.Audio||audioFailedI||capture!=null)return;
  try{if(noticeToneI==null){byte[] samples=new byte[4410];for(int n=0;n<samples.Length/2;n++){short v=(short)(Math.Sin(n*Math.Tau*660/22050)*Math.Min(1,n/100d)*Math.Max(0,1-n/2205d)*6000);samples[2*n]=(byte)v;samples[2*n+1]=(byte)(v>>8);}noticeToneI=new SoundEffect(samples,22050,AudioChannels.Mono);}noticeToneI.Play(world.Experience.Settings.Volume,0,0);}catch(Exception){audioFailedI=true;world.Say("Notification audio unavailable; visual notifications remain enabled.");}
 }
 void DrawExperienceOverlayI()
 {
  if(world.NextGoalI is {} goal){ui.Box(new(70,112,500,55),new Color(12,25,32,235));ui.Text("NEXT / "+goal.Name,82,122,Cyan,12);ui.Text("Open Experience for commands and guidance",82,143,White,11);}
  if(world.Experience.Settings.PerformanceOverlay){ui.Box(new(70,174,500,59),new Color(12,25,32,235));ui.Text($"Draw CPU {(drawSamplesI.Count==0?0:drawSamplesI.Average()):0.0} ms | targets {world.Targets.Count} | draw cap {world.Experience.Settings.RenderBudget}",82,185,Cyan,11);ui.Text($"Managed memory {GC.GetTotalMemory(false)/1048576:0} MB | thumbnails {hullThumbnails.Count}/64 | previews {informationPreviews.Count}/8",82,209,White,11);}
  if(world.CorporationG is {} corp&&world.Experience.CorporateMarks.TryGetValue(corp.Id,out string? mark)){var center=new Vector2(430,80);if(mark=="Ring")ui.Circle(center,15,Cyan,2);else if(mark=="Chevron"){ui.Line(center+new Vector2(-14,-8),center+new Vector2(0,8),Cyan,2);ui.Line(center+new Vector2(0,8),center+new Vector2(14,-8),Cyan,2);}else{int points=mark=="Star"?10:4;Vector2 previous=center+new Vector2(0,-15);for(int n=1;n<=points;n++){float angle=-MathF.PI/2+n*MathF.Tau/points;float radius=mark=="Star"&&n%2==1?7:15;var next=center+new Vector2(MathF.Cos(angle),MathF.Sin(angle))*radius;ui.Line(previous,next,Cyan,2);previous=next;}}ui.Text(mark,452,75,Cyan,11);}
 }
}
