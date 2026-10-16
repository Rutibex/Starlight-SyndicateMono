namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public decimal ManagedContractEscrowD=>Commander.Contracts.Sum(c=>(c.Issuer!="player"?c.MoneyEscrow:0)+(c.Carrier.Length>0&&c.Carrier!="player"?c.CollateralEscrow:0)+(c.Bidder.Length>0&&c.Bidder!="player"?c.BidEscrow:0));
 public decimal ActorBalanceD(string actor)=>actor=="player"?Balance:PopulationPilot(actor)?.Wallet??Population.Businesses.FirstOrDefault(b=>b.Id==actor)?.Wallet??-1;
 void ActorChangeD(string actor,decimal amount,string reason){if(actor=="player")WalletChange(amount,reason);else if(PopulationPilot(actor) is {} p)p.Wallet+=amount;else if(Population.Businesses.FirstOrDefault(b=>b.Id==actor) is {} b)b.Wallet+=amount;else throw new InvalidOperationException("Unknown contract actor");}
 void TransferActorsD(string from,string to,decimal amount,string reason){ActorChangeD(to,amount,reason);if(from=="player"&&to!="player")Population.ExternalNet+=amount;else if(from!="player"&&to=="player")Population.ExternalNet-=amount;}
 public bool AudienceD(ContractD c,string actor)=>c.CorporationQ?Organisations.Corps.Any(org=>org.Id==c.Recipient&&org.Members.ContainsKey(actor)):c.Audience=="Public"||c.Audience=="Private"&&c.Recipient==actor||c.Audience=="Corporation"&&(PopulationPilot(actor)?.Company??(actor=="player"?StationCorporation:""))==c.Recipient;
 public ContractD? CreateContractD(ContractKindD kind,string title,Dictionary<string,decimal> items,string destination,decimal price=0,decimal reward=0,decimal collateral=0,int expiryDays=7,double deliveryHours=24,string audience="Public",string recipient="",Dictionary<string,decimal>? requested=null,decimal buyout=0,IEnumerable<string>? assetIds=null)
 {
  bool corporation=audience=="Corporation"&&Organisations.Corps.Any(c=>c.Id==recipient);if(corporation&&(CorporationG?.Id!=recipient||!HasRoleG("Accountant")))return null;var ids=assetIds?.ToArray()??[];if(ids.Distinct().Count()!=ids.Length)return null;var unique=ids.Select(PreviewUniqueD).ToArray();if(unique.Any(u=>u==null))return null;
  if(!Docked||(items.Count==0&&ids.Length==0)||expiryDays is <1 or >28||deliveryHours<=0||deliveryHours>720||price<0||reward<0||collateral<0||buyout<0||new[]{price,reward,collateral,buyout}.Any(v=>v!=Math.Round(v,2))||!new[]{"Public","Private","Corporation"}.Contains(audience)||audience!="Public"&&recipient.Length==0)return null;
  if(kind==ContractKindD.Courier&&(!LocationExists(destination)||Commander.LostFacilities.Contains(destination)||PopulationRouteC(SystemId,LocationSystem(destination)).Count==0))return null;
  if(items.Any(i=>!ItemCatalog.IsSupported(i.Key)||i.Value<=0||i.Value!=decimal.Floor(i.Value)||Hangar.GetValueOrDefault(i.Key)<i.Value)||requested!=null&&requested.Any(i=>!ItemCatalog.IsSupported(i.Key)||i.Value<=0||i.Value!=decimal.Floor(i.Value)))return null;
  decimal escrow=kind==ContractKindD.Courier?reward:0;decimal fee=10;if(Balance<escrow+fee)return null;
  var c=new ContractD{Kind=kind,Title=string.IsNullOrWhiteSpace(title)?kind.ToString():title[..Math.Min(80,title.Length)],Origin=StationKey44,Destination=kind==ContractKindD.Courier?destination:StationKey44,Items=new(items),Requested=requested==null?new():new(requested),Price=price,Reward=reward,Collateral=collateral,Buyout=buyout,MoneyEscrow=escrow,Expires=Now.AddDays(expiryDays),DeliveryHours=deliveryHours,Audience=audience,Recipient=recipient,CorporationQ=corporation};
  foreach(string id in ids){var asset=ReserveUniqueD(id);if(asset==null)throw new InvalidOperationException("Validated unique asset disappeared");c.Instances.Add(asset);}
  foreach(var item in items){c.Instances.AddRange(TakeInstancesD(item.Key,item.Value,true,StationKey44));ChangeStack(item.Key,true,-item.Value);}WalletChange(-escrow-fee,"Contract reward escrow and registration / "+c.Id);Commander.ServiceSinks+=fee;Commander.Contracts.Add(c);HistoryD("Created "+kind+" contract / "+c.Id);return c;
 }
 public ContractD? CreateNpcExchangeD()
 {
  if(!Docked)return null;EnsurePopulationC();var b=Population.Businesses.FirstOrDefault(b=>b.Station==StationKey44&&b.Kind=="Trader");if(b==null||b.Stock.GetValueOrDefault("ammo")<20)return null;
  var c=new ContractD{Kind=ContractKindD.Exchange,Issuer=b.Id,Title=b.Name+" ammunition lot",Origin=b.Station,Destination=b.Station,Price=20*ItemCatalog.Get("ammo").Price,Items=new(){["ammo"]=20},Expires=Now.AddDays(7)};b.Stock["ammo"]-=20;Commander.Contracts.Add(c);HistoryD("NPC exchange posted from actual trader stock");return c;
 }
 public ContractD? CreateNpcCourierD()
 {
  if(!Docked)return null;EnsurePopulationC();var b=Population.Businesses.FirstOrDefault(b=>b.Station==StationKey44&&b.Kind=="Trader"&&b.Stock.GetValueOrDefault("ammo")>=20&&b.Wallet>=30);var recipient=Population.Businesses.FirstOrDefault(r=>r.Kind=="Factory"&&r.Station!=StationKey44&&!Commander.LostFacilities.Contains(r.Station));if(b==null||recipient==null||PopulationRouteC(SystemId,LocationSystem(recipient.Station)).Count==0)return null;
  var c=new ContractD{Kind=ContractKindD.Courier,Issuer=b.Id,DeliveryOwner=recipient.Id,Title=b.Name+" supplies commission",Origin=b.Station,Destination=recipient.Station,Reward=30,Collateral=100,MoneyEscrow=30,Items=new(){["ammo"]=20},Expires=Now.AddDays(7),DeliveryHours=24};b.Stock["ammo"]-=20;b.Wallet-=30;Commander.Contracts.Add(c);HistoryD("NPC courier offer reserved actual trader cargo and reward budget");return c;
 }
 void DeliverContractItemsD(ContractD c,string actor,string station)
 {
  var storage=actor=="player"?StorageAt(station):PopulationPilot(actor)?.Cargo??Population.Businesses.Single(b=>b.Id==actor).Stock;
  foreach(var item in c.Items)storage[item.Key]=storage.GetValueOrDefault(item.Key)+item.Value;
  if(actor=="player")foreach(var instance in c.Instances)RestoreInstanceD(instance,true,station);
  // NPC recipients retain condition instances rather than stripping their identity.
  else if(c.Instances.Count>0){Commander.Containers.Add(new(){Name="NPC contract delivery",Station=station,Owner=actor,Access=false,Items=new(c.Items),Instances=c.Instances.ToList()});foreach(var item in c.Items)storage[item.Key]-=item.Value;}
  c.Items.Clear();c.Instances.Clear();c.ItemsClaimed=true;
 }
 public string CancelContractD(string id)
 {
  var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.State!="Outstanding"||c.Bidder.Length>0)return "Only outstanding unbid contracts can be cancelled.";
  if(c.MoneyEscrow>0)ActorChangeD(c.Issuer,c.MoneyEscrow,"Contract escrow returned");c.MoneyEscrow=0;DeliverContractItemsD(c,c.Issuer,c.Origin);c.State="Cancelled";HistoryD("Contract cancelled / "+id);return "";
 }
 public string AcceptExchangeD(string id,string actor="player")
 {
  var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.Kind!=ContractKindD.Exchange||c.State!="Outstanding"||c.Expires<=Now||actor==c.Issuer||!AudienceD(c,actor))return "Exchange unavailable or audience denied.";
  var stock=actor=="player"?Hangar:PopulationPilot(actor)?.Cargo??Population.Businesses.FirstOrDefault(b=>b.Id==actor)?.Stock;
  if(actor=="player"&&(!Docked||StationKey44!=c.Origin)||stock==null||ActorBalanceD(actor)<c.Price||c.Requested.Any(i=>stock.GetValueOrDefault(i.Key)<i.Value))return "Travel to the origin; provide the requested items and full price.";
  if(actor!="player"&&PopulationPilot(actor) is {} p&&(p.System!=LocationSystem(c.Origin)||p.Status!="Docked"))return "NPC recipient must be docked locally.";
  if(actor=="player"&&c.Requested.Any(i=>ConditionBanksD().Any(b=>b.List.Cast<object>().Any(e=>PropertyD(e,"State") is {} s&&(string?)PropertyD(s,"ModuleId")==i.Key))))return "Requested exchange items must be repackaged.";
  var issuerStock=c.Issuer=="player"?StorageAt(c.Origin):PopulationPilot(c.Issuer)?.Cargo??Population.Businesses.Single(b=>b.Id==c.Issuer).Stock;
  foreach(var item in c.Requested){stock[item.Key]-=item.Value;issuerStock[item.Key]=issuerStock.GetValueOrDefault(item.Key)+item.Value;}
  ActorChangeD(actor,-c.Price,"Exchange payment");TransferActorsD(actor,c.Issuer,c.Price,"Exchange proceeds");DeliverContractItemsD(c,actor,c.Origin);c.State="Completed";HistoryD("Exchange settled / "+id);return "";
 }
 public string BidAuctionD(string id,string actor,decimal amount)
 {
  var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.Kind!=ContractKindD.Auction||c.State!="Outstanding"||c.Expires<=Now||actor==c.Issuer||!AudienceD(c,actor)||amount<=0||amount!=Math.Round(amount,2))return "Auction unavailable.";
  decimal minimum=c.Bidder.Length==0?c.Price:c.BidEscrow+Math.Max(.01m,Math.Round(c.BidEscrow*.1m,2));if(amount<minimum||ActorBalanceD(actor)+(c.Bidder==actor?c.BidEscrow:0)<amount)return "Bid below increment or wallet too small.";
  if(c.Bidder.Length>0)ActorChangeD(c.Bidder,c.BidEscrow,"Outbid escrow refund");ActorChangeD(actor,-amount,"Auction bid escrow");c.Bidder=actor;c.BidEscrow=amount;
  if(c.Buyout>0&&amount>=c.Buyout){c.State="Finished";}else if((c.Expires-Now).TotalMinutes<5)c.Expires=Now.AddMinutes(5);HistoryD("Auction bid escrow / "+id+" / "+amount);return "";
 }
 public string ClaimAuctionD(string id,string actor)
 {
  AdvanceCommanderD();var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.Kind!=ContractKindD.Auction||c.State is not ("Finished" or "Completed"))return "Auction has not finished.";
  if(actor==c.Issuer&&!c.MoneyClaimed){TransferActorsD(c.Bidder,c.Issuer,c.BidEscrow,"Auction proceeds");c.BidEscrow=0;c.MoneyClaimed=true;}
  else if(actor==c.Bidder&&!c.ItemsClaimed){DeliverContractItemsD(c,actor,c.Origin);}else return "Nothing claimable by this actor.";
  if(c.MoneyClaimed&&c.ItemsClaimed)c.State="Completed";HistoryD("Auction claim / "+id+" / "+actor);return "";
 }
 public string AcceptCourierD(string id,string actor)
 {
  var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.Kind!=ContractKindD.Courier||c.State!="Outstanding"||c.Expires<=Now||!AudienceD(c,actor)||actor==c.Issuer||ActorBalanceD(actor)<c.Collateral)return "Courier unavailable, audience denied, or insufficient collateral.";
  var p=PopulationPilot(actor);if(actor=="player"){if(!Docked||StationKey44!=c.Origin||Commander.Capsule)return "Dock a cargo ship at the origin.";if(PackageVolumeD(c)>(decimal)(Player.CargoMax-CargoUsed))return "Package exceeds ship capacity.";}
  else if(p==null||p.Contracted||p.Fit.Hull<=0||p.Role is not ("Hauler" or "Trader")||p.Status!="Docked"||p.Home!=c.Origin||p.Cargo.Sum(i=>i.Value*(decimal)ItemVolume(i.Key))+PackageVolumeD(c)>(decimal)p.Fit.CargoMax)return "A funded local idle hauler with sufficient capacity is required.";
  var route=PopulationRouteC(LocationSystem(c.Origin),LocationSystem(c.Destination));if(route.Count==0)return "No navigable route.";
  ActorChangeD(actor,-c.Collateral,"Courier collateral escrow");c.CollateralEscrow=c.Collateral;c.Carrier=actor;c.State="In progress";c.Deadline=Now.AddHours(c.DeliveryHours);c.Route=route;
  if(p!=null){p.Status="Transit";p.Goal="Sealed courier package "+c.Id;p.Grid=Population.Sectors[p.System].Belt;var op=new ShipmentC{Id="courier-d:"+c.Id,Pilot=p.Id,Seller=p.Company,Buyer=p.Company,Destination=c.Destination,System=p.System,Grid=p.Grid,Route=route,Due=Population.Tick+5,State="Transit"};Population.Shipments.Add(op);}
  HistoryD("Courier accepted; sealed items and collateral reserved / "+id);return "";
 }
 public string CompleteCourierD(string id)
 {
  var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.State!="In progress"||c.Kind!=ContractKindD.Courier||c.PackageBroken||Commander.LostFacilities.Contains(c.Destination))return "Package cannot be delivered.";
  if(c.Carrier=="player"&&(!Docked||StationKey44!=c.Destination))return "Dock at the exact destination.";
  if(c.Carrier!="player"){var op=Population.Shipments.FirstOrDefault(o=>o.Id=="courier-d:"+c.Id);if(op==null||op.Leg+1<op.Route.Count||op.System!=LocationSystem(c.Destination)||Population.Tick<op.Due)return "NPC carrier must finish the actual route first.";}
  DeliverContractItemsD(c,c.DeliveryOwner.Length>0?c.DeliveryOwner:c.Issuer,c.Destination);ActorChangeD(c.Carrier,c.CollateralEscrow,"Courier collateral returned");c.CollateralEscrow=0;TransferActorsD(c.Issuer,c.Carrier,c.MoneyEscrow,"Courier reward");c.MoneyEscrow=0;c.State="Completed";FinishCourierPilotD(c,false);HistoryD("Courier completed exactly once / "+id);return "";
 }
 public string FailCourierD(string id,bool issuerDeadline=false,bool lost=false)
 {
  var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.State!="In progress"||c.Kind!=ContractKindD.Courier||issuerDeadline&&Now<=c.Deadline)return "Only an in-progress courier can fail; deadline must have passed for issuer failure.";
  TransferActorsD(c.Carrier,c.Issuer,c.CollateralEscrow,"Failed courier collateral");c.CollateralEscrow=0;ActorChangeD(c.Issuer,c.MoneyEscrow,"Failed courier reward returned");c.MoneyEscrow=0;c.State="Failed";
  if(lost){var p=PopulationPilot(c.Carrier);var wreck=new WreckD{Id="wreck-d:contract:"+c.Id,System=p?.System??SystemId,Grid=p?.Grid??Grid,Position=p?.Position??Player.Position,Expires=Now.AddHours(2)};foreach(var i in c.Items)wreck.Items[i.Key]=decimal.Floor(i.Value*.5m);foreach(var u in c.Instances.Where(u=>u.Bank is "ship" or "blueprint"?StableC(u.Id+c.Id)%2==0:wreck.Instances.Count(i=>i.Item==u.Item)<wreck.Items.GetValueOrDefault(u.Item)))wreck.Instances.Add(u);Commander.Wrecks.Add(wreck);c.Items.Clear();c.Instances.Clear();}
  else if((c.Items.Count>0||c.Instances.Count>0)){Commander.Containers.Add(new(){Name="Failed courier package / "+c.Title,Station=PopulationPilot(c.Carrier)?.Home??c.Origin,Owner=c.Carrier,Access=c.Carrier=="player",Items=new(c.Items),Instances=c.Instances.ToList()});c.Items.Clear();c.Instances.Clear();}
  FinishCourierPilotD(c,lost);HistoryD("Courier failed; collateral settled once / "+id);return "";
 }
 void FinishCourierPilotD(ContractD c,bool lost){var op=Population.Shipments.FirstOrDefault(o=>o.Id=="courier-d:"+c.Id);if(op!=null){op.State=lost?"Lost":"Delivered";op.Cargo.Clear();}var p=PopulationPilot(c.Carrier);if(p!=null){if(lost)LosePilotC(p);else{p.Status="Returning";p.RecoverAt=Population.Tick+Math.Max(5,c.Route.Count*5);p.System=LocationSystem(c.Destination);p.Grid=c.Destination;p.Experience++;}Targets.RemoveAll(t=>t.Id==p.Id);}}
 public string BreakCourierPackageD(string id){var c=Commander.Contracts.FirstOrDefault(c=>c.Id==id);if(c==null||c.Carrier!="player"||c.State!="In progress"||!Docked)return "Dock with your in-progress package.";c.PackageBroken=true;return FailCourierD(id);}
 void AdvanceCourierD(ContractD c){var p=PopulationPilot(c.Carrier);if(p?.Fit.Hull<=0)FailCourierD(c.Id,false,true);}
 public void FacilityLostD(string station,bool wormhole=false)
 {
  if(!Commander.LostFacilities.Add(station))return;
  foreach(var c in Commander.Contracts.Where(c=>(c.Origin==station||c.Destination==station)&&c.State is "Outstanding" or "In progress" or "Finished").ToArray()){
   if(c.State=="Outstanding"){if(c.Bidder.Length>0){ActorChangeD(c.Bidder,c.BidEscrow,"Facility loss bid refund");c.Bidder="";c.BidEscrow=0;}CancelContractD(c.Id);}
   else if(c.Kind==ContractKindD.Courier&&c.State=="In progress"){
    if(wormhole){FailCourierD(c.Id,false,true);continue;}var safe=LocationExists(RecoveryStationId)&&!Commander.LostFacilities.Contains(RecoveryStationId)?RecoveryStationId:Population.Sectors.Values.Select(s=>s.Station).FirstOrDefault(s=>!Commander.LostFacilities.Contains(s));if(safe==null){FailCourierD(c.Id,false,true);continue;}c.Destination=safe;var op=Population.Shipments.FirstOrDefault(o=>o.Id=="courier-d:"+c.Id);if(op!=null){op.Route=PopulationRouteC(op.System,LocationSystem(safe));op.Leg=0;op.Destination=safe;op.Due=Population.Tick+5;}HistoryD("Courier destination rerouted to asset safety / "+safe);
   }else if(c.Kind==ContractKindD.Auction&&c.State=="Finished"){ClaimAuctionD(c.Id,c.Issuer);ClaimAuctionD(c.Id,c.Bidder);}
  }
 }
}
