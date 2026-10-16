using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int enhancementUiChecks;
 void RunEnhancementUiChecks(int stage)
 {
  void Check(bool ok,string name){if(!ok)throw new Exception("UPGRADE UI FAIL: "+name);enhancementUiChecks++;Console.WriteLine("UI PASS: "+name);}
  void Click(string prefix){var hit=ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}
  var w=desktop.First(x=>x.Kind==ViewMode.Enhancements);var p=world.Player;var damage=EnhancementCatalog.All.First(d=>d.Kind=="DroneDamage");var sensor=EnhancementCatalog.All.First(d=>d.Kind=="Sensor");
  switch(stage)
  {
   case 0:Check(world.Docked&&EnhancementCatalog.All.Length==21,"Upgrade window opens all 21 profiles");int x=w.X;TestDrag(new(w.X+90,w.Y+12),new(w.X+110,w.Y+22));Check(w.X==x+20,"Upgrade title drag");int width=w.Width;TestDrag(new(w.Bounds.Right-5,w.Bounds.Bottom-5),new(w.Bounds.Right+15,w.Bounds.Bottom+5));Check(w.Width==width+20,"Upgrade resize");break;
   case 1:Click("Upgrade catalog / "+damage.Id);break;
   case 2:Check(upgradeType==damage.Id,"Catalog selection updates description");Click("Fit selected upgrade /");break;
   case 3:Check(p.Enhancements[8].ModuleId==damage.Id&&world.ItemCount(damage.Id,true)==2,"Fit consumes actual stock");Click("Upgrade online / 8");break;
   case 4:Check(!p.Enhancements[8].Online&&p.EnhancementFactor("droneDamage")==1,"Offline removes passive bonus");Click("Upgrade online / 8");Click("Upgrade socket / 2");upgradePage=1;break;
   case 5:Click("Upgrade catalog / "+sensor.Id);break;
   case 6:Click("Fit selected upgrade /");break;
   case 7:Check(p.Enhancements[2].ModuleId==sensor.Id,"Sensor fits medium socket");Click("Upgrade script / "+EnhancementCatalog.Scripts.First(s=>s.Mode=="Range").Id);break;
   case 8:Check(p.Enhancements[2].Script?.Mode=="Range","Load sensor reach script");Click("Upgrade activate / 2");break;
   case 9:Check(!p.Enhancements[2].Active,"Docked active module blocked");Click("LIVE PERFORMANCE");break;
   case 10:Check(upgradeTab==1,"Live performance tab");Click("MODULES / SCRIPTS");Click("WEAPON RANGE");break;
   case 11:Check(world.Grid=="range","Trial range control");Click("Upgrade activate / 2");break;
   case 12:Check(p.Enhancements[2].Active&&p.Stat("maxTargetRange")>p.Attributes["maxTargetRange"],"Activation updates actual targeting range");world.SelectedId="range_0";Click("LOCK SELECTED");break;
   case 13:Check(world.Acquisitions.Any(a=>a.Id=="range_0"),"Lock control begins timed acquisition");world.UpdateEnhancements(15);Click("TARGET MANAGEMENT");break;
   case 14:Check(world.RetainedLocks.Any(l=>l.Id=="range_0"),"Completed target listed");Click("Retained lock / range_0");break;
   case 15:Check(world.LockedId=="range_0","Retained target activates focus");Click("Unlock target / range_0");break;
   case 16:Check(!world.RetainedLocks.Any(l=>l.Id=="range_0"),"Individual unlock control");Click("MODULES / SCRIPTS");Click("DOCK");break;
   case 17:Check(world.Docked&&!p.Enhancements[2].Active,"Dock stops active upgrade");Click("Upgrade socket / 9");upgradeType=damage.Id;EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;inventoryScroll=Array.IndexOf(DemoWorld.InventoryIds.Where(id=>world.ItemCount(id,true)>.001f).ToArray(),damage.Id);break;
   case 18:var item=itemHits.First(h=>h.Id==damage.Id);TestDrag(item.Rect.Center.ToVector2(),new(1160,425));break;
   case 19:Check(p.Enhancements[9].ModuleId==damage.Id,"Inventory drag fits selected upgrade socket");desktop.RemoveAll(x=>x.Kind==ViewMode.Inventory);break;
   case 20:TestPointer(new(w.X+w.Width-90,w.Y+405),right:ButtonState.Pressed);TestPointer(new(w.X+w.Width-90,w.Y+405));Check(context&&popupActions.Any(a=>a.Label=="Toggle online"),"Right-click upgrade controls");break;
   case 21:Click("Toggle online");break;
   case 22:Check(!p.Enhancements[9].Online,"Context online toggle");Click("MARKET");break;
   case 23:Check(desktop.Last().Kind==ViewMode.Market&&MarketItem==damage.Id&&world.Offers(damage.Id,world.SystemId,false).Any(),"Market shortcut shows real offers");desktop.RemoveAll(x=>x.Kind==ViewMode.Market);break;
   case 24:Click("INFO");break;
   case 25:Check(desktop.Last().Kind==ViewMode.Information&&desktop.Last().InfoKey==InformationCatalog.ItemKey(damage.Id),"Item database shortcut");desktop.RemoveAll(x=>x.Kind==ViewMode.Information);break;
   case 26:world.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.Player.Enhancements[9].ModuleId==damage.Id&&!loaded.Player.Enhancements[9].Online&&loaded.Player.Enhancements[2].ScriptId==p.Enhancements[2].ScriptId,"UI fitting and script save/load");break;
   case 27:Click("Upgrade socket / 2");break;
   case 28:Click("REMOVE SCRIPT");break;
   case 29:Check(p.Enhancements[2].ScriptId.Length==0,"Script unload control");break;
   case 30:upgradeType=sensor.Id;upgradePage=1;break;
   case 32:Console.WriteLine($"{enhancementUiChecks} damage and targeting graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
}
