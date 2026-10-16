namespace StarlightSyndicateDemo;
internal static class MarketTests
{
 public static void Run()
 {
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception("MARKET FAIL: "+label);checks++;Console.WriteLine("MARKET PASS: "+label);}
  DemoWorld Fresh(){var w=new DemoWorld();w.NewGame(0);w.Credits=10000;return w;}
  var w=Fresh();decimal start=w.Balance;int count=w.MarketOrders.Count;
  w.SubmitMarket("ammo",w.SystemId,true,-1,10,0,true);Check(w.Balance==start&&w.MarketOrders.Count==count,"Invalid quantity is atomic");
  var ask=w.Offers("ammo",w.SystemId,false).First();int supply=ask.Remaining;decimal price=ask.Price;float stock=w.ItemCount("ammo",true);
  w.SubmitMarket("ammo",w.SystemId,true,2,price,0,true);Check(w.Balance==start-2*price&&w.ItemCount("ammo",true)==stock+2&&ask.Remaining==supply-2,"Immediate purchase debits wallet, consumes supply and delivers to hangar");
  start=w.Balance;stock=w.ItemCount("ammo",true);w.SubmitMarket("ammo",w.SystemId,true,1000,price,0,true);Check(w.Balance==start&&w.ItemCount("ammo",true)==stock,"Insufficient immediate supply has no partial side effects");
  var bid=w.Offers("ammo",w.SystemId,true).First();price=bid.Price;w.SubmitMarket("ammo",w.SystemId,false,2,price,0,true);Check(w.Balance==start+Math.Round(2*price*.98m,2)&&w.ItemCount("ammo",true)==stock-2,"Sale removes inventory and pays proceeds after tax");
  start=w.Balance;decimal listing38=Math.Round(4*w.StationBrokerRate38,2),relisting38=Math.Round(4*(w.StationBrokerRate38+(1-(.5m+.06m*w.Player.SkillLevel("Advanced Broker Relations")))*w.StationBrokerRate38*2),2);w.SubmitMarket("ammo",w.SystemId,true,4,1,7,true);var own=w.MarketOrders.Single(o=>o.Player);Check(w.Balance==start-4-listing38&&own.Remaining==4,"Standing buy reserves escrow and broker fee");
  w.ModifyMarketOrder(own.Id,2);Check(w.Balance==start-8-listing38-relisting38&&own.Price==2,"Modification adjusts escrow and charges relisting fee");
  w.CancelMarketOrder(own.Id);Check(w.Balance==start-listing38-relisting38&&!w.MarketOrders.Any(o=>o.Player),"Cancellation refunds unfilled escrow but retains fees");
  stock=w.ItemCount("ammo",true);w.SubmitMarket("ammo",w.SystemId,false,3,100,1,true);own=w.MarketOrders.Single(o=>o.Player);Check(w.ItemCount("ammo",true)==stock-3,"Sell listing reserves real items");
  w.Day+=1.1;w.AdvanceMarket();Check(w.ItemCount("ammo",true)==stock&&!w.MarketOrders.Any(o=>o.Player),"Expiry returns unsold stock");
  var remote=w.MarketOrders.First(o=>!o.Buy&&o.Item=="ammo"&&o.Station!=w.SystemId&&DemoWorld.Systems.First(s=>s.Id==o.Station).Region==w.Current.Region);decimal remoteStock=w.StorageAt(remote.Station).GetValueOrDefault("ammo");stock=w.ItemCount("ammo",true);
  w.SubmitMarket("ammo",remote.Station,true,1,remote.Price,0,true);Check(w.StorageAt(remote.Station)["ammo"]==remoteStock+1&&w.ItemCount("ammo",true)==stock,"Regional purchases deliver at seller station");
  w.SubmitMarket("ammo",w.SystemId,true,3,1,7,true);
  string temp=Path.Combine(Path.GetTempPath(),"starlight-market-"+Guid.NewGuid().ToString("N"));string? prev=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
  try{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",temp);w.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.MarketOrders.Count==w.MarketOrders.Count&&loaded.WalletJournal.Count==w.WalletJournal.Count&&loaded.MarketTrades.Count==w.MarketTrades.Count&&loaded.Balance==w.Balance,"Orders, journal, trades and wallet survive save/load");}
  finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",prev);if(Directory.Exists(temp))Directory.Delete(temp,true);}
  var tick=Fresh();tick.SubmitMarket("ammo",tick.SystemId,true,10,12.1m,7,true);own=tick.MarketOrders.Single(o=>o.Player);stock=tick.ItemCount("ammo",true);tick.AdvanceHours(12);Check(own.Remaining==4&&tick.ItemCount("ammo",true)==stock+6,"Campaign time fills competitive standing orders");
  Check(MarketGroups.Root.Descendants().Count()==ItemCatalog.All.Length&&MarketGroups.Root.Descendants().Distinct().Count()==ItemCatalog.All.Length,"Every catalog item occurs exactly once in the category tree");
  Check(ItemCatalog.All.Count(i=>i.Slot=="ship")==FleetCatalog.Playable.Length,"All 400 playable hulls appear in Ships by class and faction");
  var ranged=Fresh();ranged.MarketOrders.RemoveAll(o=>o.Item=="ammo");var remoteStation=DemoWorld.Systems.First(s=>s.Id!=ranged.SystemId&&s.Region==ranged.Current.Region).Id;
  ranged.MarketOrders.Add(new(){Item="ammo",Station=remoteStation,Buy=true,Range=-1,Remaining=10,MinimumVolume=3,Price=8,Created=ranged.Day,Expires=ranged.Day+30});
  decimal cash=ranged.Balance;float ammo=ranged.ItemCount("ammo",true);ranged.SubmitMarket("ammo",ranged.SystemId,false,2,8,0,true);Check(ranged.Balance==cash&&ranged.ItemCount("ammo",true)==ammo,"Minimum-volume rejection is atomic");
  ranged.SubmitMarket("ammo",ranged.SystemId,false,3,8,0,true);Check(ranged.ItemCount("ammo",true)==ammo-3&&ranged.Balance==cash+23.52m,"Regional buyer accepts goods at seller station with tax");
  cash=ranged.Balance;ranged.SubmitMarket("ammo",ranged.SystemId,true,1,1,90,true,-2);Check(ranged.Balance==cash,"Invalid range rejected before escrow");
  ranged.SubmitMarket("ammo",ranged.SystemId,true,2,1,90,true,-1,2);var ro=ranged.MarketOrders.Single(o=>o.Player);Check(ro.Range==-1&&ro.MinimumVolume==2&&ro.Expires==ranged.Day+90,"Range, minimum volume and 90-day expiry stored");
  var ships=Fresh();var profile=ShipBatch21.All.First(h=>!h.Starter&&!h.LegacyCharter);ships.SystemId=profile.HomeSystem;ships.Credits=1000000;var offer=ships.Offers(profile.ShipId,ships.SystemId,false).First();ships.SubmitMarket(profile.ShipId,ships.SystemId,true,1,offer.Price,0,true);Check(ships.ItemCount(profile.ShipId,true)==1,"Market hull purchase delivers packaged ship to hangar");
  var active=ships.Player;Check(ships.AssembleMarketHull(profile.ShipId)&&ships.ItemCount(profile.ShipId,true)==0&&ships.FleetBerths.Single().Ship.CombatHull==profile.ShipId&&ReferenceEquals(active,ships.Player),"Assembly creates correct local berth and preserves active ship");
  Check(!ships.AssembleMarketHull(profile.ShipId)&&ships.FleetBerths.Count==1,"Assembly cannot duplicate ship");
  var flight=Fresh();flight.Screen=ViewMode.Space;var flightAsk=flight.Offers("ammo",flight.SystemId,false).First();float stored=flight.ItemCount("ammo",true);flight.SubmitMarket("ammo",flight.SystemId,true,1,flightAsk.Price,0,true);Check(flight.ItemCount("ammo",true)==stored+1,"Market purchase while in space delivers to station hangar");
  Console.WriteLine($"{checks} market checks passed.");
 }
}
