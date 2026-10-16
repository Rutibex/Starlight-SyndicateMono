namespace StarlightSyndicateDemo;
public sealed class EnhancementStored{public EnhancementState State{get;set;}=new();public bool Hangar{get;set;}public string System{get;set;}="";}
public sealed partial class DemoWorld
{
 public int FittingEnhancementSlot{get;set;}=8;public List<EnhancementStored> EnhancementStorage{get;set;}=[];
 public int EnhancementCount(string slot)=>Player.Enhancements.Count(s=>s.Definition?.Slot==slot);
 public string EnhancementFitError(EnhancementDefinition d,int slot)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(slot<0||slot>=14||Ship.EnhancementSlot(slot)!=d.Slot)return "Choose a "+d.Slot+" socket.";
  if(d.Role=="FighterHull"&&CombatHulls.Get(Player.CombatHull).Role is not ("Carrier" or "Supercarrier"))return "Requires a fighter carrier hull.";
  var others=Player.Enhancements.Where((s,i)=>i!=slot&&s.Definition!=null).ToArray();int count=SpecialCount(d.Slot)+UtilityCount(d.Slot)+MobilityCount(d.Slot)+others.Count(s=>s.Definition!.Slot==d.Slot)+1+Player.Defense.Count(s=>s.Definition?.Slot==d.Slot)+Player.Support.Count(s=>s.Definition?.Slot==d.Slot);
  count+=d.Slot=="high"?Player.WeaponSocketCount+(Player.Ewar[0].Definition!=null?1:0):d.Slot=="medium"?Player.Ewar.Skip(1).Count(s=>s.Definition!=null):0;
  if(count>Player.Stat(d.Slot=="high"?"hiSlots":d.Slot=="medium"?"medSlots":"lowSlots"))return "Shared hull sockets exceeded.";
  var weapons=Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null).Select(w=>w!.Definition).ToArray();
  if(Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.SupportCpu+Player.DefenseCpu+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigCpu(s.Definition.Id,s.Definition.Cpu)))+weapons.Sum(w=>Player.RigCpu(w.Id,w.Cpu))+others.Where(s=>s.Online).Sum(s=>Player.RigCpu(s.Definition!.Id,s.Definition.Cpu))+Player.RigCpu(d.Id,d.Cpu)>Player.Stat("cpuOutput")||Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+SupportFittedPower+Player.DefensePower+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigPower(s.Definition.Id,s.Definition.Power)))+weapons.Sum(w=>Player.RigPower(w.Id,w.Power))+others.Where(s=>s.Online).Sum(s=>Player.RigPower(s.Definition!.Id,s.Definition.Power))+Player.RigPower(d.Id,d.Power)>Player.Stat("powerOutput"))return "CPU or power grid exceeded.";
  return "";
 }
 public bool FitEnhancement(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||!EnhancementCatalog.ById.TryGetValue(id,out var d)||ItemCount(id,hangar)<1)return false;string error=EnhancementFitError(d,slot);if(error.Length>0){Say(error);return false;}UnfitEnhancement(slot);ChangeStack(id,hangar,-(decimal)(1));
  var stored=EnhancementStorage.FirstOrDefault(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));Player.Enhancements[slot]=stored?.State??new(){ModuleId=id};if(stored!=null)EnhancementStorage.Remove(stored);RefreshEnhancedFighters();RecomputeWeaponFit();Say("Fitted "+d.Name+".");return true;
 }
 public bool UnfitEnhancement(int slot)
 {if(!RequireService44(17))return false;if(!Docked||slot<0||slot>=14)return false;var s=Player.Enhancements[slot];if(s.Definition==null)return true;ReturnEnhancementScript(s,true);ChangeStack(s.ModuleId,true,(decimal)(1));StopEnhancement(s);if(s.ModuleId.Length>0)EnhancementStorage.Add(new(){State=s,Hangar=true,System=StationKey44});Player.Enhancements[slot]=new();RefreshEnhancedFighters();RecomputeWeaponFit();return true;}
 void ReturnEnhancementScript(EnhancementState s,bool hangar){if(s.Script!=null)ChangeStack(s.ScriptId,hangar,(decimal)(1));s.ScriptId="";}
 public bool LoadEnhancementScript(int slot,string id)
 {
  if(slot<0||slot>=14)return false;var s=Player.Enhancements[slot];var p=EnhancementCatalog.Scripts.FirstOrDefault(p=>p.Id==id);if(s.Definition==null||s.CycleLeft>0||s.Active||s.Burnt)return false;
  if(id.Length>0&&(p==null||!EnhancementCatalog.Fits(s.Definition,p)||ItemCount(id,Docked)<1))return false;
  if(!Docked&&s.Script!=null&&CargoUsed+s.Script.Volume>Player.CargoMax+.001f)return false;
  ReturnEnhancementScript(s,Docked);if(p!=null){ChangeStack(id,Docked,-(decimal)(1));s.ScriptId=id;}return true;
 }
 public bool SetEnhancementOnline(int slot,bool online)
 {if(slot<0||slot>=14)return false;var s=Player.Enhancements[slot];if(s.Definition==null||online&&s.Burnt)return false;if(online&&!s.Online&&(Player.Stat("cpuLoad")+Player.RigCpu(s.Definition.Id,s.Definition.Cpu)>Player.Stat("cpuOutput")||Player.Stat("powerLoad")+Player.RigPower(s.Definition.Id,s.Definition.Power)>Player.Stat("powerOutput"))){Say("CPU or power grid exceeded.");return false;}if(online&&!s.Online&&!Docked){if(Player.Cap<Player.MaxCap*.95f){Say("Online in space requires 95% capacitor.");return false;}Player.Cap-=Player.MaxCap*.66f;}s.Online=online;if(!online)StopEnhancement(s);RefreshEnhancedFighters();RecomputeWeaponFit();return true;}
 public bool RepairEnhancement(int slot)
 {if(!RequireService44(13))return false;if(!Docked||slot<0||slot>=14)return false;var s=Player.Enhancements[slot];if(s.Definition==null)return false;float price=(float)s.Definition.Price*.12f*Math.Max(s.Damage,s.Burnt?1:0);if(Credits<price)return false;WalletChange(-(decimal)(price),"Gameplay transaction");s.Damage=0;s.Burnt=false;s.Overheat=false;RefreshEnhancedFighters();return true;}
 public bool HasDamagedEnhancementStock(string id,bool hangar)=>EnhancementStorage.Any(x=>x.State.ModuleId==id&&(x.State.Damage>0||x.State.Burnt)&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));
 public void TransferEnhancementCondition(string id,bool hangar,int q){foreach(var x in EnhancementStorage.Where(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44)).Take(q)){x.Hangar=!hangar;x.System=StationKey44;}}
 public bool RepairStoredEnhancements(){if(!RequireService44(13))return false;if(!Docked)return false;var a=EnhancementStorage.Where(x=>!x.Hangar||x.System==StationKey44).ToArray();float price=a.Sum(x=>(float)(x.State.Definition?.Price??0)*.12f*Math.Max(x.State.Damage,x.State.Burnt?1:0));if(Credits<price)return false;WalletChange(-(decimal)(price),"Gameplay transaction");foreach(var x in a){x.State.Damage=0;x.State.Burnt=false;x.State.Overheat=false;}return true;}
 public void ProvisionEnhancementTrial()
 {for(int i=0;i<14;i++)UnfitEnhancement(i);foreach(var d in EnhancementCatalog.All)Hangar[d.Id]=Math.Max(3,Hangar.GetValueOrDefault(d.Id));foreach(var p in EnhancementCatalog.Scripts)Hangar[p.Id]=Math.Max(3,Hangar.GetValueOrDefault(p.Id));}
 public void EnhancementTrial(string hull){WeaponTrial(hull);for(int i=0;i<4;i++)UnfitEwar(i);Say("UPGRADE TRIAL: all 21 profiles and seven reusable scripts supplied.");}
}
