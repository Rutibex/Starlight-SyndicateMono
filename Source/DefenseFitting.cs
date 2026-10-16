namespace StarlightSyndicateDemo;
public sealed class DefenseStored
{public DefenseState State{get;set;}=new();public string System{get;set;}="";public bool Hangar{get;set;}}
public sealed partial class DemoWorld
{
 public List<DefenseStored> DefenseStorage{get;set;}=[];
 public int FittingDefenseSlot{get;set;}=1;
 public int DefenseHighCount=>Player.Defense.Count(s=>s.Definition?.Slot=="high");
 public int DefenseMidCount=>Player.Defense.Count(s=>s.Definition?.Slot=="medium");
 public void ProvisionDefenseTrial()
 {
  foreach(var s in Player.Defense){ReturnDefensePayload(s,true);if(s.ModuleId.Length>0)ChangeStack(s.ModuleId,true,(decimal)(1));}
  Player.Defense=Enumerable.Range(0,12).Select(i=>new DefenseState()).ToArray();Player.BoosterOn=false;
  foreach(var d in DefenseCatalog.All)Hangar[d.Id]=Math.Max(3,Hangar.GetValueOrDefault(d.Id));
  foreach(var p in DefensePayloads.All)Hangar[p.Id]=Math.Max(p.Consumable?200:3,Hangar.GetValueOrDefault(p.Id));
 }
 public void DefenseTrial(string hull)
 {WeaponTrial(hull);for(int i=0;i<4;i++)UnfitEwar(i);Say("DEFENSE TRIAL: all defense modules, cells, paste and scripts supplied. Choose a socket and fit from stock.");}
 public string DefenseFitError(DefenseDefinition d,int index)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(index<0||index>=12)return "Invalid defense socket.";
  if(Ship.DefenseSlot(index)!=d.Slot)return "Requires a "+d.Slot+"-power socket.";
  var h=CombatHulls.Get(Player.CombatHull);
  if(d.Size==4&&h.Size<4)return "Capital equipment requires a capital hull.";
  if(d.Role=="Capital"&&h.Size<4||d.Role=="Assault"&&h.Role is not ("Assault" or "HeavyAssault")||d.Role=="Battleship"&&h.Size!=3||d.Role=="Nullifiable"&&h.Role is not ("Interceptor" or "CovertOps" or "Transport" or "Industrial" or "StrategicCruiser"))return "Requires a "+d.Role+" hull.";
  var other=Player.Defense.Where((s,i)=>i!=index&&s.Definition!=null).ToArray();
  if(d.Kind=="AncillaryShield"&&other.Any(s=>s.Definition!.Kind==d.Kind&&(d.Size==4||s.Definition.Size==4)))return "Capital ancillary shield systems share a one-per-ship ancillary shield limit.";
  if(d.UniqueGroup.Length>0&&other.Any(s=>s.Definition!.UniqueGroup==d.UniqueGroup))return "Only one "+d.UniqueGroup+" module per ship.";
  int count=SpecialCount(d.Slot)+UtilityCount(d.Slot)+MobilityCount(d.Slot)+EnhancementCount(d.Slot)+other.Count(s=>s.Definition!.Slot==d.Slot)+1+(d.Slot=="high"?SupportHighCount:d.Slot=="medium"?SupportMidCount:0);
  if(d.Slot=="medium")count+=Player.Ewar.Where((s,i)=>i>0&&s.Definition!=null).Count();
  if(d.Slot=="high")count+=Player.WeaponSocketCount+(Player.Ewar[0].Definition!=null?1:0);
  if(count>Player.Stat(d.Slot=="high"?"hiSlots":d.Slot=="medium"?"medSlots":"lowSlots"))return "Not enough "+d.Slot+"-power hull sockets.";
  var weapons=Enumerable.Range(0,Player.Weapons.Length).Select(Player.WeaponAt).Where(w=>w!=null).Select(w=>w!.Definition).ToArray();
  if(Player.FittingBaseCpu+Player.SpecialCpu+Player.UtilityCpu+Player.MobilityCpu+Player.EnhancementCpu+Player.SupportCpu+weapons.Sum(w=>Player.RigCpu(w.Id,w.Cpu))+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigCpu(s.Definition.Id,s.Definition.Cpu)))+other.Sum(s=>Player.RigCpu(s.Definition!.Id,s.Definition.Cpu))+Player.RigCpu(d.Id,d.Cpu)>Player.Stat("cpuOutput")||Player.FittingBasePower+Player.SpecialPower+Player.UtilityPower+Player.MobilityPower+Player.EnhancementPower+SupportFittedPower+weapons.Sum(w=>Player.RigPower(w.Id,w.Power))+Player.Ewar.Where(s=>s.Online).Sum(s=>(s.Definition==null?0:Player.RigPower(s.Definition.Id,s.Definition.Power)))+other.Sum(s=>Player.RigPower(s.Definition!.Id,s.Definition.Power))+Player.RigPower(d.Id,d.Power)>Player.Stat("powerOutput"))return "CPU or power grid exceeded.";
  var old=Player.Defense[index];try{Player.Defense[index]=DefenseState.Create(d.Id);if(CargoUsed>Player.CargoMax+.001)return "Unload cargo before reducing hold capacity.";}finally{Player.Defense[index]=old;}
  return "";
 }
 public bool FitDefense(string id,bool hangar,int index)
 {if(!RequireService44(17))return false;
  if(!Docked){Say("Dock before fitting defenses.");return false;}
  if(!DefenseCatalog.ById.TryGetValue(id,out var d)||ItemCount(id,hangar)<1)return false;
  string error=DefenseFitError(d,index);if(error.Length>0){Say(error);return false;}
  var fractions=DefenseFractions();var old=Player.Defense[index];ReturnDefensePayload(old,true);if(old.ModuleId.Length>0)ChangeStack(old.ModuleId,true,(decimal)(1));
  StoreDefenseCondition(old,true);
  ChangeStack(id,hangar,-(decimal)(1));Player.Defense[index]=TakeDefenseCondition(id,hangar)??DefenseState.Create(id);RestoreDefenseFractions(fractions);RecomputeWeaponFit();Say("Fitted "+d.Name+" to "+Ship.DefenseSlot(index)+" socket "+(index+1)+".");return true;
 }
 public bool UnfitDefense(int index)
 {if(!RequireService44(17))return false;
  if(!Docked||index<0||index>=12)return false;var s=Player.Defense[index];if(s.Definition==null)return false;
  var old=s;try{Player.Defense[index]=new();if(CargoUsed>Player.CargoMax+.001){Say("Unload excess cargo first.");return false;}}finally{Player.Defense[index]=old;}
  var f=DefenseFractions();ReturnDefensePayload(s,true);ChangeStack(s.ModuleId,true,(decimal)(1));StoreDefenseCondition(s,true);Player.Defense[index]=new();RestoreDefenseFractions(f);RecomputeWeaponFit();return true;
 }
 float[] DefenseFractions()=>[Player.Shield/Player.MaxShield,Player.Armor/Player.MaxArmor,Player.Hull/Player.MaxHull,Player.Cap/Player.MaxCap];
 void RestoreDefenseFractions(float[] f){Player.Shield=f[0]*Player.MaxShield;Player.Armor=f[1]*Player.MaxArmor;Player.Hull=f[2]*Player.MaxHull;Player.Cap=f[3]*Player.MaxCap;Player.BoosterOn=Player.Defense.Any(s=>s.Active&&s.Definition?.Kind is "ShieldBoost" or "AncillaryShield");}
 void ReturnDefensePayload(DefenseState s,bool hangar)
 {if(s.Loaded>0&&s.Payload!=null)ChangeStack(s.PayloadId,hangar,(decimal)(s.Loaded));s.Loaded=0;s.PayloadId="";s.Active=false;s.PendingRepair=0;s.EffectLeft=0;s.CycleLeft=0;s.ReloadLeft=0;}
 public bool LoadDefensePayload(int index,string id)
 {
  if(index<0||index>=12)return false;var s=Player.Defense[index];var d=s.Definition;if(d==null||s.ReloadLeft>0||s.CycleLeft>0||s.Burnt)return false;
  DefensePayload? p=id.Length==0?null:DefensePayloads.ById.GetValueOrDefault(id);
  if(id.Length>0&&(p==null||!p.Fits(d)||ItemCount(id,Docked)<1)){Say("No compatible defense payload in this inventory.");return false;}
  if(!Docked&&s.Loaded>0&&CargoUsed+s.Loaded*(s.Payload?.Volume??0)>Player.CargoMax+.001){Say("Cargo cannot accept old defense magazine.");return false;}
  ReturnDefensePayload(s,Docked);if(p==null)return true;
  int cap=p.Consumable?(int)Math.Floor((d.MagazineVolume+.00001)/p.Volume):1;
  s.Loaded=Math.Min(cap,(int)ItemCount(id,Docked));ChangeStack(id,Docked,-(decimal)(s.Loaded));s.PayloadId=id;s.ReloadLeft=Docked?0:d.Reload;return s.Loaded>0;
 }
 public void ProvisionDefenseCargo(int index)
 {
  if(!Docked||index<0||index>=12)return;var s=Player.Defense[index];var p=s.Payload;if(p==null)return;
  int room=(int)Math.Floor(Math.Max(0,Player.CargoMax-CargoUsed)/p.Volume);int quantity=Math.Min(room,Math.Min((int)ItemCount(p.Id,true),Math.Max(1,s.Loaded*3)));if(quantity>0)TransferItem(p.Id,true,quantity);
 }
 public bool RepairDefense(int index)
 {if(!RequireService44(13))return false;
  if(!Docked||index<0||index>=12)return false;var s=Player.Defense[index];if(s.Definition==null)return false;float cost=(float)s.Definition.Price*.12f*Math.Max(s.Damage,s.Burnt?1:0);if(Credits<cost){Say("Insufficient funds for module repair.");return false;}WalletChange(-(decimal)(cost),"Gameplay transaction");s.Burnt=false;s.Damage=0;s.Overheat=false;Say("Defense module repaired.");return true;
 }
 void StoreDefenseCondition(DefenseState state,bool hangar)
 {if(state.ModuleId.Length>0)DefenseStorage.Add(new(){State=state,System=StationKey44,Hangar=hangar});}
 DefenseState? TakeDefenseCondition(string id,bool hangar)
 {var record=DefenseStorage.FirstOrDefault(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));if(record==null)return null;DefenseStorage.Remove(record);return record.State;}
 public void TransferDefenseCondition(string id,bool fromHangar,int quantity)
 {foreach(var record in DefenseStorage.Where(x=>x.State.ModuleId==id&&x.Hangar==fromHangar&&(!fromHangar||x.System==StationKey44)).Take(quantity)){record.Hangar=!fromHangar;record.System=StationKey44;}}
 public bool HasDamagedDefenseStock(string id,bool hangar)=>DefenseStorage.Any(x=>x.State.ModuleId==id&&(x.State.Damage>0||x.State.Burnt)&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));
 public bool RepairStoredDefenses()
 {if(!RequireService44(13))return false;if(!Docked)return false;var records=DefenseStorage.Where(x=>!x.Hangar||x.System==StationKey44).ToArray();float cost=records.Sum(x=>(float)(x.State.Definition?.Price??0)*.12f*Math.Max(x.State.Damage,x.State.Burnt?1:0));if(Credits<cost){Say("Insufficient funds for stored module repair.");return false;}WalletChange(-(decimal)(cost),"Gameplay transaction");foreach(var r in records){r.State.Burnt=false;r.State.Damage=0;r.State.Overheat=false;}Say("Stored defense modules repaired.");return true;}
 public void EnterDefenseRange()
 {
  EnterWeaponRange();if(Grid!="range")return;Grid="defenserange";foreach(var t in Targets)t.LockId="";Say("Defense range: pulse incoming damage and capacitor pressure from the Defense window. Dock with D to refit.");
 }
 public void DefensePressure(float damage=120,DamageType type=DamageType.Thermal)
 {if(!WeaponTestMode||Docked)return;ApplyPlayerPacket(DamagePacket.Single(damage,type));Say("Defense exercise: "+damage+" "+type+" raw damage.");}
 public void DefenseNeutPressure(float amount=100)
 {if(!WeaponTestMode||Docked)return;Player.Cap=Math.Max(0,Player.Cap-amount*(float)Player.Stat("energyWarfareResistance"));}
}
