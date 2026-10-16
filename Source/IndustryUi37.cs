using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int industryTab,industryScroll,industryRuns=1,industryCopies=1,industryTarget=1,industryMaterialScroll;string industrySelected="",industryNotice="",industrySearch="",industryFilter="All",industryStation="Current station",industryFocus="";bool industryNative;IndustryActivity industryActivity;Dictionary<string,long> reprocessSelection=[];int reprocessScroll;
 void IndustryText(char c){if(c=='\b'){if(industrySearch.Length>0)industrySearch=industrySearch[..^1];}else if(!char.IsControl(c)&&industrySearch.Length<80)industrySearch+=c;industryScroll=0;}
 void IndustryDo(Func<string> action){industryNotice=action();if(industryNotice.Length==0){industryNotice="Completed.";world.Save();}}
 void OpenIndustryItem(string id){EnsureWindow(ViewMode.Industry);industryTab=0;industrySelected=world.Industry.Blueprints.FirstOrDefault(b=>b.Item==id&&b.Station==world.StationKey44)?.Id??"";industrySearch=DemoWorld.ItemName(id);industryScroll=0;}
 void OpenReprocessing(string id,long count){EnsureWindow(ViewMode.Reprocessing);reprocessSelection[id]=count;}
 void DrawIndustry(DesktopWindow w)
 {
  var r=w.Bounds;int x=r.X+14,y=r.Y+40,width=r.Width-28;string[] tabs=["Blueprints","Jobs","Facilities","Blueprint market"];
  for(int i=0;i<4;i++){int n=i;ui.Button(new(x+i*155,y,148,29),tabs[i],()=>{industryTab=n;industryScroll=0;},active:industryTab==i);}ui.Button(new(r.Right-410,y,220,29),"Source activities / research",()=>OpenQ(ViewMode.IndustryQ),"Q industry link");ui.Text($"{world.Balance:N0} CR",r.Right-170,y+8,Gold,12);y+=42;
  if(industryTab==1){DrawIndustryJobs(x,y,width,r.Bottom-76);}
  else if(industryTab==2){DrawIndustryFacilities(x,y,width,r.Bottom-76);}
  else
  {
   int left=310;ui.Button(new(x,y,left,28),industrySearch.Length==0?"Search blueprints...":ShortInfo(industrySearch,36),()=>industryFocus="search",active:industryFocus.Length>0);
   ui.Button(new(x,y+34,148,26),industryFilter,()=>ShowPopup("Blueprint filter",new[]{"All","Originals","Copies","Ships","Modules","Ammunition","Materials"}.Select(s=>(s,(Action)(()=>{industryFilter=s;industryScroll=0;}))).ToList()));
   ui.Button(new(x+154,y+34,156,26),industryStation,()=>{industryStation=industryStation=="Current station"?"All stations":"Current station";industryScroll=0;});
   int top=y+70,bottom=r.Bottom-86,visible=Math.Max(1,(bottom-top)/43);
   if(industryTab==3)
   {
    var items=ItemCatalog.All.Where(i=>MatchesIndustry(i)&&!IndustryCatalog37.MineralTypes.Contains(IndustryCatalog37.Type(i.Id))).OrderBy(i=>i.Name).ToArray();industryScroll=Math.Clamp(industryScroll,0,Math.Max(0,items.Length-visible));
    for(int n=industryScroll;n<Math.Min(items.Length,industryScroll+visible);n++){var item=items[n];int yy=top+(n-industryScroll)*43;ui.Button(new(x,yy,left,38),ShortInfo(item.Name,35),()=>{industrySelected="catalog/"+item.Id;},active:industrySelected=="catalog/"+item.Id);}
    if(industrySelected.StartsWith("catalog/")){string id=industrySelected[8..];var recipe=IndustryCatalog37.Recipes[id];int right=x+left+20;ui.Text(DemoWorld.ItemName(id),right,y,Cyan,18);ui.Wrap(recipe.Provenance,right,y+36,width-left-26,Muted,12);ui.Text($"Original licence: {world.BlueprintPrice(id):N0} CR",right,y+83,Gold,16);ui.Button(new(right,y+118,270,34),"Buy blueprint original",()=>IndustryDo(()=>world.BuyBlueprint(id)));ui.Wrap("Every supported catalogue product has an authored licence. Advanced component designs may have separate acquisition rules.",right,y+174,width-left-28,Muted,13);DrawIndustryRecipePreview(id,right,y+260,width-left-28,bottom);}
   }
   else
   {
    var owned=world.Industry.Blueprints.Where(b=>(industryStation=="All stations"||b.Station==world.StationKey44||b.Station=="cargo")&&MatchesIndustry(ItemCatalog.Get(b.Item))&&(industryFilter!="Originals"||b.Original)&&(industryFilter!="Copies"||!b.Original)).OrderBy(b=>DemoWorld.ItemName(b.Item)).ToArray();industryScroll=Math.Clamp(industryScroll,0,Math.Max(0,owned.Length-visible));
    for(int n=industryScroll;n<Math.Min(owned.Length,industryScroll+visible);n++){var bp=owned[n];int yy=top+(n-industryScroll)*43;ui.Button(new(x,yy,left,39),ShortInfo(DemoWorld.ItemName(bp.Item),30),()=>{industrySelected=bp.Id;industryTarget=Math.Min(10,(industryActivity==IndustryActivity.TimeResearch?bp.TE/2:bp.ME)+1);industryMaterialScroll=0;},active:industrySelected==bp.Id);ui.Text($"{(bp.Original?"BPO":"BPC / "+bp.Runs)}  ME {bp.ME}% TE {bp.TE}% / {bp.Station}",x+8,yy+25,bp.LockedJob.Length>0?Gold:Muted,9);}
    var selected=world.Industry.Blueprints.FirstOrDefault(b=>b.Id==industrySelected);if(selected!=null)DrawIndustrySetup(selected,x+left+20,y,width-left-20,bottom);else ui.Wrap("Select an owned blueprint or buy one from Blueprint market. Originals are blue; finite copies retain the original's research.",x+left+20,y+60,width-left-30,Muted,16);
   }
   ui.Button(new(x,r.Bottom-75,90,25),"Previous",()=>industryScroll=Math.Max(0,industryScroll-visible));ui.Button(new(x+96,r.Bottom-75,90,25),"Next",()=>industryScroll+=visible);
  }
  ui.Wrap(industryNotice,x,r.Bottom-37,width,Gold,12);
 }
 bool MatchesIndustry(ItemDefinition item)=>item.Name.Contains(industrySearch,StringComparison.OrdinalIgnoreCase)&&(industryFilter switch{"Ships"=>item.Slot=="ship","Modules"=>item.Slot is "high" or "medium" or "low" or "rig","Ammunition"=>item.Slot=="charge","Materials"=>item.Group.StartsWith("Materials")||item.Group.StartsWith("Industry"),_=>true});
 void DrawIndustryRecipePreview(string id,int x,int y,int width,int bottom)
 {
  var recipe=IndustryCatalog37.Recipes[id];ui.Text($"Output {recipe.Definition.Output} / run | base {Duration(recipe.Definition.Seconds)}",x,y,White,13);y+=29;ui.Wrap(string.Join(", ",recipe.Definition.Skills.Select(s=>(SkillCatalog.ById.GetValueOrDefault(s.Skill)?.Name??s.Skill.ToString())+" "+Roman(s.Level))),x,y,width,Muted,12);y+=48;
  foreach(var m in recipe.Minerals){if(y>bottom-25)break;ui.Text(DemoWorld.ItemName(m.Key),x,y,White,12);ui.Text(m.Value.ToString("N0"),x+width-140,y,Cyan,12);y+=25;}
 }
 void DrawIndustrySetup(OwnedBlueprint37 bp,int x,int y,int width,int bottom)
 {
  var recipe=IndustryCatalog37.Recipes[bp.Item];ui.Text(ShortInfo(DemoWorld.ItemName(bp.Item),(width-265)/10),x,y,Cyan,19);y+=31;ui.Text($"{(bp.Original?"ORIGINAL":"COPY / "+bp.Runs+" runs")}  ME {bp.ME}%  TE {bp.TE}%  / {bp.Station}",x,y,White,12);y+=27;
  ui.Button(new(x+width-245,y-52,245,26),bp.Station=="cargo"?"Deposit blueprint into hangar":"Move blueprint to ship cargo",()=>IndustryDo(()=>world.MoveIndustryBlueprint(bp.Id)));
  string[] names=["Manufacturing","Material research","Time research","Copying"];for(int i=0;i<4;i++){var a=(IndustryActivity)i;ui.Button(new(x+i*(width/4),y,width/4-5,30),names[i],()=>{industryActivity=a;industryTarget=Math.Min(10,(a==IndustryActivity.TimeResearch?bp.TE/2:bp.ME)+1);},active:industryActivity==a);}y+=40;
  ui.Button(new(x,y,width,27),industryNative?"Recipe: CCP component chain":"Recipe: Starlight mineral fabrication",()=>{industryNative=!industryNative;industryMaterialScroll=0;},recipe.Provenance);y+=37;
  void Quantity(string label,int value,int xx,Action<int> setter,int max){ui.Text(label,xx,y+7,Muted,12);ui.Button(new(xx+115,y,28,27),"-",()=>setter(Math.Max(1,value-1)));ui.Button(new(xx+147,y,80,27),value.ToString(),()=>ShowPopup(label,new[]{1,5,10,50,100,600}.Where(n=>n<=max).Select(n=>(n.ToString(),(Action)(()=>setter(n)))).ToList()));ui.Button(new(xx+231,y,28,27),"+",()=>setter(Math.Min(max,value+1)));}
  if(industryActivity==IndustryActivity.Manufacturing||industryActivity==IndustryActivity.Copying){Quantity(industryActivity==IndustryActivity.Copying?"Runs per copy":"Job runs",industryRuns,x,v=>industryRuns=v,industryActivity==IndustryActivity.Copying?recipe.Definition.MaxRuns:100000);if(industryActivity==IndustryActivity.Copying)Quantity("Copies",industryCopies,x+width/2,v=>industryCopies=v,1000);}
  else Quantity("Target level",industryTarget,x,v=>industryTarget=v,10);y+=37;
  var q=world.QuoteIndustry(bp.Id,industryActivity,industryRuns,industryCopies,industryTarget,industryNative);ui.Text($"Duration {Duration(q.Seconds)}   Installation {q.Fee:N2} CR",x,y,Gold,14);y+=26;ui.Text($"Index {world.IndustryIndex(bp.Station,industryActivity):P1} / facility tax {(world.Industry.Facility=="Station"?.0025:.01):P2} / surcharge 4%",x,y,Muted,11);y+=24;
  var requirements=industryActivity==IndustryActivity.Manufacturing?IndustryCatalog37.BuildSkills(recipe,industryNative):recipe.Definition.ActivitySkills.GetValueOrDefault(industryActivity switch{IndustryActivity.MaterialResearch=>"research_material",IndustryActivity.TimeResearch=>"research_time",_=>"copying"},[]);
  ui.Button(new(x,y,width,26),"Build skills / add required training",()=>{IndustryDo(()=>world.Skills.Add(requirements));EnsureWindow(ViewMode.Skills);},string.Join(", ",requirements.Select(s=>(SkillCatalog.ById.GetValueOrDefault(s.Skill)?.Name??s.Skill.ToString())+" "+s.Level)));y+=34;
  ui.Text("INPUT MATERIAL                         REQUIRED / AVAILABLE",x,y,Cyan,11);if(industryActivity==IndustryActivity.Manufacturing)ui.Button(new(x+width-230,y-5,230,22),"Buy missing local materials",()=>IndustryDo(()=>world.BuyIndustryInputs(bp.Id,industryNative,industryRuns)));y+=24;var inputs=q.Inputs.ToArray();int visible=Math.Max(1,(bottom-y-220)/24);industryMaterialScroll=Math.Clamp(industryMaterialScroll,0,Math.Max(0,inputs.Length-visible));
  for(int n=industryMaterialScroll;n<Math.Min(inputs.Length,industryMaterialScroll+visible);n++){var m=inputs[n];long available=(long)world.StorageAt(bp.Station).GetValueOrDefault(m.Key);ui.Text(ShortInfo(DemoWorld.ItemName(m.Key),(width-245)/7),x,y,White,12);ui.Text($"{m.Value:N0} / {available:N0}",x+width-230,y,available>=m.Value?Green:Red,12);y+=24;}
  if(inputs.Length>visible){ui.Button(new(x,y,90,24),"Materials <",()=>industryMaterialScroll=Math.Max(0,industryMaterialScroll-visible));ui.Button(new(x+96,y,90,24),"Materials >",()=>industryMaterialScroll+=visible);}
  int flowY=bottom-164;var center=new Vector2(x+width/2,flowY);ui.Line(new(x+165,flowY),center-new Vector2(43,0),Muted);ui.Line(center+new Vector2(43,0),new(x+width-165,flowY),Cyan);for(int n=0;n<40;n++){float a=n*MathF.Tau/40,b=(n+1)*MathF.Tau/40;ui.Line(center+new Vector2(MathF.Cos(a),MathF.Sin(a))*40,center+new Vector2(MathF.Cos(b),MathF.Sin(b))*40,bp.Original?Cyan:Gold,2);}ui.Center("BP",center.X,center.Y-8,bp.Original?Cyan:Gold,19);ui.Center("MATERIALS + LICENCE",x+120,flowY-24,Muted,11);string output=industryActivity switch{IndustryActivity.Manufacturing=>$"{(long)industryRuns*recipe.Definition.Output:N0} x {DemoWorld.ItemName(bp.Item)}",IndustryActivity.Copying=>$"{industryCopies} copies / {industryRuns} runs",IndustryActivity.MaterialResearch=>$"ME {industryTarget}% original",_=>$"TE {industryTarget*2}% original"};ui.Center(ShortInfo(output,35),x+width-145,flowY-24,Green,12);ui.Center("OUTPUT",x+width-145,flowY+16,Muted,10);
  ui.Wrap(q.Error.Length==0?"Ready to install. Inputs and fees are consumed immediately.":q.Error,x,bottom-99,width,q.Error.Length==0?Green:Red,12);
  ui.Button(new(x,bottom-55,width,36),"START JOB",()=>IndustryDo(()=>world.InstallIndustry(bp.Id,industryActivity,industryRuns,industryCopies,industryTarget,industryNative)),q.Error);
 }
 void DrawIndustryJobs(int x,int y,int width,int bottom)
 {
  world.UpdateIndustry(world.Now);ui.Text($"Manufacturing slots {world.IndustrySlots(true)} / Research slots {world.IndustrySlots(false)}",x,y,Cyan,14);y+=34;
  var jobs=world.Industry.Jobs.AsEnumerable().Reverse().ToArray();int visible=Math.Max(1,(bottom-y-42)/57);industryScroll=Math.Clamp(industryScroll,0,Math.Max(0,jobs.Length-visible));
  for(int n=industryScroll;n<Math.Min(jobs.Length,industryScroll+visible);n++){var j=jobs[n];int yy=y+(n-industryScroll)*57;ui.Box(new(x,yy,width,51),new Color(20,32,40));ui.Text(ShortInfo(DemoWorld.ItemName(j.Item),55),x+10,yy+7,White,14);ui.Text($"{j.Activity} / {j.Station} / {j.Runs} runs / {j.State}",x+10,yy+29,Muted,12);float progress=(float)Math.Clamp(1-(j.FinishUtc-world.Now).TotalSeconds/j.Seconds,0,1);ui.Bar(new(x+width-370,yy+11,210,6),progress,Cyan);ui.Text(j.State=="Active"?Duration((j.FinishUtc-world.Now).TotalSeconds):j.State,x+width-370,yy+25,Gold,12);
   if(j.State=="Ready")ui.Button(new(x+width-141,yy+10,130,31),"DELIVER",()=>IndustryDo(()=>world.DeliverIndustry(j.Id)));else if(j.State=="Active")ui.Button(new(x+width-141,yy+10,130,31),"Cancel",()=>ShowPopup("Cancel job: inputs and fees are lost",[("Cancel permanently",()=>IndustryDo(()=>world.CancelIndustry(j.Id))),("Keep running",()=>{})]));}
  ui.Button(new(x,bottom-32,110,27),"Previous",()=>industryScroll=Math.Max(0,industryScroll-visible));ui.Button(new(x+116,bottom-32,110,27),"Next",()=>industryScroll+=visible);ui.Button(new(x+240,bottom-32,200,27),"Deliver all ready jobs",()=>{foreach(var j in world.Industry.Jobs.Where(j=>j.State=="Ready").ToArray())world.DeliverIndustry(j.Id);world.Save();});
 }
 void DrawIndustryFacilities(int x,int y,int width,int bottom)
 {
  ui.Wrap("Blueprints and inputs must already be at the facility. Supply Chain Management and Scientific Networking extend installation range by 5 jumps per level. Finished output remains at its installation station.",x,y,width,Muted,14);y+=82;
  foreach(var station in world.IndustryLocations()){ui.Text(station.Name+" / "+station.Id,x,y,White,14);ui.Text($"{world.JumpDistance(world.StationKey44,station.Id)} jumps | cost index {world.IndustryIndex(station.Id,industryActivity):P1}",x+400,y,Cyan,13);y+=28;}
  y+=17;ui.Button(new(x,y,370,32),"Facility: "+world.Industry.Facility,()=>{if(world.ImportedWorld43)return;world.Industry.Facility=world.Industry.Facility=="Station"?"Engineering complex":"Station";world.Save();});y+=48;ui.Wrap("Station: no material or time bonuses, 0.25% industry tax. Engineering complex: 1% material saving, 15% time saving, 1% tax. These two public service profiles are a Starlight extension; no structure construction is required. Timers use simulated time. Pause freezes them; docked waiting advances them. Imported facilities use their source service flags.",x,y,width,Muted,13);
 }
 void DrawReprocessing(DesktopWindow w)
 {
  var r=w.Bounds;int x=r.X+16,y=r.Y+43,width=r.Width-32;ui.Text("REPROCESSING  /  "+world.Current.Name,x,y,Cyan,17);y+=34;int left=width/2-15;
  var ids=DemoWorld.InventoryIds.Where(id=>world.Hangar.GetValueOrDefault(id)>=1&&!IndustryCatalog37.MineralTypes.Contains(IndustryCatalog37.Type(id))).ToArray();int visible=Math.Max(1,(r.Height-220)/39);reprocessScroll=Math.Clamp(reprocessScroll,0,Math.Max(0,ids.Length-visible));
  ui.Text("INPUT HANGAR / click to add or remove",x,y,Muted,12);y+=26;
  for(int n=reprocessScroll;n<Math.Min(ids.Length,reprocessScroll+visible);n++){string id=ids[n];int yy=y+(n-reprocessScroll)*39;ui.Button(new(x,yy,left,34),ShortInfo(DemoWorld.ItemName(id),Math.Max(12,left/8))+" / "+world.Hangar[id].ToString("N0"),()=>{if(!reprocessSelection.Remove(id))reprocessSelection[id]=(long)Math.Floor(world.Hangar[id]);},active:reprocessSelection.ContainsKey(id));}
  var q=world.QuoteReprocess(reprocessSelection);int right=x+left+25,yy2=r.Y+101;ui.Text("OUTPUT MINERALS / AFTER TAX",right,yy2,Cyan,13);yy2+=31;
  foreach(var m in q.Outputs){if(yy2>r.Bottom-140)break;ui.Text(DemoWorld.ItemName(m.Key),right,yy2,White,12);ui.Text(m.Value.ToString("N0"),r.Right-130,yy2,Green,13);ui.Text($"Gross {q.Gross.GetValueOrDefault(m.Key):N0} / loss + tax {q.Gross.GetValueOrDefault(m.Key)-m.Value:N0}",right,yy2+16,Muted,9);yy2+=38;}
  ui.Wrap($"Equipment 50%. Ore: Reprocessing, Efficiency and ore skill bonuses. Scrap: Scrapmetal Processing only (55% maximum). Tax {q.Tax:P1} is withheld from output. Incomplete ore batches stay in the hangar.",right,Math.Max(yy2+15,r.Bottom-220),left-10,Muted,12);
  ui.Button(new(x,r.Bottom-108,90,26),"Previous",()=>reprocessScroll=Math.Max(0,reprocessScroll-visible));ui.Button(new(x+96,r.Bottom-108,90,26),"Next",()=>reprocessScroll+=visible);ui.Button(new(x+194,r.Bottom-108,100,26),"Clear",()=>reprocessSelection.Clear());
  ui.Wrap(q.Error.Length==0?$"Ready / estimated withheld value {q.Fee:N2} CR":q.Error,x,r.Bottom-71,width,q.Error.Length==0?Green:Red,12);
  ui.Button(new(r.Right-231,r.Bottom-46,213,30),"REPROCESS",()=>IndustryDo(()=>{string error=world.ReprocessIndustry(reprocessSelection);if(error.Length==0)reprocessSelection.Clear();return error;}),q.Error);ui.Text(industryNotice,x,r.Bottom-34,Gold,12);
 }
}
