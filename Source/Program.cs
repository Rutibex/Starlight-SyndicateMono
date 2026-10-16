using System.Text.Json;
using StarlightSyndicateDemo;
if(args.Contains("--stage-q-selftest")){SystemsTestsQ.Run();return;}
if(args.Contains("--stage-p-selftest")){OperationsTestsP.Run();return;}
if(args.Contains("--stage-o-selftest")){ExplorationTestsO.Run();return;}
if(args.Contains("--stage-n-selftest")){ExplorationTestsN.Run();return;}
if(args.Contains("--stage-m-selftest")){MissionTestsM.Run();return;}
if(args.Contains("--stage-l-selftest")){EncounterTestsL.Run();return;}
if(args.Contains("--stage-i-verify-save")){int at=Array.IndexOf(args,"--stage-i-verify-save");var world=new DemoWorld();if(at+1>=args.Length||!world.Load(args[at+1]))throw new Exception("Campaign checkpoint failed validation");Console.WriteLine($"Campaign checkpoint loaded: day {world.Day:0.00}, schema {world.SaveSchemaVersion}, NPC ledger error {world.PopulationMoneyErrorC}");return;}
if(args.Contains("--stage-i-benchmark")){PerformanceTestsI.Run();return;}
if(args.Contains("--stage-i-selftest")){ExperienceTestsI.Run();return;}
if(args.Contains("--stage-i-soak")){ExperienceTestsI.Soak();return;}
if(args.Contains("--mission-cargo-selftest")){MissionInventoryTests.Run();return;}
if(args.Contains("--stage-h-selftest")){AdvancedTestsH.Run();return;}
if(args.Contains("--stage-g-selftest")){OrganisationTestsG.Run();return;}
if(args.Contains("--stage-f-selftest")){ExplorationTestsF.Run();return;}
if(args.Contains("--stage-e-selftest")){ProductionTestsE.Run();return;}
if(args.Contains("--stage-d-selftest")){CommanderTestsD.Run();return;}
if(args.Contains("--stage-c-selftest")){PopulationTestsC.Run();return;}
if(args.Contains("--stage-b-selftest")){StageBTests.Run();return;}
if(args.Contains("--weapons-selftest")){SkillCatalog.VerificationAllV=!args.Contains("--stage-h-ui-test")&&true;WeaponTests.Run();return;}
if(args.Contains("--ewar-selftest")){SkillCatalog.VerificationAllV=!args.Contains("--stage-h-ui-test")&&true;EwarTests.Run();return;}
if(args.Contains("--defense-selftest")){SkillCatalog.VerificationAllV=!args.Contains("--stage-h-ui-test")&&true;DefenseTests.Run();return;}
if(args.Contains("--support-selftest")){SkillCatalog.VerificationAllV=!args.Contains("--stage-h-ui-test")&&true;SupportTests.Run();return;}
if(args.Contains("--stage-a-selftest")){StageATests.Run();return;}
if(args.Contains("--dogma-selftest")){DogmaTests45.Run();return;}
if(args.Contains("--stations-selftest")){StationTests44.Run();return;}
if(args.Contains("--spatial-selftest")){SpatialTests43.Run();return;}
if(args.Contains("--galaxy-selftest")){GalaxyTests42.Run();return;}
if(args.Contains("--retheme-selftest")){RethemeTests41.Run();return;}
if(args.Contains("--items-export")){File.WriteAllText(args[Array.IndexOf(args,"--items-export")+1],JsonSerializer.Serialize(ItemCatalog.All.Select(i=>new {i.Id,i.Name,i.Group,i.Slot,i.Volume,i.Price,SourceTypeId=UnifiedCatalog40.FindSourceId(i.Id),SourceName=i.SourceType?.Name,MarketPath=MarketGroups.PathFor(i),i.Supported}),new JsonSerializerOptions{WriteIndented=true}));return;}
if(args.Contains("--items-selftest")){ItemTests40.Run();return;}
if(args.Contains("--agents-selftest")){AgentTests38.Run();return;}
if(args.Contains("--industry-selftest")){IndustryTests37.Run();return;}
if(args.Contains("--industry-export")){File.WriteAllText(args[Array.IndexOf(args,"--industry-export")+1],JsonSerializer.Serialize(IndustryCatalog37.Recipes,new JsonSerializerOptions{WriteIndented=true}));return;}
if(args.Contains("--skills-selftest")){SkillsTests36.Run();return;}
SkillCatalog.VerificationAllV=!args.Contains("--stage-h-ui-test")&&!args.Contains("--stage-g-ui-test")&&!args.Contains("--stage-f-ui-test")&&!args.Contains("--stage-e-ui-test")&&!args.Contains("--stage-d-ui-test")&&!args.Contains("--stage-c-ui-test")&&!args.Contains("--stage-b-ui-test")&&!args.Contains("--agents-ui-test")&&!args.Contains("--industry-ui-test")&&!args.Contains("--skills-ui-test")&&args.Any(a=>a.EndsWith("selftest")||a.EndsWith("-test"));
if(args.Contains("--fitting-planner-selftest")){FittingPlannerTests.Run();return;}
if(args.Contains("--information-selftest")){InformationTests.Run();return;}
if(args.Contains("--probe-selftest")){ProbeScanningTests.Run();return;}
if(args.Contains("--drone-selftest")){DroneTests.Run();return;}
if(args.Contains("--solar-selftest")){SolarTests.Run();return;}
if(args.Contains("--market-selftest")){MarketTests.Run();return;}
if(args.Contains("--batch29-selftest")){ShipBatch29Tests.Run();return;}
if(args.Contains("--batch28-selftest")){ShipBatch28Tests.Run();return;}
if(args.Contains("--batch27-selftest")){ShipBatch27Tests.Run();return;}
        if(args.Contains("--batch26-selftest")){ShipBatch26Tests.Run();return;}
