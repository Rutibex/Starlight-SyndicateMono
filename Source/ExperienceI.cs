using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class ExperienceStateI
{
 public string Career{get;set;}="Freeform";public bool Guided{get;set;}public bool TutorialSkipped{get;set;}
 public Dictionary<string,decimal> Progress{get;set;}=[];public HashSet<string> Achievements{get;set;}=[];
 public List<MessageI> Messages{get;set;}=[];public HashSet<string> Contacts{get;set;}=[];public List<IntelI> Intel{get;set;}=[];
 public List<CosmeticI> Cosmetics{get;set;}=[];public List<SequenceI> Sequences{get;set;}=[];public HashSet<string> BoughtListings{get;set;}=[];
 public Dictionary<string,string> Applied{get;set;}=[];public Dictionary<string,string> CorporateMarks{get;set;}=[];public string Title{get;set;}="";
 public Dictionary<string,DateTime> Negotiated{get;set;}=[];public HashSet<string> Packs{get;set;}=[];public Dictionary<string,int> Science{get;set;}=[];
 public List<ScienceClaimI> ScienceClaims{get;set;}=[];public List<CombatEventB> CombatArchive{get;set;}=[];public List<string> Notifications{get;set;}=[];
 public SettingsI Settings{get;set;}=new();
}
public sealed class SettingsI {public int Width{get;set;}=1600;public int Height{get;set;}=900;public bool HighContrast{get;set;}public bool LargeText{get;set;}public bool ReducedMotion{get;set;}public bool Audio{get;set;}=true;public float Volume{get;set;}=.25f;public int RenderBudget{get;set;}=256;public bool PerformanceOverlay{get;set;}public Dictionary<string,string> Bindings{get;set;}=[];}
public sealed class MessageI {public string Id{get;set;}=Guid.NewGuid().ToString("N");public DateTime At{get;set;}public string Channel{get;set;}="Local";public string Sender{get;set;}="player";public string System{get;set;}="";public string Body{get;set;}="";public List<string> Recipients{get;set;}=[];public bool Read{get;set;}}
public sealed class IntelI {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Scout{get;set;}="";public string Target{get;set;}="";public string Name{get;set;}="";public string System{get;set;}="";public string Grid{get;set;}="";public Vector3 Position{get;set;}public DateTime Observed{get;set;}public DateTime Expires{get;set;}}
public sealed class CosmeticI {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Name{get;set;}="";public string Hull{get;set;}="";public string Primary{get;set;}="Slate";public string Accent{get;set;}="Cyan";public string Pattern{get;set;}="Panels";public bool Owned{get;set;}}
public sealed class SequenceI {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Design{get;set;}="";public string Station{get;set;}="";public DateTime Finish{get;set;}public string State{get;set;}="Active";}
public sealed class ScienceClaimI {public string System{get;set;}="";public DateTime Next{get;set;}}
public sealed record GoalI(string Id,string Name,decimal Target,string Guidance,ViewMode Window);
public static class ExperienceCatalogI
{
 public static readonly string[] Careers=["Freeform","Prospector","Security","Explorer","Industrialist"];
 public static readonly GoalI[] Goals=[new("mine","First extraction",50,"Fit a miner while docked. Undock, warp to a belt, lock an asteroid and activate mining. Mission ore counts too.",ViewMode.Inventory),new("combat","First applied damage",1,"Undock, lock a hostile, load a weapon and activate it. Misses and training-range damage do not count.",ViewMode.Fitting),new("scan","First probe analysis",1,"Fit a probe launcher, carry probes, deploy a formation and complete Analyze. Use the results to refine range.",ViewMode.Scanner),new("industry","First manufactured delivery",1,"Buy a blueprint, acquire its quoted materials, install manufacturing and deliver the finished job.",ViewMode.Industry)];
 public static readonly Dictionary<string,Color> Paints=new(){["Slate"]=new(112,136,155),["Cyan"]=new(56,198,219),["Amber"]=new(221,161,66),["Crimson"]=new(181,61,71),["Ivory"]=new(223,219,196),["Emerald"]=new(67,167,111)};
 public static readonly string[] Patterns=["Panels","Alternating","Bands"];
 public static readonly Dictionary<string,string> DefaultBindings=new(){["Experience"]="Alt+F10",["Inventory"]="I",["Market"]="B",["Industry"]="Y",["Skills"]="X",["Commander"]="Alt+K",["Population"]="Alt+P",["Map"]="G",["Manual"]="F12"};
 public static readonly string[] Marks=["Star","Chevron","Ring","Diamond"];
}
public sealed partial class DemoWorld
{
 public ExperienceStateI Experience{get;set;}=new();
 public void RecordExperienceI(string id,decimal amount)
 {
  var goal=ExperienceCatalogI.Goals.FirstOrDefault(g=>g.Id==id);if(goal==null||amount<=0)return;
  Experience.Progress[id]=Math.Min(goal.Target,Experience.Progress.GetValueOrDefault(id)+amount);
  if(Experience.Progress[id]>=goal.Target&&Experience.Achievements.Add(id))NotifyExperienceI("Achievement: "+goal.Name);
  if(ExperienceCatalogI.Goals.All(g=>Experience.Achievements.Contains(g.Id))&&Experience.Achievements.Add("graduate"))NotifyExperienceI("Title earned: Independent Capsuleer");
 }
 public void NotifyExperienceI(string text){Experience.Notifications.Add(Now.ToString("u")+" / "+text);if(Experience.Notifications.Count>100)Experience.Notifications.RemoveAt(0);Say(text);}
 public string ChooseCareerI(string career,bool guided)
 {if(!ExperienceCatalogI.Careers.Contains(career))return "Choose a listed career.";Experience.Career=career;Experience.Guided=guided;Experience.TutorialSkipped=!guided;return "";}
 public IEnumerable<GoalI> CareerGoalsI=>ExperienceCatalogI.Goals.OrderBy(g=>g.Id==(Experience.Career switch{"Explorer"=>"scan","Security"=>"combat","Industrialist"=>"industry",_=>"mine"})?0:1);
 public GoalI? NextGoalI=>Experience.Guided?CareerGoalsI.FirstOrDefault(g=>!Experience.Achievements.Contains(g.Id)):null;
 public IEnumerable<PilotC> ChannelPilotsI(string channel)=>Population.Pilots.Where(p=>p.Status!="Lost"&&(channel=="Local"?p.System==SystemId:channel=="Fleet"?p.Contracted&&!p.Suspended:channel=="Corp"?CorporationG?.Members.ContainsKey(p.Id)==true:false));
 public string PostMessageI(string channel,string body)
 {
  if(channel is not("Local" or "Fleet" or "Corp")||string.IsNullOrWhiteSpace(body)||body.Length>240||body.Any(char.IsControl))return "Choose Local, Corp or Fleet and enter up to 240 printable characters.";
  var recipients=ChannelPilotsI(channel).Select(p=>p.Id).ToList();if(channel!="Local"&&recipients.Count==0)return "No living recipients in that channel.";
  AddMessageI(new(){At=Now,System=SystemId,Channel=channel,Body=body,Recipients=recipients});return "";
 }
 void AddMessageI(MessageI m){Experience.Messages.Add(m);if(Experience.Messages.Count>200)Experience.Messages.RemoveAt(0);}
 public string ContactPilotI(string id)
 {var p=PopulationPilot(id);if(p==null||p.System!=SystemId||p.Status=="Lost")return "Meet a living pilot in this system first.";Experience.Contacts.Add(id);AddMessageI(new(){At=Now,System=SystemId,Sender=id,Channel="Mail",Body=$"{p.Name}: {p.Role} / {p.Goal}. My present assignment is in {p.System}. Reports and negotiations reflect local knowledge.",Recipients=["player"]});return "";}
 public string NegotiatePilotI(string id)
 {
  var p=PopulationPilot(id);if(!Docked||p?.Contracted!=true||p.Suspended||p.System!=SystemId||!Experience.Contacts.Contains(id)||Experience.Negotiated.GetValueOrDefault(id)>Now)return "Dock with a contacted active local crew member; negotiation has a one-day cooldown.";
  // Improvement requires earned shared service, not a free wage toggle.
  if(p.Experience<2)return "Serve together first: pilot needs two experience points.";
  p.Wage=Math.Max(10,p.Wage-1);p.Relationships["player"]=Math.Min(10,p.Relationships.GetValueOrDefault("player")+1);Experience.Negotiated[id]=Now.AddDays(1);RememberC(p,"Negotiated earned crew rate "+p.Wage);AddMessageI(new(){At=Now,System=SystemId,Sender=id,Channel="Mail",Body="Crew service agreement updated. Wage: "+p.Wage+" CR per crew payroll period.",Recipients=["player"]});return "";
 }
 public string RequestIntelI(string scout)
 {
  var p=PopulationPilot(scout);if(p?.Contracted!=true||p.Role!="Scout"||p.Suspended||p.Status=="Lost"||p.System!=SystemId||p.Grid!=Grid||Docked)return "Deploy an active contracted scout on your current grid.";
  var visible=Targets.Where(t=>t.Alive&&t.Id!=p.Id&&p.Knowledge.GetValueOrDefault(t.Id)>Now).ToArray();if(visible.Length==0)return "Scout has no fresh observed contacts. Wait for its sensor cycle.";
  foreach(var t in visible.Take(16)){Experience.Intel.RemoveAll(i=>i.Scout==scout&&i.Target==t.Id);Experience.Intel.Add(new(){Scout=scout,Target=t.Id,Name=t.Name,System=SystemId,Grid=Grid,Position=t.Position,Observed=Now,Expires=Now.AddSeconds(20)});}
  while(Experience.Intel.Count>100)Experience.Intel.RemoveAt(0);return "";
 }
 public bool IntelFreshI(IntelI report)=>report.Observed<=Now&&report.Expires>Now;
 public string DesignCosmeticI(string name,string primary,string accent,string pattern)
 {
  if(string.IsNullOrWhiteSpace(name)||name.Length>48||name.Any(char.IsControl)||!ExperienceCatalogI.Paints.ContainsKey(primary)||!ExperienceCatalogI.Paints.ContainsKey(accent)||!ExperienceCatalogI.Patterns.Contains(pattern)||Experience.Cosmetics.Count>=128)return "Use a name up to 48 characters, valid paints and a listed pattern; design limit 128.";
  Experience.Cosmetics.Add(new(){Name=name,Hull=Player.CombatHull,Primary=primary,Accent=accent,Pattern=pattern});return "";
 }
 public string SequenceCosmeticI(string id)
 {
  var d=Experience.Cosmetics.FirstOrDefault(d=>d.Id==id);if(!Docked||d==null||d.Owned||Experience.Sequences.Any(j=>j.Design==id&&j.State!="Delivered")||Balance<50||Hangar.GetValueOrDefault("ore")<5)return "Dock with an unowned design, 50 CR and 5 local hangar ore; no duplicate job.";
  Hangar["ore"]-=5;WalletChange(-50,"Local paint sequencing / "+d.Name);Experience.Sequences.Add(new(){Design=id,Station=StationKey44,Finish=Now.AddHours(1)});return "";
 }
 public string ClaimCosmeticI(string job)
 {
  AdvanceExperienceI();var j=Experience.Sequences.FirstOrDefault(j=>j.Id==job);if(!Docked||j==null||j.State!="Ready"||j.Station!=StationKey44)return "Collect a ready sequence at its issuing station.";
  Experience.Cosmetics.Single(d=>d.Id==j.Design).Owned=true;j.State="Delivered";return "";
 }
 public string BuyCosmeticI(string pilot)
 {
  var p=PopulationPilot(pilot);string listing=StationKey44+"/"+pilot+"/"+Player.CombatHull;
  if(!Docked||p==null||p.Role!="Trader"||p.Status=="Lost"||p.System!=SystemId||Balance<100||Experience.BoughtListings.Contains(listing)||Experience.Cosmetics.Count>=128)return "Dock with 100 CR and a living local trader; this hull listing has one license.";
  WalletChange(-100,"Cosmetic license / "+p.Name);p.Wallet+=100;Population.ExternalNet+=100;Experience.BoughtListings.Add(listing);Experience.Cosmetics.Add(new(){Name=p.Name+" / local livery",Hull=Player.CombatHull,Primary="Amber",Accent="Slate",Pattern="Bands",Owned=true});return "";
 }
 public string ApplyCosmeticI(string id)
 {var d=Experience.Cosmetics.FirstOrDefault(d=>d.Id==id);if(!Docked||d?.Owned!=true||d.Hull!=Player.CombatHull)return "Dock with an owned design for the active hull.";Experience.Applied[Player.InstanceId]=id;return "";}
 public CosmeticI? ActiveCosmeticI=>Experience.Cosmetics.FirstOrDefault(d=>d.Id==Experience.Applied.GetValueOrDefault(Player.InstanceId)&&d.Hull==Player.CombatHull&&d.Owned);
 public string SetCorporateMarkI(string mark)
 {if(CorporationG==null||!HasRoleG("Director")||!ExperienceCatalogI.Marks.Contains(mark))return "Corporate Director access and a listed mark required.";Experience.CorporateMarks[CorporationG.Id]=mark;return "";}
 public string SetTitleI(string title){if(title!=""&&!(title=="Independent Capsuleer"&&Experience.Achievements.Contains("graduate")))return "Earn the career graduate title first.";Experience.Title=title;return "";}
 public string RebindI(string action,string chord)
 {
  if(!ExperienceCatalogI.DefaultBindings.ContainsKey(action)||!ValidChordI(chord)||ExperienceCatalogI.DefaultBindings.Keys.Any(a=>a!=action&&BindingI(a)==chord))return "Choose a registered action and a unique supported key chord.";
  Experience.Settings.Bindings[action]=chord;return "";
 }
 public string BindingI(string action)=>Experience.Settings.Bindings.GetValueOrDefault(action,ExperienceCatalogI.DefaultBindings.GetValueOrDefault(action,""));
 public static bool ValidChordI(string value)=>value!="Alt+Shift+P"&&Regex.IsMatch(value,@"^(Alt\+)?(Shift\+)?(F10|F12|I|B|Y|X|K|P|G|F7|F6|F5)$");
 public void AdvanceExperienceI(){foreach(var j in Experience.Sequences.Where(j=>j.State=="Active"&&j.Finish<=Now)){j.State="Ready";NotifyExperienceI("Paint sequence ready at "+LocationName(j.Station));}}
 public string TogglePackI(string id)
 {if(id!="local-science")return "Only the validated local-science optional pack is available. Raffles and legacy packs remain excluded.";if(!Experience.Packs.Add(id))Experience.Packs.Remove(id);return "";}
 public string SubmitScienceI(string classification)
 {
  if(!Experience.Packs.Contains("local-science")||Docked)return "Enable local-science and examine a celestial in space.";
  string correct=ImportedWorld43?Celestials.FirstOrDefault(c=>c.Id==SolarCelestial)?.Kind.ToString()??"":Grid=="belt"?"AsteroidBelt":Grid=="station"?"Station":"";
  if(correct.Length==0||classification!=correct)return "Classification does not match the observed celestial.";
  if(Experience.ScienceClaims.Any(c=>c.System==SystemId&&c.Next>Now))return "One accepted survey per system per simulated day.";
  Experience.ScienceClaims.RemoveAll(c=>c.System==SystemId);Experience.ScienceClaims.Add(new(){System=SystemId,Next=Now.AddDays(1)});Experience.Science[classification]=Experience.Science.GetValueOrDefault(classification)+1;NotifyExperienceI("Local science survey recorded: "+classification);return "";
 }
 public void ArchiveCombatI(CombatEventB e){Experience.CombatArchive.Add(e);if(Experience.CombatArchive.Count>2000)Experience.CombatArchive.RemoveAt(0);if(e.Source=="player"&&e.Applied>0&&!WeaponTestMode)RecordExperienceI("combat",1);}
 public static void ValidateExperienceLinksI(SaveData data)
 {
  var s=data.Experience;if(s==null)return;bool Actor(string id)=>id=="player"||data.Population?.Pilots.Any(p=>p.Id==id)==true;
  bool System(string id)=>data.WorldDefinitionId==Spatial43.WorldId?Spatial43.IsSystem(id):Systems.Any(sys=>sys.Id==id);
  bool Station(string id)=>data.WorldDefinitionId==Spatial43.WorldId?Stations44.ById.ContainsKey(id):Systems.Any(sys=>sys.Id==id);
  if(s.Contacts.Any(id=>!Actor(id))||s.Negotiated.Keys.Any(id=>!Actor(id))||s.Messages.Any(m=>!Actor(m.Sender)||m.Recipients.Any(id=>!Actor(id))||!System(m.System))||s.Intel.Any(i=>!Actor(i.Scout)||!System(i.System))||s.ScienceClaims.Any(c=>!System(c.System))||s.Sequences.Any(j=>!Station(j.Station))||s.Cosmetics.Any(d=>!(d.Hull is "frigate" or "cruiser" or "battleship"||FleetCatalog.ById.ContainsKey(d.Hull)))||s.CorporateMarks.Any(m=>data.Organisations?.Corps.Any(c=>c.Id==m.Key)!=true||!ExperienceCatalogI.Marks.Contains(m.Value)))throw new InvalidDataException("Experience contains unresolved actors, hulls or locations.");
 }
 public static void ValidateExperienceI(ExperienceStateI? s)
 {
  if(s==null)return;
  if(s.Progress==null||s.Achievements==null||s.Messages==null||s.Contacts==null||s.Intel==null||s.Cosmetics==null||s.Sequences==null||s.Applied==null||s.Settings==null||s.Settings.Bindings==null||s.Notifications==null||s.CombatArchive==null||s.Packs==null||s.BoughtListings==null||s.Negotiated==null||s.CorporateMarks==null||s.Science==null||s.ScienceClaims==null||!ExperienceCatalogI.Careers.Contains(s.Career)||s.Progress.Any(p=>!ExperienceCatalogI.Goals.Any(g=>g.Id==p.Key&&p.Value>=0&&p.Value<=g.Target))||s.Achievements.Any(a=>a!="graduate"&&!ExperienceCatalogI.Goals.Any(g=>g.Id==a))||s.Messages.Count>200||s.Messages.Any(m=>m.Body==null||m.Body.Length>500||m.Recipients==null||m.Channel is not("Local" or "Corp" or "Fleet" or "Mail"))||s.Intel.Count>100||s.Intel.Any(i=>i.Expires<i.Observed||!float.IsFinite(i.Position.X)||!float.IsFinite(i.Position.Y)||!float.IsFinite(i.Position.Z))||s.Cosmetics.Count>128||s.Cosmetics.Select(d=>d.Id).Distinct().Count()!=s.Cosmetics.Count||s.Cosmetics.Any(d=>d.Name==null||d.Name.Length>100||!ExperienceCatalogI.Paints.ContainsKey(d.Primary)||!ExperienceCatalogI.Paints.ContainsKey(d.Accent)||!ExperienceCatalogI.Patterns.Contains(d.Pattern))||s.Sequences.Count>128||s.Applied.Count>1024||s.Sequences.Select(j=>j.Id).Distinct().Count()!=s.Sequences.Count||s.Sequences.Any(j=>!s.Cosmetics.Any(d=>d.Id==j.Design)||j.State is not("Active" or "Ready" or "Delivered"))||s.Applied.Any(a=>!s.Cosmetics.Any(d=>d.Id==a.Value&&d.Owned))||!new[]{"1280x720","1600x900","1920x1080"}.Contains(s.Settings.Width+"x"+s.Settings.Height)||s.Settings.RenderBudget is <32 or >512||!float.IsFinite(s.Settings.Volume)||s.Settings.Volume<0||s.Settings.Volume>1||s.Settings.Bindings.Any(b=>!ExperienceCatalogI.DefaultBindings.ContainsKey(b.Key)||!ValidChordI(b.Value))||s.CombatArchive.Count>2000||s.Notifications.Count>100||s.Packs.Any(p=>p!="local-science")||s.Science.Values.Any(n=>n<0))throw new InvalidDataException("Invalid experience, communications, cosmetics or settings state.");
  var chords=ExperienceCatalogI.DefaultBindings.Keys.Select(a=>s.Settings.Bindings.GetValueOrDefault(a,ExperienceCatalogI.DefaultBindings[a]));if(chords.Distinct().Count()!=chords.Count())throw new InvalidDataException("Duplicate command bindings.");
 }
}
