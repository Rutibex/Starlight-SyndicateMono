using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
internal static class EwarTests
{
 public static void Run()
 {
  int count=0;void Check(bool ok,string label){if(!ok)throw new Exception("EWAR FAIL: "+label);count++;Console.WriteLine("EWAR PASS: "+label);}
  void Step(DemoWorld w,float seconds){for(int i=0;i<(int)Math.Ceiling(seconds/.05f);i++)w.Update(.05f);}
  DemoWorld Trial(EwarDefinition d)
  {
   var w=new DemoWorld();w.NewGame(0);var h=CombatHulls.All.First(h=>h.Size>=Math.Max(1,d.Size)&&(d.Role.Length==0||h.Role==d.Role));w.WeaponTrial(h.Id);
   int slot=d.Slot=="high"?0:1;Check(w.FitEwar(d.Id,true,slot),"Fit / "+d.Name);
   if(d.Magazine>0)Check(w.LoadEwarPayload(slot,d.Kind==EwarKind.Sphere?"ewar_probe":"ewar_fuel"),"Payload / "+d.Name);
   w.EnterEwarRange();w.WeaponSafety=false;w.SelectedId=w.LockedId="range_0";
   foreach(var t in w.Targets){t.Position=new(500,0,0);t.LockId="player";t.Cap=t.Profile!.MaxCap;t.Profile.NativeSensor=d.Sensor.Length>0&&d.Sensor!="All"?d.Sensor:"Magnetometric";t.Profile.SetStat("scan"+t.Profile.NativeSensor+"Strength",.1);}
   return w;
  }
  Check(EwarCatalog.All.Length==34&&EwarCatalog.All.Select(d=>d.Id).Distinct().Count()==34,"All 34 entries mapped exactly once");
  Check(EwarCatalog.All.Select(d=>d.Name).Distinct().Count()==34,"Original distinct EWAR names");
  foreach(var d in EwarCatalog.All)
  {
   var w=Trial(d);var t=w.Locked!;int slot=d.Slot=="high"?0:1;float before=t.Cap;Vector3 position=t.Position;
   if(d.Kind==EwarKind.Siphon)w.Player.Cap=0;
   if(d.Kind==EwarKind.Displacement){t.Profile!.SetCombatHull("frigate");t.Signature=t.Profile.Signature;}
   w.ToggleEwar(slot);Step(w,d.Warmup+.1f);var effect=w.EwarFor(t.Id);
   bool applied=d.Kind switch
   {
    EwarKind.ECM=>effect.Jammers.Contains("player"),
    EwarKind.BurstJam or EwarKind.BurstECM=>t.LockId=="",
    EwarKind.Neutralizer or EwarKind.Siphon or EwarKind.BurstNeutral=>t.Cap<before,
    EwarKind.Damp or EwarKind.BurstDamp=>effect.LockRange<1,
    EwarKind.Tracking=>effect.Tracking<1,
    EwarKind.Guidance=>effect.ExplosionVelocity<1,
    EwarKind.Web or EwarKind.Grapple or EwarKind.BurstWeb=>effect.Speed<1,
    EwarKind.Paint or EwarKind.BurstPaint=>effect.Signature>1,
    EwarKind.Point or EwarKind.Scram=>effect.WarpStrength>0,
    EwarKind.Sphere or EwarKind.Field or EwarKind.BurstPoint=>effect.Bubble,
    EwarKind.BurstWeapon=>effect.Tracking<1&&effect.ExplosionVelocity<1,
    EwarKind.Displacement=>t.Position!=position,
    _=>false
   };
   Check(applied,"Combat behavior / "+d.Name);
  }
  var damp=EwarCatalog.All.First(d=>d.Kind==EwarKind.Damp);var script=Trial(damp);script.Screen=ViewMode.Station;Check(script.LoadEwarPayload(1,"ewar_range_script"),"Load damp range script");script.Screen=ViewMode.Space;script.ToggleEwar(1);script.Update(.05f);var s=script.EwarFor("range_0");Check(s.LockRange<.6&&s.ScanResolution==1,"Range script doubles one effect and removes the other");
  var guidance=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Guidance));guidance.Screen=ViewMode.Station;guidance.LoadEwarPayload(1,"ewar_flight_script");guidance.Screen=ViewMode.Space;guidance.ToggleEwar(1);guidance.Update(.05f);s=guidance.EwarFor("range_0");Check(s.MissileRange<1&&s.ExplosionVelocity==1,"Guidance flight script leaves application unchanged");
  var prism=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Tracking));prism.Screen=ViewMode.Station;prism.LoadEwarPayload(1,"ewar_tracking_script");prism.Screen=ViewMode.Space;prism.ToggleEwar(1);prism.Update(.05f);s=prism.EwarFor("range_0");Check(s.Tracking<1&&s.TurretRange==1,"Tracking script leaves gun range unchanged");
  var target=new Ship();target.NativeSensor="Radar";target.SetStat("scanRadarStrength",10);var ecm=EwarCatalog.All.First(d=>d.Kind==EwarKind.ECM&&d.Sensor=="Radar");var mismatch=EwarCatalog.All.First(d=>d.Kind==EwarKind.ECM&&d.Sensor=="Ladar");Check(DemoWorld.JamChance(ecm,target,1000)>DemoWorld.JamChance(mismatch,target,1000),"Matching sensor type improves ECM chance");
  var jam=Trial(ecm);jam.Player.NativeSensor="Radar";jam.Player.SetStat("scanRadarStrength",.1);jam.ApplyEwarCycle(ecm,"range_0","player","test_jam");Check(jam.CanLock("player","range_0")&&!jam.CanLock("player","range_1"),"ECM permits targeting the jammer and blocks other ships");
  var nos=EwarCatalog.All.First(d=>d.Kind==EwarKind.Siphon&&d.Size==1);var siphon=Trial(nos);siphon.Player.Cap=100;siphon.Targets[0].Cap=80;siphon.ApplyEwarCycle(nos,"player","range_0","test_nos");Check(siphon.Player.Cap==100&&siphon.Targets[0].Cap==80,"Siphon does nothing against a lower absolute capacitor");siphon.Targets[0].Cap=150;siphon.ApplyEwarCycle(nos,"player","range_0","test_nos");Check(Math.Abs(siphon.Player.Cap+siphon.Targets[0].Cap-250)<.001&&siphon.Player.Cap>100,"Siphon conserves total capacitor");
  var neut=EwarCatalog.All.First(d=>d.Kind==EwarKind.Neutralizer&&d.Size==1);siphon.Targets[0].Profile!.SetStat("energyWarfareResistance",.5);siphon.Targets[0].Cap=100;siphon.ApplyEwarCycle(neut,"player","range_0","test_neut");Check(Math.Abs(siphon.Targets[0].Cap-85)<.001,"Neutralizer honors capacitor warfare resistance");
  Check(DemoWorld.RangeScale(10,10,10)==1&&Math.Abs(DemoWorld.RangeScale(20,10,10)-.5)<.001,"Capacitor and grappler falloff curve");Check(DemoWorld.StackMultiplier([.5f,.5f])>.25f&&DemoWorld.StackMultiplier([.5f,.5f])<.5f,"Repeated EWAR has a stacking penalty");
  var points=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Point&&d.Size==0));points.Player.SetStat("warpCoreStrength",1);points.ApplyEwarCycle(EwarCatalog.All.First(d=>d.Kind==EwarKind.Point&&d.Size==0),"range_0","player","one");Check(!points.WarpBlocked("player"),"Equal disruption strength does not overpower warp core");points.ApplyEwarCycle(EwarCatalog.All.First(d=>d.Kind==EwarKind.Scram&&d.Size==0),"range_1","player","two");Check(points.WarpBlocked("player")&&!points.CanUseMicroDrive("player")&&points.CanUseGate("player"),"Scram disables micro drives; tackled subcapital can use gates");points.Player.SetCombatHull("dread");Check(!points.CanUseGate("player")&&!points.CanUseJumpDrive("player")&&!points.CanDockAtStructure("player"),"Tackled capital cannot gate, jump-drive or structure-dock");
  var hic=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Field));hic.Screen=ViewMode.Station;Check(hic.LoadEwarPayload(0,"ewar_point_script"),"Load focused disruption script");hic.Screen=ViewMode.Space;hic.ToggleEwar(0);hic.Update(.05f);Check(hic.EwarFor("range_0").WarpStrength==10000&&!hic.EwarFor("range_0").Bubble&&!hic.EwarFor("range_0").Scrammed,"Focused field is infinite targeted point, not a bubble or scram");
  var sphere=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Sphere));sphere.ToggleEwar(0);sphere.Update(.05f);Check(sphere.WarpBlocked("player"),"Own bubble blocks the launching ship too");sphere.Player.SetStat("warpBubbleImmune",1);Check(!sphere.WarpBlocked("player"),"Intrinsic nullification ignores bubbles");sphere.EwarZones[0].Remaining=.05f;sphere.Update(.1f);Check(sphere.EwarZones.Count==0,"Probe bubble expires");
  var invalid=new DemoWorld();invalid.NewGame(0);var capital=EwarCatalog.All.First(d=>d.Kind==EwarKind.Displacement);invalid.Hangar[capital.Id]=1;Check(!invalid.FitEwar(capital.Id,true,0)&&invalid.Hangar[capital.Id]==1,"Role/size rejection preserves inventory");
  var reload=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Sphere));reload.Screen=ViewMode.Station;var module=reload.Player.Ewar[0];float payloads=reload.ItemCount("ewar_probe",true)+module.Loaded;Check(reload.LoadEwarPayload(0,"ewar_probe")&&reload.ItemCount("ewar_probe",true)+module.Loaded==payloads,"Reloading probes conserves inventory");
  var victim=Trial(damp);victim.ApplyEwarCycle(damp,"range_0","player","incoming","Resolution");victim.SelectedId="range_1";victim.LockTarget();Check(victim.PendingLockId=="range_1"&&victim.LockTimeLeft>1.5f,"Scan-resolution suppression delays lock acquisition");
  var paint=Trial(EwarCatalog.All.First(d=>d.Kind==EwarKind.Paint));float signature=paint.Targets[0].Signature;paint.ToggleEwar(1);paint.Update(.05f);Check(paint.EffectiveSignature(paint.Targets[0])>signature&&paint.Targets[0].Signature==signature,"Painter changes live signature without mutating hull stats");
  string temp=Path.Combine(Path.GetTempPath(),"starlight-ewar-"+Guid.NewGuid().ToString("N"));string? old=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
  try
  {
   Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",temp);hic.EwarZones.Add(new(){Kind=EwarKind.Sphere,Center=new(4,5,6),Radius=2500,Remaining=12});hic.Player.Ewar[0].WarmupLeft=2;hic.Save();var loaded=new DemoWorld();Check(loaded.Load(),"EWAR save loads");Check(loaded.Player.Ewar[0].ModuleId==hic.Player.Ewar[0].ModuleId&&loaded.Player.Ewar[0].PayloadId=="ewar_point_script"&&loaded.Player.Ewar[0].WarmupLeft==2&&loaded.EwarEffects.Count==hic.EwarEffects.Count&&loaded.EwarZones.Single().Center==new Vector3(4,5,6)&&loaded.Targets.Count==6,"Fitting, scripts, windups, effects, bubble coordinates and range persist");
  }
  finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",old);if(Directory.Exists(temp))Directory.Delete(temp,true);}
  Console.WriteLine($"{count} electronic warfare checks passed.");
 }
}