if(args.Contains("--batch25-selftest")){ShipBatch25Tests.Run();return;}
if(args.Contains("--batch24-selftest")){ShipBatch24Tests.Run();return;}
if(args.Contains("--batch23-selftest")){ShipBatch23Tests.Run();return;}
if(args.Contains("--batch22-selftest")){ShipBatch22Tests.Run();return;}
if(args.Contains("--batch21-selftest")){ShipBatch21Tests.Run();return;}
if(args.Contains("--model-selftest")){ShipModelTests.Run();return;}
if(args.Contains("--fitting-simulation-selftest")){StarlightSyndicateDemo.FittingSimulationTests.Run();return;}

if(args.Contains("--fitting-layout-selftest")){FittingLayoutTests.Run();return;}
if(args.Contains("--ship-info-selftest")){DemoGame.RunShipInformationAudit();return;}

if(args.Contains("--export-stats")){int index=Array.IndexOf(args,"--export-stats");ShipStatExport.Write(args[index+1]);return;}
if(args.Contains("--fleet-selftest")){FleetTests.Run();return;}
if(args.Contains("--rig-selftest")){RigTests.Run();return;}
if(args.Contains("--special-selftest")){SpecialTests.Run();return;}
if(args.Contains("--utility-selftest")){UtilityTests.Run();return;}
if(args.Contains("--mobility-selftest")){MobilityTests.Run();return;}
if(args.Contains("--selftest")) { SmokeTests.Run(); MarketTests.Run(); ShipStatTests.Run(); WeaponTests.Run(); EwarTests.Run(); DroneTests.Run(); DefenseTests.Run(); SupportTests.Run(); EnhancementTests.Run();MobilityTests.Run();UtilityTests.Run();SpecialTests.Run();RigTests.Run(); return; }
if(args.Contains("--stage-k-selftest")){ClientTestsK.Run();return;}
if(args.Contains("--stage-j-selftest")){FoundationTestsJ.Run();return;}
using var game = new DemoGame();
game.Run();
