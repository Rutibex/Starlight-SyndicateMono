using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int rigPage,rigSize=1;string rigGroup="ALL",rigSelected="";
 void RigWindow()
 {
  var p=world.Player;var groups=new[]{"ALL"}.Concat(RigCatalog.All.Select(d=>d.Group).Distinct()).ToArray();var defs=RigCatalog.All.Where(d=>d.Size==rigSize&&(rigGroup=="ALL"||d.Group==rigGroup)).ToArray();rigPage=Math.Clamp(rigPage,0,Math.Max(0,(defs.Length-1)/9));var d=RigCatalog.ById.GetValueOrDefault(rigSelected)??defs.FirstOrDefault()??RigCatalog.All[0];int slot=world.FittingRigSlot;
  ui.Text(p.HullName+" / PERMANENT HULL UPGRADES",237,126,White,18);ui.Text($"{RigCatalog.SizeName((int)p.Stat("rigSize"))} hull / calibration {p.RigCalibration:0}/{p.Stat("upgradeCapacity"):0} / free sockets {p.Stat("upgradeSlotsLeft"):0} / CPU {world.TotalFitCpu:0}/{p.Stat("cpuOutput"):0} / grid {world.TotalFitPower:0}/{p.Stat("powerOutput"):0}",237,158,Cyan,12);
  for(int i=1;i<=4;i++){int n=i;ui.Button(new(237+(i-1)*245,183,235,29),RigCatalog.SizeName(i),()=>{rigSize=n;rigPage=0;rigSelected="";},active:rigSize==i);}
  ui.Button(new(237,222,978,27),"CATEGORY: "+rigGroup,()=>{rigGroup=groups[(Array.IndexOf(groups,rigGroup)+1)%groups.Length];rigPage=0;rigSelected="";});
  for(int i=0;i<3;i++){int n=i;var fitted=RigCatalog.ById.GetValueOrDefault(p.Rigs[i]);ui.Button(new(237+i*326,264,316,40),$"RIG {i+1}: "+(fitted?.Name??"EMPTY"),()=>world.FittingRigSlot=n,"Rig socket / "+i,active:slot==i);}
  int y=322;foreach(var def in defs.Skip(rigPage*9).Take(9)){var selected=def;ui.Button(new(237,y,320,29),def.Name[..Math.Min(36,def.Name.Length)],()=>rigSelected=selected.Id,def.Description,active:def.Id==d.Id);y+=33;}
  ui.Button(new(237,629,90,27),"PREV",()=>rigPage=Math.Max(0,rigPage-1));ui.Text($"{rigPage+1}/{Math.Max(1,(defs.Length+8)/9)} / {defs.Length} rigs",339,638,Muted,12);ui.Button(new(467,629,90,27),"NEXT",()=>rigPage++);
  ui.Text(d.Name,576,323,Gold,17);ui.Wrap(d.Description,576,359,635,White,12);ui.Wrap(world.RigFitError(d,slot),576,474,635,Red,12);
  ui.Text("INSTALLED: "+(RigCatalog.ById.GetValueOrDefault(p.Rigs[slot])?.Name??"empty"),576,527,Cyan,12);
  ui.Button(new(576,562,201,32),"INSTALL SELECTED",()=>world.FitRig(d.Id,true,world.FittingRigSlot));ui.Button(new(787,562,201,32),"DESTROY INSTALLED",()=>world.RemoveRig(world.FittingRigSlot));ui.Button(new(998,562,217,32),"MARKET / DETAILS",()=>{catalogSelection=Array.FindIndex(ItemCatalog.All,i=>i.Id==d.Id);Open(ViewMode.Market);});
  if(world.PendingRigRemoval>=0){int pending=world.PendingRigRemoval;ui.Wrap("Destroy rig "+(pending+1)+" permanently? No item is returned.",576,610,635,Red,14);ui.Button(new(576,654,310,29),"CONFIRM DESTRUCTION",()=>world.RemoveRig(pending,true));ui.Button(new(896,654,319,29),"CANCEL",()=>world.PendingRigRemoval=-1);}
  else {int level=p.RiggingSkills.GetValueOrDefault(d.Group);ui.Text($"{d.Group} rigging skill: {level}/5 / drawback multiplier {1-.1*level:0%}",576,617,Muted,12);if(!world.WeaponTestMode)ui.Button(new(576,654,639,29),$"TRAIN RIGGING / {1000*(level+1)*(level+1)} CR",()=>world.TrainRiggingSkill(d.Group));if(world.WeaponTestMode)ui.Button(new(576,654,639,29),"LOAN: CYCLE SELECTED CATEGORY SKILL",()=>world.SetRiggingSkill(d.Group,(level+1)%6));}
  ui.Text("326 size-specific rigs / 83 families / drag inventory onto selected rig socket / Alt+R",237,708,Muted,12);
  string[] hulls=["frigate","cruiser","battleship","carrier"];for(int i=0;i<4;i++){string id=hulls[i];int sz=i+1;ui.Button(new(237+i*196,732,186,28),"LOAN "+RigCatalog.SizeName(sz),()=>{world.RigTrial(id);rigSize=sz;rigPage=0;rigSelected="";});}ui.Button(new(1021,732,194,28),"INVENTORY",()=>Open(ViewMode.Inventory));
 }
 void RigMenu(){var hit=ui.Hits.LastOrDefault(h=>h.Owner==ViewMode.Rigs.ToString()&&h.Rect.Contains(mouse.ToPoint())&&h.Tip.StartsWith("Rig socket / "));if(hit!=null&&int.TryParse(hit.Tip[13..],out int n))world.FittingRigSlot=n;ShowPopup("Rig socket",[("Install selected",()=>{if(RigCatalog.ById.ContainsKey(rigSelected))world.FitRig(rigSelected,true,world.FittingRigSlot);}), ("Request destruction",()=>world.RemoveRig(world.FittingRigSlot)),("Show Info",()=>ShowItemInfo(world.Player.Rigs[world.FittingRigSlot].Length>0?world.Player.Rigs[world.FittingRigSlot]:rigSelected)),("Inventory",()=>Open(ViewMode.Inventory))]);}
}
