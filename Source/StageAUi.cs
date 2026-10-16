using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void ShowDockedWaitStageA(){ShowPopup("Advance simulated time while docked",[("Wait 1 hour",()=>world.WaitDocked(1)),("Wait 1 day",()=>world.WaitDocked(24)),("Wait 7 days",()=>world.WaitDocked(24*7)),("Wait 30 days",()=>world.WaitDocked(24*30))]);}
 void DrawAllStationAssets(DesktopWindow w,int x,int y,int width)
 {
  ui.Text("OWNED STATION ASSETS / physical collection required",x,y,Cyan,12);y+=28;
  var rows=world.StationItems.SelectMany(station=>station.Value.Where(stack=>stack.Value>0).Select(stack=>(Location:station.Key,Item:stack.Key,Quantity:stack.Value))).OrderBy(row=>row.Location).ThenBy(row=>row.Item).ToArray();int visible=Math.Max(1,(w.Bounds.Bottom-y-48)/44);inventoryScroll=Math.Clamp(inventoryScroll,0,Math.Max(0,rows.Length-visible));
  foreach(var row in rows.Skip(inventoryScroll).Take(visible)){string location=row.Location;ui.Text(ShortInfo(world.LocationName(location),48),x,y,Muted,11);ui.Text(ShortInfo(DemoWorld.ItemName(row.Item),42)+" x"+row.Quantity.ToString("N0"),x,y+16,White,12);ui.Button(new(x+width-110,y,106,30),"Set route",()=>world.SetDestination(world.LocationSystem(location)));y+=44;}
  ui.Button(new(x,w.Bounds.Bottom-40,110,27),"Previous",()=>inventoryScroll=Math.Max(0,inventoryScroll-visible));ui.Button(new(x+118,w.Bounds.Bottom-40,110,27),"Next",()=>inventoryScroll+=visible);
 }
}
