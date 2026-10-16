namespace StarlightSyndicateDemo;
public sealed class MobilityStored{public MobilityState State{get;set;}=new();public bool Hangar{get;set;}public string System{get;set;}="";}
public sealed partial class DemoWorld
{
 public int FittingMobilitySlot{get;set;}public List<MobilityStored> MobilityStorage{get;set;}=[];
 public int MobilityCount(string slot)=>Player.Mobility.Count(s=>s.Definition?.Slot==slot);
 public double TotalFitCpu=>Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.EnhancementCpu+Player.SupportCpu+Player.DefenseCpu+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigCpu(s.Definition.Id,s.Definition.Cpu)))+Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null&&w.Online).Sum(w=>Player.RigCpu(w!.Definition.Id,w.Definition.Cpu));
 public double TotalFitPower=>Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+Player.EnhancementPower+SupportFittedPower+Player.DefensePower+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigPower(s.Definition.Id,s.Definition.Power)))+Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null&&w.Online).Sum(w=>Player.RigPower(w!.Definition.Id,w.Definition.Power));
 public string MobilityFitError(MobilityDefinition d,int slot)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(slot<0||slot>=12||Ship.MobilitySlot(slot)!=d.Slot)return "Select a "+d.Slot+" socket.";var h=CombatHulls.Get(Player.CombatHull);
  bool role=d.Role switch{"Capital"=>h.Size>=4,"Carrier"=>h.Role=="Carrier","CommandDestroyer"=>h.Role=="CommandDestroyer"||Player.HullCapability("micro jump field generators"),"Battlecruiser"=>h.Role is "Battlecruiser" or "Command" or "DeepTransport"||Player.HullCapability("medium micro jump drive"),"Battleship"=>h.Size==3&&h.Role is not ("Command" or "Battlecruiser" or "ExpeditionCommand"),"JumpFreighter,CapitalIndustrial"=>h.Role is "JumpFreighter" or "CapitalIndustrial",_=>true};if(!role)return "Requires "+d.Role+" hull.";
  var others=Player.Mobility.Where((s,i)=>i!=slot&&s.Definition!=null).ToArray();if(d.MaxGroup>0&&others.Count(s=>s.Definition!.Group==d.Group)>=d.MaxGroup)return "Fitted module group limit exceeded.";
  int count=SpecialCount(d.Slot)+UtilityCount(d.Slot)+others.Count(s=>s.Definition!.Slot==d.Slot)+1+EnhancementCount(d.Slot)+Player.Defense.Count(s=>s.Definition?.Slot==d.Slot)+Player.Support.Count(s=>s.Definition?.Slot==d.Slot)+(d.Slot=="medium"?Player.Ewar.Skip(1).Count(s=>s.Definition!=null):0);if(count>Player.Stat(d.Slot=="medium"?"medSlots":"lowSlots"))return "Shared hull sockets exceeded.";
  var old=Player.Mobility[slot];try{Player.Mobility[slot]=new(){ModuleId=d.Id};return MobilityBudgetError();}finally{Player.Mobility[slot]=old;}
 }
 public string MobilityBudgetError()=>CargoUsed>Player.CargoMax+.001?"Unload cargo before reducing capacity.":TotalFitCpu>Player.Stat("cpuOutput")+.001||TotalFitPower>Player.Stat("powerOutput")+.001?"CPU or power grid exceeded.":"";
 public bool FitMobility(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||!MobilityCatalog.ById.TryGetValue(id,out var d)||ItemCount(id,hangar)<1)return false;string error=MobilityFitError(d,slot);if(error.Length>0){Say(error);return false;}
  var stored=MobilityStorage.FirstOrDefault(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));var candidate=stored?.State??new(){ModuleId=id};var old=Player.Mobility[slot];try{Player.Mobility[slot]=candidate;error=MobilityBudgetError();}finally{Player.Mobility[slot]=old;}if(error.Length>0){Say(error);return false;}var fractions=DefenseFractions();ReturnMobilityModule(old);ChangeStack(id,hangar,-(decimal)(1));Player.Mobility[slot]=candidate;if(stored!=null)MobilityStorage.Remove(stored);RestoreDefenseFractions(fractions);RecomputeWeaponFit();Say("Fitted "+d.Name+".");return true;
 }
 void ReturnMobilityModule(MobilityState s){if(s.Definition==null)return;StopMobility(s);ChangeStack(s.ModuleId,true,(decimal)(1));if(s.ModuleId.Length>0)MobilityStorage.Add(new(){State=s,Hangar=true,System=StationKey44});}
 public bool UnfitMobility(int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||slot<0||slot>=12)return false;var old=Player.Mobility[slot];if(old.Definition==null)return true;try{Player.Mobility[slot]=new();string error=MobilityBudgetError();if(error.Length>0){Say(error);return false;}}finally{Player.Mobility[slot]=old;}
  var f=DefenseFractions();ReturnMobilityModule(old);Player.Mobility[slot]=new();RestoreDefenseFractions(f);RecomputeWeaponFit();return true;
 }
 public bool SetMobilityOnline(int slot,bool online)
 {
  if(slot<0||slot>=12)return false;var s=Player.Mobility[slot];if(s.Definition==null||online&&s.Burnt||s.Active&&s.Definition.Skip)return false;if(s.Online==online)return true;var f=DefenseFractions();s.Online=online;string error=MobilityBudgetError();s.Online=!online;if(error.Length>0){Say(error);return false;}
  if(online&&!Docked){if(Player.Cap<Player.MaxCap*.95f)return false;Player.Cap-=Player.MaxCap*.66f;f[3]=Player.Cap/Player.MaxCap;}s.Online=online;if(!online)StopMobility(s);RestoreDefenseFractions(f);RecomputeWeaponFit();return true;
 }
 public bool RepairMobility(int slot)
 {if(!RequireService44(13))return false;if(!Docked||slot<0||slot>=12)return false;var s=Player.Mobility[slot];if(s.Definition==null)return false;float cost=(float)s.Definition.Price*.12f*Math.Max(s.Damage,s.Burnt?1:0);if(Credits<cost)return false;var f=DefenseFractions();WalletChange(-(decimal)(cost),"Gameplay transaction");s.Damage=0;s.Burnt=false;s.Overheat=false;RestoreDefenseFractions(f);return true;}
 public bool HasDamagedMobilityStock(string id,bool hangar)=>MobilityStorage.Any(x=>x.State.ModuleId==id&&(x.State.Damage>0||x.State.Burnt)&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));
 public void TransferMobilityCondition(string id,bool hangar,int q){foreach(var x in MobilityStorage.Where(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44)).Take(q)){x.Hangar=!hangar;x.System=StationKey44;}}
 public bool RepairStoredMobility(){if(!RequireService44(13))return false;if(!Docked)return false;var a=MobilityStorage.Where(x=>!x.Hangar||x.System==StationKey44).ToArray();float cost=a.Sum(x=>(float)(x.State.Definition?.Price??0)*.12f*Math.Max(x.State.Damage,x.State.Burnt?1:0));if(Credits<cost)return false;WalletChange(-(decimal)(cost),"Gameplay transaction");foreach(var x in a){x.State.Damage=0;x.State.Burnt=false;}return true;}
 public void ProvisionMobilityTrial()
 {for(int i=0;i<12;i++){ReturnMobilityModule(Player.Mobility[i]);Player.Mobility[i]=new();}foreach(var d in MobilityCatalog.All)Hangar[d.Id]=Math.Max(3,Hangar.GetValueOrDefault(d.Id));Hangar["jump_fuel"]=Math.Max(3000,Hangar.GetValueOrDefault("jump_fuel"));}
 public void MobilityTrial(string hull){if(!Docked){Say("Dock before selecting a loan hull.");return;}if(CargoUsed>CombatHulls.Get(hull).Cargo)foreach(var id in InventoryIds){float q=ItemCount(id,false);if(q>0)TransferItem(id,false,q);}WeaponTrial(hull);for(int i=0;i<4;i++)UnfitEwar(i);Say("MOBILITY TRIAL: 23 module profiles and transit fuel supplied.");}
}
