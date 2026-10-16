using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int droneTab=2,dronePage,droneFamily,droneHullPage,droneUnitPage;string droneType="dr_2203",droneGroupName="Alpha";bool droneGroupEdit;int fighterMoveStage;string fighterMoveId="";Vector3 fighterMovePoint;Vector2 fighterMoveMouse;DroneUnit? droneDrag;bool droneDragging;
 string[] DroneFamilies=>new[]{"All drones / fighters"}.Concat(DroneCatalog.All.Select(d=>d.Family).Distinct()).ToArray();
 void DroneWindow()
 {
  int x=406,y=130;var p=world.Player;ui.Text(p.HullName+" / REMOTE CRAFT",x,y,White,18);
  ui.Text($"Bay {p.DroneBayUsed:0}/{p.Stat("droneCapacity"):0} m3 | In space {world.ActiveDroneCount}/{p.Stat("maxActiveDrones"):0} | Bandwidth {world.UsedDroneBandwidth:0}/{p.Stat("droneBandwidth"):0} Mbit/s",x,y+34,Cyan,12);
  ui.Text($"Fighter bay {p.FighterBayUsed:0}/{p.Stat("fighterCapacity"):0} m3 | Tubes {p.Stat("fighterTubes"):0} | Bay DPS {p.BayDroneDps:0.0} | Active {p.ActiveDroneDps:0.0} | Control {p.Stat("droneControlDistance")/1000:0} km",x,y+55,Muted,12);
  for(int i=0;i<3;i++){int tab=i;ui.Button(new(x+i*263,y+78,255,27),new[]{"DRONE BAY / SPACE","FIGHTER SQUADRONS","CATALOG / LOAD"}[i],()=>{droneTab=tab;droneUnitPage=0;},active:droneTab==i);}
  if(droneTab==2)DroneLibrary(x,y+119);
  else
  {
   var units=p.Drones.Where(u=>u.Owner=="player"&&u.Location is "Bay" or "Space" or "Abandoned" or "Reserve"&&u.Definition.Fighter==(droneTab==1)&&(world.DroneGroupFilter=="All"||u.Group==world.DroneGroupFilter)).OrderBy(u=>u.Definition.Fighter?u.Tube:u.Location=="Bay"?0:1).ToArray();int pages=Math.Max(1,(units.Length+6)/7);droneUnitPage=Math.Clamp(droneUnitPage,0,pages-1);
   ui.Text(droneTab==1?"TUBE / SQUADRON / STATUS":"GROUP / CRAFT / STATUS",x,y+121,Cyan,11);ui.Text("SHIELD   ARMOR   HULL",x+414,y+121,Muted,10);
   int ry=y+143;foreach(var u in units.Skip(droneUnitPage*7).Take(7))
   {
    var unit=u;var d=world.EffectiveDrone(u);string name=d.Fighter?(u.Location=="Reserve"?$"RESERVE [{u.Count}] ":$"T{u.Tube+1} [{u.Count}/{d.SquadronSize}] ")+d.Name:u.Group+" / "+d.Name;
    ui.Button(new(x,ry,398,32),name,()=>{world.DroneSelection=unit.Id;if(unit.Tube>=0)world.SelectedFighterTube=unit.Tube;},"Drone unit / "+u.Id,world.DroneSelection==u.Id);
    float[] health=[u.Shield/Math.Max(1,d.Shield),u.Armor/Math.Max(1,d.Armor),u.Hull/Math.Max(1,d.Hull)];for(int h=0;h<3;h++){ui.Box(new(x+414+h*70,ry+4,61,5),new Color(40,47,53));ui.Box(new(x+414+h*70,ry+4,(int)(61*Math.Clamp(health[h],0,1)),5),h==0?Cyan:h==1?Gold:Green);}ui.Text(u.Location+" / "+u.Order,x+414,ry+19,White,10);
    ui.Button(new(x+628,ry,151,32),u.Location=="Reserve"?"LOAD TUBE":u.Location=="Bay"?world.Docked?"UNLOAD":"LAUNCH":u.Location=="Abandoned"?"SCOOP":"RECALL",()=>{if(unit.Location=="Reserve")world.ReplenishFighter(unit.Id);else if(unit.Location=="Bay"){if(world.Docked)world.UnloadDrone(unit.Id);else world.LaunchDrone(unit.Id);}else if(unit.Location=="Abandoned")world.ScoopDrone(unit.Id);else world.DroneCommand("Return",unit.Id);},"Drone action / "+u.Id);ry+=38;
   }
   int by=y+425;ui.Button(new(x,by,64,25),"Prev",()=>droneUnitPage=Math.Max(0,droneUnitPage-1));ui.Text($"{droneUnitPage+1}/{pages}",x+79,by+7,Muted,11);ui.Button(new(x+138,by,64,25),"Next",()=>droneUnitPage=Math.Min(pages-1,droneUnitPage+1));
   ui.Button(new(x+217,by,203,25),"GROUP: "+world.DroneGroupFilter,()=>{var groups=new[]{"All"}.Concat(p.Drones.Select(u=>u.Group).Distinct()).ToArray();world.DroneGroupFilter=groups[(Array.IndexOf(groups,world.DroneGroupFilter)+1)%groups.Length];});
   ui.Button(new(x+431,by,164,25),p.DroneAggressive?"AGGRESSIVE":"PASSIVE",()=>p.DroneAggressive=!p.DroneAggressive);ui.Button(new(x+605,by,174,25),p.DroneFocusFire?"FOCUS FIRE":"FREE FIRE",()=>p.DroneFocusFire=!p.DroneFocusFire,active:p.DroneFocusFire);
   string[] labels=["ENGAGE SELECTED","RETURN TO BAY","RETURN / ORBIT","MOVE TO POINT","RECONNECT LOST"];Action[] cmds=[()=>world.DroneCommand("Engage",world.DroneSelection),()=>world.DroneCommand("Return"),()=>world.DroneCommand("Orbit"),()=>{if(fighterSelftest)world.DroneCommand("Move",world.DroneSelection);else BeginFighterMove();},world.ReconnectDrones];for(int i=0;i<5;i++)ui.Button(new(x+i*157,by+37,149,26),labels[i],cmds[i],i==0?"Engage selected drone / "+world.DroneSelection:labels[i]);
   var selected=p.Drones.FirstOrDefault(u=>u.Id==world.DroneSelection);
   if(selected?.Definition.Fighter==true){for(int i=0;i<selected.Definition.Abilities.Length;i++){int slot=i;var a=world.EffectiveDrone(selected).Abilities[i];string status=selected.AbilityReloads[i]>0?$" / R {selected.AbilityReloads[i]:0}":selected.AbilityCooldowns[i]>0?$" / CD {selected.AbilityCooldowns[i]:0}":a.Charges>0?$" / {selected.AbilityCharges[i]}":"";ui.Button(new(x+i*203,by+72,195,27),a.Name+status,()=>world.FighterAbility(selected.Id,slot),"Fighter ability "+(i+1)+" / "+selected.Id);}ui.Button(new(x+616,by+72,163,27),"REARM ABILITIES",()=>world.RearmFighter(selected.Id));}
   else {ui.Button(new(x,by+72,190,27),"MINE ONCE",()=>world.DroneCommand("MineOnce",world.DroneSelection));ui.Button(new(x+200,by+72,190,27),"AUTO SALVAGE",()=>world.DroneCommand("Engage",world.DroneSelection));ui.Button(new(x+400,by+72,190,27),"SCOOP SELECTED",()=>world.ScoopDrone(world.DroneSelection));}
   ui.Button(new(x,by+107,197,27),"GROUP NAME: "+droneGroupName,()=>droneGroupEdit=!droneGroupEdit);ui.Button(new(x+205,by+107,173,27),"ASSIGN GROUP",()=>{if(selected!=null&&droneGroupName.Trim().Length>0)selected.Group=droneGroupName.Trim();droneGroupEdit=false;});ui.Button(new(x+387,by+107,194,27),world.WeaponSafety?"SAFETY: ON":"SAFETY: OFF",()=>world.WeaponSafety=!world.WeaponSafety);ui.Button(new(x+598,by+107,181,27),"REPAIR BAY",world.RepairBayDrones);
  }
  ui.Text("N open | Shift+E engage group | Shift+R recall all | Right-click rows for individual commands",x,738,Muted,11);
 }
 void DroneLibrary(int x,int top)
 {
  var families=DroneFamilies;droneFamily=Math.Clamp(droneFamily,0,families.Length-1);ui.Button(new(x,top,32,25),"<",()=>{droneFamily=(droneFamily+families.Length-1)%families.Length;dronePage=0;});ui.Center(families[droneFamily],x+157,top+8,Cyan,10);ui.Button(new(x+284,top,32,25),">",()=>{droneFamily=(droneFamily+1)%families.Length;dronePage=0;});
  var list=DroneCatalog.All.Where(d=>droneFamily==0||d.Family==families[droneFamily]).ToArray();int pages=Math.Max(1,(list.Length+8)/9);dronePage=Math.Clamp(dronePage,0,pages-1);int y=top+35;foreach(var d in list.Skip(dronePage*9).Take(9)){var def=d;ui.Button(new(x,y,316,30),d.Name,()=>droneType=def.Id,"Drone catalog / "+d.Id,droneType==d.Id);ui.Text($"{world.ItemCount(d.Id,world.Docked):0}",x+282,y+19,Gold,8);y+=34;}
  ui.Button(new(x,top+348,64,25),"Prev",()=>dronePage=Math.Max(0,dronePage-1));ui.Text($"{dronePage+1}/{pages} / {list.Length}",x+90,top+354,Muted,11);ui.Button(new(x+247,top+348,69,25),"Next",()=>dronePage=Math.Min(pages-1,dronePage+1));
  var selected=DroneCatalog.ById[droneType];ui.Text(selected.Name,x+338,top+6,Gold,18);ui.Wrap(selected.Description,x+338,top+37,439,White,9);
  ui.Button(new(x+338,top+229,211,28),"LOAD ONE",()=>world.LoadDrone(droneType,world.ItemCount(droneType,true)>0),"Load one drone / selected tube");ui.Button(new(x+558,top+229,221,28),selected.Fighter?"FILL SELECTED TUBE":"LOAD FIVE",()=>{int current=world.Player.Drones.FirstOrDefault(u=>u.Tube==world.SelectedFighterTube&&u.Location=="Bay")?.Count??0;world.LoadDrone(droneType,world.ItemCount(droneType,true)>0,selected.Fighter?Math.Max(1,selected.SquadronSize-current):5);});
  if(selected.Fighter)for(int i=0;i<world.Player.Stat("fighterTubes");i++){int tube=i;ui.Button(new(x+338+i*87,top+268,80,26),"TUBE "+(i+1),()=>world.SelectedFighterTube=tube,active:world.SelectedFighterTube==i);}
  ui.Button(new(x+338,top+309,211,28),"SHOW ITEM INFO",()=>{ShowItemInfo(droneType);});ui.Button(new(x+558,top+309,221,28),"MARKET DETAILS",()=>{catalogSelection=Array.FindIndex(ItemCatalog.All,d=>d.Id==droneType);EnsureWindow(ViewMode.Market);});
  if(selected.Fighter){ui.Button(new(x+338,top+348,211,25),"STOW FIGHTER SPARES",()=>world.StoreFighterReserve(droneType,selected.SquadronSize));ui.Button(new(x+558,top+348,221,25),"REPLENISH TUBE",()=>{var stock=world.Player.Drones.FirstOrDefault(u=>u.TypeId==droneType&&u.Location=="Reserve");if(stock!=null)world.ReplenishFighter(stock.Id);});}
  ui.Text("TRIAL HULLS / stocked remote craft",x,top+391,Gold,11);int hp=Math.Max(1,(CombatHulls.All.Length+3)/4);droneHullPage=Math.Clamp(droneHullPage,0,hp-1);int hx=x;foreach(var h in CombatHulls.All.Skip(droneHullPage*4).Take(4)){var hull=h;ui.Button(new(hx,top+416,145,27),h.Name,()=>world.WeaponTrial(hull.Id),"Drone trial hull / "+h.Id,world.Player.CombatHull==h.Id);hx+=151;}ui.Button(new(x+606,top+416,78,27),"Hulls >",()=>droneHullPage=(droneHullPage+1)%hp);ui.Button(new(x+692,top+416,87,27),"DR RANGE",world.EnterDroneRange,"Launch drone test range");
 }
 void DroneText(char c){if(!droneGroupEdit)return;if(c=='\b'){if(droneGroupName.Length>0)droneGroupName=droneGroupName[..^1];}else if(c=='\r'||c=='\n'){if(!world.Docked){var u=world.Player.Drones.FirstOrDefault(u=>u.Id==world.DroneSelection);if(u!=null&&droneGroupName.Trim().Length>0)AssignFlightGroup(u,droneGroupName.Trim());}droneGroupEdit=false;}else if(!char.IsControl(c)&&droneGroupName.Length<20)droneGroupName+=c;}
 void DroneContextMenu()
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Tip.StartsWith("Drone unit / ")&&h.Rect.Contains(mouse.ToPoint()));if(hit==null)return;var u=world.Player.Drones.First(v=>v.Id==hit.Tip[13..]);world.DroneSelection=u.Id;var actions=new List<(string,Action)>{("Show info",()=>{ShowItemInfo(u.TypeId);})};
  if(u.Location=="Reserve"){actions.Add(("Load selected tube",()=>world.ReplenishFighter(u.Id)));if(world.Docked)actions.Add(("Unload reserve to station",()=>world.UnloadDrone(u.Id)));}
  else if(u.Location=="Bay"){actions.Add((world.Docked?"Unload to station":"Launch",()=>{if(world.Docked)world.UnloadDrone(u.Id);else world.LaunchDrone(u.Id);}));if(!world.Docked&&u.Definition.Fighter)actions.Add(("Unload squadron to reserve",()=>world.UnloadDrone(u.Id)));}
  else {foreach(var cmd in new[]{("Engage selected target","Engage"),("Return to bay","Return"),("Return and orbit","Orbit"),("Assist selected friendly","Assist"),("Guard selected friendly","Guard"),("Abandon control","Abandon")}){var order=cmd.Item2;actions.Add((cmd.Item1,()=>world.DroneCommand(order,u.Id)));}actions.Add(("Scoop nearby",()=>world.ScoopDrone(u.Id)));}ShowPopup(u.Definition.Name,actions);
 }
 bool HandleDroneDrag(MouseState m)
 {
  bool downNow=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released,upNow=m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed;
  if(droneDrag!=null){if(Vector2.Distance(mouse,gestureOrigin)>6)droneDragging=true;if(upNow){if(droneDragging){var top=WindowAt(mouse);if(droneDrag.Location=="Bay"&&top==null)world.LaunchDrone(droneDrag.Id);else if(droneDrag.Location=="Space"&&top?.Kind==ViewMode.Drones)world.DroneCommand("Return",droneDrag.Id);else if(droneDrag.Location=="Bay"&&top?.Kind==ViewMode.Inventory)world.UnloadDrone(droneDrag.Id);}else {world.DroneSelection=droneDrag.Id;if(droneDrag.Tube>=0)world.SelectedFighterTube=droneDrag.Tube;}droneDrag=null;droneDragging=false;}return true;}
  if(downNow&&WindowAt(mouse)?.Kind==ViewMode.Drones){var hit=ui.Hits.LastOrDefault(h=>h.Tip.StartsWith("Drone unit / ")&&h.Rect.Contains(mouse.ToPoint()));if(hit!=null){droneDrag=world.Player.Drones.First(u=>u.Id==hit.Tip[13..]);gestureOrigin=mouse;return true;}}return false;
 }
 void BeginFighterMove(){var u=world.Player.Drones.FirstOrDefault(u=>u.Id==world.DroneSelection&&u.Definition.Fighter&&u.Location=="Space");if(u==null)return;fighterMoveId=u.Id;fighterMoveStage=1;renderer.Tactical=true;var window=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Drones);if(window!=null)window.Minimized=true;world.Say("Fighter movement: click a point on the squadron plane, then click to set altitude. Escape cancels.");}
 void FighterPick(Vector2 point){var u=world.Player.Drones.FirstOrDefault(u=>u.Id==fighterMoveId&&u.Location=="Space");if(u==null){fighterMoveStage=0;return;}if(fighterMoveStage==1){var ray=renderer.PickRay(point);float along=Math.Abs(ray.Direction.Y)>.001f?(u.Position.Y-ray.Position.Y)/ray.Direction.Y:renderer.Distance+6000;along=Math.Clamp(along,100,100000);fighterMovePoint=ray.Position+ray.Direction*along;fighterMoveMouse=point;fighterMoveStage=2;}else if(fighterMoveStage==2){fighterMovePoint.Y+=(fighterMoveMouse.Y-point.Y)*40;world.MoveFighter(u.Id,fighterMovePoint);fighterMoveStage=0;var window=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Drones);if(window!=null)window.Minimized=false;}}
 void FighterMovePreview(){if(fighterMoveStage==0)return;ui.Text(fighterMoveStage==1?"FIGHTER MOVE: select plane position":"FIGHTER MOVE: select altitude / Escape cancels",480,53,Cyan,14);if(fighterMoveStage==2){var p=renderer.Project(fighterMovePoint);ui.Line(new(p.X-10,p.Y),new(p.X+10,p.Y),Cyan,2);ui.Line(new(p.X,p.Y-10),new(p.X,p.Y+10),Cyan,2);}}
 void DroneHud()=>SolarDroneHud();
}
