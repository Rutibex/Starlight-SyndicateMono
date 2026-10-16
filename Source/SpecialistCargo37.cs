namespace StarlightSyndicateDemo;
public sealed record CargoHoldView(string Key,float Capacity,float Used);
public sealed partial class DemoWorld
{
 static readonly string[] HoldKeys=["specialIceHoldCapacity","specialGasHoldCapacity","specialMineralHoldCapacity","specialPlanetaryCommoditiesHoldCapacity","specialAmmoHoldCapacity","specialColonyResourcesHoldCapacity","specialFuelBayCapacity","generalMiningHoldCapacity","specialExpeditionHoldCapacity","fleetHangarCapacity"];
 public static bool HoldAccepts(string hold,string id)=>hold switch {
 "specialIceHoldCapacity"=>id is "ice" or "compressed_ice", "specialGasHoldCapacity"=>id is "gas" or "compressed_gas", "specialMineralHoldCapacity"=>id=="mineral24"||(id.StartsWith("ind_")&&IndustryCatalog37.MineralTypes.Contains(IndustryCatalog37.Type(id))), "specialPlanetaryCommoditiesHoldCapacity"=>id is "planetary24" or "ut_coolant", "specialAmmoHoldCapacity"=>id=="ammo"||ChargeCatalog.All.Any(c=>c.Id==id)||DefensePayloads.All.Any(c=>c.Id==id), "specialColonyResourcesHoldCapacity"=>id is "infrastructure24" or "planetary24" or "ut_coolant", "specialFuelBayCapacity"=>id=="jump_fuel"||id=="ut_coolant"||id.StartsWith("sp_isotope")||id is "sp_ozone" or "sp_catalyst", "generalMiningHoldCapacity"=>id is "ore" or "ice" or "gas" or "deepore" or "moonore"||id.StartsWith("compressed_"), "specialExpeditionHoldCapacity"=>id is "ore" or "gas" or "ice" or "deepore" or "moonore"||id.StartsWith("compressed_")||id.StartsWith("ut_probe"), "fleetHangarCapacity"=>true,_=>false};
 public (float General,CargoHoldView[] Holds) CargoAllocation(string addedId="",float addedVolume=0)
 {
  var volume=CargoItems.Where(x=>x.Value>0&&x.Key is not ("ore" or "ammo")).ToDictionary(x=>x.Key,x=>(float)x.Value*ItemVolume(x.Key));foreach(var m in AgentsState.Missions.Where(m=>m.Cargo>0)){string category=m.Spec.Division==AgentDivision38.Mining?m.Spec.Resource:"mission-freight";volume[category]=volume.GetValueOrDefault(category)+m.Cargo;}volume["ore"]=volume.GetValueOrDefault("ore")+Player.Ore;volume["ammo"]=Player.Missiles*.1f;if(addedId.Length>0)volume[addedId]=Math.Max(0,volume.GetValueOrDefault(addedId)+addedVolume);
  var holds=new List<CargoHoldView>();foreach(string key in HoldKeys){float max=Player.UsesDedicatedHolds?(float)Player.Stat(key):0,free=max;foreach(string id in volume.Keys.OrderBy(x=>x,StringComparer.Ordinal).ToArray()){if(free<=0||!HoldAccepts(key,id))continue;float q=Math.Min(volume[id],free);volume[id]-=q;free-=q;}if(max>0)holds.Add(new(key,max,max-free));}
  return(volume.Values.Sum(),holds.ToArray());
 }
 public float CargoRoom(string id)
 {
  float room=Math.Max(0,Player.CargoMax-CargoUsed);foreach(var hold in CargoAllocation().Holds.Where(h=>HoldAccepts(h.Key,id)))room+=Math.Max(0,hold.Capacity-hold.Used);return room;
 }
 public float TotalCargoVolume=>MissionCargoVolume38+Player.Ore+Player.Missiles*.1f+CargoItems.Where(x=>x.Key is not ("ore" or "ammo")).Sum(x=>(float)x.Value*ItemVolume(x.Key));
 public string SpecialistHoldSummary=>string.Join(" | ",CargoAllocation().Holds.Select(h=>$"{HoldLabel(h.Key)} {h.Used:0}/{h.Capacity:0}"));
 public static string HoldLabel(string key)=>key switch {"generalMiningHoldCapacity"=>"MINING","specialFuelBayCapacity"=>"FUEL","fleetHangarCapacity"=>"FLEET","specialMineralHoldCapacity"=>"MINERALS","specialPlanetaryCommoditiesHoldCapacity"=>"PLANETARY","specialAmmoHoldCapacity"=>"AMMO","specialColonyResourcesHoldCapacity"=>"INFRASTRUCTURE","specialExpeditionHoldCapacity"=>"EXPEDITION","specialIceHoldCapacity"=>"ICE","specialGasHoldCapacity"=>"GAS",_=>key};
}

public sealed partial class Ship {public List<ParkedShip> CarriedShips{get;set;}=[];public double MaintenanceUsed=>CarriedShips.Sum(b=>b.Ship.Stat("volume"));}
public sealed partial class DemoWorld
{
 public bool StoreFleetBerthInBay(int index){if(!Docked||index<0||index>=FleetBerths.Count||!Player.Specialist)return false;var b=FleetBerths[index];if(b.System!=StationKey44||b.Ship.CarriedShips.Count>0||Player.MaintenanceUsed+b.Ship.Stat("volume")>Player.Stat("shipMaintenanceBayCapacity"))return false;Player.CarriedShips.Add(b);FleetBerths.RemoveAt(index);Say("Fitted ship loaded into maintenance bay: "+b.Ship.HullName);return true;}
 public bool UnloadMaintenanceShip(int index){if(!Docked||index<0||index>=Player.CarriedShips.Count)return false;var b=Player.CarriedShips[index];Player.CarriedShips.RemoveAt(index);b.System=StationKey44;FleetBerths.Add(b);Say("Fitted ship unloaded to local berth: "+b.Ship.HullName);return true;}
}
