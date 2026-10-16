using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;

// Definitions are copied into each run. Changing the catalogue cannot change an accepted contract.
public sealed class GraphNodeL
{
 public string Id{get;set;}="";public string Kind{get;set;}="Event";public string Ref{get;set;}="";public string Text{get;set;}="";
 public List<string> Needs{get;set;}=[];public bool Any{get;set;}public int Pocket{get;set;}=-1;public double Seconds{get;set;}
 public List<string> Next{get;set;}=[];
}
public sealed class GraphGateL
{
 public string Id{get;set;}="";public int From{get;set;}public int To{get;set;}public List<string> Needs{get;set;}=[];
 public string Key{get;set;}="";public bool ConsumeKey{get;set;}public int MaxSize{get;set;}=6;public float Range{get;set;}=2500;public string Text{get;set;}="Complete the pocket objective.";
}
public sealed class GraphDefinitionL
{
 public string Id{get;set;}="";public int Version{get;set;}=1;public string Source{get;set;}="";public string Profile{get;set;}="Playable adaptation";
 public List<GraphNodeL> Nodes{get;set;}=[];public List<GraphGateL> Gates{get;set;}=[];public int Pockets{get;set;}=1;
}
public sealed class NpcStateL
{
 public string Role{get;set;}="gunner";public string Group{get;set;}="";public string Hull{get;set;}="frigate";public string Ewar{get;set;}="";
 public bool Aggro{get;set;}=true;public float EwarLeft{get;set;}public float RepairLeft{get;set;}public float PendingRepair{get;set;}public string RepairTarget{get;set;}="";
 public string AggroTarget{get;set;}="";public float ThinkLeft{get;set;}public decimal Bounty{get;set;}=70;
}
public sealed class DropSealL
{
 public string Target{get;set;}="";public string Owner{get;set;}="player";public Vector3 Position{get;set;}public Dictionary<string,float> Loot{get;set;}=[];
 public float Salvage{get;set;}public int Pocket{get;set;}public bool BountyPaid{get;set;}public Dictionary<string,decimal> BountyShares{get;set;}=[];
}
public sealed class SettlementL
{
 public string Id{get;set;}="";public DateTime Time{get;set;}public decimal Total{get;set;}public int LP{get;set;}public string Corporation{get;set;}="";
 public Dictionary<string,decimal> Money{get;set;}=[];public Dictionary<string,int> Loyalty{get;set;}=[];public Dictionary<string,double> Standings{get;set;}=[];
}
public sealed class GraphRunL
{
 public GraphDefinitionL Definition{get;set;}=new();public string Digest{get;set;}="";public string State{get;set;}="Active";public string Failure{get;set;}="";
 public int Pocket{get;set;}public DateTime Started{get;set;}public DateTime Expires{get;set;}public HashSet<string> Events{get;set;}=[];public HashSet<string> Fired{get;set;}=[];
 public Dictionary<string,string> Choices{get;set;}=[];public Dictionary<int,DateTime> Entered{get;set;}=[];
 public Dictionary<int,List<SpaceTarget>> Snapshots{get;set;}=[];public Dictionary<string,NpcStateL> Npcs{get;set;}=[];public Dictionary<string,DropSealL> Drops{get;set;}=[];
 public Dictionary<string,Dictionary<string,float>> Contributions{get;set;}=[];
 public HashSet<string> Generated{get;set;}=[];public List<string> History{get;set;}=[];public List<string> Effects{get;set;}=[];
 public ulong RandomState{get;set;}=21977;public List<string> SharePilots{get;set;}=[];public bool ShareRewards{get;set;}public SettlementL? Settlement{get;set;}
 public bool Emit(string key){if(State is "Failed" or "Expired")return false;return Events.Add(key);}
 public double Roll(){ulong x=RandomState==0?21977:RandomState;x^=x<<13;x^=x>>7;x^=x<<17;RandomState=x;return (x>>11)*(1.0/9007199254740992.0);}
 public bool Choose(string group,string value){if(State!="Active"||Choices.ContainsKey(group))return false;Choices[group]=value;Emit("choice:"+group+":"+value);return true;}
 public bool Ready(GraphNodeL node,DateTime now)
 {
  if(node.Pocket>=0&&node.Pocket!=Pocket)return false;
  bool deps=node.Needs.Count==0||(node.Any||node.Kind=="Any"?node.Needs.Any(Fired.Contains):node.Needs.All(Fired.Contains));if(!deps)return false;
  return node.Kind switch{"Event" or "Choice" or "Proximity"=>Events.Contains(node.Ref),"Timer"=>Entered.TryGetValue(Pocket,out var at)&&now>=at.AddSeconds(node.Seconds),_=>true};
 }
 public void Pump(DateTime now)
 {
  if(State!="Active")return;if(now>=Expires){State="Expired";Failure="Encounter deadline expired";History.Add(Failure);return;}
  bool progress;do{progress=false;foreach(var n in Definition.Nodes){if(Fired.Contains(n.Id)||!Ready(n,now))continue;Fired.Add(n.Id);progress=true;
   if(n.Kind=="Spawn")Effects.Add(n.Ref);if(n.Kind=="Complete")State="Completed";if(n.Kind=="Fail"){State="Failed";Failure=n.Text;}
   if(n.Text.Length>0){History.Add(n.Text);if(History.Count>300)History.RemoveAt(0);}foreach(string next in n.Next)Emit("node:"+next);if(State!="Active")break;
  }}while(progress&&State=="Active");
 }
 public bool GateReady(GraphGateL gate)=>State is "Active" or "Completed"&&gate.From==Pocket&&gate.Needs.All(Fired.Contains)&&(gate.Key.Length==0||gate.Key.StartsWith("item:")||Events.Contains("key:"+gate.Key));
}
public static class GraphCompilerL
{
 static readonly HashSet<string> Kinds=["Event","Choice","Proximity","Timer","All","Any","Spawn","Objective","Complete","Fail"];
 public static string Validate(GraphDefinitionL d)
 {
  if(d.Id.Length==0||d.Version<1||d.Pockets<1||d.Pockets>32||d.Nodes.Count>10000)return "Invalid graph identity or bounds.";
  var ids=new HashSet<string>();foreach(var n in d.Nodes)if(n.Id.Length==0||!ids.Add(n.Id)||!Kinds.Contains(n.Kind)||n.Pocket>=d.Pockets||n.Pocket< -1||!double.IsFinite(n.Seconds)||n.Seconds<0)return "Invalid or duplicate graph node.";
  foreach(var n in d.Nodes)if(n.Needs.Concat(n.Next).Any(id=>!ids.Contains(id)))return "Unresolved graph edge.";
  // Dependency cycles never produce an executable event; backtracking belongs to the gate graph.
  var visiting=new HashSet<string>();var done=new HashSet<string>();bool Visit(string id){if(done.Contains(id))return true;if(!visiting.Add(id))return false;foreach(string dep in d.Nodes.First(n=>n.Id==id).Needs)if(!Visit(dep))return false;visiting.Remove(id);done.Add(id);return true;}
  if(d.Nodes.Any(n=>!Visit(n.Id)))return "Cyclic trigger dependency.";
  var gates=new HashSet<string>();foreach(var g in d.Gates)if(!gates.Add(g.Id)||g.Id.Length==0||g.From<0||g.To<0||g.From>=d.Pockets||g.To>=d.Pockets||g.MaxSize<0||g.MaxSize>8||!float.IsFinite(g.Range)||g.Range<=0||g.Needs.Any(n=>!ids.Contains(n)))return "Invalid gate rule.";
  return "";
 }
 public static string Digest(GraphDefinitionL definition)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(definition))));
 public static GraphRunL Start(GraphDefinitionL definition,DateTime now,DateTime expires,ulong seed)
 {
  string error=Validate(definition);if(error.Length>0)throw new InvalidDataException(error);string json=JsonSerializer.Serialize(definition);var copy=JsonSerializer.Deserialize<GraphDefinitionL>(json)!;
  return new(){Definition=copy,Digest=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))),Started=now,Expires=expires,RandomState=seed==0?21977:seed,Entered=new(){{0,now}}};
 }
 public static GraphDefinitionL Mission(Mission38 m)
 {
  if(m.Definition=="l_relay")return Relay(m);
  m.ProfileM??=m.Spec.ProfileM==null?null:MissionCatalogM.Copy(m.Spec.ProfileM);if(m.ProfileM!=null)return MissionCatalogM.Graph(m);
  var d=new GraphDefinitionL{Id="agent/"+m.Definition,Source=m.Spec.Source,Profile="Legacy mission adaptation / graph v1",Pockets=Math.Max(1,m.Spec.Pockets)};
  if(m.Spec.Division==AgentDivision38.Distribution){d.Nodes.Add(new(){Id="delivery",Ref="delivery",Kind="Event",Text="Deliver the mission freight at the destination."});d.Nodes.Add(new(){Id="done",Kind="Complete",Needs=["delivery"]});return d;}
  var finals=new List<string>();for(int p=0;p<d.Pockets;p++){
   int waves=m.Spec.Division==AgentDivision38.Security||m.Spec.Hostiles?Math.Max(1,m.Spec.Waves.Length):0;string prior="";
   for(int w=0;w<waves;w++){string spawn=$"spawn/{p}/{w}",clear=$"clear/{p}/{w}";d.Nodes.Add(new(){Id=spawn,Kind="Spawn",Ref=$"legacy/{p}/{w}",Pocket=p,Needs=prior.Length>0?[prior]:[]});var kills=new List<string>();int count=Math.Max(0,m.Spec.Waves[w]);for(int i=0;i<count;i++){string k=$"kill/{p}/{w}/{i}";kills.Add(k);d.Nodes.Add(new(){Id=k,Ref=$"kill:{m.WarpId}:{p}:{w}:{i}",Needs=[spawn]});}d.Nodes.Add(new(){Id=clear,Kind="All",Needs=kills,Text=$"Pocket {p+1}, wave {w+1} cleared."});prior=clear;
   }
   string ready="ready/"+p;if(prior.Length>0)d.Nodes.Add(new(){Id=ready,Kind="All",Needs=[prior]});else d.Nodes.Add(new(){Id=ready,Kind="All",Pocket=p});finals.Add(ready);
   if(p+1<d.Pockets)d.Gates.Add(new(){Id="forward/"+p,From=p,To=p+1,Needs=[ready],Text="Clear the pocket's hostile waves."});
   if(p>0)d.Gates.Add(new(){Id="back/"+p,From=p,To=p-1,Text="Return to the previous pocket."});
  }
  if(m.Spec.Division==AgentDivision38.Mining)d.Nodes.Add(new(){Id="done",Kind="Complete",Ref="",Needs=["harvest"]});
  else if(m.Spec.Objective=="Structure"){d.Nodes.Add(new(){Id="structure",Ref=$"kill:{m.WarpId}:0:0:0"});d.Nodes.Add(new(){Id="done",Kind="Complete",Needs=["structure",finals.Last()],Any=true});}
  else d.Nodes.Add(new(){Id="done",Kind="Complete",Needs=[finals.Last()]});
  if(m.Spec.Division==AgentDivision38.Mining)d.Nodes.Add(new(){Id="harvest",Ref="harvest",Text="Mission resource harvested. Deliver the physical cargo."});return d;
 }
 public static GraphDefinitionL Relay(Mission38 m)
 {
  var d=new GraphDefinitionL{Id="starlight/relay-recovery",Source="Starlight authored acceptance operation",Profile="Authored branching tactical operation",Pockets=2};
  d.Nodes.Add(new(){Id="initial",Kind="Spawn",Ref="relay/0",Pocket=0});
  d.Nodes.Add(new(){Id="commander",Ref="kill:"+m.WarpId+":0:0:0",Text="Commander neutralized. The relay gate is unlocked."});
  d.Nodes.Add(new(){Id="near",Kind="Proximity",Ref="near:relay",Pocket=0,Text="Relay proximity alarm triggered."});
  d.Nodes.Add(new(){Id="alarm",Kind="Spawn",Ref="relay/alarm",Pocket=0,Needs=["near"],Text="A neutralizer reinforcement has arrived."});
  d.Nodes.Add(new(){Id="second",Kind="Spawn",Ref="relay/1",Pocket=1});
  d.Nodes.Add(new(){Id="dispatch",Ref="collect",Text="Dispatch recovered in your mission cargo."});
  d.Nodes.Add(new(){Id="done",Kind="Complete",Needs=["dispatch"]});
  d.Gates.Add(new(){Id="forward/0",From=0,To=1,Needs=["commander"],MaxSize=3,Text="Destroy the relay commander; optional escorts may survive."});d.Gates.Add(new(){Id="back/1",From=1,To=0,Text="Return to the relay entry pocket."});return d;
 }
 public static GraphDefinitionL Exploration(EncounterDefinitionF def)
 {
  var d=new GraphDefinitionL{Id="exploration/"+def.Id,Source="Stage F/H authored operation",Profile="Authored exploration adapter",Pockets=def.Pockets};
  for(int p=0;p<d.Pockets;p++){d.Nodes.Add(new(){Id="ready/"+p,Kind="Event",Ref="objective/"+p,Pocket=p,Text="Pocket objective secured."});if(p+1<d.Pockets)d.Gates.Add(new(){Id="forward/"+p,From=p,To=p+1,Needs=["ready/"+p],MaxSize=def.MaxSize});}
  d.Nodes.Add(new(){Id="done",Kind="Complete",Needs=["ready/"+(d.Pockets-1)]});return d;
 }
}
