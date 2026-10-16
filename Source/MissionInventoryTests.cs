namespace StarlightSyndicateDemo;
public static class MissionInventoryTests
{
 public static void Run()
 {
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS: "+label);checks++;}
  string? previous=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");string dir=Path.Combine(Path.GetTempPath(),"mission-inventory-"+Guid.NewGuid().ToString("N"));Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",dir);
  try{
   var w=new DemoWorld();w.NewGame(0);w.Player.CargoMax=100000;
   var a=AgentCatalog38.Agents.First(a=>a.Station=="aster"&&a.Level==1&&a.Division==AgentDivision38.Mining);
   Check(w.RequestMission38(a.Id)=="","offer mining contract");var m=w.AgentsState.Missions.Last();Check(w.AcceptMission38(m.Id)=="","accept contract");
   w.Undock();w.EnterGrid(m.WarpId,false);var rock=w.Targets.First(t=>t.Kind==TargetKind.Asteroid);float before=w.CargoUsed;
   Check(w.HarvestMission38(rock,50)==50,"harvest mission ore");string id="mission-cargo:"+m.Id;
   Check(w.InventoryDisplayIds(false).Contains(id)&&w.InventoryDisplayCount(id,false)==50,"mined ore appears in cargo inventory");
   Check(w.Player.Ore==0&&m.Harvested==50,"mission identity and objective progress preserved");
   Check(Math.Abs(w.CargoUsed-before-50)<.01,"volume counted exactly once");
   Check(w.InventoryDisplayName(id).Contains(m.Spec.Resource)&&w.InventoryDisplayVolume(id)==1,"correct resource and volume labels");
   Check(!ItemCatalog.Contains(id)&&!w.TransferItem(id,false,50),"contract cannot enter ordinary item ledger");
   w.Screen=ViewMode.Station;w.Grid="station";
   Check(w.TransferInventoryMission(id,false,25),"unload contract ore through inventory");
   Check(w.InventoryDisplayCount(id,false)==25&&w.InventoryDisplayCount(id,true)==25,"cargo and local hangar projections agree");
   Check(w.TransferInventoryMission(id,true,12.5f)&&w.InventoryDisplayCount(id,false)==37.5m,"fractional mission transfer preserves volume");
   Check(!w.TransferInventoryMission(id,true,100),"oversized transfer rejected");
   var other=new Mission38{Agent=m.Agent,Definition=m.Definition,State="Accepted",Cargo=7};w.AgentsState.Missions.Add(other);
   Check(w.InventoryDisplayIds(false).Count(i=>i.StartsWith("mission-cargo:"))==2,"same resource contracts remain separate stacks");
   var freight=new Mission38{Agent=m.Agent,Definition=AgentCatalog38.Missions.Values.First(d=>d.Division==AgentDivision38.Distribution&&d.Volume>0).Id,State="Accepted",Cargo=8};w.AgentsState.Missions.Add(freight);
   Check(w.InventoryDisplayName("mission-cargo:"+freight.Id)=="Mission freight"&&w.InventoryDisplayCount("mission-cargo:"+freight.Id,false)==8,"courier freight also appears in cargo");
   w.Save();var load=new DemoWorld();Check(load.Load()&&load.InventoryDisplayCount(id,false)==37.5m,"existing mission cargo survives save and appears on reload");
   load.SystemId="boreal";Check(!load.InventoryDisplayIds(true).Contains(id),"remote mission hangar is hidden");load.SystemId="aster";
   m=load.AgentsState.Missions.First(x=>x.Id==m.Id);m.Cargo=m.Spec.Volume;m.Harvested=m.Spec.Volume;m.Hangars.Clear();
   Check(load.CompleteMission38(m.Id)==""&&!load.InventoryDisplayIds(false).Contains(id),"turn-in consumes displayed cargo");
   Console.WriteLine($"Mission inventory: {checks} checks passed.");
  }finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",previous);if(Directory.Exists(dir))Directory.Delete(dir,true);}
 }
}
