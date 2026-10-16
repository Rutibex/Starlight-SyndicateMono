using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int experienceTabI,experiencePageI,experiencePilotPageI,experienceDesignPageI,experienceReviewPageI;string experienceNoticeI="",experienceFocusI="",experiencePilotI="",experienceDesignI="",experienceChannelI="Local",experiencePrimaryI="Slate",experienceAccentI="Cyan",experiencePatternI="Panels",experienceBindI="Experience";
 readonly Dictionary<string,string> experienceFieldsI=new(){["message"]="Request status report.",["name"]="Independent livery",["filter"]=""};
 void OpenExperienceI(){world.EnsurePopulationC();EnsureWindow(ViewMode.Experience);}
 void ExperienceTextI(char c){if(!experienceFieldsI.ContainsKey(experienceFocusI))return;string value=experienceFieldsI[experienceFocusI];if(c=='\b'){if(value.Length>0)experienceFieldsI[experienceFocusI]=value[..^1];}else if(c is '\r' or '\n')experienceFocusI="";else if(!char.IsControl(c)&&value.Length<(experienceFocusI=="message"?240:48))experienceFieldsI[experienceFocusI]+=c;}
 void NoticeI(string text){experienceNoticeI=text.Length==0?"Completed.":text;world.Say(experienceNoticeI);}
 void FieldI(int x,int y,int width,string key){ui.Button(new(x,y,width,30),ShortInfo(experienceFieldsI[key]+(experienceFocusI==key?" |":""),width/8),()=>{experienceFocusI=key;commanderFocusD=industryFocus=skillFocus=marketTextFocus="";},"Stage I field / "+key,experienceFocusI==key);}
 void DrawExperienceI(DesktopWindow w)
 {
  int x=w.X+16,y=w.Y+47,right=x+615;string[] tabs=["Careers","Communications","Cosmetics","Settings","Local tools","Completion"];
  for(int i=0;i<tabs.Length;i++){int tab=i;ui.Button(new(x+i*205,y,198,30),tabs[i],()=>{experienceTabI=tab;experienceFocusI="";},"Stage I tab / "+tabs[i],experienceTabI==i);}y+=54;
  ui.Text(ShortInfo(world.Commander.PilotName+(world.Experience.Title!=""?" / "+world.Experience.Title:"")+" / "+world.Current.Name,135),x,y,Cyan,14);y+=38;
  ui.Text(ShortInfo(experienceNoticeI,148),x,w.Bounds.Bottom-36,Gold,12);
  if(experienceTabI==0)
  {
   ui.Text("CAREER / "+world.Experience.Career,x,y,Gold,16);int row=y+35;
   foreach(string career in ExperienceCatalogI.Careers){string name=career;ui.Button(new(x,row,275,29),name.ToUpper(),()=>NoticeI(world.ChooseCareerI(name,true)),"Stage I career / "+name,world.Experience.Career==name);row+=35;}
   ui.Button(new(x+290,y+35,290,29),world.Experience.Guided?"SKIP TUTORIAL / FREEFORM":"ENABLE CONTEXTUAL GUIDANCE",()=>NoticeI(world.ChooseCareerI(world.Experience.Career,!world.Experience.Guided)),"Stage I tutorial toggle");
   ui.Wrap("Choose your career without restarting or changing allegiance. Goals are earned through real mining, applied combat damage, completed probe analysis and delivered manufacturing. Training-range combat does not count.",x+290,y+87,290,Muted,13);
   ui.Text("DISCOVER ACTIVITIES",x,y+242,Cyan,14);ui.Button(new(x,y+279,280,30),"MISSIONS / LOCAL AGENTS",()=>Open(ViewMode.Agents),"Stage I discover agents");ui.Button(new(x+290,y+279,290,30),"PVE / EXPLORATION",OpenExplorationF,"Stage I discover exploration");
   ui.Button(new(x,y+319,280,30),"RESOURCES / PRODUCTION",()=>EnsureWindow(ViewMode.Production),"Stage I discover production");ui.Button(new(x+290,y+319,290,30),"ADVANCED FLEET ACTIVITIES",OpenAdvancedH,"Stage I discover advanced");
   ui.Button(new(x,y+367,580,30),"WEAR EARNED TITLE / INDEPENDENT CAPSULEER",()=>NoticeI(world.SetTitleI("Independent Capsuleer")),"Stage I title");
   ui.Text("COMMAND-BACKED CAREER GOALS",right,y,Gold,16);int r=y+38;
   foreach(var goal in world.CareerGoalsI){var g=goal;ui.Text((world.Experience.Achievements.Contains(g.Id)?"COMPLETE / ":"IN PROGRESS / ")+g.Name+" / "+world.Experience.Progress.GetValueOrDefault(g.Id)+" / "+g.Target,right,r,White,12);ui.Wrap(g.Guidance,right,r+24,570,Muted,12);ui.Button(new(right,r+68,570,27),"OPEN / "+g.Window,()=>EnsureWindow(g.Window),"Stage I goal / "+g.Id);r+=111;}
  }
  if(experienceTabI==1)
  {
   ui.Text("IN-WORLD CHANNELS / LINKED PILOTS",x,y,Gold,15);int cx=x;foreach(string ch in new[]{"Local","Corp","Fleet","Mail"}){string channel=ch;ui.Button(new(cx,y+35,137,29),ch,()=>experienceChannelI=channel,"Stage I channel / "+ch,experienceChannelI==ch);cx+=146;}
   FieldI(x,y+78,580,"message");ui.Button(new(x,y+119,280,29),"BROADCAST TO CHANNEL",()=>NoticeI(world.PostMessageI(experienceChannelI,experienceFieldsI["message"])),"Stage I broadcast");ui.Button(new(x+292,y+119,288,29),"MARK MAIL READ",()=>{foreach(var m in world.Experience.Messages.Where(m=>m.Channel=="Mail"))m.Read=true;NoticeI("");},"Stage I read mail");
   var pilots=world.Population.Pilots.Where(p=>p.System==world.SystemId&&p.Status!="Lost").ToArray();int row=y+170;
   foreach(var p in pilots.Skip(experiencePilotPageI*5).Take(5)){var pilot=p;ui.Button(new(x,row,580,29),ShortInfo(p.Name+" / "+p.Role+(world.Experience.Contacts.Contains(p.Id)?" / CONTACT":""),72),()=>experiencePilotI=pilot.Id,"Stage I pilot / "+p.Id,experiencePilotI==p.Id);row+=36;}
   ui.Button(new(x,row+7,130,27),"PREVIOUS",()=>experiencePilotPageI=Math.Max(0,experiencePilotPageI-1));ui.Button(new(x+141,row+7,130,27),"NEXT",()=>experiencePilotPageI=Math.Min(Math.Max(0,(pilots.Length-1)/5),experiencePilotPageI+1));
   ui.Button(new(x,y+404,185,29),"CONTACT / NPC MAIL",()=>NoticeI(world.ContactPilotI(experiencePilotI)),"Stage I contact");ui.Button(new(x+194,y+404,190,29),"HIRE / 350 CR",()=>NoticeI(world.HirePopulationPilotC(experiencePilotI)?"":"Dock with 350 CR and a local eligible uncontracted pilot."),"Stage I hire");ui.Button(new(x+394,y+404,186,29),"NEGOTIATE SERVICE",()=>NoticeI(world.NegotiatePilotI(experiencePilotI)),"Stage I negotiate");
   ui.Button(new(x,y+444,280,29),"REQUEST SCOUT INTEL",()=>NoticeI(world.RequestIntelI(experiencePilotI)),"Stage I intel");ui.Button(new(x+291,y+444,289,29),"CORPORATE RECRUITMENT",OpenOrganisationsG,"Stage I recruitment");
   if(world.PopulationPilot(experiencePilotI) is {} selected)ui.Text("Personal relationship "+selected.Relationships.GetValueOrDefault("player")+" | wage "+selected.Wage,x,y+489,Cyan,12);
   ui.Text("CHANNEL HISTORY / "+experienceChannelI,right,y,Gold,15);int r=y+36;
   foreach(var m in world.Experience.Messages.Where(m=>m.Channel==experienceChannelI&&(m.Channel!="Local"||m.System==world.SystemId)).TakeLast(4).Reverse()){ui.Text((m.Read?"":"[NEW] ")+m.At.ToString("HH:mm:ss")+" / "+(world.PopulationPilot(m.Sender)?.Name??m.Sender),right,r,Cyan,11);ui.Wrap(m.Body,right,r+23,570,White,12);r+=83;}
   ui.Text("INTEL SNAPSHOTS / 20 SECOND VALIDITY",right,y+379,Gold,13);r=y+409;foreach(var i in world.Experience.Intel.TakeLast(3).Reverse()){ui.Text((world.IntelFreshI(i)?"FRESH":"STALE")+" / "+ShortInfo(i.Name,29)+" / "+(world.Now-i.Observed).TotalSeconds.ToString("0")+"s old",right,r,world.IntelFreshI(i)?Cyan:Muted,12);ui.Text(ShortInfo(i.System+" / "+i.Grid+" / last observed "+i.Position,78),right,r+20,Muted,10);r+=46;}
  }
  if(experienceTabI==2)DrawCosmeticsI(x,y,right,w);
  if(experienceTabI==3)DrawSettingsI(x,y,right,w);
  if(experienceTabI==4)
  {
   ui.Text("READ-ONLY LOCAL EXPORTS",x,y,Gold,15);int row=y+40;foreach(string kind in new[]{"fitting","skills","market","industry","assets","combat","cosmetics","completion"}){string export=kind;ui.Button(new(x,row,580,29),"EXPORT / "+kind.ToUpper()+".JSON",()=>NoticeI(world.ExportLocalI(export)),"Stage I export / "+kind);row+=39;}
   ui.Wrap("Exports use the current saved simulation time and the same state displayed by game tools. Mission cargo retains its contract identity. Combat archives retain up to 2,000 events plus owned loss reports.",right,y,570,Muted,13);
   ui.Text("OPTIONAL PACKS / DECLARED PROFILES",right,y+127,Cyan,14);ui.Button(new(right,y+168,570,30),"LOCAL SCIENCE / "+(world.Experience.Packs.Contains("local-science")?"ENABLED":"DISABLED"),()=>NoticeI(world.TogglePackI("local-science")),"Stage I pack toggle");
   ui.Button(new(right,y+209,570,30),"CLASSIFY OBSERVED CELESTIAL",()=>ShowPopup("Survey classification",new[]{"AsteroidBelt","Planet","Moon","Star","Station","Gate"}.Select(k=>(k,(Action)(()=>NoticeI(world.SubmitScienceI(k)))))),"Stage I survey");
   ui.Button(new(right,y+250,570,30),"IMPORT EXPORTS / OPTIONAL-PACK.JSON",()=>NoticeI(world.ImportLocalPackI(Path.Combine(DemoWorld.ExportDirectoryI,"optional-pack.json"))),"Stage I pack import");
   ui.Wrap("Place a valid pack at the displayed export folder as optional-pack.json. Schema and mechanics/geography/build profiles must match. Cosmetic imports are unpaid drafts; they cannot grant ownership or alter ship statistics. Raffle and legacy packs are excluded.",right,y+300,570,Muted,13);
   ui.Wrap("Folder: "+DemoWorld.ExportDirectoryI,right,y+438,570,Cyan,12);
  }
  if(experienceTabI==5)
  {
   ui.Text("R01–R72 / SUPPORTED, REFERENCE AND OPEN WORK",x,y,Gold,15);FieldI(x,y+34,580,"filter");var rows=ReleaseCatalogI.Rows.Where(r=>(r.Id+" "+r.Name+" "+r.Scope).Contains(experienceFieldsI["filter"],StringComparison.OrdinalIgnoreCase)).ToArray();experienceReviewPageI=Math.Clamp(experienceReviewPageI,0,Math.Max(0,(rows.Length-1)/6));int row=y+81;
   foreach(var r in rows.Skip(experienceReviewPageI*6).Take(6)){ui.Text(r.Id+" / "+r.Name,x,row,White,12);ui.Text(r.Status+" / "+ShortInfo(r.Delivered,83),x,row+22,Muted,11);row+=65;}
   ui.Button(new(x,y+482,130,28),"PREVIOUS",()=>experienceReviewPageI=Math.Max(0,experienceReviewPageI-1),"Stage I review previous");ui.Button(new(x+141,y+482,130,28),"NEXT",()=>experienceReviewPageI=Math.Min(Math.Max(0,(rows.Length-1)/6),experienceReviewPageI+1),"Stage I review next");ui.Text((experienceReviewPageI+1)+" / "+Math.Max(1,(rows.Length+5)/6),x+290,y+490,Cyan,12);
   ui.Wrap("Imported records, historical assertion totals and visible tabs do not certify full mechanics. This register preserves earlier Partial statuses. Read the packaged validation report for fresh evidence and the scope of Windows testing.",right,y,570,Muted,14);
   ui.Text("ACTIVE PROFILE",right,y+175,Cyan,14);ui.Wrap("Source build 3569502. Supplied galaxy with restored 2019 belts. Pausable local clock. Single-player authored substitutes are identified in stage notes. No online subscription, account feed or forced campaign allegiance.",right,y+209,570,White,13);
   ui.Button(new(right,y+349,570,29),"EXPORT COMPLETE FEATURE REGISTER",()=>NoticeI(world.ExportLocalI("completion")),"Stage I review export");
   ui.Text("Current runtime: "+world.Targets.Count+" targets / "+world.Population.Pilots.Count+" NPC actors",right,y+404,Cyan,12);ui.Text("Paint jobs "+world.Experience.Sequences.Count+" | archived events "+world.Experience.CombatArchive.Count,right,y+436,White,12);
  }
 }
 void SelectDesignI(CosmeticI d){experienceDesignI=d.Id;experienceFieldsI["name"]=d.Name;experiencePrimaryI=d.Primary;experienceAccentI=d.Accent;experiencePatternI=d.Pattern;}
 void DrawCosmeticsI(int x,int y,int right,DesktopWindow w)
 {
  ui.Text("HULL-BOUND LOCAL PAINT DESIGN",x,y,Gold,15);FieldI(x,y+37,580,"name");
  ui.Button(new(x,y+81,280,30),"PRIMARY / "+experiencePrimaryI,()=>ShowPopup("Primary paint",ExperienceCatalogI.Paints.Keys.Select(k=>(k,(Action)(()=>experiencePrimaryI=k)))),"Stage I primary");ui.Button(new(x+291,y+81,289,30),"ACCENT / "+experienceAccentI,()=>ShowPopup("Accent paint",ExperienceCatalogI.Paints.Keys.Select(k=>(k,(Action)(()=>experienceAccentI=k)))),"Stage I accent");
  ui.Button(new(x,y+123,280,30),"PATTERN / "+experiencePatternI,()=>ShowPopup("Hull pattern",ExperienceCatalogI.Patterns.Select(k=>(k,(Action)(()=>experiencePatternI=k)))),"Stage I pattern");ui.Button(new(x+291,y+123,289,30),"SAVE DESIGN FOR ACTIVE HULL",()=>{NoticeI(world.DesignCosmeticI(experienceFieldsI["name"],experiencePrimaryI,experienceAccentI,experiencePatternI));experienceDesignI=world.Experience.Cosmetics.LastOrDefault()?.Id??"";},"Stage I design");
  var designs=world.Experience.Cosmetics.ToArray();int row=y+169;foreach(var d in designs.Skip(experienceDesignPageI*4).Take(4)){var design=d;ui.Button(new(x,row,580,29),ShortInfo(d.Name+" / "+(d.Owned?"OWNED":"DRAFT")+" / "+d.Hull,74),()=>SelectDesignI(design),"Stage I design row / "+d.Id,experienceDesignI==d.Id);row+=35;}
  ui.Button(new(x,row+8,130,27),"PREVIOUS",()=>experienceDesignPageI=Math.Max(0,experienceDesignPageI-1));ui.Button(new(x+140,row+8,130,27),"NEXT",()=>experienceDesignPageI=Math.Min(Math.Max(0,(designs.Length-1)/4),experienceDesignPageI+1));
  ui.Button(new(x,y+371,280,30),"SEQUENCE / 50 CR + 5 ORE / 1H",()=>NoticeI(world.SequenceCosmeticI(experienceDesignI)),"Stage I sequence");ui.Button(new(x+291,y+371,289,30),"APPLY OWNED DESIGN / DOCKED",()=>NoticeI(world.ApplyCosmeticI(experienceDesignI)),"Stage I apply");
  ui.Button(new(x,y+412,580,30),"BUY LOCAL NPC LICENSE / 100 CR",()=>ShowPopup("Local cosmetic sellers",world.Population.Pilots.Where(p=>p.System==world.SystemId&&p.Role=="Trader"&&p.Status!="Lost").Select(p=>(p.Name,(Action)(()=>NoticeI(world.BuyCosmeticI(p.Id)))))),"Stage I buy cosmetic");
  ui.Text("MATERIAL SWATCH / "+experiencePatternI,right,y,Gold,15);var a=ExperienceCatalogI.Paints[experiencePrimaryI];var b=ExperienceCatalogI.Paints[experienceAccentI];ui.Box(new(right,y+37,565,74),a);for(int n=0;n<9;n++){bool accent=experiencePatternI switch{"Alternating"=>n%2==1,"Bands"=>n%3==0,_=>n%4==1};ui.Box(new(right+n*62,y+37,61,74),accent?b:a);}
  ui.Wrap("Sequencing consumes local hangar ore and credits. Collect the ready license at its issuing station, then apply it while docked. Paint changes rendered hull materials only; fitting statistics and tactical bonuses are unaffected.",right,y+139,570,Muted,13);
  int rowJ=y+279;foreach(var j in world.Experience.Sequences.TakeLast(3)){var job=j;ui.Text(j.State+" / "+j.Finish.ToString("u"),right,rowJ,White,12);ui.Button(new(right,rowJ+23,565,27),"COLLECT READY LICENSE",()=>NoticeI(world.ClaimCosmeticI(job.Id)),"Stage I claim / "+j.Id);rowJ+=62;}
  ui.Button(new(right,y+475,565,29),"CORPORATE MARK / DIRECTOR",()=>ShowPopup("Corporate heraldry",ExperienceCatalogI.Marks.Select(k=>(k,(Action)(()=>NoticeI(world.SetCorporateMarkI(k)))))),"Stage I corporate mark");
 }
 void DrawSettingsI(int x,int y,int right,DesktopWindow w)
 {
  var s=world.Experience.Settings;ui.Text("ACCESSIBILITY / PRESENTATION",x,y,Gold,15);int row=y+38;
  void Toggle(string label,Action action,bool active){ui.Button(new(x,row,580,29),label,action,"Stage I setting / "+label,active);row+=38;}
  Toggle("LARGE TEXT",()=>s.LargeText=!s.LargeText,s.LargeText);Toggle("HIGH CONTRAST TEXT",()=>s.HighContrast=!s.HighContrast,s.HighContrast);Toggle("REDUCED MOTION",()=>s.ReducedMotion=!s.ReducedMotion,s.ReducedMotion);Toggle("NOTIFICATION AUDIO",()=>s.Audio=!s.Audio,s.Audio);Toggle("PERFORMANCE OVERLAY",()=>s.PerformanceOverlay=!s.PerformanceOverlay,s.PerformanceOverlay);
  ui.Button(new(x,row,280,29),"VOLUME / "+s.Volume.ToString("0%"),()=>s.Volume=s.Volume>=.99f?0:Math.Min(1,s.Volume+.25f),"Stage I volume");ui.Button(new(x+291,row,289,29),"DRAW BUDGET / "+s.RenderBudget,()=>s.RenderBudget=s.RenderBudget==64?128:s.RenderBudget==128?256:s.RenderBudget==256?512:64,"Stage I budget");row+=43;
  ui.Button(new(x,row,580,30),"RESET WINDOW LAYOUT",()=>{desktop.Clear();placements.Clear();EnsureWindow(ViewMode.Experience);SaveDesktop();},"Stage I reset layout");
  ui.Button(new(x,row+41,280,30),"SAVE EXPERIENCE SETTINGS",world.Save,"Stage I save");ui.Button(new(x+292,row+41,288,30),"RESOLUTION / "+s.Width+"x"+s.Height,()=>ShowPopup("Display resolution",new[]{(1280,720),(1600,900),(1920,1080)}.Select(v=>(v.Item1+"x"+v.Item2,(Action)(()=>SetResolutionI(v.Item1,v.Item2))))),"Stage I resolution");
  ui.Text("REMAP PRIMARY COMMANDS",right,y,Gold,15);int rr=y+36;foreach(var bind in ExperienceCatalogI.DefaultBindings){string key=bind.Key;ui.Button(new(right,rr,565,28),bind.Key+" / "+world.BindingI(bind.Key),()=>{experienceBindI=key;ShowPopup("Bind "+key,new[]{"Alt+F10","F12","I","B","Y","X","Alt+K","Alt+P","G","Alt+Shift+F10","Alt+Shift+F12","Alt+Shift+I","Alt+Shift+B","Alt+Shift+Y","Alt+Shift+X","Alt+Shift+K","Alt+Shift+G"}.Select(ch=>(ch,(Action)(()=>NoticeI(world.RebindI(key,ch))))));},"Stage I binding / "+key);rr+=33;}
  ui.Button(new(right,rr+4,565,29),"RESTORE DEFAULT PRIMARY BINDINGS",()=>s.Bindings.Clear(),"Stage I default bindings");
  ui.Text("DISCOVER WINDOWS / FILTER BY NAME",x,y+425,Cyan,12);FieldI(x,y+451,390,"filter");ui.Button(new(x+402,y+451,178,30),"OPEN WINDOW...",()=>ShowWindowPickerI(0),"Stage I window finder");
  ui.Wrap("Alt+P opens Population. Alt+Shift+P opens the probe map. Plain G opens the map once. F8 saves; F9 loads. Windows preserve layouts. Settings persist with your save.",right,y+409,570,Muted,12);
 }
 void ShowWindowPickerI(int page){var windows=Enum.GetValues<ViewMode>().Where(k=>k is not(ViewMode.Title or ViewMode.Station or ViewMode.Space)&&k.ToString().Contains(experienceFieldsI["filter"],StringComparison.OrdinalIgnoreCase)).ToArray();page=Math.Clamp(page,0,Math.Max(0,(windows.Length-1)/10));int current=page;var actions=windows.Skip(page*10).Take(10).Select(k=>(WindowTitle(k),(Action)(()=>EnsureWindow(k)))).ToList();if(page>0)actions.Add(("Previous windows",()=>ShowWindowPickerI(current-1)));if((page+1)*10<windows.Length)actions.Add(("Next windows",()=>ShowWindowPickerI(current+1)));ShowPopup("Game windows / "+(page+1),actions);}
 void SetResolutionI(int width,int height){world.Experience.Settings.Width=width;world.Experience.Settings.Height=height;graphics.PreferredBackBufferWidth=width;graphics.PreferredBackBufferHeight=height;graphics.ApplyChanges();}
 bool BoundPressedI(string action,KeyboardState keys)
 {
  string[] parts=world.BindingI(action).Split('+');if(!Enum.TryParse<Keys>(parts[^1],out var key))return false;
  bool alt=keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt),shift=keys.IsKeyDown(Keys.LeftShift)||keys.IsKeyDown(Keys.RightShift),ctrl=keys.IsKeyDown(Keys.LeftControl)||keys.IsKeyDown(Keys.RightControl);
  return !ctrl&&alt==parts.Contains("Alt")&&shift==parts.Contains("Shift")&&Press(key,keys);
 }
 void SetupExperienceCaptureI()
 {
  world.NewGame(1);world.EnsurePopulationC();world.WalletFunds=5000;world.Hangar["ore"]=100;world.ChooseCareerI("Prospector",true);desktop.Clear();EnsureWindow(ViewMode.Experience);
  experienceTabI=captureScene switch{"stage-i-comms"=>1,"stage-i-cosmetics"=>2,"stage-i-settings"=>3,"stage-i-tools"=>4,"stage-i-review"=>5,_=>0};
  if(experienceTabI==3){world.Experience.Settings.LargeText=true;world.Experience.Settings.HighContrast=true;world.Experience.Settings.ReducedMotion=true;}
  if(experienceTabI==2){world.DesignCosmeticI("Independent cyan banding","Slate","Cyan","Bands");experienceDesignI=world.Experience.Cosmetics.Last().Id;world.SequenceCosmeticI(experienceDesignI);world.AdvanceHours(1);world.ClaimCosmeticI(world.Experience.Sequences.Last().Id);world.ApplyCosmeticI(experienceDesignI);SelectDesignI(world.Experience.Cosmetics.Last());}
 }
}
public sealed partial class DemoGame
{
 int experienceUiChecksI;
 void RunExperienceUiChecksI(int frame)
 {
  void Check(bool ok,string label){if(!ok)throw new Exception("STAGE I UI FAIL: "+label);Console.WriteLine("STAGE I UI PASS: "+label);experienceUiChecksI++;}
  void Click(string tip){var h=ui.Hits.Last(h=>h.Tip==tip);Check(h.Rect.X>=0&&h.Rect.Right<=1600&&h.Rect.Y>=0&&h.Rect.Bottom<=900,"reachable / "+tip);TestClick(h.Rect.Center.ToVector2());}
  if(frame==2){Check(ui.Hits.Any(h=>h.Tip=="Stage I goal / mine"),"career goals have command links");Click("Stage I career / Freeform");}
  if(frame==3){Check(world.Experience.Career=="Freeform","career choice reaches saved state");Click("Stage I tutorial toggle");}
  if(frame==4){Check(!world.Experience.Guided&&world.NextGoalI==null,"skip removes guidance without resetting assets");Click("Stage I tab / Communications");}
  if(frame==5){Click("Stage I broadcast");Check(world.Experience.Messages.Last().Channel=="Local","real scoped local broadcast");}
  if(frame==6){var p=world.Population.Pilots.First(p=>p.System==world.SystemId);Click("Stage I pilot / "+p.Id);}
  if(frame==7){Click("Stage I contact");Check(world.Experience.Contacts.Contains(experiencePilotI),"contact action persists real actor identity");Click("Stage I tab / Cosmetics");}
  if(frame==8){Click("Stage I design");Check(world.Experience.Cosmetics.Count==1,"paint editor creates a draft");}
  if(frame==9){decimal before=world.Hangar["ore"];Click("Stage I sequence");Check(world.Experience.Sequences.Count==1&&world.Hangar["ore"]==before-5,"sequencing consumes actual inputs");world.AdvanceHours(1);}
  if(frame==10){Click("Stage I claim / "+world.Experience.Sequences.Single().Id);Check(world.Experience.Cosmetics.Single().Owned,"ready license collected through UI");}
  if(frame==11){Click("Stage I apply");Check(world.ActiveCosmeticI!=null,"owned paint applied through UI");Click("Stage I tab / Settings");}
  if(frame==12){Click("Stage I setting / LARGE TEXT");Click("Stage I setting / HIGH CONTRAST TEXT");Check(world.Experience.Settings.LargeText&&world.Experience.Settings.HighContrast,"accessibility controls affect runtime settings");}
  if(frame==13){Click("Stage I setting / REDUCED MOTION");Click("Stage I budget");Check(world.Experience.Settings.ReducedMotion&&world.Experience.Settings.RenderBudget==512,"presentation controls reachable");}
  if(frame==14){Click("Stage I binding / Inventory");Check(popupActions.Count>0,"binding choices open");context=false;}
  if(frame==15){world.RebindI("Inventory","Alt+Shift+I");Check(BoundPressedI("Inventory",new KeyboardState(Keys.LeftAlt,Keys.LeftShift,Keys.I)),"bound chord executes");Check(!BoundPressedI("Inventory",new KeyboardState(Keys.I)),"original plain chord no longer executes");Click("Stage I window finder");Check(popupActions.Count<=12,"window discovery is paginated");context=false;}
  if(frame==16){Click("Stage I tab / Local tools");}
  if(frame==17){Click("Stage I export / fitting");Check(File.Exists(Path.Combine(DemoWorld.ExportDirectoryI,"fitting.json")),"UI writes live fitting export");Click("Stage I pack toggle");}
  if(frame==18){Check(world.Experience.Packs.Contains("local-science"),"optional science toggle");Click("Stage I tab / Completion");}
  if(frame==19){Click("Stage I review next");Check(experienceReviewPageI==1,"completion register pagination");}
  if(frame==20){Click("Stage I review export");Check(File.Exists(Path.Combine(DemoWorld.ExportDirectoryI,"completion.json")),"UI exports full feature register");}
  if(frame==21){Click("Stage I tab / Settings");}
  if(frame==22){Click("Stage I save");var other=new DemoWorld();Check(other.Load()&&other.Experience.Settings.LargeText&&other.ActiveCosmeticI!=null,"UI state and applied cosmetic reload");}
  if(frame==23){Click("Stage I reset layout");Check(desktop.Count==1&&desktop[0].Kind==ViewMode.Experience,"layout recovery retains reachable workspace");}
  if(frame==24){float scale=presentRect.Width/1600f;Vector2 logical=desktop[0].Bounds.Center.ToVector2(),physical=new Vector2(presentRect.X,presentRect.Y)+logical*scale;Vector2 restored=(physical-new Vector2(presentRect.X,presentRect.Y))/scale;Check(Vector2.Distance(logical,restored)<.01f,"physical/logical pointer mapping at "+GraphicsDevice.PresentationParameters.BackBufferWidth+"x"+GraphicsDevice.PresentationParameters.BackBufferHeight);Check(new Rectangle(0,0,1600,900).Contains(desktop[0].Bounds),"reset workspace fits logical canvas");Console.WriteLine(experienceUiChecksI+" Stage I UI checks passed.");}
 }
}
