using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunFlightDroneUiChecks(int stage){if(stage>6)return;var w=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Drones);void Check(bool value,string label)=>UiCheck(value,label);var u=world.Player.Drones.FirstOrDefault(u=>u.Owner=="player"&&u.Location is "Bay" or "Space");
 switch(stage){
 case 0:Check(w!=null&&HasFlightDrones,"Drone panel opens automatically in space with drones");Check(ui.Hits.Any(h=>h.Tip=="Flight folder / Bay")&&ui.Hits.Any(h=>h.Tip=="Flight folder / Space"),"Bay and space folders expose their own commands");int x=w!.X;TestDrag(new(w.X+80,w.Y+12),new(w.X+60,w.Y-18));Check(w.X==x-20,"Flight drone title drag moves panel");break;
 case 1:int width=w!.Width;TestDrag(new(w.Bounds.Right-5,w.Bounds.Bottom-5),new(w.Bounds.Right+5,w.Bounds.Bottom+15));Check(w.Width==width+10,"Flight drone resize grip changes panel width");SaveDesktop();Check(File.Exists(LayoutPath),"Flight drone geometry is saved");break;
 case 2:var row=ui.Hits.First(h=>h.Tip=="Drone unit / "+u!.Id);var folder=flightFolders.First(f=>f.Location=="Space"&&f.Group.Length==0);TestDrag(row.Rect.Center.ToVector2(),folder.Rect.Center.ToVector2());Check(u!.Location=="Space"&&world.UsedDroneBandwidth==5,"Dragging bay drone to space folder launches actual craft");flightDroneScroll=100;break;
 case 3:var spaceRow=ui.Hits.First(h=>h.Tip=="Drone unit / "+u!.Id);mouse=spaceRow.Rect.Center.ToVector2();FlightDroneMenu();Check(spaceMenu!.Any(c=>c.Label=="Engage target")&&spaceMenu!.Any(c=>c.Label=="Return to drone bay")&&spaceMenu!.Any(c=>c.Label=="Assist"),"Space drone menu exposes engage, return and fleet commands");spaceMenu=null;world.DroneCommand("Return",u!.Id);Check(u.Order=="Return","Return menu orders physical recall");break;
 case 4:Check(JumpShipCommands().Count==0,"Subcapital ship has no jump-drive menu");world.Player.SetCombatHull("blackops");world.JumpBeacons.Add(new(){System="umbra",Kind="CovertCyno",Left=600});var cmds=JumpShipCommands();Check(cmds.Any(c=>c.Label=="Jump to"&&c.Children!.Count>0),"Jump-capable ship exposes compatible live beacon destinations");world.Player.SetCombatHull("cruiser");break;
 case 5:var kept=world.Player.Drones.ToList();world.Player.Drones.Clear();SyncFlightDroneWindow();Check(!desktop.Any(w=>w.Kind==ViewMode.Drones),"Panel disappears when no drones remain");world.Player.Drones.AddRange(kept);SyncFlightDroneWindow();Check(desktop.Any(w=>w.Kind==ViewMode.Drones),"Panel returns when drones become available");break;
 case 6:world.Screen=ViewMode.Station;SyncFlightDroneWindow();Check(!desktop.Any(w=>w.Kind==ViewMode.Drones),"Automatic flight panel hides when docked");world.Screen=ViewMode.Space;SyncFlightDroneWindow();Console.WriteLine("Flight drone and jump UI checks passed.");break;
 }}
}
