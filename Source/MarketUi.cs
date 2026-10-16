using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed class TradeDraft
{
    public string Item="ore",Station="aster",Quantity="1",Price="1.00",ModifyId="",Minimum="1";
    public bool Buy,Hangar=true;
    public int Duration,Range; public int MinimumVolume=1;
}
public sealed partial class DemoGame
{
    TradeDraft? tradeDialog;
    readonly List<(Rectangle Rect,MarketOrder Order)> marketRows=[];
    readonly HashSet<string> marketFavorites=new();
    string marketQuery="",marketTextFocus="",selectedMarketOrder="",marketNotice="";
    bool selectText,marketLocal;
    int marketTab,askScroll,bidScroll;
    Rectangle searchRect,quantityRect,priceRect;
    HitArea? dialogPressed;
    readonly int[] durations=[0,1,3,7,14,30,90];
    readonly string[] durationNames=["Immediate","1 day","3 days","7 days","14 days","30 days","90 days"];
    readonly Rectangle tradeBounds=new(535,225,535,420);
    string MarketItem=>ItemCatalog.All[Math.Clamp(catalogSelection,0,ItemCatalog.All.Length-1)].Id;
    void MarketText(char c)
    {
        if(marketTextFocus.Length==0)return;
        string value=marketTextFocus=="search"?marketQuery:marketTextFocus=="qty"?tradeDialog?.Quantity??"":marketTextFocus=="min"?tradeDialog?.Minimum??"":tradeDialog?.Price??"";
        if(c=='\b'){value=selectText?"":value.Length>0?value[..^1]:"";selectText=false;}
        else if(!char.IsControl(c)&&((marketTextFocus=="search"&&value.Length<45)||(char.IsDigit(c)&&value.Length<10)||(marketTextFocus=="price"&&c=='.'&&!value.Contains('.'))))
        {if(selectText)value="";value+=c;selectText=false;}
        if(marketTextFocus=="search")marketQuery=value;
        else if(tradeDialog!=null){if(marketTextFocus=="qty")tradeDialog.Quantity=value;else if(marketTextFocus=="min")tradeDialog.Minimum=value;else tradeDialog.Price=value;}
    }
    void MarketKeyboard(KeyboardState k)
    {
        if(marketTextFocus.Length==0)return;
        if((k.IsKeyDown(Keys.LeftControl)||k.IsKeyDown(Keys.RightControl))&&Press(Keys.A,k))selectText=true;
        if(Press(Keys.Enter,k))marketTextFocus="";
        if(Press(Keys.Tab,k)&&tradeDialog!=null){marketTextFocus=marketTextFocus=="qty"?"price":"qty";selectText=true;}
    }
    bool HandleMarketInput(MouseState m)
    {
        bool down=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released;
        bool up=m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed;
        bool right=m.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released;
        if(tradeDialog!=null)
        {
            if(down)
            {
                if(tradeDialog.ModifyId.Length==0&&quantityRect.Contains(mouse.ToPoint())){marketTextFocus="qty";selectText=true;}
                else if(priceRect.Contains(mouse.ToPoint())){marketTextFocus="price";selectText=true;}
                else {marketTextFocus="";dialogPressed=ui.Hits.LastOrDefault(h=>h.Owner=="trade"&&h.Rect.Contains(mouse.ToPoint()));}
            }
            if(up&&dialogPressed!=null){var h=dialogPressed;dialogPressed=null;if(h.Rect.Contains(mouse.ToPoint()))h.Action();}
            return true;
        }
        var top=WindowAt(mouse);
        if(down)
        {
            marketTextFocus=top?.Kind==ViewMode.Market&&searchRect.Contains(mouse.ToPoint())?"search":"";
            if(marketTextFocus=="search"){selectText=false;return true;}
        }
        if(top?.Kind==ViewMode.Market&&m.ScrollWheelValue!=oldMouse.ScrollWheelValue)
        {
            int delta=(m.ScrollWheelValue-oldMouse.ScrollWheelValue)/120;
            if(marketTreeRect.Contains(mouse.ToPoint()))marketTreeScroll=Math.Max(0,marketTreeScroll-delta*3);else if(marketGroupsView)marketGroupScroll=Math.Max(0,marketGroupScroll-delta);else if(marketSellersRect.Contains(mouse.ToPoint()))askScroll=Math.Max(0,askScroll-delta);else if(marketBuyersRect.Contains(mouse.ToPoint()))bidScroll=Math.Max(0,bidScroll-delta);
        }
        if(right&&top?.Kind==ViewMode.Market)
        {
            var hit=marketRows.LastOrDefault(h=>h.Rect.Contains(mouse.ToPoint()));
            if(hit.Order!=null)
            {
                var o=hit.Order;
                var actions=new List<(string,Action)>{("Show item information",()=>MarketShowInfo(Array.FindIndex(ItemCatalog.All,i=>i.Id==o.Item))),("Set destination / inspect map",()=>{if(world.ImportedWorld43){world.SetDestination(world.LocationSystem(o.Station));}else mapSelection=Array.FindIndex(DemoWorld.Systems,s=>s.Id==o.Station);EnsureWindow(ViewMode.Map);})};
                if(o.Player){actions.Add(("Modify order price",()=>OpenTrade(o,true)));actions.Add(("Cancel order",()=>world.CancelMarketOrder(o.Id)));}
                else actions.Insert(0,(o.Buy?"Sell to this station's buyers":"Buy at this station",()=>OpenTrade(o)));
                ShowPopup(ItemCatalog.Get(o.Item).Name,actions);return true;
            }
        }
        return false;
    }
    void OpenTrade(MarketOrder o,bool modify=false)
    {
        if(o.Buy&&!world.Docked&&world.ItemCount(o.Item,true)<1){marketNotice="Dock to sell cargo, or use items stored at this station.";return;}
        tradeDialog=new(){Item=o.Item,Station=!modify&&o.Buy?world.StationKey44:o.Station,Range=modify?o.Range:0,MinimumVolume=modify?o.MinimumVolume:1,Minimum=(modify?o.MinimumVolume:1).ToString(),Buy=modify?o.Buy:!o.Buy,Price=o.Price.ToString("0.00",CultureInfo.InvariantCulture),Quantity=modify?o.Remaining.ToString():"1",ModifyId=modify?o.Id:"",Hangar=world.ItemCount(o.Item,true)>0};
        marketTextFocus="qty";selectText=true;marketNotice="";context=false;
        if(modify){marketTextFocus="price";selectText=true;}
    }
    void NewMarketOrder(bool buy)
    {
        if(!buy&&!world.Docked&&world.ItemCount(MarketItem,true)<1){marketNotice="Dock to sell cargo, or use station hangar stock.";return;}
        tradeDialog=new(){Item=MarketItem,Station=world.StationKey44,Buy=buy,Price=ItemCatalog.Get(MarketItem).Price.ToString("0.00",CultureInfo.InvariantCulture),Duration=3,Hangar=world.ItemCount(MarketItem,true)>0};
        marketTextFocus="qty";selectText=true;marketNotice="";
    }
    void ConfirmTrade()
    {
        var t=tradeDialog;if(t==null)return;
        if(!int.TryParse(t.Minimum,out t.MinimumVolume)||!int.TryParse(t.Quantity,out int q)||!decimal.TryParse(t.Price,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out decimal p)){marketNotice="Enter a valid whole quantity and unit price.";return;}
        string result=t.ModifyId.Length>0?world.ModifyMarketOrder(t.ModifyId,p):world.SubmitMarket(t.Item,t.Station,t.Buy,q,p,durations[t.Duration],t.Hangar,t.Range,t.MinimumVolume);
        marketNotice=result;
        if(result.StartsWith("Trade complete")||result.StartsWith("Filled")||result.StartsWith("Order price updated")){tradeDialog=null;marketTextFocus="";}
    }
    void DrawMarketWindow(DesktopWindow w)
    {
        if(world.Docked)world.EnsureLocationMarket(world.StationKey44);
        marketRows.Clear();var r=w.Bounds;int sidebar=285,x=r.X+sidebar+15,y=r.Y+42,width=r.Width-sidebar-30;
        ui.Box(new(r.X+sidebar+1,r.Y+29,r.Width-sidebar-2,r.Height-30),new Color(10,12,14));
        DrawMarketBrowser(r,sidebar);
        var def=ItemCatalog.Get(MarketItem);
        ui.Button(new(x,y,86,24),"Details",()=>marketGroupsView=false,active:!marketGroupsView);ui.Button(new(x+91,y,86,24),"Groups",()=>{marketGroupsView=true;if(marketGroupPath.Length==0)marketGroupPath=MarketGroups.PathFor(def);},active:marketGroupsView);
        if(marketGroupsView){ui.Text(marketGroupPath,x,y+40,Cyan,14);}
        else{
        DrawMarketIcon(def,new(x,y+29,48,48));ui.Text(def.Name,x+60,y+29,White,18);ui.Text(def.Group+"  /  "+def.Volume.ToString("0.##")+" m3 per unit",x+60,y+53,Muted,11);
        ui.Button(new(r.Right-121,y,105,28),marketFavorites.Contains(def.Id)?"Unfavorite":"Favorite",()=>ToggleMarketFavorite(def.Id));
        ui.Text(world.Current.Region+" regional market",x,y+80,Cyan,11);
        ui.Button(new(r.Right-172,y+33,156,24),"Show information",()=>MarketShowInfo(catalogSelection));
        string[] tabs=["Market data","Price history","My orders","Wallet"];
        for(int i=0;i<tabs.Length;i++){int tab=i;ui.Button(new(x+i*132,y+103,125,27),tabs[i],()=>marketTab=tab,active:marketTab==i);}
        }
        int body=y+(marketGroupsView?72:141),bottom=r.Bottom-100;
        if(marketGroupsView)DrawMarketGroupCards(new(x,body,width,bottom-body));
        else if(marketTab==0)
        {
            int tableH=(bottom-body-30)/2;
            marketSellersRect=new(x,body,width,tableH);DrawOrderTable(marketSellersRect,false,ref askScroll);
            marketBuyersRect=new(x,body+tableH+20,width,tableH);DrawOrderTable(marketBuyersRect,true,ref bidScroll);
        }
        else if(marketTab==1)DrawMarketHistory(new(x,body,width,bottom-body));
        else if(marketTab==2)DrawOwnOrders(new(x,body,width,bottom-body));
        else DrawWallet(new(x,body,width,bottom-body));
        ui.Text($"Wallet {world.Balance:N2} CR",x,r.Bottom-78,Gold,15);
        if(!marketGroupsView&&def.Slot=="ship")ui.Button(new(x+220,r.Bottom-85,150,31),"Assemble hull",()=>AssembleMarketHull(def.Id));
        if(!marketGroupsView)ui.Button(new(r.Right-327,r.Bottom-85,147,31),"Place buy order",()=>NewMarketOrder(true));
        if(!marketGroupsView)ui.Button(new(r.Right-172,r.Bottom-85,156,31),"Place sell order",()=>NewMarketOrder(false));
        ui.Wrap(marketNotice.Length>0?marketNotice:"Items bought here are delivered to the seller's station hangar. Right-click an order for actions.",x,r.Bottom-43,width,marketNotice.Length>0?Gold:Muted,11);
    }
    string lastMarketClick="";DateTime lastMarketClickTime;
    int askSort=2,bidSort=2;bool askReverse,bidReverse;
    void DrawOrderTable(Rectangle r,bool buy,ref int scroll)
    {
        ui.Text(buy?"Buyers":"Sellers",r.X,r.Y,White,16);
        var orders=world.MarketOrders.Where(o=>o.Item==MarketItem&&o.Buy==buy&&o.Remaining>0&&o.Expires>world.Day&&world.LocationRegion(o.Station)==world.Current.Region&&(marketScope==0||marketScope==1&&world.LocationSystem(o.Station)==world.SystemId||marketScope==2&&o.Station==world.StationKey44));
        int sort=buy?bidSort:askSort;bool reverse=buy?bidReverse:askReverse;
        orders=sort switch{0=>orders.OrderBy(o=>world.JumpDistance(world.StationKey44,o.Station)),1=>orders.OrderBy(o=>o.Remaining),2=>buy?orders.OrderByDescending(o=>o.Price):orders.OrderBy(o=>o.Price),3=>orders.OrderBy(o=>o.Station),4=>orders.OrderBy(o=>o.Range),5=>orders.OrderBy(o=>o.MinimumVolume),_=>orders.OrderBy(o=>o.Expires)};
        if(reverse)orders=orders.Reverse();var all=orders.ToArray();int visible=Math.Max(1,(r.Height-49)/24);scroll=Math.Clamp(scroll,0,Math.Max(0,all.Length-visible));
        float[] fractions=buy?[0,.065f,.145f,.33f,.69f,.79f,.88f,1]:[0,.065f,.145f,.33f,.85f,1];
        string[] headers=buy?["Jumps","Quantity","Price / CR","Location","Range","Min vol","Expires"]:["Jumps","Quantity","Price / CR","Location","Expires"];
        for(int i=0;i<headers.Length;i++){int ix=i==4&&!buy?6:i;int left=(int)(fractions[i]*r.Width),cw=(int)((fractions[i+1]-fractions[i])*r.Width);ui.Button(new(r.X+left,r.Y+22,cw-2,21),headers[i]+(sort==ix?(reverse?" v":" ^"):""),()=>{if(buy){bidReverse=bidSort==ix?!bidReverse:false;bidSort=ix;bidScroll=0;}else{askReverse=askSort==ix?!askReverse:false;askSort=ix;askScroll=0;}});}
        for(int i=0;i<visible&&i+scroll<all.Length;i++)
        {
            var o=all[i+scroll];int yy=r.Y+47+i*24;var rect=new Rectangle(r.X,yy,r.Width,22);bool reachable=buy&&world.InBuyRange(o,world.StationKey44);
            ui.Box(rect,selectedMarketOrder==o.Id?new Color(42,59,64):reachable?new Color(47,61,39):i%2==0?new Color(17,18,20):new Color(22,23,25));
            string[] cells=buy?[world.JumpDistance(world.StationKey44,o.Station).ToString(),o.Remaining.ToString("N0"),o.Price.ToString("N2"),world.LocationName(o.Station)+" - Anchorage",o.Range==-1?"Region":o.Range==0?"Station":o.Range+" jumps",o.MinimumVolume.ToString(),$"{Math.Max(0,o.Expires-world.Day):0.0}d"]:[world.JumpDistance(world.StationKey44,o.Station).ToString(),o.Remaining.ToString("N0"),o.Price.ToString("N2"),world.LocationName(o.Station)+" - Anchorage",$"{Math.Max(0,o.Expires-world.Day):0.0}d"];
            for(int c=0;c<cells.Length;c++){int left=(int)(fractions[c]*r.Width),cw=(int)((fractions[c+1]-fractions[c])*r.Width);string value=cells[c];int max=Math.Max(4,(cw-8)/6);if(value.Length>max)value=value[..Math.Max(1,max-3)]+"...";ui.Text(value,r.X+left+4,yy+5,c==0&&reachable?Green:White,11);}
            ui.Hits.Add(new(rect,()=>{selectedMarketOrder=o.Id;var now=DateTime.UtcNow;if(lastMarketClick==o.Id&&(now-lastMarketClickTime).TotalMilliseconds<450)OpenTrade(o,o.Player);lastMarketClick=o.Id;lastMarketClickTime=now;},"Double-click to "+(buy?"sell":"buy")+" at "+world.LocationName(o.Station)));marketRows.Add((rect,o));
        }
        if(all.Length==0)ui.Text("No matching orders",r.X+8,r.Y+52,Muted,12);
        if(all.Length>visible)ui.Text($"{scroll+1}-{Math.Min(all.Length,scroll+visible)} / {all.Length}",r.Right-110,r.Y,Muted,10);
    }
    void DrawOwnOrders(Rectangle r)
    {
        ui.Text("TYPE      ITEM                         QTY     UNIT PRICE    STATION",r.X,r.Y,Muted,11);
        int row=0;
        foreach(var o in world.MarketOrders.Where(o=>o.Player).Take(Math.Max(1,(r.Height-55)/35)))
        {
            int y=r.Y+26+row++*35;var rect=new Rectangle(r.X,y,r.Width,32);ui.Box(rect,new Color(25,38,45));
            ui.Text(o.Buy?"BUY":"SELL",r.X+6,y+8,o.Buy?Green:Gold,11);string name=ItemCatalog.Get(o.Item).Name;ui.Text(name.Length>29?name[..26]+"...":name,r.X+55,y+8,White,11);
            ui.Text($"{o.Remaining}    {o.Price:N2} CR",r.X+267,y+8,White,11);ui.Text(world.LocationName(o.Station),r.X+423,y+8,Muted,11);
            ui.Button(new(r.Right-156,y+3,73,25),"Modify",()=>OpenTrade(o,true));ui.Button(new(r.Right-77,y+3,73,25),"Cancel",()=>world.CancelMarketOrder(o.Id));marketRows.Add((rect,o));
        }
        if(row==0)ui.Text("No active player orders. Buy orders reserve credits; sell orders reserve items.",r.X,r.Y+45,Muted,12);
        ui.Text($"Escrow {world.MarketOrders.Where(o=>o.Player&&o.Buy).Sum(o=>o.Price*o.Remaining):N2} CR",r.X,r.Bottom-25,Gold,12);
        ui.Button(new(r.Right-218,r.Bottom-33,210,29),"Wait 12 campaign hours",()=>world.AdvanceHours(12),"Advances the JoyOS deadline and lets frontier dealers trade");
    }
    void DrawWallet(Rectangle r)
    {
        ui.Text("DAY       TRANSACTION                                      CHANGE / CR",r.X,r.Y,Muted,11);
        int y=r.Y+28;
        foreach(var e in world.WalletJournal.TakeLast(Math.Max(1,(r.Height-30)/27)).Reverse())
        {ui.Text(e.Day.ToString("0.00"),r.X+5,y,Muted,11);ui.Text(e.Description.Length>57?e.Description[..57]:e.Description,r.X+65,y,White,11);ui.Text(e.Amount.ToString("+0.00;-0.00;0.00"),r.Right-114,y,e.Amount>=0?Green:Red,12);y+=27;}
    }
    void DrawMarketHistory(Rectangle r)
    {
        var days=world.MarketTrades.Where(t=>t.Item==MarketItem&&world.LocationRegion(t.Station)==world.Current.Region).GroupBy(t=>(int)t.Day).OrderBy(g=>g.Key).TakeLast(90).ToArray();
        ui.Text("Price history / completed trades",r.X,r.Y,Cyan,12);
        ui.Button(new(r.Right-130,r.Y,128,24),marketHistoryGraph?"Show table":"Show graph",()=>marketHistoryGraph=!marketHistoryGraph);
        if(days.Length==0){ui.Wrap("No recorded trades for this item yet. Completed trades build regional price history.",r.X,r.Y+40,r.Width,Muted,14);return;}
        if(!marketHistoryGraph){ui.Text("DAY       LOW          HIGH         AVERAGE       VOLUME",r.X,r.Y+30,Muted,11);int y=r.Y+56;foreach(var g in days.TakeLast(Math.Max(1,(r.Height-65)/28))){decimal avg=g.Sum(t=>t.Price*t.Quantity)/g.Sum(t=>t.Quantity);ui.Text($"{g.Key+1,-9}{g.Min((MarketTrade t)=>t.Price),-13:0.00}{g.Max((MarketTrade t)=>t.Price),-13:0.00}{avg,-16:0.00}{g.Sum((MarketTrade t)=>t.Quantity)}",r.X+4,y,White,12);y+=28;}return;}
        var chart=new Rectangle(r.X+65,r.Y+43,r.Width-82,r.Height-100);decimal min=days.Min(g=>g.Min(t=>t.Price)),max=days.Max(g=>g.Max(t=>t.Price));if(max==min){max+=1;min=Math.Max(0,min-1);}int volMax=days.Max(g=>g.Sum(t=>t.Quantity));
        for(int grid=0;grid<=4;grid++){int yy=chart.Y+grid*chart.Height/4;ui.Line(new(chart.X,yy),new(chart.Right,yy),new Color(41,44,47));ui.Text((max-(max-min)*grid/4).ToString("N2"),r.X,yy-5,Muted,10);}
        Vector2? prev=null;for(int i=0;i<days.Length;i++){var g=days[i];int xx=chart.X+(days.Length==1?chart.Width/2:(g.Key-days[0].Key)*chart.Width/Math.Max(1,days[^1].Key-days[0].Key));decimal avg=g.Sum(t=>t.Price*t.Quantity)/g.Sum(t=>t.Quantity);float yy=chart.Bottom-(float)((avg-min)/(max-min))*chart.Height;int bar=(int)(g.Sum(t=>t.Quantity)/(float)volMax*chart.Height*.25f);ui.Box(new(xx-3,chart.Bottom-bar,6,bar),new Color(45,77,87));ui.Line(new(xx,chart.Bottom-(float)((g.Max(t=>t.Price)-min)/(max-min))*chart.Height),new(xx,chart.Bottom-(float)((g.Min(t=>t.Price)-min)/(max-min))*chart.Height),Gold,2);var point=new Vector2(xx,yy);if(prev.HasValue)ui.Line(prev.Value,point,Green,2);ui.Box(new(xx-2,(int)yy-2,4,4),Green);prev=point;}
        ui.Text($"Day {days[0].Key+1} - {days[^1].Key+1}    Average (green) / Low-high (gold) / Volume (blue)",r.X+65,r.Bottom-32,Muted,11);
    }
    void DrawTradeDialog()
    {
        var t=tradeDialog;if(t==null)return;
        ui.Box(new(54,0,1546,900),new Color(0,0,0,120));ui.Panel(tradeBounds,t.ModifyId.Length>0?"Modify order":t.Buy?"Buy / place order":"Sell / place order");
        int x=tradeBounds.X+22,y=tradeBounds.Y+46;ui.Text(ItemCatalog.Get(t.Item).Name,x,y,White,20);
        ui.Text("Delivery: "+world.LocationName(t.Station)+" Anchorage",x,y+32,Cyan,13);
        quantityRect=new(x+128,y+68,325,32);priceRect=new(x+128,y+111,325,32);
        ui.Text("Quantity",x,y+78,Muted,13);ui.Text("Unit price / CR",x,y+121,Muted,13);
        ui.Box(quantityRect,new Color(7,17,24));ui.Border(quantityRect,marketTextFocus=="qty"?Cyan:Muted);ui.Text(t.Quantity,quantityRect.X+9,quantityRect.Y+8,White,14);
        ui.Box(priceRect,new Color(7,17,24));ui.Border(priceRect,marketTextFocus=="price"?Cyan:Muted);ui.Text(t.Price,priceRect.X+9,priceRect.Y+8,White,14);
        void Button(Rectangle r,string label,Action action){ui.Button(r,label,action);ui.Hits[^1]=ui.Hits[^1] with {Owner="trade"};}
        if(t.ModifyId.Length==0)
        {
            Button(new(x,y+161,220,31),"Duration: "+durationNames[t.Duration],()=>t.Duration=(t.Duration+1)%durations.Length);
            if(!t.Buy)Button(new(x+232,y+161,221,31),t.Hangar?"Source: item hangar":"Source: ship cargo",()=>t.Hangar=!t.Hangar);
        }
        if(t.Buy&&t.ModifyId.Length==0){Button(new(x,y+197,220,25),"Order range: "+(t.Range==-1?"Region":t.Range==0?"Station":t.Range+" jumps"),()=>{int[] ranges=[0,1,2,5,10,20,40,-1];t.Range=ranges[(Array.IndexOf(ranges,t.Range)+1)%ranges.Length];});Button(new(x+232,y+197,221,25),"Min volume: "+t.Minimum,()=>{marketTextFocus="min";selectText=true;});}
        decimal.TryParse(t.Price,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var p);int.TryParse(t.Quantity,out var q);
        decimal oldPrice=world.MarketOrders.FirstOrDefault(o=>o.Id==t.ModifyId)?.Price??p;
        decimal gross=p*q,fee=t.ModifyId.Length>0?Math.Round(q*(Math.Max(0,world.BrokerRateAt38(t.Station)*(p-oldPrice))+(1-(.5m+.06m*world.Player.SkillLevel("Advanced Broker Relations")))*world.BrokerRateAt38(t.Station)*p),2):t.Duration>0?Math.Round(gross*world.BrokerRateAt38(t.Station),2):0,tax=t.Buy?0:Math.Round(gross*world.SalesTax,2);
        ui.Text($"Gross {gross:N2} CR   |   Broker fee {fee:N2}   |   Sales tax {tax:N2}",x,y+229,Gold,12);
        decimal required=t.ModifyId.Length>0?fee+(t.Buy?q*(p-(world.MarketOrders.FirstOrDefault(o=>o.Id==t.ModifyId)?.Price??p)):0):t.Buy?gross+fee:gross-fee-tax;
        ui.Text($"{(t.ModifyId.Length>0?"Due now":t.Buy?"Required":"Net proceeds")} {required:N2} CR   /   Wallet {world.Balance:N2} CR",x,y+251,White,13);
        ui.Wrap(marketNotice.Length>0?marketNotice:"Broker matches the best compatible orders at this station. Unfilled listed quantities remain in escrow.",x,y+276,453,marketNotice.Length>0?Red:Muted,11);
        Button(new(x,y+323,221,34),"Cancel",()=>{tradeDialog=null;marketTextFocus="";});Button(new(x+232,y+323,221,34),t.ModifyId.Length>0?"Update order":"Confirm transaction",ConfirmTrade);
    }
}
