using Microsoft.Xna.Framework;
using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed class SiteProfileN
{
 public string Id{get;set;}="";public string Name{get;set;}="";public string Family{get;set;}="";public string Faction{get;set;}="";public int Rating{get;set;}public int Pockets{get;set;}=1;public int Groups{get;set;}public int MaxSize{get;set;}=6;public string Objective{get;set;}="KillAll";public bool Public{get;set;}public int Containers{get;set;}=1;public int Leg{get;set;}public bool Blitz{get;set;}public int Version{get;set;}=1;
 public string Source{get;set;}="https://support.eveonline.com/hc/en-us/articles/203269661-DED-Complexes";
 public string Qualification{get;set;}="Playable adaptation";
 public EncounterDefinitionF Definition=>new(Id,Name,Family,Objective,Math.Clamp(Rating,1,5),Pockets,Groups,Blitz?"Commander":"None",MaxSize,180,"e_relic",0,"Recover physical loot. "+(Blitz?"The commander unlocks a blitz route; escorts may survive. ":"Complete the pocket objective. ")+"Approach gates within 2,500 m. This is a Starlight adaptation.");
}
public static class SiteCatalogN
{
 public static readonly string[] Factions=["Angel","Blood","Guristas","Sansha","Serpentis"];
 public static readonly SiteProfileN[] All=Build();
 static SiteProfileN[] Build(){var list=new List<SiteProfileN>();foreach(var f in Factions){
  foreach(var kind in new[]{"Data","Relic"})for(int band=1;band<=3;band++)list.Add(new(){Id=$"n_{f}_{kind}_{band}",Name=$"{DisplayFaction(f)} {kind.ToLowerInvariant()} archive / sector {band}",Faction=f,Family=kind,Rating=band,Objective="Hack",Containers=3+band,Source="https://support.eveonline.com/hc/en-us/articles/203209902-Scanning"});
  list.Add(new(){Id=$"n_{f}_belt",Name=$"{DisplayFaction(f)} belt patrol",Faction=f,Family="Belt",Public=true,Groups=3,Rating=1});
  for(int band=1;band<=3;band++)list.Add(new(){Id=$"n_{f}_anomaly_{band}",Name=$"{DisplayFaction(f)} contested strongpoint / sector {band}",Faction=f,Family="Anomaly",Public=true,Groups=2+band*2,Rating=band,Pockets=band});
  for(int rating=1;rating<=10;rating++)list.Add(new(){Id=$"n_{f}_ded_{rating}",Name=$"{DisplayFaction(f)} rated complex / {rating} of 10",Faction=f,Family="DED",Rating=rating,Pockets=rating<=4?2:3,Groups=2+rating,MaxSize=rating==1?1:rating==2?1:rating==3?2:rating==4?2:6,Blitz=rating%2==0});
  for(int leg=1;leg<=4;leg++)list.Add(new(){Id=$"n_{f}_unrated_{leg}",Name=$"{DisplayFaction(f)} lost expedition / leg {leg}",Faction=f,Family="Unrated",Rating=leg,Groups=3+leg,Pockets=2,Leg=leg,Blitz=true});
  list.Add(new(){Id=$"n_{f}_ghost",Name=$"{DisplayFaction(f)} covert research facility",Faction=f,Family="Ghost",Rating=3,Objective="Hack",Containers=4,Source="https://support.eveonline.com/hc/en-us/articles/203209812-Ghost-Sites-Covert-Research-Facility-Besieged-Research-Facility"});
 }
 foreach(var (name,level) in new[]{("Limited",1),("Standard",2),("Superior",3)})list.Add(new(){Id=$"n_cache_{level}",Name=name+" reservoir puzzle",Family="Cache",Rating=level,Pockets=level+1,Objective="Hack",Containers=2+level,MaxSize=level==1?1:6,Source="https://support.eveonline.com/hc/en-us/articles/203209842-Sleeper-Cache"});
 return list.ToArray();}
 public static string DisplayFaction(string f)=>Retheme41.Text(f switch{"Angel"=>"Angel Cartel","Blood"=>"Blood Raider Covenant","Guristas"=>"Guristas Pirates","Sansha"=>"Sansha's Nation","Serpentis"=>"Serpentis",_=>f});
 public static SiteProfileN Copy(SiteProfileN d)=>JsonSerializer.Deserialize<SiteProfileN>(JsonSerializer.Serialize(d))!;
 public static SiteProfileN Get(string id)=>All.Single(p=>p.Id==id);
}
public sealed partial class DemoWorld
{
 public string RegionalFactionN(){if(ImportedWorld43&&SystemId.StartsWith("system:")&&Galaxy42.ById.TryGetValue(int.Parse(SystemId[7..]),out var system)){switch(system.FactionId){case 500001:return "Guristas";case 500002:return "Angel";case 500003:return "Blood";case 500004:return "Serpentis";}}string f=Current.Faction.ToLowerInvariant();if(f.Contains("caldari"))return "Guristas";if(f.Contains("gallente"))return "Serpentis";if(f.Contains("minmatar"))return "Angel";if(f.Contains("amarr"))return "Blood";return SiteCatalogN.Factions[StableC(Current.Region)%5];}
 public EncounterF AddSiteN(string profile,string system,string parent=""){
  var p=SiteCatalogN.Copy(SiteCatalogN.Get(profile));var e=AddEncounterF(p.Id,system,p.Public,parent);e.ProfileN=p;UtilitySites.First(s=>s.Id=="f:"+e.Id).Kind=p.Family is "Ghost" or "Cache"?"Data":p.Family is "Data" or "Relic"?p.Family:"Combat";e.Reward=0;e.NextCompetitorN=Now.AddMinutes(30);e.Expires=Now.AddHours(parent.Length>0?24:3);return e;
 }
 public void EnsureRegionalSitesN(){
  string key=SystemId+"/N1";if(!Exploration.SeededSystems.Add(key))return;string f=RegionalFactionN();int band=Current.Security>=.5?1:Current.Security>0?2:3;
  foreach(string id in new[]{$"n_{f}_Data_{band}",$"n_{f}_Relic_{band}",$"n_{f}_belt",$"n_{f}_anomaly_{band}",$"n_{f}_ded_{(band==1?1:band==2?5:8)}",$"n_{f}_unrated_1",$"n_{f}_ghost",$"n_cache_{band}"})AddSiteN(id,SystemId);
 }
 public bool SiteAvailableN(UtilitySite site){if(!site.Id.StartsWith("f:"))return true;var e=Exploration.Encounters.FirstOrDefault(e=>"f:"+e.Id==site.Id);if(e==null||e.Expires<=Now)return false;if(e.ProfileN==null)return e.State is "Available" or "Active" or "Completed";return !site.Completed&&(e.State is "Available" or "Active");}
 public bool BindSiteN(EncounterF e){var p=e.ProfileN;if(p==null)return false;var run=EnsureExplorationGraphL(e);
  for(int i=0;i<p.Groups;i++){
   string role=i==0?"commander":p.Rating>=5&&i==1?"logistics":i==2?"ewar":"guard"+i;SpawnEncounterEnemyF(e,role,new(6000+i*1600,0,1200*i));var target=Targets.Last(t=>t.Id==$"f:{e.Id}:{role}:{e.Pocket}");target.Name=SiteCatalogN.DisplayFaction(p.Faction)+" / "+role;
   string ew=role=="ewar"?p.Faction switch{"Blood"=>"Neutralizer","Guristas"=>"ECM","Serpentis"=>"Damp","Sansha"=>"Tracking",_=>"Web"}:"";
   var state=new NpcStateL{Role=role=="logistics"?"logistics":role=="ewar"?"ewar":"gunner",Ewar=ew,Group="n/"+e.Pocket,Bounty=40+p.Rating*30};run.Npcs[target.Id]=state;PrepareNpcL(target,state);
  }
  if(p.Objective=="Hack")for(int i=0;i<p.Containers;i++)Targets.Add(new(){Id=$"f:{e.Id}:archive:{e.Pocket}:{i}",Name=p.Family=="Cache"&&i==0?"Hazard control unit":p.Name+" container "+(i+1),Kind=TargetKind.Container,SiteKind=p.Family=="Relic"?"Relic":"Data",Position=new(1800+i*3000,0,i*1900),Contents=new(){{p.Family=="Data"?"e_decryptor":p.Family=="Ghost"?"e_mutator":"e_relic",(float)(1+(int)(run.Roll()*3))}},Signature=100});
  if(p.Family=="Cache")Targets.Add(new(){Id=$"f:{e.Id}:bypass:{e.Pocket}",Name="Hazard bypass control",Kind=TargetKind.Container,SiteKind="Data",Position=new(12000,0,12000),Contents=[],Signature=100});
  Targets.Add(new(){Id=$"f:{e.Id}:gate:{e.Pocket}",Name=e.Pocket+1==p.Pockets?"Completion beacon":"Acceleration gate",Kind=TargetKind.Celestial,Position=new(9000,0,0),Signature=2000});
  if(p.Family=="Ghost"&&e.HazardUntilN==default)e.HazardUntilN=Now.AddSeconds(90+run.Roll()*90);
  e.HazardTickN=Now;SyncEncounterF();return true;
 }
 public bool EvaluateSiteN(EncounterF e){var p=e.ProfileN;if(p==null)return false;
  var cans=Targets.Where(t=>t.Id.StartsWith($"f:{e.Id}:archive:{e.Pocket}:" )).ToArray();
  bool clear=!Targets.Any(t=>t.Id.StartsWith("f:"+e.Id+":")&&t.Kind==TargetKind.Raider&&!t.Friendly&&t.Alive);
  bool bypass=Targets.Any(t=>t.Id==$"f:{e.Id}:bypass:{e.Pocket}"&&t.Hacked);
  bool control=cans.FirstOrDefault()?.Hacked==true;if(control)e.Events.Add("control:"+e.Pocket);if(bypass)e.Events.Add("bypass:"+e.Pocket);
  bool exhausted=cans.Length>0&&cans.All(t=>t.Destroyed||t.Hacked&&t.Contents.Values.All(q=>q<=0));
  e.PocketReady=p.Objective=="Hack"?p.Family=="Cache"?control||bypass:exhausted:clear||p.Blitz&&(e.Events.Contains("key:"+e.Pocket)||StackCount("n_gate_key",false)>=1);
  var run=EnsureExplorationGraphL(e);if(e.PocketReady)run.Emit("objective/"+e.Pocket);run.Pump(Now);return true;
 }
 public void CompleteSiteN(EncounterF e){var p=e.ProfileN;if(p==null||e.ExpeditionRolledN)return;e.ExpeditionRolledN=true;var run=EnsureExplorationGraphL(e);
  if(p.Family is "DED" or "Unrated"&&p.Leg<4){bool escalate=run.Roll()<.35;if(escalate){string destination=Current.Links.FirstOrDefault()??"";if(destination.Length>0){int next=p.Family=="DED"?1:p.Leg+1;var child=AddSiteN($"n_{p.Faction}_unrated_{next}",destination,e.Id);child.OriginStation=e.OriginStation;child.ExpeditionRootN=e.ExpeditionRootN.Length>0?e.ExpeditionRootN:e.Id;JournalF("Expedition discovered / "+destination+" / 24-hour expiry");}}}
 }
 public void FailHackSiteN(SpaceTarget target){var e=ActiveEncounterF;if(e?.ProfileN==null)return;if(e.ProfileN.Family=="Ghost"){target.Destroyed=true;target.Contents.Clear();if(Vector3.Distance(Player.Position,target.Position)<10000)DealPlayerDamage(800,DamageType.Explosive);}if(target.Destroyed)target.Contents.Clear();SyncEncounterF();}
 public void TickSitesN(double seconds){
  foreach(var e in Exploration.Encounters.Where(e=>e.ProfileN!=null&&e.State is "Available" or "Active").ToArray()){
   var site=UtilitySites.FirstOrDefault(s=>s.Id=="f:"+e.Id);if(e.State=="Available"&&e.Expires>Now&&Now>=e.NextCompetitorN&&e.ProfileN!.Public){e.NextCompetitorN=Now.AddMinutes(30);var pilot=Population.Pilots.FirstOrDefault(p=>p.System==e.System&&!p.Contracted&&p.Status!="Lost"&&p.Role=="Scout");if(pilot!=null&&!pilot.Knowledge.ContainsKey("site:"+e.Id)){pilot.Knowledge["site:"+e.Id]=Now;e.NextCompetitorN=Now.AddMinutes(30);continue;}if(pilot!=null&&pilot.Knowledge.ContainsKey("site:"+e.Id)&&EnsureExplorationGraphL(e).Roll()<.2){e.State="Expired";e.Failure="Consumed by competing explorer";pilot.Cargo["salvage"]=pilot.Cargo.GetValueOrDefault("salvage")+3;pilot.Memory.Add("Recovered survey site "+e.Id);if(site!=null)site.Completed=true;}}
   if(e.ProfileN!.Family=="Ghost"&&e.State=="Active"&&e.HazardUntilN!=default&&Now>=e.HazardUntilN){var targets=Grid=="f:"+e.Id?Targets:e.Targets;foreach(var t in targets.Where(t=>t.Kind==TargetKind.Container&&t.Alive)){if(Grid=="f:"+e.Id&&Vector3.Distance(Player.Position,t.Position)<10000)DealPlayerDamage(800,DamageType.Explosive);t.Destroyed=true;t.Contents.Clear();}FailEncounterF(e,"Research charges detonated");if(site!=null)site.Completed=true;SyncEncounterF();}
  }
  if(ActiveEncounterF is {} a&&a.ProfileN?.Family=="Cache"&&a.State=="Active"&&!a.Events.Contains("control:"+a.Pocket)&&!a.Events.Contains("bypass:"+a.Pocket)&&seconds>0){DealPlayerDamage((float)Math.Min(200,seconds*8*a.ProfileN.Rating),DamageType.Thermal);a.HazardTickN=Now;}
 }
}
