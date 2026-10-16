using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 string fitTestGun="",fitTestMobility="";float fitTestStock,fitTestCpu;int fitLayoutUiChecks;
 void RunFittingLayoutUiChecks(int stage)
 {
  void Check(bool ok,string label){UiCheck(ok,label);fitLayoutUiChecks++;}
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Tip==tip).Rect.Center.ToVector2());
  void Item(string id)=>TestClick(ui.Hits.Last(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Key=="fit-item:"+id).Rect.Center.ToVector2());
  void Socket(string key){var s=fitSockets.Single(s=>s.Key==key);Click(FitSocketTip(s));}
  switch(stage)
  {
   case 0:Check(!fittingWorkshop&&!fleetBrowser&&world.Docked,"Circular fitting overview is the default docked view");Check(fitSockets.Count==13&&ui.Hits.Count(h=>h.Tip.StartsWith("Slot / "))==13,"Four launcher hull draws exact high, medium, low and rig banks");Socket("weapon:3");break;
   case 1:Check(world.FittingWeaponSlot==3&&SelectedFitModule?.Index==3,"Fourth launcher selects its actual underlying weapon socket");fitTestGun=SelectedFitModule!.Id;fitTestStock=world.ItemCount(fitTestGun,true);Click("Unfit selected fitting module");break;
   case 2:Check(world.Player.WeaponAt(3)==null&&world.ItemCount(fitTestGun,true)==fitTestStock+1,"Unfit button returns fourth launcher to owned stock");Socket("high:empty:3");fitOwnedOnly=true;fitSearch=DemoWorld.ItemName(fitTestGun);fitBrowsePage=0;break;
   case 3:Item(fitTestGun);Click("Fit / load selected catalog item");Check(world.Player.WeaponAt(3)?.ModuleId==fitTestGun&&world.ItemCount(fitTestGun,true)==fitTestStock,"Owned catalog refits actual fourth launcher without duplicate stock");fitOwnedOnly=false;break;
   case 4:Click("Browse compatible fitting charges");Check(fitBrowseTab==2&&FitPayloads(SelectedFitModule).Length>0,"Charges tab filters to the selected launcher");break;
   case 5:Item(ChargeCatalog.Default(world.Player.WeaponAt(3)!.Definition)!.Id);Click("Fit / load selected catalog item");Check(world.Player.WeaponAt(3)!.Loaded>0,"Charge action loads a real magazine from owned inventory");break;
   case 6:var preview=ui.Hits.Last(h=>h.Tip==FitPreviewTip).Rect;float beforeYaw=fitYaw,cameraYaw=renderer.Yaw;TestDrag(preview.Center.ToVector2(),preview.Center.ToVector2()+new Vector2(35,18));Check(fitYaw!=beforeYaw&&renderer.Yaw==cameraYaw,"Fitting preview rotates independently of world camera");Click("Reset fitting preview");Check(Math.Abs(fitYaw-.7f)<.001f,"Preview reset returns default orientation");Click("Open selected module controls");break;
   case 7:Check(fittingWorkshop,"Selected weapon opens existing detailed controls");Click("Return to fitting overview");break;
   case 8:Check(!fittingWorkshop,"Detailed controls return to the circular overview");Click("Toggle fitting stats / Capacitor");Check(fitCollapsed.Contains("Capacitor"),"Capacitor section collapses");Click("Modules");break;
   case 9:Click("Toggle fitting stats / Capacitor");Check(!fitCollapsed.Contains("Capacitor"),"Capacitor section expands");Click("Search fitting catalog");FitText('x');FitText('\b');Check(fitSearchFocus&&fitSearch.Length==0,"Search accepts text and backspace without fitting anything");fitSearchFocus=false;Click("Next fitting catalog page");Check(fitBrowsePage==1,"Catalog supports paging");break;
   case 10:var w=desktop.Single(w=>w.Kind==ViewMode.Fitting);int x=w.X;TestDrag(new(w.X+100,w.Y+12),new(w.X+120,w.Y+18));Check(w.X==x+20,"Fitting window moves with its title bar");int width=w.Width;TestDrag(new(w.Bounds.Right-5,w.Bounds.Bottom-5),new(w.Bounds.Right-65,w.Bounds.Bottom-25));Check(w.Width==width-60,"Fitting window resizes while retaining controls");break;
   case 11:Socket("weapon:3");Check(SelectedFitModule?.Index==3,"Socket selection works after moving and resizing");var d=MobilityCatalog.All.Where(d=>d.Kind=="AB"&&d.Size==1).OrderBy(d=>d.Power).First();fitTestMobility=d.Id;world.Hangar[d.Id]=1;EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;inventoryScroll=0;break;
   case 12:var item=itemHits.Single(h=>h.Id==fitTestMobility);var empty=fitSockets.First(s=>s.Bank=="medium"&&s.Module==null);var hit=ui.Hits.Last(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Tip==FitSocketTip(empty));TestDrag(item.Rect.Center.ToVector2(),hit.Rect.Center.ToVector2());Check(world.Player.Mobility.Any(s=>s.ModuleId==fitTestMobility)&&world.ItemCount(fitTestMobility,true)==0,"Inventory drag to medium socket fits propulsion and consumes stock");desktop.RemoveAll(w=>w.Kind==ViewMode.Inventory);fitTestCpu=(float)world.TotalFitCpu;break;
   case 13:Click("Toggle selected fitting module online");break;
   case 14:Check(!world.Player.Mobility.First(s=>s.ModuleId==fitTestMobility).Online&&world.TotalFitCpu<fitTestCpu,"Offline control removes propulsion resource use");Click("Toggle selected fitting module online");break;
   case 15:Check(world.Player.Mobility.First(s=>s.ModuleId==fitTestMobility).Online,"Online control restores module");var slot=ui.Hits.Last(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Tip==FitSocketTip(SelectedFitSocket!));TestPointer(slot.Rect.Center.ToVector2(),right:ButtonState.Pressed);TestPointer(slot.Rect.Center.ToVector2());Check(context&&popupActions.Any(a=>a.Label=="Unfit to hangar"),"Right-click slot opens selected module context actions");break;
   case 16:TestClick(ui.Hits.Last(h=>h.Owner=="popup"&&h.Tip=="Unfit to hangar").Rect.Center.ToVector2());Check(!world.Player.Mobility.Any(s=>s.ModuleId==fitTestMobility)&&world.ItemCount(fitTestMobility,true)==1,"Context unfit returns correct system module");Click("Hull & berths");break;
   case 17:Check(fitBrowseTab==0&&ui.Hits.Any(h=>h.Tip.StartsWith("Board local berth / ")),"Hull tab exposes real local ship berths");Click(ui.Hits.First(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Tip.StartsWith("Board local berth / ")).Tip);break;
   case 18:Check(!world.Player.IsFleetHull,"Berth button boards the retained legacy ship");break;
   case 19:Click(ui.Hits.First(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Tip.StartsWith("Board local berth / ")).Tip);break;
   case 20:Check(world.Player.CombatHull==FleetCatalog.Source("Kestrel").Id&&world.Player.WeaponAt(3)!.Loaded>0,"Boarding back restores the four-launcher fit and magazine");Click("FLEET REGISTRY");break;
   case 21:Check(fleetBrowser,"Fleet registry remains connected to fitting");break;
   case 22:Click("SHOW SHIP INFORMATION");Check(desktop.Last().Kind==ViewMode.ShipInfo,"Registry opens Pass 17 ship information");desktop.RemoveAll(w=>w.Kind==ViewMode.ShipInfo);fleetBrowser=false;fitBrowseTab=1;fitSearch="";fitOwnedOnly=false;fitBrowsePage=0;break;
   case 23:Socket("weapon:3");Check(SelectedFitModule?.Index==3,"Overview still routes fourth launcher after window navigation");world.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.Player.WeaponAt(3)!.Loaded==world.Player.WeaponAt(3)!.Loaded,"Layout changes persist through campaign save/load");Console.WriteLine($"{fitLayoutUiChecks} fitting layout UI checks passed.");break;
  }
  oldMouse=Mouse.GetState();
 }
}
