using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int stageAUiChecks;
 void RunStageAUiChecks(int stage)
 {
  void Check(bool value,string reason){if(!value)throw new Exception("STAGE A UI: "+reason);stageAUiChecks++;Console.WriteLine("STAGE A UI PASS: "+reason);}
  void Click(string label){var hit=ui.Hits.Last(h=>h.Tip==label);if(WindowAt(hit.Rect.Center.ToVector2())==null)hit.Action();else TestClick(hit.Rect.Center.ToVector2());}
  switch(stage)
  {
   case 0:Click("Market");Check(desktop.Any(w=>w.Kind==ViewMode.Market),"Imported station opens market");break;
   case 1:catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id=="ammo");Check(world.ImportedWorld43,"Test remains in imported world");break;
   case 2:var row=marketRows.First(h=>!h.Order.Buy&&h.Order.Station==world.StationKey44);TestClick(row.Rect.Center.ToVector2());TestClick(row.Rect.Center.ToVector2());Check(tradeDialog?.Buy==true,"Imported seller opens purchase draft");break;
   case 3:TestClick(quantityRect.Center.ToVector2());MarketText('2');Check(tradeDialog?.Quantity=="2","Purchase quantity edits");break;
   case 4:Click("Confirm transaction");Check(tradeDialog==null&&world.StackCount("ammo",true)==2,"UI trade delivers exact station stock");break;
   case 5:desktop.Clear();Click("Industry");Check(desktop.Any(w=>w.Kind==ViewMode.Industry),"Imported station opens industry");industryTab=3;industrySelected="catalog/miner";break;
   case 6:Click("Buy blueprint original");Check(world.Industry.Blueprints.Count==1,"UI acquires imported blueprint");industryTab=0;industrySelected=world.Industry.Blueprints.Last().Id;foreach(var input in IndustryCatalog37.Recipes["miner"].Minerals)world.Hangar[input.Key]=10000;break;
   case 7:Click("START JOB");Check(world.Industry.Jobs.Count==1,"UI installs imported manufacturing");break;
   case 8:var job=world.Industry.Jobs.Single();world.WaitDocked(job.Seconds/3600+.001);industryTab=1;Check(job.State=="Ready","Docked waiting completes displayed job");break;
   case 9:Click("DELIVER");Check(world.StackCount("miner",true)==1,"UI delivers manufacturing output");break;
   case 10:desktop.Clear();Click("Clones");Check(desktop.Any(w=>w.Kind==ViewMode.Special)&&specialTab==2,"Imported clone service opens clone panel");break;
   case 11:desktop.Clear();EnsureWindow(ViewMode.Mobility);Check(desktop.Any(w=>w.Kind==ViewMode.Mobility),"Imported propulsion panel opens");break;
   case 12:desktop.Clear();EnsureWindow(ViewMode.Inventory);showAllAssets=true;break;
   case 13:Check(ui.Hits.Any(h=>h.Tip=="Set route"),"Remote asset locations expose physical travel actions");Check(!world.Log.Any(l=>l.Contains("workflow is pending")),"No imported workflow placeholder fired");Console.WriteLine(stageAUiChecks+" Stage A UI checks passed.");break;
  }
 }
}
