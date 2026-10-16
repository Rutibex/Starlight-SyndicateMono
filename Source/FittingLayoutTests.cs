using System.Text.Json;
namespace StarlightSyndicateDemo;
public static class FittingLayoutTests
{
 static int checks;
 static void Check(bool ok,string label){if(!ok)throw new Exception("FITTING LAYOUT FAIL: "+label);checks++;Console.WriteLine("FIT PASS: "+label);}
 static DemoWorld World(){var w=new DemoWorld();w.NewGame(0);w.Screen=ViewMode.Station;w.WeaponTrial("battleship");return w;}
 public static void Run()
 {
  checks=0;Check(FittingLayoutModel.Catalog.Select(d=>d.Kind).Distinct().Count()==10,"All nine module systems and original utility modules share one fitting browser");
  Check(FittingLayoutModel.Catalog.All(d=>ItemCatalog.All.Any(i=>i.Id==d.Id))&&FittingLayoutModel.Catalog.Select(d=>d.Id).Distinct().Count()==FittingLayoutModel.Catalog.Length,"Catalog IDs resolve uniquely to actual item definitions");
  foreach(var h in FleetCatalog.Playable){var w=World();w.Credits=1000000;w.SystemId=ShipBatch21.ById[h.Id].HomeSystem;Check(w.BoardFleetHull(h.Id),"Acquire layout fixture "+h.Name);var before=JsonSerializer.Serialize(w.Player);var wallet=w.Credits;var hangar=JsonSerializer.Serialize(w.Hangar);var sockets=FittingLayoutModel.Sockets(w.Player);var installed=FittingLayoutModel.Installed(w.Player);
   Check(FittingLayoutModel.Banks.All(b=>sockets.Count(s=>s.Bank==b)==(int)(b=="rig"?h.Value("upgradeSlotsLeft"):w.Player.SubsystemBase(b=="high"?"hiSlots":b=="medium"?"medSlots":"lowSlots",h.Value(b=="high"?"hiSlots":b=="medium"?"medSlots":"lowSlots")))),h.Name+" rings use source capacities");
   Check(sockets.Count(s=>s.Module!=null)==installed.Count&&sockets.Where(s=>s.Module!=null).Select(s=>s.Module!.Key).Distinct().Count()==installed.Count,h.Name+" every installed module appears once");
   Check(JsonSerializer.Serialize(w.Player)==before&&w.Credits==wallet&&JsonSerializer.Serialize(w.Hangar)==hangar,h.Name+" drawing projection changes no ship, inventory or wallet state");
  }
  foreach(string kind in FittingLayoutModel.Systems.Skip(1)){
   var w=World();w.UnfitLayoutWeapon(0);w.UnfitLayoutWeapon(1);foreach(var s in w.Player.Defense.Select((s,i)=>(s,i)).Where(p=>p.s.Definition!=null).ToArray())w.UnfitDefense(s.i);foreach(var s in w.Player.Ewar.Select((s,i)=>(s,i)).Where(p=>p.s.Definition!=null).ToArray())w.UnfitEwar(s.i);
   var candidate=FittingLayoutModel.Catalog.Where(d=>d.Kind==kind).First(d=>{int index=FittingLayoutModel.Destination(w.Player,d);return FittingLayoutModel.Error(w,d,index).Length==0;});int target=FittingLayoutModel.Destination(w.Player,candidate);w.Hangar[candidate.Id]=2;float stock=w.ItemCount(candidate.Id,true);float credits=w.Credits;
   Check(FittingLayoutModel.Fit(w,candidate,true,target)&&w.ItemCount(candidate.Id,true)==stock-1&&w.Credits==credits,kind+" fitting adapter consumes owned stock without purchasing");
   var module=FittingLayoutModel.Installed(w.Player).Single(m=>m.Kind==kind&&m.Index==target);
   Check(module.Id==candidate.Id&&module.Bank==candidate.Bank&&FittingLayoutModel.Destination(w.Player,candidate,module)==target,kind+" visual selection retains its actual system socket");
  }
  var conservation=World();var gun=conservation.Player.WeaponAt(0)!;string id=gun.ModuleId,charge=gun.ChargeId;gun.Loaded=8;gun.Active=true;conservation.Player.TurretOn=true;float magazineStock=conservation.ItemCount(charge,true),moduleStock=conservation.ItemCount(id,true);
  Check(conservation.UnfitLayoutWeapon(0)&&conservation.Player.WeaponAt(0)==null&&conservation.ItemCount(id,true)==moduleStock+1&&conservation.ItemCount(charge,true)==magazineStock+8,"Legacy weapon unfit returns module and complete magazine exactly once");
  Check(!gun.Active&&!conservation.Player.TurretOn,"Unfitting stops the removed weapon and its legacy control flag");
  Check(!conservation.UnfitLayoutWeapon(0)&&conservation.ItemCount(id,true)==moduleStock+1,"Repeated unfit cannot duplicate a module");
  var spec=FittingLayoutModel.ById[id];int slot=FittingLayoutModel.Destination(conservation.Player,spec);conservation.Player.SetStat("cpuOutput",1);float count=conservation.ItemCount(id,true);
  Check(FittingLayoutModel.Error(conservation,spec,slot).Length>0&&!FittingLayoutModel.Fit(conservation,spec,true,slot)&&conservation.ItemCount(id,true)==count,"CPU rejection keeps owned stock and empty socket unchanged");
  conservation.Screen=ViewMode.Space;Check(FittingLayoutModel.Error(conservation,spec,slot).Contains("Dock")&&!conservation.UnfitLayoutWeapon(1),"In-space refitting and removal remain blocked");
  var rigWorld=World();var rig=RigCatalog.All.First(r=>r.Size==3);rigWorld.Hangar[rig.Id]=1;Check(rigWorld.FitRig(rig.Id,true,0)&&FittingLayoutModel.Sockets(rigWorld.Player).Count(s=>s.Bank=="rig")==FittingLayoutModel.Capacity(rigWorld.Player,"rig"),"Installed rig does not shrink the drawn rig bank");
  Check(!rigWorld.RemoveRig(0)&&rigWorld.PendingRigRemoval==0&&rigWorld.Player.Rigs[0]==rig.Id,"Rig removal still requires the existing destruction confirmation");
  Console.WriteLine($"{checks} fitting layout checks passed; {FittingLayoutModel.Catalog.Length} module definitions integrated.");
 }
}
