using System.Text.Json;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class SpawnM
{
 public string Name{get;set;}="";public string Hull{get;set;}="frigate";public int Count{get;set;}=1;public string Ewar{get;set;}="";public string Role{get;set;}="gunner";public float Distance{get;set;}=20000;
}
public sealed class WaveM {public string Trigger{get;set;}="entry";public List<SpawnM> Ships{get;set;}=[];}
public sealed class MissionProfileM
{
 public string Mode{get;set;}="Retrieve";public string CargoName{get;set;}="Mission freight";public float UnitVolume{get;set;}=1;public decimal Collateral{get;set;}
 public string Revision{get;set;}="";public string Qualification{get;set;}="Reviewed playable adaptation";public string Gaps{get;set;}="NPC statistics, rewards, layout and visuals remain adaptations.";
 public float ObjectiveDistance{get;set;}=2000;public int Rocks{get;set;}=1;public List<WaveM> Waves{get;set;}=[];public string Task{get;set;}="";
}
public sealed class ChainStepM {public string Id{get;set;}="";public string Mission{get;set;}="";public string Next{get;set;}="";public Dictionary<string,string> Branches{get;set;}=[];}
public sealed class ChainDefinitionM
{
 public string Id{get;set;}="";public string Name{get;set;}="";public string Kind{get;set;}="Epic";public string Briefing{get;set;}="";public bool Once{get;set;}public int RepeatDays{get;set;}=90;public double FactionGain{get;set;}=.02;public List<ChainStepM> Steps{get;set;}=[];
}
public sealed class ChainRunM
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public ChainDefinitionM Definition{get;set;}=new();public string Step{get;set;}="";public string State{get;set;}="Active";public string Mission{get;set;}="";public string Origin{get;set;}="";public string Faction{get;set;}="";public DateTime RepeatAfter{get;set;}public Dictionary<string,string> Choices{get;set;}=[];public List<string> Completed{get;set;}=[];
}
public static class MissionCatalogM
{
 public static List<MissionDefinition38> Load()=>JsonSerializer.Deserialize<List<MissionDefinition38>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","MissionsM.json")),WeaponCatalog.JsonOptions)??[];
 public static readonly List<ChainDefinitionM> Chains=JsonSerializer.Deserialize<List<ChainDefinitionM>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ChainsM.json")))??[];
 public static readonly List<ReferenceM> References=JsonSerializer.Deserialize<List<ReferenceM>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","MissionReferencesM.json")))??[];
 public static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
 public static GraphDefinitionL Graph(Mission38 m)
 {
  var p=m.ProfileM!;var d=new GraphDefinitionL{Id="m/"+m.Definition,Source=m.Spec.Source+" / "+p.Revision,Profile=p.Qualification};
  void Event(string id,string reference)=>d.Nodes.Add(new(){Id=id,Ref=reference});
  if(p.Mode=="Retrieve"){d.Nodes.Add(new(){Id="objective",Kind="Spawn",Ref="m/objective",Pocket=0});Event("collect","collect");d.Nodes.Add(new(){Id="done",Kind="Objective",Needs=["collect"],Text="Required cargo recovered. Return to the issuing station; surviving hostiles are optional."});}
  else if(p.Mode=="Mining"){d.Nodes.Add(new(){Id="resource",Kind="Spawn",Ref="m/resource",Pocket=0});Event("harvest","harvest");d.Nodes.Add(new(){Id="done",Kind="Objective",Needs=["harvest"],Text="Harvest target reached. Deliver the physical cargo."});}
  else if(p.Mode=="Task"){Event("task","task:"+p.Task);d.Nodes.Add(new(){Id="done",Kind="Objective",Needs=["task"],Text="Training task completed. Return to your instructor."});}
  else if(p.Mode=="Combat"){}
  else {Event("delivery","delivery");d.Nodes.Add(new(){Id="done",Kind="Complete",Needs=["delivery"]});return d;}
  for(int w=0;w<p.Waves.Count;w++){
   var wave=p.Waves[w];string spawn="spawn/"+w;var deps=new List<string>();if(wave.Trigger!="entry"){string trigger="trigger/"+w;Event(trigger,wave.Trigger);deps.Add(trigger);}
   d.Nodes.Add(new(){Id=spawn,Kind="Spawn",Ref="m/wave/"+w,Needs=deps,Pocket=0});int count=wave.Ships.Sum(s=>s.Count);var killed=new List<string>();for(int i=0;i<count;i++){string id=$"kill/{w}/{i}";Event(id,$"kill:{m.WarpId}:0:{w}:{i}");d.Nodes.Last().Needs=[spawn];killed.Add(id);}d.Nodes.Add(new(){Id="clear/"+w,Kind="All",Needs=killed,Text="Hostile group "+(w+1)+" cleared."});
  }
  if(p.Mode=="Combat")d.Nodes.Add(new(){Id="done",Kind="Objective",Needs=p.Waves.Select((_,i)=>"clear/"+i).ToList(),Text="Encounter secured. Return to the contact."});
  Event("delivery","delivery");d.Nodes.Add(new(){Id="settled",Kind="Complete",Needs=["delivery","done"]});return d;
 }
}
public sealed class ReferenceM {public string Name{get;set;}="";public string Kind{get;set;}="";public string Source{get;set;}="";public string Status{get;set;}="Reference / not implemented";}
public sealed partial class DemoWorld
{
 public Agent38? StorylineContactM(Mission38 m)
 {
  if(ImportedWorld43)return Stations44.Agents.Where(a=>a.AgentTypeId is 6 or 7&&a.Level==m.Spec.Level&&a.Faction==m.Contact.Faction&&a.Space.ValueKind==JsonValueKind.Null&&Stations44.ById.ContainsKey(a.Station)).OrderBy(a=>JumpDistance(LocationSystem(m.Contact.Station),a.System)).ThenBy(a=>a.Id).Select(a=>a.Contact).FirstOrDefault();
  var a=WorldAgents44.Where(a=>a.Faction==m.Contact.Faction&&a.Level==m.Spec.Level&&a.Division==AgentDivision38.Distribution).OrderBy(a=>JumpDistance(m.Contact.Station,a.Station)).ThenBy(a=>a.Id).FirstOrDefault();return a==null?null:a with{Id="storyline/"+a.Id,Name="Envoy "+a.Name};
 }
 public static Agent38? ResolveStorylineM(string id)
 {
  if(id.StartsWith("storyline/")){var a=AgentCatalog38.Agents.FirstOrDefault(a=>a.Id==id[10..]);return a==null?null:a with{Id=id,Name="Envoy "+a.Name};}
  return Stations44.AgentById.TryGetValue(id,out var imported)&&imported.AgentTypeId is 6 or 7?imported.Contact:null;
 }
 public void QualifyStorylineM(Mission38 m,DateTime now)
 {
  string key=m.Contact.Faction+"/"+m.Spec.Level;int tally=AgentsState.StorylineCounts.GetValueOrDefault(key)+1;AgentsState.StorylineCounts[key]=tally;if(tally<16)return;
  var a=StorylineContactM(m);if(a==null){Say("Storyline qualification retained: no reachable contact in this catalogue.");return;}
  AgentsState.StorylineCounts[key]=tally-16;var offer=new Mission38{Agent=a.Id,Definition="m_story"+m.Spec.Level,Pickup=a.Station,Destination=a.Station,SiteSystem=LocationSystem(a.Station),Offered=now,Expires=now.AddDays(7),Storyline=true,Reward=m.Reward*2,Bonus=m.Bonus,LP=0};offer.DefinitionSnapshotM=MissionCatalogM.Copy(AgentCatalog38.Missions[offer.Definition]);AgentsState.Missions.Add(offer);EnsureMissionL(offer);Say("Important mission offered by "+a.Name+". Open the journal Offers tab.");
 }
 public string RequestReviewedM(string definition,string chain="",string runId="")
 {
  if(!Docked)return "Dock at an eligible agent station.";if(!AgentCatalog38.Missions.TryGetValue(definition,out var d)||d.ProfileM==null)return "This variant has no Stage M profile.";
  var a=WorldAgents44.FirstOrDefault(a=>a.Station==StationKey44&&a.Level==d.Level&&a.Division==d.Division&&CanUseAgent38(a));if(a==null)return "No eligible local L"+d.Level+" "+d.Division+" agent. Use the Agent Finder.";
  if(AgentsState.Missions.Any(m=>m.Agent==a.Id&&m.State is "Offered" or "Accepted"))return "This agent already has an outstanding contract.";
  string destination=d.ProfileM.Mode=="Courier"&&d.Division==AgentDivision38.Distribution?(ImportedWorld43?ImportedMissionDestination44(a):Systems.Where(s=>s.Id!=SystemId).OrderBy(s=>JumpDistance(SystemId,s.Id)).ThenBy(s=>s.Id).First().Id):StationKey44;
  if(destination.Length==0)return "No reachable delivery station.";decimal reward=(decimal)(80*Math.Pow(4,d.Level-1));var m=new Mission38{Agent=a.Id,Definition=d.Id,Pickup=StationKey44,Destination=destination,SiteSystem=SystemId,Offered=Now,Expires=Now.AddDays(7),Reward=reward,Bonus=reward/2,LP=20*(int)Math.Pow(4,d.Level-1),ChainRunM=runId,ProfileM=MissionCatalogM.Copy(d.ProfileM),DefinitionSnapshotM=MissionCatalogM.Copy(d)};AgentsState.Missions.Add(m);EnsureMissionL(m);return "";
 }
 public string ChainAvailabilityM(ChainDefinitionM d)
 {
  UpdateChainsM();var runs=AgentsState.ChainsM.Where(r=>r.Definition.Id==d.Id).ToArray();if(runs.Any(r=>r.State=="Active"))return "Active";if(d.Once&&runs.Length>0)return "One-time contact exhausted";var last=runs.LastOrDefault();if(last!=null&&Now<last.RepeatAfter)return "Repeat locked until "+last.RepeatAfter.ToString("yyyy-MM-dd");return "Available";
 }
 public string StartChainM(string id)
 {
  if(!Docked)return "Dock to contact a chain instructor.";var d=MissionCatalogM.Chains.FirstOrDefault(d=>d.Id==id);if(d==null)return "Unknown chain.";if(ChainAvailabilityM(d)!="Available")return ChainAvailabilityM(d);
  var r=new ChainRunM{Definition=MissionCatalogM.Copy(d),Step=d.Steps[0].Id,Origin=StationKey44,Faction=StationFaction};string error=RequestReviewedM(d.Steps[0].Mission,id,r.Id);if(error.Length>0)return error;r.Mission=AgentsState.Missions.Last().Id;AgentsState.ChainsM.Add(r);return "";
 }
 public string ContinueChainM(string id,string branch="")
 {
  UpdateChainsM();var r=AgentsState.ChainsM.FirstOrDefault(r=>r.Id==id&&r.State=="Active");if(r==null)return "No active chain.";if(!Docked||StationKey44!=r.Origin)return "Return to the chain's contact station.";if(r.Mission.Length>0)return "Complete the outstanding step first.";
  var step=r.Definition.Steps.Single(s=>s.Id==r.Step);string next=step.Next;if(step.Branches.Count>0){if(!step.Branches.TryGetValue(branch,out next))return "Choose a displayed branch.";}
  if(next.Length==0)return "The chain is complete.";var target=r.Definition.Steps.Single(s=>s.Id==next);string error=RequestReviewedM(target.Mission,r.Definition.Id,r.Id);if(error.Length>0)return error;if(branch.Length>0)r.Choices[step.Id]=branch;r.Step=next;r.Mission=AgentsState.Missions.Last().Id;return "";
 }
 public void UpdateChainsM()
 {
  foreach(var r in AgentsState.ChainsM.Where(r=>r.State=="Active")){
   var m=AgentsState.Missions.FirstOrDefault(m=>m.Id==r.Mission);if(m==null)continue;
   if(m.State is "Failed" or "Expired" or "Declined"){r.State="Failed";r.RepeatAfter=Now.AddDays(r.Definition.RepeatDays);continue;}
   if(m.State!="Completed")continue;if(!r.Completed.Contains(r.Step))r.Completed.Add(r.Step);r.Mission="";var step=r.Definition.Steps.Single(s=>s.Id==r.Step);
   if(step.Next.Length==0&&step.Branches.Count==0){r.State="Completed";r.RepeatAfter=Now.AddDays(r.Definition.RepeatDays);if(r.Definition.Kind=="COSMOS")FactionGain38(r.Faction,r.Definition.FactionGain,"Special chain completed");else ChangeStanding38(r.Faction,r.Definition.FactionGain,"Epic/career chain completed");}
  }
 }
 public void RecordTaskM(string task)
 {
  foreach(var m in AgentsState.Missions.Where(m=>m.State=="Accepted"&&m.ProfileM?.Mode=="Task"&&m.ProfileM.Task==task)){EnsureMissionL(m).Emit("task:"+task);EnsureMissionL(m).Pump(Now);m.ObjectiveDone=EnsureMissionL(m).Fired.Contains("done");}
 }
 void SpawnContentM(Mission38 m,string action)
 {
  var p=m.ProfileM!;var run=m.GraphL!;
  if(action=="m/objective"){if(!run.Events.Contains("collect")&&!Targets.Any(t=>t.Id==m.WarpId+":objective"))Targets.Add(new(){Id=m.WarpId+":objective",Name="Warehouse / "+p.CargoName,Kind=TargetKind.Container,OwnerId=m.WarpId,Position=new(p.ObjectiveDistance,0,0),Signature=300});return;}
  if(action=="m/resource"){float remaining=m.Spec.Volume-m.Harvested;if(remaining<=0||Targets.Any(t=>t.Id.StartsWith(m.WarpId+":resource")))return;for(int i=0;i<p.Rocks;i++)Targets.Add(new(){Id=m.WarpId+":resource:"+i,Name=p.CargoName+" / mission deposit "+(i+1),Kind=TargetKind.Asteroid,Resource=m.Spec.Resource,Ore=remaining/p.Rocks,Position=new(1800+i*1100,0,i*500),Signature=300});return;}
  int wave=int.Parse(action.Split('/')[2]);int index=0;foreach(var source in p.Waves[wave].Ships)for(int i=0;i<source.Count;i++){
   string id=$"{m.WarpId}:0:{wave}:{index++}";if(run.Generated.Contains(id)||m.Kills.Contains(id))continue;var t=MakeRaider(new(source.Distance+i*650,300*i,900*i));t.Id=id;t.Name=Retheme41.Text(source.Name);t.Profile=new Ship();t.Profile.SetCombatHull(source.Hull);t.Shield=t.Profile.MaxShield;t.Armor=t.Profile.MaxArmor;t.Hull=t.Profile.MaxHull;t.Signature=(float)t.Profile.Stat("signatureRadius");var npc=new NpcStateL{Hull=source.Hull,Role=source.Role,Ewar=source.Ewar,Group="m/"+wave,Bounty=70*(decimal)Math.Pow(3,m.Spec.Level-1)};run.Npcs[id]=npc;PrepareNpcL(t,npc);run.Generated.Add(id);Targets.Add(t);
  }m.Wave=Math.Max(m.Wave,wave);
 }
 void MissionEventsM(Mission38 m,string target)
 {
  if(m.ProfileM==null)return;var bits=target.Split(':');if(bits.Length<5||!int.TryParse(bits[^2],out int wave))return;var run=m.GraphL!;run.Emit("killany:"+wave);if(bits[^1]=="0")run.Emit("killfirst:"+wave);if(run.Npcs.TryGetValue(target,out var npc)&&npc.Hull is "destroyer" or "cruiser" or "battlecruiser" or "battleship")run.Emit("killheavy:"+wave);
  if(!Targets.Any(t=>t.Alive&&t.Kind==TargetKind.Raider&&t.Id.StartsWith(m.WarpId+":0:"+wave+":")))run.Emit("clear:"+wave);
 }
}
