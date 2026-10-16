using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;

public sealed partial class DemoGame
{
 public static void RunShipInformationAudit()
 {
  int values=0;foreach(var h in FleetCatalog.All){
   if(h.BaseHullId.Length>0&&!FleetCatalog.ById.ContainsKey(h.BaseHullId))throw new InvalidDataException("Unresolved lineage "+h.Name);
   foreach(var (key,value) in h.Attributes){if(!double.IsFinite(value)||string.IsNullOrWhiteSpace(InfoValue(key,value)))throw new InvalidDataException("Invalid formatted value "+h.Name);values++;}
   if(h.RawFields.Count==0||string.IsNullOrWhiteSpace(h.Description))throw new InvalidDataException("Missing registry content "+h.Name);
  }
  Console.WriteLine($"Ship information audit passed: {FleetCatalog.All.Length} hulls; {values} formatted attributes; {FleetCatalog.All.Sum(h=>h.RawFields.Count)} preserved source fields.");
 }
 static readonly string[] InfoTabs=["Overview","Attributes","Fitting","Skills","Variations","Industry","SKINs","Traits"];
 string shipInfoId="";int shipInfoTab,shipInfoPage,shipInfoHistoryIndex=-1;bool shipInfoFitted,shipInfoRaw,shipInfoCompare,shipInfoLore;
 static readonly Dictionary<string,string> InfoFactionLore=System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","Factions.json")))!.ToDictionary(f=>f.GetProperty("Name").GetString()!,f=>f.GetProperty("Lore").GetString()!);
 readonly List<string> shipInfoHistory=[];readonly HashSet<string> shipInfoCollapsed=[];
 bool shipPreviewDragging;Vector2 shipPreviewLast;
 RenderTarget2D? shipPreview;float shipPreviewYaw=.7f,shipPreviewPitch=-.25f,shipPreviewZoom=1;
 void ShowCurrentShipInfo()=>ShowInformation(CurrentShipInfoKey());
 void ShowShipInformation(string id,bool history=true){if(id.Length==0)ShowCurrentShipInfo();else ShowItemInfo(id);}
 void InfoHistory(int delta){int index=shipInfoHistoryIndex+delta;if(index<0||index>=shipInfoHistory.Count)return;shipInfoHistoryIndex=index;ShowShipInformation(shipInfoHistory[index],false);}
 void PrepareShipPreview()
 {
  if(!desktop.Any(w=>w.Kind==ViewMode.ShipInfo)||!renderer.HasPreview(shipInfoId))return;
  shipPreview??=new RenderTarget2D(GraphicsDevice,400,420,false,SurfaceFormat.Color,DepthFormat.Depth24);
  GraphicsDevice.SetRenderTarget(shipPreview);GraphicsDevice.Clear(new Color(7,14,22));renderer.DrawPreview(shipInfoId,shipPreviewYaw,shipPreviewPitch,shipPreviewZoom,shipInfoId==world.Player.CombatHull?world.Player:null);
 }
 public static string InfoValue(string key,double value)
 {
  var a=ShipAttributeCatalog.ByKey.GetValueOrDefault(key);string unit=(a?.Unit??"").Replace("³","3").Replace("²","2");
  if(key=="rigSize")return value switch{1=>"Small",2=>"Medium",3=>"Large",4=>"Capital",_=>value.ToString("0")};
  if(key.Contains("Resonance"))return $"{(1-value)*100:0.#}% resistance";
  if(unit=="ms")return $"{value/1000:0.###} s";
  if(unit=="fraction %")return $"{value*100:0.#}%";
  return value.ToString("0.###")+(unit.Length>0?" "+unit:"");
 }
 Dictionary<string,double> InfoAttributes(FleetHull? h)
 {
  if(h==null||shipInfoFitted&&h.Id==world.Player.CombatHull)return world.Player.Attributes.Where(p=>p.Value.HasValue).ToDictionary(p=>p.Key,p=>world.Player.Stat(p.Key));
  return h.Attributes;
 }
 static string CompareValue(FleetHull h,string key)=>h.Attributes.TryGetValue(key,out var value)?value.ToString("0"):"N/A";
 static string ShortInfo(string text,int length=52)=>text.Length>length?text[..(length-3)]+"...":text;
 void InfoParagraph(string text,int x,int y,int width,int maxLines=12,Color? color=null)
 {
  int chars=Math.Max(10,width/7);var words=text.Replace("\r"," ").Replace("\n"," ").Split(' ',StringSplitOptions.RemoveEmptyEntries);string line="";int count=0;
  foreach(var word in words){if(line.Length+word.Length+1>chars&&line.Length>0){ui.Text(line,x,y,color??White,12);y+=21;if(++count>=maxLines){ui.Text("Continued in source document",x,y,Muted,10);return;}line="";}line+=(line.Length==0?"":" ")+word;}if(line.Length>0)ui.Text(line,x,y,color??White,12);
 }
 void InfoRows(List<(string key,string label,string value)> rows,int y=239,int pageSize=15)
 {
  int pages=Math.Max(1,(rows.Count+pageSize-1)/pageSize);shipInfoPage=Math.Clamp(shipInfoPage,0,pages-1);
  foreach(var row in rows.Skip(shipInfoPage*pageSize).Take(pageSize))
  {
   if(row.key.StartsWith("group:")){var group=row.key[6..];ui.Button(new(839,y,453,26),(shipInfoCollapsed.Contains(group)?"+ ":"- ")+row.label,()=>{if(!shipInfoCollapsed.Add(group))shipInfoCollapsed.Remove(group);shipInfoPage=0;},"Expand / collapse "+group);}
   else{ui.Box(new(839,y,453,26),new Color(17,29,38));ui.Text(ShortInfo(row.label,35),849,y+8,White,11);ui.Text(ShortInfo(row.value,27),1100,y+8,Cyan,11);if(row.key.Contains("Resonance")&&double.TryParse(row.value.Split('%')[0],out var resistance)){var tint=row.key.Contains("Em")?Cyan:row.key.Contains("Thermal")?Red:row.key.Contains("Kinetic")?Muted:Gold;ui.Box(new(1100,y+23,(int)(186*Math.Clamp(resistance/100,0,1)),2),tint);}
   ui.Hits.Add(new(new(839,y,453,26),()=>{},row.label+": "+row.value));}y+=29;
  }
  ui.Button(new(839,715,105,27),"Previous page",()=>shipInfoPage=Math.Max(0,shipInfoPage-1));ui.Text($"{shipInfoPage+1} / {pages}",1029,724,Muted,12);ui.Button(new(1187,715,105,27),"Next page",()=>shipInfoPage=Math.Min(pages-1,shipInfoPage+1));
 }
 void ShipInfo()
 {
  var h=FleetCatalog.ById.GetValueOrDefault(shipInfoId);var data=InfoAttributes(h);string name=h?.Name??world.Player.HullName;
  DrawWindow(new(260,100,1060,700),"Ship information / "+name);
  ui.Text(ShortInfo(name,46),423,148,White,23);ui.Text(ShortInfo((h?.Faction??"Independent")+" / "+(h?.Class??"Legacy hull"),55),423,184,Cyan,11);
  ui.Button(new(276,148,55,28),"Back",()=>InfoHistory(-1));ui.Button(new(337,148,65,28),"Forward",()=>InfoHistory(1));
  for(int i=0;i<InfoTabs.Length;i++){int tab=i;ui.Button(new(276,203+i*53,126,42),InfoTabs[i],()=>{shipInfoTab=tab;shipInfoPage=0;},active:shipInfoTab==i);}
  if(renderer.HasPreview(shipInfoId)&&shipPreview!=null)ui.Image(shipPreview,new(423,217,394,414));
  else{ui.Box(new(423,217,394,414),new Color(7,14,22));ui.Hex(new(620,398),73,Cyan,2);ui.Center("MODEL UNAVAILABLE",620,507,Muted,13);ui.Center("Hull specifications are available",620,534,Muted,11);}
  ui.Hits.Add(new(new(423,217,394,414),()=>{},"Drag to rotate / wheel to zoom"));
  ui.Button(new(423,643,56,28),"<",()=>shipPreviewYaw-=.25f,"Rotate left");ui.Button(new(486,643,56,28),">",()=>shipPreviewYaw+=.25f,"Rotate right");
  ui.Button(new(549,643,56,28),"Up",()=>shipPreviewPitch=Math.Clamp(shipPreviewPitch-.15f,-1.3f,1.3f));ui.Button(new(612,643,56,28),"Down",()=>shipPreviewPitch=Math.Clamp(shipPreviewPitch+.15f,-1.3f,1.3f));
  ui.Button(new(675,643,42,28),"+",()=>shipPreviewZoom=Math.Max(.6f,shipPreviewZoom-.1f),"Zoom preview in");ui.Button(new(724,643,42,28),"-",()=>shipPreviewZoom=Math.Min(2,shipPreviewZoom+.1f),"Zoom preview out");ui.Button(new(773,643,44,28),"R",()=>{shipPreviewYaw=.7f;shipPreviewPitch=-.25f;shipPreviewZoom=1;},"Reset preview");
  ui.Text(h?.Playable==true?"Playable hull / original procedural model":h!=null&&renderer.HasPreview(h.Id)?"Edition preview / shared base chassis":h==null?"Active ship / legacy mesh":"Registry hull / model pending",423,690,Muted,10);
  ui.Button(new(423,715,188,28),"CURRENT SHIP",ShowCurrentShipInfo);ui.Button(new(623,715,194,28),"FLEET REGISTRY",()=>{fleetBrowser=true;fleetSelected=h?.Id??"";EnsureWindow(ViewMode.Fitting);});
  ui.Text(InfoTabs[shipInfoTab].ToUpper(),839,150,Gold,20);
  ui.Text(h==null||shipInfoFitted?"Active fit / current modifiers":"Hull specifications / unmodified",839,185,Muted,11);
  if(h!=null&&h.Id==world.Player.CombatHull)ui.Button(new(839,204,217,26),shipInfoFitted?"Use hull baseline":"Use current fit",()=>{shipInfoFitted=!shipInfoFitted;shipInfoPage=0;});
  string Value(string key)=>data.TryGetValue(key,out var value)?InfoValue(key,value):"Unavailable";
  switch(shipInfoTab)
  {
   case 0:
    ui.Button(new(1070,204,222,26),shipInfoLore?"Hull description":"Faction lore",()=>{shipInfoLore=!shipInfoLore;shipInfoPage=0;});
    string overview=shipInfoLore?InfoFactionLore.GetValueOrDefault(h?.Faction??"","No faction history is defined for this vessel."):(h?.Quote??"An independent vessel in the Syndicate frontier.")+"\n\n"+(h?.Description??"Inspect attributes and fitting for your active vessel.");
    var lines=new List<string>();foreach(var paragraph in overview.Replace("\r","").Split('\n')){string line="";foreach(var word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries)){if(line.Length+word.Length+1>61&&line.Length>0){lines.Add(line);line="";}line+=(line.Length==0?"":" ")+word;}lines.Add(line);}
    int overviewPages=Math.Max(1,(lines.Count+17)/18);shipInfoPage=Math.Clamp(shipInfoPage,0,overviewPages-1);int overviewY=247;foreach(var line in lines.Skip(shipInfoPage*18).Take(18)){ui.Text(line,839,overviewY,White,12);overviewY+=21;}
    ui.Button(new(839,633,453,28),"HULL TRAITS / PROFICIENCY / ACQUISITION",()=>{shipInfoTab=7;shipInfoPage=0;});
    InfoParagraph(h?.TraitsStatus??"Legacy ship systems use the current simulation rules.",839,674,445,1,Muted);
    ui.Button(new(839,715,105,27),"Previous page",()=>shipInfoPage=Math.Max(0,shipInfoPage-1));ui.Text($"{shipInfoPage+1} / {overviewPages}",1029,724,Muted,12);ui.Button(new(1187,715,105,27),"Next page",()=>shipInfoPage=Math.Min(overviewPages-1,shipInfoPage+1));break;
   case 7:
    if(h!=null&&ShipBatch21.ById.TryGetValue(h.Id,out var profile)){
     ui.Text(profile.Role+" / "+profile.Price+" CR / basic fit included",839,239,Cyan,12);
     ui.Text("Available at "+profile.HomeSystem+" station",839,266,Gold,12);
     var displayTraits=world.Player.CombatHull==h.Id?world.Player.ActiveHullTraits:profile.Traits;int ty=299;
     if(profile.Effects.Any(e=>e.Mode.Length>0)){
      foreach(var (mode,i) in new[]{"Primary","Secondary","Tertiary"}.Select((m,i)=>(m,i))){string targetMode=mode;ui.Button(new(839+i*151,ty,145,26),world.Player.CombatHull==h.Id?world.Player.TacticalModeLabel(mode):mode,()=>{if(world.Player.CombatHull==h.Id)world.SetHullTacticalMode(targetMode);},"Switch tactical mode / "+mode,world.Player.CombatHull==h.Id&&world.Player.HullTacticalMode==mode);}ty+=32;
      ui.Text("Mode switch delay: "+world.Player.HullTacticalCooldown.ToString("0.0")+"s"+(profile.Batch==26?" / "+world.Player.TacticalModeDelay+"s cooldown":""),839,ty,Muted,11);ty+=26;
     }
foreach(int skill in displayTraits.Where(t=>t.Skill!=0).Select(t=>t.Skill).Distinct()){
      int skillId=skill;int level=world.Player.HullProficiency.GetValueOrDefault(skill,1);
      ui.Button(new(839,ty,453,27),(profile.Batch==29?Ship.EliteSkillLabel(skill):"Hull proficiency")+" "+level+"/5 / TRAIN "+(250*(level+1)*(level+1))+" CR",()=>{if(world.Player.CombatHull==h.Id)world.TrainHullProficiency(skillId);else world.Say("Board this hull before training.");},"Dock and board this hull to train; bonuses apply per level.");ty+=29;
     }
     int traitPageSize=profile.Batch is 28 or 29||profile.Effects.Any(e=>e.Mode.Length>0)?3:4;int traitPages=Math.Max(1,(displayTraits.Length+traitPageSize-1)/traitPageSize);shipInfoPage=Math.Clamp(shipInfoPage,0,traitPages-1);
     foreach(var trait in displayTraits.Skip(shipInfoPage*traitPageSize).Take(traitPageSize)){
      string prefix=profile.Batch==24&&ShipBatch24.ContextOnly(trait.Text)?"REFERENCE / ":trait.Bonus==0?"":trait.Skill==0?"ROLE / ":"PER LEVEL / ";
      if(profile.Batch==29&&trait.Skill!=0)prefix=Ship.EliteSkillLabel(trait.Skill)+" / PER LEVEL / ";
      InfoParagraph(prefix+(trait.Bonus==0?"":trait.Bonus.ToString("0.##"))+(trait.Unit==105?"% ":" ")+trait.Text,839,ty,445,3,White);ty+=79;
     }
     if(displayTraits.Length==0)InfoParagraph("No readable hull bonuses are defined in the source supplement. This vessel uses its recorded hull attributes.",839,ty,445,4,Muted);
     ui.Button(new(839,715,105,27),"Previous page",()=>shipInfoPage=Math.Max(0,shipInfoPage-1));ui.Text($"{shipInfoPage+1}/{traitPages}",1029,724,Muted,12);ui.Button(new(1187,715,105,27),"Next page",()=>shipInfoPage=Math.Min(traitPages-1,shipInfoPage+1));
    }else InfoParagraph("This hull awaits its mechanics batch.",839,247,445);
    break;
   case 1:
    ui.Button(new(1070,204,222,26),shipInfoRaw?"Formatted attributes":"Source fields",()=>{shipInfoRaw=!shipInfoRaw;shipInfoPage=0;});
    var rows=new List<(string,string,string)>();
    if(shipInfoRaw&&h!=null)rows.AddRange(h.RawFields.OrderBy(p=>p.Key).Select(p=>(p.Key,p.Key,p.Value)));
    else foreach(var group in data.GroupBy(p=>ShipAttributeCatalog.ByKey.GetValueOrDefault(p.Key)?.Group??"Other hull properties").OrderBy(g=>g.Key))
    {rows.Add(("group:"+group.Key,group.Key.ToUpper(),""));if(!shipInfoCollapsed.Contains(group.Key))rows.AddRange(group.OrderBy(p=>p.Key).Select(p=>(p.Key,ShipAttributeCatalog.ByKey.GetValueOrDefault(p.Key)?.Label??p.Key,InfoValue(p.Key,p.Value))));}
    InfoRows(rows);break;
   case 2:
    string[] keys=["cpuOutput","powerOutput","upgradeCapacity","hiSlots","medSlots","lowSlots","upgradeSlotsLeft","rigSize","turretSlotsLeft","launcherSlotsLeft","droneCapacity","droneBandwidth","capacity"];
    InfoRows(keys.Select(k=>(k,ShipAttributeCatalog.ByKey.GetValueOrDefault(k)?.Label??k,Value(k))).ToList(),239,14);
    if(h==null||h.Id==world.Player.CombatHull)ui.Button(new(1070,204,222,26),"OPEN FITTING",()=>{fleetBrowser=false;EnsureWindow(ViewMode.Fitting);});break;
   case 3:
    ui.Text("REQUIRED SKILLS",839,248,Cyan,13);
    var skills=h?.RawFields.Where(p=>p.Key.Contains("Skill",StringComparison.OrdinalIgnoreCase)).OrderBy(p=>p.Key).ToList();
    if(skills?.Count>0)InfoRows(skills.Select(p=>(p.Key,p.Key,p.Value)).ToList(),283,12);else InfoParagraph("Skill prerequisites were not supplied for this hull.",839,288,445,4,Muted);
    InfoParagraph("Source skill IDs are preserved. Skill names, mastery certificates and skill training enforcement await the skill catalog. No mastery level is awarded by this window.",839,642,445,4,Muted);break;
   case 4:
    var root=h==null?"":h.BaseHullId.Length>0?h.BaseHullId:h.Id;
    var variants=h==null?Array.Empty<FleetHull>():FleetCatalog.All.Where(v=>v.Id==root||v.BaseHullId==root).ToArray();
    ui.Button(new(1070,204,222,26),shipInfoCompare?"Variation cards":"Compare all",()=>{shipInfoCompare=!shipInfoCompare;shipInfoPage=0;});
    int pages=Math.Max(1,(variants.Length+5)/6);shipInfoPage=Math.Clamp(shipInfoPage,0,pages-1);int y=246;
    foreach(var v in variants.Skip(shipInfoPage*6).Take(6)){var hull=v;ui.Button(new(839,y,453,30),ShortInfo(v.Name,48),()=>ShowShipInformation(hull.Id),"Inspect variation / "+v.Name,active:v.Id==h?.Id);ui.Text(shipInfoCompare?$"Shield {CompareValue(v,"shieldCapacity")} | Armor {CompareValue(v,"armorHP")} | Speed {CompareValue(v,"maxVelocity")}":ShortInfo(v.Class+" / "+(v.Playable?"Playable":"Registry reference"),60),849,y+38,Muted,11);y+=73;}
    if(variants.Length==0)InfoParagraph("No registry lineage is defined for this legacy hull.",839,250,445,5,Muted);
    ui.Button(new(839,715,105,27),"Previous page",()=>shipInfoPage=Math.Max(0,shipInfoPage-1));ui.Text($"{shipInfoPage+1}/{pages} / {variants.Length} hulls",979,724,Muted,11);ui.Button(new(1187,715,105,27),"Next page",()=>shipInfoPage=Math.Min(pages-1,shipInfoPage+1));break;
   case 5:
    ui.Text("BLUEPRINT",839,249,Cyan,13);InfoParagraph("Blueprint recipes, production times and material requirements are not present in the ship registry.",839,282,445,5,Muted);
    ui.Text("REPROCESSING",839,421,Cyan,13);InfoParagraph("Reprocessing yields and manufacturing dependencies are unavailable. Industry actions will be connected when authoritative recipes are imported.",839,454,445,6,Muted);
    ui.Text("Cargo capacity: "+Value("capacity"),839,625,White,12);ui.Text("Hull volume: "+Value("volume"),839,655,White,12);break;
   case 6:
    ui.Text("FACTORY FINISH",839,249,Cyan,13);InfoParagraph("The preview displays the authored factory finish where a ship model exists.",839,283,445,5);
    ui.Text("SKIN COLLECTION",839,431,Cyan,13);InfoParagraph("No SKIN catalog or owned licenses have been imported. Edition hulls appear under Variations. Preview controls do not purchase, grant or equip a skin.",839,465,445,7,Muted);break;
  }
  ui.Text(h?.Playable==true?"Integrated hull mechanics / source data retained / native fitting rules":"Unavailable means missing source data. Specifications do not imply implemented gameplay.",423,765,Muted,10);
 }
}
