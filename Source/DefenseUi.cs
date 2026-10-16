using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int defensePage,defenseFamily,defensePayloadPage;string defenseType="";
 string[] DefenseFamilies=>new[]{"All defense / capacitor"}.Concat(DefenseCatalog.All.Select(d=>d.Family).Distinct()).ToArray();
 void DefenseWindow()
 {
  var ship=world.Player;int index=world.FittingDefenseSlot;var state=ship.Defense[index];var selected=DefenseCatalog.ById.GetValueOrDefault(defenseType)??DefenseCatalog.All[0];var d=state.Definition;
  ui.Text(ship.HullName+" / DEFENSE & CAPACITOR",237,126,White,19);
  ui.Button(new(1028,123,198,28),"SHIP FITTING",()=>Open(ViewMode.Fitting));
  ui.Text($"CPU {ship.Stat("cpuLoad"):0}/{ship.Stat("cpuOutput"):0} | GRID {ship.Stat("powerLoad"):0}/{ship.Stat("powerOutput"):0} | EHP {ship.EffectiveHitpoints:0} | Align {ship.AlignTime:0.00}s",237,158,Cyan,12);
  ui.Text($"CAP {ship.Cap:0}/{ship.MaxCap:0} GJ | Peak {ship.PeakCapRecharge:0.00}/s | Active drain {ship.CapacitorUse:0.00}/s | "+(double.IsNaN(ship.CapacitorStableFraction)?"UNSTABLE":$"Stable {ship.CapacitorStableFraction:P0}")+" | Recharge "+(ship.Stat("rechargeRate")/1000).ToString("0")+"s",237,181,Gold,12);
  for(int i=0;i<12;i++)
  {int ix=i;var s=ship.Defense[i];string label=(i==0?"HIGH":i<6?"MID "+i:"LOW "+(i-5))+" / "+(s.Definition?.Name??"empty");if(label.Length>20)label=label[..17]+"...";ui.Button(new(237+i%6*164,207+i/6*34,157,29),label,()=>{world.FittingDefenseSlot=ix;defensePayloadPage=0;},"Defense socket / "+i,active:index==i);}
  var families=DefenseFamilies;defenseFamily=Math.Clamp(defenseFamily,0,families.Length-1);ui.Box(new(234,285,312,371),new Color(13,25,31));
  ui.Button(new(242,292,31,25),"<",()=>{defenseFamily=(defenseFamily+families.Length-1)%families.Length;defensePage=0;});ui.Center(families[defenseFamily],390,300,Cyan,10);ui.Button(new(504,292,31,25),">",()=>{defenseFamily=(defenseFamily+1)%families.Length;defensePage=0;});
  var list=DefenseCatalog.All.Where(d=>defenseFamily==0||d.Family==families[defenseFamily]).ToArray();int pages=Math.Max(1,(list.Length+8)/9);defensePage=Math.Clamp(defensePage,0,pages-1);int y=326;
  foreach(var def in list.Skip(defensePage*9).Take(9)) {var item=def;ui.Button(new(242,y,293,28),def.Name,()=>defenseType=item.Id,"Defense catalog / "+def.Id,active:selected.Id==def.Id);ui.Text(world.ItemCount(def.Id,true).ToString("0"),517,y+20,Gold,8);y+=32;}
  ui.Button(new(242,621,65,25),"Prev",()=>defensePage=Math.Max(0,defensePage-1));ui.Text($"{defensePage+1}/{pages} / {list.Length} types",323,628,Muted,11);ui.Button(new(469,621,66,25),"Next",()=>defensePage=Math.Min(pages-1,defensePage+1));
  ui.Text(selected.Name,562,292,Gold,17);ui.Wrap(selected.Description,562,321,659,White,11);
  ui.Text("INSTALLED: "+(d?.Name??"empty"),562,427,Cyan,13);
  ui.Text($"Cycle {state.CycleLeft:0.0}s | Reload {state.ReloadLeft:0.0}s | Reactivation {state.ReactivationLeft:0.0}s | Heat damage {state.Damage:P0}"+(state.Burnt?" / BURNT":""),562,450,Gold,11);
  ui.Text($"Shield {ship.Shield:0}/{ship.MaxShield:0} | Armor {ship.Armor:0}/{ship.MaxArmor:0} | Structure {ship.Hull:0}/{ship.MaxHull:0}",562,471,White,11);
  if(d!=null)
  {
   var payloads=DefensePayloads.For(d);int pp=Math.Max(1,(payloads.Length+3)/4);defensePayloadPage=Math.Clamp(defensePayloadPage,0,pp-1);int py=494;
   foreach(var payload in payloads.Skip(defensePayloadPage*4).Take(4)){var p=payload;ui.Button(new(562,py,324,25),p.Name+" / stock "+world.ItemCount(p.Id,world.Docked).ToString("0"),()=>world.LoadDefensePayload(index,p.Id),"Defense payload / "+p.Id,active:state.PayloadId==p.Id);py+=29;}
   if(pp>1){ui.Button(new(893,494,85,25),"Payload <",()=>defensePayloadPage=Math.Max(0,defensePayloadPage-1));ui.Button(new(985,494,85,25),"Payload >",()=>defensePayloadPage=Math.Min(pp-1,defensePayloadPage+1));}
   ui.Text($"Loaded {state.Loaded} / {(state.Payload==null?0:state.Payload.Consumable?(int)(d.MagazineVolume/state.Payload.Volume):1)} | "+(state.Payload?.Name??"dry / unscripted"),562,615,Cyan,11);
   ui.Button(new(893,528,156,25),state.Active?"STOP AFTER CYCLE":"ACTIVATE / PULSE",()=>world.ToggleDefense(index),"Defense activate / "+index,active:state.Active||state.Running);
   ui.Button(new(1057,528,164,25),"RELOAD",()=>world.ReloadDefense(index));
   ui.Button(new(893,559,156,25),state.Repeat?"REPEAT: ON":"REPEAT: OFF",()=>state.Repeat=!state.Repeat,active:state.Repeat);
   ui.Button(new(1057,559,164,25),state.AutoReload?"AUTO RELOAD: ON":"AUTO RELOAD: OFF",()=>state.AutoReload=!state.AutoReload,active:state.AutoReload);
   ui.Button(new(893,590,156,25),state.Overheat?"HEAT: ON":"HEAT: OFF",()=>{if(!state.Burnt&&d.HeatDamage>0)state.Overheat=!state.Overheat;},active:state.Overheat);
   ui.Button(new(1057,590,164,25),"STOW SPARES",()=>world.ProvisionDefenseCargo(index));
   if(d.Kind=="Reactive")ui.Text("Adaptive EM / TH / KI / EX: "+string.Join(" / ",state.ReactiveResists.Select(x=>x.ToString("P0"))),562,637,Gold,11);
  }
  ui.Button(new(562,662,155,27),"FIT SELECTED",()=>world.FitDefense(selected.Id,world.ItemCount(selected.Id,true)>0,index),"Fit selected defense / "+selected.Id+" / "+world.DefenseFitError(selected,index));
  ui.Button(new(725,662,155,27),"UNFIT MODULE",()=>world.UnfitDefense(index));ui.Button(new(889,662,155,27),"REPAIR MODULE",()=>world.RepairDefense(index));ui.Button(new(1053,662,80,27),"INFO",()=>{ShowItemInfo(selected.Id);});ui.Button(new(1140,662,81,27),"MARKET",()=>{catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id==selected.Id);Open(ViewMode.Market);});
  ui.Text("DEMO LOAN HULLS / catalog stock supplied; shared hull sockets and CPU/grid enforced",237,705,Gold,11);
  if(!world.Docked){for(int i=0;i<4;i++){int type=i;ui.Button(new(237+i*156,725,149,27),"PULSE "+((DamageType)i),()=>world.DefensePressure(world.Player.MaxArmor*.12f+100,(DamageType)type));}ui.Button(new(864,725,142,27),"NEUT PRESSURE",()=>world.DefenseNeutPressure(world.Player.MaxCap*.2f));ui.Button(new(1016,725,90,27),"DOCK",world.Dock);}
  else { int hp=(CombatHulls.All.Length+4)/5;hullPage=Math.Clamp(hullPage,0,hp-1);for(int i=hullPage*5;i<Math.Min(CombatHulls.All.Length,(hullPage+1)*5);i++){var h=CombatHulls.All[i];ui.Button(new(237+i%5*155,725,149,27),h.Name,()=>world.DefenseTrial(h.Id),h.Role+" / class "+h.Size,ship.CombatHull==h.Id);}
  ui.Button(new(1016,725,90,27),"Hulls >",()=>hullPage=(hullPage+1)%hp);ui.Button(new(1114,725,108,27),"DEF RANGE",world.EnterDefenseRange); }
  ui.Text("Use fitting specialists to open defense | F3 toggles first shield booster | Right-click for module controls | Drag stock into selected socket",237,766,Muted,11);
 }
 void DefenseHud()
 {
  if(world.Screen!=ViewMode.Space)return;
  ui.Button(new(76,496,320,25),"DEFENSE / CAPACITOR",()=>Open(ViewMode.Defense));
  var list=world.Player.Defense.Select((s,i)=>(s,i)).Where(x=>x.s.Definition!=null).ToArray();int y=526;
  for(int n=0;n<Math.Min(12,list.Length);n++){var (s,index)=list[n];string name=s.Definition!.Name;if(name.Length>10)name=name[..9]+"…";string status=s.Burnt?" BURNT":s.ReloadLeft>0?" R "+s.ReloadLeft.ToString("0"):s.ReactivationLeft>0?" C "+s.ReactivationLeft.ToString("0"):s.Active||s.Running?" ON":"";int ix=index;ui.Button(new(76+n%3*108,y+n/3*29,103,25),name+status,()=>{world.FittingDefenseSlot=ix;if(s.Definition.Active)world.ToggleDefense(ix);else Open(ViewMode.Defense);},s.Definition.Description,active:s.Active||s.Running);}
 }
 void DefenseMenu()
 {
  var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Defense.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Defense socket / "));if(hit!=null&&int.TryParse(hit.Tip[17..],out int socket)&&socket>=0&&socket<12)world.FittingDefenseSlot=socket;
  int index=world.FittingDefenseSlot;var s=world.Player.Defense[index];var actions=new List<(string,Action)>();
  actions.Add(("Activate / stop after cycle",()=>world.ToggleDefense(index)));actions.Add(("Reload selected module",()=>world.ReloadDefense(index)));actions.Add(("Unload payload / script",()=>world.LoadDefensePayload(index,"")));actions.Add(("Toggle automatic reload",()=>s.AutoReload=!s.AutoReload));actions.Add(("Toggle repeat",()=>s.Repeat=!s.Repeat));actions.Add(("Toggle overload",()=>{if(s.Definition?.HeatDamage>0)s.Overheat=!s.Overheat;}));
  if(world.Docked){actions.Add(("Repair stored defense modules",()=>world.RepairStoredDefenses()));actions.Add(("Unfit selected module",()=>world.UnfitDefense(index)));actions.Add(("Repair selected module",()=>world.RepairDefense(index)));}if(s.Definition!=null)actions.Add(("Show Info",()=>ShowItemInfo(s.ModuleId)));ShowPopup(s.Definition?.Name??"Defense socket",actions);
 }
}
