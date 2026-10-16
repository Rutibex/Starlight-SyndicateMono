namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunItemsUiChecks40(int stage)
 {
  if(stage>5)return;
  void Check(bool ok,string label)=>UiCheck(ok,"Pass 40: "+label);
  switch(stage){
   case 0:
    Check(encyclopediaEntries.Count(e=>e.Kind=="Reference type")==53065,"all source types appear in encyclopedia");
    encyclopediaQuery=Retheme41.TypeName(587);encyclopediaGroup="";var found=EncyclopediaResults();
    Check(found.Any(e=>e.Key=="item/sde_587"),"source name search finds exact canonical record");
    Check(ReferenceEquals(found,EncyclopediaResults()),"unchanged search reuses cached results");encyclopediaResultScroll=Array.FindIndex(found,e=>e.Key=="item/sde_587");break;
   case 1:
    TestClick(ui.Hits.Last(h=>h.Tip=="Encyclopedia result / item/sde_587").Rect.Center.ToVector2());
    Check(encyclopediaSelected=="item/sde_587","click selects source record");
    ShowInformation(encyclopediaSelected);break;
   case 2:
    var reference=desktop.Last(w=>w.Kind==ViewMode.Information);var info=ResolveInformation(reference.InfoKey)!;
    Check(info.Kind=="Reference type"&&info.MarketId==""&&info.Tabs.ContainsKey("Source dogma"),"source info exposes dogma without purchase action");
    TestClick(ui.Hits.Last(h=>h.Owner==reference.Owner&&h.Tip=="Info tab / Game aliases").Rect.Center.ToVector2());
    Check(reference.InfoTab=="Game aliases","alias tab receives click");break;
   case 3:
    var window=desktop.Last(w=>w.Kind==ViewMode.Information);var alias=UnifiedCatalog40.GameAliases[587].First(ItemCatalog.IsSupported);
    TestClick(ui.Hits.Last(h=>h.Owner==window.Owner&&h.Tip=="Info link / item/"+alias).Rect.Center.ToVector2());
    Check(window.InfoKey=="item/"+alias&&ResolveInformation(window.InfoKey)!.MarketId==alias,"source alias link opens supported hull information");break;
   case 4:
    encyclopediaQuery="type 12005";Check(EncyclopediaResults().Any(e=>e.Key=="item/sde_12005"),"canonical numeric search finds newer ship");
    OpenEncyclopedia("item/sde_12005");break;
   case 5:
    Check(encyclopediaSelected=="item/sde_12005"&&ResolveInformation(encyclopediaSelected)!.Related.Contains("item/sde_626"),"reference variations resolve true source lineage");
    Console.WriteLine("Pass 40 source database UI: 9 checks passed.");break;
  }
 }
}
