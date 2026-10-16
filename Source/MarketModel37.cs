namespace StarlightSyndicateDemo;
public sealed class MarketOrder
{
    public string BusinessC{get;set;}="";
    public string Id{get;set;}=Guid.NewGuid().ToString("N");
    public string Item{get;set;}="ore";
    public string Station{get;set;}="aster";
    public bool Buy{get;set;}
    public bool Player{get;set;}
    public int Range{get;set;}
    public int MinimumVolume{get;set;}=1;
    public int Remaining{get;set;}
    public decimal Price{get;set;}
    public double Created{get;set;}
    public double Expires{get;set;}
}
public sealed record WalletEntry(double Day,string Description,decimal Amount,decimal Balance);
public sealed record MarketTrade(double Day,string Item,string Station,int Quantity,decimal Price);
public sealed partial class DemoWorld
{
    public List<MarketOrder> MarketOrders=new();
    public List<WalletEntry> WalletJournal=new();
    public List<MarketTrade> MarketTrades=new();
    public const decimal BrokerRate=.01m;public decimal SalesTax=>Commander.MarketProfile=="EVE reference"?.075m*(1-.11m*Player.SkillLevel("Accounting")):.02m;
    public double LastMarketTick;
    double MarketExpiryDue;
    public decimal Balance=>Math.Round(WalletFunds,2);
    public void WalletChange(decimal delta,string reason)
    {
        WalletFunds=Math.Round(Balance+delta,2);
        WalletJournal.Add(new(Day,reason,delta,Balance));
        if(WalletJournal.Count>300)WalletJournal.RemoveAt(0);
    }
    public Dictionary<string,decimal> StorageAt(string station)
    {if(!StationItems.TryGetValue(station,out var items))StationItems[station]=items=new();return items;}
    public void InitializeMarket()
    {
        MarketOrders.Clear();MarketTrades.Clear();WalletJournal.Clear();LastMarketTick=Day;MarketExpiryDue=0;
        for(int s=0;s<Systems.Length;s++)for(int i=0;i<ItemCatalog.All.Length;i++)
        {
            var item=ItemCatalog.All[i];if(ProductionCatalogE.Restricted(item.Id))continue;if(item.Slot=="ship"&&ShipBatch21.ById[item.Id].HomeSystem!=Systems[s].Id)continue;decimal ask=Math.Round(item.Price*(1+(s%4)*.055m+(i%3)*.02m),2);
            MarketOrders.Add(new(){Item=item.Id,Station=Systems[s].Id,Buy=false,Remaining=item.Id.StartsWith("ind_")?1000000:item.Id=="ammo"?160: item.Id=="ore"?240:12,Price=ask,Created=Day,Expires=Day+90});
            MarketOrders.Add(new(){Item=item.Id,Station=Systems[s].Id,Buy=true,Remaining=item.Id=="ore"?400: item.Id=="ammo"?90:6,Price=Math.Round(ask*.78m,2),Created=Day,Expires=Day+90});
        }
        WalletJournal.Add(new(Day,"Opening balance",0,Balance));
    }
    public void EnsureWeaponMarketStock()
    {
        foreach(var system in Systems)foreach(var item in ItemCatalog.All)
        {
            if(ProductionCatalogE.Restricted(item.Id))continue;
            if(item.Slot=="ship"&&ShipBatch21.ById[item.Id].HomeSystem!=system.Id)continue;
            if(MarketOrders.Any(o=>o.Station==system.Id&&o.Item==item.Id&&!o.Player))continue;
            MarketOrders.Add(new(){Item=item.Id,Station=system.Id,Buy=false,Remaining=item.Id.StartsWith("ind_")?1000000:item.Slot=="charge"?200:12,Price=item.Price,Created=Day,Expires=Day+90});
            MarketOrders.Add(new(){Item=item.Id,Station=system.Id,Buy=true,Remaining=12,Price=Math.Round(item.Price*.78m,2),Created=Day,Expires=Day+90});
        }
    }
    public int JumpDistance(string from,string to)
    {
        from=LocationSystem(from);to=LocationSystem(to);
        var q=new Queue<(string,int)>();var seen=new HashSet<string>{from};q.Enqueue((from,0));
        while(q.Count>0){var (id,d)=q.Dequeue();if(id==to)return d;foreach(var next in ResolveSystem43(id).Links)if(seen.Add(next))q.Enqueue((next,d+1));}return 999;
    }
    public IEnumerable<MarketOrder> Offers(string item,string station,bool buy){EnsureLocationMarket(station);return MarketOrders.Where(o=>ItemCatalog.IsSupported(item)&&o.Item==item&&o.Station==station&&o.Buy==buy&&o.Remaining>0&&o.Expires>Day&&!o.Player);}
    public string SubmitMarket(string item,string station,bool buy,int quantity,decimal price,int duration,bool fromHangar,int range=0,int minimumVolume=1)
    {
        EnsureLocationMarket(station);if(!LocationService(station,7))return "Market unavailable at this station.";
        if(!ItemCatalog.IsSupported(item))return "This reference item is awaiting gameplay integration and cannot be traded.";
        if(!Docked&&!buy&&!fromHangar)return "Dock before selling ship cargo.";
        if(!ItemCatalog.All.Any(i=>i.Id==item)||!LocationExists(station)||LocationRegion(station)!=Current.Region)return "Station is outside this regional market.";
        if(quantity<1||quantity>100000||price<=0||price>1000000||price!=Math.Round(price,2))return "Enter a positive quantity and a price with at most two decimal places.";
        if(!buy&&HasMutatedStockQ(item,fromHangar))return "Mutated instances require an inspected item-exchange contract.";
        if(!buy&&WeaponStorage.Any(w=>w.State.ModuleId==item&&w.Hangar==fromHangar&&(!fromHangar||w.Station==station)))return "Conditioned weapon instances cannot be sold as repackaged stacks.";
        if(!buy&&(HasDamagedUtilityStock(item,fromHangar)||CrystalConditions.Any(c=>c.Id==item&&c.Hangar==fromHangar&&(!fromHangar||c.System==SystemId))))return "Repair utility modules or consume worn crystals before selling this stack.";
        if(!buy&&HasDamagedMobilityStock(item,fromHangar))return "Repair damaged mobility modules before selling this stack.";
        if(!buy&&HasDamagedEnhancementStock(item,fromHangar))return "Repair damaged upgrade modules before selling this stack.";
        if(!buy&&HasDamagedSupportStock(item,fromHangar))return "Repair damaged support modules before selling this stack.";
        if(!buy&&DefenseStorage.Any(s=>s.State.ModuleId==item&&s.State.MutatedE&&s.Hangar==fromHangar&&(!fromHangar||s.System==StationKey44)))return "Mutated repairers require an inspected item-exchange contract.";
        if(!buy&&HasDamagedDefenseStock(item,fromHangar))return "Repair damaged defense modules before selling this stack.";
        if(!buy&&station!=StationKey44)return "Items can only be sold at their current station.";
        if(range < -1 || range>40 || minimumVolume<1 || minimumVolume>quantity)return "Invalid order range or minimum volume.";
        if(duration is not (0 or 1 or 3 or 7 or 14 or 30 or 90))return "Unsupported duration.";
        var offers=MarketOrders.Where(o=>o.Item==item&&!o.Player&&o.Remaining>0&&o.Expires>Day&&o.Buy!=buy&&LocationRegion(o.Station)==Current.Region)
            .Where(o=>buy? (range==-1||JumpDistance(station,o.Station)<=range)&&o.Price<=price : InBuyRange(o,station)&&o.Price>=price)
            .Where(o=>Math.Min(quantity,o.Remaining)>=Math.Min(o.MinimumVolume,o.Remaining)&&(!buy||Math.Min(quantity,o.Remaining)>=minimumVolume));
        offers=buy?offers.OrderBy(o=>o.Price).ThenBy(o=>o.Created):offers.OrderByDescending(o=>o.Price).ThenBy(o=>o.Created);
        var matches=offers.ToArray();int supply=matches.Sum(o=>o.Remaining);
        if(duration==0&&supply<quantity)return $"Only {supply} units can fill immediately at this price.";
        decimal gross=quantity*price,fee=duration>0?Math.Round(gross*BrokerRateAt38(station),2):0;
        if(buy&&Balance<gross+fee)return "Insufficient wallet balance including escrow and broker fee.";
        if(!buy&&(StackCount(item,fromHangar)<quantity||Balance<fee))return "Insufficient items or balance for the listing fee.";
        var order=new MarketOrder{Item=item,Station=station,Buy=buy,Player=true,Remaining=quantity,Price=price,Range=buy?range:0,MinimumVolume=minimumVolume,Created=Day,Expires=Day+duration};
        if(buy)WalletChange(-gross,"Buy escrow / "+ItemCatalog.Get(item).Name);
        else ChangeStack(item,fromHangar,-(decimal)(quantity));
        if(fee>0)WalletChange(-fee,"Broker fee / "+ItemCatalog.Get(item).Name);
        foreach(var match in matches)
        {
            int q=Math.Min(order.Remaining,match.Remaining);if(q<=0)break;
            CompleteBusinessTradeC(match,q,price);FillPlayerOrder(order,q,price,buy?match.Station:station);match.Remaining-=q;
        }
        if(order.Remaining>0&&duration>0){MarketOrders.Add(order);MarketExpiryDue=Math.Min(MarketExpiryDue,order.Expires);}
        MarketOrders.RemoveAll(o=>o.Remaining<=0);
        string result=order.Remaining>0?$"Filled {quantity-order.Remaining}; listed {order.Remaining} units.":$"Trade complete: {quantity} units at {price:0.00} CR. Delivery: {LocationName(station)} item hangar.";
        Say(result);return result;
    }
    public bool InBuyRange(MarketOrder order,string station)=>LocationRegion(station)==LocationRegion(order.Station)&&(order.Range==-1||JumpDistance(order.Station,station)<=order.Range);
    void FillPlayerOrder(MarketOrder order,int q,decimal price,string? delivery=null)
    {
        if(order.Buy)
        {
            var h=StorageAt(delivery??order.Station);h[order.Item]=h.GetValueOrDefault(order.Item)+q;
            WalletJournal.Add(new(Day,$"Purchase delivered / {ItemCatalog.Get(order.Item).Name} x{q}",0,Balance));
        }
        else WalletChange(Math.Round(q*price*(1-SalesTax),2),$"Sale / {ItemCatalog.Get(order.Item).Name} x{q} (tax {SalesTax:P0})");
        order.Remaining-=q;MarketTrades.Add(new(Day,order.Item,delivery??order.Station,q,price));
        if(MarketTrades.Count>500)MarketTrades.RemoveAt(0);
    }
    public void CancelMarketOrder(string id)
    {
        var o=MarketOrders.FirstOrDefault(o=>o.Id==id&&o.Player);if(o==null)return;
        if(o.Buy)WalletChange(o.Remaining*o.Price,"Buy escrow returned");
        else {var h=StorageAt(o.Station);h[o.Item]=h.GetValueOrDefault(o.Item)+o.Remaining;}
        MarketOrders.Remove(o);Say("Order cancelled. Unfilled assets returned; broker fees are not refunded.");
    }
    public string ModifyMarketOrder(string id,decimal price)
    {
        var o=MarketOrders.FirstOrDefault(o=>o.Id==id&&o.Player);if(o==null)return "Order is no longer open.";
        if(!ItemCatalog.IsSupported(o.Item))return "This reference item is awaiting gameplay integration and cannot be traded.";
        if(price<=0||price>1000000||price!=Math.Round(price,2))return "Invalid unit price.";
        decimal fee=Math.Round(o.Remaining*(Math.Max(0,BrokerRateAt38(o.Station)*(price-o.Price))+(1-(.5m+.06m*Player.SkillLevel("Advanced Broker Relations")))*BrokerRateAt38(o.Station)*price),2),adjust=o.Buy?o.Remaining*(price-o.Price):0;
        if(Balance<fee+adjust)return "Insufficient balance for the new escrow and relisting fee.";
        if(adjust!=0)WalletChange(-adjust,"Buy escrow adjusted");WalletChange(-fee,"Relisting fee");o.Price=price;o.Created=Day;
        MatchStandingOrders(o.Id);return "Order price updated.";
    }
    void MatchStandingOrders(string incoming="")
    {
        foreach(var o in MarketOrders.Where(o=>o.Player&&ItemCatalog.IsSupported(o.Item)&&o.Expires>Day).OrderBy(o=>o.Item).ThenBy(o=>o.Buy).ThenBy(o=>o.Buy?-o.Price:o.Price).ThenBy(o=>o.Created).ToArray())
        {
            var candidates=MarketOrders.Where(n=>!n.Player&&n.Item==o.Item&&n.Buy!=o.Buy&&n.Remaining>0&&n.Expires>Day).Where(n=>o.Buy?InBuyRange(o,n.Station)&&n.Price<=o.Price:InBuyRange(n,o.Station)&&n.Price>=o.Price).Where(n=>Math.Min(n.Remaining,o.Remaining)>=Math.Min(o.MinimumVolume,o.Remaining)&&Math.Min(n.Remaining,o.Remaining)>=Math.Min(n.MinimumVolume,n.Remaining));
            candidates=o.Buy?candidates.OrderBy(n=>n.Price).ThenBy(n=>n.Created):candidates.OrderByDescending(n=>n.Price).ThenBy(n=>n.Created);
            foreach(var n in candidates.ToArray())
            {int q=Math.Min(n.Remaining,o.Remaining);if(q<=0)break;decimal execution=incoming==o.Id?o.Price:n.Price;if(o.Buy&&execution<o.Price)WalletChange(q*(o.Price-execution),"Buy escrow price improvement");CompleteBusinessTradeC(n,q,execution);FillPlayerOrder(o,q,execution,o.Buy?n.Station:o.Station);n.Remaining-=q;}
        }
        MarketOrders.RemoveAll(o=>o.Remaining<=0);
    }
    public void AdvanceMarket()
    {
        if(Day<MarketExpiryDue&&Day-LastMarketTick<.5)return;
        foreach(var o in MarketOrders.Where(o=>o.Expires<=Day).ToArray())
        {if(o.Player)CancelMarketOrder(o.Id);else if(o.BusinessC.Length>0)CancelBusinessOrdersC(Population.Businesses.Single(b=>b.Id==o.BusinessC));else if(ImportedWorld43&&o.Remaining>0){o.Created=Day;o.Expires=Day+90;}else MarketOrders.Remove(o);}
        MarketExpiryDue=MarketOrders.Count==0?double.PositiveInfinity:MarketOrders.Min(o=>o.Expires);
        if(Day-LastMarketTick<.5)return;
        LastMarketTick=Day;MatchStandingOrders();
        if(ImportedWorld43)return;
        // Frontier dealers fill competitively priced player orders in small batches.
        foreach(var o in MarketOrders.Where(o=>o.Player&&ItemCatalog.IsSupported(o.Item)&&(!Population.Enabled||!Population.Sectors.Values.Any(s=>s.Station==o.Station))).ToArray())
        {
            decimal reference=ItemCatalog.Get(o.Item).Price;
            if(o.Buy?o.Price>=reference*.9m:o.Price<=reference*1.1m)
                FillPlayerOrder(o,Math.Min(o.Remaining,Math.Max(o.MinimumVolume,o.Item=="ammo"||o.Item=="ore"?6:1)),o.Price);
        }
        MarketOrders.RemoveAll(o=>o.Remaining<=0);
    }
}
