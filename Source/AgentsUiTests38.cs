using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int agentsUiChecks38;
 void RunAgentsUiChecks38(int stage)
 {
  void Check(bool value,string label){if(!value)throw new Exception("AGENTS UI FAIL: "+label);Console.WriteLine("AGENTS UI PASS: "+label);agentsUiChecks38++;}
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  var m=world.AgentsState.Missions.Last();
  switch(stage)
  {
   case 0:Check(ui.Hits.Any(h=>h.Tip=="Agent finder")&&ui.Hits.Any(h=>h.Tip=="Opportunities")&&ui.Hits.Any(h=>h.Tip=="Station tab / Agents")&&ui.Hits.Any(h=>h.Tip=="Station tab / Offices")&&ui.Hits.Any(h=>h.Tip=="Station tab / Hangars"),"station icon and tab controls rendered");Check(ui.Hits.Any(h=>h.Tip=="Accept")&&ui.Hits.Any(h=>h.Tip=="Delay")&&ui.Hits.Any(h=>h.Tip=="Decline"),"offer exposes all three conversation actions");Click("Accept");Check(m.State=="Accepted","real pointer accepts mission");break;
   case 1:Click("Load mission cargo");Check(m.Cargo>0&&m.Cargo<m.Spec.Volume&&m.Hangars.GetValueOrDefault(world.SystemId)>0,"cargo control loads only free hold capacity");break;
   case 2:Click("Complete");Check(m.State=="Accepted"&&agentNotice38.Length>0,"incomplete mission cannot pay reward through UI");Click("Standings");break;
   case 3:Check(desktop.Last().Kind==ViewMode.Standings&&ui.Hits.Any(h=>h.Tip=="Transactions"),"standings window opened through conversation");var sw=desktop.Last();TestClick(new(sw.Bounds.Right-37,sw.Y+12));Check(sw.Minimized,"standings window minimizes through real input");break;
   case 4:var w=desktop.Last();TestClick(new(w.Bounds.Right-37,w.Y+12));Check(!w.Minimized,"standings window restores through real input");break;
   case 5:var dragWindow=desktop.Last();int bx=dragWindow.X,by=dragWindow.Y;TestDrag(new(bx+80,by+12),new(bx+100,by+22));Check(dragWindow.X==bx+20&&dragWindow.Y==by+10,"standings window drags");OpenJournalL(m.Id);break;
   case 6:Check(ui.Hits.Any(h=>h.Tip=="L / load cargo")&&ui.Hits.Any(h=>h.Tip=="L / travel"),"opportunities exposes cargo and navigation controls");Click("L / route");Check(world.Waypoints.Contains(m.Destination),"mission route sets real navigation waypoints");break;
   case 7:Click("L / unload cargo");Check(m.Cargo==0&&m.Hangars[world.SystemId]==m.Spec.Volume,"cargo unload restores local contract stack");break;
   case 8:OpenConversationL(m.Contact);break;
   case 9:Click("L / quit");Check(context&&popupActions.Any(a=>a.Label=="Abandon"),"quit control explains standings penalty before abandonment");context=false;break;
   case 10:var resized=desktop.Last();int ow=resized.Width,oh=resized.Height;TestDrag(new(resized.Bounds.Right-7,resized.Bounds.Bottom-7),new(resized.Bounds.Right+23,resized.Bounds.Bottom+13));Check(resized.Width==ow+30&&resized.Height==oh+20,"mission window resizes through real input");break;
   case 11:Console.WriteLine($"Agents UI Pass 38: {agentsUiChecks38} checks passed.");break;
  }
  oldMouse=Mouse.GetState();
 }
}
