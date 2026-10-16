using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static StarlightSyndicateDemo.HudCanvas;

namespace StarlightSyndicateDemo;

public sealed partial class DemoGame : Game
{
    readonly GraphicsDeviceManager graphics;
    readonly DemoWorld world = new();
    readonly ProbePuzzle probes = new();
    readonly HackPuzzle hack = new();
    SpaceRenderer renderer = null!;
    HudCanvas ui = null!;
    SpriteBatch present = null!;
    RenderTarget2D surface = null!;
    RenderTarget2D sceneJ = null!;
    KeyboardState oldKeys;
    MouseState oldMouse;
    Vector2 mouse, down;
    bool drag, cameraGesture, context;
    int inventorySelection, catalogSelection, mapSelection;
    double lastClick;
    float time, warpTime;
    string warpLabel = "";
    int overviewTab, frame;
    int captureRetries;
    readonly string? capture;
    readonly string captureScene;
    readonly bool fleetSelftest;
    readonly bool fittingLayoutSelftest;
    readonly bool uiSelftest, marketSelftest, shipStatSelftest, weaponSelftest, ewarSelftest, droneSelftest, fighterSelftest, defenseSelftest, supportSelftest, enhancementSelftest,mobilitySelftest,utilitySelftest,specialSelftest,rigSelftest;
    Rectangle presentRect;
    Rectangle modal = Rectangle.Empty;
    public DemoGame()
    {
        graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1600, PreferredBackBufferHeight = 900, SynchronizeWithVerticalRetrace = true, GraphicsProfile = GraphicsProfile.HiDef };
        Window.Title = "Starlight Syndicate | Stage L01-K06";
        Window.AllowUserResizing = true; IsMouseVisible = true;
        IsFixedTimeStep = true; TargetElapsedTime = TimeSpan.FromSeconds(1.0/60);
        var args=Environment.GetCommandLineArgs();
        int resolution=Array.IndexOf(args,"--resolution");if(resolution>=0&&resolution+2<args.Length&&int.TryParse(args[resolution+1],out int width)&&int.TryParse(args[resolution+2],out int height)&&new[]{"1280x720","1600x900","1920x1080","2560x1440"}.Contains(width+"x"+height)){graphics.PreferredBackBufferWidth=width;graphics.PreferredBackBufferHeight=height;}
        fittingLayoutSelftest=args.Contains("--fitting-layout-ui-test");fleetSelftest=args.Contains("--fleet-ui-test");rigSelftest=args.Contains("--rig-ui-test");specialSelftest=args.Contains("--special-ui-test");utilitySelftest=args.Contains("--utility-ui-test");mobilitySelftest=args.Contains("--mobility-ui-test");enhancementSelftest=args.Contains("--enhancement-ui-test");supportSelftest=args.Contains("--support-ui-test");defenseSelftest=args.Contains("--defense-ui-test");uiSelftest=args.Contains("--ui-test");marketSelftest=args.Contains("--market-test");shipStatSelftest=args.Contains("--ship-stat-test");weaponSelftest=args.Contains("--weapon-ui-test");ewarSelftest=args.Contains("--ewar-ui-test");droneSelftest=args.Contains("--drone-ui-test");fighterSelftest=args.Contains("--fighter-ui-test");
        int index=Array.IndexOf(args,"--screenshot"); if(index>=0 && index+1<args.Length) capture=args[index+1];
        int scene=Array.IndexOf(args,"--scene"); captureScene=scene>=0 && scene+1<args.Length ? args[scene+1] : "space";
    }
    protected override void LoadContent()
    {
        renderer=new SpaceRenderer(GraphicsDevice);ui=new HudCanvas(GraphicsDevice);present=new SpriteBatch(GraphicsDevice);
        surface=new RenderTarget2D(GraphicsDevice,1600,900,false,SurfaceFormat.Color,DepthFormat.Depth24);
        sceneJ=new RenderTarget2D(GraphicsDevice,1600,900,false,SurfaceFormat.Color,DepthFormat.Depth24);
        LoadDesktop();
        var batchArgs=Environment.GetCommandLineArgs();int exportIndex=Array.IndexOf(batchArgs,"--export-ship-thumbnails");
        if(exportIndex>=0){ExportShipThumbnails(batchArgs[exportIndex+1]);Exit();return;}
        Window.TextInput += (_, e)=>{if(QuantityTextK(e.Character))return;if(JTextInput(e.Character))return;if(experienceFocusI.Length>0)ExperienceTextI(e.Character);else if(commanderFocusD.Length>0)CommanderTextD(e.Character);else if(galaxyFocus42)GalaxyText42(e.Character);else if(industryFocus.Length>0)IndustryText(e.Character);else if(skillFocus.Length>0)SkillsText(e.Character);else if(encyclopediaSearchFocus)EncyclopediaText(e.Character);else if(probeTextFocus.Length>0)ProbeScannerText(e.Character);else if(fitSearchFocus)FitText(e.Character);else if(droneGroupEdit)DroneText(e.Character);else MarketText(e.Character);};
        if(capture!=null)
        {
            Start(0);world.EnterGrid("belt");world.Select("raider");world.LockTarget();
            if(captureScene.StartsWith("stage-i-"))SetupExperienceCaptureI();
            if(captureScene.StartsWith("stations44"))SetupStationCapture44();
            if(captureScene.StartsWith("spatial43")){world.NewExploration43(30000001);desktop.Clear();overviewTab=4;if(captureScene=="spatial43-map")EnsureWindow(ViewMode.SolarMap);if(captureScene=="spatial43-info")ShowObjectInfo(world.SolarCelestial);if(captureScene=="spatial43-galaxy")OpenGalaxy42();if(captureScene=="spatial43-landmarks"){OpenGalaxy42();galaxyKind42=3;}if(captureScene=="spatial43-moon"){var moon=world.Celestials.First(c=>c.Kind==CelestialKind.Moon);world.EnterGrid(moon.Id,false);}if(captureScene=="spatial43-secondary"){world.NewExploration43(31002367);world.EnterGrid(world.Celestials.First(c=>c.Kind==CelestialKind.Beacon).Id,false);}}

            if(captureScene.StartsWith("industry")||captureScene=="reprocessing"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.Credits=1000000;world.Skills.Grant(SkillCatalog.All.Select(s=>new SkillRequirement(s.Id,5)));world.SyncSkills();foreach(var m in IndustryCatalog37.Recipes["miner"].Minerals)world.Hangar[m.Key]=10000;EnsureWindow(ViewMode.Industry);industrySelected=world.Industry.Blueprints.First(b=>b.Item=="miner").Id;if(captureScene=="industry-jobs"){world.InstallIndustry(industrySelected,IndustryActivity.Manufacturing,5);industryTab=1;}if(captureScene=="industry-market"){industryTab=3;industrySelected="catalog/miner";}if(captureScene=="industry-research"){industryActivity=IndustryActivity.MaterialResearch;industryTarget=5;}if(captureScene=="reprocessing"){desktop.Clear();world.Hangar["ore"]=1200;reprocessSelection["ore"]=1200;EnsureWindow(ViewMode.Reprocessing);}}
            if(captureScene.StartsWith("skills")){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.Credits=100000;EnsureWindow(ViewMode.Skills);skillGroup="Engineering";world.Skills.Add([new(SkillCatalog.Id("CPU Management"),5),new(SkillCatalog.Id("Capacitor Management"),4)]);world.BuySkillBooks(world.Skills.Queue.Select(q=>q.Skill));world.Skills.Unallocated=10000;if(captureScene=="skills-plans"){SaveQueuePlan();skillTab=1;}if(captureScene=="skills-character")skillTab=2;if(captureScene=="skills-requirements"){ShowItemInfo(FleetCatalog.Source("Paladin").Id);desktop.Last().InfoTab="Requirements";}if(captureScene=="skills-info")OpenSkillInfo(SkillCatalog.Id("CPU Management"));}
            if(captureScene=="flight-drones"){world.Screen=ViewMode.Station;world.Grid="station";world.WeaponTrial("cruiser");world.Hangar["dr_2203"]=5;world.LoadDrone("dr_2203",true,5);world.EnterDroneRange();desktop.Clear();SyncFlightDroneWindow();}
            if(captureScene.StartsWith("solar")||captureScene.StartsWith("warp")||captureScene.StartsWith("space-menu")||captureScene.StartsWith("space-radial")){
                desktop.Clear();world.EnterGrid(captureScene=="solar-gate"?"aster:gate:rook":"station",false);world.SelectedId="aster:planet:3";overviewTab=4;renderer.Distance=renderer.DesiredDistance=6000;
                if(captureScene=="solar-map")EnsureWindow(ViewMode.SolarMap);
                if(captureScene.StartsWith("warp")){world.Player.Cap=world.Player.MaxCap;world.BeginWarp("aster:planet:4");for(int i=0;i<1000&&!world.InWarp;i++)world.Update(.05f);if(world.Transit!=null)world.UpdateSolarPreview(8);}
                if(captureScene=="space-menu"){mouse=new(820,230);OpenSpaceContext();menuPath.Add(8);}
                if(captureScene=="space-radial"){radialActive=true;radialId="aster:planet:3";radialCenter=new(800,400);radialIndex=1;}
            }
            if(captureScene.StartsWith("batch21")||captureScene.StartsWith("batch22")||captureScene.StartsWith("batch23")||captureScene.StartsWith("batch24")||(captureScene.StartsWith("batch25")||(captureScene.StartsWith("batch26")||(captureScene.StartsWith("batch27")||(captureScene.StartsWith("batch28")||captureScene.StartsWith("batch29")))))){
             int idIndex=Array.IndexOf(batchArgs,"--ship-id");string id=idIndex>=0?batchArgs[idIndex+1]:FleetCatalog.Playable[0].Id;
             var profile=ShipBatch21.ById[id];world.Screen=ViewMode.Station;world.Grid="station";world.SystemId=profile.HomeSystem;world.Credits=(captureScene.StartsWith("batch23")||captureScene.StartsWith("batch24")||(captureScene.StartsWith("batch25")||(captureScene.StartsWith("batch26")||(captureScene.StartsWith("batch27")||(captureScene.StartsWith("batch28")||captureScene.StartsWith("batch29"))))))?1000000:100000;desktop.Clear();world.BoardFleetHull(id);
             if(captureScene.Contains("dscan")){world.Undock();directionalScanner=true;directionalResults=world.DirectionalScan();Open(ViewMode.Scanner);}
             else if(captureScene.Contains("traits")){ShowShipInformation(id);shipInfoTab=7;}
             else if(captureScene.Contains("info")){ShowShipInformation(id);shipInfoTab=0;}
             else if(captureScene.Contains("jump")){specialTab=1;Open(ViewMode.Special);}
             else if(captureScene.Contains("bastion")){world.Undock();world.Targets.Clear();world.Player.Cap=world.Player.MaxCap;world.ToggleSpecial(Array.FindIndex(world.Player.Special,s=>s.Definition?.Kind=="Bastion"));world.UpdateSpecial(1.5f);Open(ViewMode.Fitting);}
             else if(captureScene.Contains("ops")){utilityTab=5;Open(ViewMode.Utilities);}
             else if(captureScene.Contains("industry")){utilityTab=3;Open(ViewMode.Utilities);}
             else if(captureScene.Contains("subsystem")){Open(ViewMode.Fitting);subsystemPanel=true;}
             else if(captureScene.Contains("fit"))Open(ViewMode.Fitting);
             else if(captureScene.Contains("registry")){fleetBrowser=true;fleetSelected=id;fleetBatchOnly=true;fleetCurrentBatch=captureScene.StartsWith("batch22")||captureScene.StartsWith("batch23")||captureScene.StartsWith("batch24")||(captureScene.StartsWith("batch25")||(captureScene.StartsWith("batch26")||(captureScene.StartsWith("batch27")||(captureScene.StartsWith("batch28")||captureScene.StartsWith("batch29")))));fleetBattleships=captureScene.StartsWith("batch23");fleetSpecialists=captureScene.StartsWith("batch24");fleetAdvanced=captureScene.StartsWith("batch25");fleetDestroyers=captureScene.StartsWith("batch26");fleetCruisers=captureScene.StartsWith("batch27");fleetStrategic=captureScene.StartsWith("batch28");fleetElite=captureScene.StartsWith("batch29");if(fleetCurrentBatch)fleetFaction=Array.IndexOf(new[]{"All factions"}.Concat(FleetCatalog.All.Select(h=>h.Faction).Distinct()).ToArray(),FleetCatalog.ById[id].Faction);Open(ViewMode.Fitting);}
             else{world.Undock();world.Targets.Clear();}
            }
            if(captureScene.StartsWith("fleet")){
             world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();
             var source=captureScene.Contains("kestrel")?"Kestrel":captureScene.Contains("punisher")?"Punisher":captureScene.Contains("rifter")?"Rifter":"Tristan";
             world.BoardFleetHull(FleetCatalog.Source(source).Id);
             if(captureScene=="fleet"){if(fleetSelftest)fittingWorkshop=true;fleetBrowser=true;fleetSelected=world.Player.CombatHull;Open(ViewMode.Fitting);}
             else if(captureScene.Contains("fitting"))Open(ViewMode.Fitting);
             else{world.Undock();world.Targets.Clear();}
            }
            if(captureScene.StartsWith("fitlayout")){
                world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();
                if(captureScene.Contains("trial")){
                    world.WeaponTrial("battleship");
                    world.FitDefense(DefenseCatalog.All.First(d=>d.Kind=="ShieldBoost"&&d.Size==3).Id,true,1);
                    world.FitMobility(MobilityCatalog.All.First(d=>d.Kind=="AB"&&d.Size==3).Id,true,0);
                    world.FitSupport(SupportCatalog.All.First(d=>d.Kind=="Armor"&&d.Size==2).Id,true,0);
                    world.FitUtility(UtilityCatalog.All.First(d=>d.Kind=="Relic").Id,true,6);
                    world.FitEnhancement(EnhancementCatalog.All.First(d=>d.Slot=="low"&&d.Role.Length==0).Id,true,8);
                    world.FitMobility(MobilityCatalog.All.First(d=>d.Kind=="Nano").Id,true,6);
                    var rig=RigCatalog.All.First(d=>d.Size==3&&d.Family=="Trimark Armor Pump");world.Hangar[rig.Id]=1;world.FitRig(rig.Id,true,0);
                }
                else world.BoardFleetHull(FleetCatalog.Source(captureScene.Contains("kestrel")?"Kestrel":captureScene.Contains("punisher")?"Punisher":captureScene.Contains("rifter")?"Rifter":"Tristan").Id);
                if(captureScene.Contains("wire"))renderer.PreviewWireframe=true;
                if(captureScene.Contains("lod1"))renderer.PreviewLod=1;
                if(captureScene.Contains("lod2"))renderer.PreviewLod=2;
                if(captureScene.Contains("charges"))fitBrowseTab=2;
                EnsureWindow(ViewMode.Fitting);
            }
            if(captureScene=="fitlayout-save"){ToggleFittingSimulation();BeginFitSave();fitDesignName="Border survey design";fitDesignNotes="Probe scanner, light drones and a compact shield fit.";}
            if(captureScene=="fitlayout-inventory"){var fw=desktop.Single(w=>w.Kind==ViewMode.Fitting);fw.X=-330;fw.Y=45;world.Hangar[MobilityCatalog.All.First(d=>d.Kind=="AB"&&d.Size==1).Id]=2;world.Hangar[UtilityCatalog.All.First(d=>d.Kind=="Relic").Id]=1;showHangar=true;inventorySelection=2;EnsureWindow(ViewMode.Inventory);var iw=desktop.Last();iw.X=1010;iw.Y=95;iw.Width=570;iw.Height=650;}
            if(captureScene=="fitting") Open(ViewMode.Fitting);
            if(captureScene is "drones" or "fighters" or "dronebattle" or "fighterbattle"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();bool fighters=captureScene.Contains("fighter");world.WeaponTrial(fighters?"carrier":"battleship");droneTab=2;EnsureWindow(ViewMode.Drones);if(fighters){var d=DroneCatalog.All.First(d=>d.Kind=="AttackFighter");droneType=d.Id;world.LoadDrone(d.Id,true,d.SquadronSize,0);world.EnterDroneRange();world.LaunchDrone(world.DroneSelection);world.SelectedId=world.LockedId="range_0";world.DroneCommand("Engage",world.DroneSelection);droneTab=1;}if(captureScene.EndsWith("battle")){if(!fighters){world.LoadDrone("dr_2203",true,5);world.EnterDroneRange();foreach(var u in world.Player.Drones.Where(u=>u.Location=="Bay").ToArray())world.LaunchDrone(u.Id);world.SelectedId=world.LockedId="range_0";world.DroneCommand("Engage");}world.Update(3);desktop.Clear();renderer.Distance=renderer.DesiredDistance=4300;renderer.Tactical=true;}}
            if(captureScene=="defensebattle"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.DefenseTrial("battleship");foreach(var pair in new[]{("ShieldBoost",1),("Extender",2),("Battery",3),("ArmorRepair",6),("Reactive",7)}){var d=DefenseCatalog.All.First(d=>d.Kind==pair.Item1&&(pair.Item1=="Reactive"||d.Size==3));world.FitDefense(d.Id,true,pair.Item2);}world.FittingDefenseSlot=7;defenseType=world.Player.Defense[7].ModuleId;world.EnterDefenseRange();world.Player.Shield=0;world.Player.Armor*=.7f;world.ToggleDefense(7);world.DefensePressure(350,DamageType.Thermal);world.UpdateDefense(12);world.ToggleDefense(6);world.ToggleDefense(1);EnsureWindow(ViewMode.Defense);}
            if(captureScene=="mobilityfit"||captureScene=="mobilityflight"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.MobilityTrial("battleship");foreach(var d in MobilityCatalog.All.Where(d=>d.Kind is "AB" or "MJD"&&d.Size==3))world.FitMobility(d.Id,true,d.Kind=="AB"?0:1);foreach(var pair in new[]{("Cargo",6),("Nano",7),("Warp",8)})world.FitMobility(MobilityCatalog.All.Single(d=>d.Kind==pair.Item1).Id,true,pair.Item2);mobilityType=MobilityCatalog.All.Single(d=>d.Kind=="MJD"&&d.Size==3).Id;mobilityPage=1;world.FittingMobilitySlot=1;if(captureScene=="mobilityflight"){world.EnterWeaponRange();world.Player.Facing=Vector3.UnitX;world.ToggleMobility(0);mobilityTab=1;}EnsureWindow(ViewMode.Mobility);}
            if(captureScene=="rigs"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.RigTrial();EnsureWindow(ViewMode.Rigs);}
            if(captureScene.StartsWith("special")){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.SpecialTrial("covert");EnsureWindow(ViewMode.Special);if(captureScene=="specialjump")specialTab=1;if(captureScene=="specialclone")specialTab=2;}
            if(captureScene.StartsWith("utility")){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.UtilityTrial("mining_barge");EnsureWindow(ViewMode.Utilities);if(captureScene=="utilityhack"){var analyzer=UtilityCatalog.All.First(d=>d.Kind=="Data");world.FitUtility(analyzer.Id,true,6);world.EnterUtilityRange();world.SelectedId=world.LockedId="ut_Data";world.Player.Position=world.Locked!.Position-new Vector3(100,0,0);world.ToggleUtility(6);world.ClickUtilityHack(1);world.ClickUtilityHack(7);utilityTab=2;}if(captureScene=="utilityscan"){var launcher=UtilityCatalog.All.First(d=>d.Kind=="CoreProbe");world.FitUtility(launcher.Id,true,0);world.LoadUtilityPayload(0,"ut_probe_core");world.EnterUtilityRange();world.UpdateUtility(11);world.ToggleUtility(0);world.UpdateUtility(3);world.AnalyzeUtilityProbes();world.UpdateUtility(11);utilityTab=1;}if(captureScene=="utilityindustry"){world.UtilityTrial("mining_tender");world.FitUtility(UtilityCatalog.All.First(d=>d.Kind=="Compress"&&d.Family=="Asteroid Ore"&&d.Role=="MiningTender").Id,true,0);world.TransferItem("ut_coolant",true,100);world.EnterUtilityRange();world.Player.Ore=100;world.ToggleIndustrialCore();world.ToggleUtility(0);world.CompressUtility("ore",100);utilityTab=3;}}
            if(captureScene=="mobility"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.MobilityTrial("battleship");EnsureWindow(ViewMode.Mobility);}
            if(captureScene=="enhancements"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.EnhancementTrial("battleship");EnsureWindow(ViewMode.Enhancements);}
            if(captureScene=="support"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.SupportTrial(supportSelftest?"command":"logistics");EnsureWindow(ViewMode.Support);}
            if(captureScene=="supportbattle"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.SupportTrial("command");world.HireWingmate("Escort");var rep=SupportCatalog.All.First(d=>d.Kind=="AncArmor"&&d.Size==2);world.FitSupport(rep.Id,true,0);world.LoadSupportPayload(0,"dc_paste");var burst=SupportCatalog.All.First(d=>d.Kind=="Burst"&&d.Family=="Shield");world.FitSupport(burst.Id,true,1);world.LoadSupportPayload(1,SupportPayloads.New.First(p=>p.Family=="Shield").Id);world.EnterSupportRange();world.FleetSelection=world.Wingmates[0].Id;world.SelectedId=world.LockedId=world.FleetSelection;world.SupportPressure(world.FleetSelection);world.ToggleSupport(0);world.ToggleSupport(1);supportType=rep.Id;EnsureWindow(ViewMode.Support);}
            if(captureScene=="defense"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.DefenseTrial("battleship");EnsureWindow(ViewMode.Defense);}
            if(captureScene=="ewar"){fittingWorkshop=true;world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.WeaponTrial("cruiser");fittingEwar=true;EnsureWindow(ViewMode.Fitting);}
            if(captureScene=="ewarfield"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.WeaponTrial("hic");world.FitEwar(EwarCatalog.All.First(e=>e.Kind==EwarKind.Field).Id,true,0);world.EnterEwarRange();world.WeaponSafety=false;world.ToggleEwar(0);renderer.Distance=renderer.DesiredDistance=22000;renderer.Tactical=true;}
            if(captureScene=="weapons"){fittingWorkshop=true;world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();world.WeaponTrial("destroyer");EnsureWindow(ViewMode.Fitting);}
            if(captureScene=="scanner") {world.Screen=ViewMode.Station;world.Grid="station";world.UtilityTrial("covert");var launcher=UtilityCatalog.All.First(d=>d.Kind=="CoreProbe");world.FitUtility(launcher.Id,true,0);world.CargoItems["ut_probe_core"]=8;world.LoadUtilityPayload(0,"ut_probe_core",false);world.EnterUtilityRange();world.UpdateUtility(20);world.LaunchScanProbes();world.UpdateUtility(launcher.Cycle+.1f);world.RefreshProbeResults();var site=world.UtilitySites.First(s=>s.Kind=="Data");world.ProbeSelection=site.Id;world.MoveProbeFormation(new(site.X,site.Y,site.Z));world.SetProbeRange(2);world.AnalyzeUtilityProbes();world.UpdateUtility(world.ProbeScanLeft+.1f);probeMapFocus=world.ProbeCenter;probeMapScale=75;desktop.Clear();directionalScanner=false;OpenProbeMap();}
            if(captureScene=="hack") { hack.Reset(world.Player);Open(ViewMode.Hack); }
            if(captureScene=="map") Open(ViewMode.Map);
            if(captureScene=="market"||captureScene=="marketgroups"||captureScene=="marketships"||captureScene=="marketcompact"||captureScene=="markethistory"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();EnsureWindow(ViewMode.Market);if(captureScene=="marketcompact"){var window=desktop.Last();window.Width=1100;window.Height=650;}if(captureScene=="markethistory"){catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id=="ammo");world.Credits=10000;for(int i=0;i<7;i++){world.Day=i;world.SubmitMarket("ammo",world.SystemId,true,i+1,10+i,0,true);}marketTab=1;}if(captureScene=="marketgroups"){marketGroupPath="Ships / Marauder";marketGroupsView=true;}if(captureScene=="marketships"||captureScene=="marketcompact"){var item=ItemCatalog.All.First(i=>i.Slot=="ship"&&FleetCatalog.ById[i.Id].Class=="Marauder");catalogSelection=Array.IndexOf(ItemCatalog.All,item);marketExpanded.Add("Ships");marketExpanded.Add("Ships / Marauder");marketExpanded.Add(item.Group);}}
            if(captureScene.StartsWith("galaxy42")){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();OpenGalaxy42();if(captureScene=="galaxy42-3d"){galaxyThreeD42=true;FitGalaxy42();}if(captureScene=="galaxy42-all"){galaxyConnection42=0;FitGalaxy42();}}
            if(captureScene=="retheme41"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();OpenCharacterOrigin41();}
            if(captureScene=="items40"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();OpenEncyclopedia("item/sde_587");}
            if(captureScene=="catalog"||captureScene=="encyclopedia"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();OpenEncyclopedia("system/aster");}
            if(captureScene=="dogma45"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();ShowItemInfo(FleetCatalog.Source("Tristan").Id);desktop.Last().X=230;desktop.Last().Y=100;desktop.Last().Width=950;desktop.Last().Height=740;desktop.Last().InfoTab="Mechanics";}
            if(captureScene=="information"||captureScene=="info-system"||captureScene=="info-ship"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();if(captureScene=="information"){ShowItemInfo(MobilityCatalog.All.First(d=>d.Kind=="MWD").Id);var a=desktop.Last();a.X=230;a.Y=140;ShowItemInfo(MobilityCatalog.All.First(d=>d.Kind=="AB").Id);var b=desktop.Last();b.X=860;b.Y=85;b.InfoTab="Attributes";}else if(captureScene=="info-system"){ShowInformation("system/aster");desktop.Last().X=200;desktop.Last().InfoTab="Orbital Bodies";ShowInformation("region/Aster Reach");desktop.Last().X=840;desktop.Last().InfoTab="Solar Systems";}else ShowItemInfo(FleetCatalog.Source("Tristan").Id);}
            if(captureScene=="ship") ShowCurrentShipInfo();
            if(captureScene.StartsWith("shipinfo")){world.Screen=ViewMode.Station;world.BoardFleetHull(FleetCatalog.Source("Tristan").Id);ShowCurrentShipInfo();if(int.TryParse(captureScene[8..],out var tab))shipInfoTab=Math.Clamp(tab,0,6);}
            if(captureScene=="inventory") EnsureWindow(ViewMode.Inventory);
            if(captureScene is "agents38" or "standings38" or "missions38" or "station38"){world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();var contact=AgentCatalog38.Agents.First(a=>a.Station==world.SystemId&&a.Level==1&&a.Division==AgentDivision38.Distribution);world.RequestMission38(contact.Id);agentSelected38=contact.Id;agentDivision38=2;if(captureScene=="agents38")EnsureWindow(ViewMode.Agents);if(captureScene=="missions38"){world.AcceptMission38(world.AgentsState.Missions.Last().Id);EnsureWindow(ViewMode.Opportunities);}if(captureScene=="standings38"){world.ChangeStanding38(contact.Corporation,.1,"Successful contract");world.ChangeStanding38(contact.Id,.2,"Successful contract");standingTab38=1;EnsureWindow(ViewMode.Standings);}}
            if(captureScene.StartsWith("stage-h")){SetupAdvancedCaptureH();}
            if(captureScene.StartsWith("stage-g")){SetupOrganisationCaptureG();}
            if(captureScene.StartsWith("stage-f")){SetupExplorationCaptureF();}
            if(captureScene.StartsWith("stage-e")){SetupProductionCaptureE();}
            if(captureScene.StartsWith("stage-d")){SetupCommanderCaptureD();}
            if(captureScene.StartsWith("stage-c")){SetupPopulationCaptureC();}
            if(captureScene.StartsWith("stage-b")){SetupStageBCapture();}
            if(captureScene.StartsWith("stage-a")){var station=Stations44.Stations.First(s=>s.Services.Contains(7)&&s.Services.Contains(14)&&s.Services.Contains(15));world.NewExploration43(int.Parse(station.System[7..]));world.EnterGrid(station.Id,false);world.StationId=station.Id;world.Grid=station.Id;world.Screen=ViewMode.Station;world.RecoveryStationId=station.Id;world.WalletFunds=1000000;world.EnsureLocationMarket(station.Id);world.Skills.Grant(SkillCatalog.All.Select(s=>new SkillRequirement(s.Id,5)));world.SyncSkills();desktop.Clear();if(captureScene=="stage-a-market")EnsureWindow(ViewMode.Market);if(captureScene=="stage-a-industry"){world.BuyBlueprint("miner");industrySelected=world.Industry.Blueprints.Last().Id;foreach(var input in IndustryCatalog37.Recipes["miner"].Minerals)world.Hangar[input.Key]=10000;EnsureWindow(ViewMode.Industry);}}
            if(captureScene.StartsWith("stage-j"))SetupCaptureJ();
            if(captureScene.StartsWith("stage-k"))SetupCaptureK();
            if(captureScene.StartsWith("stage-l"))SetupCaptureL();
            if(captureScene.StartsWith("stage-m"))SetupCaptureM();
            if(captureScene.StartsWith("stage-n"))SetupCaptureN();
            if(captureScene.StartsWith("stage-q"))SetupCaptureQ();if(captureScene.StartsWith("stage-p"))SetupCaptureP();if(captureScene.StartsWith("stage-o"))SetupCaptureO();
            if(captureScene=="station") {world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();}
            if(captureScene=="desktop") {world.Screen=ViewMode.Station;world.Grid="station";desktop.Clear();Open(ViewMode.Fitting);Open(ViewMode.Inventory);showHangar=true;}
        }
    }
    bool Press(Keys k, KeyboardState state)=>state.IsKeyDown(k)&&!oldKeys.IsKeyDown(k);
    void Start(int origin) { world.NewGame(origin);probes.Reset();hack.Reset();renderer.Reset();context=false;desktop.Clear();EnsureWindow(ViewMode.Inventory); }
    void Open(ViewMode mode) => ToggleWindow(mode);
    void Close() { if(desktop.Count>0)desktop.RemoveAt(desktop.Count-1);context=false;SaveDesktop(); }
    void LoadGame() { if(world.Load(world.ImportedWorld43?DemoWorld.ImportedSavePath43:null)) {desktop.Clear();probes.Reset();hack.Reset();renderer.Reset();RestoreLayoutJ();if(world.SignalResolved)probes.Signal=100;SetResolutionI(world.Experience.Settings.Width,world.Experience.Settings.Height);} }
    void Warp(string grid) { world.BeginWarp(grid,world.DefaultWarpRange);renderer.LookAtTarget=false;context=false; }
    void Jump(string id) { var previous=world.SystemId;world.Jump(id);if(previous!=world.SystemId){warpTime=2.3f;warpLabel="GATE TRANSIT / "+world.Current.Name.ToUpper();renderer.LookAtTarget=false;} }
    void StartHack()
    {
        int utility=Array.FindIndex(world.Player.Utilities,s=>s.Definition?.Analyzer==true);if(utility>=0){world.ToggleUtility(utility);utilityTab=2;Open(ViewMode.Utilities);return;}
        if(world.Selected?.Kind!=TargetKind.Container){world.Say("Select the JoyOS vault at the resolved signal site.");return;}
        if(world.Player.UtilityFit!="analyzer"){world.Say("Dock and fit a data analyzer first.");return;}
        if(Vector3.Distance(world.Player.Position,world.Selected.Position)>2200*world.Player.SupportFactor("analyzerRange")){world.Say("Approach the vault within 2.2 km.");return;}
        hack.Reset(world.Player);Open(ViewMode.Hack);
    }
    protected override void Update(GameTime gameTime)
    {
        if(world.Screen!=ViewMode.Title){skillSyncTimer+=gameTime.ElapsedGameTime.TotalSeconds;skillSaveTimer+=gameTime.ElapsedGameTime.TotalSeconds;if(skillSyncTimer>=1){world.SyncSkills();world.UpdateIndustry(world.Now);world.UpdateMissions38(world.Now);skillSyncTimer=0;}if(skillSaveTimer>=60&&(world.Skills.Queue.Count>0||world.Industry.Jobs.Any(j=>j.State=="Active"))){world.Save();skillSaveTimer=0;}}
        if(!IsActive && capture==null){base.Update(gameTime);return;}
        float dt=(float)Math.Min(.05,gameTime.ElapsedGameTime.TotalSeconds);time+=dt;warpTime=Math.Max(0,warpTime-dt);
        var keys=Keyboard.GetState();var m=Mouse.GetState();
        float scale=Math.Min(GraphicsDevice.PresentationParameters.BackBufferWidth/1600f,GraphicsDevice.PresentationParameters.BackBufferHeight/900f);
        presentRect=new Rectangle((int)((GraphicsDevice.PresentationParameters.BackBufferWidth-1600*scale)/2),(int)((GraphicsDevice.PresentationParameters.BackBufferHeight-900*scale)/2),(int)(1600*scale),(int)(900*scale));
        mouse=new Vector2((m.X-presentRect.X)/scale,(m.Y-presentRect.Y)/scale);ui.Mouse=mouse;
        MarketKeyboard(keys);
        bool consumed=InputK(m,keys);if(!consumed)consumed=InputJ(m,keys);
        if(!consumed)consumed=SkillsInput(m);
        if(!consumed)consumed=InformationInput(m);
        if(!consumed)consumed=ProbeMapInput(m,keys);
        if(!consumed)consumed=SpaceInteractionInput(m,keys,dt);
        if(!consumed)consumed=DesktopInput(m,keys);
        bool onUi=IsOverUi(mouse)||consumed;
        if(m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released)
        {down=mouse;drag=false;cameraGesture=!onUi;}
        if(m.LeftButton==ButtonState.Pressed&&cameraGesture)
        {
            if(Vector2.Distance(mouse,down)>5)drag=true;
            if(drag)renderer.Orbit((m.X-oldMouse.X)/scale,(m.Y-oldMouse.Y)/scale);
        }
        if(m.MiddleButton==ButtonState.Pressed&&!onUi)renderer.Orbit((m.X-oldMouse.X)/scale,(m.Y-oldMouse.Y)/scale);
        if(!onUi&&m.ScrollWheelValue!=oldMouse.ScrollWheelValue)renderer.Zoom(m.ScrollWheelValue-oldMouse.ScrollWheelValue);
        if(!consumed&&m.LeftButton==ButtonState.Released&&oldMouse.LeftButton==ButtonState.Pressed&&!drag)
        {
            var hit=ui.Hits.LastOrDefault(h=>h.Rect.Contains(mouse.ToPoint()));
            if(hit!=null)hit.Action();
            else if(!onUi && world.Screen==ViewMode.Space)
            {
                if(fighterMoveStage>0){FighterPick(mouse);oldMouse=m;oldKeys=keys;base.Update(gameTime);return;}
                bool selected=Pick(mouse);
                if(selected && (keys.IsKeyDown(Keys.LeftControl)||keys.IsKeyDown(Keys.RightControl)))world.LockTarget();
                if(!selected && gameTime.TotalGameTime.TotalSeconds-lastClick<.34)
                {
                    var ray=renderer.PickRay(mouse);world.Player.ManualPoint=ray.Position+ray.Direction*(renderer.Distance+6000);
                    world.Player.Order="manual";world.Player.OrderTarget="";world.Say("Course set in three-dimensional space.");
                }
                lastClick=gameTime.TotalGameTime.TotalSeconds;context=false;
            }
        }
        if(Press(Keys.Escape,keys)&&experienceFocusI.Length>0){experienceFocusI="";}
        else if(Press(Keys.Escape,keys)&&commanderFocusD.Length>0){commanderFocusD="";}
        else if(Press(Keys.Escape,keys)&&industryFocus.Length>0){industryFocus="";}
        else if(Press(Keys.Escape,keys)&&skillFocus.Length>0){skillFocus="";}
        else if(Press(Keys.Escape,keys)&&galaxyFocus42){galaxyFocus42=false;}
        else if(Press(Keys.Escape,keys)&&encyclopediaSearchFocus){encyclopediaSearchFocus=false;}
        else if(Press(Keys.Escape,keys))
        {
            if(fighterMoveStage>0){fighterMoveStage=0;var window=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Drones);if(window!=null)window.Minimized=false;}
            else if(fitSaveDialog){fitSaveDialog=fitSearchFocus=false;}
            else if(fitBuyDialog)fitBuyDialog=false;
            else if(fitSearchFocus)fitSearchFocus=false;
            else if(probeTextFocus.Length>0)probeTextFocus="";
            else if(droneGroupEdit)droneGroupEdit=false;
            else if(tradeDialog!=null){tradeDialog=null;marketTextFocus="";}
            else if(marketTextFocus.Length>0)marketTextFocus="";
            else if(context)context=false;
            else if(desktop.Count>0)Close();
            else if(world.Screen!=ViewMode.Title)world.Screen=ViewMode.Title;
        }
        if(Press(Keys.F8,keys)&&world.Screen!=ViewMode.Title)world.Save();
        if(Press(Keys.F9,keys))LoadGame();
        if(Press(Keys.F11,keys)) {graphics.ToggleFullScreen();}
        if(world.Screen==ViewMode.Title)
        {if(Press(Keys.D1,keys))Start(0);if(Press(Keys.D2,keys))Start(1);if(Press(Keys.D3,keys))Start(2);}
        else if(experienceFocusI.Length==0&&commanderFocusD.Length==0&&industryFocus.Length==0&&skillFocus.Length==0&&marketTextFocus.Length==0&&tradeDialog==null&&!droneGroupEdit&&!fitSearchFocus&&probeTextFocus.Length==0&&!encyclopediaSearchFocus&&!galaxyFocus42)
        {
            if(Press(Keys.Escape,keys)){industryFocus="";commanderFocusD="";}
            if(BoundPressedI("Experience",keys))OpenExperienceI();
            if(Press(Keys.A,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Agents);
            if(Press(Keys.H,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Standings);
            if(Press(Keys.O,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Opportunities);
            if(BoundPressedI("Industry",keys))Open(ViewMode.Industry);
            if(BoundPressedI("Skills",keys))Open(ViewMode.Skills);
            if(Press(Keys.Q,keys))BeginFighterMove();
            if(Press(Keys.R,keys)&&!keys.IsKeyDown(Keys.LeftShift)&&!keys.IsKeyDown(Keys.RightShift)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Rigs);
            if(Press(Keys.Z,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Special);
            if(Press(Keys.E,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Utilities);
            if(Press(Keys.P,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt))&&(keys.IsKeyDown(Keys.LeftShift)||keys.IsKeyDown(Keys.RightShift))){directionalScanner=false;OpenProbeMap();}
            if(Press(Keys.G,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Enhancements);
            if(Press(Keys.C,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))Open(ViewMode.Support);
            if(BoundPressedI("Commander",keys))OpenCommanderD();
            if(Press(Keys.N,keys))Open(ViewMode.Drones);
            if((keys.IsKeyDown(Keys.LeftShift)||keys.IsKeyDown(Keys.RightShift))&&Press(Keys.E,keys))world.DroneCommand("Engage",world.DroneGroupFilter);
            if((keys.IsKeyDown(Keys.LeftShift)||keys.IsKeyDown(Keys.RightShift))&&Press(Keys.R,keys))world.DroneCommand(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)?"Orbit":"Return");
            if(BoundPressedI("Map",keys))OpenWorldMap43();
            if(Press(Keys.S,keys)&&world.Screen!=ViewMode.Station)Open(ViewMode.Scanner);
            if(BoundPressedI("Inventory",keys))Open(ViewMode.Inventory);
            if(BoundPressedI("Market",keys))Open(ViewMode.Market);
            if(Press(Keys.V,keys)&&!keys.IsKeyDown(Keys.LeftAlt)&&!keys.IsKeyDown(Keys.RightAlt))ShowCurrentShipInfo();
            if(Press(Keys.V,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))OpenExplorationF();
            if(Press(Keys.J,keys))Open(ViewMode.Catalog);
            if(Press(Keys.T,keys))renderer.Tactical=!renderer.Tactical;
            if(Press(Keys.F,keys)){if(world.Docked)Open(ViewMode.Fitting);else if(keys.IsKeyDown(Keys.LeftShift)||keys.IsKeyDown(Keys.RightShift))LaunchFlightDrones();else world.DroneCommand("Engage");}
            
            if(Press(Keys.Home,keys))renderer.Reset();
            if(BoundPressedI("Population",keys))OpenPopulationC();
            if(BoundPressedI("Manual",keys))Open(ViewMode.Help);
            if(Press(Keys.F11,keys)&&(keys.IsKeyDown(Keys.LeftAlt)||keys.IsKeyDown(Keys.RightAlt)))EnsureWindow(ViewMode.DesktopSettingsJ);
            if(world.Screen==ViewMode.Space)
            {
                if(Press(Keys.L,keys))world.LockTarget();
            if(Press(Keys.P,keys)&&!keys.IsKeyDown(Keys.LeftAlt)&&!keys.IsKeyDown(Keys.RightAlt))world.SetOrder("approach");if(Press(Keys.O,keys)&&!keys.IsKeyDown(Keys.LeftAlt)&&!keys.IsKeyDown(Keys.RightAlt))world.SetOrder("orbit");
            
            
            if(Press(Keys.K,keys)&&!keys.IsKeyDown(Keys.LeftAlt)&&!keys.IsKeyDown(Keys.RightAlt))world.SetOrder("keep");
                if(Press(Keys.F1,keys))world.ToggleModule(1);if(Press(Keys.F2,keys))world.ToggleModule(2);if(Press(Keys.F3,keys))world.ToggleModule(3);if(Press(Keys.F4,keys))world.ToggleModule(4);if(Press(Keys.F5,keys))world.ToggleEwar(0);if(Press(Keys.F6,keys))world.ToggleEwar(2);if(Press(Keys.F7,keys))world.ToggleEwar(3);
                if(Press(Keys.M,keys)){if(world.Player.UtilityFit=="salvager")world.ToggleSalvager();else world.ToggleMining();}if(Press(Keys.H,keys)&&!keys.IsKeyDown(Keys.LeftAlt)&&!keys.IsKeyDown(Keys.RightAlt))StartHack();if(Press(Keys.D,keys))world.Dock();
                if(Press(Keys.W,keys)&&desktop.LastOrDefault()?.Kind!=ViewMode.Scanner)Warp(world.SelectedId.Length>0&&world.ResolveWarpTarget(world.SelectedId,out _,out _)?world.SelectedId:world.Grid=="station"?"belt":"station");
                if(Press(Keys.Space,keys))world.StopShip();
            }
            if(desktop.LastOrDefault()?.Kind==ViewMode.Scanner)
            {if(Press(Keys.A,keys))world.AnalyzeUtilityProbes();if(Press(Keys.OemPlus,keys))world.SetProbeRange(world.ProbeRange/2);if(Press(Keys.OemMinus,keys))world.SetProbeRange(world.ProbeRange*2);if(Press(Keys.W,keys))world.WarpUtilitySite();}
            if(world.Screen==ViewMode.Station&&Press(Keys.U,keys))world.Undock();
        }
        world.Update(dt);renderer.Update(world,dt);
        oldKeys=keys;oldMouse=m;base.Update(gameTime);
    }
    bool IsOverUi(Vector2 p)
    {
        if(infoSpaceId.Length>0&&new Rectangle(510,210,590,320).Contains(p.ToPoint()))return true;
        if(ui.Hits.Any(h=>h.Rect.Contains(p.ToPoint())))return true;
        if(desktop.Any(w=>w.Bounds.Contains(p.ToPoint())))return true;
        if(world.Screen==ViewMode.Title)return p.X<570;
        return p.X<56||p.X>1245||p.Y>716||p.X<416&&p.Y>650||p.X<377&&p.Y<325;
    }
    bool Pick(Vector2 point)
    {
        SpaceTarget? closest=null;float best=32;
        foreach(var t in world.Targets.Where(t=>t.Alive&&ShowBracketK(t)))
        {
            var p=renderer.Project(t.Position);if(p.Z<0||p.Z>1)continue;
            float d=Vector2.Distance(point,new Vector2(p.X,p.Y));if(d<best){best=d;closest=t;}
        }
        if(closest==null)
        {
            var ray=renderer.PickRay(point);float distance=float.MaxValue;
            foreach(var t in world.Targets.Where(t=>t.Alive))
            {
                float radius=t.Kind==TargetKind.Station?700:t.Kind==TargetKind.Asteroid?250:45;
                float? hit=ray.Intersects(new BoundingSphere(t.Position,radius));
                if(hit.HasValue&&hit.Value<distance){distance=hit.Value;closest=t;}
            }
        }
        if(world.InWarp)return false;
        if(closest==null)return false;world.Select(closest.Id);return true;
    }
    protected override void Draw(GameTime gameTime)
    {
        BeginPresentationI();PrepareInformationPreviews();PrepareShipPreview();PrepareFittingPreview();
        GraphicsDevice.SetRenderTarget(sceneJ);GraphicsDevice.Clear(new Color(3,7,13));
        renderer.Draw(world,world.Experience.Settings.ReducedMotion?0:time,world.Screen is ViewMode.Title or ViewMode.Fitting or ViewMode.Station);
        GraphicsDevice.SetRenderTarget(surface);GraphicsDevice.Clear(new Color(3,7,13));
        GraphicsDevice.SetVertexBuffer(null);GraphicsDevice.Indices=null;
        ui.Begin();ui.Image(sceneJ,new Rectangle(0,0,1600,900));modal=Rectangle.Empty;
        if(world.Screen==ViewMode.Title)Title();
        else
        {
            Rail();Location();Log();DrawExperienceOverlayI();
            ui.Button(new(1300,850,190,25),"EXPERIENCE",OpenExperienceI,"Open experience workspace");
            ui.Button(new(942,850,110,25),"TIME "+world.SimulationSpeed+"x",world.CycleSimulationSpeed);ui.Button(new(1060,850,110,25),world.SimulationPaused?"RESUME":"PAUSE",()=>world.SimulationPaused=!world.SimulationPaused);if(world.Docked)ui.Button(new(1178,850,110,25),"WAIT...",ShowDockedWaitStageA);
            if(world.Docked)StationInterior();
            else {DrawSolarEffects();SolarBrackets();Brackets();Selected();Overview();ShipHud();DroneHud();DrawSpaceUtilities();}
            DrawObjectiveTrackerL();DrawDesktop();DrawSpaceCommands();DrawPopup();DrawTradeDialog();FighterMovePreview();DrawQuantityK();
            ui.Button(new(8,849,45,32),"UI",()=>EnsureWindow(ViewMode.DesktopSettingsJ),"Desktop settings [Alt+F11]");
            if(droneDrag!=null&&droneDragging){ui.Box(new((int)mouse.X+12,(int)mouse.Y+10,245,32),new Color(26,52,61,245));ui.Text(droneDrag.Definition.Name,(int)mouse.X+20,(int)mouse.Y+18,Cyan,12);}
            DrawFittingDrag();
            if(itemDrag!=null&&itemDragging){ui.Box(new((int)mouse.X+12,(int)mouse.Y+10,215,32),new Color(26,52,61,245));ui.Text(world.InventoryDisplayName(itemDrag.Id),(int)mouse.X+20,(int)mouse.Y+18,Cyan,12);}
            if(warpTime>0&&!world.Experience.Settings.ReducedMotion)
            {
                for(int i=0;i<26;i++)
                {float a=i*MathF.Tau/26;var d=new Vector2(MathF.Cos(a),MathF.Sin(a));ui.Line(new Vector2(800,420)+d*180,new Vector2(800,420)+d*(400+warpTime*120),new Color(135,204,229,75),2);}
                ui.Center(warpLabel,800,580,Cyan,18);
            }
        }
        if(tradeDialog==null&&!context&&!itemDragging&&movingWindow==null)ui.Tooltip(WindowAt(mouse)?.Owner??"");ui.End();
        GraphicsDevice.SetRenderTarget(null);GraphicsDevice.Clear(Color.Black);
        present.Begin(samplerState:SamplerState.LinearClamp);present.Draw(surface,presentRect,Color.White);present.End();EndPresentationI();
        if(Environment.GetCommandLineArgs().Contains("--stage-j-ui-test"))RunUiChecksJ(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-k-ui-test"))RunClientUiChecksK(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-l-ui-test"))RunUiChecksL(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-m-ui-test"))RunUiChecksM(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-n-ui-test"))RunUiChecksN(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-q-ui-test"))RunUiQ(frame);if(Environment.GetCommandLineArgs().Contains("--stage-p-ui-test"))RunUiP(frame);if(Environment.GetCommandLineArgs().Contains("--stage-o-ui-test"))RunUiChecksO(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-i-ui-test"))RunExperienceUiChecksI(frame-1);
        if(Environment.GetCommandLineArgs().Contains("--agents-ui-test"))RunAgentsUiChecks38(frame-1);
        if(Environment.GetCommandLineArgs().Contains("--industry-ui-test"))RunIndustryUiChecks37(frame-1);
        if(Environment.GetCommandLineArgs().Contains("--skills-ui-test"))RunSkillsUiChecks36(frame-1);
        if(Environment.GetCommandLineArgs().Contains("--flight-drone-ui-test"))RunFlightDroneUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--solar-ui-test")&&frame==2)RunSolarUiChecks();
        if(Environment.GetCommandLineArgs().Contains("--batch29-registry-ui-test"))RunShipBatch29RegistryUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch29-jump-ui-test"))RunShipBatch29JumpUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch28-ui-test"))RunShipBatch28UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch28-registry-ui-test"))RunShipBatch28RegistryUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch27-scan-ui-test"))RunShipBatch27ScanUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch27-ui-test"))RunShipBatch27UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch26-ui-test"))RunShipBatch26UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch25-ui-test"))RunShipBatch25UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch24-ui-test"))RunShipBatch24UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch23-ui-test"))RunShipBatch23UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch22-ui-test"))RunShipBatch22UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--batch21-ui-test"))RunShipBatch21UiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--galaxy-ui-test"))RunGalaxyUiChecks42(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-a-ui-test"))RunStageAUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-h-ui-test"))RunAdvancedUiChecksH(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-g-ui-test"))RunOrganisationUiChecksG(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-f-ui-test"))RunExplorationUiChecksF(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-e-ui-test"))RunProductionUiChecksE(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-d-ui-test"))RunCommanderUiChecksD(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-c-ui-test"))RunPopulationUiChecksC(frame);
        if(Environment.GetCommandLineArgs().Contains("--stage-b-ui-test"))RunStageBUiChecks(frame);
        if(Environment.GetCommandLineArgs().Contains("--stations-ui-test"))RunStationUiChecks44(frame);
        if(Environment.GetCommandLineArgs().Contains("--spatial-ui-test"))RunSpatialUiChecks43(frame);
        if(Environment.GetCommandLineArgs().Contains("--dogma-ui-test"))RunDogmaUiChecks45(frame);if(Environment.GetCommandLineArgs().Contains("--retheme-ui-test"))RunRethemeUiChecks41(frame);if(Environment.GetCommandLineArgs().Contains("--items-ui-test"))RunItemsUiChecks40(frame);if(Environment.GetCommandLineArgs().Contains("--fitting-planner-ui-test"))RunFittingPlannerUiChecks(frame);if(Environment.GetCommandLineArgs().Contains("--information-ui-test"))RunInformationUiChecks(frame);if(Environment.GetCommandLineArgs().Contains("--probe-ui-test"))RunProbeUiChecks(frame);if(Environment.GetCommandLineArgs().Contains("--model-ui-test"))RunShipModelUiChecks(frame);if(fittingLayoutSelftest)RunFittingLayoutUiChecks(frame);if(fleetSelftest)RunFleetUiChecks(frame);if(rigSelftest)RunRigUiChecks(frame);if(specialSelftest)RunSpecialUiChecks(frame);if(utilitySelftest)RunUtilityUiChecks(frame);if(mobilitySelftest)RunMobilityUiChecks(frame);if(enhancementSelftest)RunEnhancementUiChecks(frame);if(supportSelftest)RunSupportUiChecks(frame);if(defenseSelftest)RunDefenseUiChecks(frame);if(droneSelftest)RunDroneUiChecks(frame);if(fighterSelftest)RunFighterUiChecks(frame);if(ewarSelftest)RunEwarUiChecks(frame);if(weaponSelftest)RunWeaponUiChecks(frame);if(uiSelftest)RunUiChecks(frame);if(marketSelftest)RunMarketChecks(frame);if(shipStatSelftest)RunShipStatUiChecks(frame);
        if(capture!=null && ++frame==(Environment.GetCommandLineArgs().Contains("--stage-q-ui-test")?19:Environment.GetCommandLineArgs().Contains("--stage-p-ui-test")?17:Environment.GetCommandLineArgs().Contains("--stage-o-ui-test")?16:Environment.GetCommandLineArgs().Contains("--stage-n-ui-test")?12:Environment.GetCommandLineArgs().Contains("--stage-m-ui-test")?30:Environment.GetCommandLineArgs().Contains("--stage-l-ui-test")?34:Environment.GetCommandLineArgs().Contains("--stage-k-ui-test")?32:Environment.GetCommandLineArgs().Contains("--stage-j-ui-test")?20:Environment.GetCommandLineArgs().Contains("--stage-i-ui-test")?26:Environment.GetCommandLineArgs().Contains("--stage-h-ui-test")?16:Environment.GetCommandLineArgs().Contains("--stage-g-ui-test")?16:Environment.GetCommandLineArgs().Contains("--stage-f-ui-test")?16:Environment.GetCommandLineArgs().Contains("--stage-e-ui-test")?16:Environment.GetCommandLineArgs().Contains("--stage-d-ui-test")?16:Environment.GetCommandLineArgs().Contains("--stage-c-ui-test")?13:Environment.GetCommandLineArgs().Contains("--stage-b-ui-test")?15:Environment.GetCommandLineArgs().Contains("--stage-a-ui-test")?15:Environment.GetCommandLineArgs().Contains("--stations-ui-test")?14:Environment.GetCommandLineArgs().Contains("--spatial-ui-test")?18:Environment.GetCommandLineArgs().Contains("--galaxy-profile")?120:Environment.GetCommandLineArgs().Contains("--galaxy-ui-test")?21:Environment.GetCommandLineArgs().Contains("--retheme-ui-test")?12:Environment.GetCommandLineArgs().Contains("--agents-ui-test")?13:Environment.GetCommandLineArgs().Contains("--industry-ui-test")?19:Environment.GetCommandLineArgs().Contains("--skills-ui-test")?18:Environment.GetCommandLineArgs().Contains("--fitting-planner-ui-test")?28:Environment.GetCommandLineArgs().Contains("--information-ui-test")?24:((Environment.GetCommandLineArgs().Contains("--batch29-registry-ui-test")||Environment.GetCommandLineArgs().Contains("--batch29-jump-ui-test")||Environment.GetCommandLineArgs().Contains("--batch28-ui-test")||Environment.GetCommandLineArgs().Contains("--batch28-registry-ui-test")||Environment.GetCommandLineArgs().Contains("--batch22-ui-test")||Environment.GetCommandLineArgs().Contains("--batch23-ui-test")||Environment.GetCommandLineArgs().Contains("--batch24-ui-test")||Environment.GetCommandLineArgs().Contains("--batch25-ui-test")||Environment.GetCommandLineArgs().Contains("--batch26-ui-test")||Environment.GetCommandLineArgs().Contains("--batch27-ui-test")||Environment.GetCommandLineArgs().Contains("--batch27-scan-ui-test"))?12:fittingLayoutSelftest?32:fleetSelftest?16:rigSelftest?30:specialSelftest?30:utilitySelftest?29:mobilitySelftest?32:enhancementSelftest?36:supportSelftest?36:defenseSelftest?30:droneSelftest||fighterSelftest?24:uiSelftest||marketSelftest||shipStatSelftest||weaponSelftest||ewarSelftest?14:8)))
        {WriteGalaxyProfile42();using var stream=File.Create(capture);NativePng.Write(surface,stream);stream.Flush(true);if(stream.Length>0)Exit();else if(++captureRetries<4)frame--;else throw new IOException("Screenshot encoder produced an empty image.");}
        base.Draw(gameTime);
    }
    void Title()
    {
        ui.Box(new(0,0,565,900),new Color(5,12,18,230));
        ui.Text("STARLIGHT",62,80,White,42);ui.Text("S Y N D I C A T E",64,139,Cyan,27);
        ui.Text("THE JOYOS INCIDENT",65,211,Gold,16);
        ui.Wrap("A Concord intelligence has crossed the border. The Syndicate of Worlds has nine days to decide what it is willing to contain.",65,250,415,White,17);
        string[] names=["LOST CONCORD PILOT","SYNDICATE SURVEYOR","SYNDICATE SECURITY"];
        string[] descriptions=["A hidden CODEX shipmind. A lost route home.","An ore claim. A signal that should not exist.","A recovery contract. A quarantine deadline."];
        for(int i=0;i<3;i++){int choice=i;ui.Button(new(65,390+i*105,430,45),(i+1)+"   "+names[i],()=>Start(choice));ui.Text(descriptions[i],73,442+i*105,Muted,14);}
        ui.Button(new(65,732,208,38),"CONTINUE / F9",LoadGame);ui.Button(new(287,732,208,38),"EXIT",Exit);
        ui.Button(new(520,781,430,27),"GUIDED PROSPECTOR / ORIGINAL CAMPAIGN",()=>{Start(1);world.ChooseCareerI("Prospector",true);OpenExperienceI();},"Stage I guided start");
        ui.Button(new(520,817,430,27),"GUIDED STARTER SPACE / IMPORTED UNIVERSE",()=>{world.NewExploration43(30000142);world.ChooseCareerI("Explorer",true);probes.Reset();hack.Reset();renderer.Reset();desktop.Clear();OpenExperienceI();},"Stage I starter space");
        ui.Button(new(65,781,430,27),"EXPLORE IMPORTED GALAXY / ORIGINAL BELTS",()=>{world.NewExploration43(30000001);probes.Reset();hack.Reset();renderer.Reset();desktop.Clear();OpenGalaxy42();},"Pass43 imported exploration selection");
        ui.Text("Drag in space to orbit  |  Scroll to zoom",66,818,Muted,14);
        ui.Text("CONCORD STARTER HULL",1030,725,Cyan,14);ui.Text("Original model from your Godot prototype",1030,748,Muted,12);
    }
    void OpenWorldMap43(){if(world.ImportedWorld43)OpenGalaxy42(int.Parse(world.SystemId[7..]));else Open(ViewMode.Map);}
    void Rail()=>RailK();
    void Location()
    {
        ui.Text(world.Current.Name,76,25,White,26);ui.Text(world.Current.Security.ToString("0.0"),76,65,Green,17);
        ui.Text(world.Current.Region+" / "+world.Current.Faction,115,68,Muted,13);
        ui.Text(world.Docked?"DOCKED / HANGAR":world.DockPending?"DOCKING APPROACH":GridCaptionL()+" GRID",76,94,Cyan,13);
        if(world.ImportedWorld43){ui.Panel(new(69,135,306,185),"IMPORTED EXPLORATION");ui.Text("Original belt era / 2019-11-28",82,171,Gold,14);ui.Wrap("Explore real celestial positions. Choose WarpTo in the overview, inspect the local map, or follow a gate route. Dock at imported stations for hangars, fitting, repair and local agents.",82,203,275,White,14);ui.Text($"{world.Credits:0} CR / exploration save",76,337,Muted,12);return;}
        ui.Panel(new(69,135,306,185),"THE JOYOS INCIDENT");
        ui.Text($"Day {(int)world.Day+1}  /  Quarantine on day 9",82,171,world.Day>=3?Red:Gold,15);
        string objective=world.EndingSet?world.Ending:world.Quarantine?"The source has been seized. Freeform flight remains available.":world.SignalHacked?"Return to a station. Decide the recovered intelligence's fate.":world.SignalResolved?"Warp to the signal. Fit an analyzer and recover the JoyOS fragment.":"Trace the rogue Concord intelligence. The lead points to Umbra Knot.";
        ui.Wrap(objective,82,203,275,White,14);
        ui.Text($"{world.Credits:0} CR    /    {world.OriginName}",76,337,Muted,12);
    }
    void Selected()=>DrawSelectedK();
    int overviewScroll;
    IEnumerable<SpaceTarget> VisibleTargets()=>world.Targets.Where(t=>t.Alive&&(overviewTab==0||overviewTab==1&&t.Kind==TargetKind.Raider||overviewTab==2&&t.Kind==TargetKind.Asteroid||overviewTab==3&&t.DroneId.Length>0)).OrderBy(t=>Vector3.DistanceSquared(t.Position,world.Player.Position));
    void Overview()=>SolarOverview();
    void Brackets()
    {
        if(world.Screen!=ViewMode.Space)return;
        foreach(var t in world.Targets.Where(t=>t.Alive&&ShowBracketK(t)))
        {
            var p=renderer.Project(t.Position);if(p.Z<0||p.Z>1||p.X<55||p.X>1245||p.Y<15||p.Y>713)continue;
            var v=new Vector2(p.X,p.Y);Color c=t.Friendly?Green:t.Kind==TargetKind.Raider?Red:t.Kind==TargetKind.Asteroid?Gold:Cyan;
            ui.Hex(v,8,c);if(world.SelectedId==t.Id){ui.Circle(v,19,White);ui.Text(t.Name,p.X+25,p.Y-15,c,13);ui.Text($"{Vector3.Distance(world.Player.Position,t.Position)/1000:0.0} km",p.X+25,p.Y+5,Muted,12);}
            if(world.LockedId==t.Id)ui.Circle(v,25,Red,2);
        }
        DrawTargetsK();
    }
    void ShipHud()=>SolarShipHud();
    void Module(int x,int y,string name,string hotkey,bool on,float cycle,Action action,string tip)
    {
        var p=new Vector2(x,y);ui.Circle(p,31,new Color(48,65,74),5);ui.Circle(p,27,on?Green:Muted,2);
        if(on)ui.Arc(p,34,-MathF.PI/2,MathF.Tau*Math.Clamp(cycle,0,1),Green,3);
        ui.Center(name,x,y-6,on?Green:White,10);ui.Center(hotkey,x,y-49,Muted,11);
        ui.Hits.Add(new(new(x-32,y-32,64,64),action,tip));
    }
    void Log()
    {
        ui.Panel(new(69,659,343,221),"Local / ship log / CODEX");
        var log=world.Log.TakeLast(5).ToArray();
        for(int i=0;i<log.Length;i++)ui.Wrap(log[i].Length>91?log[i][..88]+"...":log[i],81,698+i*34,315,i==log.Length-1?White:Muted,11);
    }
    void DrawWindow(Rectangle r,string title)
    {
        if(drawingWindow!=null)return;
        modal=r;ui.Panel(r,title);ui.Button(new(r.Right-35,r.Y+3,29,22),"X",Close,"Close window [Esc]");
    }
    void Fitting() => DetailedFitting();
    static readonly Dictionary<string,Vector2> mapPoints=new()
    { ["aster"]=new(.32f,.38f),["vale"]=new(.16f,.5f),["rook"]=new(.51f,.35f),["orison"]=new(.36f,.17f),["keel"]=new(.19f,.73f),["saltwake"]=new(.48f,.69f),["umbra"]=new(.73f,.37f),["blackquota"]=new(.77f,.72f),["rim"]=new(.92f,.22f) };
    Vector2 MapPoint(string id)=>new Vector2(443,158)+mapPoints[id]*new Vector2(735,460);
    void Map() => DetailedMap();
    Vector2 ScanPoint(Vector2 p)=>new Vector2(930,365)+p*.0075f;
    void Scanner(){if(directionalScanner)DirectionalScanWindow();else DrawProbeScanner(drawingWindow!);}
    Vector2 NodePoint(int c,int r)=>new(543+c*101+(r%2)*50,255+r*70);
    void Hacking()
    {
        DrawWindow(new(425,138,816,510),"Relic analyzer / JoyOS memory vault");
        ui.Text($"VIRUS COHERENCE {hack.Coherence}     STRENGTH 22",448,186,Cyan,15);
        for(int r=0;r<HackPuzzle.Rows;r++)for(int c=0;c<HackPuzzle.Columns;c++)
        {
            var p=NodePoint(c,r);
            foreach(var n in HackPuzzle.Neighbors(c,r))if(n.X>c||n.Y>r)ui.Line(p,NodePoint(n.X,n.Y),new Color(55,79,77));
        }
        for(int r=0;r<HackPuzzle.Rows;r++)for(int c=0;c<HackPuzzle.Columns;c++)
        {
            int col=c,row=r;var p=NodePoint(c,r);bool reveal=hack.Revealed[c,r];int type=hack.Nodes[c,r];
            Color color=hack.Position==new Point(c,r)?Cyan:!reveal?Muted:type==1?Red:type==2?Green:type==3?Gold:Muted;
            ui.Hex(p,24,color,2);ui.Center(!reveal?"?":type==1?"DEF":type==2?"REP":type==3?"CORE":".",p.X,p.Y-9,color,13);
            ui.Hits.Add(new(new((int)p.X-27,(int)p.Y-27,54,54),()=>hack.Click(col,row,world),reveal&&type==1?"Attack security daemon":"Explore adjacent node"));
        }
        ui.Wrap(hack.Message,450,602,763,hack.Won?Green:White,13);
    }
    void Decision()
    {
        DrawWindow(new(432,149,805,475),"JoyOS / evidence disposition");
        ui.Text("AN INTELLIGENCE REQUESTS ASYLUM",458,195,Gold,22);
        ui.Wrap("CODEX identifies a Concord origin. The Syndicate calls it illegal sovereign cognition. JoyOS asks who is entitled to decide what it becomes.",458,245,744,White,17);
        string[] options=["CONCORD ASYLUM / hide the fragment with CODEX","SYNDICATE CUSTODY / witnessed containment","PUBLIC DISCLOSURE / broadcast the evidence"];
        for(int i=0;i<3;i++){int choice=i+1;ui.Button(new(458,360+i*72,751,50),options[i],()=>world.Decide(choice));}
    }
    void Help()
    {
        DrawWindow(new(434,127,802,507),"Flight controls");
        string[] lines=["Left drag in space: orbit camera / Mouse wheel: zoom","Left click: select / Ctrl-click or L: lock target","Right click: selected-object command menu","Double click empty space: set a course in 3D","P: approach / O: orbit / K: keep range / Space: stop","F1/F2: weapons / F3: booster / F4-F7: EWAR","M: miner / H: analyzer / S: probes / G: star map","F: engage drones in space / Shift+F: launch / I: inventory / B: market / J: item registry","V: ship info / Alt+E: exploration/mining / Alt+C: logistics / T: tactical / Home: camera","D: dock / U: undock / W: warp / F8: save / F9: load","N: drones / Shift+E: engage / Shift+R: recall / Q: fighter move","Scan and hack the Umbra Knot vault; return before day 9."];
        for(int i=0;i<lines.Length;i++)ui.Text(lines[i],457,179+i*35,i>9?Cyan:White,15);
    }
    protected override void UnloadContent(){noticeToneI?.Dispose();SaveDesktop();SavePreferencesK();foreach(var preview in informationPreviews.Values)preview.Dispose();foreach(var thumb in hullThumbnails.Values)thumb.Dispose();fittingPreview?.Dispose();shipPreview?.Dispose();renderer.Dispose();ui.Dispose();sceneJ.Dispose();surface.Dispose();present.Dispose();base.UnloadContent();}
}
