using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int supportPage,supportFamily,supportTab,supportPayloadPage;string supportType="";
 string[] SupportFamilies=>new[]{"All logistics / fleet"}.Concat(SupportCatalog.All.Select(d=>d.Family).Distinct()).ToArray();
 void SupportWindow()
 {
  var ship=world.Player;int index=world.FittingSupportSlot;var s=ship.Support[index];var chosen=SupportCatalog.ById.GetValueOrDefault(supportType)??SupportCatalog.All[0];
  ui.Text(ship.HullName+" / LOGISTICS & FLEET SUPPORT",237,126,White,19);
  ui.Button(new(1005,123,216,28),"SHIP FITTING",()=>Open(ViewMode.Fitting));
  ui.Text($"CPU {ship.Stat("cpuLoad"):0}/{ship.Stat("cpuOutput"):0} | GRID {ship.Stat("powerLoad"):0}/{ship.Stat("powerOutput"):0} | CAP {ship.Cap:0}/{ship.MaxCap:0} GJ | BURSTS {world.BurstLimit} allowed",237,158,Cyan,12);
  ui.Button(new(237,183,206,27),"MODULES / PAYLOADS",()=>supportTab=0,active:supportTab==0);ui.Button(new(451,183,206,27),"FLEET / BROADCASTS",()=>supportTab=1,active:supportTab==1);ui.Button(new(665,183,206,27),"ACTIVE BONUSES",()=>supportTab=2,active:supportTab==2);
  if(supportTab==0)
  {
   for(int i=0;i<8;i++){int slot=i;var state=ship.Support[i];string label=(i<6?"HIGH "+(i+1):"MID "+(i-5))+" / "+(state.Definition?.Name??"empty");if(label.Length>29)label=label[..26]+"…";ui.Button(new(237+i%4*247,221+i/4*33,240,28),label,()=>{world.FittingSupportSlot=slot;supportPayloadPage=0;},"Support socket / "+i,active:index==i);}
   var families=SupportFamilies;supportFamily=Math.Clamp(supportFamily,0,families.Length-1);ui.Box(new(234,291,312,362),new Color(13,25,31));
   ui.Button(new(242,298,31,25),"<",()=>{supportFamily=(supportFamily+families.Length-1)%families.Length;supportPage=0;});ui.Center(families[supportFamily],391,305,Cyan,10);ui.Button(new(504,298,31,25),">",()=>{supportFamily=(supportFamily+1)%families.Length;supportPage=0;});
   var all=SupportCatalog.All.Where(d=>supportFamily==0||d.Family==families[supportFamily]).ToArray();int pages=Math.Max(1,(all.Length+8)/9);supportPage=Math.Clamp(supportPage,0,pages-1);int y=334;
   foreach(var d in all.Skip(supportPage*9).Take(9)){var item=d;string name=d.Name;if(name.Length>29)name=name[..26]+"…";ui.Button(new(242,y,293,28),name,()=>supportType=item.Id,"Support catalog / "+d.Id,active:chosen.Id==d.Id);y+=31;}
   ui.Button(new(242,620,65,25),"Prev",()=>supportPage=Math.Max(0,supportPage-1));ui.Text($"{supportPage+1}/{pages} / {all.Length} models",325,627,Muted,11);ui.Button(new(469,620,66,25),"Next",()=>supportPage=Math.Min(pages-1,supportPage+1));
   ui.Text(chosen.Name,562,299,Gold,18);ui.Wrap(chosen.Description,562,329,657,White,11);
   ui.Text("INSTALLED: "+(s.Definition?.Name??"empty"),562,429,Cyan,13);
   if(s.Definition is {} current)ui.Text($"Hull range x{world.LogisticsRangeFactor(current):0.#} | Activation cap x{world.LogisticsCapFactor(current):0.##}",893,479,Muted,10);
   ui.Text($"Cycle {s.CycleLeft:0.0}s | Reload {s.ReloadLeft:0.0}s | Ramp +{s.Ramp:P0} | Heat {s.Damage:P0}"+(s.Burnt?" / BURNT":""),562,452,Gold,12);
   ui.Text("Target: "+(world.Targets.FirstOrDefault(t=>t.Id==s.TargetId)?.Name??"none")+$" | Pending repair {s.PendingAmount:0.#}",562,474,White,11);
   if(s.Definition is {} fitted)
   {
    var payloads=SupportPayloads.For(fitted);int pp=Math.Max(1,(payloads.Length+3)/4);supportPayloadPage=Math.Clamp(supportPayloadPage,0,pp-1);int py=502;
    foreach(var payload in payloads.Skip(supportPayloadPage*4).Take(4)){var p=payload;ui.Button(new(562,py,319,25),p.Name+" / "+world.ItemCount(p.Id,world.Docked).ToString("0"),()=>world.LoadSupportPayload(index,p.Id),"Support payload / "+p.Id,active:s.PayloadId==p.Id);py+=29;}
    if(pp>1){ui.Button(new(893,502,150,25),"Payload <",()=>supportPayloadPage=Math.Max(0,supportPayloadPage-1));ui.Button(new(1051,502,170,25),"Payload >",()=>supportPayloadPage=Math.Min(pp-1,supportPayloadPage+1));}
    ui.Button(new(893,535,156,25),s.Active?"STOP AFTER CYCLE":"ACTIVATE / PULSE",()=>world.ToggleSupport(index),"Support activate / "+index,active:s.Active);
    ui.Button(new(1057,535,164,25),"RELOAD",()=>world.ReloadSupport(index));ui.Button(new(893,566,156,25),s.Repeat?"REPEAT: ON":"REPEAT: OFF",()=>s.Repeat=!s.Repeat,active:s.Repeat);ui.Button(new(1057,566,164,25),s.AutoReload?"AUTO RELOAD: ON":"AUTO RELOAD: OFF",()=>s.AutoReload=!s.AutoReload,active:s.AutoReload);
    ui.Button(new(893,597,156,25),s.Overheat?"HEAT: ON":"HEAT: OFF",()=>{if(!s.Burnt&&fitted.HeatDamage>0)s.Overheat=!s.Overheat;},active:s.Overheat);ui.Button(new(1057,597,164,25),"STOW SPARES",()=>world.ProvisionSupportCargo(index));
    ui.Text("Loaded "+s.Loaded+" / "+(s.Payload?.Name??"dry / unscripted"),562,633,Cyan,11);
   }
   ui.Button(new(562,664,150,27),"FIT SELECTED",()=>world.FitSupport(chosen.Id,world.ItemCount(chosen.Id,true)>0,index),"Fit selected support / "+chosen.Id+" / "+world.SupportFitError(chosen,index));ui.Button(new(720,664,150,27),"UNFIT MODULE",()=>world.UnfitSupport(index));ui.Button(new(878,664,150,27),"REPAIR MODULE",()=>world.RepairSupport(index));
   ui.Button(new(1036,664,86,27),"INFO",()=>{ShowItemInfo(chosen.Id);});ui.Button(new(1130,664,91,27),"MARKET",()=>{catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id==chosen.Id);Open(ViewMode.Market);});
  }
  else if(supportTab==1)SupportFleetPane();else SupportEffectsPane();
  ui.Text(world.Docked?"DEMO LOAN HULLS / modules and payload stock supplied":"Targeted support requires a lock | Command pulses affect fleet ships in radius; no drones",237,706,Gold,11);
  if(world.Docked){var hulls=CombatHulls.All.Where(h=>h.Role is "Logistics" or "AdaptiveLogistics" or "Command" or "ExpeditionCommand" or "CapitalIndustrial").ToArray();for(int i=0;i<hulls.Length;i++){var h=hulls[i];ui.Button(new(237+i*166,728,159,27),h.Name,()=>world.SupportTrial(h.Id),h.Role,ship.CombatHull==h.Id);}ui.Button(new(1075,728,146,27),"FLEET RANGE",world.EnterSupportRange);}
  else {ui.Button(new(237,728,183,27),"LOCK SELECTED",world.LockTarget);ui.Button(new(428,728,207,27),"PRESSURE SELECTED",()=>world.SupportPressure(world.SelectedId));ui.Button(new(643,728,150,27),"DOCK",world.Dock);ui.Text($"Pulse docking interlock {world.SupportDockLock:0}s",814,737,Muted,11);}
  ui.Text("Alt+C: Logistics | Movable / resizable | Right-click sockets | Drag module or payload into selected socket",237,769,Muted,11);
 }
 void SupportFleetPane()
 {
  ui.Text("FLIGHT CONTRACTS / 4 WINGMATES MAXIMUM",237,231,Gold,17);int y=267;
  foreach(var w in world.Wingmates)
  {
   var t=world.Targets.FirstOrDefault(t=>t.Id==w.Id);var p=t?.Profile;bool selected=world.FleetSelection==w.Id;var mate=w;
   ui.Button(new(237,y,270,29),w.Name+(w.Hull<=0?" / LOST":""),()=>{world.FleetSelection=mate.Id;if(t!=null)world.Select(mate.Id);},"Fleet member / "+w.Id,active:selected);
   ui.Text($"SH {t?.Shield??w.Shield:0} | AR {t?.Armor??w.Armor:0} | HL {t?.Hull??w.Hull:0} | CAP {t?.Cap??w.Cap:0}",525,y+4,White,12);ui.Text(w.Order+" | "+(w.Fleet?"FLEET":"DETACHED")+" | "+w.Broadcast,525,y+24,w.Broadcast.Length>0?Gold:Cyan,11);y+=58;
  }
  var selectedMate=world.Wingmates.FirstOrDefault(w=>w.Id==world.FleetSelection);
  ui.Text("SELECTED: "+(selectedMate?.Name??"none"),237,511,Cyan,14);
  if(selectedMate!=null)
  {
   var w=selectedMate;string[] orders=["Follow","Hold","Attack","Mine"];for(int i=0;i<4;i++){string order=orders[i];ui.Button(new(237+i*153,541,145,29),order.ToUpper(),()=>world.OrderWingmate(w.Id,order),"Fleet order / "+order,active:w.Order==order);}
   ui.Button(new(865,541,178,29),w.Fleet?"LEAVE FLEET":"JOIN FLEET",()=>w.Fleet=!w.Fleet);ui.Button(new(1051,541,170,29),"LOCK ALLY",()=>{world.Select(w.Id);world.LockTarget();});
   ui.Button(new(237,580,199,29),"REPAIR / REPLACE",()=>world.RebuildWingmate(w.Id));ui.Button(new(444,580,199,29),"DISMISS CONTRACT",()=>world.DismissWingmate(w.Id));
   ui.Button(new(660,580,247,29),"LOAD MINING PRISM",()=>world.LoadMiningFocus(world.ItemCount("mining_focus",true)>0));ui.Text(world.Player.MiningFocusLoaded?$"Prism wear {world.Player.MiningFocusWear:P0}":"No mining prism loaded",925,589,Muted,11);
   ui.Text("Broadcast: "+(w.Broadcast.Length>0?w.Broadcast:"No assistance requested"),237,626,Gold,12);
  }
  string[] roles=["Escort","Medic","Miner"];for(int i=0;i<3;i++){string role=roles[i];ui.Button(new(237+i*252,668,244,28),"HIRE "+role.ToUpper()+(world.WeaponTestMode?" / DEMO":" / 350 CR"),()=>world.HireWingmate(role),"Hire wingmate / "+role);}
  ui.Wrap("Escorts use linked turret tracking and range. Medics repair your ship. Miners deliver finite asteroid yield to your hold. Contracts persist between grids and systems; lost ships require replacement at port. Fleet membership controls command pulse eligibility.",237,459,980,Muted,11);
 }
 void SupportEffectsPane()
 {
  var ally=world.Targets.FirstOrDefault(t=>t.Id==world.FleetSelection);Ship profile=ally?.Profile??world.Player;
  ui.Text("BONUS RECIPIENT: "+(ally?.Name??world.Player.HullName),237,235,Gold,17);ui.Button(new(1010,231,211,28),"PLAYER / ALLY",()=>world.FleetSelection=world.FleetSelection.Length>0?"":world.Wingmates.FirstOrDefault()?.Id??"");
  ui.Text("Same charge: strongest bonus. Distinct bonuses coexist. Targeted links stack with diminishing returns.",237,272,Muted,12);int y=310;
  foreach(var e in profile.SupportEffects.Where(e=>e.Left>0).Take(10))
  {ui.Text(e.Name+" / "+e.Left.ToString("0.0")+"s",237,y,e.Burst?Gold:Cyan,13);ui.Text(string.Join(" | ",e.Bonuses.Select(v=>v.Key=="miningSurvey"?$"Survey tier {v.Value:0}":$"{v.Key} {v.Value:+0.0%;-0.0%;0%}")),237,y+21,White,11);y+=35;}
  if(y==310)ui.Text("No active support effects. Activate a loaded pulse or a targeted support link.",237,y,Muted,13);
  ui.Text($"Lock {profile.Stat("maxTargetRange")/1000:0.#} km | Scan {profile.Stat("scanResolution"):0} | Probe {profile.Stat("probeStrength"):0.#} | Virus {profile.Stat("virusCoherence"):0}",237,689,Cyan,12);
 }
 void SupportHud()
 {
  if(world.Screen!=ViewMode.Space)return;ui.Button(new(76,464,320,25),"LOGISTICS / FLEET [Alt+C]",()=>Open(ViewMode.Support));
  int n=0;foreach(var pair in world.Player.Support.Select((s,i)=>(s,i)).Where(x=>x.s.Definition!=null).Take(8))
  {var s=pair.s;int slot=pair.i;string name=s.Definition!.Name;if(name.Length>12)name=name[..11]+"…";ui.Button(new(420+n%4*120,653+n/4*29,114,25),name+(s.Burnt?" X":s.ReloadLeft>0?" R":s.Active?" ON":""),()=>world.ToggleSupport(slot),s.Definition.Description,active:s.Active);n++;}
 }
 void SupportMenu()
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Support.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Support socket / "));if(hit!=null&&int.TryParse(hit.Tip[17..],out int index))world.FittingSupportSlot=index;
  int slot=world.FittingSupportSlot;var s=world.Player.Support[slot];var actions=new List<(string,Action)>{("Activate / stop after cycle",()=>world.ToggleSupport(slot)),("Reload support magazine",()=>world.ReloadSupport(slot)),("Unload payload / script",()=>world.LoadSupportPayload(slot,"")),("Toggle repeat",()=>s.Repeat=!s.Repeat),("Toggle automatic reload",()=>s.AutoReload=!s.AutoReload),("Toggle overload",()=>{if(s.Definition?.HeatDamage>0)s.Overheat=!s.Overheat;})};
  if(world.Docked){actions.Add(("Unfit module",()=>world.UnfitSupport(slot)));actions.Add(("Repair module",()=>world.RepairSupport(slot)));actions.Add(("Repair stored support modules",()=>world.RepairStoredSupport()));}if(s.Definition!=null)actions.Add(("Show Info",()=>ShowItemInfo(s.ModuleId)));ShowPopup(s.Definition?.Name??"Support socket",actions);
 }
}
