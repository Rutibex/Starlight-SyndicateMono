using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int skillsUiCheckCount;
 void RunSkillsUiChecks36(int stage)
 {
  void Check(bool b,string text){if(!b)throw new Exception("SKILL UI FAIL: "+text);skillsUiCheckCount++;Console.WriteLine("SKILL UI PASS: "+text);}
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  var w=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Skills);if(w==null&&stage<12)throw new Exception("Use --scene skills with --skills-ui-test");
  switch(stage){
   case 0:Check(!SkillCatalog.VerificationAllV,"skill UI tests use real pilot progression");Check(ui.Hits.Count(h=>h.Tip.StartsWith("Skill category / "))==24,"all 24 skill categories rendered");Check(ui.Hits.Any(h=>h.Tip=="Pause skill training"),"queue controls rendered");Click("Pause skill training");Check(world.Skills.Paused,"pause button pauses training");break;
   case 1:Click("Pause skill training");Check(!world.Skills.Paused,"resume button resumes training");Click("Skills filter");Check(skillFilter==1,"catalogue filter changes");break;
   case 2:Click("Skills search");SkillsText('c');SkillsText('p');SkillsText('u');Check(skillSearch=="cpu"&&skillFocus=="search","search text edits native field");skillFocus="";break;
   case 3:Check(ui.Hits.Count(h=>h.Tip.StartsWith("Skill row / "))==1,"search filters skill rows");skillSearch="";Click("Skills tab / Skill Plans");break;
   case 4:Check(skillTab==1&&ui.Hits.Any(h=>h.Tip=="Skill plan / cert/Explorer"),"certified plans available");Click("Skill plan / cert/Explorer");break;
   case 5:Check(skillSelectedPlan=="cert/Explorer","plan selection works");SaveQueuePlan();Check(world.Skills.Plans.Count==1,"queue saves as personal plan");break;
   case 6:Click("Skills tab / Character");Check(skillTab==2,"character attributes tab opens");break;
   case 7:Check(remapDraft!=null&&remapDraft.Count==5,"neural remap GUI rendered");Click("Skills tab / History");break;
   case 8:Check(skillTab==3,"history tab opens");Click("Skills tab / Skill Catalogue");OpenSkillInfo(3300);Check(desktop.Last().InfoKey=="skill/3300","skill opens independent info window");break;
   case 9:var info=desktop.Last();Check(ui.Hits.Any(h=>h.Tip=="Info tab / Required For"),"skill info has Required For tab");Check(ResolveInformation("skill/3300")!.Tabs["Required For"].Count>0,"reverse requirement links contain actual source items");desktop.Remove(info);break;
   case 10:var old=w!.Bounds;TestDrag(new(w.X+75,w.Y+12),new(w.X+110,w.Y+33));Check(w.X==old.X+35&&w.Y==old.Y+21,"skills window drags independently");break;
   case 11:TestClick(new(w!.Bounds.Right-37,w.Y+12));Check(w.Minimized,"skills window minimizes");break;
   case 12:var skills=desktop.First(w=>w.Kind==ViewMode.Skills);TestClick(new(skills.Bounds.Right-37,skills.Y+12));Check(!skills.Minimized,"skills window restores");break;
   case 13:Console.WriteLine($"{skillsUiCheckCount} skill UI checks passed.");break;
  }
  oldMouse=Mouse.GetState();
 }
}
