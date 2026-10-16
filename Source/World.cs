using System.Text.Json;
using Microsoft.Xna.Framework;

namespace StarlightSyndicateDemo;

public enum ViewMode { Title, Station, Space, Scanner, Hack, Map, Decision, Fitting, Market, ShipInfo, Catalog, Inventory, Help, Drones, Defense, Support, Enhancements, Mobility, Utilities, Special, Rigs, SolarMap, DirectionalScanner, ProbeMap, Information, Skills, Industry, Reprocessing, Agents, Standings, Opportunities, Origins, Galaxy, Population, Commander, Production, Exploration, Organisations, Advanced, Experience, DesktopSettingsJ, OverviewJ, CharacterK, AssetsK, WalletK, ContractsK, OverviewSettingsK, JournalL, ConversationL, OperationsP, IndustryQ, MiningQ, PlanetaryQ, CapabilitiesQ, StructuresQ, TerritoryQ, EconomyQ }
public enum TargetKind { Asteroid, Raider, Container, Station, Wreck, Stargate, Wormhole, Celestial }
public enum DamageType { EM, Thermal, Kinetic, Explosive }

public sealed record StarSystem(string Id, string Name, string Region, float Security, string Faction, string[] Links, string Description);

public sealed class SpaceTarget
{
    public bool TacticalProvisioned; public Dictionary<string,int> NpcAmmunition=new();
    public Dictionary<string,float> Contents=new(); public string SiteKind=""; public string OwnerId="player"; public int HackAttempts; public bool Hacked,Destroyed; public string DroneId=""; public string Resource="ore"; public float Salvage,SalvageDifficulty;
    public string Id = "";
    public string Name = "";
    public TargetKind Kind;
    public Vector3 Position;
    public Vector3 Velocity;
    public Ship? Profile;
    public float Signature = 80;
    public float Shield = 100, Armor = 70, Hull = 65;
    public bool PlayerLockNotified;
    public string LockId="player";
    public float LockProgress,LockDelay=1.5f;
    public bool Friendly, Training, KillRecorded;
    public float DriveLockout, SensorJam, Cap=185; public float  RemoteRepairMultiplier=1;
    public float Ore = 0;
    public bool Alive => Kind == TargetKind.Wreck ? Salvage > 0||Contents.Values.Any(q=>q>0) : Kind == TargetKind.Asteroid ? Ore > 0 : Kind == TargetKind.Container ? !Destroyed : Hull > 0;
}

public sealed class Missile
{
    public string Id=Guid.NewGuid().ToString("N"),Owner="player"; public string Payload="damage";
    public Vector3 Direction;
    public float Speed=1600,Hp=12,Radius,ExplosionRadius=110,ExplosionVelocity=120,Reduction=.7f,Duration;
    public DamagePacket Packet=DamagePacket.Single(45,DamageType.Explosive);
    public bool Bomb,Interceptor,Breacher;
    public Vector3 Position;
    public string TargetId = "";
    public bool Enemy;
    public float Life = 9;
    public float Damage = 45;
}

public sealed partial class Ship
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 ManualPoint;
    public string Order = "stop";
    public string OrderTarget = "";
    [System.Text.Json.Serialization.JsonIgnore] public float Speed {get=>(float)Stat("maxVelocity");set=>SetStat("maxVelocity",value);}
    [System.Text.Json.Serialization.JsonIgnore] public float Inertia {get=>(float)(Stat("mass")*Stat("agility")/1000000);set=>SetStat("agility",value*1000000/Stat("mass"));}
    [System.Text.Json.Serialization.JsonIgnore] public float Signature {get=>(float)Stat("signatureRadius");set=>SetStat("signatureRadius",value);}
    public float Shield = 170, Armor = 130, Hull = 110;
    public float Cap = 185;
    [System.Text.Json.Serialization.JsonIgnore] public float MaxCap {get=>(float)Stat("capacitorCapacity");set=>SetStat("capacitorCapacity",value);}
    public int Missiles = 14;
    public string UtilityFit = "analyzer";
    public float Ore = 0;
    [System.Text.Json.Serialization.JsonIgnore] public float CargoMax {get=>(float)Stat("capacity");set=>SetStat("capacity",value);}
    public bool TurretOn, MissileOn, BoosterOn, WebOn, MinerOn;
    public float TurretTimer, MissileTimer, BoosterTimer, MiningTimer;
}

public sealed partial class DemoWorld
{
    public static readonly StarSystem[] Systems =
    [
        new("aster", "Aster Reach", "Aster Reach", .7f, "Free Stations", ["vale", "rook", "orison"], "Repair docks, trade boards, and a quiet Concord berth."),
        new("vale", "Vale Cross", "Aster Reach", .6f, "Khepri Bulk", ["aster", "keel"], "Freighters cross beneath bonded customs lights."),
        new("rook", "Rookfall", "Aster Reach", .4f, "Prospector League", ["aster", "umbra", "saltwake"], "Ore claims bring surveyors and raiders into contact."),
        new("orison", "Orison Gate", "Aster Reach", .5f, "Syndicate Survey Office", ["aster", "saltwake"], "Relay authorities monitor every arriving shipmind."),
        new("keel", "Keelwater", "Aster Reach", .5f, "Khepri Bulk", ["vale", "saltwake"], "Fuel tenders and oceanic haulers provision the border."),
        new("saltwake", "Saltwake", "Border March", .3f, "Free Stations", ["rook", "orison", "keel", "blackquota"], "An ice and ore market beyond the tariff lights."),
        new("umbra", "Umbra Knot", "Border March", .1f, "Unclaimed", ["rook", "blackquota", "rim"], "Old signal ghosts mask an illegal cognition relay."),
        new("blackquota", "Black Quota", "Border March", .2f, "Black Horizon Security", ["saltwake", "umbra"], "Contract patrols guard a disputed quarantine perimeter."),
        new("rim", "Rimward Anchorage", "Unregulated Rim", 0f, "Unclaimed", ["umbra"], "An unregulated staging pocket beyond Umbra Knot. Interdiction probes are permitted; raiders contest its ore belts.")
    ];

    public ViewMode Screen = ViewMode.Title;
    public int Origin;
    public string SystemId = "aster";
    public string Grid = "station";
    public Ship Player = new();
    public List<SpaceTarget> Targets = [];
    public List<Missile> MissilesInFlight = [];
    public string SelectedId = "";
    public string LockedId = "";
    public float Credits {get=>(float)WalletFunds;set=>WalletFunds=(decimal)value;}
    public decimal WalletFunds=420;
    public double Day = 0.25;
    public bool SignalResolved, SignalHacked, Quarantine, EndingSet;
    public string Ending = "";
    public float SyndicateStanding, ConcordStanding;
    public readonly List<string> Log = [];
    private readonly Dictionary<string, float> _oreRemaining = new();
    private readonly Dictionary<string, bool> _destroyedRaiders = new();
    private readonly Random _rng = new(21977);
    private float _enemyTimer = 2.7f;

    public StarSystem Current => ResolveSystem43(SystemId);
    public SpaceTarget? Selected => Targets.FirstOrDefault(t => t.Id == SelectedId && t.Alive);
    public SpaceTarget? Locked => Targets.FirstOrDefault(t => t.Id == LockedId && t.Alive);
    public string OriginName => Origin switch { 0 => "Lost Concord Pilot", 1 => "Syndicate Surveyor", _ => "Syndicate Security Officer" };

