using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int mobilityUiChecks;
 void RunMobilityUiChecks(int stage)
 {
  void Check(bool ok,string name){if(!ok)throw new Exception("MOBILITY UI FAIL: "+name);mobilityUiChecks++;Console.WriteLine("UI PASS: "+name);}
  void Click(string prefix){var hit=ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}
  var w=desktop.First(x=>x.Kind==ViewMode.Mobility);var p=world.Player;var ab=MobilityCatalog.All.First(d=>d.Kind=="AB"&&d.Size==1);var cargo=MobilityCatalog.All.First(d=>d.Kind=="Cargo");
  switch(stage)
  {
   case 0:Check(world.Docked&&MobilityCatalog.All.Length==23,"Mobility window lists all 23 profiles");int x=w.X;TestDrag(new(w.X+90,w.Y+12),new(w.X+110,w.Y+22));Check(w.X==x+20,"Mobility title drag");int width=w.Width;TestDrag(new(w.Bounds.Right-5,w.Bounds.Bottom-5),new(w.Bounds.Right+15,w.Bounds.Bottom+5));Check(w.Width==width+20,"Mobility resize");break;
   case 1:Click("Mobility catalog / "+ab.Id);break;
   case 2:Check(mobilityType==ab.Id,"Catalog updates module details");Click("Fit selected mobility /");break;
   case 3:Check(p.Mobility[0].ModuleId==ab.Id&&world.ItemCount(ab.Id,true)==2,"Fitting consumes actual module stock");Click("Mobility online / 0");break;
   case 4:Check(!p.Mobility[0].Online,"Offline control");Click("Mobility online / 0");Click("Mobility activate / 0");break;
   case 5:Check(p.Mobility[0].Online&&!p.Mobility[0].Active,"Docked activation blocked");Click("LIVE FLIGHT STATS");break;
   case 6:Check(mobilityTab==1,"Live flight performance tab");Click("MODULES / FITTING");Click("WEAPON RANGE");break;
   case 7:Check(world.Grid=="range"&&!world.Docked,"Trial range control");Click("Mobility activate / 0");break;
   case 8:Check(p.Mobility[0].Active&&p.Speed>p.Attributes["maxVelocity"],"Activation changes actual thrust");Click("HEAT: OFF");break;
   case 9:Check(p.Mobility[0].Overheat,"Overload control");Click("REPEAT: ON");break;
   case 10:Check(!p.Mobility[0].Repeat,"Repeat control");world.UpdateMobility(ab.Cycle+.1f);Check(!p.Mobility[0].Active,"Single cycle ends");Click("DOCK");break;
   case 11:Check(world.Docked,"Trial docking control");Click("Mobility socket / 6");mobilityPage=1;break;
   case 12:Click("Mobility catalog / "+cargo.Id);break;
   case 13:Click("Fit selected mobility /");break;
   case 14:Check(p.Mobility[6].ModuleId==cargo.Id&&p.CargoMax>p.Attributes["capacity"],"Hold frame changes capacity");Click("Mobility socket / 7");EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;inventoryScroll=Array.IndexOf(DemoWorld.InventoryIds.Where(id=>world.ItemCount(id,true)>.001f).ToArray(),cargo.Id);break;
   case 15:var item=itemHits.First(h=>h.Id==cargo.Id);TestDrag(item.Rect.Center.ToVector2(),new(1160,425));break;
   case 16:Check(p.Mobility[7].ModuleId==cargo.Id,"Inventory drag fits selected mobility socket");desktop.RemoveAll(x=>x.Kind==ViewMode.Inventory);break;
   case 17:TestPointer(new(w.X+w.Width-90,w.Y+405),right:ButtonState.Pressed);TestPointer(new(w.X+w.Width-90,w.Y+405));Check(context&&popupActions.Any(a=>a.Label=="Toggle online"),"Right-click mobility controls");break;
   case 18:Click("Toggle online");break;
   case 19:Check(!p.Mobility[7].Online,"Context online toggle");Click("MARKET");break;
   case 20:Check(desktop.Last().Kind==ViewMode.Market&&MarketItem==cargo.Id&&world.Offers(cargo.Id,world.SystemId,false).Any(),"Market shortcut shows real offers");desktop.RemoveAll(x=>x.Kind==ViewMode.Market);break;
   case 21:Click("INFO");break;
   case 22:Check(desktop.Last().Kind==ViewMode.Information&&desktop.Last().InfoKey==InformationCatalog.ItemKey(cargo.Id),"Item database shortcut");desktop.RemoveAll(x=>x.Kind==ViewMode.Information);world.MobilityTrial("industrial_cap");break;
   case 23:Click("CAPITAL BEACON TRANSIT");break;
   case 24:Check(mobilityTab==2,"Capital transit tab");Click("Mobility loan hull / industrial_cap");break;
   case 25:Check(p.CombatHull=="industrial_cap","Industrial capital loan control");Click("LOAD TRANSIT FUEL");break;
   case 26:Check(world.ItemCount("jump_fuel",false)>0,"Fuel loading uses real cargo");world.Save();var load=new DemoWorld();Check(load.Load()&&load.Player.CombatHull=="industrial_cap"&&load.ItemCount("jump_fuel",false)==world.ItemCount("jump_fuel",false),"UI hull and fuel save/load");Click("MODULES / FITTING");break;
   case 28:Console.WriteLine($"{mobilityUiChecks} propulsion graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
}
