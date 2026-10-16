namespace StarlightSyndicateDemo;
// Inventory projections only: contract cargo remains owned by the mission ledger.
public sealed partial class DemoWorld
{
    public Mission38? InventoryMission(string id)=>id.StartsWith("mission-cargo:",StringComparison.Ordinal)?AgentsState.Missions.FirstOrDefault(m=>id=="mission-cargo:"+m.Id):null;
    public IEnumerable<string> InventoryDisplayIds(bool hangar)=>VisibleInventoryIds(hangar).Concat(AgentsState.Missions.Where(m=>(hangar?m.Hangars.GetValueOrDefault(StationKey44):m.Cargo)>.001f).Select(m=>"mission-cargo:"+m.Id));
    public decimal InventoryDisplayCount(string id,bool hangar)=>InventoryMission(id) is { } m?(decimal)((hangar?m.Hangars.GetValueOrDefault(StationKey44):m.Cargo)/(m.ProfileM?.UnitVolume??1)):StackCount(id,hangar);
    public string InventoryDisplayName(string id)=>InventoryMission(id) is { } m?m.ProfileM?.CargoName??"Mission "+(m.Spec.Division==AgentDivision38.Mining?m.Spec.Resource:"freight"):ItemName(id);
    public float InventoryDisplayVolume(string id)=>InventoryMission(id) is {} m?m.ProfileM?.UnitVolume??1:ItemVolume(id);
    public bool TransferInventoryMission(string id,bool fromHangar,float volume)
    {
        var m=InventoryMission(id);if(m==null)return false;
        string error=TransferMissionCargo38(m.Id,fromHangar,volume*(m.ProfileM?.UnitVolume??1));
        Say(error.Length>0?error:$"Moved {volume:0.##} m3 of {m.Spec.Name} cargo to {(fromHangar?"ship cargo":"station hangar")}.");return error.Length==0;
    }
}
