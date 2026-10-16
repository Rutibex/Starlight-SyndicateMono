using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int industryUiChecks;
 void RunIndustryUiChecks37(int stage)
 {
  void Check(bool value,string message){if(!value)throw new Exception("INDUSTRY UI FAIL: "+message);industryUiChecks++;Console.WriteLine("INDUSTRY UI PASS: "+message);}
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  var w=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Industry);
  switch(stage)
  {
   case 0:Check(ui.Hits.Any(h=>h.Tip=="Reprocessing plant")&&ui.Hits.Any(h=>h.Tip=="Industry [Y] / blueprints and jobs"),"station service buttons open industry and reprocessing");Check(w!=null&&ui.Hits.Any(h=>h.Tip=="START JOB"),"manufacturing setup and install action rendered");Check(ui.Hits.Any(h=>h.Tip=="Material research")&&ui.Hits.Any(h=>h.Tip=="Copying"),"research and copying activity buttons rendered");Click("START JOB");Check(world.Industry.Jobs.Count==1,"install button creates real job");break;
   case 1:Click("Jobs");break;
   case 2:Check(ui.Hits.Any(h=>h.Tip=="Cancel"),"active job exposes cancellation");world.Industry.Jobs.Last().FinishUtc=DateTime.UtcNow.AddSeconds(-1);break;
   case 3:Click("DELIVER");Check(world.Industry.Jobs.Last().State=="Delivered","deliver button completes job");break;
   case 4:Click("Blueprints");break;
   case 5:Click("Copying");break;
   case 6:Click("START JOB");Check(world.Industry.Jobs.Last().Activity==IndustryActivity.Copying,"copying UI installs copy activity");break;
   case 7:world.Industry.Jobs.Last().FinishUtc=DateTime.UtcNow.AddSeconds(-1);Click("Jobs");break;
   case 8:Click("DELIVER");Check(world.Industry.Blueprints.Any(b=>!b.Original),"copy delivery creates finite blueprint");break;
   case 9:Click("Facilities");break;
   case 10:Click("Facility: Station");Check(world.Industry.Facility=="Engineering complex","facility profile selector updates quote bonuses");break;
   case 11:var before=w!.Bounds;TestDrag(new(w.X+75,w.Y+12),new(w.X+95,w.Y+26));Check(w.X==before.X+20&&w.Y==before.Y+14,"industry window drags through real pointer handlers");break;
   case 12:TestClick(new(w!.Bounds.Right-37,w.Y+12));Check(w.Minimized,"industry window minimizes");break;
   case 13:TestClick(new(w!.Bounds.Right-37,w.Y+12));Check(!w.Minimized,"industry window restores");world.Hangar["ore"]=250;ItemMenu(new(new Rectangle(0,0,1,1),"ore",true));Check(popupActions.Any(a=>a.Label=="Reprocess stack")&&popupActions.Any(a=>a.Label=="View blueprint / manufacturing"),"inventory context menus expose crafting and reprocessing");context=false;OpenReprocessing("ore",250);break;
   case 14:Check(ui.Hits.Any(h=>h.Tip=="REPROCESS"),"reprocessing preview and confirmation rendered");Click("REPROCESS");Check(world.Hangar["ore"]==50&&reprocessSelection.Count==0,"reprocessing button preserves partial batch and clears consumed selection");break;
   case 15:Console.WriteLine($"Industry UI Pass 37: {industryUiChecks} checks passed.");break;
  }
  oldMouse=Mouse.GetState();
 }
}
