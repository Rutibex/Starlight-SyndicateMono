using System.Collections;
using System.Reflection;
using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 // Known module types only: metadata never chooses an arbitrary CLR type.
 static readonly Dictionary<string,Type> InstanceTypesD=new(){["weapon"]=typeof(WeaponState),["defense"]=typeof(DefenseState),["ewar"]=typeof(EwarState),["support"]=typeof(SupportState),["enhancement"]=typeof(EnhancementState),["mobility"]=typeof(MobilityState),["utility"]=typeof(UtilityState),["special"]=typeof(SpecialState)};
 IEnumerable<(string Bank,IList List)> ConditionBanksD(){yield return ("weapon",WeaponStorage);yield return ("defense",DefenseStorage);yield return ("ewar",EwarStorage);yield return ("support",SupportStorage);yield return ("enhancement",EnhancementStorage);yield return ("mobility",MobilityStorage);yield return ("utility",UtilityStorage);yield return ("special",SpecialStorage);}
 static object? PropertyD(object obj,string name)=>obj.GetType().GetProperty(name)?.GetValue(obj);
 static UniqueAssetD CaptureInstanceD(string bank,string item,object state)=>new(){Bank=bank,Item=item,Id=(string?)PropertyD(state,"InstanceId")??Guid.NewGuid().ToString("N"),Json=JsonSerializer.Serialize(state,state.GetType())};
 public bool RestoreInstanceD(UniqueAssetD asset,bool hangar,string station)
 {
  if(asset.Bank=="crystal"){var crystal=JsonSerializer.Deserialize<CrystalCondition>(asset.Json);if(crystal==null)return false;crystal.Hangar=hangar;crystal.System=station;CrystalConditions.Add(crystal);return true;}
  if(asset.Bank=="blueprint"){var bp=JsonSerializer.Deserialize<OwnedBlueprint37>(asset.Json);if(bp==null)return false;bp.Station=hangar?station:"cargo";Industry.Blueprints.Add(bp);return true;}
  if(asset.Bank=="fleet-q"){var berth=System.Text.Json.JsonSerializer.Deserialize<ParkedShip>(asset.Json,WeaponCatalog.JsonOptions);if(berth==null)return false;berth.System=hangar?station:"cargo";if(hangar)ParkedShips.Add(berth);else SystemsQ.RecoveredShips.Add(berth);return true;}
  if(asset.Bank=="ship"){var parked=JsonSerializer.Deserialize<ParkedShip>(asset.Json);if(parked==null||!hangar)return false;parked.System=station;ParkedShips.Add(parked);return true;}
  if(!InstanceTypesD.TryGetValue(asset.Bank,out var type))return false;object? state=JsonSerializer.Deserialize(asset.Json,type);if(state==null)return false;
  var list=ConditionBanksD().Single(b=>b.Bank==asset.Bank).List;var entry=Activator.CreateInstance(list.GetType().GetGenericArguments()[0])!;entry.GetType().GetProperty("State")!.SetValue(entry,state);entry.GetType().GetProperty("Hangar")!.SetValue(entry,hangar);(entry.GetType().GetProperty("Station")??entry.GetType().GetProperty("System"))!.SetValue(entry,station);list.Add(entry);return true;
 }
 List<UniqueAssetD> TakeInstancesD(string item,decimal quantity,bool hangar,string station)
 {
  var result=new List<UniqueAssetD>();foreach(var bank in ConditionBanksD())foreach(var entry in bank.List.Cast<object>().Where(e=>PropertyD(e,"Hangar") is bool h&&h==hangar&&(!hangar||(string?)(PropertyD(e,"Station")??PropertyD(e,"System"))==station)&&PropertyD(e,"State") is {} s&&(string?)PropertyD(s,"ModuleId")==item).Take((int)Math.Min(int.MaxValue,quantity-result.Count)).ToArray()){
   object state=PropertyD(entry,"State")!;result.Add(CaptureInstanceD(bank.Bank,item,state));bank.List.Remove(entry);
  }foreach(var c in CrystalConditions.Where(c=>c.Id==item&&c.Hangar==hangar&&(!hangar||c.System==station)).Take((int)Math.Min(int.MaxValue,quantity-result.Count)).ToArray()){result.Add(new(){Id=Guid.NewGuid().ToString("N"),Item=item,Bank="crystal",Json=JsonSerializer.Serialize(c)});CrystalConditions.Remove(c);}return result;
 }
 public bool ContainerAccessD(AssetContainerD container)
 {
  var seen=new HashSet<string>();for(var c=container;c!=null;){if(!seen.Add(c.Id)||!c.Access||c.Locked||c.Owner!="player")return false;if(c.Parent.Length==0)return true;c=Commander.Containers.FirstOrDefault(x=>x.Id==c.Parent);}return false;
 }
 public AssetContainerD? CreateContainerD(string name,string parent="",string division="Personal")
 {
  if(!Docked||string.IsNullOrWhiteSpace(name)||name.Length>80)return null;var p=Commander.Containers.FirstOrDefault(c=>c.Id==parent);if(parent.Length>0&&(p==null||p.Station!=StationKey44||!ContainerAccessD(p)))return null;
  decimal fee=50;if(Balance<fee)return null;WalletChange(-fee,"Container registration");Commander.ServiceSinks+=fee;var c=new AssetContainerD{Name=name,Station=StationKey44,Parent=parent,Division=division};Commander.Containers.Add(c);return c;
 }
 public decimal ContainerVolumeD(AssetContainerD c,HashSet<string>? visited=null){visited??=new();if(!visited.Add(c.Id))return decimal.MaxValue/100;return c.Items.Sum(i=>i.Value*(decimal)ItemVolume(i.Key))+Commander.Containers.Where(x=>x.Parent==c.Id).Sum(x=>ContainerVolumeD(x,visited));}
 bool CapacityD(AssetContainerD c,string item,decimal quantity,string source=""){var seen=new HashSet<string>();for(var current=c;current!=null;current=Commander.Containers.FirstOrDefault(x=>x.Id==current.Parent)){if(!seen.Add(current.Id)||ContainerVolumeD(current)+(ContainerDescendsD(source,current.Id)?0:quantity*(decimal)ItemVolume(item))>current.Capacity)return false;if(current.Parent.Length==0)break;}return true;}
 bool ContainerDescendsD(string id,string ancestor){var seen=new HashSet<string>();for(var c=Commander.Containers.FirstOrDefault(c=>c.Id==id);c!=null&&seen.Add(c.Id);c=Commander.Containers.FirstOrDefault(x=>x.Id==c.Parent)){if(c.Id==ancestor)return true;}return false;}
 public string NestContainerD(string id,string parent)
 {
  var c=Commander.Containers.FirstOrDefault(c=>c.Id==id);var target=Commander.Containers.FirstOrDefault(c=>c.Id==parent);if(!Docked||c==null||c.Station!=StationKey44||!ContainerAccessD(c)||parent.Length>0&&(target==null||target.Station!=c.Station||!ContainerAccessD(target)))return "Local accessible containers required.";
  var seen=new HashSet<string>{id};for(var node=target;node!=null;node=Commander.Containers.FirstOrDefault(x=>x.Id==node.Parent)){if(!seen.Add(node.Id))return "A container cannot contain itself or an ancestor.";if(node.Parent.Length==0)break;}
  string old=c.Parent;c.Parent=parent;if(target!=null&&!CapacityD(target,"ore",0)){c.Parent=old;return "Destination capacity exceeded.";}return "";
 }
 public string MoveContainerItemD(string item,decimal quantity,string from,string to)
 {
  if(!Docked||!ItemCatalog.IsSupported(item)||quantity<=0||item!="ore"&&quantity!=decimal.Floor(quantity))return "Dock locally and enter a valid quantity.";
  var source=Commander.Containers.FirstOrDefault(c=>c.Id==from);var target=Commander.Containers.FirstOrDefault(c=>c.Id==to);
  if(from==to||from.Length>0&&(source==null||source.Station!=StationKey44||!ContainerAccessD(source))||to.Length>0&&(target==null||target.Station!=StationKey44||!ContainerAccessD(target)))return "Container access denied or assets are at another station.";
  var stocks=source?.Items??Hangar;var dest=target?.Items??Hangar;if(stocks.GetValueOrDefault(item)<quantity)return "Insufficient accessible items.";
  if(target!=null&&!CapacityD(target,item,quantity,from))return "Container capacity exceeded.";
  List<UniqueAssetD> states=source==null?TakeInstancesD(item,quantity,true,StationKey44):source.Instances.Where(i=>i.Item==item).Take((int)Math.Min(int.MaxValue,quantity)).ToList();
  if(source!=null)foreach(var state in states)source.Instances.Remove(state);
  stocks[item]-=quantity;dest[item]=dest.GetValueOrDefault(item)+quantity;if(target==null)foreach(var state in states)RestoreInstanceD(state,true,StationKey44);else target.Instances.AddRange(states);
  HistoryD($"Moved {quantity} {item} / {from} > {to}");return "";
 }
 public string AssetLocationD(string station)=>station.Length==0?"In space / no station":LocationExists(station)?LocationName(station):station;
 public string AssetNameD(string id)=>ItemCatalog.Contains(id)?ItemName(id):CombatHulls.All.FirstOrDefault(h=>h.Id==id)?.Name??id;
 public IEnumerable<AssetRowD> FindAssetsD(string search="",string kind="All",string station="")
 {
  IEnumerable<AssetRowD> Enumerate(){
   foreach(var stack in OwnedStacks())yield return new(stack.Item,stack.Quantity,stack.Location.Station,stack.Location.Container,"Stack","",true,ItemCatalog.Get(stack.Item).Price*stack.Quantity);
   if(Player.Ore>0)yield return new("ore",(decimal)Player.Ore,Docked?StationKey44:"",Player.InstanceId,"Cargo","",true,ItemCatalog.Get("ore").Price*(decimal)Player.Ore);
   if(Player.Missiles>0)yield return new("ammo",Player.Missiles,Docked?StationKey44:"",Player.InstanceId,"Cargo","",true,ItemCatalog.Get("ammo").Price*Player.Missiles);
   foreach(var c in Commander.Containers.Where(ContainerAccessD))foreach(var item in c.Items.Where(i=>i.Value>0))yield return new(item.Key,item.Value,c.Station,c.Id,"Container","",true,ItemCatalog.Get(item.Key).Price*item.Value);
   foreach(var ship in FleetBerths.Concat(ParkedShips).Select(s=>(Ship:s.Ship,Station:s.System)).Append((Ship:Player,Station:Docked?StationKey44:""))){if(ship.Ship.CapsuleD)continue;yield return new(ship.Ship.CombatHull,1,ship.Station,ship.Ship.InstanceId,"Ship",FleetBerths.Concat(ParkedShips).FirstOrDefault(p=>ReferenceEquals(p.Ship,ship.Ship))?.Id??ship.Ship.InstanceId,true,HullValueD(ship.Ship));foreach(var equipment in FittingDesignStore.Equipment(ship.Ship))yield return new(equipment.Key,equipment.Value,ship.Station,ship.Ship.InstanceId,"Fitted","",true,ItemCatalog.Get(equipment.Key).Price*equipment.Value);}
   foreach(var stored in FleetBerths.Concat(ParkedShips)){if(stored.Ship.CapsuleD)continue;if(stored.Ship.Ore>0)yield return new("ore",(decimal)stored.Ship.Ore,stored.System,stored.Id,"Cargo","",true,ItemCatalog.Get("ore").Price*(decimal)stored.Ship.Ore);if(stored.Ship.Missiles>0)yield return new("ammo",stored.Ship.Missiles,stored.System,stored.Id,"Cargo","",true,ItemCatalog.Get("ammo").Price*stored.Ship.Missiles);}
   foreach(var bp in Industry.Blueprints)yield return new(bp.Item,1,bp.Station,bp.Id,bp.Original?"Blueprint original":"Blueprint copy",bp.Id,bp.LockedJob.Length==0,BlueprintPrice(bp.Item));
   foreach(var order in MarketOrders.Where(o=>o.Player&&!o.Buy&&o.Remaining>0))yield return new(order.Item,order.Remaining,order.Station,order.Id,"Market escrow","",false,order.Price*order.Remaining);
   foreach(var c in Commander.Contracts.Where(c=>c.Issuer=="player"&&!c.ItemsClaimed&&c.State is not ("Cancelled" or "Completed" or "Failed")))foreach(var item in c.Items)yield return new(item.Key,item.Value,c.Origin,c.Id,"Contract escrow","",false,ItemCatalog.Get(item.Key).Price*item.Value);
   foreach(var c in Commander.Contracts.Where(c=>c.Issuer=="player"&&!c.ItemsClaimed))foreach(var u in c.Instances.Where(u=>u.Bank is "ship" or "blueprint"))yield return new(u.Item,1,c.Origin,c.Id,"Contract escrow",u.Id,false,ItemCatalog.IsSupported(u.Item)?ItemCatalog.Get(u.Item).Price:0);
  }
  return Enumerate().Where(r=>(kind=="All"||r.Kind==kind)&&(station.Length==0||r.Station==station)&&(search.Length==0||r.Item.Contains(search,StringComparison.OrdinalIgnoreCase)||AssetNameD(r.Item).Contains(search,StringComparison.OrdinalIgnoreCase)||AssetLocationD(r.Station).Contains(search,StringComparison.OrdinalIgnoreCase)||r.Container.Contains(search,StringComparison.OrdinalIgnoreCase))).OrderBy(r=>r.Station).ThenBy(r=>r.Kind).ThenBy(r=>r.Item).ToArray();
 }
 public bool RouteToAssetD(string station){if(!LocationExists(station))return false;SetDestination(LocationSystem(station));return true;}
 public decimal HullValueD(Ship ship)=>ItemCatalog.IsSupported(ship.CombatHull)?ItemCatalog.Get(ship.CombatHull).Price:Math.Max(100,(decimal)(ship.MaxHull+ship.MaxArmor+ship.MaxShield));
}
