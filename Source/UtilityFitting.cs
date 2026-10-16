namespace StarlightSyndicateDemo;
public sealed class UtilityStored{public UtilityState State{get;set;}=new();public bool Hangar{get;set;}public string System{get;set;}="";}
public sealed partial class DemoWorld
{
 public int FittingUtilitySlot{get;set;}public List<UtilityStored> UtilityStorage{get;set;}=[];
 public int UtilityCount(string slot)=>Player.Utilities.Count(s=>s.Definition?.Slot==slot);
 public string UtilityFitError(UtilityDefinition d,int slot)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(slot<0||slot>=16||Ship.UtilitySlot(slot)!=d.Slot)return "Choose a "+d.Slot+" socket.";var h=CombatHulls.Get(Player.CombatHull);if(d.Role.Length>0&&d.Role!=h.Role)return "Requires "+d.Role+" hull.";
  var others=Player.Utilities.Where((s,i)=>i!=slot&&s.Definition!=null);if(d.MaxGroup>0&&others.Count(s=>s.Definition!.Group==d.Group)>=d.MaxGroup)return "Fitted group limit.";
  int count=SpecialCount(d.Slot)+others.Count(s=>s.Definition!.Slot==d.Slot)+1+MobilityCount(d.Slot)+EnhancementCount(d.Slot)+Player.Defense.Count(s=>s.Definition?.Slot==d.Slot)+Player.Support.Count(s=>s.Definition?.Slot==d.Slot)+(d.Slot=="high"?Player.WeaponSocketCount+(Player.Ewar[0].Definition!=null?1:0):d.Slot=="medium"?Player.Ewar.Skip(1).Count(s=>s.Definition!=null):0);
  if(count>Player.Stat(d.Slot=="high"?"hiSlots":d.Slot=="medium"?"medSlots":"lowSlots"))return "Shared hull socket limit.";
  var old=Player.Utilities[slot];try{Player.Utilities[slot]=new(){ModuleId=d.Id};return MobilityBudgetError();}finally{Player.Utilities[slot]=old;}
 }
 public bool FitUtility(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||!UtilityCatalog.ById.TryGetValue(id,out var d)||ItemCount(id,hangar)<1)return false;string error=UtilityFitError(d,slot);if(error.Length>0){Say(error);return false;}
  var stored=UtilityStorage.FirstOrDefault(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));var candidate=stored?.State??new(){ModuleId=id};var old=Player.Utilities[slot];try{Player.Utilities[slot]=candidate;error=MobilityBudgetError();}finally{Player.Utilities[slot]=old;}if(error.Length>0)return false;
  ReturnUtility(old);ChangeStack(id,hangar,-(decimal)(1));Player.Utilities[slot]=candidate;if(stored!=null)UtilityStorage.Remove(stored);RecomputeWeaponFit();Say("Fitted "+d.Name+".");return true;
 }
 void ReturnUtility(UtilityState s){if(s.Definition==null)return;StopUtility(s);if(s.Payload!=null&&s.Loaded>0){ChangeStack(s.PayloadId,true,(decimal)(s.Loaded));if(s.Payload.Kind=="Crystal"&&s.CrystalWear>0)CrystalConditions.Add(new(){Id=s.PayloadId,Wear=s.CrystalWear,Hangar=true,System=StationKey44});}s.PayloadId="";s.Loaded=0;s.CrystalWear=0;ChangeStack(s.ModuleId,true,(decimal)(1));if(s.ModuleId.Length>0)UtilityStorage.Add(new(){State=s,Hangar=true,System=StationKey44});}
 public bool UnfitUtility(int slot){if(!RequireService44(17))return false;if(!Docked||slot<0||slot>=16)return false;var s=Player.Utilities[slot];if(s.Definition==null)return true;ReturnUtility(s);Player.Utilities[slot]=new();RecomputeWeaponFit();return true;}
 public bool SetUtilityOnline(int slot,bool online)
 {if(slot<0||slot>=16)return false;var s=Player.Utilities[slot];if(s.Definition==null||online&&s.Burnt||s.Active||s.ReloadLeft>0)return false;if(s.Online==online)return true;s.Online=online;string error=MobilityBudgetError();s.Online=!online;if(error.Length>0)return false;if(online&&!Docked){if(Player.Cap<Player.MaxCap*.95f)return false;Player.Cap-=Player.MaxCap*.66f;}s.Online=online;RecomputeWeaponFit();return true;}
 public bool RepairUtility(int slot){if(!RequireService44(13))return false;if(!Docked||slot<0||slot>=16)return false;var s=Player.Utilities[slot];if(s.Definition==null)return false;float price=(float)s.Definition.Price*.12f*Math.Max(s.Damage,s.Burnt?1:0);if(Credits<price)return false;WalletChange(-(decimal)(price),"Gameplay transaction");s.Damage=0;s.Burnt=false;return true;}
 public bool HasDamagedUtilityStock(string id,bool hangar)=>UtilityStorage.Any(x=>x.State.ModuleId==id&&(x.State.Damage>0||x.State.Burnt)&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));
 public void TransferUtilityCondition(string id,bool hangar,int q){foreach(var x in UtilityStorage.Where(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44)).Take(q)){x.Hangar=!hangar;x.System=StationKey44;}foreach(var x in CrystalConditions.Where(x=>x.Id==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44)).Take(q)){x.Hangar=!hangar;x.System=StationKey44;}}
 public bool LoadUtilityPayload(int slot,string id,bool? fromHangar=null)
 {
  if(slot<0||slot>=16)return false;var s=Player.Utilities[slot];var p=UtilityPayloads.Get(id);if(s.Definition==null||s.Active||s.ReloadLeft>0||p!=null&&!UtilityPayloads.Fits(s.Definition,p)||id.Length>0&&p==null)return false;bool hangar=fromHangar??(Docked&&(id.Length==0||ItemCount(id,true)>0));if(hangar&&!Docked)return false;
  if(p!=null&&ItemCount(id,hangar)<1)return false;if(!hangar&&s.Payload!=null&&CargoUsed+s.Loaded*s.Payload.Volume>Player.CargoMax)return false;
  if(s.Payload!=null&&s.Loaded>0){ChangeStack(s.PayloadId,hangar,(decimal)(s.Loaded));if(s.Payload.Kind=="Crystal"&&s.CrystalWear>0)CrystalConditions.Add(new(){Id=s.PayloadId,Wear=s.CrystalWear,Hangar=hangar,System=StationKey44});}
  s.PayloadId=id;s.Loaded=0;s.CrystalWear=0;if(p!=null){int max=p.Kind=="Crystal"?1:p.Kind is "Core" or "Combat"?8:p.Kind=="Survey"?1:3;int q=Math.Min(max,(int)ItemCount(id,hangar));ChangeStack(id,hangar,-(decimal)(q));s.Loaded=q;var condition=CrystalConditions.FirstOrDefault(x=>x.Id==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));if(condition!=null){s.CrystalWear=condition.Wear;CrystalConditions.Remove(condition);}s.ReloadLeft=s.Definition.Reload;}return true;
 }
 public bool RepairStoredUtility(){if(!RequireService44(13))return false;if(!Docked)return false;var a=UtilityStorage.Where(x=>!x.Hangar||x.System==StationKey44).ToArray();float price=a.Sum(x=>(float)(x.State.Definition?.Price??0)*.12f*Math.Max(x.State.Damage,x.State.Burnt?1:0));if(Credits<price)return false;WalletChange(-(decimal)(price),"Gameplay transaction");foreach(var x in a){x.State.Damage=0;x.State.Burnt=false;}return true;}
 public void StowUtilityPayloads(){if(!Docked)return;foreach(var p in UtilityPayloads.All){int q=Math.Min(p.Kind=="Crystal"?2:16,Math.Min((int)ItemCount(p.Id,true),(int)((Player.CargoMax-CargoUsed)/p.Volume)));if(q>0)TransferItem(p.Id,true,q);}}
 public void ProvisionUtilityTrial(){for(int i=0;i<16;i++){ReturnUtility(Player.Utilities[i]);Player.Utilities[i]=new();}foreach(var d in UtilityCatalog.All)Hangar[d.Id]=Math.Max(3,Hangar.GetValueOrDefault(d.Id));foreach(var p in UtilityPayloads.All)Hangar[p.Id]=Math.Max(40,Hangar.GetValueOrDefault(p.Id));Hangar["ut_coolant"]=Math.Max(500,Hangar.GetValueOrDefault("ut_coolant"));}
 public void UtilityTrial(string hull){if(!Docked)return;MobilityTrial(hull);Say("UTILITY LOAN: all 42 profiles, probes, prisms and coolant stocked.");}
}
public sealed class CrystalCondition{public string Id{get;set;}="";public float Wear{get;set;}public bool Hangar{get;set;}public string System{get;set;}="";}
