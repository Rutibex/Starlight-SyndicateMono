using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int upgradePage,upgradeTab;string upgradeType="";
 void EnhancementWindow()
 {
  var p=world.Player;int slot=world.FittingEnhancementSlot;var s=p.Enhancements[slot];var d=EnhancementCatalog.ById.GetValueOrDefault(upgradeType)??EnhancementCatalog.All[0];
  ui.Text(p.HullName+" / DAMAGE, TARGETING & FLIGHT SUPPORT",237,126,White,18);
  ui.Text($"CPU {p.Stat("cpuLoad"):0}/{p.Stat("cpuOutput"):0} | GRID {p.Stat("powerLoad"):0}/{p.Stat("powerOutput"):0} | CAP {p.Cap:0}/{p.MaxCap:0} GJ | LOCKS {world.RetainedLocks.Count}/{world.TargetCapacity}",237,158,Cyan,12);
  ui.Button(new(1008,123,213,28),"SHIP FITTING",()=>Open(ViewMode.Fitting));
  ui.Button(new(237,183,240,27),"MODULES / SCRIPTS",()=>upgradeTab=0,active:upgradeTab==0);ui.Button(new(485,183,240,27),"LIVE PERFORMANCE",()=>upgradeTab=1,active:upgradeTab==1);ui.Button(new(733,183,240,27),"TARGET MANAGEMENT",()=>upgradeTab=2,active:upgradeTab==2);
  if(upgradeTab==0)
  {
   for(int i=0;i<14;i++){int n=i;var m=p.Enhancements[i];string text=(i<2?"HIGH "+(i+1):i<8?"MID "+(i-1):"LOW "+(i-7))+" / "+(m.Definition?.Name??"empty");if(text.Length>17)text=text[..15]+"…";ui.Button(new(237+i%7*141,221+i/7*34,134,29),text,()=>world.FittingEnhancementSlot=n,"Upgrade socket / "+i,active:slot==i);}
   ui.Box(new(234,297,312,364),new Color(13,25,31));int y=307;foreach(var model in EnhancementCatalog.All.Skip(upgradePage*10).Take(10)){var selected=model;string text=model.Name;if(text.Length>29)text=text[..26]+"…";ui.Button(new(242,y,293,28),text,()=>upgradeType=selected.Id,"Upgrade catalog / "+model.Id,active:d.Id==model.Id);y+=31;}
   ui.Button(new(242,627,65,25),"Prev",()=>upgradePage=Math.Max(0,upgradePage-1));ui.Text($"{upgradePage+1}/3 / 21 profiles",320,634,Muted,11);ui.Button(new(469,627,66,25),"Next",()=>upgradePage=Math.Min(2,upgradePage+1));
   ui.Text(d.Name,562,302,Gold,18);ui.Wrap(d.Description,562,333,651,White,12);
   ui.Text("INSTALLED: "+(s.Definition?.Name??"empty"),562,458,Cyan,13);ui.Text($"{(s.Burnt?"BURNT":s.Online?"ONLINE":"OFFLINE")} / {(s.Active?"ACTIVE":"IDLE")} | Cycle {s.CycleLeft:0.0}s | Heat damage {s.Damage:P0}",562,482,Gold,12);
   if(s.Definition is {} fitted)
   {
    int sy=509;foreach(var script in EnhancementCatalog.Scripts.Where(script=>EnhancementCatalog.Fits(fitted,script))){var scriptCopy=script;ui.Button(new(562,sy,332,26),script.Name+" / "+world.ItemCount(script.Id,world.Docked).ToString("0"),()=>world.LoadEnhancementScript(slot,scriptCopy.Id),"Upgrade script / "+script.Id,active:s.ScriptId==script.Id);sy+=29;}
    ui.Button(new(909,509,150,26),s.Active?"STOP AFTER CYCLE":"ACTIVATE",()=>world.ToggleEnhancement(slot),"Upgrade activate / "+slot,active:s.Active);
    ui.Button(new(1067,509,154,26),s.Online?"OFFLINE":"ONLINE",()=>world.SetEnhancementOnline(slot,!s.Online),"Upgrade online / "+slot,active:s.Online);
    ui.Button(new(909,542,150,26),s.Repeat?"REPEAT: ON":"REPEAT: OFF",()=>s.Repeat=!s.Repeat,active:s.Repeat);ui.Button(new(1067,542,154,26),s.Overheat?"HEAT: ON":"HEAT: OFF",()=>{if(fitted.HeatDamage>0&&!s.Burnt)s.Overheat=!s.Overheat;},active:s.Overheat);
    ui.Button(new(909,575,312,26),"REMOVE SCRIPT",()=>world.LoadEnhancementScript(slot,""));ui.Text(s.Script?.Name??"Unscripted / balanced bonuses",562,612,Cyan,11);
   }
   ui.Button(new(562,664,150,27),"FIT SELECTED",()=>world.FitEnhancement(d.Id,world.ItemCount(d.Id,true)>0,slot),"Fit selected upgrade / "+d.Id+" / "+world.EnhancementFitError(d,slot));ui.Button(new(720,664,150,27),"UNFIT MODULE",()=>world.UnfitEnhancement(slot));ui.Button(new(878,664,150,27),"REPAIR MODULE",()=>world.RepairEnhancement(slot));
   ui.Button(new(1036,664,86,27),"INFO",()=>{ShowItemInfo(d.Id);});ui.Button(new(1130,664,91,27),"MARKET",()=>{catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id==d.Id);Open(ViewMode.Market);});
  }
  else if(upgradeTab==1)EnhancementPerformance();else EnhancementTargetPane();
  ui.Text("Alt+G: Upgrades | Shared hull sockets | Scripts are reusable | Percentage bonuses diminish when stacked",237,706,Muted,11);
  if(world.Docked){string[] hulls=["frigate","cruiser","battleship","carrier","supercarrier"];for(int i=0;i<hulls.Length;i++){string id=hulls[i];ui.Button(new(237+i*166,728,159,27),CombatHulls.Get(id).Name,()=>world.EnhancementTrial(id),"Upgrade loan hull / "+id,p.CombatHull==id);}ui.Button(new(1075,728,146,27),"WEAPON RANGE",world.EnterWeaponRange);}
  else{ui.Button(new(237,728,183,27),"LOCK SELECTED",world.LockTarget);ui.Button(new(428,728,183,27),"FIRE PRIMARY",()=>world.ToggleWeapon(0));ui.Button(new(619,728,183,27),"DRONE BAY",()=>Open(ViewMode.Drones));ui.Button(new(810,728,183,27),"DOCK",world.Dock);}
  ui.Text("Drag modules / scripts into this window's selected socket. Right-click sockets for controls.",237,769,Muted,11);
 }
 void EnhancementPerformance()
 {
  var p=world.Player;ui.Text("LIVE FITTING / effective bonuses, ammunition included",237,232,Gold,17);int y=271;
  for(int i=0;i<2;i++){var w=p.WeaponAt(i);if(w==null)continue;ui.Text(w.Definition.Name+$" / volley {w.Definition.Damage*(w.Charge?.DamageMultiplier??1)*p.WeaponEnhancement(w.Definition,"damage"):0.#} / cycle {w.Definition.Cycle*p.WeaponEnhancement(w.Definition,"cycle"):0.##}s / {p.EnhancedWeaponDps(w):0.##} DPS / reach {world.WeaponReach(w)/1000:0.#} km",237,y,White,13);y+=33;}
  ui.Text($"Drone bay DPS {p.BayDroneDps:0.##} | Active flight DPS {p.ActiveDroneDps:0.##} | Control range {p.Stat("droneControlDistance")/1000:0.#} km",237,y+5,Cyan,13);y+=48;
  foreach(var u in p.Drones.Where(u=>u.Owner=="player"&&u.Location is "Bay" or "Space").Take(4)){var d=world.EffectiveDrone(u);ui.Text(d.Name+$" / {u.Count} craft / {p.EnhancedDroneDps(u.Definition,u.Count):0.#} DPS / speed {d.Speed:0} / shield {d.Shield:0} / recharge {d.ShieldRecharge:0.#}s",237,y,White,12);y+=29;}
  ui.Text($"Lock reach {p.Stat("maxTargetRange")/1000:0.#} km | Scan resolution {p.Stat("scanResolution"):0.#} | Lock capacity {world.TargetCapacity}",237,511,Gold,13);ui.Text($"Mirage ECM strength x{p.EnhancementFactor("ecmStrength"):0.###} / optimal x{p.EnhancementFactor("ecmOptimal"):0.###}",237,540,Cyan,13);
  ui.Text($"Turret tracking x{p.SupportFactor("tracking"):0.###} / optimal x{p.SupportFactor("optimal"):0.###} / falloff x{p.SupportFactor("falloff"):0.###}",237,569,Cyan,13);
  ui.Text($"Missile speed x{p.EnhancementFactor("missileVelocity"):0.###} / life x{p.EnhancementFactor("missileFlightTime"):0.###} / radius x{p.EnhancementFactor("explosionRadius"):0.###} / explosion speed x{p.EnhancementFactor("explosionVelocity"):0.###}",237,598,Cyan,13);
  ui.Wrap("Weapon regulators apply only to their matching family. Drone amplifiers affect drone and fighter attacks. Squadron support applies to fighters only. Flight solution modules affect drone/fighter tracking, optimal and falloff, and fighter explosion radius/velocity. Offline or burnt modules provide no bonus.",237,641,984,Muted,12);
 }
 void EnhancementTargetPane()
 {
  ui.Text($"RETAINED LOCKS {world.RetainedLocks.Count} / {world.TargetCapacity} | ACTIVE: {world.Locked?.Name??"none"}",237,232,Gold,17);int y=271;
  foreach(var l in world.RetainedLocks.Take(12)){var id=l.Id;var target=world.Targets.FirstOrDefault(t=>t.Id==id);ui.Button(new(237,y,650,26),(target?.Name??id)+(l.Silent?" / SILENT":" / REVEALED"),()=>{world.LockedId=id;world.SelectedId=id;},"Retained lock / "+id,active:world.LockedId==id);ui.Button(new(901,y,180,26),"UNLOCK",()=>world.UnlockEnhancedTarget(id),"Unlock target / "+id);y+=29;}
  foreach(var a in world.Acquisitions.Take(4)){ui.Text("ACQUIRING "+(world.Targets.FirstOrDefault(t=>t.Id==a.Id)?.Name??a.Id)+$" / {a.Left:0.0}s"+(a.Silent?" / silent":""),237,y,Cyan,12);y+=25;}
  ui.Text("SELECTED: "+(world.Selected?.Name??"none"),237,669,White,13);
  ui.Wrap("Auto arrays acquire hostile ships, within module and sensor range. Silent arrays must be armed before locking. Attacks reveal silent locks. Retained locks are selectable; range loss, jamming, death and transit clear invalid locks.",237,612,975,Muted,12);
 }
 void EnhancementMenu()
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Enhancements.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Upgrade socket / "));if(hit!=null&&int.TryParse(hit.Tip[17..],out int i))world.FittingEnhancementSlot=i;
  int slot=world.FittingEnhancementSlot;var s=world.Player.Enhancements[slot];var actions=new List<(string,Action)>{("Activate / stop after cycle",()=>world.ToggleEnhancement(slot)),("Toggle online",()=>world.SetEnhancementOnline(slot,!s.Online)),("Remove script",()=>world.LoadEnhancementScript(slot,"")),("Toggle repeat",()=>s.Repeat=!s.Repeat),("Toggle overload",()=>{if(s.Definition?.HeatDamage>0&&!s.Burnt)s.Overheat=!s.Overheat;})};if(world.Docked){actions.Add(("Unfit upgrade",()=>world.UnfitEnhancement(slot)));actions.Add(("Repair upgrade",()=>world.RepairEnhancement(slot)));actions.Add(("Repair stored upgrades",()=>world.RepairStoredEnhancements()));}if(s.Definition!=null)actions.Add(("Show Info",()=>ShowItemInfo(s.ModuleId)));ShowPopup(s.Definition?.Name??"Upgrade socket",actions);
 }
 void EnhancementHud()
 {
  if(world.Screen!=ViewMode.Space)return;ui.Button(new(76,494,320,25),"DAMAGE / TARGETING [Alt+G]",()=>Open(ViewMode.Enhancements));int n=0;foreach(var pair in world.Player.Enhancements.Select((s,i)=>(s,i)).Where(x=>x.s.Definition?.Active==true).Take(6)){var s=pair.s;int slot=pair.i;string name=s.Definition!.Name;ui.Button(new(420+n*120,620,114,25),name[..Math.Min(10,name.Length)]+(s.Active?" ON":""),()=>world.ToggleEnhancement(slot),s.Definition.Description,active:s.Active);n++;}
  int x=425;foreach(var l in world.RetainedLocks.Where(l=>l.Id!=world.LockedId).Take(5)){var id=l.Id;string name=world.Targets.FirstOrDefault(t=>t.Id==id)?.Name??id;ui.Button(new(x,151,150,26),name[..Math.Min(17,name.Length)],()=>{world.LockedId=id;world.SelectedId=id;},"Retained lock / "+id);x+=156;}
  if(world.Acquisitions.Count>0)ui.Text($"Acquiring {world.Acquisitions.Count} / next {world.Acquisitions.Min(a=>a.Left):0.0}s",425,184,Cyan,12);
 }
}
