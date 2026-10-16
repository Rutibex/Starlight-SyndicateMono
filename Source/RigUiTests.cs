using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int rigUiChecks;
 void RunRigUiChecks(int stage)
 {
  void Check(bool ok,string n){if(!ok)throw new Exception("RIG UI FAIL: "+n);rigUiChecks++;Console.WriteLine("UI PASS: "+n);}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal)).Rect.Center.ToVector2());var win=desktop.First(x=>x.Kind==ViewMode.Rigs);
  switch(stage)
  {
   case 0:Check(RigCatalog.All.Length==326,"Rigs desktop catalog");int x=win.X;TestDrag(new(win.X+80,win.Y+12),new(win.X+100,win.Y+22));Check(win.X==x+20,"Rig window moves");int width=win.Width;TestDrag(new(win.Bounds.Right-5,win.Bounds.Bottom-5),new(win.Bounds.Right+15,win.Bounds.Bottom+5));Check(win.Width==width+20,"Rig window resizes");break;
   case 1:Click("INSTALL SELECTED");break;
   case 2:Check(world.Player.Rigs[0].Length>0&&world.Player.RigCalibration==150,"UI consumes stock and calibration");Click("Rig socket / 1");Click("INSTALL SELECTED");break;
   case 3:Check(world.Player.Rigs[1].Length>0&&world.Player.RigCalibration==300,"UI second rig socket");Click("Rig socket / 2");Click("INSTALL SELECTED");break;
   case 4:Check(world.Player.Rigs[2]=="","UI calibration rejection");Click("Rig socket / 0");Click("DESTROY INSTALLED");break;
   case 5:Check(world.PendingRigRemoval==0&&world.Player.Rigs[0].Length>0,"Destruction confirmation prompt");Click("CANCEL");break;
   case 6:Check(world.PendingRigRemoval==-1&&world.Player.Rigs[0].Length>0,"Destruction cancelled safely");Click("DESTROY INSTALLED");break;
   case 7:Click("CONFIRM DESTRUCTION");break;
   case 8:Check(world.Player.Rigs[0]==""&&world.Player.RigCalibration==150,"Confirmed destruction frees calibration");Click("Medium");break;
   case 9:Click("INSTALL SELECTED");Check(world.Player.Rigs[0]=="","UI refuses incorrect rig size");Click("LOAN Medium");break;
   case 10:Check(world.Player.Stat("rigSize")==2&&rigSize==2,"Medium hull and rig browser");Click("INSTALL SELECTED");break;
   case 11:Check(world.Player.Rigs[0].Length>0,"Medium rig installed");world.FittingRigSlot=1;EnsureWindow(ViewMode.Inventory);showHangar=true;inventorySelection=2;string id=RigCatalog.All.First(d=>d.Size==2&&d.Family=="Auxiliary Thrusters").Id;inventoryScroll=Array.IndexOf(DemoWorld.InventoryIds.Where(id=>world.ItemCount(id,true)>.001f).ToArray(),id);break;
   case 12:string itemId=RigCatalog.All.First(d=>d.Size==2&&d.Family=="Auxiliary Thrusters").Id;var item=itemHits.First(h=>h.Id==itemId);TestDrag(item.Rect.Center.ToVector2(),new(1160,425));break;
   case 13:Check(world.Player.InstalledRigs.Any(d=>d.Family=="Auxiliary Thrusters"),"Inventory drag installs into selected rig socket");desktop.RemoveAll(w=>w.Kind==ViewMode.Inventory);break;
   case 14:TestPointer(new(win.X+600,win.Y+205),right:ButtonState.Pressed);TestPointer(new(win.X+600,win.Y+205));Check(context&&popupActions.Any(a=>a.Label=="Request destruction"),"Rig context menu");break;
   case 15:Click("Request destruction");break;
   case 16:Click("CONFIRM DESTRUCTION");break;
   case 17:Check(world.Player.Rigs[1]=="","Context destruction executes confirmation");world.Save();var loaded=new DemoWorld();Check(loaded.Load()&&loaded.Player.Rigs.SequenceEqual(world.Player.Rigs),"UI rig fitting persists");Click("LOAN: CYCLE");break;
   case 18:Check(world.Player.RiggingSkills.GetValueOrDefault("Core")==1,"Loan rigging skill control");Click("MARKET / DETAILS");break;
   case 19:Check(desktop.Last().Kind==ViewMode.Market,"Rig market navigation");desktop.RemoveAll(w=>w.Kind==ViewMode.Market);break;
   case 20:Click("LOAN Capital");break;
   case 21:Check(world.Player.Stat("rigSize")==4&&rigSize==4,"Capital hull and browser");Click("INSTALL SELECTED");break;
   case 22:Check(world.Player.Rigs[0].Length>0&&world.Player.InstalledRigs.First().Size==4,"Capital rig installation");break;
   case 26:Console.WriteLine($"{rigUiChecks} rig graphical interaction checks passed.");break;
  }oldMouse=Mouse.GetState();
 }
}
