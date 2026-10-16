using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 bool fittingEwar;
 int ewarPage,ewarFamily;
 string[] EwarFamilies=>new[]{"All EWAR"}.Concat(EwarCatalog.All.Select(e=>e.Family).Distinct()).ToArray();
 void EwarFitting()
 {
  ui.Button(new(1015,135,195,29),"LOGISTICS / FLEET",()=>Open(ViewMode.Support));
  ui.Button(new(815,135,195,29),"DEFENSE / CAP",()=>Open(ViewMode.Defense));
  var ship=world.Player;int slot=world.FittingEwarSlot;var state=ship.Ewar[slot];var families=EwarFamilies;
  DrawWindow(new(142,93,1085,695),"EWAR fitting / "+ship.HullName);
  ui.Text(ship.HullName+" / ELECTRONIC WARFARE",160,140,White,18);
  ui.Button(new(1020,135,177,29),"WEAPON FITTING",()=>fittingEwar=false);
  ui.Text($"CPU {ship.Stat("cpuLoad"):0}/{ship.Stat("cpuOutput"):0} | GRID {ship.Stat("powerLoad"):0}/{ship.Stat("powerOutput"):0} | CAP {ship.Cap:0}/{ship.MaxCap:0} GJ",160,174,Cyan,12);
  string[] slots=["EW HIGH / F5","EW MID 1 / F4","EW MID 2 / F6","EW MID 3 / F7"];
  for(int i=0;i<4;i++){int ix=i;ui.Button(new(158+i*190,201,181,29),slots[i],()=>world.FittingEwarSlot=ix,active:slot==i);}
  ui.Button(new(1000,199,198,29),world.WeaponSafety?"SAFETY: ON":"SAFETY: OFF",()=>world.WeaponSafety=!world.WeaponSafety,active:world.WeaponSafety);
  ui.Box(new(156,247,340,383),new Color(13,25,31));ewarFamily=Math.Clamp(ewarFamily,0,families.Length-1);
  ui.Button(new(166,259,35,27),"<",()=>{ewarFamily=(ewarFamily+families.Length-1)%families.Length;ewarPage=0;});ui.Center(families[ewarFamily],324,268,Cyan,10);ui.Button(new(450,259,35,27),">",()=>{ewarFamily=(ewarFamily+1)%families.Length;ewarPage=0;});
  var list=EwarCatalog.All.Where(e=>ewarFamily==0||e.Family==families[ewarFamily]).ToArray();int pages=Math.Max(1,(list.Length+8)/9);ewarPage=Math.Clamp(ewarPage,0,pages-1);int y=298;
  foreach(var e in list.Skip(ewarPage*9).Take(9))
  {
   var def=e;string label=e.Name.Length>36?e.Name[..33]+"...":e.Name;ui.Button(new(166,y,318,29),label,()=>world.FitEwar(def.Id,world.ItemCount(def.Id,true)>0,slot),e.Name+" / "+e.Description+" / "+world.EwarFitError(e,slot),state.ModuleId==e.Id);y+=33;
  }
  ui.Button(new(166,598,65,25),"Prev",()=>ewarPage=Math.Max(0,ewarPage-1));ui.Text($"{ewarPage+1}/{pages} / {list.Length} types",247,605,Muted,11);ui.Button(new(416,598,68,25),"Next",()=>ewarPage=Math.Min(pages-1,ewarPage+1));
  if(state.Definition is {} d)
  {
   ui.Text(d.Name,516,270,Gold,17);ui.Wrap(d.Description,516,301,668,White,12);
   ui.Text($"MODE {state.Mode} | LOADED {state.Loaded}/{d.Magazine} | CYCLE {state.Cooldown:0.0}s | WINDUP {state.WarmupLeft:0.0}s",516,400,Cyan,11);
   int cy=432;foreach(var p in EwarPayloads.For(d))
   {var payload=p;ui.Button(new(516,cy,423,28),p.Name+" / "+world.ItemCount(p.Id,world.Docked).ToString("0"),()=>world.LoadEwarPayload(slot,payload.Id),p.Name+" / Load EWAR payload",state.PayloadId==p.Id);cy+=33;}
   ui.Button(new(947,432,231,28),state.Active?"STOP EWAR":"ACTIVATE EWAR",()=>world.ToggleEwar(slot));
   ui.Button(new(947,470,231,28),"UNLOAD SCRIPT / PAYLOAD",()=>world.LoadEwarPayload(slot,""));
   ui.Button(new(947,508,231,28),"STOW EWAR SPARES",()=>world.ProvisionEwarCargo(slot));
   ui.Button(new(947,546,231,28),"UNFIT EWAR",()=>world.UnfitEwar(slot));
  }
  else ui.Text("Select a compatible module for this socket.",516,289,Muted,15);
  var player=world.EwarFor("player");var target=world.Locked;var victim=target==null?null:world.EwarFor(target.Id);
  ui.Text($"YOUR SHIP: speed x{player.Speed:0.00}, sensors x{player.LockRange:0.00}, tracking x{player.Tracking:0.00}",516,589,Cyan,11);
  ui.Text(target==null?"Lock a test ship to inspect its EWAR state.":$"TARGET: speed x{victim!.Speed:0.00}, signature x{victim.Signature:0.00}, tackle {victim.WarpStrength:0}, cap {target.Cap:0} GJ",516,611,Gold,11);
  ui.Text("TRIAL HULLS / choose class and role before fitting; all EWAR modules and payloads are supplied",160,645,Gold,11);
  int hp=(CombatHulls.All.Length+4)/5;hullPage=Math.Clamp(hullPage,0,hp-1);
  for(int i=hullPage*5;i<Math.Min(CombatHulls.All.Length,(hullPage+1)*5);i++){var h=CombatHulls.All[i];ui.Button(new(159+(i%5)*164,668,157,27),h.Name,()=>world.WeaponTrial(h.Id),h.Role+" / class "+h.Size,ship.CombatHull==h.Id);}
  ui.Button(new(997,668,90,27),"Hulls >",()=>hullPage=(hullPage+1)%hp);ui.Button(new(1094,668,105,27),"EW RANGE",world.EnterEwarRange);
  ui.Button(new(516,711,179,27),ship.MicroWarpOn?"TRIAL MWD OFF":"TRIAL MWD ON",()=>{if(world.WeaponTestMode)world.ToggleMicroWarp();});ui.Button(new(701,711,179,27),"TRIAL MICRO-JUMP",()=>world.MicroJump());
  ui.Text("F4 mid 1 | F5 high | F6 mid 2 | F7 mid 3. Drag modules into fitting or right-click inventory to fit.",160,749,Muted,11);
 }
 void EwarHud()
 {
  void Icon(int slot,int x,int y,string key)
  {
   var s=world.Player.Ewar[slot];var e=s.Definition;Module(x,y,e?.Kind.ToString().ToUpper()??"EW -",key,s.Active,s.Cooldown/Math.Max(.1f,e?.Cycle??1),()=>world.ToggleEwar(slot),e?.Description??"Empty electronic warfare socket");
  }
  Icon(0,460,753,"F5");Icon(2,540,753,"F6");Icon(3,540,831,"F7");
  var incoming=world.EwarEffects.Where(e=>e.TargetId=="player"&&e.Remaining>0).GroupBy(e=>e.Kind).Select(g=>g.Key.ToString()+" "+g.Max(e=>e.Remaining).ToString("0.0")+"s").ToList();
  if(world.EwarFor("player").Bubble)incoming.Add("INTERDICTION FIELD");
  if(incoming.Count>0){ui.Panel(new(433,561,340,119),"Incoming electronic warfare");int y=600;foreach(var t in incoming.Take(4)){ui.Text(t,449,y,Gold,11);y+=17;}}
  if(world.Locked is {} target){var ew=world.EwarFor(target.Id);ui.Text($"TARGET CAP {target.Cap:0} GJ | SIG {world.EffectiveSignature(target):0} | SPEED x{ew.Speed:0.00}",969,203,Cyan,10);}
  if(world.PendingLockId.Length>0)ui.Text($"LOCKING / {world.LockTimeLeft:0.0}s",969,223,Gold,11);
 }
}