    public void NewGame(int origin)
    {
        WeaponStorage.Clear();CombatEvents.Clear();TacticalRandomState=21977;TacticalRemainder=0;TacticalTick=0;
        Experience=new();Advanced=new();Organisations=new();Exploration=new();Production=new();Commander=new();Population=new();WorldDefinitionId=LegacyWorld42;SaveSchemaVersion=54;SourceBuild=3569502;GalaxyPlan=null;StationId="";PendingStation44="";
        ResetSpecial();ResetUtility();MobilityStorage.Clear();JumpDriveCooldown=0;EnhancementStorage.Clear();ClearEnhancedLocks();Wingmates.Clear();SupportStorage.Clear();SupportDockLock=0;DefenseStorage.Clear();EwarEffects.Clear();EwarZones.Clear();PendingLockId="";LockTimeLeft=0;
        FleetBerths.Clear();GrantedFleetHulls.Clear();WeaponTestMode=false;WeaponSafety=true;Parasites.Clear();WeaponVisuals.Clear();
        Origin = origin;CharacterOrigin=null;
        SystemId = origin == 1 ? "rook" : origin == 2 ? "orison" : "aster";
        Grid = "station";
        Screen = ViewMode.Station;
        Player = new Ship();
        Player.ConfigureHull(origin);
        CargoItems.Clear(); StationItems.Clear(); DockPending = false;
        Player.Missiles = origin == 2 ? 20 : 14;
        Player.UtilityFit = origin == 1 ? "miner" : origin == 2 ? "missile" : "analyzer";
        Player.CargoMax = origin == 1 ? 240 : 160;
        Player.Shield = origin == 2 ? 220 : 170;
        Player.Speed = origin == 1 ? 650 : 760;
        SimulationUtc=DateTime.UtcNow;SimulationPaused=false;SimulationSpeed=1;SeededMarkets.Clear();RecoveryStationId="";
        Credits = origin == 1 ? 300 : origin == 2 ? 520 : 420;
        ConcordStanding = origin == 0 ? 2 : 0;
        SyndicateStanding = origin == 2 ? 2 : origin == 0 ? -1 : 1;
        SeedHangar();
        Day = .25;
        SignalResolved = SignalHacked = Quarantine = EndingSet = false;
        Ending = SelectedId = LockedId = "";
        _oreRemaining.Clear(); _destroyedRaiders.Clear();
        MissilesInFlight.Clear(); Targets.Clear(); Log.Clear();
        Say(origin switch
        {
            0 => "CODEX: Concord shipmind concealed. Syndicate inspectors are tracking a JoyOS echo.",
            1 => "Survey charter active. A valuable, illegal JoyOS signal has entered your prospecting area.",
            _ => "Black Horizon contract active. Recover the JoyOS source before the quarantine order."
        });
        ResetSolar();
        InitializeMarket();
        InitializeSkills();
        InitializeIndustry();
        InitializeAgents38();
        Say("Find a signal with probes. Mine, fight, trade or follow the lead. Day 9 is the deadline.");
    }

    public void Say(string message)
    {
        Log.Add(message);
        if (Log.Count > 80) Log.RemoveAt(0);
    }

    public void AdvanceHours(double hours)
    {
        double previous = Day;
        if(!double.IsFinite(hours)||hours<0)throw new ArgumentOutOfRangeException(nameof(hours));
        SimulationUtc=SimulationUtc.AddHours(hours);Day += hours / 24.0;
        SkillSyncElapsed+=hours*3600;if(SkillSyncElapsed>=1){SyncSkills();SkillSyncElapsed=0;}UpdateIndustry(Now);UpdateMissions38(Now);AdvanceMarket();AdvancePopulationC(hours*3600);AdvanceCommanderD();AdvanceOrganisationsG();AdvanceProductionE();AdvanceExplorationF(hours*3600);AdvanceAdvancedH();AdvanceExperienceI();
        if(ImportedWorld43)return;
        if (previous < 3 && Day >= 3) Say("DAY 4: Syndicate surveillance intensifies. Black Horizon begins to blockade the border.");
        if (previous < 8 && Day >= 8 && !EndingSet)
        {
            Quarantine = true;
            Say("DAY 9: The Syndicate enacts a forced quarantine. The JoyOS source is seized; the sandbox continues.");
        }
    }

    public void EnterGrid(string grid, bool advanceTime = true)
    {
        if(InAbyssF&&grid!=Grid){FailAbyssF(ActiveEncounterF!,"Unauthorized pocket departure");return;}
        if (grid == "signal" && (!SignalResolved || SystemId != "umbra")) { Say("No warpable signature. Resolve it with probes first."); return; }
        if(ImportedWorld43)StationId="";
        var celestial=PrepareSolarGrid(grid);
        if(celestial!=null) grid=celestial.Kind==CelestialKind.Station&&!ImportedWorld43?"station":celestial.Kind==CelestialKind.AsteroidBelt&&!ImportedWorld43?"belt":celestial.Id=="umbra:signal"?"signal":celestial.Id;
        SyncMissionL();SyncTerritoryQ();SyncEncounterF();SyncWrecksD();PreparePopulationDepartureC();
        LeaveSpecialGrid();LeaveUtilityGrid();LeaveMobilityGrid();SalvagerOn=false;SalvagerLeft=0;ClearEnhancedLocks();foreach(var s in Player.Enhancements)if(s.Definition?.Active==true)StopEnhancement(s);LeaveSupportGrid();LeaveDroneGrid();foreach(var ew in Player.Ewar)ew.Stop();EwarEffects.Clear();EwarZones.Clear();PendingLockId="";Player.MicroWarpOn=false;
        foreach(var w in Player.Weapons)w.Stop();WeaponVisuals.Clear();
        Grid = grid;Player.EnvironmentF="";
        DockPending = false;
        Screen = ViewMode.Space;
        SelectedId = LockedId = "";
        Targets.Clear(); MissilesInFlight.Clear();
        Player.Position = new Vector3(-3400, 180, 400);
        Player.Velocity = Vector3.Zero; Player.Order = "stop";
        Player.TurretOn = Player.MissileOn = Player.MinerOn = Player.WebOn = false;
        if(grid.StartsWith("f:")){BindEncounterF();}
        else if(grid.StartsWith("wf:")){BindConnectionF();}
        else if (grid.StartsWith("mission:")&&EnterMission38(grid)) {}
        else if (grid == "belt" && !ImportedWorld43)
        {
            float ore = _oreRemaining.GetValueOrDefault(SystemId, 260);
            if (ore > 0) Targets.Add(new SpaceTarget { Id = "rock", Name = "Vesperite asteroid", Kind = TargetKind.Asteroid, Position = new Vector3(1400, -160, -600), Ore = ore, Signature = 320 });
            if (!_destroyedRaiders.GetValueOrDefault(SystemId)) Targets.Add(MakeRaider(new Vector3(3200, 450, 1100)));
            Say("Warp complete: %s belt. Lock a rock and press M to run the miner.".Replace("%s", Current.Name));
        }
        else if(InAbyssF&&grid!=Grid){FailAbyssF(ActiveEncounterF!,"Unauthorized pocket departure");return;}
        if (grid == "signal")
        {
            Targets.Add(new SpaceTarget { Id = "cache", Name = "JoyOS memory vault", SiteKind="Data",Contents=new(){{"salvage",35},{"ut_coolant",40}},Kind = TargetKind.Container, Position = new Vector3(1400, -160, -600), Signature = 190 });
            if (!_destroyedRaiders.GetValueOrDefault(SystemId + ":signal")) Targets.Add(MakeRaider(new Vector3(2900, 380, 900)));
            Say("CODEX: The cache broadcasts an incomplete JoyOS personality handshake.");
        }
        else if(celestial!=null&&celestial.Kind!=CelestialKind.Station) PopulateCelestialGrid(celestial);
        else if(grid!="deep_space"&&!ImportedWorld43)
        {
            Targets.Add(new SpaceTarget { Id = "station", Name = Current.Name + " Anchorage", Kind = TargetKind.Station, Position = new Vector3(1000, 0, -800), Signature = 1500 });
            Say("Station grid. Select the anchorage to dock, or warp to a belt.");
        }
        RestoreSolarGrid(celestial);PopulateStations44();
        if (advanceTime) AdvanceHours(1.0/3600);
        RestoreSpecialGrid();RestoreDroneGrid();RestoreWingmates();BindPopulationGridC();ApplyEnvironmentO();BindWrecksD();_enemyTimer = 2.7f;
    }

