using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public UniqueAssetD? PreviewUniqueD(string id)
 {
  var ship=ParkedShips.Concat(FleetBerths).FirstOrDefault(s=>s.Id==id);if(ship!=null)return Docked&&ship.System==StationKey44&&!ship.Ship.CapsuleD?new(){Bank="ship",Item=ship.Ship.CombatHull,Id=ship.Id,Json=JsonSerializer.Serialize(ship)}:null;
  var bp=Industry.Blueprints.FirstOrDefault(b=>b.Id==id);return bp!=null&&Docked&&bp.Station==StationKey44&&bp.LockedJob.Length==0?new(){Bank="blueprint",Item=bp.Item,Id=bp.Id,Json=JsonSerializer.Serialize(bp)}:null;
 }
 UniqueAssetD? ReserveUniqueD(string id){var asset=PreviewUniqueD(id);if(asset==null)return null;ParkedShips.RemoveAll(s=>s.Id==id);FleetBerths.RemoveAll(s=>s.Id==id);Industry.Blueprints.RemoveAll(b=>b.Id==id);if(asset.Bank=="ship"){var ship=JsonSerializer.Deserialize<ParkedShip>(asset.Json)!;foreach(var policy in Commander.Insurance.Where(p=>p.Ship==ship.Ship.InstanceId&&!p.Claimed))policy.Expires=Now;}return asset;}
 public decimal PackageVolumeD(ContractD c)=>c.Items.Sum(i=>i.Value*(decimal)ItemVolume(i.Key))+c.Instances.Where(i=>i.Bank is "ship" or "blueprint").Sum(i=>i.Bank=="blueprint"?.01m:ItemCatalog.IsSupported(i.Item)?(decimal)ItemVolume(i.Item):1000m);
 public decimal PlayerPackagesVolumeD=>Commander.Contracts.Where(c=>c.Kind==ContractKindD.Courier&&c.Carrier=="player"&&c.State=="In progress").Sum(PackageVolumeD);
 public string CreateUniqueContractD(string id,ContractKindD kind,string destination,decimal price,decimal reward,decimal collateral,int days,double hours,decimal buyout=0)
 {
  var c=CreateContractD(kind,"Unique asset commission",new(),destination,price,reward,collateral,days,hours,buyout:buyout,assetIds:[id]);return c==null?"Only owned, local, unlocked assembled ships or blueprint instances can be reserved.":"";
 }
 public void ValidateCommanderD(CommanderStateD? state)
 {
  if(state==null)return;if(state.Contracts.Select(c=>c.Id).Distinct().Count()!=state.Contracts.Count||state.Containers.Select(c=>c.Id).Distinct().Count()!=state.Containers.Count||state.Wrecks.Select(w=>w.Id).Distinct().Count()!=state.Wrecks.Count)throw new InvalidDataException("Duplicate Stage D identity in save.");
  bool Bad(Dictionary<string,decimal> items)=>items.Any(i=>i.Value<0||i.Value>1000000000000m||!ItemCatalog.IsSupported(i.Key));
  foreach(var c in state.Containers){if(c.Capacity<=0||Bad(c.Items))throw new InvalidDataException("Invalid container state.");var seen=new HashSet<string>{c.Id};var at=c;while(at.Parent.Length>0){at=state.Containers.FirstOrDefault(x=>x.Id==at.Parent)??throw new InvalidDataException("Missing container parent.");if(!seen.Add(at.Id)||at.Station!=c.Station)throw new InvalidDataException("Cyclic or cross-station container.");}}
  foreach(var c in state.Contracts)if(Bad(c.Items)||Bad(c.Requested)||c.MoneyEscrow<0||c.CollateralEscrow<0||c.BidEscrow<0||c.Price<0||c.Reward<0||c.Collateral<0||!Enum.IsDefined(c.Kind)||!new[]{"Outstanding","In progress","Finished","Completed","Cancelled","Failed"}.Contains(c.State))throw new InvalidDataException("Invalid contract state.");
  foreach(var w in state.Wrecks)if(Bad(w.Items))throw new InvalidDataException("Invalid wreck contents.");
  var unique=state.Contracts.SelectMany(c=>c.Instances).Concat(state.Containers.SelectMany(c=>c.Instances)).Concat(state.Wrecks.SelectMany(w=>w.Instances)).ToArray();if(unique.Select(u=>u.Id).Distinct().Count()!=unique.Length||unique.Any(u=>u.Id.Length==0||!InstanceTypesD.ContainsKey(u.Bank)&&u.Bank is not ("ship" or "blueprint" or "crystal" or "fleet-q")))throw new InvalidDataException("Invalid or duplicated escrow instance.");
 }
}
