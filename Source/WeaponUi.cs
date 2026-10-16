using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int weaponPage,weaponFamily,chargePage,hullPage;
 string[] WeaponFamilies=>new[]{"All weapons"}.Concat(WeaponCatalog.All.Select(w=>w.Family).Distinct()).ToArray();
 void WeaponFitting()
 {
  if(fleetBrowser){FleetRegistryWindow();return;}
  ui.Button(new(425,135,190,29),"DAMAGE / TARGETING",()=>Open(ViewMode.Enhancements));
  ui.Button(new(1015,169,195,29),"LOGISTICS / FLEET",()=>Open(ViewMode.Support));
  ui.Button(new(625,135,180,29),"DEFENSE / CAP",()=>Open(ViewMode.Defense));
  ui.Button(new(815,135,195,29),"DRONE / FIGHTER BAY",()=>Open(ViewMode.Drones));
  var ship=world.Player;int slot=world.FittingWeaponSlot=Math.Clamp(world.FittingWeaponSlot,0,ship.Weapons.Length-1);var state=ship.WeaponAt(slot);
  DrawWindow(new(142,93,1085,695),"Ship fitting / "+ship.HullName);
  ui.Button(new(1020,135,177,29),"EWAR FITTING",()=>fittingEwar=true);
  ui.Text(ship.HullName+" / "+CombatHulls.Get(ship.CombatHull).Role+" "+CombatHulls.Get(ship.CombatHull).Size,160,140,White,19);
  ui.Text($"CPU {ship.Stat("cpuLoad"):0}/{ship.Stat("cpuOutput"):0}  |  GRID {ship.Stat("powerLoad"):0}/{ship.Stat("powerOutput"):0}  |  DPS {ship.TotalDps:0.0}",160,170,Cyan,13);
  for(int i=0;i<ship.Weapons.Length;i++){int index=i;ui.Button(new(158+i*71,201,67,29),"HIGH "+(i+1),()=>{world.FittingWeaponSlot=index;chargePage=0;},i==1&&!ship.IsFleetHull?"HIGH 2 / UTILITY":"HIGH "+(i+1),active:slot==i);}
  ui.Button(new(762,199,230,29),"PROPULSION / CARGO",()=>Open(ViewMode.Mobility));
  ui.Button(new(762,235,230,29),"EXPLORATION / MINING",()=>Open(ViewMode.Utilities));
  ui.Button(new(516,630,225,28),"FLEET REGISTRY / 569",()=>fleetBrowser=true);
  if(ship.IsFleetHull)ui.Button(new(748,630,220,28),"UNFIT SELECTED WEAPON",()=>world.UnfitFleetWeapon(slot));
  ui.Text("INSTALLED",463,207,Gold,11);
  ui.Text("Selected high socket "+(slot+1),463,230,White,12);
  ui.Button(new(1000,199,198,29),world.WeaponSafety?"SAFETY: ON":"SAFETY: OFF",()=>world.WeaponSafety=!world.WeaponSafety,active:world.WeaponSafety);
  ui.Box(new(156,247,340,383),new Color(13,25,31));var families=WeaponFamilies;
  weaponFamily=Math.Clamp(weaponFamily,0,families.Length-1);
  ui.Button(new(166,259,35,27),"<",()=>{weaponFamily=(weaponFamily+families.Length-1)%families.Length;weaponPage=0;});
  ui.Center(weaponFamily==0?families[0]:WeaponCatalog.All.First(w=>w.Family==families[weaponFamily]).DisplayFamily,324,268,Cyan,11);
  ui.Button(new(450,259,35,27),">",()=>{weaponFamily=(weaponFamily+1)%families.Length;weaponPage=0;});
  var list=WeaponCatalog.All.Where(w=>weaponFamily==0||w.Family==families[weaponFamily]).ToArray();int pages=Math.Max(1,(list.Length+8)/9);weaponPage=Math.Clamp(weaponPage,0,pages-1);
  int y=298;foreach(var w in list.Skip(weaponPage*9).Take(9))
  {
   var def=w;bool stock=world.ItemCount(w.Id,true)+world.ItemCount(w.Id,false)>0;string name=w.Name.Length>36?w.Name[..33]+"...":w.Name;
   ui.Button(new(166,y,318,29),name,()=>{world.FitWeapon(def.Id,world.ItemCount(def.Id,true)>0,slot);},w.Name+" / "+w.Description+" / "+world.FitError(w,slot),state?.ModuleId==w.Id);
   ui.Text(stock?"OWNED":"MARKET",433,y+20,stock?Green:Muted,8);y+=33;
  }
  ui.Button(new(166,598,65,25),"Prev",()=>weaponPage=Math.Max(0,weaponPage-1));ui.Text($"{weaponPage+1}/{pages} / {list.Length} types",247,605,Muted,11);ui.Button(new(416,598,68,25),"Next",()=>weaponPage=Math.Min(pages-1,weaponPage+1));
  if(state!=null)
  {
   var w=state.Definition;ui.Text(w.Name,516,290,Gold,17);ui.Wrap(w.Description,516,321,668,White,12);
   ui.Text($"CHARGE: {state.Charge?.Name??"none"}  |  MAG {state.Loaded}/{w.Magazine}  |  RELOAD {state.ReloadLeft:0.0}s  |  CYCLE {state.Cooldown:0.0}s",516,427,Cyan,11);
   var charges=ChargeCatalog.For(w);int cp=Math.Max(1,(charges.Length+3)/4);chargePage=Math.Clamp(chargePage,0,cp-1);
   int cy=453;foreach(var charge in charges.Skip(chargePage*4).Take(4))
   {
    var c=charge;float count=world.ItemCount(c.Id,world.Docked&&world.ItemCount(c.Id,true)>0);
    ui.Button(new(516,cy,423,28),c.Name+" / stock "+count.ToString("0"),()=>world.LoadWeapon(slot,c.Id),c.Name+" / Load from "+(world.Docked?"hangar":"cargo")+"; incompatible payloads are rejected.",c.Id==state.ChargeId);cy+=33;
   }
   if(cp>1){ui.Button(new(947,453,99,27),"Charges <",()=>chargePage=Math.Max(0,chargePage-1));ui.Button(new(1050,453,99,27),"Charges >",()=>chargePage=Math.Min(cp-1,chargePage+1));}
   ui.Button(new(947,493,110,28),"RELOAD",()=>world.ReloadWeapon(slot));
   ui.Button(new(947,534,232,28),"STOW SPARE CHARGES",()=>world.ProvisionWeapon(slot));
   ui.Button(new(1064,493,115,28),state.Active?"STOP":"ACTIVATE",()=>world.ToggleWeapon(slot),active:state.Active);
  }
  if(slot==1)
  {
   ui.Button(new(516,594,166,28),"Mining laser",()=>world.FitItem("miner",world.ItemCount("miner",true)>0));
   ui.Button(new(690,594,166,28),"Data analyzer",()=>world.FitItem("analyzer",world.ItemCount("analyzer",true)>0));
  }
  ui.Text("WEAPON TRIAL HULLS / demo loan service; stocks all 112 weapons and charges",160,645,Gold,11);
  int hp=(CombatHulls.All.Length+4)/5;hullPage=Math.Clamp(hullPage,0,hp-1);
  for(int i=hullPage*5;i<Math.Min(CombatHulls.All.Length,(hullPage+1)*5);i++)
  {var h=CombatHulls.All[i];ui.Button(new(159+(i%5)*164,668,157,27),h.Name,()=>world.WeaponTrial(h.Id),h.Role+" / class "+h.Size,ship.CombatHull==h.Id);}
  ui.Button(new(997,668,90,27),"Hulls >",()=>hullPage=(hullPage+1)%hp);
  ui.Button(new(1094,668,105,27),"RANGE",world.EnterWeaponRange);
  ui.Text("Inventory: right-click Fit uses selected high slot. Drag a weapon anywhere into this fitting window. F1/F2 fire.",160,720,Muted,11);
  ui.Text(world.WeaponTestMode?"WEAPON TRIAL ACTIVE / campaign economy unchanged":"Campaign fitting / owned modules and charges only",160,749,world.WeaponTestMode?Gold:Cyan,11);
 }
 int registryPage;
 void WeaponRegistry()
 {
  DrawWindow(new(256,103,964,661),"Item database / Syndicate registry");
  const int pageSize=12;int total=ItemCatalog.All.Length,pages=(total+pageSize-1)/pageSize;
  registryPage=Math.Clamp(registryPage,0,pages-1);int selected=Math.Clamp(catalogSelection,0,total-1);
  ui.Text($"{total} TYPES / 112 WEAPON DESIGNS",274,148,Cyan,12);
  for(int i=registryPage*pageSize;i<Math.Min(total,(registryPage+1)*pageSize);i++)
  {
   int index=i;var item=ItemCatalog.All[i];int y=180+(i%pageSize)*37;
   ui.Button(new(274,y,336,32),item.Name.Length>36?item.Name[..33]+"...":item.Name,()=>catalogSelection=index,item.Description,selected==i);
  }
  ui.Button(new(274,644,87,28),"Previous",()=>registryPage=Math.Max(0,registryPage-1));ui.Text($"{registryPage+1}/{pages}",395,653,White,12);ui.Button(new(491,644,119,28),"Next",()=>registryPage=Math.Min(pages-1,registryPage+1));
  var def=ItemCatalog.All[selected];ui.Text(def.Name,639,162,Gold,18);ui.Wrap(def.Group,639,198,550,Cyan,12);ui.Wrap(def.Description,639,236,550,White,11);
  ui.Text($"VOLUME {def.Volume:0.##} m3 | REFERENCE PRICE {def.Price:N0} CR",639,550,Muted,12);
  ui.Button(new(639,588,244,31),"VIEW MARKET",()=>Open(ViewMode.Market));
  if(EnhancementCatalog.ById.ContainsKey(def.Id))ui.Button(new(894,588,294,31),"FIT SELECTED UPGRADE SOCKET",()=>world.FitEnhancement(def.Id,world.ItemCount(def.Id,true)>0,world.FittingEnhancementSlot));
  else if(SupportCatalog.ById.ContainsKey(def.Id))ui.Button(new(894,588,294,31),"FIT SELECTED SUPPORT SOCKET",()=>world.FitSupport(def.Id,world.ItemCount(def.Id,true)>0,world.FittingSupportSlot));
  if(SupportPayloads.New.Any(p=>p.Id==def.Id))ui.Button(new(894,588,294,31),"LOAD SUPPORT PAYLOAD",()=>world.LoadSupportPayload(world.FittingSupportSlot,def.Id));
  if(DefenseCatalog.ById.ContainsKey(def.Id))ui.Button(new(894,588,294,31),"FIT SELECTED DEFENSE SOCKET",()=>world.FitDefense(def.Id,world.ItemCount(def.Id,true)>0,world.FittingDefenseSlot));
  if(DefensePayloads.ById.ContainsKey(def.Id))ui.Button(new(894,588,294,31),"LOAD DEFENSE PAYLOAD",()=>world.LoadDefensePayload(world.FittingDefenseSlot,def.Id));
  if(DroneCatalog.ById.ContainsKey(def.Id))ui.Button(new(894,588,294,31),"LOAD DRONE / FIGHTER",()=>world.LoadDrone(def.Id,world.ItemCount(def.Id,true)>0));
  if(WeaponCatalog.ById.ContainsKey(def.Id))ui.Button(new(894,588,294,31),"FIT SELECTED HIGH SLOT",()=>world.FitWeapon(def.Id,world.ItemCount(def.Id,true)>0,world.FittingWeaponSlot));
  ui.Button(new(639,631,244,31),"SHIP FITTING",()=>Open(ViewMode.Fitting));
  ui.Text("Use the market search to filter by weapon name.",639,702,Muted,12);
 }
}
