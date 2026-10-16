using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int mobilityPage,mobilityTab;string mobilityType="";
 void MobilityWindow()
 {
  var p=world.Player;int slot=world.FittingMobilitySlot;var s=p.Mobility[slot];var d=MobilityCatalog.ById.GetValueOrDefault(mobilityType)??MobilityCatalog.All[0];
  ui.Text(p.HullName+" / PROPULSION, FITTING & CARGO",237,126,White,18);
  ui.Text($"CPU {p.Stat("cpuLoad"):0}/{p.Stat("cpuOutput"):0} | GRID {p.Stat("powerLoad"):0}/{p.Stat("powerOutput"):0} | CAP {p.Cap:0}/{p.MaxCap:0} GJ | HOLD {world.CargoUsed:0.#}/{p.CargoMax:0.#} m3",237,158,Cyan,12);
  ui.Button(new(1008,123,213,28),"SHIP FITTING",()=>Open(ViewMode.Fitting));
  ui.Button(new(237,183,310,27),"MODULES / FITTING",()=>mobilityTab=0,active:mobilityTab==0);ui.Button(new(555,183,310,27),"LIVE FLIGHT STATS",()=>mobilityTab=1,active:mobilityTab==1);if(world.Docked&&p.Stat("canJump")>0)ui.Button(new(873,183,348,27),"CAPITAL BEACON TRANSIT",()=>mobilityTab=2,active:mobilityTab==2);else if(mobilityTab==2)mobilityTab=0;
  if(mobilityTab==0)
  {
   for(int i=0;i<12;i++){int n=i;var m=p.Mobility[i];string text=(i<6?"MID "+(i+1):"LOW "+(i-5))+" / "+(m.Definition?.Name??"empty");if(text.Length>21)text=text[..19]+"…";ui.Button(new(237+i%6*165,221+i/6*34,157,29),text,()=>world.FittingMobilitySlot=n,"Mobility socket / "+i,active:slot==i);}
   ui.Box(new(234,297,312,364),new Color(13,25,31));int y=307;foreach(var model in MobilityCatalog.All.Skip(mobilityPage*10).Take(10)){var selected=model;string text=model.Name;if(text.Length>29)text=text[..26]+"…";ui.Button(new(242,y,293,28),text,()=>mobilityType=selected.Id,"Mobility catalog / "+model.Id,active:d.Id==model.Id);y+=31;}
   ui.Button(new(242,627,65,25),"Prev",()=>mobilityPage=Math.Max(0,mobilityPage-1));ui.Text($"{mobilityPage+1}/3 / 23 profiles",320,634,Muted,11);ui.Button(new(469,627,66,25),"Next",()=>mobilityPage=Math.Min(2,mobilityPage+1));
   ui.Text(d.Name,562,302,Gold,18);ui.Wrap(d.Description,562,333,651,White,12);
   string error=world.MobilityFitError(d,slot);ui.Wrap(error.Length==0?"Compatible / stock in station: "+world.ItemCount(d.Id,true).ToString("0"):error,562,427,645,error.Length==0?Green:Red,12);
   ui.Text("INSTALLED: "+(s.Definition?.Name??"empty"),562,478,Cyan,13);ui.Text($"{(s.Burnt?"BURNT":s.Online?"ONLINE":"OFFLINE")} / {(s.Active?"ACTIVE":"IDLE")} | Cycle {s.CycleLeft:0.0}s | Rearm {s.Cooldown:0.0}s | Damage {s.Damage:P0}",562,503,Gold,12);
   ui.Button(new(562,538,210,27),s.Active?"STOP AFTER CYCLE":"ACTIVATE",()=>world.ToggleMobility(slot),"Mobility activate / "+slot,active:s.Active);ui.Button(new(780,538,210,27),s.Online?"OFFLINE":"ONLINE",()=>world.SetMobilityOnline(slot,!s.Online),"Mobility online / "+slot,active:s.Online);ui.Button(new(998,538,223,27),"FACE SELECTED",world.FaceSelected);
   ui.Button(new(562,575,210,27),s.Repeat?"REPEAT: ON":"REPEAT: OFF",()=>s.Repeat=!s.Repeat,active:s.Repeat);ui.Button(new(780,575,210,27),s.Overheat?"HEAT: ON":"HEAT: OFF",()=>{if(s.Definition?.HeatDamage>0&&!s.Burnt)s.Overheat=!s.Overheat;},active:s.Overheat);ui.Button(new(998,575,223,27),"REPAIR STORED",()=>world.RepairStoredMobility());
   ui.Wrap("Vector drives commit to their captured heading. Scrambling interrupts the spool and disables slipstream drives. Ordinary warp disruption does not. Thermal drives remain usable under scramble.",562,614,650,Muted,12);
   ui.Button(new(562,664,150,27),"FIT SELECTED",()=>world.FitMobility(d.Id,world.ItemCount(d.Id,true)>0,slot),"Fit selected mobility / "+d.Id);ui.Button(new(720,664,150,27),"UNFIT MODULE",()=>world.UnfitMobility(slot));ui.Button(new(878,664,150,27),"REPAIR MODULE",()=>world.RepairMobility(slot));
   ui.Button(new(1036,664,86,27),"INFO",()=>{ShowItemInfo(d.Id);});ui.Button(new(1130,664,91,27),"MARKET",()=>{catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id==d.Id);Open(ViewMode.Market);});
  }
  else if(mobilityTab==1)
  {
   ui.Text("LIVE HULL / fitted effects and thrust-to-mass calculation",237,237,Gold,17);int y=278;
   foreach(var line in new[]{$"Current speed {p.Velocity.Length():0.#} / maximum {p.Speed:0.#} m/s / web factor {world.EwarFor("player").Speed:0.###}",$"Mass {p.Stat("mass"):N0} kg / inertia {p.Stat("agility"):0.###} / align {p.AlignTime:0.##} s",$"Signature {p.Stat("signatureRadius"):0.#} m / hull {p.Hull:0}/{p.MaxHull:0}",$"Cargo {world.CargoUsed:0.#}/{p.CargoMax:0.#} m3 / warp rate {world.WarpRate:0.###} AU/s",$"Shield {p.Shield:0}/{p.MaxShield:0} / peak passive recharge {p.PeakShieldRecharge:0.##} HP/s",$"Capacitor {p.Cap:0}/{p.MaxCap:0} / peak recharge {p.PeakCapRecharge:0.##} GJ/s / active use {p.CapacitorUse:0.##} GJ/s",$"Heading {p.Facing.X:0.##}, {p.Facing.Y:0.##}, {p.Facing.Z:0.##} / field passengers {world.LastMobilityPassengers}",$"Warp travel clock {world.WarpTravelHours:0.##} hours / presentation {world.WarpPresentationSeconds:0.##} s"}){ui.Text(line,237,y,White,14);y+=38;}
   ui.Wrap("Oversized drives must meet real CPU/grid budgets. Added drive mass reduces thrust efficiency. Inertia and speed bonuses diminish when stacked. Cargo frames trade hull and speed for hold space; injection manifolds trade hold space for speed. Removing or offlining needed fitting/cargo support is refused.",237,620,977,Muted,13);
  }
  else
  {
   ui.Text("PROTOTYPE BEACON NETWORK / CAPITAL TRANSIT",237,237,Gold,17);ui.Text($"Fuel in hold: {world.ItemCount("jump_fuel",false):0} / consumption {p.Stat("jumpDriveConsumptionAmount"):0.##} per LY / reach {p.Stat("jumpDriveRange"):0.#} LY / cooldown {world.JumpDriveCooldown:0}s",237,276,Cyan,13);int y=318;
   foreach(var system in world.BeaconDestinations()){string id=system.Id;string error=world.JumpDriveError(id);ui.Button(new(237,y,426,32),system.Name+$" / {DemoWorld.BeaconDistance(world.SystemId,id):0.##} LY / {world.JumpFuelRequired(id)} fuel",()=>world.JumpToBeacon(id),"Beacon transit / "+id);ui.Text(error.Length==0?"READY":error,679,y+9,error.Length==0?Green:Muted,12);y+=40;}
   ui.Button(new(237,632,260,30),"LOAD TRANSIT FUEL",world.ProvisionTransitFuel);ui.Wrap("Trial beacon routes make fuel efficiency playable. This network uses original cluster distances; cynosural beacons, fatigue and fuel species are not yet simulated. Fuel reclaimers are restricted to the Worldforger industrial capital.",237,670,977,Muted,11);
  }
  ui.Text("Alt+P: Propulsion | Shared hull sockets | Drag inventory to selected socket | Right-click for controls",237,706,Muted,11);
  if(world.Docked){string[] hulls=["command_destroyer","battlecruiser","battleship","carrier","industrial_cap"];for(int i=0;i<hulls.Length;i++){string id=hulls[i];ui.Button(new(237+i*166,728,159,27),CombatHulls.Get(id).Name,()=>world.MobilityTrial(id),"Mobility loan hull / "+id,p.CombatHull==id);}ui.Button(new(1075,728,146,27),"WEAPON RANGE",world.EnterWeaponRange);}
  else{ui.Button(new(237,728,220,27),"FACE SELECTED",world.FaceSelected);ui.Button(new(465,728,220,27),"SHIP INFO",()=>Open(ViewMode.ShipInfo));ui.Button(new(693,728,220,27),"DOCK",world.Dock);ui.Button(new(921,728,300,27),"INVENTORY",()=>Open(ViewMode.Inventory));}
  ui.Text("Loan hull service supplies all profiles; campaign purchases use the regional market.",237,769,Muted,11);
 }
 void MobilityMenu()
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Mobility.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Mobility socket / "));if(hit!=null&&int.TryParse(hit.Tip[18..],out int i))world.FittingMobilitySlot=i;
  int slot=world.FittingMobilitySlot;var s=world.Player.Mobility[slot];var actions=new List<(string,Action)>{("Activate / stop after cycle",()=>world.ToggleMobility(slot)),("Toggle online",()=>world.SetMobilityOnline(slot,!s.Online)),("Toggle repeat",()=>s.Repeat=!s.Repeat),("Toggle overload",()=>{if(s.Definition?.HeatDamage>0&&!s.Burnt)s.Overheat=!s.Overheat;}),("Face selected",world.FaceSelected)};if(world.Docked){actions.Add(("Unfit mobility",()=>world.UnfitMobility(slot)));actions.Add(("Repair mobility",()=>world.RepairMobility(slot)));actions.Add(("Repair stored mobility",()=>world.RepairStoredMobility()));}if(s.Definition!=null)actions.Add(("Show Info",()=>ShowItemInfo(s.ModuleId)));ShowPopup(s.Definition?.Name??"Mobility socket",actions);
 }
 void MobilityHud()
 {
  if(world.Screen!=ViewMode.Space)return;ui.Button(new(76,523,320,25),"PROPULSION / CARGO [Alt+P]",()=>Open(ViewMode.Mobility));int n=0;foreach(var pair in world.Player.Mobility.Select((s,i)=>(s,i)).Where(x=>x.s.Definition?.Active==true).Take(6)){var s=pair.s;int slot=pair.i;string name=s.Definition!.Name;ui.Button(new(420+n*120,650,114,25),name[..Math.Min(10,name.Length)]+(s.Active?$" {s.CycleLeft:0}":s.Cooldown>0?$" {s.Cooldown:0}":""),()=>world.ToggleMobility(slot),s.Definition.Description,active:s.Active);n++;}
 }
}
