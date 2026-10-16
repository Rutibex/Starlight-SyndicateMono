using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunMarketChecks(int stage)
 {
  var w=desktop.Single(w=>w.Kind==ViewMode.Market);
  void ClickLabel(string label){var h=ui.Hits.Last(h=>h.Tip==label);TestClick(h.Rect.Center.ToVector2());}
  switch(stage)
  {
   case 0:TestClick(searchRect.Center.ToVector2());foreach(char c in "Needle")MarketText(c);UiCheck(marketQuery=="Needle","Market search accepts text");break;
   case 1:UiCheck(ui.Hits.Any(h=>h.Tip=="Needle missile")&&!ui.Hits.Any(h=>h.Tip=="Vesperite ore"),"Search filters catalog results");ClickLabel("Needle missile");marketQuery="";marketTextFocus="";break;
   case 2:TestClick(marketRows.First(h=>!h.Order.Buy&&h.Order.Station==world.SystemId).Rect.Center.ToVector2());TestClick(marketRows.First(h=>!h.Order.Buy&&h.Order.Station==world.SystemId).Rect.Center.ToVector2());UiCheck(tradeDialog?.Item=="ammo"&&tradeDialog.Buy,"Seller row opens purchase dialog");break;
   case 3:TestClick(quantityRect.Center.ToVector2());MarketText('2');UiCheck(tradeDialog?.Quantity=="2","Quantity edit replaces selected text");break;
   case 4:ClickLabel("Confirm transaction");UiCheck(tradeDialog==null&&world.ItemCount("ammo",true)==14,"Confirmation purchases into station inventory");break;
   case 5:ClickLabel("Place buy order");UiCheck(tradeDialog?.Duration==3,"Listing button opens timed order draft");break;
   case 6:TestClick(priceRect.Center.ToVector2());MarketText('1');break;
   case 7:ClickLabel("Confirm transaction");UiCheck(world.MarketOrders.Any(o=>o.Player&&o.Buy&&o.Price==1),"Standing order reserves real escrow");marketTab=2;break;
   case 8:ClickLabel("Cancel");UiCheck(!world.MarketOrders.Any(o=>o.Player),"Order cancellation returns escrow through UI");marketTab=0;break;
   case 9:var row=marketRows.First();TestPointer(row.Rect.Center.ToVector2(),right:ButtonState.Pressed);TestPointer(row.Rect.Center.ToVector2());UiCheck(context&&popupActions.Any(a=>a.Label.StartsWith("Buy at")),"Market right-click exposes trade actions");context=false;break;
   case 10:ClickLabel("Favorite");UiCheck(marketFavorites.Contains(MarketItem),"Favorite button adds selected item to Quickbar");break;
   case 11:ClickLabel("Quickbar");break;
   case 12:UiCheck(ui.Hits.Any(h=>h.Tip=="Needle missile")&&!ui.Hits.Any(h=>h.Tip=="Focus combat booster"),"Quickbar shows only saved favorites");ClickLabel("Browse");break;
   case 13:ClickLabel("Ships");UiCheck(marketGroupsView&&marketGroupPath=="Ships","Category click opens group summaries");Console.WriteLine("12 market UI interaction checks passed.");break;
  }
  oldMouse=Mouse.GetState();
 }
}
