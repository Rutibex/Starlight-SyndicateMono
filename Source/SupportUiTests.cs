using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int supportUiChecks;
 void RunSupportUiChecks(int stage)
 {
  if(stage>32)return;void Check(bool ok,string label){UiCheck(ok,label);supportUiChecks++;}void Click(string prefix){var hit=ui.Hits.LastOrDefault(h=>h.Tip==prefix)??ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}
  var window=desktop.First(w=>w.Kind==ViewMode.Support);var p=world.Player;var rep=SupportCatalog.All.First(d=>d.Kind=="Armor"&&d.Size==1);var shield=SupportCatalog.All.First(d=>d.Kind=="Shield"&&d.Size==1);var burst=SupportCatalog.All.First(d=>d.Kind=="Burst"&&d.Family=="Shield");
  switch(stage)
  {
   case 0:Check(world.Docked&&SupportCatalog.All.Length==33,"Support window opens all 33 profiles");int x=window.X;TestDrag(new(window.X+90,window.Y+12),new(window.X+110,window.Y+22));Check(window.X==x+20,"Support title drag");int width=window.Width;TestDrag(new(window.Bounds.Right-5,window.Bounds.Bottom-5),new(window.Bounds.Right+15,window.Bounds.Bottom+5));Check(window.Width==width+20,"Support window resize");break;
   case 1:Click("Support catalog / "+rep.Id);break;
   case 2:Check(supportType==rep.Id,"Catalog selection displays profile");Click("Fit selected support /");break;
   case 3:Check(p.Support[0].ModuleId==rep.Id&&world.ItemCount(rep.Id,true)==2,"Fit control consumes stock");Click("Support activate / 0");break;
   case 4:Check(!p.Support[0].Active,"Docked support activation blocked");Click("FLEET / BROADCASTS");break;
   case 5:Click("Hire wingmate / Escort");break;
   case 6:Check(world.Wingmates.Count==1,"Fleet UI hires persistent escort");Click("MODULES / PAYLOADS");supportFamily=Array.IndexOf(SupportFamilies,"Shield");supportPage=0;break;
   case 7:Click("Support socket / 1");Click("Support catalog / "+burst.Id);break;
   case 8:Click("Fit selected support /");break;
   case 9:Check(p.Support[1].ModuleId==burst.Id,"Burst fits shared high socket");Click("Support payload / "+SupportPayloads.For(burst)[0].Id);break;
   case 10:Check(p.Support[1].Loaded==300&&world.ItemCount(p.Support[1].PayloadId,true)==700,"Pulse charge magazine reserves real inventory");Click("REPEAT: ON");Click("FLEET RANGE");break;
   case 11:Check(world.Grid=="supportrange"&&world.Targets.Any(t=>t.Id==world.Wingmates[0].Id),"Fleet range creates allied body");world.SelectedId=world.Wingmates[0].Id;Click("LOCK SELECTED");Click("PRESSURE SELECTED");Click("Support socket / 0");break;
   case 12:Click("Support activate / 0");break;
   case 13:Check(p.Support[0].PendingAmount>0&&world.LockedId==world.Wingmates[0].Id,"Support UI schedules armor repair on locked ally");Click("Support activate / 0");break;
   case 14:world.UpdateSupport(4);Check(world.Targets.First(t=>t.Id==world.Wingmates[0].Id).Armor>190,"Stop finishes paid ally repair");Click("Support socket / 1");break;
   case 15:Click("Support activate / 1");break;
   case 16:Check(p.SupportEffects.Count>0&&world.Targets.First(t=>t.Id==world.Wingmates[0].Id).Profile!.SupportEffects.Count>0,"Pulse UI applies fleet bonus");Click("ACTIVE BONUSES");break;
   case 17:Check(supportTab==2,"Bonus panel displays live recipients");Click("FLEET / BROADCASTS");break;
   case 18:Click("Fleet order / Hold");break;
   case 19:Check(world.Wingmates[0].Order=="Hold","Fleet order control changes navigation");Click("LEAVE FLEET");break;
   case 20:Check(!world.Wingmates[0].Fleet,"Fleet membership control");Click("MODULES / PAYLOADS");break;
   case 21:TestPointer(new(window.X+window.Width-90,window.Y+405),right:ButtonState.Pressed);TestPointer(new(window.X+window.Width-90,window.Y+405));Check(context&&popupActions.Any(a=>a.Label=="Toggle repeat"),"Support right-click cycle controls");break;
   case 22:Click("Toggle repeat");break;
   case 23:Check(p.Support[1].Repeat,"Support context repeat toggle");p.Support[1].Repeat=false;world.UpdateSupport(70);world.Dock();Check(world.Docked,"Fleet range docks after pulse interlock");Click("Support socket / 2");EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;inventoryScroll=Array.IndexOf(DemoWorld.InventoryIds.Where(id=>world.ItemCount(id,true)>.001f).ToArray(),shield.Id);break;
   case 24:var item=itemHits.First(h=>h.Id==shield.Id);TestDrag(item.Rect.Center.ToVector2(),new(1160,425));break;
   case 25:Check(p.Support[2].ModuleId==shield.Id,"Cross-window inventory drag fits support module");desktop.RemoveAll(w=>w.Kind==ViewMode.Inventory);break;
   case 26:Click("MARKET");break;
   case 27:Check(desktop.Last().Kind==ViewMode.Market&&MarketItem==burst.Id&&world.Offers(burst.Id,world.SystemId,false).Any(),"Support market shortcut selects real offers");desktop.RemoveAll(w=>w.Kind==ViewMode.Market);break;
   case 28:Click("INFO");break;
   case 29:Check(desktop.Last().Kind==ViewMode.Information&&desktop.Last().InfoKey==InformationCatalog.ItemKey(burst.Id),"Support item database shortcut");desktop.RemoveAll(w=>w.Kind==ViewMode.Information);break;
   case 30:world.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.Player.Support[2].ModuleId==shield.Id&&loaded.Wingmates.Count==1&&loaded.Player.Support[1].Loaded==p.Support[1].Loaded,"UI-created fitting, fleet and charge counts save/load");break;
   case 31:supportTab=1;break;
   case 32:Console.WriteLine($"{supportUiChecks} support graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
}
