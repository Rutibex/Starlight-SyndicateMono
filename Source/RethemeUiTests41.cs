namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int rethemeUiChecks41;
 void RunRethemeUiChecks41(int stage)
 {
  if(stage>7)return;void Check(bool ok,string label){UiCheck(ok,"Pass 41: "+label);rethemeUiChecks41++;}
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  switch(stage){
   case 0:
    Check(desktop.Any(w=>w.Kind==ViewMode.Origins),"personal history window opens");Check(HeritageChoices41().Length>=12,"all core diaspora heritages available");Click("Origin choice / heritage:7");Check(originHeritage41=="heritage:7","heritage click selects Boreal background");break;
   case 1:
    Click("Origin HERITAGE next");Check(originHeritagePage41==1,"heritage pagination reaches remaining diaspora choices");Click("Origin HERITAGE previous");Check(originHeritagePage41==0,"heritage pagination returns without changing selection");
    var title=Retheme41.Index.Records.Where(t=>t.Kind=="title").OrderBy(t=>t.Name).First();Click("Origin choice / "+title.Key);Check(originTitle41==title.Key,"title click selects profession");Click("Origin titles next");Check(originTitlePage41==1,"title pagination reaches further titles");break;
   case 2:
    Click("Origin save");Check(world.CharacterOrigin?.Heritage=="heritage:7"&&world.CharacterOrigin.Title==originTitle41&&originNotice41.Contains("saved"),"background selection persists");Click("Origin pilot info");break;
   case 3:
    var info=desktop.Last(w=>w.Kind==ViewMode.Information);Click("Info tab / Background");Check(info.InfoTab=="Background"&&ResolveInformation("pilot/player")!.Tabs["Background"].Count==4,"pilot record exposes linked personal history");break;
   case 4:
    var record=desktop.Last(w=>w.Kind==ViewMode.Information);Click("Info link / lore/heritage:7");Check(record.InfoKey=="lore/heritage:7"&&ResolveInformation(record.InfoKey)!.Name.Contains("Boreal"),"pilot heritage link opens authored biography");OpenEncyclopedia("lore/corporation:1000002");break;
   case 5:
    Check(encyclopediaEntries.Any(e=>e.Key=="lore/corporation:1000002")&&ResolveInformation(encyclopediaSelected)!.Description.Contains("Amur"),"corporation identities are searchable with original lore");OpenEncyclopedia("item/sde_12005");break;
   case 6:
    Check(ResolveInformation(encyclopediaSelected)!.Name==Retheme41.TypeName(12005)&&!Retheme41.SourceLorePattern.IsMatch(ResolveInformation(encyclopediaSelected)!.Description),"default pattern page uses Starlight display identity");break;
   case 7:
    Check(UnifiedCatalog40.Information(12005).Tabs["Source audit"].Any(r=>r.Label=="Source name"&&r.Value=="Ishtar"),"raw source remains explicitly auditable");Console.WriteLine($"Pass 41 retheme and origin UI: {rethemeUiChecks41} checks passed.");break;
  }
 }
}
