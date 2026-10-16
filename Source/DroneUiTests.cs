using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 float droneUiTargetHp;int droneUiChecks;
 void RunDroneUiChecks(int stage)
 {
  if(stage>17)return;void Check(bool ok,string label){UiCheck(ok,label);droneUiChecks++;}void Click(string prefix){var h=ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(h.Rect.Center.ToVector2());}
  var window=desktop.First(w=>w.Kind==ViewMode.Drones);var u=world.Player.Drones.FirstOrDefault(u=>u.Owner=="player"&&u.TypeId=="dr_2203"&&u.Location is "Bay" or "Space" or "Abandoned");
  switch(stage)
  {
   case 0:Check(droneTab==2&&world.Docked&&DroneCatalog.All.Length==104,"Drone catalog opens with all 104 craft");int ox=window.X;TestDrag(new(window.X+80,window.Y+12),new(window.X+100,window.Y+22));Check(window.X==ox+20,"Drone window title drag moves window");break;
   case 1:int ow=window.Width;TestDrag(new(window.Bounds.Right-5,window.Bounds.Bottom-5),new(window.Bounds.Right+15,window.Bounds.Bottom+5));Check(window.Width==ow+20,"Drone window resize scales controls");break;
   case 2:Click("Drone catalog / dr_2203");break;
   case 3:Check(droneType=="dr_2203","Catalog row selects craft stats");Click("Load one drone /");break;
   case 4:Check(u?.Location=="Bay"&&world.ItemCount("dr_2203",true)==9,"Load consumes real hangar inventory");Click("Launch drone test range");break;
   case 5:Check(world.Grid=="dronerange"&&world.Targets.Any(t=>t.Kind==TargetKind.Wreck)&&world.Targets.Any(t=>t.Resource=="ice"),"Range has salvage, ice and combat targets");Click("DRONE BAY / SPACE");break;
   case 6:var row=ui.Hits.Last(h=>h.Tip=="Drone unit / "+u!.Id);TestDrag(row.Rect.Center.ToVector2(),new(395,350));break;
   case 7:Check(u!.Location=="Space"&&world.UsedDroneBandwidth==5,"Dragging bay row into space launches physical craft");Click("Drone unit / "+u.Id);break;
   case 8:var t=world.Targets.First(t=>t.Id=="range_0");world.SelectedId=world.LockedId=t.Id;world.Targets.First(t=>t.Id==u!.Id).Position=t.Position+new Vector3(1000,0,0);droneUiTargetHp=t.Shield+t.Armor+t.Hull;Click("Engage selected drone /");break;
   case 9:var victim=world.Targets.First(t=>t.Id=="range_0");Check(u!.Order=="Attack"&&victim.Shield+victim.Armor+victim.Hull<droneUiTargetHp,"Engage control applies actual damage");Click("GROUP NAME:");break;
   case 10:droneGroupName="";foreach(char c in "TEST")DroneText(c);Click("ASSIGN GROUP");break;
   case 11:Check(u!.Group=="TEST","Group editor assigns named flight");var b=world.Targets.First(t=>t.Id==u.Id);b.Position=world.Player.Position+new Vector3(200,0,0);b.Armor=u.Definition.Armor*.5f;Click("RETURN TO BAY");break;
   case 12:Check(u!.Location=="Bay"&&u.Armor==u.Definition.Armor*.5f,"Recall lands damaged craft without repair");Click("Drone action / "+u.Id);break;
   case 13:Check(u!.Location=="Space","Re-launch restores craft body");var hit=ui.Hits.Last(h=>h.Tip=="Drone unit / "+u.Id);TestPointer(hit.Rect.Center.ToVector2(),right:ButtonState.Pressed);TestPointer(hit.Rect.Center.ToVector2());Check(context&&popupActions.Any(a=>a.Label=="Abandon control"),"Right click exposes drone commands");break;
   case 14:Click("Abandon control");break;
   case 15:Check(u!.Location=="Abandoned"&&world.UsedDroneBandwidth==0,"Abandon releases bandwidth");Click("RECONNECT LOST");break;
   case 16:Check(u!.Location=="Space"&&world.UsedDroneBandwidth==5,"Reconnect control restores craft");break;
   case 17:Console.WriteLine($"{droneUiChecks} drone graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
 int fighterUiChecks;
 void RunFighterUiChecks(int stage)
 {
  if(stage>15)return;void Check(bool ok,string label){UiCheck(ok,label);fighterUiChecks++;}void Click(string prefix){var h=ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(h.Rect.Center.ToVector2());}var u=world.Player.Drones.First(u=>u.Owner=="player"&&u.Definition.Fighter);
  switch(stage)
  {
   case 0:Check(droneTab==1&&world.Player.Stat("fighterTubes")==3&&u.Count==u.Definition.SquadronSize,"Fighter tab shows carrier tube and squadron size");world.Update(2.2f);Click("Fighter ability 2 /");break;
   case 1:Check(u.BoostLeft>0&&u.AbilityCooldowns[1]>0,"Movement ability starts boost and cooldown");Click("Fighter ability 3 /");break;
   case 2:Check(u.AbilityCharges[2]==u.Definition.Abilities[2].Charges-1&&world.MissilesInFlight.Any(m=>m.Owner==u.Id),"Salvo consumes charge and creates ordnance");Click("REARM ABILITIES");break;
   case 3:Check(u.AbilityReloads.Max()>0,"Rearm control starts ability reload");world.Player.ManualPoint=world.Targets.First(t=>t.Id==u.Id).Position+new Vector3(3000,0,1000);Click("MOVE TO POINT");break;
   case 4:Check(u.Order=="Move"&&u.MovePoint==world.Player.ManualPoint,"Tactical move control sets point");Click("RETURN TO BAY");break;
   case 5:Check(u.Order=="Return"||u.Location=="Bay","Recall starts fighter return");world.Targets.First(t=>t.Id==u.Id).Position=world.Player.Position+new Vector3(200,0,0);world.Update(.1f);break;
   case 6:Check(u.Location=="Bay"&&u.Count==u.Definition.SquadronSize,"Squadron lands in launch tube");world.Dock();break;
   case 7:Click("Drone action / "+u.Id);break;
   case 8:Check(u.Location=="Hangar"&&world.ItemCount(u.TypeId,true)>=u.Count,"Tube unload returns survivors to inventory");Click("CATALOG / LOAD");break;
   case 9:droneType=u.TypeId;Click("FILL SELECTED TUBE");break;
   case 10:Check(world.Player.Drones.Any(x=>x.Location=="Bay"&&x.Definition.Fighter&&x.Count==u.Definition.SquadronSize),"Tube fill provisions ready squadron");Click("STOW FIGHTER SPARES");break;
   case 11:Check(world.Player.Drones.Any(x=>x.Location=="Reserve"&&x.Count==u.Definition.SquadronSize),"Spare control stores actual fighter stock");world.EnterDroneRange();world.SelectedFighterTube=1;Click("REPLENISH TUBE");break;
   case 12:Check(world.Player.Drones.Any(x=>x.Location=="Bay"&&x.Tube==1&&x.Count==u.Definition.SquadronSize)&&!world.Player.Drones.Any(x=>x.Location=="Reserve"),"Replenish control loads tube from reserves in space");Click("FIGHTER SQUADRONS");break;
   case 13:var moved=world.Player.Drones.First(x=>x.Location=="Bay"&&x.Tube==1);world.LaunchDrone(moved.Id);world.DroneSelection=moved.Id;BeginFighterMove();FighterPick(new(800,430));Check(fighterMoveStage==2,"Tactical movement picker fixes the plane position");break;
   case 14:string previousOrder=world.Player.Order;float baseY=fighterMovePoint.Y;FighterPick(new(800,400));var moving=world.Player.Drones.First(x=>x.Id==world.DroneSelection);Check(fighterMoveStage==0&&moving.Order=="Move"&&Math.Abs(moving.MovePoint.Y-baseY-1200)<.01&&world.Player.Order==previousOrder,"Altitude picker moves the squadron without changing ship course");break;
   case 15:Console.WriteLine($"{fighterUiChecks} fighter graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
}
