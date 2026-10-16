using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;

public sealed partial class DemoWorld
{
    public Dictionary<string,decimal> CargoItems = new();
    public Dictionary<string,Dictionary<string,decimal>> StationItems = new();
    public bool DockPending;
    public bool Docked => Screen==ViewMode.Station;
    public Dictionary<string,decimal> Hangar
    {
        get { if(ImportedWorld43&&StationId.Length==0)return new();if(!StationItems.TryGetValue(StationKey44,out var items))StationItems[StationKey44]=items=new();return items; }
    }
    public static readonly string[] InventoryIds=ItemCatalog.All.Select(i=>i.Id).ToArray();
    public IEnumerable<string> VisibleInventoryIds(bool hangar)=>InventoryIds.Concat((hangar?Hangar:CargoItems).Keys).Where(ItemCatalog.Contains).Distinct(StringComparer.Ordinal);
    public static string ItemName(string id)=>ItemCatalog.Get(id).Name;
    public static float ItemVolume(string id)=>ItemCatalog.Get(id).Volume;
    public float CargoUsed => (float)PlayerPackagesVolumeD+CargoAllocation().General+.01f*Industry.Blueprints.Count(b=>b.Station=="cargo");
    void SeedHangar(){ var primary=Player.Weapons[0];primary.Loaded=20;CargoItems[primary.ChargeId]=60; foreach(var id in new[]{"missile","miner","analyzer"})Hangar[id]=id==Player.UtilityFit?0:1;Hangar["ammo"]=12;Hangar["dr_2203"]=2;Hangar["dr_32787"]=1; }
    public decimal StackCount(string id,bool hangar)=>hangar?Hangar.GetValueOrDefault(id):id=="ore"?(decimal)Player.Ore:id=="ammo"?Player.Missiles:CargoItems.GetValueOrDefault(id);
    public float ItemCount(string id,bool hangar)=>(float)StackCount(id,hangar);
    public void ChangeStack(string id,bool hangar,decimal delta){SetStack(id,hangar,StackCount(id,hangar)+delta);}
    public void SetStack(string id,bool hangar,decimal count){if(count<0)throw new InvalidOperationException("Inventory cannot be negative.");if(hangar)Hangar[id]=count;else if(id=="ore")Player.Ore=(float)count;else if(id=="ammo")Player.Missiles=checked((int)count);else CargoItems[id]=count;}
    void SetItem(string id,bool hangar,float count)
    {
        count=Math.Max(0,count);
        if(hangar)Hangar[id]=(decimal)count;
        else if(id=="ore")Player.Ore=count;
        else if(id=="ammo")Player.Missiles=(int)count;
        else CargoItems[id]=(decimal)count;
    }
    public bool TransferItem(string id,bool fromHangar,float quantity)
    {
        if(!Docked){Say("Station hangars are accessible only while docked.");return false;}
        if(!ItemCatalog.Contains(id)||!float.IsFinite(quantity)||quantity<=0||(decimal)quantity>StackCount(id,fromHangar)){Say("That stack is no longer available.");return false;}
        if(id!="ore")quantity=MathF.Floor(quantity);
        if(quantity<=0)return false;
        if(fromHangar && quantity*ItemVolume(id)>CargoRoom(id)+.001f){Say("Not enough cargo capacity. Move a smaller quantity.");return false;}
        foreach(var x in SpecialStorage.Where(x=>x.State.ModuleId==id&&x.Hangar==fromHangar&&(!fromHangar||x.System==StationKey44)).Take((int)quantity)){x.Hangar=!fromHangar;x.System=StationKey44;}TransferWeaponCondition(id,fromHangar,(int)Math.Min(quantity,int.MaxValue));TransferUtilityCondition(id,fromHangar,(int)quantity);TransferMobilityCondition(id,fromHangar,(int)quantity);TransferEnhancementCondition(id,fromHangar,(int)quantity);TransferDefenseCondition(id,fromHangar,(int)quantity);TransferSupportCondition(id,fromHangar,(int)quantity);TransferEwarCondition(id,fromHangar,(int)quantity);
        ChangeStack(id,fromHangar,-(decimal)(quantity));
        ChangeStack(id,!fromHangar,(decimal)(quantity));
        Say($"Moved {quantity:0.##} {ItemName(id)} to {(fromHangar?"ship cargo":"station hangar")}.");return true;
    }
    public bool FitItem(string id,bool fromHangar)
    {
        if(!RequireService44(17)){return false;}if(!Docked){Say("Dock before changing equipment.");return false;}
        if(!ItemCatalog.IsSupported(id)){Say("This reference item is awaiting gameplay integration.");return false;}
        if(!SkillAllowed(id))return false;
        if(RigCatalog.ById.ContainsKey(id))return FitRig(id,fromHangar,FittingRigSlot);
        if(SpecialCatalog.ById.ContainsKey(id))return FitSpecial(id,fromHangar,FittingSpecialSlot);
        if(id=="mining_focus")return LoadMiningFocus(fromHangar);
        if(UtilityCatalog.ById.ContainsKey(id))return FitUtility(id,fromHangar,FittingUtilitySlot);
        if(MobilityCatalog.ById.ContainsKey(id))return FitMobility(id,fromHangar,FittingMobilitySlot);
        if(EnhancementCatalog.ById.ContainsKey(id))return FitEnhancement(id,fromHangar,FittingEnhancementSlot);
        if(SupportCatalog.ById.ContainsKey(id))return FitSupport(id,fromHangar,FittingSupportSlot);
        if(DefenseCatalog.ById.ContainsKey(id))return FitDefense(id,fromHangar,FittingDefenseSlot);
        if(DroneCatalog.ById.ContainsKey(id))return LoadDrone(id,fromHangar);
        if(EwarCatalog.ById.ContainsKey(id))return FitEwar(id,fromHangar,FittingEwarSlot);
        if(WeaponCatalog.ById.ContainsKey(id))return FitWeapon(id,fromHangar,FittingWeaponSlot);
        if(id is not ("miner" or "analyzer" or "salvager")){Say("This item does not fit the utility high slot.");return false;}
        if(ItemCount(id,fromHangar)<1){Say("No module in the selected inventory.");return false;}
        if(Player.IsFleetHull&&Player.WeaponAt(1)==null&&!Player.LegacyUtilityOccupied&&Player.WeaponSocketCount+SpecialCount("high")+UtilityCount("high")+EnhancementCount("high")+SupportHighCount+DefenseHighCount+(Player.Ewar[0].Definition!=null?1:0)>=Player.Stat("hiSlots")){Say("No free high socket.");return false;}
        string old=Player.UtilityFit;
        ReturnMagazine(Player.Weapons[1],true);
        ChangeStack(id,fromHangar,-(decimal)(1));
        if(old.Length>0)ChangeStack(old,fromHangar,(decimal)(1));
        Player.Weapons[1]=new WeaponState{ModuleId=""};
        Player.UtilityFit=id;Player.MissileOn=Player.MinerOn=false;RecomputeWeaponFit();AdvanceHours(.75);
        Say($"Fitted {ItemName(id)}. Previous module returned to its inventory.");return true;
    }
    void CompleteDock()
    {
        if(Player.MobilitySpooling){Say("Wait for vector spool completion.");return;}if(Player.DriveLockout>0){Say("Docking interlock active.");return;}
        RecoverProbesForTransit();LeaveUtilityGrid();LeaveMobilityGrid();SalvagerOn=false;SalvagerLeft=0;ClearEnhancedLocks();foreach(var s in Player.Enhancements)if(s.Definition?.Active==true)StopEnhancement(s);LeaveSupportGrid(true);SupportDockLock=0;StopDefenses();LeaveDroneGrid();foreach(var ew in Player.Ewar)ew.Stop();EwarEffects.Clear();EwarZones.Clear();PendingLockId="";
        foreach(var w in Player.Weapons)w.Stop();Parasites.Clear();MissilesInFlight.Clear();WeaponVisuals.Clear();
        if(ImportedWorld43&&RecoveryStationId.Length==0)RecoveryStationId=StationId;EnsureLocationMarket(StationKey44);
        DockPending=false;Screen=ViewMode.Station;Player.Velocity=Vector3.Zero;Player.Order="stop";
        Player.TurretOn=Player.MissileOn=Player.MinerOn=Player.WebOn=Player.BoosterOn=false;
        Transit=null;Autopilot=false;LockedId="";AdvanceHours(10.0/3600);Say("Docking complete. Welcome to "+StationName44+".");
    }
    void CheckDocking()
    {
        if(!DockPending||Screen!=ViewMode.Space)return;
        if(ImportedWorld43){SelectedId=PendingStation44;DockSource44();return;}
        var t=Targets.FirstOrDefault(t=>t.Kind==TargetKind.Station);
        if(t!=null&&Vector3.Distance(Player.Position,t.Position)<=1800)CompleteDock();
    }
    public void Undock()
    {
        if(!Docked)return;
        if(!SkillAllowed(Player.CombatHull))return;
        if(Player.StrategicCruiser){string error=StrategicFitError();if(error.Length>0){Say(error);return;}}
        if(ImportedWorld43){string station=StationId;EnterGrid(station,false);Player.Position=new Vector3(0,1200,0);StationId="";}else{EnterGrid("station",false);Player.Position=new Vector3(-1100,0,-800);}
        AdvanceHours(5.0/3600);Say("Undock complete. Clear the approach lane.");
    }
}
