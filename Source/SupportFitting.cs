namespace StarlightSyndicateDemo;
public sealed class SupportStored
{public SupportState State{get;set;}=new();public bool Hangar{get;set;}public string System{get;set;}="";}
public sealed partial class DemoWorld
{
 public int FittingSupportSlot{get;set;}
 public List<SupportStored> SupportStorage{get;set;}=[];
 public int SupportHighCount=>Player.Support.Count(s=>s.Definition?.Slot=="high");public int SupportMidCount=>Player.Support.Count(s=>s.Definition?.Slot=="medium");
 public static string SupportSlot(int slot)=>slot<6?"high":"medium";
 public int BurstLimit=>ShipBatch21.ById.TryGetValue(Player.CombatHull,out var bp)&&bp.Batch==28?(int)Player.HullFlat("hull","commandSlots")+(int)Player.RigAdd("maxGangModulesAdd"):Player.Specialist?(int)Player.HullFlat("hull","commandSlots")+(int)Player.RigAdd("maxGangModulesAdd"):(int)Player.HullFlat("hull","commandSlots")+(int)Player.RigAdd("maxGangModulesAdd")+(CombatHulls.Get(Player.CombatHull).Role switch{"Command"=>3,"ExpeditionCommand"=>2,"CapitalIndustrial"=>3,"Carrier" or "Supercarrier" or "Titan"=>2,_=>0});
 public float SupportGrid(SupportDefinition d)=>Player.RigPower(d.Id,d.Power)*(!Player.Specialist&&!(ShipBatch21.ById.TryGetValue(Player.CombatHull,out var gp)&&gp.Batch==28)&&d.Kind=="Burst"&&CombatHulls.Get(Player.CombatHull).Role is "Command" or "ExpeditionCommand"?.05f:1);
 public double SupportFittedPower=>Player.Support.Where(s=>s.Online&&s.Definition!=null).Sum(s=>SupportGrid(s.Definition!));
 public string SupportFitError(SupportDefinition d,int slot)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(slot<0||slot>=8||SupportSlot(slot)!=d.Slot)return "Choose a "+d.Slot+" support socket.";
  var h=CombatHulls.Get(Player.CombatHull);if(d.Size==4&&h.Size<4)return "Capital module requires a capital hull.";
  if(d.Role.Length>0&&h.Role!=d.Role)return "Requires an adaptive logistics hull.";
  var other=Player.Support.Where((s,i)=>i!=slot&&s.Definition!=null).ToArray();
  if(d.Kind=="Burst"&&(BurstLimit==0||other.Count(s=>s.Definition!.Kind=="Burst")>=BurstLimit))return "Command bursts exceed this hull's command limit.";
  if(d.MaxGroup==1&&other.Any(s=>s.Definition!.Kind==d.Kind))return "Only one "+d.Family+" module per ship.";
  int count=SpecialCount(d.Slot)+UtilityCount(d.Slot)+MobilityCount(d.Slot)+EnhancementCount(d.Slot)+other.Count(s=>s.Definition!.Slot==d.Slot)+1;
  if(d.Slot=="high")count+=DefenseHighCount+(Player.Ewar[0].Definition!=null?1:0)+Player.WeaponSocketCount;
  else count+=DefenseMidCount+Player.Ewar.Skip(1).Count(s=>s.Definition!=null);
  if(count>Player.Stat(d.Slot=="high"?"hiSlots":"medSlots"))return "Shared hull sockets exceeded.";
  var weapons=Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null).Select(w=>w!.Definition).ToArray();
  if(Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.EnhancementCpu+Player.DefenseCpu+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigCpu(s.Definition.Id,s.Definition.Cpu)))+weapons.Sum(w=>Player.RigCpu(w.Id,w.Cpu))+other.Sum(s=>Player.RigCpu(s.Definition!.Id,s.Definition.Cpu))+Player.RigCpu(d.Id,d.Cpu)>Player.Stat("cpuOutput")||Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+Player.EnhancementPower+Player.DefensePower+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigPower(s.Definition.Id,s.Definition.Power)))+weapons.Sum(w=>Player.RigPower(w.Id,w.Power))+other.Sum(s=>SupportGrid(s.Definition!))+SupportGrid(d)>Player.Stat("powerOutput"))return "CPU or power grid exceeded.";
  return "";
 }
 public bool FitSupport(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked){Say("Dock before fitting support equipment.");return false;}
  if(!SupportCatalog.ById.TryGetValue(id,out var d)||ItemCount(id,hangar)<1)return false;
  var error=SupportFitError(d,slot);if(error.Length>0){Say(error);return false;}
  UnfitSupport(slot);ChangeStack(id,hangar,-(decimal)(1));
  var stored=SupportStorage.FirstOrDefault(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));Player.Support[slot]=stored?.State??SupportState.Create(id);if(stored!=null)SupportStorage.Remove(stored);
  RecomputeWeaponFit();Say("Fitted "+d.Name+".");return true;
 }
 public bool UnfitSupport(int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||slot<0||slot>=8)return false;var s=Player.Support[slot];if(s.Definition==null)return true;
  ReturnSupportPayload(s,true);ChangeStack(s.ModuleId,true,(decimal)(1));if(s.ModuleId.Length>0)SupportStorage.Add(new(){State=s,Hangar=true,System=StationKey44});Player.Support[slot]=new();RecomputeWeaponFit();return true;
 }
 void ReturnSupportPayload(SupportState s,bool hangar)
 {if(s.Loaded>0&&s.Payload!=null)ChangeStack(s.PayloadId,hangar,(decimal)(s.Loaded));s.Loaded=0;s.PayloadId="";s.Active=false;s.CycleLeft=0;s.ReloadLeft=0;s.PendingAmount=0;s.Ramp=0;s.TargetId="";s.PendingTarget="";}
 public bool LoadSupportPayload(int slot,string id)
 {
  if(slot<0||slot>=8)return false;var s=Player.Support[slot];var d=s.Definition;if(d==null||s.CycleLeft>0||s.ReloadLeft>0||s.Burnt)return false;
  var p=SupportPayloads.ById.GetValueOrDefault(id);if(id.Length>0&&(p==null||!p.Fits(d)||ItemCount(id,Docked)<1)){Say("No compatible support payload available here.");return false;}
  if(!Docked&&s.Loaded>0&&CargoUsed+s.Loaded*(s.Payload?.Volume??0)>Player.CargoMax+.001f){Say("No cargo room to return old magazine.");return false;}
  ReturnSupportPayload(s,Docked);if(p==null)return true;
  int max=p.Consumable?Math.Max(1,(int)Math.Floor((d.MagazineVolume+.000001f)/p.Volume)):1;
  s.PayloadId=id;s.Loaded=Math.Min(max,(int)ItemCount(id,Docked));ChangeStack(id,Docked,-(decimal)(s.Loaded));s.ReloadLeft=Docked?0:d.Reload;return s.Loaded>0;
 }
 public bool ReloadSupport(int slot)=>slot>=0&&slot<8&&Player.Support[slot].PayloadId.Length>0&&LoadSupportPayload(slot,Player.Support[slot].PayloadId);
 public void ProvisionSupportCargo(int slot)
 {if(!Docked||slot<0||slot>=8)return;var s=Player.Support[slot];var p=s.Payload;if(p==null)return;int q=Math.Min((int)ItemCount(p.Id,true),Math.Min(Math.Max(1,s.Loaded*2),(int)((Player.CargoMax-CargoUsed)/p.Volume)));if(q>0)TransferItem(p.Id,true,q);}
 public void ProvisionSupportTrial()
 {
  for(int i=0;i<8;i++)UnfitSupport(i);
  Hangar["salvager"]=Math.Max(1,Hangar.GetValueOrDefault("salvager"));Hangar["mining_focus"]=Math.Max(20,Hangar.GetValueOrDefault("mining_focus"));
  foreach(var d in SupportCatalog.All)Hangar[d.Id]=Math.Max(3,Hangar.GetValueOrDefault(d.Id));foreach(var p in SupportPayloads.All)Hangar[p.Id]=Math.Max(p.Consumable?(p.Id.StartsWith("lc_")?1000:200):3,Hangar.GetValueOrDefault(p.Id));
 }
 public void SupportTrial(string hull)
 {WeaponTrial(hull);for(int i=0;i<4;i++)UnfitEwar(i);ProvisionSupportTrial();Say("SUPPORT TRIAL: all logistics modules, scripts and pulse charges supplied. Hire wingmates and enter the fleet range.");}
 public bool RepairSupport(int slot)
 {if(!RequireService44(13))return false;if(!Docked||slot<0||slot>=8)return false;var s=Player.Support[slot];if(s.Definition==null)return false;float price=(float)s.Definition.Price*.12f*Math.Max(s.Damage,s.Burnt?1:0);if(Credits<price)return false;WalletChange(-(decimal)(price),"Gameplay transaction");s.Damage=0;s.Burnt=false;s.Overheat=false;return true;}
 public bool HasDamagedSupportStock(string id,bool hangar)=>SupportStorage.Any(x=>x.State.ModuleId==id&&(x.State.Damage>0||x.State.Burnt)&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));
 public void TransferSupportCondition(string id,bool hangar,int q)
 {foreach(var x in SupportStorage.Where(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44)).Take(q)){x.Hangar=!hangar;x.System=StationKey44;}}
 public bool RepairStoredSupport()
 {if(!RequireService44(13))return false;if(!Docked)return false;var all=SupportStorage.Where(x=>!x.Hangar||x.System==StationKey44).ToArray();float price=all.Sum(x=>(float)(x.State.Definition?.Price??0)*.12f*Math.Max(x.State.Damage,x.State.Burnt?1:0));if(Credits<price)return false;WalletChange(-(decimal)(price),"Gameplay transaction");foreach(var x in all){x.State.Damage=0;x.State.Burnt=false;x.State.Overheat=false;}return true;}
}
