namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public decimal BrokerReferenceD(string station){string corp=ImportedWorld43?Stations44.ById.GetValueOrDefault(station)?.Owner??"":AgentCatalog38.Corporation(station);string faction=ImportedWorld43?Stations44.ById.GetValueOrDefault(station)?.Faction??"":AgentCatalog38.Faction(corp);return Math.Max(.01m,.03m-.003m*Player.SkillLevel("Broker Relations")-.0003m*(decimal)RawStanding38(faction)-.0002m*(decimal)RawStanding38(corp));}
 public decimal MarketEscrowD=>MarketOrders.Where(o=>o.Player&&o.Buy).Sum(o=>o.Price*o.Remaining);
 public HistoryBarD[] TradeHistoryD(string item,string station="")=>MarketTrades.Where(t=>t.Item==item&&(station.Length==0||t.Station==station)).GroupBy(t=>(int)Math.Floor(t.Day)).Select(g=>new HistoryBarD(g.Key,g.Sum(t=>t.Quantity),g.Min(t=>t.Price),g.Max(t=>t.Price),g.Sum(t=>t.Price*t.Quantity)/g.Sum(t=>t.Quantity))).OrderBy(g=>g.Day).ToArray();
 public IEnumerable<WalletEntry> WalletFilterD(string search)=>WalletJournal.Where(e=>search.Length==0||e.Description.Contains(search,StringComparison.OrdinalIgnoreCase)).Reverse();
 public (string Error,decimal Total,BuyLineD[] Lines,Dictionary<string,decimal> Limits) QuoteMultiBuyD(IEnumerable<BuyLineD> draft)
 {
  var raw=draft.ToArray();if(raw.Length==0||raw.Any(l=>l.Quantity<=0||l.Quantity>100000||!ItemCatalog.IsSupported(l.Item)||!LocationExists(l.Station)||LocationRegion(l.Station)!=Current.Region))return ("Enter supported regional items and positive quantities.",0,[],new());
  var lines=raw.GroupBy(l=>(l.Item,l.Station)).Select(g=>new BuyLineD(g.Key.Item,g.Sum(l=>l.Quantity),g.Key.Station)).ToArray();decimal total=0;var limits=new Dictionary<string,decimal>();
  foreach(var l in lines){if(l.Quantity>100000||!LocationService(l.Station,7))return ("Invalid quantity or market service.",0,lines,limits);EnsureLocationMarket(l.Station);int left=l.Quantity;decimal limit=0;
   foreach(var o in MarketOrders.Where(o=>!o.Player&&!o.Buy&&o.Remaining>0&&o.Expires>Day&&o.Item==l.Item&&o.Station==l.Station).OrderBy(o=>o.Price).ThenBy(o=>o.Created)){int q=Math.Min(left,o.Remaining);if(q<Math.Min(o.MinimumVolume,o.Remaining))continue;left-=q;limit=o.Price;if(left==0)break;}
   if(left>0)return ("Insufficient eligible depth for "+ItemName(l.Item)+" at "+LocationName(l.Station),0,lines,limits);total+=l.Quantity*limit;limits[l.Station+":"+l.Item]=limit;
  }return (Balance<total?"Insufficient funds for the entire multibuy.":"",total,lines,limits);
 }
 public string MultiBuyD(IEnumerable<BuyLineD> lines){var q=QuoteMultiBuyD(lines);if(q.Error.Length>0)return q.Error;foreach(var l in q.Lines){string result=SubmitMarket(l.Item,l.Station,true,l.Quantity,q.Limits[l.Station+":"+l.Item],0,true);if(!result.StartsWith("Trade complete"))throw new InvalidOperationException("A validated multibuy unexpectedly changed: "+result);}HistoryD("Multibuy settled / "+q.Lines.Length+" lines / "+q.Total+" CR");return "";}
 public string RedeemLoyaltyD(string offer)
 {
  if(!Docked||!LocationService(StationKey44,7))return "Dock at a corporation service station.";string corporation=StationCorporation;int lp=offer=="rail-bpc"?300:offer=="voucher"?150:0;decimal cash=offer=="rail-bpc"?100:offer=="voucher"?50:0;
  if(lp==0)return "Unknown loyalty offer.";if(AgentsState.Loyalty.GetValueOrDefault(corporation)<lp||Balance<cash||Hangar.GetValueOrDefault("d_tag")<1||RawStanding38(corporation)<.1)return "Requires corporation standing 0.1, LP, credits and one service tag.";
  AgentsState.Loyalty[corporation]-=lp;WalletChange(-cash,"Loyalty store / "+offer);Commander.ServiceSinks+=cash;ChangeStack("d_tag",true,-1);
  if(offer=="rail-bpc")Industry.Blueprints.Add(new(){Item="rail",Station=StationKey44,Original=false,Runs=3});else ChangeStack("d_faction_token",true,1);HistoryD("Redeemed corporation LP offer / "+offer);return "";
 }
 public string StartResearchD()
 {
  if(!Docked||!LocationService(StationKey44,15))return "Dock at a research facility.";if(Skills.Trained(SkillCatalog.Id("Science"))<3||RawStanding38(StationCorporation)<.1)return "Requires Science III and corporation standing 0.1.";
  if(Commander.Research.Any(r=>!r.Claimed&&r.Station==StationKey44))return "Claim the existing research batch first.";if(Balance<100||Hangar.GetValueOrDefault("salvage")<5)return "Research requires 100 CR and five recovered components.";
  WalletChange(-100,"Datacore research installation");Commander.ServiceSinks+=100;ChangeStack("salvage",true,-5);Commander.Research.Add(new(){Corporation=StationCorporation,Station=StationKey44,Finish=Now.AddHours(24),Quantity=5});HistoryD("Finite datacore research batch installed");return "";
 }
 public string ClaimResearchD(string id){var r=Commander.Research.FirstOrDefault(r=>r.Id==id);if(r==null||r.Claimed||r.Finish>Now||!Docked||StationKey44!=r.Station)return "Research must finish and be claimed locally once.";r.Claimed=true;ChangeStack("d_datacore",true,r.Quantity);HistoryD("Research output claimed / "+id);return "";}
 public string RedeemDatacoresD(){if(!Docked||Hangar.GetValueOrDefault("d_datacore")<5||Balance<100)return "Dock with five datacores and 100 CR.";ChangeStack("d_datacore",true,-5);WalletChange(-100,"Research trader blueprint copy");Commander.ServiceSinks+=100;Industry.Blueprints.Add(new(){Item="analyzer",Station=StationKey44,Original=false,Runs=2});HistoryD("Datacores redeemed for a two-run analyzer blueprint copy");return "";}
}
