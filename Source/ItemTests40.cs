using System.Text.Json;
namespace StarlightSyndicateDemo;
public static class ItemTests40
{
 public static void Run()
 {
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception("ITEM DATABASE FAIL: "+label);checks++;}
  Check(UnifiedCatalog40.Index.Build==3569502&&UnifiedCatalog40.Types.Count==53065,"Current complete type registry");
  Check(UnifiedCatalog40.Index.TypeLists.Count==467&&UnifiedCatalog40.Markets.Count==2114&&UnifiedCatalog40.MetaGroups.Count==13,"All relationships imported");
  foreach(var t in UnifiedCatalog40.Types.Values){Check(UnifiedCatalog40.Groups.ContainsKey(t.GroupId),"Type group "+t.Id);Check(UnifiedCatalog40.Categories.ContainsKey(UnifiedCatalog40.CategoryId(t)),"Type category "+t.Id);Check(t.MarketGroupId==null||UnifiedCatalog40.MarketPaths.ContainsKey(t.MarketGroupId.Value),"Type market "+t.Id);Check(UnifiedCatalog40.Variations(t.Id).Contains(t.Id),"Variation family "+t.Id);}
  foreach(var pair in UnifiedCatalog40.Index.Aliases)Check(UnifiedCatalog40.SourceId(pair.Key)==pair.Value&&UnifiedCatalog40.Types.ContainsKey(pair.Value),"Alias "+pair.Key);
  foreach(var item in ItemCatalog.All){Check(ReferenceEquals(ItemCatalog.Get(item.Id),item),"Original item identity "+item.Id);Check(item.Supported,"Support is explicit "+item.Id);if(item.SourceType!=null)Check(UnifiedCatalog40.GameAliases[item.SourceTypeId].Contains(item.Id)||item.Id.StartsWith("ind_"),"Reverse alias "+item.Id);}
  foreach(var h in FleetCatalog.All)Check(h.SourceType!=null&&UnifiedCatalog40.CategoryId(h.SourceType)==6,"Every fleet hull has current source identity "+h.Id);
  var indices=MarketGroups.Root.Descendants().ToArray();Check(indices.Length==ItemCatalog.All.Length&&indices.Distinct().Count()==indices.Length,"Exactly one market leaf per supported item");
  var nodes=new List<MarketGroup>();void Visit(MarketGroup n){nodes.Add(n);foreach(var child in n.Children)Visit(child);}Visit(MarketGroups.Root);
  Check(nodes.Count(n=>n.SourceId!=null)==2114&&nodes.Where(n=>n.SourceId!=null).Select(n=>n.SourceId).Distinct().Count()==2114,"Complete source market hierarchy without duplicate nodes");
  Check(DroneCatalog.All.All(d=>UnifiedCatalog40.Source(d.Id)!=null),"Every supported drone has an explicit source alias");
  Check(UnifiedCatalog40.MarketPaths.Values.Distinct().Count()==2114,"Market paths remain unambiguous");
  Check(UnifiedCatalog40.Source("sde_0")?.Id==0&&UnifiedCatalog40.Source("unmapped_authored_item")==null,"Source zero and unmapped identities are distinct");
  Check(UnifiedCatalog40.SourceId("sde_587")==587&&UnifiedCatalog40.SourceId("type:587")==587,"Canonical lookup");
  Check(UnifiedCatalog40.Variations(12005).Contains(626),"Ishtar source family points to Vexor");
  Check(UnifiedCatalog40.InTypeList(10,587),"Type list includes ship category");
  var info=UnifiedCatalog40.Information(587);Check(info.MarketId==""&&info.Tabs.ContainsKey("Source dogma")&&info.Stats.Count>0,"Reference info has raw attributes and no market action");
  Check(InformationCatalog.All.Count(e=>e.Kind=="Reference type")==53065,"Every type is searchable");
  foreach(int id in new[]{34,587,12005,97620})Check(UnifiedCatalog40.Information(id).Id==UnifiedCatalog40.ReferenceId(id),"Lazy detail shard "+id);
  foreach(var t in UnifiedCatalog40.Types.Values.GroupBy(t=>t.Id/1000).Select(g=>g.First()))Check(UnifiedCatalog40.Information(t.Id).Name==Retheme41.TypeName(t.Id),"Cross-shard lookup "+t.Id);
  Check(UnifiedCatalog40.Information(587).Name==Retheme41.TypeName(587),"Detail cache eviction reloads correctly");
  var w=new DemoWorld();w.NewGame(0);w.Screen=ViewMode.Station;w.Credits=10000;w.Hangar["sde_587"]=1;
  float cash=w.Credits;int orders=w.MarketOrders.Count;
  Check(w.SubmitMarket("sde_587",w.SystemId,true,1,1,0,true).Contains("cannot be traded")&&w.Credits==cash&&w.MarketOrders.Count==orders,"Unsupported purchases rejected atomically");
  var unsupportedOrder=new MarketOrder{Item="sde_587",Station=w.SystemId,Buy=true,Player=true,Remaining=1,Price=1,Expires=w.Day+10};w.MarketOrders.Add(unsupportedOrder);
  Check(w.ModifyMarketOrder(unsupportedOrder.Id,2).Contains("cannot be traded")&&w.Credits==cash,"Loaded reference orders cannot be repriced");
  w.Day+=1;w.AdvanceMarket();Check(unsupportedOrder.Remaining==1&&w.Hangar["sde_587"]==1,"Loaded reference orders cannot acquire equipment through dealer ticks");
  Check(!w.FitItem("sde_587",true)&&w.Hangar["sde_587"]==1,"Unsupported fitting rejected without consuming stack");
  Check(w.VisibleInventoryIds(true).Contains("sde_587")&&DemoWorld.ItemName("sde_587")==Retheme41.TypeName(587),"Reference inventory stack remains inspectable");
  w.Hangar["sde_34"]=2;Check(w.TransferItem("sde_34",true,1)&&w.ItemCount("sde_34",false)==1,"Known reference cargo can be moved without enabling equipment");
  var copy=JsonSerializer.Deserialize<DemoWorld>(JsonSerializer.Serialize(w,FittingDesignStore.Json),FittingDesignStore.Json)!;
  Check(copy.Hangar["sde_587"]==1&&copy.CargoItems["sde_34"]==1&&copy.Player.UtilityFit==w.Player.UtilityFit,"Saved inventory and fitted identities round trip unchanged");
  Check(!ItemCatalog.Contains("sde_99999999")&&!ItemCatalog.IsSupported("type:587"),"Unknown IDs and source aliases are never implicitly supported");
  Console.WriteLine($"{checks} Pass 40 item database checks passed / {ItemCatalog.All.Length} supported game items / {UnifiedCatalog40.Types.Count} source types / {UnifiedCatalog40.Index.Aliases.Count} explicit aliases.");
 }
}