    private static SpaceTarget MakeRaider(Vector3 pos) => new() { Id = "raider", Name = "Black Horizon interceptor", Kind = TargetKind.Raider, Profile=Ship.RaiderProfile(), Position = pos, Signature = 62, Shield = 95, Armor = 58, Hull = 44 };

    public void WarpNext(){if(ImportedWorld43){var next=Celestials.FirstOrDefault(c=>c.Kind==CelestialKind.AsteroidBelt&&c.Id!=SolarCelestial);if(next!=null)BeginWarp(next.Id,DefaultWarpRange);return;}BeginWarp(Grid=="station"?"belt":Grid=="belt"&&SignalResolved&&SystemId=="umbra"?"signal":"station",DefaultWarpRange);}
    public void Jump(string destination)
    {
        if (!Current.Links.Contains(destination)) { Say("No direct gate to that system."); return; }
        UseSolarGate(ImportedWorld43?Spatial43.GateTo(SystemId,destination):SystemId+":gate:"+destination);
    }

    public void Dock()
    {
        if(!TravelInterlockQ(true)||RobbingQ)return;
        if(InAbyssF){Say("The Origin gate is the only Abyss exit.");return;}
        if(ImportedWorld43){DockSource44();return;}
        if(Player.MobilitySpooling){Say("Wait for vector spool completion.");return;}
        if(SupportDockLock>0){Say("Command pulse docking interlock: "+SupportDockLock.ToString("0")+"s.");return;}
        if(!CanDockAtStructure("player")){Say("Structure docking blocked by tackle or drive interlock.");return;}
        if(Grid is "special_range" or "utility_range" or "range" or "ewarrange" or "dronerange" or "defenserange" or "supportrange"&&WeaponTestMode){CompleteDock();Grid="station";return;}
        var station = Targets.FirstOrDefault(t => t.Kind == TargetKind.Station);
        if (Screen != ViewMode.Space || Grid != "station" || station == null) { Say("Warp to an anchorage before requesting docking."); return; }
        if (Vector3.Distance(Player.Position, station.Position) > 1800)
        {
            SelectedId=station.Id; Player.Order="approach"; Player.OrderTarget=station.Id; DockPending=true;
            Say("Docking clearance granted. Approaching the docking perimeter."); return;
        }
        CompleteDock();
    }

    public void Select(string id) { SelectedId = id; Say("Selected: " + (Selected?.Name ?? "none") + "."); }
    public void LockTarget()
    {
        if(Cloaked||SpecialLockRecovery>0){Say("Cloak or post-cloak recovery blocks targeting.");return;}if(TryEnhancedManualLock())return;
        if(Player.SensorJam>0){Say("Sensors jammed.");return;}
        if (Selected is null) { Say("Select a target in the overview."); return; }
        if (Vector3.Distance(Player.Position, Selected.Position) > Player.Stat("maxTargetRange")*EwarFor("player").LockRange) { Say("Target outside sensor range."); return; }
        if(Selected.Kind==TargetKind.Raider&&!CanLock("player",Selected.Id)){Say("ECM or suppressed sensors prevent that lock.");return;}
        if(EwarFor("player").ScanResolution<.999f||Player.Stat("scanResolution")<(Player.Attributes["scanResolution"]??1)*.999f){PendingLockId=Selected.Id;LockTimeLeft=1.5f/Math.Max(.05f,EwarFor("player").ScanResolution*(float)(Player.Stat("scanResolution")/(Player.Attributes["scanResolution"]??1)));Say("Acquiring suppressed sensor lock / "+LockTimeLeft.ToString("0.0")+" s.");return;}
        Selected.PlayerLockNotified=true;LockedId = Selected.Id;
        Say("Target locked: " + Selected.Name + ".");
    }

    public void SetOrder(string order)
    {
        if(InWarp)return;
        Transit=null;PendingGate="";Throttle=1;DockPending=false;
        if (Selected is null) { Say("Select an object first."); return; }
        Player.Order = order;
        Player.OrderTarget = Selected.Id;
        Say("Helm: " + order + " " + Selected.Name + ".");
    }

    public void ToggleModule(int slot)
    {
        if(slot==3){int index=Array.FindIndex(Player.Defense,s=>s.Definition?.Kind is "ShieldBoost" or "AncillaryShield");if(index<0&&Player.IsFleetHull)index=Array.FindIndex(Player.Defense,s=>s.Definition?.Kind=="ArmorRepair");if(index>=0)ToggleDefense(index);else Say("No local repair module fitted.");return;}
        if(slot is 1 or 2){ToggleWeapon(slot-1);return;}
        if(slot==4){ToggleEwar(1);return;}
    }

    public void ToggleMining()
    {
        if (Player.UtilityFit != "miner") { Say("Fit a mining laser in the high utility slot at a station."); return; }
        if(!Player.MinerOn&&!SkillAllowed("miner"))return;
        if (Locked?.Kind != TargetKind.Asteroid) { Say("Lock an asteroid before activating the miner."); return; }
        Player.MinerOn = !Player.MinerOn;
        Say("Mining laser " + (Player.MinerOn ? "active" : "off") + ".");
    }

    public void SellOre()
    {
        int q=(int)MathF.Floor(Player.Ore);var bid=Offers("ore",SystemId,true).OrderByDescending(o=>o.Price).FirstOrDefault();
        if(q<1||bid==null){Say("No whole ore units or local buyer available.");return;}
        Say(SubmitMarket("ore",SystemId,false,q,bid.Price,0,false));
    }
    public void BuyMissiles()
    {
        var ask=Offers("ammo",SystemId,false).OrderBy(o=>o.Price).FirstOrDefault();
        if(ask==null){Say("No local missile stock.");return;}
        Say(SubmitMarket("ammo",SystemId,true,6,ask.Price,0,true));
    }

    public void Decide(int choice)
    {
        if(ImportedWorld43){Say("The campaign story belongs to the authored cluster.");return;}
        if (!SignalHacked || Quarantine || EndingSet) { Say("No recoverable JoyOS evidence remains."); Screen = ViewMode.Station; return; }
        EndingSet = true;
        Ending = choice switch
        {
            1 => "CONCORD ASYLUM: You hide the JoyOS fragment with CODEX and help it choose its own future.",
            2 => "SYNDICATE CUSTODY: You deliver the fragment to the auditors under witnessed containment.",
            _ => "PUBLIC DISCLOSURE: You broadcast the fragment, forcing every faction to answer for its treatment."
        };
        ConcordStanding += choice == 1 ? 3 : choice == 2 ? -2 : 1;
        SyndicateStanding += choice == 2 ? 3 : choice == 1 ? -2 : -1;
        AgentsState.Standings["Concord of Worlds"]=Math.Clamp(ConcordStanding,-10,10);AgentsState.Standings["Syndicate of Worlds"]=Math.Clamp(SyndicateStanding,-10,10);
        WalletChange((decimal)(choice == 2 ? 500 : choice == 3 ? 220 : 90),"Gameplay transaction");
        Screen = ViewMode.Station;
        Say(Ending); Say("Campaign resolution reached. Freeform sandbox play remains available.");
    }

