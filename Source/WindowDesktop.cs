using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;

public sealed class DesktopWindow
{
    public ViewMode Kind {get;set;}
    public int X {get;set;} public int Y {get;set;} public int Width {get;set;} public int Height {get;set;}
    public bool Minimized {get;set;}
    public bool Locked {get;set;}public bool Compact {get;set;}public float Opacity {get;set;}=1;
    public string StackId {get;set;}="";public string SnapGroupJ {get;set;}="";public bool OpenJ {get;set;}
    public bool InventoryHangarJ {get;set;}public bool InventoryAllJ {get;set;}public int InventoryViewJ {get;set;}public int InventoryScrollJ {get;set;}public string InventorySelectedJ {get;set;}="";
    public string QueryJ {get;set;}="";public int ScrollJ {get;set;}public string PresetJ {get;set;}="All";
    public List<TableColumnJ> ColumnsJ {get;set;}=[];public string SortJ {get;set;}="distance";public bool DescendingJ {get;set;}
    public string LocationK {get;set;}="";public string HoldK {get;set;}="";public HashSet<string> SelectionK {get;set;}=[];public Dictionary<string,List<decimal>> SplitStacksK {get;set;}=[];public List<OverviewTabK> TabsK {get;set;}=[];public int TabK {get;set;}
    public List<string> CompareKeysK {get;set;}=[];public bool CompareDiffK {get;set;}public string CompareStatK {get;set;}="Volume";
    public string SubjectL{get;set;}="";public int DetailTabL{get;set;}public int DetailScrollL{get;set;}
    public Rectangle Bounds => new(X,Y,Width,Minimized?29:Height);
    public Rectangle FullBounds => new(X,Y,Width,Height);
    public string InstanceId {get;set;}="";
    public string InfoKey {get;set;}="";public string InfoTab {get;set;}="Description";
    public int InfoScroll {get;set;}public int InfoHistoryIndex {get;set;}=-1;
    public List<string> InfoHistory {get;set;}=[];public float InfoModelYaw {get;set;}=.7f;public float InfoModelPitch {get;set;}=-.25f;public float InfoModelZoom {get;set;}=1;public int InfoModelLod {get;set;}
    public string Owner=>InstanceId.Length>0?Kind+"/"+InstanceId:Kind.ToString();
}
public sealed partial class DemoGame
{
    readonly List<DesktopWindow> desktop=[];
    readonly Dictionary<ViewMode,DesktopWindow> placements=new();
    DesktopWindow? movingWindow,drawingWindow;
    bool resizing,swallowRelease;
    Vector2 gestureOrigin;
    Rectangle gestureBounds;
    HitArea? pressedHit;
    readonly List<(string Label,Action Action)> popupActions=[];
    string popupTitle="";
    Rectangle popupRect;
    static Rectangle LogicalWindow(ViewMode kind)=>kind switch
    {
        ViewMode.IndustryQ or ViewMode.MiningQ or ViewMode.PlanetaryQ or ViewMode.CapabilitiesQ or ViewMode.StructuresQ or ViewMode.TerritoryQ or ViewMode.EconomyQ=>new(120,70,1300,755),ViewMode.OperationsP=>new(120,70,1300,755),ViewMode.JournalL=>new(220,100,1060,690),ViewMode.ConversationL=>new(420,75,710,745),ViewMode.CharacterK=>new(220,90,1000,700),ViewMode.AssetsK=>new(180,90,1120,650),ViewMode.WalletK=>new(250,120,1040,600),ViewMode.ContractsK=>new(120,70,1300,755),ViewMode.OverviewSettingsK=>new(350,100,850,660),ViewMode.DesktopSettingsJ=>new(260,90,1030,650),ViewMode.OverviewJ=>new(1120,120,460,470),ViewMode.Experience=>new(120,70,1300,755),ViewMode.Advanced=>new(120,70,1300,755),ViewMode.Organisations=>new(120,70,1300,755),ViewMode.Exploration=>new(120,70,1300,755),ViewMode.Production=>new(120,70,1300,755),ViewMode.Commander=>new(120,70,1300,755),ViewMode.Population=>new(120,70,1300,755),ViewMode.Galaxy=>new(80,60,1440,795),ViewMode.Origins=>new(180,80,1180,710),ViewMode.Agents=>new(150,80,1070,720),ViewMode.Standings=>new(150,80,1070,720),ViewMode.Opportunities=>new(150,80,1070,720),ViewMode.Industry=>new(100,70,1350,755),ViewMode.Reprocessing=>new(260,120,1040,650),ViewMode.Skills=>new(110,75,1310,745),ViewMode.Information=>new(460,110,620,650),ViewMode.ProbeMap=>new(60,60,1060,800),ViewMode.SolarMap=>new(207,84,1033,690),ViewMode.DirectionalScanner=>new(423,130,817,523),ViewMode.Rigs=>new(220,85,1030,710),ViewMode.Special=>new(220,85,1030,710),ViewMode.Utilities=>new(220,85,1030,710),ViewMode.Mobility=>new(220,85,1030,710),ViewMode.Enhancements=>new(220,85,1030,710),ViewMode.Support=>new(220,85,1030,710),ViewMode.Defense=>new(220,85,1030,710),ViewMode.Drones=>new(1260,600,325,290),ViewMode.Fitting=>new(90,65,1400,770),ViewMode.Map=>new(207,84,1033,690),
        ViewMode.Market=>new(135,75,1320,760),ViewMode.Catalog=>new(80,70,1440,760),
        ViewMode.ShipInfo=>new(260,100,1060,700),ViewMode.Scanner=>new(1125,60,460,640),
        ViewMode.Hack=>new(425,138,816,510),ViewMode.Decision=>new(432,149,805,475),
        ViewMode.Help=>new(434,127,802,507),_=>new(85,150,655,485)
    };
    static string WindowTitle(ViewMode kind)=>kind switch
    {
        ViewMode.IndustryQ=>"INDUSTRY / BLUEPRINT ACTIVITIES",ViewMode.MiningQ=>"RESOURCE SURVEY / EXTRACTION",ViewMode.PlanetaryQ=>"PLANETARY COLONIES",ViewMode.CapabilitiesQ=>"MODULE MUTATION / CAPITAL HOLDS",ViewMode.StructuresQ=>"STRUCTURES / CUSTODY / LEGAL TIMERS",ViewMode.TerritoryQ=>"TERRITORIAL OPERATIONS",ViewMode.EconomyQ=>"REGIONAL INDUSTRIAL ECONOMY",ViewMode.JournalL=>"MISSION JOURNAL",ViewMode.ConversationL=>"AGENT CONVERSATION",ViewMode.CharacterK=>"CHARACTER SHEET",ViewMode.AssetsK=>"PERSONAL ASSETS",ViewMode.WalletK=>"WALLET",ViewMode.ContractsK=>"CONTRACTS",ViewMode.OverviewSettingsK=>"OVERVIEW SETTINGS",ViewMode.DesktopSettingsJ=>"DESKTOP SETTINGS",ViewMode.OverviewJ=>"OVERVIEW",ViewMode.Experience=>"EXPERIENCE / GUIDANCE / COMMUNICATIONS / LOCAL TOOLS",ViewMode.OperationsP=>"COOPERATIVE OPERATIONS / FLEET COMMAND",ViewMode.Advanced=>"ADVANCED ACTIVITIES / MODERN WORLD",ViewMode.Organisations=>"ORGANISATIONS / STRUCTURES / TERRITORIAL CONFLICT",ViewMode.Exploration=>"PVE / EXPLORATION / ENCOUNTER OPERATIONS",ViewMode.Production=>"RESOURCES / PRODUCTION NETWORK",ViewMode.Commander=>"COMMANDER / CHARACTER / ASSETS / COMMERCIAL TOOLS",ViewMode.Population=>"POPULATION / CREW / REGIONAL ECONOMY",ViewMode.Galaxy=>"GALAXY MAP / ROUTE PLANNER",ViewMode.Origins=>"PERSONAL HISTORY / TITLE",ViewMode.Agents=>"AGENT CONVERSATION / FINDER",ViewMode.Standings=>"INTERACTIONS / STANDINGS",ViewMode.Opportunities=>"OPPORTUNITIES / AGENT MISSIONS",ViewMode.Industry=>"INDUSTRY",ViewMode.Reprocessing=>"REPROCESSING",ViewMode.Skills=>"SKILLS",ViewMode.SolarMap=>"SOLAR SYSTEM MAP",ViewMode.DirectionalScanner=>"DIRECTIONAL SCANNER",ViewMode.Rigs=>"SHIP RIGS / CALIBRATION",ViewMode.Special=>"CLOAK / JUMP / CAPITAL SYSTEMS",ViewMode.Utilities=>"EXPLORATION / MINING / UTILITY",ViewMode.Mobility=>"PROPULSION / FITTING / CARGO",ViewMode.Enhancements=>"DAMAGE / TARGETING / FLIGHT SUPPORT",ViewMode.Support=>"LOGISTICS / FLEET SUPPORT",ViewMode.Defense=>"DEFENSE / CAPACITOR",ViewMode.Drones=>"DRONES / FIGHTER CONTROL",ViewMode.Fitting=>"SHIP FITTING",ViewMode.Map=>"CLUSTER NAVIGATION",ViewMode.Market=>"REGIONAL MARKET",
        ViewMode.Information=>"INFORMATION",ViewMode.Catalog=>"ENCYCLOPEDIA",ViewMode.ShipInfo=>"SHIP INFORMATION",ViewMode.ProbeMap=>"SOLAR SYSTEM / PROBE MAP",ViewMode.Scanner=>"PROBE SCANNER",
        ViewMode.Hack=>"DATA ANALYZER",ViewMode.Decision=>"JOYOS / EVIDENCE",ViewMode.Help=>"FLIGHT MANUAL",_=>"INVENTORY"
    };
    DesktopWindow? WindowAt(Vector2 p)=>desktop.LastOrDefault(w=>StackVisibleJ(w)&&w.Bounds.Contains(p.ToPoint()));
    void ToggleWindow(ViewMode kind)
    {
        context=false;
        if(world.ImportedWorld43&&kind==ViewMode.Map)kind=ViewMode.Galaxy;
        if(world.ImportedWorld43&&world.Docked&&kind==ViewMode.Fitting&&!world.RequireService44(17))return;
        if(world.ImportedWorld43&&kind==ViewMode.Reprocessing&&!world.ServiceAllowed44(5)&&!world.ServiceAllowed44(6)){world.Say("Reprocessing unavailable here.");return;}
        if(kind==ViewMode.ShipInfo){ShowCurrentShipInfo();return;}
        var existing=desktop.FirstOrDefault(w=>w.Kind==kind);
        if(existing!=null)
        {
            if(existing==desktop.Last()&&!existing.Minimized)desktop.Remove(existing);
            else {existing.Minimized=false;desktop.Remove(existing);desktop.Add(existing);}
            SaveDesktop();return;
        }
        EnsureWindow(kind);
    }
    void EnsureWindow(ViewMode kind)
    {
        if(world.ImportedWorld43&&kind==ViewMode.Map)kind=ViewMode.Galaxy;
        if(world.ImportedWorld43&&world.Docked&&kind==ViewMode.Fitting&&!world.RequireService44(17))return;
        if(world.ImportedWorld43&&kind==ViewMode.Reprocessing&&!world.ServiceAllowed44(5)&&!world.ServiceAllowed44(6)){world.Say("Reprocessing unavailable here.");return;}
        if(kind==ViewMode.ShipInfo){ShowCurrentShipInfo();return;}
        var existing=desktop.FirstOrDefault(w=>w.Kind==kind);
        if(existing!=null){existing.Minimized=false;desktop.Remove(existing);desktop.Add(existing);return;}
        if(kind==ViewMode.Drones&&!world.Docked&&!HasFlightDrones)return;
        var r=world.Docked&&kind==ViewMode.Drones?new Rectangle(390,85,840,675):LogicalWindow(kind);

        var w=placements.GetValueOrDefault(kind)??new DesktopWindow{Kind=kind,X=r.X,Y=r.Y,Width=r.Width,Height=r.Height};
        w.Minimized=false;desktop.Add(w);placements[kind]=w;TrackWindowsJ();
    }
    static string LayoutPath=>Path.Combine(Path.GetDirectoryName(DemoWorld.SavePath)!,"desktop-v2.json");
    void LoadDesktop()=>LoadLayoutJ();
    void SaveDesktop()=>SaveLayoutJ();
    bool DesktopInput(MouseState m,KeyboardState keys)
    {
        if(world.Screen==ViewMode.Title)return false;
        var frontJ=WindowAt(mouse);if(frontJ!=null)BindInventoryJ(frontJ);
        if(InputK(m,keys))return true;
        if(context){if(m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed){var popupHit=ui.Hits.LastOrDefault(h=>h.Owner=="popup"&&h.Rect.Contains(mouse.ToPoint()));context=false;popupHit?.Action();}return true;}
        if(ChromeInputJ(m,keys))return true;
        if(movingWindow==null&&!context&&GalaxyInput42(m,keys))return true;
        if(FittingCatalogInput(m,keys))return true;
        if(FitLayoutInput(m,keys))return true;
        if(HandleMarketInput(m))return true;
        bool downNow=m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released;
        bool upNow=m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed;
        bool rightNow=m.RightButton==ButtonState.Pressed&&oldMouse.RightButton==ButtonState.Released;
        var top=WindowAt(mouse);
        var previewHit=ui.Hits.LastOrDefault(h=>h.Tip=="Drag to rotate / wheel to zoom");
        if(!context&&top?.Kind==ViewMode.ShipInfo&&previewHit!=null&&previewHit.Rect.Contains(mouse.ToPoint())){
            if(downNow){shipPreviewDragging=true;shipPreviewLast=mouse;}
            int wheel=m.ScrollWheelValue-oldMouse.ScrollWheelValue;if(wheel!=0)shipPreviewZoom=Math.Clamp(shipPreviewZoom*MathF.Pow(.84f,wheel/120f),.6f,2f);
            if(wheel!=0)return true;
        }
        if(shipPreviewDragging){var delta=mouse-shipPreviewLast;shipPreviewYaw+=delta.X*.01f;shipPreviewPitch=Math.Clamp(shipPreviewPitch+delta.Y*.01f,-1.3f,1.3f);shipPreviewLast=mouse;if(upNow)shipPreviewDragging=false;return true;}
        if(swallowRelease){if(upNow)swallowRelease=false;return true;}
        if(rightNow)
        {
            if(top?.Kind==ViewMode.Information){InformationMenu(top);return true;}
            if(top?.Kind==ViewMode.Catalog){EncyclopediaMenu();return true;}
            var item=itemHits.LastOrDefault(h=>h.Rect.Contains(mouse.ToPoint())&&top?.Kind==ViewMode.Inventory&&h.Owner==top.Owner);
            if(top?.Kind==ViewMode.Fitting&&!fittingWorkshop&&!fleetBrowser){FittingLayoutMenu();return true;}
            if(top?.Kind==ViewMode.Rigs){RigMenu();return true;}
            if(top?.Kind==ViewMode.Special){SpecialMenu();return true;}
            if(top?.Kind==ViewMode.Utilities){UtilityMenu();return true;}
            if(top?.Kind==ViewMode.Mobility){MobilityMenu();return true;}
            if(top?.Kind==ViewMode.Enhancements){EnhancementMenu();return true;}
            if(top?.Kind==ViewMode.Support){SupportMenu();return true;}
            if(top?.Kind==ViewMode.Defense){DefenseMenu();return true;}
            if(top?.Kind==ViewMode.Scanner&&!directionalScanner){ProbeScannerMenu();return true;}
            if(top?.Kind==ViewMode.ProbeMap){ProbeMapMenu();return true;}
            if(top?.Kind==ViewMode.Drones){if(world.Docked)DroneContextMenu();else FlightDroneMenu();return true;}
            if(item!=null)ItemMenu(item);
            else if(top!=null)ShowPopup(WindowTitle(top.Kind),[("Bring to front",()=>{desktop.Remove(top);desktop.Add(top);}),("Minimize",()=>{top.Minimized=true;SaveDesktop();}),("Reset size and position",()=>{var r=LogicalWindow(top.Kind);top.X=r.X;top.Y=r.Y;top.Width=r.Width;top.Height=r.Height;SaveDesktop();}),("Close",()=>desktop.Remove(top))]);
            else SpaceMenu();
            return true;
        }
        if(context)
        {
            if(upNow)
            {
                var h=ui.Hits.LastOrDefault(h=>h.Owner=="popup"&&h.Rect.Contains(mouse.ToPoint()));
                context=false;h?.Action();
            }
            return true;
        }
        if(movingWindow!=null)
        {
            if(m.LeftButton==ButtonState.Pressed)
            {
                var delta=mouse-gestureOrigin;MoveGroupJ(delta);
                if(resizing)
                {
                    var logical=LogicalWindow(movingWindow.Kind);
                    int minW=movingWindow.Kind==ViewMode.ConversationL?660:movingWindow.Kind==ViewMode.JournalL?980:movingWindow.Kind==ViewMode.Opportunities?1000:movingWindow.Kind==ViewMode.OverviewJ?310:movingWindow.Kind is ViewMode.OperationsP or ViewMode.Population or ViewMode.Commander or ViewMode.Production or ViewMode.Exploration or ViewMode.Organisations or ViewMode.Advanced or ViewMode.Experience?1300:movingWindow.Kind==ViewMode.Agents?980:movingWindow.Kind==ViewMode.Information?460:movingWindow.Kind==ViewMode.Catalog?1100:movingWindow.Kind==ViewMode.ProbeMap?600:movingWindow.Kind==ViewMode.Scanner?390:movingWindow.Kind==ViewMode.Drones?280:movingWindow.Kind==ViewMode.Market?1100:movingWindow.Kind==ViewMode.Inventory?490:(int)(logical.Width*.82f);
                    int minH=movingWindow.Kind==ViewMode.ConversationL?745:movingWindow.Kind==ViewMode.JournalL?620:movingWindow.Kind==ViewMode.OverviewJ?180:movingWindow.Kind is ViewMode.OperationsP or ViewMode.Population or ViewMode.Commander or ViewMode.Production or ViewMode.Exploration or ViewMode.Organisations or ViewMode.Advanced or ViewMode.Experience?755:movingWindow.Kind==ViewMode.Agents?720:movingWindow.Kind==ViewMode.Information?360:movingWindow.Kind==ViewMode.Catalog?480:movingWindow.Kind==ViewMode.ProbeMap?420:movingWindow.Kind==ViewMode.Scanner?400:movingWindow.Kind==ViewMode.Drones?180:movingWindow.Kind==ViewMode.Market?650:movingWindow.Kind==ViewMode.Inventory?340:(int)(logical.Height*.82f);
                    movingWindow.Width=Math.Clamp(gestureBounds.Width+(int)delta.X,minW,movingWindow.Kind==ViewMode.Fitting?1544:Math.Max(minW,1600-movingWindow.X));
                    movingWindow.Height=Math.Clamp(gestureBounds.Height+(int)delta.Y,minH,movingWindow.Kind==ViewMode.Fitting?865:Math.Max(minH,900-movingWindow.Y));
                }
                else {movingWindow.X=Math.Clamp(gestureBounds.X+(int)delta.X,movingWindow.Kind==ViewMode.Fitting?80-movingWindow.Width:55,movingWindow.Kind==ViewMode.Fitting?1520:1600-movingWindow.Width);movingWindow.Y=Math.Clamp(gestureBounds.Y+(int)delta.Y,0,movingWindow.Kind==ViewMode.Fitting?872:900-movingWindow.Bounds.Height);}
            }
            if(upNow){SnapWindowJ(movingWindow);movingWindow=null;groupGestureJ.Clear();SaveDesktop();}
            return true;
        }
        if(!world.Docked&&HandleFlightDroneInput(m,keys))return true;
        if(world.Docked&&HandleDroneDrag(m))return true;
        if(itemDrag!=null)
        {
            if(Vector2.Distance(mouse,gestureOrigin)>6)itemDragging=true;
            if(upNow)
            {
                if(itemDragging)DropItem(itemDrag,keys.IsKeyDown(Keys.LeftShift)||keys.IsKeyDown(Keys.RightShift));
                itemDrag=null;dragPayloadJ=null;itemDragging=false;pressedHit=null;
            }
            return true;
        }
        if(top!=null&&m.ScrollWheelValue!=oldMouse.ScrollWheelValue&&top.Kind==ViewMode.Industry)industryScroll=Math.Max(0,industryScroll-(m.ScrollWheelValue-oldMouse.ScrollWheelValue)/120);
        if(top!=null&&m.ScrollWheelValue!=oldMouse.ScrollWheelValue&&top.Kind==ViewMode.Reprocessing)reprocessScroll=Math.Max(0,reprocessScroll-(m.ScrollWheelValue-oldMouse.ScrollWheelValue)/120);
        if(top!=null&&m.ScrollWheelValue!=oldMouse.ScrollWheelValue&&top.Kind==ViewMode.Inventory)
            {inventoryScroll=Math.Max(0,inventoryScroll-(m.ScrollWheelValue-oldMouse.ScrollWheelValue)/120);StoreInventoryJ(top);}
        if(downNow&&top!=null)
        {
            desktop.Remove(top);desktop.Add(top);
            var r=top.Bounds;
            if(mouse.X>=r.Right-24&&mouse.Y<r.Y+28){desktop.Remove(top);SaveDesktop();pressedHit=null;swallowRelease=true;return true;}
            if(mouse.X>=r.Right-49&&mouse.Y<r.Y+28){top.Minimized=!top.Minimized;SaveDesktop();pressedHit=null;swallowRelease=true;return true;}
            if(!top.Locked&&!top.Minimized&&mouse.X>=r.Right-17&&mouse.Y>=r.Bottom-17){movingWindow=top;resizing=true;gestureOrigin=mouse;gestureBounds=top.FullBounds;BeginGroupJ(top,keys);return true;}
            if(!top.Locked&&mouse.Y<r.Y+28){movingWindow=top;resizing=false;gestureOrigin=mouse;gestureBounds=top.FullBounds;BeginGroupJ(top,keys);return true;}
            var item=itemHits.LastOrDefault(h=>h.Rect.Contains(mouse.ToPoint())&&top.Kind==ViewMode.Inventory&&h.Owner==top.Owner);
            if(item!=null){itemDrag=item;dragPayloadJ=new(item.Owner,item.Id,item.Hangar,world.InventoryDisplayCount(item.Id,item.Hangar),world.InventoryMission(item.Id)!=null);gestureOrigin=mouse;selectedItem=item.Id;SelectInventoryK(top,item,keys);return true;}
            pressedHit=ui.Hits.LastOrDefault(h=>h.Owner==top.Owner&&h.Rect.Contains(mouse.ToPoint()));
            return true;
        }
        if(upNow&&pressedHit!=null)
        {
            var h=pressedHit;pressedHit=null;if(h.Rect.Contains(mouse.ToPoint())&&h.Owner==top?.Owner)h.Action();return true;
        }
        return top!=null;
    }
    void DrawDesktop()
    {
        SyncFlightDroneWindow();
        itemHits.Clear();dropHits.Clear();
        foreach(var w in desktop.Where(StackVisibleJ).ToArray())
        {
            ui.OpacityJ=w.Opacity;var r=w.Bounds;ui.Box(new(r.X+5,r.Y+6,r.Width,r.Height),new Color(0,0,0,90));
            string title=w.Kind==ViewMode.Information?(w.InfoKey=="comparison"?"COMPARE INFORMATION":ShortInfo(ResolveInformation(w.InfoKey)?.Name??"Information",Math.Max(15,(w.Width-100)/7))+" : INFORMATION"):WindowTitle(w.Kind);
            ui.Box(r,new Color(11,20,25));ui.Border(r,new Color(82,101,109));
            if(w.Kind is ViewMode.Experience or ViewMode.Advanced or ViewMode.Organisations or ViewMode.Exploration or ViewMode.Production or ViewMode.Commander or ViewMode.Agents or ViewMode.Standings or ViewMode.Opportunities or ViewMode.Industry or ViewMode.Reprocessing or ViewMode.Skills or ViewMode.Scanner or ViewMode.ProbeMap or ViewMode.Information or ViewMode.Catalog)ui.Box(new(r.X+1,r.Y+28,r.Width-2,r.Height-29),new Color(11,20,25));
            ui.Box(new(r.X+1,r.Y+1,r.Width-2,27),w==desktop.Last()?new Color(45,64,73):new Color(28,40,47));
            if(!w.Compact&&w.StackId.Length==0)ui.Text(title,r.X+12,r.Y+7,w==desktop.Last()?Cyan:White,12);else if(w.StackId.Length==0)ui.Text("::",r.X+10,r.Y+7,Muted,12);
            ui.Text("...",r.Right-75,r.Y+7,White,12);
            ui.Text(w.Minimized?"+":"_",r.Right-42,r.Y+6,White,14);ui.Text("X",r.Right-19,r.Y+7,White,12);
            if(w.Minimized){ui.OpacityJ=1;continue;}
            drawingWindow=w;int start=ui.Hits.Count;
            BindInventoryJ(w);ui.Layer(Matrix.Identity,new Rectangle(r.X+1,r.Y+29,r.Width-2,r.Height-30));
            if(IsWindowQ(w.Kind))DrawQ(w);else if(w.Kind==ViewMode.OperationsP)DrawP(w);else if(IsClientWindowK(w.Kind))DrawClientK(w);else if(w.Kind==ViewMode.DesktopSettingsJ)DrawSettingsJ(w);else if(w.Kind==ViewMode.OverviewJ)DrawOverviewK(w);else if(w.Kind==ViewMode.Experience)DrawExperienceI(w);else if(w.Kind==ViewMode.Advanced)DrawAdvancedH(w);else if(w.Kind==ViewMode.Organisations)DrawOrganisationsG(w);else if(w.Kind==ViewMode.Exploration)DrawExplorationF(w);else if(w.Kind==ViewMode.Production)DrawProductionE(w);else if(w.Kind==ViewMode.Commander)DrawCommanderD(w);
            else if(w.Kind==ViewMode.Population)DrawPopulationC(w);
            else if(w.Kind==ViewMode.Agents)DrawAgents38(w);
            else if(w.Kind==ViewMode.Standings)DrawStandings38(w);
            else if(w.Kind==ViewMode.Opportunities)DrawOpportunitiesL(w);
            else if(w.Kind==ViewMode.Industry)DrawIndustry(w);
            else if(w.Kind==ViewMode.Reprocessing)DrawReprocessing(w);
            else if(w.Kind==ViewMode.Galaxy)DrawGalaxy42(w);
            else if(w.Kind==ViewMode.Origins)DrawCharacterOrigin41(w);
            else if(w.Kind==ViewMode.Skills)DrawSkills(w);
            else if(w.Kind==ViewMode.Information)DrawInformation(w);
            else if(w.Kind==ViewMode.Catalog)DrawEncyclopedia(w);
            else if(w.Kind==ViewMode.ProbeMap)DrawProbeMap(w);
            else if(w.Kind==ViewMode.Scanner&&!directionalScanner)DrawProbeScanner(w);
            else if(w.Kind==ViewMode.Drones&&!world.Docked)DrawFlightDrones(w);
            else if(w.Kind==ViewMode.Inventory)DrawInventoryWindow(w);
            else if(w.Kind==ViewMode.Market)DrawMarketWindow(w);
            else
            {
                var original=w.Kind==ViewMode.Scanner&&directionalScanner?LogicalWindow(ViewMode.DirectionalScanner):w.Kind==ViewMode.Drones&&world.Docked?new Rectangle(390,85,840,675):LogicalWindow(w.Kind);
                float sx=(float)r.Width/original.Width,sy=(float)(r.Height-29)/(original.Height-29);
                Matrix transform=Matrix.CreateTranslation(-original.X,-original.Y-29,0)*Matrix.CreateScale(sx,sy,1)*Matrix.CreateTranslation(r.X,r.Y+29,0);
                ui.Mouse=Vector2.Transform(mouse,Matrix.Invert(transform));
                ui.Layer(transform,new(r.X+1,r.Y+29,r.Width-2,r.Height-30));
                switch(w.Kind)
                {
                    case ViewMode.SolarMap:DrawSolarMap();break;case ViewMode.DirectionalScanner:DirectionalScanWindow();break;case ViewMode.Rigs:RigWindow();break;case ViewMode.Special:SpecialWindow();break;case ViewMode.Utilities:UtilityWindow();break;case ViewMode.Mobility:MobilityWindow();break;case ViewMode.Enhancements:EnhancementWindow();break;case ViewMode.Support:SupportWindow();break;case ViewMode.Defense:DefenseWindow();break;case ViewMode.Drones:DroneWindow();break;case ViewMode.Fitting:Fitting();break;case ViewMode.Map:Map();break;
                    case ViewMode.Catalog:Catalog();break;case ViewMode.ShipInfo:ShipInfo();break;case ViewMode.Scanner:Scanner();break;
                    case ViewMode.Hack:Hacking();break;case ViewMode.Decision:Decision();break;case ViewMode.Help:Help();break;
                }
                ui.ResetLayer();ui.Mouse=mouse;
                for(int i=start;i<ui.Hits.Count;i++)
                {
                    var h=ui.Hits[i];var a=Vector2.Transform(new(h.Rect.X,h.Rect.Y),transform);var b=Vector2.Transform(new(h.Rect.Right,h.Rect.Bottom),transform);
                    ui.Hits[i]=h with {Rect=Rectangle.Intersect(new((int)a.X,(int)a.Y,(int)(b.X-a.X),(int)(b.Y-a.Y)),new(r.X,r.Y+29,r.Width,r.Height-29)),Owner=w.Owner};
                }
            }
            ui.ResetLayer();ui.Mouse=mouse;StoreInventoryJ(w);
            var client=new Rectangle(r.X+1,r.Y+29,r.Width-2,r.Height-30);
            for(int i=start;i<ui.Hits.Count;i++){var h=ui.Hits[i];var action=h.Action;ui.Hits[i]=h with {Rect=Rectangle.Intersect(h.Rect,client),Owner=w.Owner,Action=()=>{BindInventoryJ(w);action();StoreInventoryJ(w);}};}
            DrawStackTabsJ(w);
            drawingWindow=null;ui.OpacityJ=1;
            for(int i=0;i<3;i++)ui.Line(new(r.Right-4-i*5,r.Bottom-3),new(r.Right-3,r.Bottom-4-i*5),Muted);
        }
    }
    void ShowPopup(string title,IEnumerable<(string,Action)> actions)
    {
        popupTitle=title;popupActions.Clear();popupActions.AddRange(actions);context=true;
        popupScrollJ=0;int h=36+Math.Min(24,popupActions.Count)*31;popupRect=new(Math.Clamp((int)mouse.X,56,1310),Math.Clamp((int)mouse.Y,0,900-h),280,h);
    }
    void DrawPopup()
    {
        if(!context)return;
        ui.Panel(popupRect,popupTitle.Length>33?popupTitle[..33]:popupTitle);
        for(int i=0;i<Math.Min(24,popupActions.Count-popupScrollJ);i++)
        {
            var action=popupActions[i+popupScrollJ];ui.Button(new(popupRect.X+5,popupRect.Y+30+i*31,270,28),action.Label,()=>{context=false;action.Action();});
            ui.Hits[^1]=ui.Hits[^1] with {Owner="popup"};
        }
    }
    void SpaceMenu()=>OpenSpaceContext();
}
