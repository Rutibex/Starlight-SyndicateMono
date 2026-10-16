using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public string RequestRegionalSurveyN(){if(!Docked)return "Dock at a local survey office.";EnsureRegionalSitesN();string f=RegionalFactionN();int band=Current.Security>=.5?1:Current.Security>0?2:3;int count=0;
  foreach(var p in SiteCatalogN.All.Where(p=>(p.Faction==f||p.Family=="Cache")&&(p.Family is "Data" or "Relic"?p.Rating==band:p.Family=="DED"?band==1?p.Rating<=4:band==2?p.Rating>=4&&p.Rating<=6:p.Rating>=5:p.Leg<=1))){if(Exploration.Encounters.Any(e=>e.System==SystemId&&e.Template==p.Id&&e.Expires>Now))continue;AddSiteN(p.Id,SystemId);count++;}return count==0?"Current survey sites remain available; depleted sites refill after their original expiry.":"Local survey refreshed.";
 }
}
public sealed partial class DemoGame
{
 string siteSelectionN="",siteFilterN="All";
 void DrawRegionalSitesN(DesktopWindow w,int x,int y,int r){
  ui.Text("LOCAL EXPLORATION",x,y,Gold,16);ui.Button(new(r,y-5,565,28),"REFRESH SURVEY AT LOCAL STATION",()=>NoticeF(world.RequestRegionalSurveyN()),"N / survey");y+=40;
  foreach(var pair in new[]{"All","Data","Relic","DED","Anomaly","Unrated","Ghost","Cache","Belt"}.Select((v,i)=>(v,i))){string f=pair.v;ui.Button(new(x+pair.i*130,y,122,26),f,()=>{siteFilterN=f;explorationPageF=0;},"N / filter / "+f,siteFilterN==f);}y+=40;
  var rows=world.Exploration.Encounters.Where(e=>e.System==world.SystemId&&e.ProfileN!=null&&(siteFilterN=="All"||e.ProfileN.Family==siteFilterN&&(world.UtilitySites.First(s=>s.Id=="f:"+e.Id).Signal>=25||e.ProfileN.Public))).ToArray();int row=y;
  foreach(var e in rows.Skip(explorationPageF*9).Take(9)){var site=world.UtilitySites.First(s=>s.Id=="f:"+e.Id);string name=world.ProbeResultName(site);ui.Button(new(x,row,585,31),DemoWorld.ProbeSignatureCode(site)+" / "+ShortInfo(name,44)+" / "+site.Signal.ToString("0.0")+"%",()=>siteSelectionN=e.Id,"N / site / "+e.Id,siteSelectionN==e.Id);row+=37;}
  ui.Button(new(x,row+10,280,28),"PREVIOUS",()=>explorationPageF=Math.Max(0,explorationPageF-1),"N / previous");ui.Button(new(x+292,row+10,290,28),"NEXT",()=>explorationPageF=Math.Min(Math.Max(0,(rows.Length-1)/9),explorationPageF+1),"N / next");
  var pick=world.Exploration.Encounters.FirstOrDefault(e=>e.Id==siteSelectionN);if(pick==null)return;var result=world.UtilitySites.First(s=>s.Id=="f:"+pick.Id);ui.Text(world.ProbeResultName(result),r,y,Gold,16);ui.Text(world.ProbeResultGroup(result)+" / "+pick.State,r,y+35,Cyan,13);
  if(result.Signal>=75||result.Anomaly){ui.Wrap(pick.ProfileN!.Definition.Briefing,r,y+70,560,White,13);ui.Text("Pockets "+pick.ProfileN.Pockets+" / gate hull size "+pick.ProfileN.MaxSize,r,y+160,Muted,12);}
  ui.Button(new(r,y+204,560,30),"WARP TO SITE",()=>{world.ProbeSelection=result.Id;NoticeF(world.WarpUtilitySite()?"Warp initiated.":"Undock and obtain a complete, current warp solution.");},"N / warp");ui.Button(new(r,y+246,275,30),"PROBE SCANNER",OpenProbeMap,"N / scanner");ui.Button(new(r+285,y+246,275,30),"BOOKMARK",()=>NoticeF(world.BookmarkProbeResult(result.Id)?"Bookmark recorded.":"A complete warp solution is required."),"N / bookmark");ui.Button(new(r,y+287,560,30),result.Ignored?"RESTORE RESULT":"IGNORE RESULT",()=>result.Ignored=!result.Ignored,"N / ignore");
  if(result.Signal>=25&&pick.ProfileN!.Family=="Ghost")ui.Wrap("Warning: research containers can detonate. Leave loot behind if necessary; the hidden timer keeps running after departure.",r,y+340,560,Gold,13);
  if(result.Signal>=25&&pick.ProfileN.Family=="Cache")ui.Wrap("Environmental danger remains until the hazard control or distant bypass is hacked. Surviving loot and route decisions persist.",r,y+340,560,Gold,13);
 }
 void SetupCaptureN(){SkillCatalog.VerificationAllV=false;world.NewGame(0);world.SimulationPaused=true;world.EnsureProbeCatalog();desktop.Clear();EnsureWindow(ViewMode.Exploration);explorationTabF=5;siteSelectionN=world.Exploration.Encounters.First(e=>e.ProfileN!=null).Id;
  if(captureScene=="stage-n-hack"){world.UtilityTrial("cruiser");world.FitUtility(UtilityCatalog.All.First(d=>d.Kind=="Data").Id,true,6);var e=world.AddSiteN("n_Guristas_Data_3",world.SystemId);world.UtilitySites.First(s=>s.Id=="f:"+e.Id).Signal=100;world.Undock();world.Player.CombatHull="frigate";world.EnterEncounterF("f:"+e.Id);var target=world.Targets.First(t=>t.SiteKind=="Data");world.Player.Position=target.Position;world.SelectedId=world.LockedId=target.Id;world.ToggleUtility(6);desktop.Clear();utilityTab=2;EnsureWindow(ViewMode.Utilities);}
 }
}
