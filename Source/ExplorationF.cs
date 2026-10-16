using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class ExplorationStateF
{
 public List<EncounterF> Encounters{get;set;}=[];public List<ConnectionF> Connections{get;set;}=[];public List<ArcF> Arcs{get;set;}=[];public HashSet<string> SeededSystems{get;set;}=[];public List<string> Journal{get;set;}=[];public Dictionary<string,int> Supplies{get;set;}=[];
}
public sealed partial class EncounterF
{
 public GraphRunL? GraphL{get;set;}
 public SiteProfileN? ProfileN{get;set;}public DateTime HazardUntilN{get;set;}public DateTime HazardTickN{get;set;}public DateTime NextCompetitorN{get;set;}public bool ExpeditionRolledN{get;set;}public string ExpeditionRootN{get;set;}="";
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Template{get;set;}="";public string System{get;set;}="";public string OriginStation{get;set;}="";public string State{get;set;}="Available";public int Pocket{get;set;}public int Wave{get;set;}public HashSet<string> Events{get;set;}=[];public List<SpaceTarget> Targets{get;set;}=[];public DateTime Expires{get;set;}public DateTime Started{get;set;}public bool Claimed{get;set;}public bool PocketReady{get;set;}public bool Generated{get;set;}public float Harvested{get;set;}public decimal Reward{get;set;}public string Parent{get;set;}="";public string Weather{get;set;}="";public int Tier{get;set;}public string ReturnGrid{get;set;}="";public Vector3 ReturnPosition{get;set;}public SpacePosition ReturnOrigin{get;set;}public string Failure{get;set;}="";
}
public sealed partial class ConnectionF
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public string A{get;set;}="";public string B{get;set;}="";public string Type{get;set;}="Authored roaming";public double RemainingMass{get;set;}=1000000000;public double MaxShipMass{get;set;}=300000000;public DateTime Expires{get;set;}public bool Collapsed{get;set;}public Dictionary<string,List<DateTime>> Transits{get;set;}=[];
}
public sealed class ArcF {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Station{get;set;}="";public string State{get;set;}="Offered";public string Branch{get;set;}="";public string Encounter{get;set;}="";public DateTime Expires{get;set;}public DateTime RepeatAfter{get;set;}public bool Claimed{get;set;}}
public sealed record EncounterDefinitionF(string Id,string Name,string Kind,string Objective,int Level,int Pockets,int Groups,string Trigger,int MaxSize,int Minutes,string Loot,decimal Reward,string Briefing);
public static class EncounterCatalogF
{
 public static readonly EncounterDefinitionF[] All=[
 new("outpost","Silent relay outpost","Combat","KillAll",1,2,3,"Commander",2,180,"salvage",150,"Destroy the relay commander to trigger reinforcements. Destroy opposition to unlock the gate."),
 new("blitz","The courier's key","Security","Retrieve",2,2,4,"Commander",3,180,"e_relic",240,"The commander carries the gate key. Recover the dispatch from the second pocket; escorts need not all die."),
 new("archive","Cartographer's archive","Data","Hack",1,1,0,"None",3,120,"e_decryptor",100,"Probe the hidden archive, defeat the randomized core and collect its physical contents."),
 new("relic","The broken observatory","Relic","Hack",2,1,0,"None",3,120,"e_relic",180,"Use a relic analyzer. Two failed intrusion attempts destroy the container."),
 new("escort","Surveyor under fire","Security","Escort",2,1,3,"Timer",3,30,"salvage",220,"Keep the surveyor alive for 90 seconds and destroy the attackers. A lost surveyor fails the operation."),
 new("mine","The shrinking seam","Mining","Mine",1,1,2,"Timer",2,30,"e_moon",160,"Harvest 200 cubic metres from the marked ore body and survive the guards. Finite ore survives re-entry."),
 new("ghost","Covert research depot","Ghost","Hack",2,1,0,"None",3,3,"e_mutator",260,"Hidden three-minute operation. Hack before the research charges detonate. Failure destroys the archive."),
 new("cache","Sleeper reservoir","Cache","Hack",3,2,2,"Hack",2,60,"e_relic",350,"Hack the control archive to disable the hazard and open the next gate. Guard groups remain dangerous."),
 new("expedition","A stolen star chart","Expedition","KillAll",3,2,4,"Commander",3,120,"e_mutator",400,"Destroy the command group; completion opens a timed expedition in a neighbouring system."),
 new("abyss","Fractured laboratory","Abyss","KillAll",1,3,3,"None",2,20,"e_mutator",500,"Three authored pockets, a twenty-minute shared clock and a fifty-kilometre boundary. Origin gate is the only safe exit."),
 new("career","First reconnaissance","Career","Hack",1,1,0,"None",2,180,"ut_probe_core",80,"A public training anomaly introduces approach, analyzer activation, hacking and collection."),
 new("sleeper1","Sleeper perimeter","Sleeper","KillAll",2,2,3,"Commander",3,180,"e_relic",300,"An authored resident defense encounter; source C1-C6 spawn fidelity remains unreviewed.")];
 public static EncounterDefinitionF Get(string id)=>All.Concat(AdvancedCatalogH.Encounters).Concat(SiteCatalogN.All.Select(p=>p.Definition)).Concat(EncounterCatalogO.All.Select(p=>p.Definition)).Concat(CatalogP.All.Select(p=>p.Definition)).Single(d=>d.Id==id);
 public static readonly ItemDefinition[] Items=[new("n_gate_key","Survey complex gate key","Exploration / Keys","Physical access token for Starlight rated-complex bypass gates. Consumed on use.",.1f,0,""),new("f_filament","Fracture laboratory filament","Exploration / Filaments","Authored three-pocket laboratory. Consume from cargo in space; cruiser admission, twenty-minute limit.",.1f,200,""),new("f_chart","Expedition chart","Exploration / Objectives","Physical archive acquisition; mission and arc objectives check collection, not clicking a claim button.",.1f,50,"")];
}
public sealed partial class DemoWorld
{
 public ExplorationStateF Exploration{get;set;}=new();
 public EncounterF? ActiveEncounterF=>Exploration.Encounters.FirstOrDefault(e=>Grid=="f:"+e.Id&&e.System==SystemId&&e.State is "Active" or "Completed");
 public bool InAbyssF=>ActiveEncounterF is {Template:"abyss",State:"Active"};
 public void JournalF(string text){Exploration.Journal.Add(Now.ToString("u")+" / "+text);if(Exploration.Journal.Count>400)Exploration.Journal.RemoveAt(0);}
 public EncounterF AddEncounterF(string template,string system,bool publicSite=false,string parent="")
 {
  var def=EncounterCatalogF.Get(template);var e=new EncounterF{Template=template,System=system,OriginStation=EncounterIssuerF(),Expires=Now.AddMinutes(def.Kind=="Ghost"?120:def.Minutes),Reward=def.Reward,Parent=parent};Exploration.Encounters.Add(e);RegisterEncounterF(e,publicSite);return e;
 }
 public string EncounterIssuerF()=>Docked?StationKey44:ImportedWorld43?Stations44.Stations.Where(s=>s.System==SystemId).OrderBy(s=>s.Id).FirstOrDefault()?.Id??(Stations44.ById.ContainsKey(RecoveryStationId)?RecoveryStationId:Stations44.Stations.First().Id):SystemId;
 public void RegisterEncounterF(EncounterF e,bool publicSite=false)
 {if(UtilitySites.Any(s=>s.Id=="f:"+e.Id))return;var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));uint hash=StableC(e.Id);UtilitySites.Add(new(){Id="f:"+e.Id,System=e.System,Kind=d.Kind is "Data" or "Relic"?d.Kind:"Combat",Name=d.Name+" [Starlight authored]",X=.02f+(hash%40)*.01f,Y=.01f,Z=.03f,Difficulty=Math.Clamp(d.Level,1,5),Anomaly=publicSite,Signal=publicSite?100:0,Seen=publicSite});}
 public void EnsureExplorationF()
 {
  EnsureWormholesO();if(!Exploration.SeededSystems.Add(SystemId))return;
  foreach(string id in new[]{"outpost","archive","relic","career","mine","escort","cache","expedition","ghost"})AddEncounterF(id,SystemId,id is "outpost" or "career" or "mine" or "escort");
  string destination=ImportedWorld43?Galaxy42.Data.Systems.Where(s=>s.Id>=31000006&&s.Id<32000000&&s.RegionId>=11000000).OrderBy(s=>s.Id).Select(s=>"system:"+s.Id).FirstOrDefault()??SystemId:Systems.First(s=>s.Id!=SystemId&&s.Security<.5).Id;
  if(destination!=SystemId)AddConnectionF(SystemId,destination);
 }
 public void SyncEncounterF()
 {var e=ActiveEncounterF;if(e==null)return;if(!Targets.Any(t=>t.Id.StartsWith("f:"+e.Id+":")))return;var actors=Targets.Where(t=>(t.Id.StartsWith("f:"+e.Id+":")||t.Id.StartsWith("wreck_f:"+e.Id+":"))).ToList();e.Targets=e.ProfileO!=null?actors:CopyTargets(actors);}
 public bool EnterEncounterF(string id)
 {
  var e=Exploration.Encounters.FirstOrDefault(e=>"f:"+e.Id==id&&e.System==SystemId);if(e==null||e.State is "Failed" or "Expired"||e.Expires<=Now||Docked)return false;
  if(e.FilamentO>0||e.ProfileO!=null&&e.State=="Completed")return false;if((e.ProfileN!=null||e.ProfileO!=null)&&UtilitySites.FirstOrDefault(s=>s.Id==id) is {} site&&!ProbeWarpable(site))return false;if(!EnterGuardH(e)||!EnterGuardO(e)||!EnterGuardP(e))return false;var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));if(Commander.Capsule||CombatHulls.Get(Player.CombatHull).Size>d.MaxSize){Say("Encounter gate rejects this hull size.");return false;}
  SyncEncounterF();if(e.State=="Available"){e.State="Active";e.Started=Now;if(e.Template=="ghost")e.Expires=Now.AddMinutes(3);}EnterGrid(id,false);BindCrewH(e);return true;
 }
 public void BindEncounterF()
 {
  var e=Exploration.Encounters.FirstOrDefault(e=>Grid=="f:"+e.Id&&e.System==SystemId);if(e==null)return;Targets.Clear();Player.WeatherPenaltyO=e.WeatherPenaltyO;Player.CloudO="";Player.EnvironmentF=e.Template=="abyss"&&e.State=="Active"?e.Weather:"";if(e.Generated){EnsureExplorationGraphL(e);Targets.AddRange(CopyTargets(e.Targets));foreach(var target in Targets)if(target.Profile!=null){target.Profile.EnvironmentF=Player.EnvironmentF;target.Profile.WeatherPenaltyO=e.WeatherPenaltyO;}return;}
  EnsureExplorationGraphL(e);e.Generated=true;e.PocketReady=false;if(BindP(e)||BindEncounterO(e)||BindSiteN(e))return;var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));int count=d.Groups+e.Tier;
  for(int i=0;i<count;i++)SpawnEncounterEnemyF(e,i==0?"commander":"guard"+i,new(3500+i*1300,0,700*i));
  if(d.Objective is "Hack" or "Retrieve"||d.Kind=="Abyss")Targets.Add(new(){Id=$"f:{e.Id}:archive:{e.Pocket}",Name=d.Objective=="Retrieve"?"Dispatch container":"Control archive",Kind=TargetKind.Container,Position=new(1500,0,0),SiteKind=d.Objective=="Hack"?d.Kind=="Relic"?"Relic":"Data":"",Contents=new(){{d.Loot,d.Kind=="Abyss"?1:3},{"f_chart",1}},Signature=100});
  if(d.Objective=="Mine")Targets.Add(new(){Id=$"f:{e.Id}:ore:{e.Pocket}",Name="Contract ore seam / 200 m3",Kind=TargetKind.Asteroid,Resource="ore",Ore=200,Position=new(1800,0,0),Signature=200});
  if(d.Objective=="Escort")Targets.Add(new(){Id=$"f:{e.Id}:escort:{e.Pocket}",Name=e.Template=="h_defence"?"Mercenary den controller":e.Template=="h_escort"?"Protected Homefront convoy":"Syndicate surveyor",Kind=TargetKind.Raider,Friendly=true,Training=true,Profile=Ship.RaiderProfile(),Position=new(6000,0,0),Shield=300,Armor=200,Hull=200});
  if(e.Template.StartsWith("h_")&&d.Objective=="Escort"){var escorted=Targets.First(t=>t.Id.Contains(":escort:"));escorted.Profile!.SetStat("shieldCapacity",300);escorted.Profile.SetStat("armorHP",200);escorted.Profile.SetStat("hp",200);}
  Targets.Add(new(){Id=$"f:{e.Id}:gate:{e.Pocket}",Name=e.Pocket==d.Pockets-1?"Origin / completion gate":"Acceleration gate",Kind=TargetKind.Celestial,Position=new(9000,0,0),Signature=2000});SyncEncounterF();
 }
 void SpawnEncounterEnemyF(EncounterF e,string name,Vector3 position)
 {string id=$"f:{e.Id}:{name}:{e.Pocket}";if(Targets.Any(t=>t.Id==id)||e.Events.Contains("kill:"+id))return;var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));var t=MakeRaider(position);t.Id=id;t.Name=d.Name+" / "+name;t.Training=true;t.Profile!.EnvironmentF=e.Template=="abyss"?e.Weather:"";float scale=1+(d.Level-1)*.65f+e.Tier*.5f;t.Shield*=scale;t.Armor*=scale;t.Hull*=scale;t.Profile.SetStat("shieldCapacity",t.Shield);t.Profile.SetStat("armorHP",t.Armor);t.Profile.SetStat("hp",t.Hull);t.Shield=t.Profile.MaxShield;t.Armor=t.Profile.MaxArmor;if(d.Objective=="Escort")t.LockId=$"f:{e.Id}:escort:{e.Pocket}";Targets.Add(t);}
 public void EncounterKillF(SpaceTarget target)
 {
  var e=ActiveEncounterF;if(e==null||e.State!="Active"||!target.Id.StartsWith("f:"+e.Id+":")||!e.Events.Add("kill:"+target.Id))return;var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));
  if(target.Id.Contains(":escort:")){FailEncounterF(e,"Escort destroyed");return;}
  if(target.Id.Contains(":commander:")&&d.Trigger=="Commander"){e.Events.Add("key:"+e.Pocket);SpawnEncounterEnemyF(e,"reinforcement",new(7500,0,2000));JournalF("Commander trigger / reinforcements / "+d.Name);}
  EvaluateEncounterF(e);SyncEncounterF();
 }
 public void EvaluateEncounterF(EncounterF e)
 {
  if(e.State!="Active"||Grid!="f:"+e.Id)return;if(EvaluateP(e)||EvaluateEncounterO(e)||EvaluateSiteN(e))return;var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));bool clear=!Targets.Any(t=>t.Id.StartsWith("f:"+e.Id+":")&&t.Kind==TargetKind.Raider&&!t.Friendly&&t.Alive);
  var archive=Targets.FirstOrDefault(t=>t.Id==$"f:{e.Id}:archive:{e.Pocket}");if(archive?.Destroyed==true&&d.Objective=="Hack"){FailEncounterF(e,"Objective archive destroyed");return;}
  bool hack=archive?.Hacked==true;if(hack)e.Events.Add("hack:"+e.Pocket);
  e.PocketReady=d.Objective switch{"Hack"=>hack&&archive!.Contents.Values.All(q=>q<=0),"Retrieve"=>e.Pocket==0?e.Events.Contains("key:0"):archive!=null&&archive.Contents.Values.All(q=>q<=0),"Mine"=>e.Harvested>=200&&clear,"Escort"=>clear&&Now>=e.Started.AddSeconds(90)&&Targets.Any(t=>t.Id.Contains(":escort:")&&t.Alive),_=>clear};
  if(d.Kind=="Cache"&&hack)e.PocketReady=true;
  var graph=EnsureExplorationGraphL(e);graph.Pocket=e.Pocket;if(!graph.Entered.ContainsKey(e.Pocket))graph.Entered[e.Pocket]=Now;if(e.PocketReady)graph.Emit("objective/"+e.Pocket);graph.Pump(Now);e.PocketReady=graph.Fired.Contains("ready/"+e.Pocket);
 }
 public string AdvancePocketF(string id)
 {
  var e=ActiveEncounterF;if(e==null||e.Id!=id||e.State!="Active")return "Enter the active encounter first.";EvaluateEncounterF(e);if(!e.PocketReady)return "Gate locked: complete the displayed pocket objective.";
  if(e.ProfileN!=null&&(!CanUseGate("player")||Commander.Capsule||CombatHulls.Get(Player.CombatHull).Size>e.ProfileN.MaxSize))return "Hull or propulsion interlock prevents gate activation.";
  var gate=Targets.FirstOrDefault(t=>t.Id==$"f:{e.Id}:gate:{e.Pocket}");if(gate==null||Vector3.Distance(Player.Position,gate.Position)>2500)return "Approach the gate within 2,500 m.";var d=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template));
  if(e.ProfileN?.Blitz==true&&!e.Events.Contains("key:"+e.Pocket)&&StackCount("n_gate_key",false)>=1)ChangeStack("n_gate_key",false,-1);
  if(e.Pocket+1<d.Pockets){SyncEncounterF();e.Pocket++;if(e.ProfileO!=null)e.Wave=0;e.Generated=false;e.Targets.Clear();EnterGrid("f:"+e.Id,false);return "";}
  e.State="Completed";CompleteP(e);CompleteSiteN(e);CompleteAdvancedH(e);JournalF("Completed "+d.Name);UtilitySites.FirstOrDefault(s=>s.Id=="f:"+e.Id)!.Completed=true;
  if(d.Id=="expedition"&& !Exploration.Encounters.Any(c=>c.Parent==e.Id)){var destination=ResolveSystem43(SystemId).Links.FirstOrDefault();if(destination!=null){AddEncounterF("blitz",destination,true,e.Id);JournalF("Expedition route / "+destination+" / 180-minute expiry");}}
  if(d.Kind=="Abyss"){EndAbyssO(e,false);EnterGrid(e.ReturnGrid,false);SolarOrigin=e.ReturnOrigin;SolarGridKey=SolarCatalog.GridKey(SystemId,SolarOrigin);Player.Position=e.ReturnPosition;Player.EnvironmentF="";}return "";
 }
 public string ClaimEncounterF(string id)
 {var e=Exploration.Encounters.FirstOrDefault(e=>e.Id==id);if(e==null||e.State!="Completed"||e.Claimed||!Docked||StationKey44!=e.OriginStation)return "Completed, unclaimed operation at its issuing station required.";e.Claimed=true;SettleCrewH(e);WalletChange(e.Reward,"Encounter / "+(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template)).Name);JournalF("Reward settled once / "+e.Id);return "";}
 public float HarvestEncounterF(SpaceTarget target,float volume)
 {var e=ActiveEncounterF;if(e==null||!target.Id.Contains(":ore:")||e.State!="Active")return 0;bool siteOreP=e.RunP!=null&&CatalogP.Get(e.Template)?.Mode=="Mine";float take=Math.Min(volume,Math.Min(target.Ore,CargoRoom(siteOreP?"p_ore":"ore")));target.Ore-=take;e.Harvested+=take;if(siteOreP)ChangeStack("p_ore",false,(decimal)take);else Player.Ore+=take;EvaluateEncounterF(e);SyncEncounterF();return take;}
 public void FailEncounterF(EncounterF e,string reason){if(e.State is "Completed" or "Failed" or "Expired")return;e.State="Failed";e.Failure=reason;if(e.Template=="ghost"&&Grid=="f:"+e.Id&&Targets.Any(t=>t.Kind==TargetKind.Container&&Vector3.Distance(Player.Position,t.Position)<10000))DealPlayerDamage(800,DamageType.Thermal);foreach(var t in Targets.Concat(e.Targets).Where(t=>t.Id.StartsWith("f:"+e.Id+":archive:"))){t.Destroyed=true;t.Contents.Clear();}if(e.FilamentO>0&&Grid!="f:"+e.Id)EndAbyssO(e,false);JournalF("Failed / "+(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template)).Name+" / "+reason);}
 public void AdvanceExplorationF(double seconds)
 {
  TickSystemsQ(seconds);TickP(seconds);TickWormholesO();TickEncountersO(seconds);TickSitesN(seconds);var active=ActiveEncounterF;foreach(var e in Exploration.Encounters.Where(e=>e.State is "Available" or "Active").ToArray())if(e.Expires<=Now){if(e==active&&e.Template=="abyss"){FailAbyssF(e,"Twenty-minute timeout");return;}FailEncounterF(e,"Deadline elapsed");e.State="Expired";foreach(var t in e.Targets.Where(t=>t.Kind==TargetKind.Container)){t.Destroyed=true;t.Contents.Clear();}JournalF("Encounter expired / "+e.Id);}
  foreach(var h in Exploration.Connections.Where(h=>!h.Collapsed&&h.Expires<=Now)){h.Collapsed=true;JournalF("Connection expired / "+h.Id);}
  if(active==null||active.State!="Active"||seconds<=0)return;EvaluateEncounterF(active);var def=EncounterCatalogF.Get(active.Template);
  if(def.Trigger=="Timer"&&Now>=active.Started.AddSeconds(30)&&active.Events.Add("timed-wave"))SpawnEncounterEnemyF(active,"timed reinforcement",new(7500,0,2000));
  if(active.ProfileN==null&&def.Kind=="Cache"&&!active.Events.Contains("hack:"+active.Pocket))DealPlayerDamage((float)Math.Min(100,seconds*4),DamageType.Thermal);
  if(def.Kind=="Abyss"&&Player.Position.Length()>50000){DealPlayerDamage((float)Math.Min(1000,seconds*100),DamageType.EM);if(Player.Hull<=0)FailAbyssF(active,"Boundary destruction");}
 }
 public string BuyFilamentF()
 {if(!Docked||Balance<200)return "Dock with 200 credits.";int remaining=Exploration.Supplies.GetValueOrDefault(StationKey44,5);if(remaining<=0)return "Finite specialist filament stock exhausted.";WalletChange(-200,"Filament acquisition");Hangar["f_filament"]=Hangar.GetValueOrDefault("f_filament")+1;Exploration.Supplies[StationKey44]=remaining-1;return "";}
 public string StartAbyssF(string weather,int tier=0)
 {
  if(Docked||InWarp||InAbyssF||Commander.Capsule||!CruiserEligibleF||tier<0||tier>6||weather is not("Dark" or "Electrical" or "Exotic" or "Firestorm" or "Gamma")||CargoItems.GetValueOrDefault("f_filament")<1)return "Undock a cruiser with a filament in cargo; select a weather and tier 0–6.";
  CargoItems["f_filament"]--;var e=AddEncounterF("abyss",SystemId,true);e.Expires=Now.AddMinutes(20);e.Tier=tier;e.Weather=weather;e.ReturnGrid=Grid;e.ReturnOrigin=SolarOrigin;e.ReturnPosition=Player.Position;EnterEncounterF("f:"+e.Id);return "";
 }
 public void FailAbyssF(EncounterF e,string reason)
 {FailEncounterF(e,reason);EndAbyssO(e,true);Player.EnvironmentF="";LosePlayerD();if(Commander.Capsule)LosePlayerD();JournalF("Abyss ship and capsule lost / "+reason);}
 public void EncounterShipLossF(){var e=ActiveEncounterF;if(e!=null&&e.State=="Active"){if(e.Template=="abyss")FailAbyssF(e,"Ship destruction");else FailEncounterF(e,"Player ship lost");}}
 public ConnectionF AddConnectionF(string a,string b)
 {var h=new ConnectionF{A=a,B=b,Expires=Now.AddHours(16)};Exploration.Connections.Add(h);RegisterConnectionF(h);return h;}
 public void RegisterConnectionF(ConnectionF h)
 {foreach(string system in h.SourceTypeO>0&&!h.BackVisibleO?new[]{h.A}:new[]{h.A,h.B})if(!UtilitySites.Any(s=>s.Id=="wf:"+h.Id&&s.System==system))UtilitySites.Add(new(){Id="wf:"+h.Id,System=system,Kind="Wormhole",Name=h.SourceTypeO>0?"Unstable wormhole":"Authored paired wormhole",X=.03f,Y=.01f,Z=.04f,Difficulty=2});}
 public bool EnterConnectionGridF(string id)
 {var h=Exploration.Connections.FirstOrDefault(h=>"wf:"+h.Id==id&&!h.Collapsed&&h.Expires>Now&&(h.A==SystemId||h.B==SystemId));if(h==null||!ConnectionAvailableO(h)||h.SourceTypeO>0&&!UtilitySites.Any(s=>s.Id==id&&ProbeWarpable(s)))return false;SyncEncounterF();EnterGrid("wf:"+h.Id,false);Targets.Clear();Targets.Add(new(){Id=id,Name="Paired wormhole / "+ConnectionCodeO(h,SystemId),Kind=TargetKind.Wormhole,Position=new(2000,0,0),Signature=2000});return true;}
 public bool TransitConnectionF()
 {
  var h=Exploration.Connections.FirstOrDefault(h=>"wf:"+h.Id==SelectedId&&!h.Collapsed&&(h.A==SystemId||h.B==SystemId));if(h==null||h.RemainingMass<=0||h.Expires<=Now||Docked||Selected==null||Vector3.Distance(Player.Position,Selected.Position)>2500||Cloaked||InAbyssF)return false;
  if(!ConnectionAvailableO(h))return false;double mass=Player.Stat("mass");if(mass<=0||mass>h.MaxShipMass)return false;var history=h.Transits.GetValueOrDefault(Player.InstanceId)??[];history.RemoveAll(t=>t<=Now.AddMinutes(-5));if(history.Count>=2){Say("Five-minute polarization prevents a third transit.");return false;}
  SyncEncounterF();RecoverProbesForTransit();history.Add(Now);h.Transits[Player.InstanceId]=history;h.RemainingMass=Math.Max(0,h.RemainingMass-mass);h.BackVisibleO=true;RegisterConnectionF(h);SystemId=h.A==SystemId?h.B:h.A;if(h.RemainingMass<=0)h.Collapsed=true;EnterGrid("wf:"+h.Id,false);Targets.Clear();if(!h.Collapsed)Targets.Add(new(){Id="wf:"+h.Id,Name="Return wormhole",Kind=TargetKind.Wormhole,Position=new(2000,0,0),Signature=2000});Player.Position=new(2400,0,0);EnsureExplorationF();if(!Exploration.Encounters.Any(e=>e.Template=="sleeper1"&&e.System==SystemId))AddEncounterF("sleeper1",SystemId,true);JournalF("Wormhole transit / "+h.Id+" / "+(h.SourceTypeO>0?DescribeConnectionO(h):"mass remaining "+h.RemainingMass));return true;
 }
 public string RequestSurveyBatchF(){if(!Docked)return "Dock to request another survey batch.";int count=0;foreach(string template in new[]{"outpost","archive","relic","career","mine","escort","cache","expedition","ghost"}){if(Exploration.Encounters.Any(e=>e.System==SystemId&&e.Template==template&&(e.State is "Active" or "Available"||e.Expires>Now)))continue;AddEncounterF(template,SystemId,template is "outpost" or "career" or "mine" or "escort");count++;}return count>0?"Issued "+count+" new site instances; old losses and loot remain settled.":"Existing survey instances must finish their original availability periods.";}
 public bool CruiserEligibleF=>Player.CombatHull=="cruiser"||(FleetCatalog.ById.TryGetValue(Player.CombatHull,out var hull)&&hull.Class=="Cruiser")||CombatHulls.Get(Player.CombatHull).Role is "ForceRecon" or "CombatRecon" or "HeavyAssault" or "HeavyInterdictor" or "StrategicCruiser";
 public void BindConnectionF(){Targets.Clear();var h=Exploration.Connections.FirstOrDefault(h=>Grid=="wf:"+h.Id&&!h.Collapsed&&h.Expires>Now&&(h.A==SystemId||h.B==SystemId));if(h!=null)Targets.Add(new(){Id="wf:"+h.Id,Name="Paired wormhole / "+ConnectionCodeO(h,SystemId),Kind=TargetKind.Wormhole,Position=new(2000,0,0),Signature=2000});}
 void ConfigureHackF(SpaceTarget target,HackNode[] nodes){if(!target.Id.StartsWith("f:")||ActiveEncounterF is not {} e)return;int level=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template)).Level;for(int i=1;i<49;i++){double roll=RollProductionE();nodes[i]=roll<.12+.03*level?new(){Kind=roll<.1?"Antivirus":"Firewall",Hp=20+5*level,Strength=4+level}:roll<.35?new(){Kind="Cache",Tool=new[]{"Repair","Rot","Shield","Vector"}[(int)(RollProductionE()*4)]}:new();}int core=Enumerable.Range(1,48).Where(i=>i%7+i/7>=6).OrderBy(_=>RollProductionE()).First();nodes[core]=new(){Kind="Core",Hp=35+level*10,Strength=5+level};}
 public ArcF? OfferArcF()
 {if(!Docked||Exploration.Arcs.Any(a=>a.Station==StationKey44&&(a.State is "Offered" or "Active"||a.RepeatAfter>Now)))return null;var a=new ArcF{Station=StationKey44,Expires=Now.AddDays(7)};Exploration.Arcs.Add(a);return a;}
 public string ChooseArcF(string id,string branch)
 {var a=Exploration.Arcs.FirstOrDefault(a=>a.Id==id);if(a==null||a.State!="Offered"||a.Expires<=Now||!Docked||a.Station!=StationKey44||branch is not("Archive" or "Escort"))return "Local open arc and Archive or Escort branch required.";a.Branch=branch;a.State="Active";var e=AddEncounterF(branch=="Archive"?"career":"escort",SystemId,true);a.Encounter=e.Id;return "";}
 public string ClaimArcF(string id)
 {var a=Exploration.Arcs.FirstOrDefault(a=>a.Id==id);var e=Exploration.Encounters.FirstOrDefault(e=>e.Id==a?.Encounter);if(a==null||e?.State!="Completed"||a.State!="Active"||a.Claimed||!Docked||a.Station!=StationKey44||a.Expires<=Now)return "Complete the chosen branch and return to its contact.";a.Claimed=true;a.State="Completed";a.RepeatAfter=Now.AddDays(7);Hangar["e_relic"]=Hangar.GetValueOrDefault("e_relic")+1;JournalF("Independent reconnaissance arc / "+a.Branch+" / relic settled");return "";}
 public string LocatePilotF(string id)
 {var p=Population.Pilots.FirstOrDefault(p=>p.Id==id);if(!Docked||p==null||Balance<20)return "Dock with 20 credits and select a known pilot.";WalletChange(-20,"Local contact intelligence");JournalF("Known contact / "+p.Name+" / "+p.System+" / observed "+Now.ToString("u"));return p.System;}
 public static void ValidateExplorationF(ExplorationStateF? state)
 {if(state==null)return;ValidateP(state);ValidateStateO(state);if(state.Encounters.Any(e=>e.ProfileN is {} p&&(p.Id!=e.Template||p.Version<1||p.Groups<0||p.Groups>20||p.Pockets<1||p.Pockets>8||p.Containers<1||p.Containers>16||p.Rating<1||p.Rating>10)))throw new InvalidDataException("Invalid Stage N profile snapshot.");if(state.Encounters.Select(e=>e.Id).Distinct().Count()!=state.Encounters.Count||state.Connections.Select(e=>e.Id).Distinct().Count()!=state.Connections.Count||state.Encounters.Any(e=>!EncounterCatalogF.All.Concat(AdvancedCatalogH.Encounters).Concat(SiteCatalogN.All.Select(p=>p.Definition)).Concat(EncounterCatalogO.All.Select(p=>p.Definition)).Concat(CatalogP.All.Select(p=>p.Definition)).Any(d=>d.Id==e.Template)||e.Pocket<0||e.Pocket>=(e.ProfileO?.Definition??e.ProfileN?.Definition??EncounterCatalogF.Get(e.Template)).Pockets||e.Reward<0||e.Harvested<0||e.State is not("Available" or "Active" or "Completed" or "Failed" or "Expired")||e.Tier<0||e.Tier>6||e.Targets.Any(t=>t.Contents.Values.Any(q=>q<0)||!float.IsFinite(t.Hull)))||state.Connections.Any(h=>h.A==h.B||h.RemainingMass<0||h.MaxShipMass<=0)||state.Supplies.Values.Any(v=>v<0||v>5))throw new InvalidDataException("Invalid persistent exploration state.");}
}
public sealed partial class Ship
{
 [System.Text.Json.Serialization.JsonIgnore]public string EnvironmentF{get;set;}="";
 public double EnvironmentStatF(string key,double value)
 {
  value=StatO(key,value);if(EnvironmentF=="")return value;
  string resonance=EnvironmentF switch{"Electrical"=>"Em","Exotic"=>"Kinetic","Firestorm"=>"Thermal","Gamma"=>"Explosive",_=>""};
  if(resonance!=""&&key.Contains(resonance+"DamageResonance"))return Math.Min(1,value*(1+WeatherPenaltyO));
  if(EnvironmentF=="Electrical"&&key=="rechargeRate")return value*.5;
  if(EnvironmentF=="Exotic"&&key=="scanResolution")return value*1.5;
  if(EnvironmentF=="Firestorm"&&key=="armorHP"||EnvironmentF=="Gamma"&&key=="shieldCapacity")return value*1.5;
  if(EnvironmentF=="Dark"&&(key=="turretRange"||key=="turretFalloff"))return value*.5;
  if(EnvironmentF=="Dark"&&key=="maxVelocity")return value*1.5;
  return value;
 }
}
