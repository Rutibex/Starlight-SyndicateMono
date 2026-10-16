using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 string originHeritage41="",originBackground41="",originSchool41="",originTitle41="",originNotice41="";int originTitlePage41,originHeritagePage41,originBackgroundPage41,originSchoolPage41;
 static readonly LoreRecord41[] heritageChoices41=BuildHeritageChoices41();
 static LoreRecord41[] HeritageChoices41()=>heritageChoices41;
 static LoreRecord41[] BuildHeritageChoices41()=>Retheme41.Index.Records.Where(r=>r.Kind=="heritage"&&Retheme41.Index.Records.Any(b=>b.Kind=="background"&&b.Parent==r.Key)&&Retheme41.Index.Records.Any(s=>s.Kind=="school"&&s.Links.Contains(r.Parent))).OrderBy(r=>r.Name).ToArray();
 void SelectHeritage41(string key)
 {
  originHeritage41=key;originBackgroundPage41=0;originSchoolPage41=0;var heritage=Retheme41.Records[key];originBackground41=Retheme41.Index.Records.First(r=>r.Kind=="background"&&r.Parent==key).Key;originSchool41=Retheme41.Index.Records.First(r=>r.Kind=="school"&&r.Links.Contains(heritage.Parent)).Key;
 }
 void OpenCharacterOrigin41()
 {
  if(world.CharacterOrigin is {} origin){originHeritage41=origin.Heritage;originBackground41=origin.Background;originSchool41=origin.School;originTitle41=origin.Title;}
  else{SelectHeritage41(HeritageChoices41().First().Key);originTitle41=Retheme41.Index.Records.First(r=>r.Kind=="title").Key;}
  originNotice41="";originTitlePage41=originHeritagePage41=originBackgroundPage41=originSchoolPage41=0;EnsureWindow(ViewMode.Origins);
 }
 void ApplyCharacterOrigin41()
 {
  string error=world.ChooseCharacterOrigin(originHeritage41,originBackground41,originSchool41,originTitle41);
  if(error.Length>0){originNotice41=error;return;}world.Save();originNotice41="Personal history and title saved.";
 }
 void DrawCharacterOrigin41(DesktopWindow window)
 {
  var r=window.Bounds;int x=r.X+16,y=r.Y+42,w=r.Width-32;ui.Text("Your history among the worlds",x,y,Gold,20);y+=32;
  ui.Wrap("Record where you came from and the vocation you claim. Your campaign role remains "+world.OriginName+"; qualifications still come from training.",x,y,w,Muted,12);y+=48;
  var heritage=Retheme41.Records[originHeritage41];int gap=12,col=(w-gap*3)/4,top=y+29,cap=Math.Max(1,(r.Bottom-228-top-8)/28);
  void DrawChoices(string label,LoreRecord41[] rows,int column,string selected,Action<string> choose,int page,Action<int> setPage)
  {
   int pages=Math.Max(1,(rows.Length+cap-1)/cap);page=Math.Clamp(page,0,pages-1);int cx=x+column*(col+gap);ui.Text(label,cx,y,Cyan,13);int yy=top;
   foreach(var record in rows.Skip(page*cap).Take(cap)){var entry=record;ui.Button(new(cx,yy,col-27,25),ShortInfo(record.Name,Math.Max(12,(col-35)/7)),()=>choose(entry.Key),"Origin choice / "+record.Key,selected==record.Key);ui.Button(new(cx+col-24,yy,24,25),"i",()=>ShowInformation(Retheme41.InformationKey(entry.Key)),"Origin biography / "+record.Key);yy+=28;}
   if(rows.Length>cap){ui.Button(new(cx,r.Bottom-228,(col-8)/2,25),"Previous",()=>setPage(Math.Max(0,page-1)),column==3?"Origin titles previous":"Origin "+label+" previous");ui.Button(new(cx+(col+8)/2,r.Bottom-228,(col-8)/2,25),$"Next {page+1}/{pages}",()=>setPage(Math.Min(pages-1,page+1)),column==3?"Origin titles next":"Origin "+label+" next");}
  }
  var backgrounds=Retheme41.Index.Records.Where(b=>b.Kind=="background"&&b.Parent==originHeritage41).OrderBy(b=>b.Name).ToArray();var schools=Retheme41.Index.Records.Where(s=>s.Kind=="school"&&s.Links.Contains(heritage.Parent)).OrderBy(s=>s.Name).ToArray();var titles=Retheme41.Index.Records.Where(t=>t.Kind=="title").OrderBy(t=>t.Name).ToArray();
  DrawChoices("HERITAGE",HeritageChoices41(),0,originHeritage41,SelectHeritage41,originHeritagePage41,p=>originHeritagePage41=p);DrawChoices("PERSONAL HISTORY",backgrounds,1,originBackground41,k=>originBackground41=k,originBackgroundPage41,p=>originBackgroundPage41=p);DrawChoices("TRAINING SCHOOL",schools,2,originSchool41,k=>originSchool41=k,originSchoolPage41,p=>originSchoolPage41=p);DrawChoices("TITLE",titles,3,originTitle41,k=>originTitle41=k,originTitlePage41,p=>originTitlePage41=p);
  int bottom=r.Bottom-189;ui.Line(new(x,bottom-10),new(r.Right-16,bottom-10),new Color(42,53,61));ui.Text(heritage.Name+" / "+Retheme41.Records[originTitle41].Name,x,bottom,White,15);ui.Wrap(Retheme41.Records[originBackground41].Description,x,bottom+29,w,Muted,12);
  ui.Button(new(x,r.Bottom-71,215,30),"Save personal history",ApplyCharacterOrigin41,"Origin save");ui.Button(new(x+226,r.Bottom-71,215,30),"View pilot record",()=>ShowInformation("pilot/player"),"Origin pilot info");ui.Text(originNotice41,x,r.Bottom-28,Cyan,11);
 }
}