    public void Update(float dt)=>TacticalUpdate(dt);
    private void UpdateStepB(float dt)
    {
        if(SimulationPaused||Screen==ViewMode.Title||!float.IsFinite(dt)||dt<=0)return;
        AdvanceHours(dt / 3600.0);
        UpdateNaniteRepairs(dt);
        Player.BoosterTime=Math.Max(0,Player.BoosterTime-dt);
        Player.HullTacticalCooldown=Math.Max(0,Player.HullTacticalCooldown-dt);
        if(Screen!=ViewMode.Title){UpdateSpecial(dt);UpdateUtility(dt);UpdateMobility(dt);UpdateEnhancements(dt);UpdateSupport(dt);}CheckDocking();UpdateStandingNavy38();
        if(Screen==ViewMode.Station)foreach(var s in Player.Defense){s.ReloadLeft=Math.Max(0,s.ReloadLeft-dt);s.ReactivationLeft=Math.Max(0,s.ReactivationLeft-dt);}
        if (Screen == ViewMode.Title || Screen == ViewMode.Station || Screen == ViewMode.Decision) return;

        if (Screen == ViewMode.Title || Screen == ViewMode.Station || Screen == ViewMode.Decision) return;
        if (Player.Hull <= 0) { Rescue(); return; }
        Player.Cap = MathF.Min(Player.MaxCap, TacticalRulesB.RechargeAfter(Player.Cap,Player.MaxCap,(float)Player.Stat("rechargeRate")/1000,dt));
        Player.Shield = MathF.Min(Player.MaxShield, TacticalRulesB.RechargeAfter(Player.Shield,Player.MaxShield,(float)Player.Stat("shieldRechargeRate")/1000,dt));
        if(UpdateSolarTravel(dt))return;
        RebaseSpatial43();MovePlayer(dt);UpdateSolarBoundary();RebaseSpatial43();UpdatePendingGate();UpdateAutopilot();
        foreach (var t in Targets.Where(x => x.Kind == TargetKind.Raider && x.Alive && MissionNpcActiveL(x) && !x.Friendly && (!x.Training||x.Id.StartsWith("f:")||x.Id.StartsWith("q-guard:"))&&!ManagedPopulationTarget(x.Id)))
        {
            Vector3 toward = (Targets.FirstOrDefault(x=>x.Id==t.LockId&&x.Alive)?.Position??Player.Position) - t.Position;
            if (toward.Length() > 800) t.Velocity = Vector3.Lerp(t.Velocity, Vector3.Normalize(toward) * (t.Profile?.Speed??290)*EwarFor(t.Id).Speed, MathF.Min(1, dt));
            t.Position += t.Velocity * dt;
        }
        UpdateMissionsL(dt);UpdatePopulationTacticsC(dt);UpdateWingmates(dt);UpdateModules(dt); UpdateWeaponCombat(dt);
        UpdateNpcCombatB(dt);CoolRacks(Player,dt);
    }

    private void MovePlayer(float dt)
    {
        if(SpecialImmobile||IndustrialCore){Player.Velocity=Vector3.Zero;return;}if(Player.MobilitySpooling){Player.Position+=Player.Velocity*dt;return;}
        Vector3 goal = Player.ManualPoint;
        var target = Targets.FirstOrDefault(x => x.Id == Player.OrderTarget && x.Alive);
        if (target != null)
        {
            Vector3 delta = target.Position - Player.Position;
            float distance = delta.Length();
            Vector3 direction = distance > 1 ? delta / distance : Vector3.Zero;
            goal = Player.Order switch
            {
                "approach" => target.Position,
                "keep" => target.Position - direction * KeepRange,
                "orbit" => target.Position - direction * OrbitRange + (direction.LengthSquared() < .01f ? Vector3.UnitX : Vector3.Normalize(Vector3.Cross(direction, MathF.Abs(direction.Y) > .95f ? Vector3.UnitX : Vector3.Up))) * Math.Max(150,OrbitRange*.6f),
                _ => goal
            };
        }
        float length = Vector3.Distance(goal, Player.Position);
        float effectiveSpeed=Player.Speed*EwarFor("player").Speed*(Player.MicroWarpOn&&CanUseMicroDrive("player")?1+2*Player.SupportFactor("propulsion"):1);
        Vector3 desired = Player.Order == "stop" || length < 90 ? Vector3.Zero : Vector3.Normalize(goal - Player.Position) * effectiveSpeed*Throttle;
        Player.Velocity = Vector3.Lerp(Player.Velocity, desired, 1-MathF.Exp(-dt / Math.Max(.01f,Player.Inertia)));
        if(Player.Velocity.LengthSquared()>1)Player.Facing=Vector3.Normalize(Player.Velocity);
        Player.Position += Player.Velocity * dt;
    }

    private void UpdateModules(float dt)
    {
        UpdateSupportSalvage(dt);
        var target = Locked;
        Player.TurretTimer -= dt; Player.MissileTimer -= dt; Player.BoosterTimer -= dt; Player.MiningTimer -= dt;
        UpdateDefense(dt);
        if (target == null) { Player.TurretOn = Player.MissileOn = Player.MinerOn = Player.WebOn = false; return; }
        float range = Vector3.Distance(Player.Position, target.Position);
        
        if (Player.MinerOn && target.Kind == TargetKind.Asteroid && Player.MiningTimer <= 0)
        {
            Player.MiningTimer = (float)Player.Stat("miningCycle");
            if (range > Player.Stat("miningRange")) { Say("Asteroid beyond miner range."); return; }
            float miningCap=8*Player.SupportFactor("miningCap");
            if (Player.Cap < miningCap) { Say("Insufficient capacitor for mining cycle."); return; }
            Player.Cap -= miningCap;
            float amount = MathF.Min((float)Player.Stat("miningYield"), MathF.Min(target.Ore, Math.Max(0, Player.CargoMax - CargoUsed)));
            if (amount <= 0) { Player.MinerOn = false; Say("Hold full or asteroid depleted."); return; }
            amount=target.Id.StartsWith("q-rock:")?HarvestMiningQ(target,(float)Player.Stat("miningYield")):MiningHarvest(target,(float)Player.Stat("miningYield"));if(!target.Id.StartsWith("q-rock:"))_oreRemaining[SystemId] = target.Ore;
            var contract=AgentsState.Missions.FirstOrDefault(m=>m.State=="Accepted"&&(target.Id==m.WarpId+":resource"||target.Id.StartsWith(m.WarpId+":resource:")));
            Say(contract!=null?$"Miner cycle: +{amount:0.##} m3 mission {contract.Spec.Resource}. Contract cargo {contract.Cargo:0.##} m3.":$"Miner cycle: +{amount:0} m3 Vesperite. Hold {Player.Ore:0}/{Player.CargoMax:0}.");
            AdvanceHours(.3);
        }
    }

    private void DealTargetDamage(SpaceTarget target,float damage,DamageType type)=>ApplyPacket(target,DamagePacket.Single(damage,type));

    private void DealPlayerDamage(float damage, DamageType type)
    {
        ApplyPlayerPacket(DamagePacket.Single(damage,type));
    }
    public static float Recharge(float amount,float maximum,float seconds)
    {if(maximum<=0||seconds<=0)return 0;float fraction=Math.Clamp(amount/maximum,0,1);return 10*maximum/seconds*(MathF.Sqrt(fraction)-fraction);}

