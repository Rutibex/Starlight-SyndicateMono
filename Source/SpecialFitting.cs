namespace StarlightSyndicateDemo;
public sealed class SpecialStored{public SpecialState State{get;set;}=new();public bool Hangar{get;set;}public string System{get;set;}="";}
public sealed partial class DemoWorld
{
 public List<SpecialStored> SpecialStorage{get;set;}=[];public int FittingSpecialSlot{get;set;}
 public int SpecialCount(string slot)=>slot=="high"?Player.Special.Count(s=>s.Definition!=null):0;
 public string SpecialFitError(SpecialDefinition d,int slot)
 {
  if(SkillError(d.Id) is {Length:>0} skillError)return skillError;
  if(slot<0||slot>=8)return "Choose a high socket.";var h=CombatHulls.Get(Player.CombatHull);string role=h.Role;bool titan=d.Roles.StartsWith("Titan")&&role=="Titan"&&(d.Roles=="Titan"||d.Roles=="Titan"+TitanAffinity);
  if(d.Cloak&&Player.HullCapability("unable to use cloaking"))return "This hull cannot fit cloaking devices.";
  if(d.Roles.Length>0&&!titan&&!d.Roles.Split(',').Contains(role)&&!(d.Kind is "CovertCloak" or "CovertCyno"&&Player.HullCapability("can fit covert ops")))return "Requires "+d.Roles+" hull.";
  var others=Player.Special.Where((s,i)=>i!=slot&&s.Definition!=null);if(others.Any(s=>s.Definition!.Group==d.Group)&&d.Kind!="Mass")return "Only one "+d.Group+" module may be fitted.";
  if(d.Kind=="Mass"&&others.Count(s=>s.Definition!.Kind=="Mass")>=4)return "Maximum four mass entanglers.";
  if(d.Cloak&&Enumerable.Range(0,Player.Weapons.Length).Any(i=>Player.WeaponAt(i)?.Definition.Kind==WeaponKind.DisruptiveLance))return "A fitted disruptive lance prevents cloaking.";
  int count=others.Count()+1+UtilityCount("high")+EnhancementCount("high")+Player.Support.Count(s=>s.Definition?.Slot=="high")+Player.Defense.Count(s=>s.Definition?.Slot=="high")+Player.WeaponSocketCount+(Player.Ewar[0].Definition!=null?1:0);
  if(count>Player.Stat("hiSlots"))return "Shared hull high sockets exhausted.";
  var old=Player.Special[slot];try{Player.Special[slot]=new(){ModuleId=d.Id};return MobilityBudgetError();}finally{Player.Special[slot]=old;}
 }
 public bool FitSpecial(string id,bool hangar,int slot)
 {if(!RequireService44(17))return false;
  if(!Docked||!SpecialCatalog.ById.TryGetValue(id,out var d)||ItemCount(id,hangar)<1)return false;string error=SpecialFitError(d,slot);if(error.Length>0){Say(error);return false;}var stored=SpecialStorage.FirstOrDefault(x=>x.State.ModuleId==id&&x.Hangar==hangar&&(!hangar||x.System==StationKey44));if(!UnfitSpecial(slot))return false;ChangeStack(id,hangar,-(decimal)(1));Player.Special[slot]=stored?.State??new(){ModuleId=id,Repeat=!d.NoRepeat};if(stored!=null)SpecialStorage.Remove(stored);RecomputeWeaponFit();Say("Fitted "+d.Name+".");return true;
 }
 public bool UnfitSpecial(int slot){if(!RequireService44(17))return false;if(!Docked||slot<0||slot>=8)return false;var s=Player.Special[slot];if(s.Active)return false;if(s.Definition!=null){ChangeStack(s.ModuleId,true,(decimal)(1));if(s.Cooldown>0)SpecialStorage.Add(new(){State=s,Hangar=true,System=StationKey44});}Player.Special[slot]=new();RecomputeWeaponFit();return true;}
 public bool SetSpecialOnline(int slot,bool online){if(slot<0||slot>=8)return false;var s=Player.Special[slot];if(s.Definition==null||s.Active||s.Cooldown>0)return false;if(s.Online==online)return true;s.Online=online;string e=MobilityBudgetError();s.Online=!online;if(e.Length>0){Say(e);return false;}if(online&&!Docked){if(Player.Cap<Player.MaxCap*.95)return false;Player.Cap-=Player.MaxCap*.66f;}s.Online=online;RecomputeWeaponFit();return true;}
 public void ProvisionSpecialTrial(){foreach(var s in Player.Special)if(s.Definition!=null){ChangeStack(s.ModuleId,true,(decimal)(1));if(s.Cooldown>0)SpecialStorage.Add(new(){State=s,Hangar=true,System=StationKey44});}Player.Special=Enumerable.Range(0,8).Select(_=>new SpecialState()).ToArray();foreach(var d in SpecialCatalog.All)Hangar[d.Id]=Math.Max(3,Hangar.GetValueOrDefault(d.Id));foreach(var i in SpecialCatalog.Items.Where(i=>!SpecialCatalog.ById.ContainsKey(i.Id)))Hangar[i.Id]=Math.Max(120000,Hangar.GetValueOrDefault(i.Id));}
 public void StowSpecialFuel(){if(!Docked)return;var desired=new Dictionary<string,int>();foreach(var d in Player.Special.Select(s=>s.Definition).Where(d=>d!=null&&d.Fuel.Length>0))desired[d!.Fuel]=Math.Max(desired.GetValueOrDefault(d.Fuel),d.FuelQuantity*3);if(Player.Special.Any(s=>s.Definition?.Kind=="Recloner")){desired["sp_clonefuel"]=40;desired["sp_mapper"]=4;}if(Player.Stat("canJump")>0||Player.Special.Any(s=>s.Definition?.Portal==true))desired["jump_fuel"]=2500;foreach(var pair in desired){int q=Math.Min(Math.Max(0,pair.Value-(int)ItemCount(pair.Key,false)),Math.Min((int)ItemCount(pair.Key,true),(int)(CargoRoom(pair.Key)/ItemVolume(pair.Key))));if(q>0)TransferItem(pair.Key,true,q);}Say("Loaded fuel for currently fitted systems, subject to cargo capacity.");}
 public string TitanAffinity{get;set;}="Aurora";
 public void SpecialTrial(string hull){if(!Docked)return;MobilityTrial(hull);Say("SPECIAL SYSTEMS LOAN: 25 module profiles and fuel stocked. Fit modules before leaving station.");}
}
