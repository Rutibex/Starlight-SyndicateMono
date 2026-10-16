using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int defenseUiChecks;float defenseUiShield,defenseUiCapacity;
 void RunDefenseUiChecks(int stage)
 {
  if(stage>25)return;void Check(bool ok,string label){UiCheck(ok,label);defenseUiChecks++;}void Click(string prefix){var hit=ui.Hits.LastOrDefault(h=>h.Tip==prefix)??ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}
  var window=desktop.First(w=>w.Kind==ViewMode.Defense);var p=world.Player;var armor=DefenseCatalog.All.First(d=>d.Kind=="AncillaryArmor"&&d.Size==1);var extender=DefenseCatalog.All.First(d=>d.Kind=="Extender"&&d.Size==1);
  switch(stage)
  {
   case 0:Check(world.Docked&&DefenseCatalog.All.Length==83,"Defense catalog opens with 83 modules");int x=window.X;TestDrag(new(window.X+80,window.Y+12),new(window.X+100,window.Y+22));Check(window.X==x+20,"Defense title drag moves window");int width=window.Width;TestDrag(new(window.Bounds.Right-5,window.Bounds.Bottom-5),new(window.Bounds.Right+15,window.Bounds.Bottom+5));Check(window.Width==width+20,"Defense resize scales socket controls");break;
   case 1:Click("Defense catalog / df_399");break;
   case 2:Check(defenseType=="df_399","Catalog selection displays chosen profile");Click("Fit selected defense /");break;
   case 3:Check(p.Defense[1].ModuleId=="df_399"&&world.ItemCount("df_399",true)==2,"Fit button consumes real defense stock");Click("Defense activate / 1");break;
   case 4:Check(!p.Defense[1].Active,"Activation UI rejects docked ship");defenseFamily=Array.IndexOf(DefenseFamilies,"Ancillary armor");defensePage=0;Click("Defense socket / 6");break;
   case 5:Click("Defense catalog / "+armor.Id);break;
   case 6:Click("Fit selected defense /");break;
   case 7:Check(p.Defense[6].ModuleId==armor.Id,"Low socket accepts ancillary armor module");Click("Defense payload / dc_paste");break;
   case 8:Check(p.Defense[6].Loaded==8&&world.ItemCount("dc_paste",true)==192,"Paste control loads and consumes eight real units");Click("STOW SPARES");break;
   case 9:Check(world.ItemCount("dc_paste",false)==24,"Provision control transfers spares into cargo");Click("REPEAT: ON");Click("AUTO RELOAD: ON");Click("DEF RANGE");break;
   case 10:Check(world.Grid=="defenserange"&&!world.Docked,"Defense range service undocks into test grid");p.Armor=1;p.Shield=1;p.Cap=p.MaxCap;Click("Defense socket / 6");break;
   case 11:Click("Defense activate / 6");break;
   case 12:Check(p.Defense[6].PendingRepair>0&&p.Armor==1&&p.Defense[6].Loaded==7,"Active armor UI schedules repair and spends paste");Click("Defense activate / 6");break;
   case 13:world.UpdateDefense(8);Check(p.Armor>1&&!p.Defense[6].Active,"Stop control completes paid repair without repeating");Click("Defense socket / 1");p.Shield=1;defenseUiShield=p.Shield;break;
   case 14:Click("Defense activate / 1");break;
   case 15:Check(p.Shield>defenseUiShield&&p.Defense[1].Active,"Shield activation control repairs immediately");TestPointer(new(window.X+window.Width-80,window.Y+400),right:ButtonState.Pressed);TestPointer(new(window.X+window.Width-80,window.Y+400));break;
   case 16:Check(context&&popupActions.Any(a=>a.Label=="Toggle overload"),"Defense right-click exposes cycle and overload controls");Click("Toggle repeat");break;
   case 17:Check(!p.Defense[1].Repeat,"Context repeat control changes behavior");world.Dock();Click("Defense socket / 2");EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;var ids=DemoWorld.InventoryIds.Where(id=>world.ItemCount(id,true)>.001f).ToArray();inventoryScroll=Array.IndexOf(ids,extender.Id);defenseUiCapacity=p.MaxShield;break;
   case 18:var item=itemHits.First(h=>h.Id==extender.Id);TestDrag(item.Rect.Center.ToVector2(),new(1160,425));break;
   case 19:Check(p.Defense[2].ModuleId==extender.Id&&p.MaxShield>defenseUiCapacity,"Inventory drag fits extender and updates live capacity");desktop.RemoveAll(w=>w.Kind==ViewMode.Inventory);break;
   case 20:Click("MARKET");break;
   case 21:Check(desktop.Last().Kind==ViewMode.Market&&MarketItem==armor.Id&&world.Offers(armor.Id,world.SystemId,false).Any(),"Market shortcut selects module with real offers");desktop.RemoveAll(w=>w.Kind==ViewMode.Market);break;
   case 22:Click("INFO");break;
   case 23:Check(desktop.Last().Kind==ViewMode.Information&&desktop.Last().InfoKey==InformationCatalog.ItemKey(armor.Id),"Item information shortcut selects defense stats");desktop.RemoveAll(w=>w.Kind==ViewMode.Information);break;
   case 24:world.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.Player.Defense[6].Loaded==p.Defense[6].Loaded&&loaded.Player.Defense[2].ModuleId==extender.Id,"UI-created defense fit and ammunition survive save/load");break;
   case 25:Console.WriteLine($"{defenseUiChecks} defense graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
}
