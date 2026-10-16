using System.Text.Json;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class Station44
{
 public string Id{get;set;}="";public string System{get;set;}="";public string Owner{get;set;}="";public string Faction{get;set;}="";public string Name{get;set;}="";public string Parent{get;set;}="";
 public int SourceId{get;set;}public int TypeId{get;set;}public int OperationId{get;set;}public int[] Services{get;set;}=[];
 public SpacePosition Position{get;set;}public double Radius{get;set;}public bool RadiusKnown{get;set;}public double ReprocessingEfficiency{get;set;}public double ReprocessingTake{get;set;}public JsonElement Source{get;set;}
 // The export has no docking port geometry. This external service point is an authored convention.
 public SpacePosition DockPoint=>Position+new SpacePosition(0,Radius+5000,0);
 public Celestial Celestial=>new(Id,System,Name,CelestialKind.Station,Position,Radius,Parent,"Station");
}
public sealed class Agent44
{
 public string Id{get;set;}="";public string Character{get;set;}="";public string Name{get;set;}="";public string Corporation{get;set;}="";public string Faction{get;set;}="";public string Station{get;set;}="";public string System{get;set;}="";public string Disposition{get;set;}="";
 public int Level{get;set;}public int DivisionId{get;set;}public int AgentTypeId{get;set;}public bool Locator{get;set;}public bool Supported{get;set;}public JsonElement Space{get;set;}public JsonElement Source{get;set;}
 public Agent38 Contact=>new(Id,Name,Corporation,Faction,Station,Level,DivisionId==22?AgentDivision38.Distribution:DivisionId==23?AgentDivision38.Mining:AgentDivision38.Security);
}
public static class Stations44
{
 static string Root=>Path.Combine(AppContext.BaseDirectory,"Assets","Stations44");
 public static readonly Station44[] Stations=Read<Station44[]>("stations.json");public static readonly Agent44[] Agents=Read<Agent44[]>("agents.json");
 public static readonly Dictionary<string,Station44> ById=Stations.ToDictionary(s=>s.Id);
 public static readonly Dictionary<string,Station44[]> BySystem=Stations.GroupBy(s=>s.System).ToDictionary(g=>g.Key,g=>g.ToArray());
 public static readonly Dictionary<string,Agent44> AgentById=Agents.ToDictionary(a=>a.Id);
 public static readonly Agent38[] Playable=Agents.Where(a=>a.Supported).Select(a=>a.Contact).ToArray();
 public static readonly Dictionary<string,Agent38> Contacts=Playable.ToDictionary(a=>a.Id);
 public static readonly Lazy<JsonDocument> Raw=new(()=>JsonDocument.Parse(File.ReadAllText(Path.Combine(Root,"source.json"))));
 static T Read<T>(string file)=>JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(Root,file)))!;
 public static string Name(string key)=>Retheme41.Records.TryGetValue(key,out var r)?r.Name:key;
 public static string SystemOf(string station)=>ById.TryGetValue(station,out var s)?s.System:station;
 public static Agent38? Contact(string id)=>Contacts.GetValueOrDefault(id)??AgentCatalog38.Agents.FirstOrDefault(a=>a.Id==id)??DemoWorld.ResolveStorylineM(id);
 public static string ServiceName(int id)=>id switch{1=>"Bounty contracts",2=>"Target contracts",3=>"Delivery contracts",4=>"Transit network",5=>"Reprocessing",6=>"Refinery",7=>"Market",8=>"Private market",9=>"Exchange",10=>"Cloning",11=>"Medical",12=>"Genetic therapy",13=>"Repair",14=>"Manufacturing",15=>"Research",16=>"Games",17=>"Fitting",18=>"Paintshop",19=>"News",20=>"Storage",21=>"Insurance",22=>"Docking",23=>"Office rental",24=>"Jump clones",25=>"Loyalty store",26=>"Navy office",27=>"Security office",_=>"Service "+id};
 public static double? Required(Station44 s,int service)
 {if(s.Faction.Length==0)return null;foreach(var r in Raw.Value.RootElement.GetProperty("stationStandingsRestrictions").EnumerateArray())if("faction:"+r.GetProperty("_key").GetInt32()==s.Faction)foreach(var f in r.GetProperty("services").EnumerateArray())if(f.GetProperty("_key").GetInt32()==service)return f.GetProperty("_value").GetDouble();return null;}
 public static bool EnabledService(int id)=>id is 5 or 6 or 7 or 10 or 11 or 13 or 14 or 15 or 17 or 20 or 22 or 24 or 25;
 static InformationEntry Biography(string key)
 {
  if(!Retheme41.TryInformation(key,out var e))return new();
  var copy=new InformationEntry{Key=e.Key,Id=e.Id,Name=e.Name,Kind=e.Kind,Group=e.Group,Description=e.Description,MarketId=e.MarketId,System=e.System,LastSeen=e.LastSeen,Tabs=e.Tabs.ToDictionary(t=>t.Key,t=>t.Value.ToList()),Stats=new(e.Stats),Related=new(e.Related)};
  if(copy.Tabs.TryGetValue("Identity",out var rows))for(int i=0;i<rows.Count;i++)if(rows[i].Label=="Availability")rows[i]=rows[i] with{Value="Biography and placement metadata; supported mission contacts are listed at their station."};
  return copy;
 }
 public static InformationEntry? Information(string key)
 {
  string id=key.StartsWith("lore/")?key[5..]:key.StartsWith("station44/")?key[10..]:key.StartsWith("agent44/")?key[8..]:"";
  if(ById.TryGetValue(id,out var s)){
   var e=Biography("lore/"+s.Id);e.Key=key;e.System=s.System;
   e.Add("Attributes","Local metres",$"{s.Position.X:R}, {s.Position.Y:R}, {s.Position.Z:R}");e.Add("Attributes","Type radius (metres)",s.Radius.ToString("R"));e.Add("Attributes","Reprocessing efficiency",s.ReprocessingEfficiency.ToString("P1"));e.Add("Attributes","Base reprocessing take",s.ReprocessingTake.ToString("P1"));
   e.Add("Relationships","Orbit parent",s.Parent,"celestial/"+s.System+"/"+s.Parent);e.Add("Relationships","System",Name(s.System),"system/"+s.System);e.Add("Relationships","Owner",Name(s.Owner),"lore/"+s.Owner);
   foreach(int service in s.Services){double? required=Required(s,service);e.Add("Services",ServiceName(service),(EnabledService(service)?"Supported":"Reference; workflow pending")+(required.HasValue?$" / faction standing {required:0.##}":""));}
   foreach(var agent in Agents.Where(a=>a.Station==s.Id&&a.Space.ValueKind==JsonValueKind.Null))e.Add("Agents",agent.Name,$"L{agent.Level} / {agent.Disposition}","agent44/"+agent.Id);
   foreach(var f in s.Source.EnumerateObject())e.Add("Source audit",f.Name,f.Value.ToString());return e;
  }
  if(id.StartsWith("npc:"))id="agent:"+id[4..];
  if(AgentById.TryGetValue(id,out var a)){
   var e=Biography("lore/"+a.Character);e.Key=key;e.Id=a.Id;e.System=a.System;e.Add("Attributes","Level",a.Level.ToString());e.Add("Attributes","Division",a.DivisionId switch{22=>"Distribution",23=>"Mining",24=>"Security",18=>"Research",_=>"Specialist"});e.Add("Attributes","Locator",a.Locator?"Flag present; locator workflow pending":"No");e.Add("Attributes","Workflow",a.Disposition);
   e.Add("Relationships","Operating system",Name(a.System),"system/"+a.System);e.Add("Relationships","Recorded station",Name(a.Station),"station44/"+a.Station);e.Add("Relationships","Corporation",Name(a.Corporation),"lore/"+a.Corporation);
   if(a.Space.ValueKind==JsonValueKind.Object)e.Add("Attributes","In-space location","Dungeon/spawn reference retained; coordinates not supplied. Station record is not a docked mission offer.");
   foreach(var f in a.Source.EnumerateObject())e.Add("Source audit",f.Name,f.Value.ToString());if(a.Space.ValueKind==JsonValueKind.Object)foreach(var f in a.Space.EnumerateObject())e.Add("Source audit","space."+f.Name,f.Value.ToString());return e;
  }return null;
 }
}
public sealed partial class DemoWorld
{
 public string StationId="";public string PendingStation44="";
 public string StationKey44=>ImportedWorld43?StationId:SystemId;
 public Station44? CurrentStation44=>ImportedWorld43?Stations44.ById.GetValueOrDefault(StationId):null;
 public string StationName44=>CurrentStation44?.Name??Current.Name+" Anchorage";
 public IEnumerable<Agent38> WorldAgents44=>ImportedWorld43?Stations44.Playable:AgentCatalog38.Agents;
 public bool ServiceAllowed44(int service)
 {if(!ImportedWorld43)return true;var s=CurrentStation44;if(!Docked||s==null||!s.Services.Contains(service))return false;var req=Stations44.Required(s,service);return !req.HasValue||EffectiveStanding38(s.Faction)>=req.Value;}
 public bool RequireService44(int service){if(ServiceAllowed44(service))return true;Say(Stations44.ServiceName(service)+" unavailable at this station or standing is too low.");return false;}
 public bool DockSource44()
 {
  var id=SelectedId.StartsWith("station:")?SelectedId:SolarCelestial;var s=Stations44.ById.GetValueOrDefault(id);
  if(s==null||s.System!=SystemId||Screen!=ViewMode.Space||Transit!=null){Say("Select an imported station on this grid before docking.");return false;}
  if(Player.MobilitySpooling||SupportDockLock>0||!CanDockAtStructure("player")){Say("Docking interlock active.");return false;}
  if(!s.Services.Contains(22)||Stations44.Required(s,22) is {} r&&EffectiveStanding38(s.Faction)<r){Say("Docking service requires higher faction standing.");return false;}
  PendingStation44=s.Id;
  var d=(s.DockPoint-SystemPosition).Length;if(d>1800){if(d>=SolarCatalog.MinimumWarp){PendingStation44="";return BeginWarp(s.Id,0,"dock");}SelectedId=s.Id;Player.Order="point";Player.ManualPoint=(s.DockPoint-SolarOrigin).Local;Throttle=1;DockPending=true;return true;}
  StationId=s.Id;RememberSolarGrid();CompleteDock();PendingStation44="";return Docked;
 }
 public void PopulateStations44()
 {
  if(!ImportedWorld43)return;
  foreach(var s in Stations44.BySystem.GetValueOrDefault(SystemId)??[])if((s.DockPoint-SolarOrigin).Length<SolarCatalog.GridSize/2&&!Targets.Any(t=>t.Id==s.Id))Targets.Add(new(){Id=s.Id,Name=s.Name,Kind=TargetKind.Station,Position=(s.DockPoint-SolarOrigin).Local,Signature=(float)Math.Max(1500,s.Radius)});
 }
 public void MissionTravel44(Mission38 m)
 {string target=m.Spec.Division==AgentDivision38.Distribution?m.Destination:m.SiteSystem;string sys=Stations44.SystemOf(target);if(Docked)Undock();if(SystemId!=sys){SetDestination(sys);Say("Travel to "+Stations44.Name(sys)+" first.");return;}BeginWarp(m.Spec.Division==AgentDivision38.Distribution?(ImportedWorld43?target:SystemId+":station"):m.WarpId,0,m.Spec.Division==AgentDivision38.Distribution?"dock":"");}
}
public sealed partial class DemoWorld
{
 public static void ValidateStationSave44(SaveData data)
 {
  bool source=data.WorldDefinitionId==Spatial43.WorldId;
  if(!source){if(!string.IsNullOrEmpty(data.StationId)&&data.StationId!=data.SystemId)throw new InvalidDataException("Ambiguous legacy station identity.");data.StationId=data.SystemId;return;}
  if(data.SaveSchemaVersion==43){data.StationId="";data.PendingStation44="";data.Screen=(int)ViewMode.Space;}
  bool Valid(string? id)=>string.IsNullOrEmpty(id)||Stations44.ById.TryGetValue(id,out var s)&&s.System==data.SystemId;
  if(!Valid(data.StationId)||!Valid(data.PendingStation44)||data.Screen==(int)ViewMode.Station&&string.IsNullOrEmpty(data.StationId))throw new InvalidDataException("Docked station does not belong to the saved system.");
  if(data.StationItems!=null&&data.StationItems.Keys.Any(k=>!Stations44.ById.ContainsKey(k)))throw new InvalidDataException("Imported hangar key is ambiguous; no system-to-station conversion was guessed.");
 }
 public string ImportedMissionDestination44(Agent38 agent)
 {
  if(agent.Division!=AgentDivision38.Distribution)return StationKey44;
  var local=(Stations44.BySystem.GetValueOrDefault(SystemId)??[]).FirstOrDefault(s=>s.Id!=StationKey44&&s.Services.Contains(22)&&(!Stations44.Required(s,22).HasValue||EffectiveStanding38(s.Faction)>=Stations44.Required(s,22)));
  if(local!=null)return local.Id;
  foreach(int sys in Galaxy42.ById[int.Parse(SystemId[7..])].Neighbours.Order())foreach(var s in Stations44.BySystem.GetValueOrDefault("system:"+sys)??[])if(s.Services.Contains(22)&&(!Stations44.Required(s,22).HasValue||EffectiveStanding38(s.Faction)>=Stations44.Required(s,22)))return s.Id;
  return "";
 }
}
