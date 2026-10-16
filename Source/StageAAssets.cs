namespace StarlightSyndicateDemo;
public sealed record AssetLocation(string World,string Owner,string System,string Station,string Container);
public sealed record OwnedStack(string Item,decimal Quantity,AssetLocation Location);
public sealed partial class DemoWorld
{
 public IEnumerable<OwnedStack> OwnedStacks()
 {
  foreach(var station in StationItems)foreach(var stack in station.Value.Where(s=>s.Value>0))yield return new(stack.Key,stack.Value,new(WorldDefinitionId,"player",LocationSystem(station.Key),station.Key,"station-hangar"));
  foreach(var stack in CargoItems.Where(s=>s.Value>0))yield return new(stack.Key,stack.Value,new(WorldDefinitionId,"player",SystemId,Docked?StationKey44:"",Player.InstanceId));
  foreach(var berth in FleetBerths.Concat(ParkedShips))foreach(var stack in berth.Cargo.Where(s=>s.Value>0))yield return new(stack.Key,stack.Value,new(WorldDefinitionId,"player",LocationSystem(berth.System),berth.System,berth.Ship.InstanceId));
 }
}
