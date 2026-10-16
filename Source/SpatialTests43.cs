using System.Text.Json;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public static class SpatialTests43
{
 public static void Run()
 {
  int checks=0;void Check(bool value,string label){checks++;if(!value)throw new InvalidOperationException("Pass43: "+label);}
  Check(Spatial43.CachedSystems==0,"packages lazy before exploration");var totals=new Dictionary<CelestialKind,int>();var gates=Galaxy42.Data.Gates.ToDictionary(g=>g.Id);int maxCache=0;
  foreach(var system in Galaxy42.Data.Systems)
  {
   var p=Spatial43.Get("system:"+system.Id);maxCache=Math.Max(maxCache,Spatial43.CachedSystems);Check(p.SystemId==system.Id,"system identity");
   foreach(var b in p.Bodies)
   {
    totals[b.Kind]=totals.GetValueOrDefault(b.Kind)+1;Check(b.Position.Finite&&double.IsFinite(b.Radius)&&b.Radius>=0,"finite coordinates/radius");Check(!Retheme41.SourceLorePattern.IsMatch(b.Name),"Starlight name");
    Check(b.Parent.Length==0||p.ById[b.Parent].System==b.System,"same-system parent");
    if(b.Kind is CelestialKind.Sun or CelestialKind.Planet or CelestialKind.Moon)Check((b.Celestial.Anchor-b.Position).Length>b.Radius+100000,"warp body clearance");
    if(b.Kind==CelestialKind.AsteroidBelt)Check(b.SourceEra=="2019-11-28"&&b.Parent.Length>0,"original belt provenance/hierarchy");
    if(b.Kind==CelestialKind.Stargate){var g=gates[b.SourceId];Check(b.Position==new SpacePosition(g.Position[0],g.Position[1],g.Position[2]),"gate exact coordinates");Check(b.DestinationGateId==g.DestinationGateId&&b.Destination=="system:"+g.DestinationSystemId,"gate destination identity");}
   }
  }
  Check(maxCache==Spatial43.CacheLimit&&Spatial43.CachedSystems==4,"bounded system LRU");
  foreach(var pair in new[]{(CelestialKind.Sun,8089),(CelestialKind.Planet,68407),(CelestialKind.Moon,344457),(CelestialKind.AsteroidBelt,55577),(CelestialKind.Stargate,13978),(CelestialKind.Beacon,1038)})Check(totals[pair.Item1]==pair.Item2,"complete "+pair.Item1);
  Check(Spatial43.LandmarkData.Value.Length==45&&Spatial43.LandmarkData.Value.Count(l=>l.SystemId!=null)==10,"landmark coverage/explicit links only");
  foreach(var l in Spatial43.LandmarkData.Value)Check(Spatial43.Information("landmark43/"+l.Id,"aster")!=null&&l.GalacticPosition.Length==3&&l.GalacticPosition.All(double.IsFinite),"landmark information");
  string? prior=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");string dir=Path.Combine(Path.GetTempPath(),"Spatial43-"+Guid.NewGuid());Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",dir);
  try
  {
   var legacy=new DemoWorld();legacy.NewGame(1);legacy.Credits=7654;legacy.Save();string original=File.ReadAllText(DemoWorld.SavePath);
   var w=new DemoWorld();w.NewExploration43(30000001);Check(w.ImportedWorld43&&!w.Docked&&w.Current.Name==Galaxy42.ById[30000001].Name,"separate playable source world");
   Check(!w.Targets.Any(t=>t.Kind==TargetKind.Station||t.Kind==TargetKind.Raider&&!w.ManagedPopulationTarget(t.Id)),"no fabricated source stations or unmanaged raiders");
   var belt=w.Celestials.First(c=>c.Kind==CelestialKind.AsteroidBelt);Check(Spatial43.Information("celestial/"+w.SystemId+"/"+belt.Id,"aster")?.Id==belt.Id,"source information remains addressable from another world");w.EnterGrid(belt.Id,false);var rock=w.Targets.First(t=>t.Kind==TargetKind.Asteroid);string rid=rock.Id;var initial=rock.Position;float amount=rock.Ore;rock.Ore=7;w.RememberSolarGrid();w.EnterGrid(w.Celestials.First(c=>c.Kind==CelestialKind.Planet).Id,false);w.EnterGrid(belt.Id,false);Check(w.Targets.First(t=>t.Id==rid).Ore==7,"depletion survives unloading/re-entry");
   var bookmark=w.AddBookmark("Original belt");w.Save();Check(File.ReadAllText(DemoWorld.SavePath)==original,"legacy save bytes retained");
   var loaded=new DemoWorld();Check(loaded.Load(DemoWorld.ImportedSavePath43)&&loaded.ImportedWorld43&&loaded.SystemId==w.SystemId&&loaded.SolarCelestial==belt.Id,"source save round trip exact belt identity");Check(loaded.Targets.First(t=>t.Id==rid).Ore==7&&loaded.Bookmarks.Any(b=>b.Id==bookmark),"depletion/bookmark round trip");
   // Live floating-origin rebase at AU coordinates, including effects and ordnance.
   w.SolarOrigin=new(100*SolarCatalog.AU,-20*SolarCatalog.AU,60*SolarCatalog.AU);w.Player.Position=new(40000,123,-50);w.Player.ManualPoint=w.Player.Position+new Vector3(22,3,4);w.Targets.Clear();w.Targets.Add(new(){Id="precision",Position=w.Player.Position+new Vector3(1,2,3)});w.MissilesInFlight.Add(new(){Position=w.Player.Position+new Vector3(9,8,7)});w.EwarZones.Add(new(){Center=w.Player.Position+new Vector3(5,6,7)});w.WeaponVisuals.Add(new(){Start=w.Player.Position,End=w.Player.Position+Vector3.One});var before=w.SystemPosition;w.RebaseSpatial43();Check((w.SystemPosition-before).Length<.01&&w.Player.Position==Vector3.Zero,"AU rebase preserves absolute location");Check(w.Targets[0].Position==new Vector3(1,2,3)&&w.MissilesInFlight[^1].Position==new Vector3(9,8,7)&&w.EwarZones[^1].Center==new Vector3(5,6,7)&&w.WeaponVisuals[^1].End==Vector3.One,"all live positions rebase together");w.Player.Position+=Vector3.UnitX;Check(Math.Abs((w.SystemPosition-before).Length-1)<.01,"one metre motion at 100 AU");
   // Drive the real warp state machine to an exported planet, with the largest range.
   var planet=w.Celestials.First(c=>c.Kind==CelestialKind.Planet);w.EnterGrid(belt.Id,false);w.Player.Cap=w.Player.MaxCap;Check(w.BeginWarp(planet.Id,100000),"source warp starts");var trip=w.Transit!;w.Player.Velocity=(trip.To-w.SystemPosition).Unit.Local*w.Player.Speed;w.Player.Facing=Vector3.Normalize(w.Player.Velocity);w.UpdateSolarPreview(500);Check(!w.InWarp&&w.Transit==null&&w.SolarCelestial==planet.Id,"source warp completes");Check((w.SystemPosition-planet.Position).Length>planet.Radius,"arrival outside body");
   var gate=w.Celestials.First(c=>c.Kind==CelestialKind.Stargate);int reverse=Spatial43.Get(w.SystemId).ById[gate.Id].DestinationGateId;w.EnterGrid(gate.Id,false);w.Player.Position=Vector3.Zero;Check(w.SystemPosition==gate.Position,"departure exact exported gate");Check(w.UseSolarGate(gate.Id)&&w.SystemId==gate.Destination&&w.SolarCelestial=="stargate:"+reverse,"reverse gate ID used");var other=SolarCatalog.Resolve(w.SystemId,w.SolarCelestial)!;Check(w.SystemPosition==other.Position+new SpacePosition(0,1200,0),"arrival exact reverse gate plus documented offset");w.SetDestination(gate.System);Check(w.Route().SequenceEqual(new[]{gate.System}),"source route adapted");
   // Grid change unloads actors and restores remembered source depletion.
   w.EnterGrid(other.Id,false);w.EnterDeepSpace(other.Position+new SpacePosition(20000000,0,0));Check(w.SolarCelestial.Length==0&&!w.Targets.Any(t=>t.Kind==TargetKind.Stargate),"active grid unload");
   w.NewExploration43(32000001);Check(!w.Celestials.Any()&&w.SolarCelestial.Length==0&&w.Grid=="deep_space"&&w.SolarGridKey==SolarCatalog.GridKey(w.SystemId,w.SystemPosition),"starless empty source records stay empty with valid grid identity");w.AdvanceHours(240);Check(!w.Quarantine,"source exploration has no authored campaign deadline");w.Screen=ViewMode.Title;w.Save();var titleReload=new DemoWorld();Check(titleReload.Load(DemoWorld.ImportedSavePath43)&&!titleReload.Docked&&titleReload.Grid=="deep_space","source title save resumes flight without fabricated station");
   var legacyReload=new DemoWorld();Check(legacyReload.Load()&&legacyReload.Credits==7654&&!legacyReload.ImportedWorld43,"legacy campaign remains loadable");
  }
  finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",prior);if(Directory.Exists(dir))Directory.Delete(dir,true);}
  Console.WriteLine($"{checks:N0} Pass43 spatial checks passed / {Galaxy42.Data.Systems.Length} systems / cache {maxCache}.");
 }
}