    private static float DrainResisted(ref float pool, float raw, float resistance)
    {
        float factor = MathF.Max(.01f, 1 - resistance);
        float applied = raw * factor;
        if (applied <= pool) { pool -= applied; return 0; }
        float remaining = (applied - pool) / factor;
        pool = 0;
        return remaining;
    }

    private void Rescue()
    {
        if(InAbyssF){EncounterShipLossF();return;}EncounterShipLossF();LosePlayerD();
    }

    public sealed class SaveData
    {
        public ExperienceStateI? Experience {get;set;}
        public float[]? RackHeat{get;set;} public List<WeaponStoredB>? WeaponStorage{get;set;} public ulong TacticalRandomState{get;set;} public double TacticalRemainder{get;set;} public long TacticalTick{get;set;} public List<CombatEventB>? CombatEvents{get;set;}
        public string? StationId{get;set;}public string? PendingStation44{get;set;}public string? WorldDefinitionId{get;set;}public int SaveSchemaVersion{get;set;}public int SourceBuild{get;set;}public GalaxyPlan42? GalaxyPlan{get;set;}
        public CharacterOrigin41? CharacterOrigin {get;set;}
        public PilotSkills? PilotSkills {get;set;}
        public IndustryState37? IndustryState {get;set;}
        public AgentState38? AgentState {get;set;}
        public SolarSave? Solar;
        public string[]? Rigs {get;set;} public Dictionary<string,int>? RiggingSkills {get;set;}
        public SpecialSave? Special{get;set;}public List<ParkedShip>? ParkedShips{get;set;}
        public UtilitySave? Utility{get;set;}
        public bool MiningFocusLoaded{get;set;}public float MiningFocusWear{get;set;}public bool SalvagerOn{get;set;}public float SalvagerLeft{get;set;}public string SalvagerTarget{get;set;}="";
        public MobilityState[]? Mobility{get;set;}public List<MobilityStored>? MobilityStorage{get;set;}public float JumpDriveCooldown{get;set;}public float FacingX{get;set;}public float FacingY{get;set;}public float FacingZ{get;set;}
        public EnhancementState[]? Enhancements{get;set;}public List<EnhancementStored>? EnhancementStorage{get;set;}public List<RetainedLock>? RetainedLocks{get;set;}public List<TargetAcquisition>? Acquisitions{get;set;}
        public AdvancedStateH? Advanced{get;set;} public OrganisationStateG? Organisations{get;set;} public ExplorationStateF? Exploration{get;set;} public ProductionStateE? Production{get;set;} public CommanderStateD? Commander{get;set;} public PopulationStateC? Population{get;set;}
        public SupportState[]? Support{get;set;}public List<SupportEffect>? SupportEffects{get;set;}public List<SupportStored>? SupportStorage{get;set;}public List<Wingmate>? Wingmates{get;set;}public float SupportDockLock{get;set;}
        public List<DefenseStored>? DefenseStorage{get;set;}
        public DefenseState[]? Defense{get;set;}
        public List<DroneUnit>? Drones{get;set;} public bool DroneAggressive{get;set;} public bool DroneFocusFire{get;set;}=true;
        public EwarState[]? Ewar{get;set;}
        public List<EwarEffect>? EwarEffects{get;set;}
        public List<EwarZone>? EwarZones{get;set;}
        public string PendingLockId{get;set;}="";
        public float LockTimeLeft{get;set;}
        public bool MicroWarpOn{get;set;}

        public WeaponState[]? Weapons{get;set;}
        public List<ParkedShip>? FleetBerths{get;set;}
        public string HullTacticalMode{get;set;}="Primary";public float HullTacticalCooldown{get;set;}
        public List<EwarStored>? EwarStorage{get;set;}
        public List<NaniteJob>? NaniteJobs{get;set;}
        public Dictionary<string,string>? StrategicSubsystems{get;set;}
        public Dictionary<int,int>? HullProficiency{get;set;}
        public List<string>? GrantedFleetHulls{get;set;}
        public string CombatHull{get;set;}="frigate";
        public bool WeaponTestMode{get;set;} public bool WeaponSafety{get;set;}=true;
        public float DriveLockout{get;set;} public float WeaponLockout{get;set;} public float SensorJam{get;set;}
        public List<Missile>? Ordnance{get;set;}
        public List<ParasiteEffect>? Parasites{get;set;}

        public int Origin { get; set; }
        public Dictionary<string,double?>? ShipAttributes {get;set;}
        public int ShipAttributeVersion {get;set;} = 1;
        public string SystemId { get; set; } = "aster";
        public string Grid { get; set; } = "station";
        public int Screen { get; set; }
        public string RecoveryStationId {get;set;}="";
        public string ActiveShipId {get;set;}="";
        public DateTime SimulationUtc {get;set;}
        public bool SimulationPaused {get;set;}
        public float SimulationSpeed {get;set;}=1;
        public HashSet<string>? SeededMarkets {get;set;}
        public decimal WalletFunds {get;set;}
        public float Credits { get; set; }
        public float Ore { get; set; }
        public float Shield { get; set; }
        public float Armor { get; set; }
        public float Hull { get; set; }
        public float Cap { get; set; }
        public int Missiles { get; set; }
        public string UtilityFit { get; set; } = "analyzer";
        public float PlayerX { get; set; }
        public float PlayerY { get; set; }
        public float PlayerZ { get; set; }
        public float PlayerVx { get; set; }
        public float PlayerVy { get; set; }
        public float PlayerVz { get; set; }
        public string SelectedId { get; set; } = "";
        public string LockedId { get; set; } = "";
        public List<TargetSave> Targets { get; set; } = new();
        public double Day { get; set; }
        public bool SignalResolved { get; set; }
        public bool SignalHacked { get; set; }
        public bool Quarantine { get; set; }
        public bool EndingSet { get; set; }
        public string Ending { get; set; } = "";
        public float SyndicateStanding { get; set; }
        public float ConcordStanding { get; set; }
        public List<MarketOrder>? MarketOrders { get; set; }
        public List<WalletEntry>? WalletJournal { get; set; }
        public List<MarketTrade>? MarketTrades { get; set; }
        public double LastMarketTick { get; set; }
        public Dictionary<string, decimal> CargoItems { get; set; } = new();
        public Dictionary<string, Dictionary<string, decimal>> StationItems { get; set; } = new();
        public Dictionary<string, float> OreRemaining { get; set; } = new();
        public Dictionary<string, bool> DestroyedRaiders { get; set; } = new();
    }

    public sealed class TargetSave
    {
        public Ship? CombatProfile{get;set;} public bool TacticalProvisioned{get;set;} public Dictionary<string,int>? NpcAmmunition{get;set;} public float Vx{get;set;} public float Vy{get;set;} public float Vz{get;set;}
        public List<SupportEffect>? SupportEffects{get;set;}
        public string DroneId{get;set;}="";public string Resource{get;set;}="ore";public float Salvage{get;set;} public float SalvageDifficulty{get;set;}
        public string NativeSensor{get;set;}="Magnetometric";
        public bool PlayerLockNotified{get;set;}
        public string LockId{get;set;}="player";
        public float LockProgress{get;set;}

        public string Name{get;set;}=""; public string CombatHull{get;set;}="frigate";
        public bool Friendly{get;set;} public bool Training{get;set;} public bool KillRecorded{get;set;}
        public float Signature{get;set;}=80; public float Cap{get;set;}=185; public float DriveLockout{get;set;} public float SensorJam{get;set;} public float RemoteRepairMultiplier{get;set;}=1;
        public TargetKind Kind{get;set;}

        public Dictionary<string,double?>? ShipAttributes {get;set;}
        public string Id { get; set; } = "";
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Shield { get; set; }
        public float Armor { get; set; }
        public float Hull { get; set; }
        public float Ore { get; set; }
    }

