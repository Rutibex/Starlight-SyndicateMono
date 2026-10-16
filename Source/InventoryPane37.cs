using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed record InventoryHit(Rectangle Rect,string Id,bool Hangar,string Owner="",decimal QuantityK=0,int SegmentK=0){public string KeyK=>Id+"#"+SegmentK;}
public sealed record InventoryDrop(Rectangle Rect,bool Hangar,string Owner="");
public sealed partial class DemoGame
{
    readonly List<InventoryHit> itemHits=[];
    readonly List<InventoryDrop> dropHits=[];
    InventoryHit? itemDrag;
    bool itemDragging,showHangar,showAllAssets;
    int inventoryScroll;
    string selectedItem="";
    void DrawInventoryWindow(DesktopWindow w)
    {
        var r=w.Bounds;int left=r.X+170,top=r.Y+77,width=r.Width-182;
        ui.Box(new(r.X+1,r.Y+29,156,r.Height-30),new Color(13,24,30));
        ui.Text("LOCATIONS",r.X+14,r.Y+43,Cyan,11);
        ui.Button(new(r.X+9,r.Y+153,140,26),"Ship information",ShowCurrentShipInfo);
        var cargo=new Rectangle(r.X+9,r.Y+70,140,36);var hangar=new Rectangle(r.X+9,r.Y+113,140,36);
        ui.Button(cargo,"Ship cargo",()=>{showAllAssets=false;showHangar=false;inventoryScroll=0;w.LocationK="";w.HoldK="";},active:!showHangar);
        ui.Button(hangar,"Item hangar",()=>{showAllAssets=false;showHangar=true;inventoryScroll=0;w.LocationK="";w.HoldK="";},world.Docked?world.StationName44:"Dock to access this location",showHangar);
        dropHits.Add(new(cargo,false,w.Owner));dropHits.Add(new(hangar,true,w.Owner));
        if(itemDragging)
        {
            bool valid=world.Docked;ui.Border(itemDrag!.Hangar?cargo:hangar,valid?Green:Red);
        }
        ui.Wrap(world.StationName44,r.X+14,r.Y+190,127,Muted,12);
        ui.Text(world.Docked?"DOCKED":"OUT OF REACH",r.X+14,r.Y+224,world.Docked?Green:Gold,11);
        InventoryLocationsK(w);
        string[] views=["Icons","Details","Compact"];
        for(int i=0;i<3;i++){int v=i;ui.Button(new(left+i*81,r.Y+37,76,27),views[i],()=>{inventorySelection=v;inventoryScroll=0;},active:inventorySelection==i);}
        ui.Button(new(left+250,r.Y+37,145,27),"All station assets",()=>{showAllAssets=!showAllAssets;inventoryScroll=0;},active:showAllAssets);if(showAllAssets){DrawAllStationAssets(w,left,top,width);return;}
        if(DrawSpecialInventoryK(w,left,top,width))return;
        SearchFieldJ(w,new(left,top,width-5,27),"Filter inventory...");top+=34;
        ui.Text(showHangar?"STATION ITEM HANGAR":w.HoldK.Length>0?DemoWorld.HoldLabel(w.HoldK)+" / ELIGIBLE CARGO":"SHIP CARGO HOLD",left,top,Cyan,12);top+=27;
        var area=new Rectangle(left,top,width,r.Bottom-80-top);
        if(showHangar&&!world.Docked)
        {
            ui.Wrap("Dock at this anchorage to view and transfer stored items.",left,top+20,width,Muted,14);return;
        }
        var lines=InventoryLinesK(w,showHangar);var ids=lines.Select(l=>l.Item).ToArray();
        dropHits.Add(new(area,showHangar,w.Owner));
        int columns=inventorySelection==0?Math.Max(1,width/133):1;
        int cellW=inventorySelection==0?width/columns:width;
        int cellH=inventorySelection==0?105:inventorySelection==1?47:28;
        int rows=(ids.Length+columns-1)/columns,visible=Math.Max(1,area.Height/cellH);
        inventoryScroll=Math.Clamp(inventoryScroll,0,Math.Max(0,rows-visible));
        ui.Layer(Matrix.Identity,area);
        for(int i=0;i<ids.Length;i++)
        {
            string id=ids[i];int row=i/columns-inventoryScroll;if(row<0||row>=visible)continue;
            int x=left+i%columns*cellW,y=top+row*cellH;decimal count=lines[i].Quantity;
            var rect=new Rectangle(x,y,cellW-5,cellH-4);var color=w.SelectionK.Contains(lines[i].Key)?new Color(41,68,78):new Color(22,35,43);
            ui.Box(rect,color);if(rect.Contains(mouse.ToPoint()))ui.Border(rect,Cyan);
            int glyph=Math.Max(0,Array.IndexOf(DemoWorld.InventoryIds,id));
            if(inventorySelection==0)
            {
                Glyph(x+(cellW-32)/2,y+8,glyph,id=="ore"?Gold:Cyan);
                ui.Center(count.ToString("0.##"),x+cellW/2,y+45,Gold,12);
                ui.Center(world.InventoryDisplayName(id),x+cellW/2,y+66,White,11);
                ui.Center($"{count*(decimal)world.InventoryDisplayVolume(id):0.##} m3",x+cellW/2,y+84,Muted,10);
            }
            else
            {
                if(inventorySelection==1)Glyph(x+5,y+5,glyph,Cyan);
                ui.Text(world.InventoryDisplayName(id),x+(inventorySelection==1?45:7),y+8,White,12);
                ui.Text($"{count:0.##}",x+cellW-115,y+8,Gold,12);
                ui.Text($"{count*(decimal)world.InventoryDisplayVolume(id):0.#} m3",x+cellW-64,y+8,Muted,11);
                if(inventorySelection==1)ui.Text(world.InventoryMission(id) is { } contract?contract.Spec.Name+" / contract cargo (m3)":id is "ore" or "ammo"?"Stackable commodity":"Utility high-slot module",x+45,y+27,Muted,10);
            }
            itemHits.Add(new(Rectangle.Intersect(rect,area),id,showHangar,w.Owner,count,lines[i].Segment));
        }
        ui.ResetLayer();
        if(ids.Length==0)ui.Text("This location is empty.",left,top+20,Muted,14);
        if(rows>visible)ui.Text($"Scroll: {inventoryScroll+1}/{Math.Max(1,rows-visible+1)}",left+width-90,area.Bottom+5,Muted,10);
        int footer=r.Bottom-62;
        ui.Bar(new(left,footer,width-5,7),world.CargoUsed/Math.Max(1,world.Player.CargoMax),Cyan);
        ui.Text($"General cargo {world.CargoUsed:0.##} / {world.Player.CargoMax:0} m3",left,footer+14,White,12);if(world.Player.UsesDedicatedHolds)ui.Text(world.SpecialistHoldSummary[..Math.Min(world.SpecialistHoldSummary.Length,76)],left,footer+34,Cyan,10);
        ui.Text($"{ids.Length} stacks  |  Balance {world.Credits:0} CR",left,world.Player.Specialist?footer+49:footer+34,Muted,11);
    }
    void DropItem(InventoryHit source,bool half)
    {
        if(TryDropK(source,half))return;
        if(TrySharedDropJ(source,half))return;
        var top=WindowAt(mouse);
        if(world.InventoryMission(source.Id)!=null)
        {
            var destination=top?.Kind==ViewMode.Inventory?dropHits.LastOrDefault(d=>d.Owner==top.Owner&&d.Rect.Contains(mouse.ToPoint())):null;
            float volume=(float)world.InventoryDisplayCount(source.Id,source.Hangar);
            if(destination!=null&&destination.Hangar!=source.Hangar)world.TransferInventoryMission(source.Id,source.Hangar,half?volume/2:volume);
            else world.Say("Drop mission cargo on Ship cargo or Item hangar. Contract goods cannot be sold, fitted or reprocessed.");
            return;
        }
        float quantity=world.ItemCount(source.Id,source.Hangar);
        if(half)quantity=source.Id=="ore"?quantity/2:Math.Max(1,MathF.Floor(quantity/2));
        if(top?.Kind==ViewMode.Inventory)
        {
            var destination=dropHits.LastOrDefault(d=>d.Owner==top.Owner&&d.Rect.Contains(mouse.ToPoint()));
            if(destination!=null&&destination.Hangar!=source.Hangar){world.TransferItem(source.Id,source.Hangar,quantity);return;}
            world.Say("Drop on Ship cargo or Item hangar in the location tree.");return;
        }
        if(top?.Kind==ViewMode.Reprocessing){if(source.Hangar)OpenReprocessing(source.Id,(long)quantity);else world.Say("Move inputs to the station hangar first.");return;}
        if(top?.Kind==ViewMode.Industry){OpenIndustryItem(source.Id);return;}
        if(top?.Kind==ViewMode.Rigs){world.FitRig(source.Id,source.Hangar,world.FittingRigSlot);return;}
        if(top?.Kind==ViewMode.Special){world.FitSpecial(source.Id,source.Hangar,world.FittingSpecialSlot);return;}
        if(top?.Kind==ViewMode.Utilities){if(UtilityPayloads.Get(source.Id)!=null)world.LoadUtilityPayload(world.FittingUtilitySlot,source.Id,source.Hangar);else world.FitUtility(source.Id,source.Hangar,world.FittingUtilitySlot);return;}
        if(top?.Kind==ViewMode.Mobility){world.FitMobility(source.Id,source.Hangar,world.FittingMobilitySlot);return;}
        if(top?.Kind==ViewMode.Enhancements){if(EnhancementCatalog.ById.ContainsKey(source.Id))world.FitEnhancement(source.Id,source.Hangar,world.FittingEnhancementSlot);else if(EnhancementCatalog.Scripts.Any(p=>p.Id==source.Id))world.LoadEnhancementScript(world.FittingEnhancementSlot,source.Id);return;}
        if(top?.Kind==ViewMode.Support){if(SupportCatalog.ById.ContainsKey(source.Id))world.FitSupport(source.Id,source.Hangar,world.FittingSupportSlot);else if(SupportPayloads.ById.ContainsKey(source.Id))world.LoadSupportPayload(world.FittingSupportSlot,source.Id);return;}
        if(top?.Kind==ViewMode.Defense){if(DefenseCatalog.ById.ContainsKey(source.Id))world.FitDefense(source.Id,source.Hangar,world.FittingDefenseSlot);else if(DefensePayloads.ById.ContainsKey(source.Id))world.LoadDefensePayload(world.FittingDefenseSlot,source.Id);return;}
        if(top?.Kind==ViewMode.Drones&&DroneCatalog.ById.ContainsKey(source.Id)){var def=DroneCatalog.ById[source.Id];int ready=world.Player.Drones.FirstOrDefault(u=>u.Location=="Bay"&&u.Tube==world.SelectedFighterTube)?.Count??0;world.LoadDrone(source.Id,source.Hangar,Math.Min((int)quantity,def.Fighter?Math.Max(0,def.SquadronSize-ready):5));return;}
        if(top?.Kind==ViewMode.Fitting)
        {
            if(!fittingWorkshop&&!fleetBrowser){DropIntoFittingLayout(source);return;}
            var r=top.Bounds;var logical=LogicalWindow(top.Kind);
            var p=new Vector2(logical.X+(mouse.X-r.X)*logical.Width/r.Width,logical.Y+29+(mouse.Y-r.Y-29)*(logical.Height-29)/(r.Height-29));
            float a=-2.35f+MathF.Tau/6;var slot=new Vector2(747,419)+new Vector2(MathF.Cos(a),MathF.Sin(a))*165;
            if(EnhancementCatalog.ById.ContainsKey(source.Id)||SupportCatalog.ById.ContainsKey(source.Id)||DefenseCatalog.ById.ContainsKey(source.Id)||EwarCatalog.ById.ContainsKey(source.Id)||WeaponCatalog.ById.ContainsKey(source.Id)||Vector2.Distance(p,slot)<42){world.FitItem(source.Id,source.Hangar);return;}
            world.Say("Drop on the gold utility high-slot socket.");return;
        }
        world.Say("Move cancelled: no compatible destination.");
    }
    void ItemMenu(InventoryHit item)
    {

        selectedItem=item.Id;var sourceWindowK=desktop.FirstOrDefault(w=>w.Owner==item.Owner);if(sourceWindowK?.LocationK.StartsWith("container:")==true){string containerId=sourceWindowK.LocationK[10..];ShowPopup(DemoWorld.ItemName(item.Id),[("Show info",()=>ShowItemInfo(item.Id)),("Move to station hangar",()=>NoticeD(world.MoveContainerItemD(item.Id,item.QuantityK,containerId,"")))]);return;}
        if(world.InventoryMission(item.Id) is { } mission)
        {
            var missionActions=new List<(string,Action)>{("Mission: "+mission.Spec.Name,()=>EnsureWindow(ViewMode.Opportunities))};
            if(world.Docked&&mission.State=="Accepted")
            {
                missionActions.Add((item.Hangar?"Load mission cargo":"Unload mission cargo",()=>world.TransferInventoryMission(item.Id,item.Hangar,(float)world.InventoryDisplayCount(item.Id,item.Hangar))));
                missionActions.Add(("Move half mission cargo",()=>world.TransferInventoryMission(item.Id,item.Hangar,(float)world.InventoryDisplayCount(item.Id,item.Hangar)/2)));
            }
            InventoryMenuExtrasK(item,missionActions);ShowPopup(world.InventoryDisplayName(item.Id),missionActions);return;
        }
        void Transfer(bool half)
        {
            float q=world.ItemCount(item.Id,item.Hangar);
            if(half)q=item.Id=="ore"?q/2:Math.Max(1,MathF.Floor(q/2));
            ExecuteCommandJ(new(item.Id,true,item.Hangar),half?"transfer-half":"transfer");
        }
        var actions=new List<(string,Action)>
        {
            ("Show info",()=>ExecuteCommandJ(new(item.Id,true,item.Hangar),"info")),
            ("View blueprint / manufacturing",()=>ExecuteCommandJ(new(item.Id,true,item.Hangar),"industry")),
            ("View market details",()=>ExecuteCommandJ(new(item.Id,true,item.Hangar),"market"))
        };
        if(world.Docked)
        {
            if(item.Hangar)actions.Add(("Reprocess stack",()=>OpenReprocessing(item.Id,(long)world.ItemCount(item.Id,true))));
            if(item.Hangar&&ItemCatalog.Get(item.Id).Slot=="ship")actions.Add(("Assemble and board hull",()=>world.AssembleIndustryHull(item.Id)));
            if(DroneCatalog.ById.ContainsKey(item.Id))actions.Add(("Load into drone bay / tube",()=>world.LoadDrone(item.Id,item.Hangar)));
            actions.Add((item.Hangar?"Move stack to ship cargo":"Move stack to item hangar",()=>Transfer(false)));
            if(world.ItemCount(item.Id,item.Hangar)>1)actions.Add(("Move half stack",()=>Transfer(true)));
            if(EwarCatalog.ById.ContainsKey(item.Id)||WeaponCatalog.ById.ContainsKey(item.Id)||item.Id is "miner" or "analyzer" or "salvager" or "mining_focus")actions.Add(("Fit to selected high slot",()=>world.FitItem(item.Id,item.Hangar)));
            if(EnhancementCatalog.ById.ContainsKey(item.Id))actions.Add(("Fit selected upgrade socket",()=>world.FitEnhancement(item.Id,item.Hangar,world.FittingEnhancementSlot)));
            if(EnhancementCatalog.Scripts.Any(p=>p.Id==item.Id))actions.Add(("Load selected upgrade script",()=>world.LoadEnhancementScript(world.FittingEnhancementSlot,item.Id)));
            if(world.HasDamagedUtilityStock(item.Id,item.Hangar))actions.Add(("Repair stored utility modules",()=>world.RepairStoredUtility()));
            if(world.HasDamagedMobilityStock(item.Id,item.Hangar))actions.Add(("Repair stored mobility",()=>world.RepairStoredMobility()));
            if(world.HasDamagedEnhancementStock(item.Id,item.Hangar))actions.Add(("Repair stored upgrades",()=>world.RepairStoredEnhancements()));
            if(SupportCatalog.ById.ContainsKey(item.Id))actions.Add(("Fit selected support socket",()=>world.FitSupport(item.Id,item.Hangar,world.FittingSupportSlot)));
            if(SupportPayloads.ById.ContainsKey(item.Id))actions.Add(("Load selected support magazine",()=>world.LoadSupportPayload(world.FittingSupportSlot,item.Id)));
            if(world.HasDamagedSupportStock(item.Id,item.Hangar))actions.Add(("Repair stored support modules",()=>world.RepairStoredSupport()));
            if(DefenseCatalog.ById.ContainsKey(item.Id)&&world.HasDamagedDefenseStock(item.Id,item.Hangar))actions.Add(("Repair stored defense modules",()=>world.RepairStoredDefenses()));
            if(DefenseCatalog.ById.ContainsKey(item.Id))actions.Add(("Fit selected defense socket",()=>world.FitDefense(item.Id,item.Hangar,world.FittingDefenseSlot)));
            if(DefensePayloads.ById.ContainsKey(item.Id))actions.Add(("Load selected defense magazine",()=>world.LoadDefensePayload(world.FittingDefenseSlot,item.Id)));
            if(EwarPayloads.ById.ContainsKey(item.Id))actions.Add(("Load selected EWAR module",()=>world.LoadEwarPayload(world.FittingEwarSlot,item.Id)));
            if(ChargeCatalog.ById.ContainsKey(item.Id))actions.Add(("Load selected weapon",()=>world.LoadWeapon(world.FittingWeaponSlot,item.Id)));
            if(item.Id=="ore"&&!item.Hangar)actions.Add(("Sell cargo ore",world.SellOre));
        }
        InventoryMenuExtrasK(item,actions);ShowPopup(DemoWorld.ItemName(item.Id),actions);
    }
    void StationInterior()
    {
        ui.Text("DOCKING BAY  /  07",474,54,Cyan,15);ui.Text(ShortInfo(world.StationName44.ToUpper(),43),474,83,White,25);
        ui.Text(world.Player.HullName.ToUpper()+" / "+(world.WeaponTestMode?"WEAPON TRIAL":"ACTIVE HULL"),508,650,White,18);
        ui.Text("Drag the hangar view to inspect your ship",508,679,Muted,12);
        DrawStationServices38();
    }
}
