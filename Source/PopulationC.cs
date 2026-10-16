using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed class PopulationStateC
{
 public bool Enabled{get;set;}=true;public string Policy{get;set;}="Finite opening reserves / no emergency replenishment";
 public double RemainderSeconds{get;set;}public long Tick{get;set;}public long Sequence{get;set;}
 public Dictionary<string,SectorC> Sectors{get;set;}=[];public List<PilotC> Pilots{get;set;}=[];public List<BusinessC> Businesses{get;set;}=[];
 public List<ShipmentC> Shipments{get;set;}=[];public List<DebrisC> Debris{get;set;}=[];public List<PopulationEntryC> Journal{get;set;}=[];
 public decimal OpeningMoney{get;set;}public decimal ExternalNet{get;set;}public decimal Sinks{get;set;}public long OreExtracted{get;set;}public long GoodsProduced{get;set;}public long GoodsConsumed{get;set;}
 public string Formation{get;set;}="Arrow";public float Spacing{get;set;}=700;public HashSet<string> Watchlist{get;set;}=[];
}
public sealed class SectorC{public string System{get;set;}="";public string Station{get;set;}="";public string Belt{get;set;}="";public int Resource{get;set;}=50000;public decimal OpeningGoodsValue{get;set;}public int Shortages{get;set;}}
public sealed partial class PilotC
{
 public Dictionary<string,int> MissionLoyaltyL{get;set;}=[];public Dictionary<string,double> MissionStandingsL{get;set;}=[];
 public string Id{get;set;}="";public string Name{get;set;}="";public string Role{get;set;}="Escort";public string Company{get;set;}="";public string Home{get;set;}="";public string System{get;set;}="";public string Grid{get;set;}="";
 public Ship Fit{get;set;}=new();public string Status{get;set;}="Working";public string Goal{get;set;}="";public string Order{get;set;}="Follow";public string Target{get;set;}="";public bool Contracted{get;set;}public bool Suspended{get;set;}public decimal Wallet{get;set;}=250;public decimal Wage{get;set;}=12;
 public int Experience{get;set;}public int Losses{get;set;}public long RecoverAt{get;set;}public long PaidThrough{get;set;}public float DecisionLeft{get;set;}public float WorkLeft{get;set;}public bool Retreated{get;set;}
 public Dictionary<string,decimal> Cargo{get;set;}=[];public Dictionary<string,DateTime> Knowledge{get;set;}=[];public Dictionary<string,float> Relationships{get;set;}=[];public List<string> Memory{get;set;}=[];
 public string LegalState{get;set;}="Neutral";public Vector3 Position{get;set;}=new(1800,0,0);public Vector3 Velocity{get;set;}
}
public sealed partial class BusinessC
{
 public string Id{get;set;}="";public string Name{get;set;}="";public string Station{get;set;}="";public string Kind{get;set;}="Factory";public decimal Wallet{get;set;}=12000;
 public Dictionary<string,decimal> Stock{get;set;}=[];public int Shortages{get;set;}public string Product{get;set;}="ammo";public string State{get;set;}="Operating";public long Runs{get;set;}public long LostShipments{get;set;}
}
public sealed class ShipmentC
{
 public string Id{get;set;}="";public string Pilot{get;set;}="";public string Seller{get;set;}="";public string Buyer{get;set;}="";public string Destination{get;set;}="";public string System{get;set;}="";public string Grid{get;set;}="";
 public float AlignSeconds{get;set;}public List<string> Route{get;set;}=[];public int Leg{get;set;}public long Due{get;set;}public decimal Escrow{get;set;}public string State{get;set;}="Transit";public Dictionary<string,decimal> Cargo{get;set;}=[];
}
public sealed class DebrisC{public string Id{get;set;}="";public string Name{get;set;}="";public string System{get;set;}="";public string Grid{get;set;}="";public Vector3 Position{get;set;}public Dictionary<string,float> Contents{get;set;}=[];public float Salvage{get;set;}=3;}
public sealed record PopulationEntryC(long Tick,string Event,string Actor,string Item,decimal Quantity,decimal Money,string Detail);
public sealed partial class DemoWorld
{
 public static string PopulationHullC=>FleetCatalog.Source("Merlin").Id;
 public static string PopulationAmmoC=>WeaponState.Create("rail").ChargeId;
 public PopulationStateC Population{get;set;}=new();
 public PilotC? PopulationPilot(string id)=>Population.Pilots.FirstOrDefault(p=>p.Id==id);
 public bool ManagedPopulationTarget(string id)=>id.StartsWith("pilot-c:");
 static uint StableC(string text){uint n=2166136261;foreach(char c in text)n=(n^c)*16777619;return n;}
 void PopulationLogC(string e,string actor,string item="",decimal q=0,decimal cash=0,string detail=""){Population.Journal.Add(new(Population.Tick,e,actor,item,q,cash,detail));if(Population.Journal.Count>800)Population.Journal.RemoveRange(0,Population.Journal.Count-800);}
 public void RememberC(PilotC p,string message){p.Memory.Add(message);if(p.Memory.Count>12)p.Memory.RemoveAt(0);}
 public void EnsurePopulationC(string? system=null)
 {
  system??=SystemId;if(!Population.Enabled||Population.Sectors.ContainsKey(system)||Population.Sectors.Count>=128)return;
  string station=ImportedWorld43?(Stations44.BySystem.GetValueOrDefault(system)??[]).FirstOrDefault(s=>s.Services.Contains(7))?.Id??"":system;
  if(station.Length==0)return;var belts=SolarCatalog.ForSystem(system).Where(c=>c.Kind==CelestialKind.AsteroidBelt).ToArray();string belt=ImportedWorld43?belts.FirstOrDefault()?.Id??"deep_space":"belt";
  var sector=new SectorC{System=system,Station=station,Belt=belt};Population.Sectors[system]=sector;
  string id="company-c:"+system;var recipe=IndustryCatalog37.Recipes[PopulationAmmoC];
  foreach(string kind in new[]{"Refiner","Factory","Trader"}){
   var b=new BusinessC{Id=id+":"+kind,Name=LocationName(station)+" / "+kind,Station=station,Kind=kind,Product=PopulationAmmoC};
   b.Stock["ore"]=kind=="Refiner"?1200:100;b.Stock["ammo"]=kind=="Trader"?1000:200;b.Stock["rail"]=8;b.Stock[PopulationHullC]=8;b.Stock[PopulationAmmoC]=1200;foreach(var module in new[]{EwarCatalog.All.First(d=>d.Kind==EwarKind.Point&&d.Role.Length==0&&d.Size<=1).Id,SupportCatalog.All.First(d=>d.Kind=="Shield"&&d.Size==1).Id})b.Stock[module]=8;
   foreach(var m in recipe.Minerals)b.Stock[m.Key]=Math.Max(1000,m.Value*12);
   Population.Businesses.Add(b);Population.OpeningMoney+=b.Wallet;sector.OpeningGoodsValue+=b.Stock.Sum(s=>s.Value*ItemCatalog.Get(s.Key).Price);
  }
  var names=new[]{"Mireille","Farid","Anika","Lin","Oksana","Ramin","Émile","Nadia"};int index=0;
  foreach(string role in new[]{"Miner","Hauler","Trader","Scout","Tackler","Logi","Escort","Pirate"}){
   var p=new PilotC{Id="pilot-c:"+system+":"+role,Name=names[(index+(int)(StableC(system)%8))%8]+" "+new[]{"Laurent","Darzi","Rao","Zhou","Volkova","Nouri","Bouchard","Karimi"}[index++],Role=role,Company=id+":"+(role=="Miner"?"Refiner":role is "Hauler" or "Trader"?"Trader":"Factory"),Home=station,System=system,Grid=belt,Goal=role=="Miner"?"Extract finite local ore":role=="Hauler"?"Move paid regional supplies":role=="Pirate"?"Hunt observed vulnerable ships":"Available for a crew contract",Position=new(1600+index*450,0,800)};
   p.Fit=CreatePopulationFitC(role);p.Cargo[PopulationAmmoC]=120;if(role is "Hauler" or "Trader"){p.Status="Docked";p.Grid=station;}p.LegalState=role=="Pirate"?"Outlaw":"Neutral";Population.Pilots.Add(p);Population.OpeningMoney+=p.Wallet;
  }
  PopulationLogC("Opening reserves",system,"ore",sector.Resource,36000,"One-time finite business goods, ships, wallets and resource reserve. No hidden restock.");
 }
 public Ship CreatePopulationFitC(string role)
 {
  var s=new Ship();s.SetCombatHull(PopulationHullC);FittingSimulation.ClearEquipment(s);s.Weapons=[WeaponState.Create("rail")];s.Weapons[0].Loaded=s.Weapons[0].Definition.Magazine;
  foreach(string name in new[]{"Gunnery","Rapid Firing","Small Hybrid Turret","CPU Management","Power Grid Management","Capacitor Management","Shield Management","Mining","Leadership"}){var skill=SkillCatalog.All.FirstOrDefault(k=>k.SourceName==name);if(skill!=null)s.SkillLevels[skill.Id]=3;}
  if(role=="Tackler")s.Ewar[0]=new(){ModuleId=EwarCatalog.All.First(d=>d.Kind==EwarKind.Point&&d.Role.Length==0&&d.Size<=1).Id};
  if(role=="Logi"){s.Support[0]=SupportState.Create(SupportCatalog.All.First(d=>d.Kind=="Shield"&&d.Size==1).Id);}
  s.Shield=s.MaxShield;s.Armor=s.MaxArmor;s.Hull=s.MaxHull;s.Cap=s.MaxCap;return s;
 }
 public decimal PopulationMoneyC=>ManagedContractEscrowD+Population.Businesses.Sum(b=>b.Wallet)+Population.Pilots.Sum(p=>p.Wallet)+Population.Shipments.Sum(o=>o.Escrow)+MarketOrders.Where(o=>o.BusinessC.Length>0&&o.Buy).Sum(o=>o.Remaining*o.Price);
 public decimal PopulationMoneyErrorC=>PopulationMoneyC-(Population.OpeningMoney+Population.ExternalNet-Population.Sinks);
 public List<string> PopulationRouteC(string from,string to)
 {
  if(from==to)return [from];var queue=new Queue<string>();queue.Enqueue(from);var prev=new Dictionary<string,string>{{from,""}};
  while(queue.Count>0&&prev.Count<10000){string at=queue.Dequeue();foreach(string next in ResolveSystem43(at).Links){if(prev.ContainsKey(next))continue;prev[next]=at;if(next==to){var result=new List<string>();for(string n=to;n.Length>0;n=prev[n])result.Add(n);result.Reverse();return result;}queue.Enqueue(next);}}
  return [];
 }
 public ShipmentC? DispatchShipmentC(string sellerId,string buyerId,string item,int quantity)
 {
  var seller=Population.Businesses.FirstOrDefault(b=>b.Id==sellerId);var buyer=Population.Businesses.FirstOrDefault(b=>b.Id==buyerId);
  var pilot=Population.Pilots.FirstOrDefault(p=>p.Company==sellerId&&p.Role is "Hauler" or "Trader"&&!p.Contracted&&p.Status=="Docked"&&p.Fit.Hull>0);
  if(seller==null||buyer==null||pilot==null||quantity<=0||seller.Stock.GetValueOrDefault(item)<quantity)return null;var route=PopulationRouteC(LocationSystem(seller.Station),LocationSystem(buyer.Station));if(route.Count==0)return null;
  decimal price=Math.Round(ItemCatalog.Get(item).Price*(1+Math.Min(.5m,buyer.Shortages*.02m)),2),cost=price*quantity;if(buyer.Wallet<cost)return null;
  seller.Stock[item]-=quantity;buyer.Wallet-=cost;var op=new ShipmentC{Id="shipment-c:"+(++Population.Sequence),Seller=seller.Id,Buyer=buyer.Id,Pilot=pilot.Id,Destination=buyer.Station,System=route[0],Grid=Population.Sectors[route[0]].Belt,Route=route,Due=Population.Tick+5,Escrow=cost,Cargo=new(){{item,quantity}}};
  Population.Shipments.Add(op);pilot.Status="Transit";pilot.Grid=op.Grid;pilot.Target="";RememberC(pilot,"Accepted "+item+" shipment to "+LocationName(buyer.Station));PopulationLogC("Dispatch",pilot.Id,item,quantity,cost,op.Id);return op;
 }
 public bool ResolveShipmentC(string id,bool lost)
 {
  var op=Population.Shipments.FirstOrDefault(o=>o.Id==id);if(op==null||op.State is "Delivered" or "Lost")return false;
  if(op.Id.StartsWith("courier-d:")){var c=Commander.Contracts.FirstOrDefault(c=>"courier-d:"+c.Id==op.Id);if(c==null)return false;return (lost?FailCourierD(c.Id,false,true):CompleteCourierD(c.Id)).Length==0;}
  var buyer=Population.Businesses.Single(b=>b.Id==op.Buyer);var seller=Population.Businesses.Single(b=>b.Id==op.Seller);var pilot=PopulationPilot(op.Pilot)!;
  if(lost){buyer.Wallet+=op.Escrow;seller.LostShipments++;buyer.Shortages++;var debris=new DebrisC{Id="debris-c:"+op.Id,Name=pilot.Name+" shipment wreck",System=op.System,Grid=op.Grid,Position=pilot.Position};foreach(var c in op.Cargo){float drop=(float)decimal.Floor(c.Value*.5m);if(drop>0)debris.Contents[c.Key]=drop;Population.GoodsConsumed+=(long)c.Value-(long)drop;}Population.Debris.Add(debris);op.State="Lost";LosePilotC(pilot);}
  else{foreach(var c in op.Cargo)buyer.Stock[c.Key]=buyer.Stock.GetValueOrDefault(c.Key)+c.Value;seller.Wallet+=op.Escrow;op.State="Delivered";pilot.Status="Returning";pilot.RecoverAt=Population.Tick+Math.Max(5,op.Route.Count*5);pilot.System=LocationSystem(op.Destination);pilot.Grid=op.Destination;pilot.Experience++;RememberC(pilot,"Delivered shipment "+op.Id);}
  PopulationLogC(lost?"Shipment lost":"Shipment delivered",pilot.Id,"",op.Cargo.Values.Sum(),op.Escrow,op.Id);op.Escrow=0;op.Cargo.Clear();Targets.RemoveAll(t=>t.Id==pilot.Id);return true;
 }
 void LosePilotC(PilotC p){p.Fit.Hull=0;p.Losses++;p.Status="Recovering";p.RecoverAt=Population.Tick+60;p.Target="";p.Knowledge.Clear();p.LegalState=p.Role=="Pirate"?"Outlaw":"Neutral";RememberC(p,"Ship lost; replacement requires a real hull, gun and ammunition.");PopulationLogC("Ship loss",p.Id,"frigate",1);Population.GoodsConsumed+=2;}
 public bool RecordPopulationLossC(SpaceTarget target)
 {
  var p=PopulationPilot(target.Id);if(p==null)return false;if(target.KillRecorded)return true;target.KillRecorded=true;var op=Population.Shipments.FirstOrDefault(o=>o.Pilot==p.Id&&o.State is "Transit" or "On grid");
  if(op!=null)ResolveShipmentC(op.Id,true);else{LosePilotC(p);var debris=new DebrisC{Id="debris-c:"+(++Population.Sequence),Name=p.Name+" wreck",System=p.System,Grid=p.Grid,Position=target.Position};foreach(var c in p.Cargo)debris.Contents[c.Key]=(float)c.Value;p.Cargo.Clear();Population.Debris.Add(debris);}
  if(LockedId==p.Id)LockedId="";MaterializeDebrisC();return true;
 }
 public bool ReplacePopulationShipC(string id)
 {
  var p=PopulationPilot(id);if(p==null||p.Fit.Hull>0||ActiveEncounterF is {FilamentO:>0,State:"Active"} e&&e.CrewO.Contains(id))return false;var b=Population.Businesses.Single(b=>b.Id==p.Company);
  if(b.Stock.GetValueOrDefault(PopulationHullC)<1||b.Stock.GetValueOrDefault("rail")<1||b.Stock.GetValueOrDefault(PopulationAmmoC)<120){p.Status="Awaiting supplies";return false;}
  foreach(var m in FittingLayoutModel.Installed(p.Fit).Where(m=>m.Kind is "ewar" or "support")){if(b.Stock.GetValueOrDefault(m.Id)<1){p.Status="Awaiting supplies";return false;}}
  foreach(var m in FittingLayoutModel.Installed(p.Fit).Where(m=>m.Kind is "ewar" or "support"))b.Stock[m.Id]--;
  b.Stock[PopulationHullC]--;b.Stock["rail"]--;b.Stock[PopulationAmmoC]-=120;p.Fit=CreatePopulationFitC(p.Role);p.Fit.Weapons[0].Loaded=Math.Min(120,p.Fit.Weapons[0].Definition.Magazine);p.Cargo[PopulationAmmoC]=120-p.Fit.Weapons[0].Loaded;p.Status=p.Contracted?"Fleet":p.Role is "Hauler" or "Trader"?"Docked":"Working";p.System=LocationSystem(p.Home);p.Grid=p.Contracted?Grid:p.Status=="Docked"?p.Home:Population.Sectors[p.System].Belt;p.Retreated=false;RememberC(p,"Replacement acquired from "+b.Name);PopulationLogC("Reship",p.Id,"frigate",1);return true;
 }
 public bool HirePopulationPilotC(string id)
 {
  EnsurePopulationC();var p=PopulationPilot(id);if(!Docked||p==null||p.Contracted||p.System!=SystemId||p.Fit.Hull<=0||p.Role is "Pirate" or "Hauler" or "Trader"||Population.Pilots.Count(p=>p.Contracted)>=12||Balance<350)return false;
  WalletChange(-350,"Crew contract / "+p.Name);p.Wallet+=350;Population.ExternalNet+=350;p.Contracted=true;p.Status="Fleet";p.Suspended=false;p.Order="Follow";p.PaidThrough=Population.Tick+60;p.Wage=p.Role=="Logi"?18:12;p.Goal="Fleet contract / Follow";RememberC(p,"Accepted paid player crew contract.");Population.Watchlist.Add(p.Id);PopulationLogC("Crew hired",p.Id,"",0,350);return true;
 }
 public bool DismissPopulationPilotC(string id){var p=PopulationPilot(id);if(!Docked||p?.Contracted!=true)return false;p.Contracted=false;p.Status="Working";p.Suspended=false;p.System=LocationSystem(p.Home);p.Grid=Population.Sectors[p.System].Belt;p.Target="";Population.Watchlist.Remove(id);PopulationLogC("Crew dismissed",id);return true;}
 public bool OrderPopulationC(string order,string? id=null)
 {
  if(order is not ("Follow" or "Hold" or "Attack" or "Mine" or "Recall" or "Scout"))return false;
  if(order=="Attack"&&(Selected==null||!Selected.Alive||Selected.Friendly&&WeaponSafety)||order=="Mine"&&Selected?.Kind!=TargetKind.Asteroid)return false;
  var pilots=Population.Pilots.Where(p=>p.Contracted&&!p.Suspended&&p.Fit.Hull>0&&(id==null||p.Id==id)).ToArray();if(pilots.Length==0)return false;
  foreach(var p in pilots){p.Order=order;p.Target=order is "Attack" or "Mine"?SelectedId:"";p.Status="Fleet";p.Retreated=false;RememberC(p,"Fleet broadcast: "+order);}PopulationLogC("Fleet broadcast","player",detail:order);return true;
 }
 public void CancelBusinessOrdersC(BusinessC b){foreach(var o in MarketOrders.Where(o=>o.BusinessC==b.Id).ToArray()){if(o.Buy)b.Wallet+=o.Remaining*o.Price;else b.Stock[o.Item]=b.Stock.GetValueOrDefault(o.Item)+o.Remaining;MarketOrders.Remove(o);}}
 public void PublishBusinessOrdersC()
 {
  foreach(var b in Population.Businesses){CancelBusinessOrdersC(b);foreach(string item in new[]{"ore","ammo",PopulationAmmoC,"rail"}.Distinct()){
   int quantity=(int)Math.Min(1000,Math.Max(0,b.Stock.GetValueOrDefault(item)-(item=="ammo"||item==PopulationAmmoC?240:4)));if(quantity==0)continue;decimal price=Math.Round(ItemCatalog.Get(item).Price*(1+Math.Min(.5m,b.Shortages*.02m)),2);
   b.Stock[item]-=quantity;MarketOrders.Add(new(){BusinessC=b.Id,Item=item,Station=b.Station,Remaining=quantity,Price=price,Created=Day,Expires=Day+90});}
   string input=b.Kind=="Refiner"?"ore":b.Kind=="Trader"?"ammo":IndustryCatalog37.Recipes[b.Product].Minerals.Keys.First();decimal bid=Math.Round(ItemCatalog.Get(input).Price*.9m,2);int buy=(int)Math.Min(100,decimal.Floor(b.Wallet/Math.Max(.01m,bid)));if(buy>0){b.Wallet-=buy*bid;MarketOrders.Add(new(){BusinessC=b.Id,Item=input,Station=b.Station,Buy=true,Remaining=buy,Price=bid,Created=Day,Expires=Day+90});}
  }MatchStandingOrders();
 }
 void CompleteBusinessTradeC(MarketOrder order,int quantity,decimal price)
 {
  if(order.BusinessC.Length==0)return;var b=Population.Businesses.Single(x=>x.Id==order.BusinessC);decimal gross=quantity*price;
  if(order.Buy){b.Stock[order.Item]=b.Stock.GetValueOrDefault(order.Item)+quantity;b.Wallet+=quantity*(order.Price-price);Population.ExternalNet-=gross;}
  else{decimal tax=Math.Round(gross*SalesTax,2);b.Wallet+=gross-tax;Population.ExternalNet+=gross;Population.Sinks+=tax;}
  PopulationLogC("Player trade",b.Id,order.Item,quantity,gross);
 }
 void ProcureC(BusinessC buyer,string item,int count)
 {
  var seller=Population.Businesses.Where(b=>b.Id!=buyer.Id&&b.Station==buyer.Station&&b.Stock.GetValueOrDefault(item)>=count).OrderBy(b=>b.Shortages).FirstOrDefault();if(seller==null)return;
  decimal cost=count*ItemCatalog.Get(item).Price;if(cost>buyer.Wallet)return;buyer.Wallet-=cost;seller.Wallet+=cost;seller.Stock[item]-=count;buyer.Stock[item]=buyer.Stock.GetValueOrDefault(item)+count;PopulationLogC("Procurement",buyer.Id,item,count,cost,seller.Id);
 }
 public void AdvancePopulationC(double seconds)
 {
  if(!Population.Enabled||seconds<=0)return;EnsurePopulationC();Population.RemainderSeconds+=seconds;
  while(Population.RemainderSeconds>=60-1e-8){Population.RemainderSeconds-=60;Population.Tick++;SyncPopulationGridC();
   foreach(var p in Population.Pilots.ToArray()){
    if(p.Contracted&&Population.Tick>=p.PaidThrough){if(Balance>=p.Wage){WalletChange(-p.Wage,"Crew wage / "+p.Name);p.Wallet+=p.Wage;Population.ExternalNet+=p.Wage;p.Suspended=false;p.PaidThrough=Population.Tick+60;}else{p.Suspended=true;p.Order="Hold";p.PaidThrough=Population.Tick+60;PopulationLogC("Unpaid crew",p.Id,detail:"Contract suspended; replenish wallet to resume on next pay period.");}}
    if(p.Fit.Hull<=0){if(Population.Tick>=p.RecoverAt)ReplacePopulationShipC(p.Id);continue;}
    if(Docked||!Targets.Any(t=>t.Id==p.Id)){p.Fit.Cap=TacticalRulesB.RechargeAfter(p.Fit.Cap,p.Fit.MaxCap,(float)p.Fit.Stat("rechargeRate")/1000,60);p.Fit.Shield=TacticalRulesB.RechargeAfter(p.Fit.Shield,p.Fit.MaxShield,(float)p.Fit.Stat("shieldRechargeRate")/1000,60);}
    if(p.Status is "Awaiting repair funds" or "Awaiting ammunition")RepairPilotC(p);
    if(p.Status=="Returning"&&Population.Tick>=p.RecoverAt){p.Status="Docked";p.System=LocationSystem(p.Home);p.Grid=p.Home;}
    if(p.Status=="Retreating"&&Population.Tick>=p.RecoverAt){p.Status="Docked";p.Grid=p.Home;RepairPilotC(p);}
    if(!p.Contracted&&p.Role=="Miner"&&p.Status=="Working"&&p.Fit.Cap>=8&&(Docked||!Targets.Any(t=>t.Id==p.Id))&&Population.Tick%2==0){var sector=Population.Sectors.GetValueOrDefault(p.System);if(sector!=null){int q=Math.Min(20,sector.Resource);sector.Resource-=q;if(q>0)p.Fit.Cap-=8;var b=Population.Businesses.Single(b=>b.Id==p.Company);b.Stock["ore"]=b.Stock.GetValueOrDefault("ore")+q;Population.OreExtracted+=q;}}
    if(!p.Contracted&&p.Status=="Docked"&&p.Role is "Hauler" or "Trader"&&Population.Tick%30==0){var from=Population.Businesses.Single(b=>b.Id==p.Company);var to=Population.Businesses.Where(b=>b.Id!=from.Id&&b.Kind=="Factory"&&b.Wallet>=50*ItemCatalog.Get("ammo").Price).OrderByDescending(b=>b.Shortages).ThenByDescending(b=>PopulationRouteC(p.System,LocationSystem(b.Station)).Count).FirstOrDefault();if(to!=null)DispatchShipmentC(from.Id,to.Id,"ammo",50);}
   }
   foreach(var op in Population.Shipments.Where(o=>o.State is "Transit" or "On grid").ToArray()){
    if(Population.Tick<op.Due)continue;if(!Docked&&Targets.Any(t=>t.Id==op.Pilot)){op.Due=Population.Tick+1;continue;}
    var pirate=Population.Pilots.FirstOrDefault(p=>p.System==op.System&&p.Grid==op.Grid&&p.Role=="Pirate"&&p.Fit.Hull>0&&p.Status=="Working");if(pirate!=null&&ResolveSystem43(op.System).Security<.5f&&StableC(op.Id+":"+Population.Tick)%4==0){ResolvePopulationAmbushC(op,pirate);if(op.State=="Lost")continue;}
    if(op.Leg+1>=op.Route.Count){ResolveShipmentC(op.Id,false);continue;}op.System=op.Route[++op.Leg];EnsurePopulationC(op.System);op.Grid=Population.Sectors.GetValueOrDefault(op.System)?.Belt??"deep_space";op.Due=Population.Tick+5;op.State="Transit";var p=PopulationPilot(op.Pilot)!;p.System=op.System;p.Grid=op.Grid;}
   if(Population.Tick%5==0)foreach(var b in Population.Businesses){
    if(!SystemsQ.AdvancedEconomy){if(b.Kind=="Refiner")RefineBusinessC(b);else if(b.Kind=="Factory")ManufactureBusinessC(b);}
    b.State=b.Wallet<=0?"Bankrupt / waits for sales":b.Shortages>0?"Supply constrained":"Operating";}
   if(Population.Tick%30==0)PublishBusinessOrdersC();
   if(Population.Shipments.Count>300)Population.Shipments.RemoveAll(o=>o.State is "Lost" or "Delivered");
  }BindPopulationGridC();
 }
 void RepairPilotC(PilotC p){var b=Population.Businesses.Single(b=>b.Id==p.Company);if(b.Wallet<40){p.Status="Awaiting repair funds";return;}int needed=Math.Max(0,120-(int)p.Cargo.GetValueOrDefault(PopulationAmmoC));if(b.Stock.GetValueOrDefault(PopulationAmmoC)<needed){p.Status="Awaiting ammunition";return;}b.Wallet-=40;Population.Sinks+=40;p.Fit.Shield=p.Fit.MaxShield;p.Fit.Armor=p.Fit.MaxArmor;p.Fit.Hull=p.Fit.MaxHull;p.Fit.Cap=p.Fit.MaxCap;int topup=Math.Max(0,120-(int)p.Cargo.GetValueOrDefault(PopulationAmmoC));if(b.Stock.GetValueOrDefault(PopulationAmmoC)>=topup){b.Stock[PopulationAmmoC]-=topup;p.Cargo[PopulationAmmoC]=p.Cargo.GetValueOrDefault(PopulationAmmoC)+topup;}foreach(var w in p.Fit.Weapons){w.Damage=0;w.Burnt=false;w.Overheat=false;}p.Status=p.Contracted?"Fleet":"Working";PopulationLogC("Paid repair",p.Id,cash:40);}
 void RefineBusinessC(BusinessC b){if(b.Stock.GetValueOrDefault("ore")<100)ProcureC(b,"ore",100);if(b.Stock.GetValueOrDefault("ore")<100){b.Shortages++;return;}b.Stock["ore"]-=100;var feed=IndustryCatalog37.OreContents("ore");if(feed!=null){foreach(var m in feed.Materials){decimal output=decimal.Floor(m.Value*(100m/feed.Portion)*.5m);b.Stock[m.Key]=b.Stock.GetValueOrDefault(m.Key)+output;}}else b.Stock["ind_34"]=b.Stock.GetValueOrDefault("ind_34")+100;b.Runs++;b.Shortages=Math.Max(0,b.Shortages-1);Population.GoodsConsumed+=100;PopulationLogC("Refining",b.Id,"ore",100);}
 void ManufactureBusinessC(BusinessC b){if((CharacterItemsD.AcquisitionOnly(b.Product)||ProductionCatalogE.Restricted(b.Product))){b.State="Special acquisition required";b.Shortages++;return;}var recipe=IndustryCatalog37.Recipes[b.Product];foreach(var m in recipe.Minerals)if(b.Stock.GetValueOrDefault(m.Key)<m.Value)ProcureC(b,m.Key,(int)Math.Min(int.MaxValue,m.Value));if(b.Wallet<5||recipe.Minerals.Any(m=>b.Stock.GetValueOrDefault(m.Key)<m.Value)){b.Shortages++;return;}foreach(var m in recipe.Minerals){b.Stock[m.Key]-=m.Value;Population.GoodsConsumed+=m.Value;}b.Wallet-=5;Population.Sinks+=5;b.Stock[b.Product]=b.Stock.GetValueOrDefault(b.Product)+recipe.Definition.Output;Population.GoodsProduced+=recipe.Definition.Output;b.Runs++;b.Shortages=Math.Max(0,b.Shortages-1);PopulationLogC("Manufacturing",b.Id,b.Product,recipe.Definition.Output,5,recipe.Provenance);}
 public void SyncPopulationGridC()
 {
  if(Docked)return;
  foreach(var p in Population.Pilots){var t=Targets.FirstOrDefault(t=>t.Id==p.Id);if(t==null)continue;p.Fit.Shield=t.Shield;p.Fit.Armor=t.Armor;p.Fit.Hull=t.Hull;p.Fit.Cap=t.Cap;if(t.NpcAmmunition.Count>0)p.Cargo[PopulationAmmoC]=t.NpcAmmunition.GetValueOrDefault(p.Fit.Weapons[0].ChargeId);p.Position=t.Position;p.Velocity=t.Velocity;if(t.Hull<=0&&!t.KillRecorded)RecordPopulationLossC(t);}
  foreach(var sector in Population.Sectors.Values){var rock=Targets.FirstOrDefault(t=>t.Id=="resource-c:"+sector.System);if(rock!=null)sector.Resource=Math.Max(0,(int)rock.Ore);}
  foreach(var d in Population.Debris){var t=Targets.FirstOrDefault(t=>t.Id==d.Id);if(t!=null){d.Contents=t.Contents;d.Salvage=t.Salvage;}}
 }
 void MaterializeDebrisC(){foreach(var d in Population.Debris.Where(d=>d.System==SystemId&&d.Grid==Grid))if(!Targets.Any(t=>t.Id==d.Id))Targets.Add(new(){Id=d.Id,Name=d.Name,Kind=TargetKind.Wreck,Position=d.Position,Contents=d.Contents,Salvage=d.Salvage});}
 public void BindPopulationGridC()
 {
  if(!Population.Enabled||Docked||WeaponTestMode)return;EnsurePopulationC();
  Targets.RemoveAll(t=>ManagedPopulationTarget(t.Id)&&PopulationPilot(t.Id) is not {Fit.Hull:>0});int slot=0;
  foreach(var p in Population.Pilots.Where(p=>p.Contracted&&p.Order!="Hold"&&p.Status!="Retreating"&&p.Fit.Hull>0&&CrewAdmittedP(p.Id)&&CrewAdmittedO(p.Id))){p.System=SystemId;p.Grid=Grid;}
  foreach(var p in Population.Pilots.Where(p=>CrewAdmittedP(p.Id)&&CrewAdmittedO(p.Id)&&p.Fit.Hull>0&&p.System==SystemId&&p.Grid==Grid&&p.Status is "Working" or "Fleet" or "Transit" or "On grid")){
   if(p.Contracted){p.Grid=Grid;p.Position=Player.Position+FormationOffsetC(slot++);}
   var t=Targets.FirstOrDefault(t=>t.Id==p.Id);if(t==null){t=new(){Id=p.Id,OwnerId=p.Id,Name=p.Name+" / "+p.Role,Kind=TargetKind.Raider,Position=p.Position,Velocity=p.Velocity,Shield=p.Fit.Shield,Armor=p.Fit.Armor,Hull=p.Fit.Hull,Cap=p.Fit.Cap,TacticalProvisioned=true,LockId="",Friendly=p.Role!="Pirate"};Targets.Add(t);}
   t.Profile=p.Fit;t.Signature=p.Fit.Signature;t.TacticalProvisioned=true;var op=Population.Shipments.FirstOrDefault(o=>o.Pilot==p.Id&&o.State is "Transit" or "On grid");if(op!=null)op.State="On grid";
   foreach(var c in p.Cargo.Where(c=>c.Key==PopulationAmmoC))t.NpcAmmunition[p.Fit.Weapons[0].ChargeId]=(int)c.Value;
  }
  var sector=Population.Sectors.GetValueOrDefault(SystemId);if(sector!=null&&sector.Belt==Grid&&sector.Resource>0&&!Targets.Any(t=>t.Id=="resource-c:"+SystemId))Targets.Add(new(){Id="resource-c:"+SystemId,Name="Shared local mining reserve",Kind=TargetKind.Asteroid,Position=new(2400,0,1200),Ore=sector.Resource,Signature=320});MaterializeDebrisC();BindDensP();BindMiningQ();BindPlanetLaunchesQ();BindStructuresQ();BindTerritoryQ();
 }
 Vector3 FormationOffsetC(int i)=>Population.Formation switch{"Wall"=>new((i-5)*Population.Spacing,0,1200),"Sphere"=>new(MathF.Cos(i*2.4f)*Population.Spacing,MathF.Sin(i*1.1f)*Population.Spacing,1200),_=>new((i%2==0?1:-1)*(i/2+1)*Population.Spacing,0,1000+i/2*Population.Spacing)};
 public bool PopulationCanFightC(SpaceTarget t){var p=PopulationPilot(t.Id);return p==null?!t.Friendly:p.ManagedP||ActiveEncounterF?.RunP!=null?FleetCanFightP(p):p.Target.Length>0&&!p.Suspended&&p.Status is "Working" or "Fleet"&&p.Role is not ("Miner" or "Logi" or "Hauler" or "Trader" or "Scout");}
 public void AggressPopulationC(SpaceTarget target){var p=PopulationPilot(target.Id);if(p==null)return;p.Relationships["player"]=-10;p.Target="player";p.LegalState="Defending";RememberC(p,"Attacked by player at "+SystemId);foreach(var ally in Population.Pilots.Where(a=>a.System==p.System&&a.Role is "Escort" or "Tackler"&&!a.Contracted)){ally.Target="player";ally.LegalState="Defending";}}
 public void UpdatePopulationTacticsC(float dt)
 {
  if(!Population.Enabled||WeaponTestMode)return;foreach(var p in Population.Pilots.Where(p=>p.System==SystemId&&p.Fit.Hull>0).ToArray()){
   var t=Targets.FirstOrDefault(t=>t.Id==p.Id&&t.Alive);if(t==null)continue;if(UpdateFleetP(p,t,dt))continue;var ship=p.Fit;
   if(!PopulationCanFightC(t)){t.Cap=TacticalRulesB.RechargeAfter(t.Cap,ship.MaxCap,(float)ship.Stat("rechargeRate")/1000,dt);t.Shield=TacticalRulesB.RechargeAfter(t.Shield,ship.MaxShield,(float)ship.Stat("shieldRechargeRate")/1000,dt);}
   var convoy=Population.Shipments.FirstOrDefault(o=>o.Pilot==p.Id&&o.State=="On grid");if(convoy!=null&&Population.Tick>=convoy.Due){if(WarpBlocked(t.Id)){convoy.AlignSeconds=0;p.Goal="Convoy departure blocked by tackle";}else{convoy.AlignSeconds+=dt;p.Goal="Align for next convoy leg";if(convoy.AlignSeconds>=ship.AlignTime){SyncPopulationGridC();Targets.Remove(t);convoy.State="Transit";convoy.Due=Population.Tick+1;convoy.AlignSeconds=0;continue;}}}
   if(p.Role=="Pirate"&&Current.Security<.5f&&Vector3.Distance(t.Position,Player.Position)<ship.Stat("maxTargetRange"))p.Target="player";
   p.DecisionLeft-=dt;if(p.DecisionLeft<=0){p.DecisionLeft=.5f;foreach(var other in Targets.Where(o=>o.Alive&&o.Id!=p.Id&&Vector3.Distance(o.Position,t.Position)<ship.Stat("maxTargetRange")))p.Knowledge[other.Id]=Now.AddSeconds(20);if(Vector3.Distance(t.Position,Player.Position)<ship.Stat("maxTargetRange"))p.Knowledge["player"]=Now.AddSeconds(20);foreach(var k in p.Knowledge.Where(k=>k.Value<Now).Select(k=>k.Key).ToArray())p.Knowledge.Remove(k);}
   float hp=(t.Shield+t.Armor+t.Hull)/(ship.MaxShield+ship.MaxArmor+ship.MaxHull);bool dry=ship.Weapons.All(w=>w.Loaded<=0&&t.NpcAmmunition.GetValueOrDefault(w.ChargeId)<=0);
   if((hp<.22f||t.Cap<ship.MaxCap*.08f||dry)&&p.Role is "Pirate" or "Escort" or "Tackler"){
    p.Retreated=true;t.LockId="";p.Goal=WarpBlocked(t.Id)?"Escape blocked by tackle":"Retreat for paid repairs / supplies";if(!WarpBlocked(t.Id)){p.Status="Retreating";p.RecoverAt=Population.Tick+3;SyncPopulationGridC();Targets.Remove(t);continue;}}
   bool known=p.Target.Length>0&&p.Knowledge.ContainsKey(p.Target)&&p.Knowledge[p.Target]>=Now;var victim=p.Target=="player"?null:Targets.FirstOrDefault(o=>o.Id==p.Target&&o.Alive);Vector3 aim=known?(victim?.Position??Player.Position):Player.Position+FormationOffsetC(Population.Pilots.IndexOf(p)%12);float range=Vector3.Distance(t.Position,aim);
   t.LockId=known&&!p.Suspended&&CanLock(t.Id,p.Target)?p.Target:"";if(p.Role is "Miner" or "Logi" or "Scout" or "Hauler" or "Trader")t.LockId="";
   float desired=p.Role=="Tackler"?4500:p.Role=="Pirate"||p.Order=="Attack"?Math.Max(3000,TacticalRulesB.Weapon(ship,ship.Weapons[0]).Gun.Optimal*.8f):800;
   var delta=aim-t.Position;var direction=delta.LengthSquared()>1?Vector3.Normalize(delta):Vector3.UnitX;Vector3 velocity=p.Order=="Hold"&&p.Contracted?Vector3.Zero:known?(range>desired*1.1f?direction:range<desired*.85f?-direction:Vector3.Normalize(Vector3.Cross(direction,Math.Abs(direction.Y)>.9?Vector3.UnitX:Vector3.Up))):delta.Length()>100?direction:Vector3.Zero;
   if(!p.Contracted&&!known)velocity=Vector3.Zero;t.Velocity=Vector3.Lerp(t.Velocity,velocity*ship.Speed*EwarFor(t.Id).Speed,Math.Min(1,dt*2));t.Position+=t.Velocity*dt;p.Position=t.Position;p.Velocity=t.Velocity;p.Goal=known?"Maintain "+(desired/1000).ToString("0.0")+" km on observed target":p.Order=="Scout"?"Report local visible contacts":p.Contracted?"Formation / "+p.Order:p.Role=="Miner"?"Mine finite local reserve":"Await observations";
   foreach(var ew in ship.Ewar){ew.Cooldown=Math.Max(0,ew.Cooldown-dt);var d=ship.HullEwar(ew.Definition);if(d!=null&&ew.Online&&!ew.Burnt&&known&&t.LockId.Length>0&&ew.Cooldown==0&&t.Cap>=d.Capacitor&&range<=d.Optimal+d.Falloff*2){t.Cap-=d.Capacitor;ew.Cooldown=d.Cycle;ApplyEwarCycle(d,t.Id,p.Target,"population-tackle");}}
   p.WorkLeft=Math.Max(0,p.WorkLeft-dt);if(p.WorkLeft>0||p.Suspended)continue;
   if(p.Role=="Logi"&&p.Contracted&&ActiveEncounterF?.Template.StartsWith("h_")==true){var module=ship.Support[0].Definition;var ally=Targets.Where(o=>o.Friendly&&o.Alive&&o.Id!=t.Id&&o.Profile!=null&&o.Shield<o.Profile.MaxShield&&Vector3.Distance(t.Position,o.Position)<(module?.Optimal??0)).OrderBy(o=>o.Shield/o.Profile!.MaxShield).FirstOrDefault();if(ally!=null&&module!=null&&ship.Support[0].Online&&!ship.Support[0].Burnt&&t.Cap>=module.Capacitor){t.Cap-=module.Capacitor;ally.Shield=Math.Min(ally.Profile!.MaxShield,ally.Shield+module.Amount);ally.Profile.Shield=ally.Shield;p.WorkLeft=module.Cycle;p.Experience++;continue;}}
   if(p.Role=="Logi"&&p.Contracted&&!Player.SpecialNoAssist){var d=ship.Support[0].Definition;if(d!=null&&ship.Support[0].Online&&!ship.Support[0].Burnt&&t.Cap>=d.Capacitor&&Vector3.Distance(t.Position,Player.Position)<d.Optimal&&Player.Shield<Player.MaxShield){t.Cap-=d.Capacitor;Player.Shield=Math.Min(Player.MaxShield,Player.Shield+d.Amount);p.WorkLeft=d.Cycle;p.Experience++;}}
   if(p.Role=="Miner"){
    var rock=p.Contracted?Targets.FirstOrDefault(o=>o.Id==p.Target&&o.Kind==TargetKind.Asteroid):Targets.FirstOrDefault(o=>o.Id=="resource-c:"+SystemId);if(t.Cap>=8&&rock!=null&&rock.Ore>0&&Vector3.Distance(rock.Position,t.Position)<ship.Stat("miningRange")){int q=(int)Math.Min(rock.Ore,ship.Stat("miningYield"));if(p.Contracted)q=Math.Min(q,(int)Math.Max(0,Player.CargoMax-CargoUsed));if(q>0){t.Cap-=8;rock.Ore-=q;if(p.Contracted){if(ActiveEncounterF?.Template.StartsWith("h_")==true&&rock.Id.Contains(":ore:")){rock.Ore+=q;HarvestEncounterF(rock,q);}else Player.Ore+=q;}else{var b=Population.Businesses.Single(b=>b.Id==p.Company);b.Stock["ore"]=b.Stock.GetValueOrDefault("ore")+q;}Population.OreExtracted+=q;p.WorkLeft=(float)ship.Stat("miningCycle");p.Experience++;}}}
  }SyncPopulationGridC();
 }
}