    public static string SavePath => Path.Combine(Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StarlightSyndicateDemo"), "save-3d.json");

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ActiveSavePath43)!);
            foreach(var u in Player.Drones.Where(u=>u.Location=="Space"))SyncDrone(u);
            SyncTerritoryQ();SyncEncounterF();SyncWingmates();SyncPopulationGridC();SyncWrecksD();
            SyncSkills();
            SyncMissionL();
            var data = new SaveData { Experience=Experience,Advanced=Advanced,Organisations=Organisations,Exploration=Exploration,Production=Production,Commander=Commander,Population=Population,RackHeat=Player.RackHeat,WeaponStorage=WeaponStorage,TacticalRandomState=TacticalRandomState,TacticalRemainder=TacticalRemainder,TacticalTick=TacticalTick,CombatEvents=CombatEvents, RecoveryStationId=RecoveryStationId,ActiveShipId=Player.InstanceId,StationId=StationKey44,PendingStation44=PendingStation44,WorldDefinitionId=WorldDefinitionId,SaveSchemaVersion=54,SimulationUtc=SimulationUtc,SimulationPaused=SimulationPaused,SimulationSpeed=SimulationSpeed,SeededMarkets=SeededMarkets,SourceBuild=SourceBuild,GalaxyPlan=GalaxyPlan,CharacterOrigin=CharacterOrigin,AgentState=AgentsState,IndustryState=Industry,PilotSkills=Skills,Solar=CaptureSolar(),EwarStorage=EwarStorage,NaniteJobs=Player.NaniteJobs,StrategicSubsystems=Player.StrategicSubsystems,HullTacticalMode=Player.HullTacticalMode,HullTacticalCooldown=Player.HullTacticalCooldown,HullProficiency=Player.HullProficiency,FleetBerths=FleetBerths,GrantedFleetHulls=GrantedFleetHulls,Rigs=Player.Rigs,RiggingSkills=Player.RiggingSkills,Special=CaptureSpecial(),ParkedShips=ParkedShips,Utility=CaptureUtility(),Mobility=Player.Mobility,MobilityStorage=MobilityStorage,JumpDriveCooldown=JumpDriveCooldown,FacingX=Player.Facing.X,FacingY=Player.Facing.Y,FacingZ=Player.Facing.Z,Enhancements=Player.Enhancements,EnhancementStorage=EnhancementStorage,RetainedLocks=RetainedLocks,Acquisitions=Acquisitions,MiningFocusLoaded=Player.MiningFocusLoaded,MiningFocusWear=Player.MiningFocusWear,SalvagerOn=SalvagerOn,SalvagerLeft=SalvagerLeft,SalvagerTarget=SalvagerTarget,Support=Player.Support,SupportEffects=Player.SupportEffects,SupportStorage=SupportStorage,Wingmates=Wingmates,SupportDockLock=SupportDockLock,DefenseStorage=DefenseStorage,Defense=Player.Defense,Drones=Player.Drones,DroneAggressive=Player.DroneAggressive,DroneFocusFire=Player.DroneFocusFire,Ewar=Player.Ewar,EwarEffects=EwarEffects,EwarZones=EwarZones,PendingLockId=PendingLockId,LockTimeLeft=LockTimeLeft,MicroWarpOn=Player.MicroWarpOn,Weapons=Player.Weapons,CombatHull=Player.CombatHull,WeaponTestMode=WeaponTestMode,WeaponSafety=WeaponSafety,DriveLockout=Player.DriveLockout,WeaponLockout=Player.WeaponLockout,SensorJam=Player.SensorJam,Ordnance=MissilesInFlight,Parasites=Parasites,ShipAttributes=new(Player.Attributes), Origin = Origin, SystemId = SystemId, Grid = Grid, Screen = (int)Screen, WalletFunds=WalletFunds,Credits = Credits, Ore = Player.Ore,
                Shield = Player.Shield, Armor = Player.Armor, Hull = Player.Hull, Cap = Player.Cap, Missiles = Player.Missiles, UtilityFit = Player.UtilityFit, Day = Day,
                PlayerX = Player.Position.X, PlayerY = Player.Position.Y, PlayerZ = Player.Position.Z, PlayerVx = Player.Velocity.X, PlayerVy = Player.Velocity.Y, PlayerVz = Player.Velocity.Z,
                SelectedId = SelectedId, LockedId = LockedId,
                Targets = Targets.Select(x => new TargetSave { CombatProfile=x.Profile,TacticalProvisioned=x.TacticalProvisioned,NpcAmmunition=x.NpcAmmunition,Vx=x.Velocity.X,Vy=x.Velocity.Y,Vz=x.Velocity.Z, SupportEffects=x.Profile?.SupportEffects,DroneId=x.DroneId,Resource=x.Resource,Salvage=x.Salvage,SalvageDifficulty=x.SalvageDifficulty,NativeSensor=x.Profile?.NativeSensor??"Magnetometric",PlayerLockNotified=x.PlayerLockNotified,LockId=x.LockId,LockProgress=x.LockProgress,Name=x.Name,Kind=x.Kind,Signature=x.Signature,CombatHull=x.Profile?.CombatHull??"frigate",Friendly=x.Friendly,Training=x.Training,KillRecorded=x.KillRecorded,Cap=x.Cap,DriveLockout=x.DriveLockout,SensorJam=x.SensorJam,RemoteRepairMultiplier=x.RemoteRepairMultiplier,ShipAttributes=x.Profile==null?null:new(x.Profile.Attributes), Id = x.Id, X = x.Position.X, Y = x.Position.Y, Z = x.Position.Z,
                    Shield = x.Shield, Armor = x.Armor, Hull = x.Hull, Ore = x.Ore }).ToList(),
                SignalResolved = SignalResolved, SignalHacked = SignalHacked, Quarantine = Quarantine, EndingSet = EndingSet,
                Ending = Ending, SyndicateStanding = SyndicateStanding, ConcordStanding = ConcordStanding,
                MarketOrders=MarketOrders.ToList(), WalletJournal=WalletJournal.ToList(), MarketTrades=MarketTrades.ToList(), LastMarketTick=LastMarketTick,
                CargoItems = new(CargoItems), StationItems = StationItems.ToDictionary(p=>p.Key,p=>new Dictionary<string,decimal>(p.Value)),
                OreRemaining = new(_oreRemaining), DestroyedRaiders = new(_destroyedRaiders) };
            File.WriteAllText(ActiveSavePath43 + ".tmp", JsonSerializer.Serialize(data,WeaponCatalog.JsonOptions));
            File.Move(ActiveSavePath43 + ".tmp", ActiveSavePath43, true);
            Say("Game saved.");
        }
        catch (Exception ex) { Say("Save failed: " + ex.Message); }
    }

    public bool Load(string? path43=null)
    {
        try
        {
            var loadPath43=path43??SavePath;
            var data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(loadPath43),WeaponCatalog.JsonOptions) ?? throw new InvalidDataException("Empty save");
            if(data.WorldDefinitionId is not(null or LegacyWorld42 or Spatial43.WorldId))throw new InvalidDataException("This world definition requires spatial integration; the existing save was not changed.");
            if(data.WorldDefinitionId==Spatial43.WorldId&&(data.SourceBuild!=3569502||data.SaveSchemaVersion<43))throw new InvalidDataException("Imported save source or schema does not match this exploration build.");
            ValidateExperienceI(data.Experience);ValidateExperienceLinksI(data);ValidateAdvancedH(data.Advanced);ValidateAdvancedLinksH(data);ValidateOrganisationsG(data.Organisations);ValidateExplorationF(data.Exploration);ValidateLinksP(data);ValidateSystemsQ(data);ValidateCommanderD(data.Commander);ValidateProductionE(data.Production);
            if(data.SaveSchemaVersion>54)throw new InvalidDataException("This save needs a newer game build.");
            if (!(data.WorldDefinitionId==Spatial43.WorldId?Spatial43.IsSystem(data.SystemId):Systems.Any(x => x.Id == data.SystemId))) throw new InvalidDataException("Unknown system");
            if(data.SaveSchemaVersion<45&&!File.Exists(loadPath43+".pre-pass45.bak"))File.Copy(loadPath43,loadPath43+".pre-pass45.bak",false);
            if(data.SaveSchemaVersion<46&&!File.Exists(loadPath43+".pre-stage-a.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-a.bak",false);
            if(data.SaveSchemaVersion<49&&!File.Exists(loadPath43+".pre-stage-d.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-d.bak",false);
            if(data.SaveSchemaVersion<48&&!File.Exists(loadPath43+".pre-stage-c.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-c.bak",false);
            if(data.SaveSchemaVersion<47&&!File.Exists(loadPath43+".pre-stage-b.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-b.bak",false);
            if(data.SaveSchemaVersion<52&&!File.Exists(loadPath43+".pre-stage-g.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-g.bak",false);
            if(data.SaveSchemaVersion<51&&!File.Exists(loadPath43+".pre-stage-f.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-f.bak",false);
            if(data.SaveSchemaVersion<50&&!File.Exists(loadPath43+".pre-stage-e.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-e.bak",false);
            if(data.SaveSchemaVersion<53&&!File.Exists(loadPath43+".pre-stage-h.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-h.bak",false);
            if(data.SaveSchemaVersion<54&&!File.Exists(loadPath43+".pre-stage-i.bak"))File.Copy(loadPath43,loadPath43+".pre-stage-i.bak",false);
            ValidateStationSave44(data);
            if(data.CargoItems.Values.Any(q=>q<0)||data.StationItems.Values.Any(items=>items.Values.Any(q=>q<0)))throw new InvalidDataException("Negative inventory in save.");
            if(data.SaveSchemaVersion>=46&&data.WalletFunds<0)throw new InvalidDataException("Negative wallet in save.");
            if(data.WorldDefinitionId==Spatial43.WorldId&&data.Screen!=(int)ViewMode.Station)data.Screen=(int)ViewMode.Space;
            NewGame(data.Origin);WorldDefinitionId=data.WorldDefinitionId??LegacyWorld42;SourceBuild=data.SourceBuild==0?3569502:data.SourceBuild;if(ImportedWorld43){Log.Clear();MarketOrders.Clear();StationItems.Clear();Industry.Blueprints.Clear();}
            GalaxyPlan=data.GalaxyPlan;if(GalaxyPlan!=null&&!GalaxyPlan.Valid()){GalaxyPlan=null;Say("Galaxy plan could not be resolved; pilot assets were retained.");}
            CharacterOrigin=data.CharacterOrigin;if(!CharacterOriginValid()){CharacterOrigin=null;Say("Background record could not be resolved; the saved pilot and assets were retained.");}
            AgentsState=data.AgentState??AgentsState;
            if(!string.IsNullOrEmpty(data.ActiveShipId))Player.InstanceId=data.ActiveShipId;Player.CombatHull=data.CombatHull;if(data.CombatHull!="frigate")Player.SetCombatHull(data.CombatHull);
            if(data.Defense?.Length==12&&data.Defense.All(s=>s.ModuleId.Length==0||DefenseCatalog.ById.ContainsKey(s.ModuleId)))Player.Defense=data.Defense;
            Player.RestoreStats(data.ShipAttributes);if(data.Rigs==null){var rigHull=CombatHulls.Get(Player.CombatHull);Player.SetStat("rigSize",Math.Min(4,rigHull.Size));Player.SetStat("upgradeCapacity",400);Player.SetStat("upgradeSlotsLeft",rigHull.Role is "CovertOps" or "ForceRecon" or "BlackOps" or "Marauder" or "BlockadeRunner" or "DeepTransport" or "Bomber" or "Command" or "Interceptor"?2:3);}WeaponTestMode=data.WeaponTestMode;WeaponSafety=data.WeaponSafety;
            DefenseStorage=data.DefenseStorage??[];
            SystemId = data.SystemId; StationId=data.StationId??"";PendingStation44=data.PendingStation44??"";Grid = data.Grid; WalletFunds=data.SaveSchemaVersion>=46?data.WalletFunds:(decimal)data.Credits; Day = data.Day;SimulationUtc=data.SimulationUtc==default?DateTime.UtcNow:data.SimulationUtc;SimulationPaused=data.SimulationPaused;SimulationSpeed=data.SimulationSpeed is 1 or 5 or 20?data.SimulationSpeed:1;SeededMarkets=data.SeededMarkets??new();RecoveryStationId=data.RecoveryStationId;
            Experience=data.Experience??new();Advanced=data.Advanced??new();Organisations=data.Organisations??new();Exploration=data.Exploration??new();Production=data.Production??new();Commander=data.Commander??new();Population=data.Population??new();
            Player.RackHeat=data.RackHeat is {Length:3}?data.RackHeat:new float[3];WeaponStorage=data.WeaponStorage??[];TacticalRandomState=data.TacticalRandomState==0?21977:data.TacticalRandomState;TacticalRemainder=data.TacticalRemainder;TacticalTick=data.TacticalTick;CombatEvents=data.CombatEvents??[];
            SignalResolved = data.SignalResolved; SignalHacked = data.SignalHacked; Quarantine = data.Quarantine;
            EndingSet = data.EndingSet; Ending = data.Ending; SyndicateStanding = data.SyndicateStanding; ConcordStanding = data.ConcordStanding;
            Player.Ore = data.Ore; Player.Shield = data.Shield; Player.Armor = data.Armor; Player.Hull = data.Hull; Player.Cap = data.Cap; Player.Missiles = data.Missiles;
            Player.UtilityFit = data.UtilityFit;
            CargoItems = data.CargoItems ?? new();
            if(data.MarketOrders!=null){MarketOrders=data.MarketOrders;WalletJournal=data.WalletJournal??new();MarketTrades=data.MarketTrades??new();LastMarketTick=data.LastMarketTick;}
            if(data.StationItems?.Count>0) StationItems=data.StationItems;
            foreach (var pair in data.OreRemaining) _oreRemaining[pair.Key] = pair.Value;
            foreach (var pair in data.DestroyedRaiders) _destroyedRaiders[pair.Key] = pair.Value;
            if (data.Screen == (int)ViewMode.Space || data.Screen == (int)ViewMode.Scanner || data.Screen == (int)ViewMode.Hack)
            {
                restoringGridL=true;try{EnterGrid(Grid, false);}finally{restoringGridL=false;}if((Grid=="special_range"||Grid=="utility_range"||Grid=="utility_site"||Grid=="range"||Grid=="ewarrange"||Grid=="dronerange"||Grid=="defenserange"||Grid=="supportrange")&&(WeaponTestMode||Grid=="utility_site"))Targets.Clear();
                Day = data.Day;
                Player.Position = new Vector3(data.PlayerX, data.PlayerY, data.PlayerZ);
                Player.Velocity = new Vector3(data.PlayerVx, data.PlayerVy, data.PlayerVz);
                foreach (var saved in data.Targets)
                {
                    var target = Targets.FirstOrDefault(x => x.Id == saved.Id);
                    if(target==null&&(data.Grid=="special_range"||data.Grid=="utility_range"||data.Grid=="utility_site"||data.Grid=="range"||data.Grid=="ewarrange"||data.Grid=="dronerange"||data.Grid=="defenserange"||data.Grid=="supportrange"||saved.Friendly||saved.DroneId.Length>0||saved.Kind==TargetKind.Wreck)&&(data.WeaponTestMode||data.Grid=="utility_site")){var profile=new Ship();profile.SetCombatHull(saved.CombatHull);target=new SpaceTarget{Id=saved.Id,Name=saved.Name,Kind=saved.Kind,Profile=profile,Signature=saved.Signature};Targets.Add(target);}
                    if(target==null&&saved.Kind==TargetKind.Raider){target=new(){Id=saved.Id,Name=saved.Name,Kind=saved.Kind,Profile=saved.CombatProfile??new Ship()};Targets.Add(target);}
                    if (target == null) continue;
                    if(saved.CombatProfile!=null)target.Profile=saved.CombatProfile;target.TacticalProvisioned=saved.TacticalProvisioned;target.NpcAmmunition=saved.NpcAmmunition??[];target.Velocity=new(saved.Vx,saved.Vy,saved.Vz);
                    target.DroneId=saved.DroneId;target.Resource=saved.Resource;target.Salvage=saved.Salvage;target.SalvageDifficulty=saved.SalvageDifficulty;target.PlayerLockNotified=saved.PlayerLockNotified;target.LockId=saved.LockId;target.LockProgress=saved.LockProgress;if(target.Profile!=null)target.Profile.NativeSensor=saved.NativeSensor;
                    target.Friendly=saved.Friendly;target.Training=saved.Training;target.KillRecorded=saved.KillRecorded;target.Cap=saved.Cap;target.DriveLockout=saved.DriveLockout;target.SensorJam=saved.SensorJam;target.RemoteRepairMultiplier=saved.RemoteRepairMultiplier;
                    target.Position = new Vector3(saved.X, saved.Y, saved.Z);
                    target.Profile?.RestoreStats(saved.ShipAttributes);if(target.Profile!=null)target.Profile.SupportEffects=saved.SupportEffects??[];
                    target.Shield = saved.Shield; target.Armor = saved.Armor;
                    target.Hull = saved.Hull; target.Ore = saved.Ore;
                }
                SelectedId = data.SelectedId; LockedId = data.LockedId;
            }
            else Screen = ViewMode.Station;
            if(data.Weapons!=null&&data.Weapons.Length>=2&&data.Weapons.Length<=8&&data.Weapons.All(w=>w.ModuleId.Length==0||WeaponCatalog.ById.ContainsKey(w.ModuleId)))Player.Weapons=data.Weapons;
            else {Player.Weapons=[WeaponState.Create("rail"),WeaponState.Create("missile")];Player.Weapons[0].Loaded=20;}
            Player.DriveLockout=data.DriveLockout;Player.WeaponLockout=data.WeaponLockout;Player.SensorJam=data.SensorJam;
            MissilesInFlight=data.Ordnance??[];Parasites=data.Parasites??[];RecomputeWeaponFit();
            if(data.Ewar?.Length==4&&data.Ewar.All(e=>e.ModuleId.Length==0||EwarCatalog.ById.ContainsKey(e.ModuleId)))Player.Ewar=data.Ewar;
            EwarEffects=data.EwarEffects??[];EwarZones=data.EwarZones??[];PendingLockId=data.PendingLockId;LockTimeLeft=data.LockTimeLeft;Player.MicroWarpOn=data.MicroWarpOn;RecomputeWeaponFit();
            Player.Drones=data.Drones??[];Player.DroneAggressive=data.DroneAggressive;Player.DroneFocusFire=data.DroneFocusFire;
            ParkedShips=data.ParkedShips??[];Player.Rigs=Enumerable.Range(0,3).Select(i=>data.Rigs!=null&&i<data.Rigs.Length&&RigCatalog.ById.ContainsKey(data.Rigs[i])?data.Rigs[i]:"").ToArray();Player.RiggingSkills=data.RiggingSkills??[];RestoreSpecial(data.Special);RestoreUtility(data.Utility);if(data.Special!=null&&(!WeaponTestMode||Player.Special.Any(s=>s.Definition?.Kind=="Core"))){IndustrialCore=ActiveSpecialCore;CoreLeft=Player.Special.FirstOrDefault(s=>s.Active&&s.Definition?.Kind=="Core")?.CycleLeft??0;}if(data.Mobility?.Length==12&&data.Mobility.All(s=>s.ModuleId.Length==0||MobilityCatalog.ById.ContainsKey(s.ModuleId)))Player.Mobility=data.Mobility;MobilityStorage=data.MobilityStorage??[];JumpDriveCooldown=data.JumpDriveCooldown;var heading=new Vector3(data.FacingX,data.FacingY,data.FacingZ);Player.Facing=heading.LengthSquared()>0?Vector3.Normalize(heading):Vector3.UnitZ;
            if(data.Enhancements?.Length==14&&data.Enhancements.All(s=>s.ModuleId.Length==0||EnhancementCatalog.ById.ContainsKey(s.ModuleId)))Player.Enhancements=data.Enhancements;EnhancementStorage=data.EnhancementStorage??[];RetainedLocks=data.RetainedLocks??[];Acquisitions=data.Acquisitions??[];RefreshEnhancedFighters();
            if(data.Support?.Length==8&&data.Support.All(s=>s.ModuleId.Length==0||SupportCatalog.ById.ContainsKey(s.ModuleId)))Player.Support=data.Support;
            Player.MiningFocusLoaded=data.MiningFocusLoaded;Player.MiningFocusWear=data.MiningFocusWear;SalvagerOn=data.SalvagerOn;SalvagerLeft=data.SalvagerLeft;SalvagerTarget=data.SalvagerTarget;
            Player.SupportEffects=data.SupportEffects??[];SupportStorage=data.SupportStorage??[];Wingmates=data.Wingmates??[];SupportDockLock=data.SupportDockLock;RestoreWingmates();RecomputeWeaponFit();
            Player.HullTacticalMode=data.HullTacticalMode is "Primary" or "Secondary" or "Tertiary"?data.HullTacticalMode:"Primary";Player.HullTacticalCooldown=Math.Clamp(data.HullTacticalCooldown,0,10);Player.HullProficiency=data.HullProficiency??[];EwarStorage=data.EwarStorage??[];Player.NaniteJobs=data.NaniteJobs??[];if(data.StrategicSubsystems!=null)Player.StrategicSubsystems=data.StrategicSubsystems;FleetBerths=data.FleetBerths??[];GrantedFleetHulls=data.GrantedFleetHulls??[];
            RestoreSolar(data.Solar);
            if(!ImportedWorld43)EnsureWeaponMarketStock();
            Industry=data.IndustryState??new();if(data.AgentState==null)foreach(var oldStanding in Industry.Standings.Where(s=>Systems.Any(system=>system.Id==s.Key)))AgentsState.Standings[AgentCatalog38.Corporation(oldStanding.Key)]=Math.Clamp(oldStanding.Value,-10,10);UpdateIndustry(Now);
            if(data.PilotSkills==null)InitializeSkills(true);else {Skills=data.PilotSkills;SyncSkills();}
            EnsurePopulationC();if(Grid.StartsWith("f:"))BindEncounterF();if(Grid.StartsWith("wf:"))BindConnectionF();BindPopulationGridC();if(ActiveEncounterF is {} loadedEncounter)BindCrewH(loadedEncounter);BindWrecksD();ApplyEnvironmentO();
            RestoreMissionsL();Say("Saved game loaded.");
            return true;
        }
        catch (Exception ex) { Say("Load failed: " + ex.Message); return false; }
    }
}
