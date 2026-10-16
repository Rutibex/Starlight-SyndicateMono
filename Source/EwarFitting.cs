namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public int FittingEwarSlot{get;set;}=1;
 public void ProvisionEwarTrial()
 {
  foreach(var e in Player.Ewar){ReturnEwarPayload(e);if(e.ModuleId.Length>0)Hangar[e.ModuleId]=Hangar.GetValueOrDefault(e.ModuleId)+1;}
  Player.Ewar=[new(),new(),new(),new()];
  foreach(var d in EwarCatalog.All)Hangar[d.Id]=Math.Max(1,Hangar.GetValueOrDefault(d.Id));
  foreach(var p in EwarPayloads.All)Hangar[p.Id]=Math.Max(p.Consumable?100:3,Hangar.GetValueOrDefault(p.Id));
 }
 public string EwarFitError(EwarDefinition e,int slot)
 {
  if(SkillError(e.Id) is {Length:>0} skillError)return skillError;
  if(slot<0||slot>3)return "Invalid EWAR socket.";
  if((slot==0)!=(e.Slot=="high"))return "Choose a "+e.Slot+"-power socket.";
  var h=CombatHulls.Get(Player.CombatHull);
  if(e.Size>h.Size)return "Module size exceeds the hull class.";
  if(e.Role.Length>0&&e.Role!=h.Role)return "Requires a "+e.Role+" hull.";
  if(e.Kind==EwarKind.Grapple&&Player.Ewar.Where((_,i)=>i!=slot).Any(s=>s.Definition?.Kind==EwarKind.Grapple))return "Only one grappler per ship.";
  if(e.Kind==EwarKind.BurstJam&&Player.Ewar.Where((_,i)=>i!=slot).Any(s=>s.Definition?.Kind==EwarKind.BurstJam))return "Only one sensor-burst jammer per ship.";
  if(slot>0&&UtilityCount("medium")+MobilityCount("medium")+EnhancementCount("medium")+SupportMidCount+DefenseMidCount+Player.Ewar.Where((s,i)=>i!=slot&&i>0&&s.Definition!=null).Count()+1>Player.Stat("medSlots"))return "Not enough medium sockets (shared defense rack).";
  if(slot==0&&SpecialCount("high")+UtilityCount("high")+EnhancementCount("high")+Player.WeaponSocketCount+SupportHighCount+DefenseHighCount+1>Player.Stat("hiSlots"))return "Not enough high-power sockets.";
  float cpu=Player.Ewar.Where((_,i)=>i!=slot).Sum(s=>(s.Definition==null?0:Player.RigCpu(s.Definition.Id,s.Definition.Cpu))),power=Player.Ewar.Where((_,i)=>i!=slot).Sum(s=>(s.Definition==null?0:Player.RigPower(s.Definition.Id,s.Definition.Power)));
  var weapons=Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null).Select(w=>w!.Definition).ToArray();
  if(Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.EnhancementCpu+Player.SupportCpu+Player.DefenseCpu+cpu+Player.RigCpu(e.Id,e.Cpu)+weapons.Sum(w=>Player.RigCpu(w.Id,w.Cpu))>Player.Stat("cpuOutput")||Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+Player.EnhancementPower+SupportFittedPower+Player.DefensePower+power+Player.RigPower(e.Id,e.Power)+weapons.Sum(w=>Player.RigPower(w.Id,w.Power))>Player.Stat("powerOutput"))return "CPU or power grid exceeded.";
  return "";
 }
 public bool FitEwar(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked){Say("Dock before fitting EWAR.");return false;}
  if(!EwarCatalog.ById.TryGetValue(id,out var def)||ItemCount(id,hangar)<1)return false;
  string error=EwarFitError(def,slot);if(error.Length>0){Say(error);return false;}
  var old=Player.Ewar[slot];ReturnEwarPayload(old);StoreEwarCondition(old);if(old.ModuleId.Length>0)ChangeStack(old.ModuleId,true,(decimal)(1));
  ChangeStack(id,hangar,-(decimal)(1));Player.Ewar[slot]=TakeEwarCondition(id,hangar);RecomputeWeaponFit();Say("Fitted "+def.Name+" to EW socket "+slot+".");return true;
 }
 public void UnfitEwar(int slot)
 {
  if(!Docked||slot<0||slot>3)return;var state=Player.Ewar[slot];ReturnEwarPayload(state);StoreEwarCondition(state);if(state.ModuleId.Length>0)ChangeStack(state.ModuleId,true,(decimal)(1));Player.Ewar[slot]=new();RecomputeWeaponFit();
 }
 void ReturnEwarPayload(EwarState state)
 {if(state.Loaded>0&&state.PayloadId.Length>0)ChangeStack(state.PayloadId,true,(decimal)(state.Loaded));state.Loaded=0;state.Stop();}
 public bool LoadEwarPayload(int slot,string payloadId)
 {
  if(slot<0||slot>3)return false;var s=Player.Ewar[slot];var e=s.Definition;
  if(e==null||s.ReloadLeft>0||s.WarmupLeft>0)return false;
  if(payloadId.Length==0)
  {
   if(!Docked&&s.Loaded>0&&CargoUsed+s.Loaded*ItemVolume(s.PayloadId)>Player.CargoMax)return false;
   if(s.Loaded>0)ChangeStack(s.PayloadId,Docked,(decimal)(s.Loaded));s.PayloadId="";s.Loaded=0;s.Stop();return true;
  }
  if(!EwarPayloads.ById.TryGetValue(payloadId,out var p)||!p.Fits.Contains(e.Kind)||ItemCount(payloadId,Docked)<1){Say("No compatible EWAR payload in this location.");return false;}
  if(!Docked&&s.Loaded>0&&CargoUsed+s.Loaded*ItemVolume(s.PayloadId)>Player.CargoMax){Say("Cargo cannot accept the old EWAR payload.");return false;}
  if(s.Loaded>0)ChangeStack(s.PayloadId,Docked,(decimal)(s.Loaded));
  int count=p.Consumable?Math.Min(Math.Max(1,e.Magazine),(int)ItemCount(payloadId,Docked)):1;
  ChangeStack(payloadId,Docked,-(decimal)(count));s.PayloadId=payloadId;s.Loaded=count;s.Stop();s.ReloadLeft=Docked?0:e.Reload;Say("Loaded "+p.Name+" x"+count+".");return true;
 }
 public void ProvisionEwarCargo(int slot)
 {
  if(!Docked||slot<0||slot>3)return;var s=Player.Ewar[slot];if(s.PayloadId.Length==0)return;
  int room=(int)Math.Floor(Math.Max(0,Player.CargoMax-CargoUsed)/ItemVolume(s.PayloadId));int count=Math.Min(room,Math.Min(20,(int)ItemCount(s.PayloadId,true)));if(count>0)TransferItem(s.PayloadId,true,count);
 }
}
