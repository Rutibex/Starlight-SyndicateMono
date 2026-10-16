using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed class FittingDesign
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Name{get;set;}="Untitled fit";public string Notes{get;set;}="";public DateTime SavedUtc{get;set;}=DateTime.UtcNow;public Ship Ship{get;set;}=new();
 public static FittingDesign Capture(Ship ship,string name,string notes="")=>new(){Name=name.Trim().Length==0?ship.HullName+" fit":name.Trim(),Notes=notes,Ship=FittingSimulation.Copy(ship)};
}
internal static class FittingDesignStore
{
 internal static readonly JsonSerializerOptions Json=new(){IncludeFields=true,IgnoreReadOnlyProperties=true,WriteIndented=true,NumberHandling=JsonNumberHandling.AllowNamedFloatingPointLiterals};
 internal static string DirectoryPath=>Path.Combine(Path.GetDirectoryName(DemoWorld.SavePath)!,"Fittings");
 internal static string PathName=>Path.Combine(DirectoryPath,"saved-fittings.json");
 internal static List<FittingDesign> Load(){if(!File.Exists(PathName))return [];var list=JsonSerializer.Deserialize<List<FittingDesign>>(File.ReadAllText(PathName),Json)??[];foreach(var d in list)Restore(d.Ship);return list;}
 internal static void Save(IEnumerable<FittingDesign> designs){Directory.CreateDirectory(DirectoryPath);File.WriteAllText(PathName+".tmp",JsonSerializer.Serialize(designs,Json));File.Move(PathName+".tmp",PathName,true);}
 internal static void Restore(Ship s){if(s.IsFleetHull&&!FleetCatalog.ById.ContainsKey(s.CombatHull))throw new InvalidDataException("Unknown hull");s.RestoreStats(new(s.Attributes));}
 internal static string Export(FittingDesign d)=>JsonSerializer.Serialize(d,Json);
 internal static FittingDesign Import(string text){var d=JsonSerializer.Deserialize<FittingDesign>(text,Json)??throw new InvalidDataException("Empty fitting");if(d.Name.Length>120||d.Notes.Length>4000)throw new InvalidDataException("Fitting name/notes too long");Restore(d.Ship);foreach(var m in FittingLayoutModel.Installed(d.Ship))if(!FittingLayoutModel.ById.ContainsKey(m.Id))throw new InvalidDataException("Unknown module "+m.Id);d.Id=Guid.NewGuid().ToString("N");return d;}
 internal static Dictionary<string,int> Equipment(Ship ship){var counts=new Dictionary<string,int>();void Add(string id,int n){if(id.Length>0&&n>0)counts[id]=counts.GetValueOrDefault(id)+n;}
 foreach(var id in ship.StrategicSubsystems.Values)Add(id,1);foreach(var m in FittingLayoutModel.Installed(ship)){Add(m.Id,1);Add(m.Payload,m.Loaded>0?m.Loaded:m.Kind=="enhancement"&&m.Payload.Length>0?1:0);}foreach(var u in ship.Drones.Where(u=>u.Owner=="player"&&u.Location is "Bay" or "Reserve"))Add(u.TypeId,u.Count);return counts;}
 internal static bool Unfit(DemoWorld w,FittingModule m)=>m.Kind switch{"weapon" or "legacy"=>w.UnfitLayoutWeapon(m.Index),"ewar"=>UnfitEwar(w,m.Index),"defense"=>w.UnfitDefense(m.Index),"support"=>w.UnfitSupport(m.Index),"enhancement"=>w.UnfitEnhancement(m.Index),"mobility"=>w.UnfitMobility(m.Index),"utility"=>w.UnfitUtility(m.Index),"special"=>w.UnfitSpecial(m.Index),_=>false};
 static bool UnfitEwar(DemoWorld w,int i){w.UnfitEwar(i);return w.Player.Ewar[i].Definition==null;}
 internal static bool Strip(DemoWorld w){for(int pass=0;pass<8;pass++){var modules=FittingLayoutModel.Installed(w.Player).Where(m=>m.Kind!="rig").ToArray();if(modules.Length==0)return true;bool changed=false;foreach(var m in modules)changed|=Unfit(w,m);if(!changed)return false;}return false;}
 internal static string Apply(DemoWorld live,FittingDesign design,bool destroyRigs=false)
 {
  if(!live.ServiceAllowed44(17))return "Fitting service unavailable here.";if(!live.Docked)return "Dock before fitting a saved design.";
  if(live.Player.CombatHull!=design.Ship.CombatHull||live.Player.IsFleetHull!=design.Ship.IsFleetHull)return "Board the matching hull before Fit Ship.";
  if(live.Player.Drones.Any(u=>u.Owner=="player"&&u.Location is "Space" or "Abandoned"))return "Recover deployed drones before fitting a design.";
  if(FittingLayoutModel.Installed(live.Player).Any(m=>m.Active))return "Stop active module cycles before Fit Ship.";
  var desired=FittingSimulation.Copy(design.Ship);
  foreach(string id in Equipment(desired).Keys.Append(desired.CombatHull)){string error=live.SkillError(id);if(error.Length>0)return DemoWorld.ItemName(id)+": "+error;}var changedRigSlots=Enumerable.Range(0,live.Player.Rigs.Length).Where(i=>live.Player.Rigs[i]!=desired.Rigs.ElementAtOrDefault(i)).ToHashSet();
  if(changedRigSlots.Any(i=>live.Player.Rigs[i].Length>0)&&!destroyRigs)return "RIG CONFIRMATION: changing installed rigs destroys them permanently.";
  // Native operations run on a detached transaction. Failed validation never touches the campaign.
  var stage=JsonSerializer.Deserialize<DemoWorld>(JsonSerializer.Serialize(live,Json),Json)!;stage.Player=FittingSimulation.Copy(live.Player);stage.SyncSkills();
  if(!Strip(stage))return "Removal blocked by a cycle, fitting dependency or occupied cargo bay.";
  foreach(var u in stage.Player.Drones.Where(u=>u.Owner=="player"&&u.Location is "Bay" or "Reserve").ToArray())if(!stage.UnloadDrone(u.Id))return "Cannot unload current drone bay.";
  foreach(int i in changedRigSlots)if(stage.Player.Rigs[i].Length>0&&!stage.RemoveRig(i,true))return "Cannot remove dependent rig.";
  foreach(var (category,id) in desired.StrategicSubsystems){if(stage.Player.StrategicSubsystems.GetValueOrDefault(category)==id)continue;if(stage.ItemCount(id,true)<1)return "Missing subsystem: "+DemoWorld.ItemName(id);if(!stage.FitStrategicSubsystem(id))return "Subsystem rejected: "+DemoWorld.ItemName(id);}

  var modules=FittingLayoutModel.Installed(desired).Where(m=>m.Kind!="rig"||changedRigSlots.Contains(m.Index)).OrderBy(m=>m.Kind=="rig"?0:m.Kind=="mobility"?1:2).ToList();
  for(int pass=0,limit=modules.Count+8;pass<limit&&modules.Count>0;pass++){
   bool progress=false;foreach(var m in modules.ToArray()){
    var spec=FittingLayoutModel.ById[m.Id];if(stage.ItemCount(m.Id,true)<1)continue;
    if(m.Online==false){var attrs=new Dictionary<string,double?>(stage.Player.Attributes);try{stage.Player.SetStat("cpuOutput",1000000000);stage.Player.SetStat("powerOutput",1000000000);if(FittingLayoutModel.Error(stage,spec,m.Index).Length>0||!FittingLayoutModel.Fit(stage,spec,true,m.Index))continue;}finally{stage.Player.RestoreStats(attrs);}switch(m.Kind){case "weapon":stage.Player.Weapons[m.Index].Online=false;break;case "defense":stage.Player.Defense[m.Index].Online=false;break;case "support":stage.Player.Support[m.Index].Online=false;break;case "ewar":stage.Player.Ewar[m.Index].Online=false;break;case "mobility":stage.Player.Mobility[m.Index].Online=false;break;case "utility":stage.Player.Utilities[m.Index].Online=false;break;case "enhancement":stage.Player.Enhancements[m.Index].Online=false;break;case "special":stage.Player.Special[m.Index].Online=false;break;}}
    else {if(FittingLayoutModel.Error(stage,spec,m.Index).Length>0)continue;if(!FittingLayoutModel.Fit(stage,spec,true,m.Index))continue;}
    modules.Remove(m);progress=true;
   }if(!progress)break;
  }
  if(modules.Count>0)return "Fit blocked: "+DemoWorld.ItemName(modules[0].Id)+". Check stock, sockets, hull restrictions and CPU/grid.";
  foreach(var m in FittingLayoutModel.Installed(desired).Where(m=>m.Payload.Length>0&&(m.Loaded>0||m.Kind=="enhancement"))){int n=m.Kind=="enhancement"?1:m.Loaded;if(stage.ItemCount(m.Payload,true)<n)return "Missing charge/script: "+DemoWorld.ItemName(m.Payload);
   decimal spare=stage.Hangar.GetValueOrDefault(m.Payload)-n;stage.Hangar[m.Payload]=n;
   bool ok=m.Kind switch{"weapon"=>stage.LoadWeapon(m.Index,m.Payload),"ewar"=>stage.LoadEwarPayload(m.Index,m.Payload),"defense"=>stage.LoadDefensePayload(m.Index,m.Payload),"support"=>stage.LoadSupportPayload(m.Index,m.Payload),"enhancement"=>stage.LoadEnhancementScript(m.Index,m.Payload),"utility"=>stage.LoadUtilityPayload(m.Index,m.Payload,true),_=>false};stage.Hangar[m.Payload]+=spare;if(!ok)return "Charge rejected: "+DemoWorld.ItemName(m.Payload);
  }
  foreach(var m in FittingLayoutModel.Installed(desired).Where(m=>m.Online==false)){bool changed=m.Kind switch{"weapon"=>stage.SetWeaponOnline(m.Index,false),"defense" or "support" or "ewar"=>stage.SetBasicModuleOnlineB(m,false),"enhancement"=>stage.SetEnhancementOnline(m.Index,false),"mobility"=>stage.SetMobilityOnline(m.Index,false),"utility"=>stage.SetUtilityOnline(m.Index,false),"special"=>stage.SetSpecialOnline(m.Index,false),_=>true};if(!changed)return "Cannot preserve requested offline state.";}
  foreach(var u in desired.Drones.Where(u=>u.Owner=="player"&&u.Location is "Bay" or "Reserve")){bool ok=u.Location=="Reserve"&&u.Definition.Fighter?stage.StoreFighterReserve(u.TypeId,u.Count):stage.LoadDrone(u.TypeId,true,u.Count,u.Tube);if(!ok)return "Drone bay blocked or missing stock: "+u.Definition.Name;}
  if(stage.TotalFitCpu>stage.Player.Stat("cpuOutput")+.001||stage.TotalFitPower>stage.Player.Stat("powerOutput")+.001)return "Design exceeds online CPU or power budget.";
  stage.Player.Shield=Math.Min(live.Player.Shield,stage.Player.MaxShield);stage.Player.Armor=Math.Min(live.Player.Armor,stage.Player.MaxArmor);stage.Player.Hull=Math.Min(live.Player.Hull,stage.Player.MaxHull);stage.Player.Cap=Math.Min(live.Player.Cap,stage.Player.MaxCap);
  // Commit only ship fitting and the native storage changes; unrelated world state stays live.
  live.Player=stage.Player;live.CargoItems=stage.CargoItems;live.StationItems=stage.StationItems;
  live.EwarStorage=stage.EwarStorage;live.DefenseStorage=stage.DefenseStorage;live.SupportStorage=stage.SupportStorage;live.EnhancementStorage=stage.EnhancementStorage;live.MobilityStorage=stage.MobilityStorage;live.UtilityStorage=stage.UtilityStorage;live.SpecialStorage=stage.SpecialStorage;live.CrystalConditions=stage.CrystalConditions;live.Day=stage.Day;live.SimulationUtc=stage.SimulationUtc;live.WalletFunds=stage.WalletFunds;live.WalletJournal=stage.WalletJournal;live.MarketOrders=stage.MarketOrders;live.MarketTrades=stage.MarketTrades;live.Industry=stage.Industry;live.Skills=stage.Skills;live.AgentsState=stage.AgentsState;live.SeededMarkets=stage.SeededMarkets;live.SyncSkills();
  live.RefreshSavedFitting();live.Say("Fitted saved design: "+design.Name);return "";
 }
}

public sealed partial class DemoWorld {internal void RefreshSavedFitting()=>RecomputeWeaponFit();}
