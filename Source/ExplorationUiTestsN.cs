namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int checksUiN;
 void NCheck(bool ok,string text){UiCheck(ok,"N / "+text);checksUiN++;}
 void RunUiChecksN(int frame){if(captureScene!="stage-n-ui")return;switch(frame){
 case 1:NCheck(ui.Hits.Any(h=>h.Tip=="N / survey"),"regional survey controls are reachable");ClickL("N / survey");NCheck(world.Exploration.Encounters.Count(e=>e.ProfileN!=null)>8,"station survey publishes remaining legal sites");ClickL("N / filter / DED");break;
 case 2:NCheck(siteFilterN=="DED","pointer filters rated sites");NCheck(!ui.Hits.Any(h=>h.Tip.StartsWith("N / site / ")),"unknown site family stays undisclosed in filter");ClickL("N / filter / All");break;
 case 8:break;
 case 3:ClickL("N / bookmark");NCheck(world.Bookmarks.Count==0,"partial discovery rejects bookmark through UI");ClickL("N / warp");NCheck(world.Transit==null,"partial discovery rejects travel through UI");ClickL("N / ignore");NCheck(world.UtilitySites.First(s=>s.Id=="f:"+siteSelectionN).Ignored,"ignore action stores selected result state");break;
 case 4:ClickL("N / ignore");NCheck(!world.UtilitySites.First(s=>s.Id=="f:"+siteSelectionN).Ignored,"restore action reverses ignored result");var selected=world.Exploration.Encounters.First(e=>e.ProfileN?.Family=="DED");siteSelectionN=selected.Id;var site=world.UtilitySites.First(s=>s.Id=="f:"+siteSelectionN);site.Signal=100;world.Undock();break;
 case 5:ClickL("N / bookmark");NCheck(world.Bookmarks.Count==1,"complete solution creates a real bookmark");ClickL("N / warp");NCheck(world.Transit!=null,"complete solution starts actual travel");world.CancelWarp();world.Player.CombatHull="frigate";NCheck(world.EnterEncounterF("f:"+siteSelectionN),"resolved site admits legal ship");explorationTabF=1;break;
 case 6:ClickL("Stage F activate gate");NCheck(world.ActiveEncounterF!.Pocket==0&&noticeF.Contains("locked"),"live opposition locks physical gate through UI");ClickL("Stage F approach gate");NCheck(world.Player.Order=="approach","gate approach issues navigation command");break;
 case 7:Console.WriteLine($"{checksUiN} Stage N UI assertions passed.");break;
 }}
}
