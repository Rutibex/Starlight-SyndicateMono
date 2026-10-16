namespace StarlightSyndicateDemo;
internal static class ShipStatTests
{
 public static void Run()
 {
  int count=0;void Check(bool yes,string message){if(!yes)throw new Exception("STAT FAIL: "+message);count++;Console.WriteLine("STAT PASS: "+message);}
  Check(ShipAttributeCatalog.All.Count(a=>a.ReferenceId>0)==658,"Every audited ship Dogma attribute has a schema entry");
  Check(ShipAttributeCatalog.All.Select(a=>a.Key).Distinct().Count()==ShipAttributeCatalog.All.Length,"No duplicate attribute keys");
  for(int origin=0;origin<3;origin++)
  {var w=new DemoWorld();w.NewGame(origin);Check(w.Player.Attributes.Count==ShipAttributeCatalog.All.Length&&w.Player.Attributes.Values.All(v=>v==null||double.IsFinite(v.Value)),"Origin "+origin+" initializes complete finite schema");Check(w.Player.Shield==w.Player.MaxShield&&w.Player.Cap==w.Player.MaxCap&&w.Player.Hull==w.Player.MaxHull,"Origin "+origin+" starts at correct resource maxima");}
  var ship=new Ship();ship.SetStat("capacity",312);Check(ship.CargoMax==312,"Cargo capacity uses the attribute model");ship.SetStat("maxVelocity",845);Check(ship.Speed==845,"Velocity uses the attribute model");
  double align=ship.AlignTime;ship.SetStat("mass",ship.Stat("mass")*2);Check(Math.Abs(ship.AlignTime-align*2)<.0001,"Mass updates alignment and movement inertia");
  Check(Math.Abs(DemoWorld.Recharge(46.25f,185,95)-ship.PeakCapRecharge)<.0001,"Capacitor peaks at 25 percent");Check(DemoWorld.Recharge(185,185,95)==0&&DemoWorld.Recharge(0,185,95)==0,"Recharge curve handles full and depleted resources");
  double ehp=ship.EffectiveHitpoints;foreach(var d in Enum.GetValues<DamageType>())ship.SetStat("shield"+(d==DamageType.EM?"Em":d.ToString())+"DamageResonance",.5);Check(ship.EffectiveHitpoints>ehp&&ship.Resistance("shield",DamageType.EM)==.5f,"Layer resistance changes EHP without changing raw hitpoints");
  bool invalid=false;try{ship.SetStat("capacitorCapacity",double.NaN);}catch(ArgumentOutOfRangeException){invalid=true;}Check(invalid,"Non-finite attributes rejected");invalid=false;try{ship.SetStat("rechargeRate",0);}catch(ArgumentOutOfRangeException){invalid=true;}Check(invalid,"Invalid recharge times rejected");
  var range=new DemoWorld();range.NewGame(0);range.EnterGrid("belt");range.Select("raider");range.Player.SetStat("maxTargetRange",10);range.LockTarget();Check(range.LockedId=="","Locking obeys reduced sensor range");range.Player.SetStat("maxTargetRange",50000);range.LockTarget();Check(range.LockedId=="raider","Locking obeys increased sensor range");
  string temp=Path.Combine(Path.GetTempPath(),"starlight-stats-"+Guid.NewGuid().ToString("N"));string? prev=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
  try{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",temp);range.Selected!.Profile!.SetStat("cpuOutput",89);range.Player.SetStat("cpuOutput",167);range.Player.SetStat("mass",1600000);range.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.Player.Stat("cpuOutput")==167&&loaded.Player.Stat("mass")==1600000&&loaded.Player.Attributes.Count==ShipAttributeCatalog.All.Length&&loaded.Selected!.Profile!.Stat("cpuOutput")==89,"Full attribute set and custom hull values persist");
   string json=File.ReadAllText(DemoWorld.SavePath);var node=System.Text.Json.Nodes.JsonNode.Parse(json)!;node.AsObject().Remove("ShipAttributes");File.WriteAllText(DemoWorld.SavePath,node.ToJsonString());Check(loaded.Load()&&loaded.Player.Stat("cpuOutput")==112,"Older saves migrate to origin hull defaults");}
  finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",prev);if(Directory.Exists(temp))Directory.Delete(temp,true);}
  Console.WriteLine($"{count} ship-stat checks passed.");
 }
}
