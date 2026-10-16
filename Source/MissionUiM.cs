using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int missionLevelM;string missionDivisionM="All";string missionSelectionM="m_cargo1";string chainSelectionM="horizons";
 void DrawMissionCatalogueM(DesktopWindow w,int x,int y,int width,bool coverage)
 {
  int left=Math.Clamp(width/2,330,450);if(coverage)ui.Button(new(x+left+15,y,width-left-25,29),"Special content reference inventory",()=>{missionLevelM=-1;w.ScrollJ=0;},"M / references");SearchFieldJ(w,new(x,y,left-12,29),"Search mission catalogue...");y+=39;
  for(int i=0;i<6;i++){int n=i;ui.Button(new(x+i*57,y,52,27),i==0?"All":"L"+i,()=>{missionLevelM=n;w.ScrollJ=0;},"M / level / "+i,missionLevelM==i);}y+=36;
  string[] divisions=["All","Security","Distribution","Mining"];for(int i=0;i<4;i++){string d=divisions[i];ui.Button(new(x+i*101,y,96,27),d,()=>{missionDivisionM=d;w.ScrollJ=0;},"M / division / "+d,missionDivisionM==d);}y+=39;
  if(coverage&&missionLevelM==-1){DrawReferencesM(w,x,y,width,"Special");return;}
  var rows=AgentCatalog38.Missions.Values.Where(d=>coverage||d.ProfileM!=null).Where(d=>(missionLevelM==0||d.Level==missionLevelM)&&(missionDivisionM=="All"||d.Division.ToString()==missionDivisionM)&&(d.Name.Contains(w.QueryJ,StringComparison.OrdinalIgnoreCase)||d.SourceFaction.Contains(w.QueryJ,StringComparison.OrdinalIgnoreCase))).OrderBy(d=>d.ProfileM==null?1:0).ThenBy(d=>d.Level).ThenBy(d=>d.Name).ToArray();
  ui.Text($"{rows.Length} entries / select a variant",x,y,Muted,11);y+=24;int visible=Math.Max(1,(w.Bounds.Bottom-y-45)/57);w.ScrollJ=Math.Clamp(w.ScrollJ,0,Math.Max(0,rows.Length-visible));int row=y;
  foreach(var d in rows.Skip(w.ScrollJ).Take(visible)){var def=d;ui.Button(new(x,row,left-20,51),"",()=>{missionSelectionM=def.Id;w.DetailScrollL=0;},"M / select / "+d.Id,missionSelectionM==d.Id);ui.Text(ShortInfo(d.Name,(left-36)/7),x+8,row+9,White,12);ui.Text($"L{d.Level} {d.Division} / "+(d.ProfileM==null?d.Coverage:d.Coverage=="Authored M"?"Authored":"Reviewed adaptation"),x+8,row+31,Muted,10);row+=57;}
  DrawScrollbarJ(w,new(x+left-11,y,7,visible*57),rows.Length,visible);var chosen=rows.FirstOrDefault(d=>d.Id==missionSelectionM)??rows.FirstOrDefault();if(chosen==null){if(coverage&&missionLevelM==5){DrawReferencesM(w,x,y,width,"Level 5");return;}ui.Wrap("No matching missions.",x+left+15,y,width-left-25,Muted,13);return;}
  int dx=x+left+15,dw=width-left-25;ui.Text(ShortInfo(chosen.Name,dw/10),dx,w.Y+226,Gold,17);var lines=new List<string>{chosen.Briefing,$"Level {chosen.Level} / {chosen.Division}",coverage?chosen.ProfileM?.Qualification??chosen.Coverage:"Agent assignment"};
  if(chosen.ProfileM is {} p){if(chosen.Volume>0)lines.Add($"Required cargo: {chosen.Volume:0.##} m3 / {p.CargoName}");if(p.Collateral>0)lines.Add($"Collateral held on acceptance: {p.Collateral:N0} CR; returned on successful delivery.");if(coverage)lines.Add(p.Gaps);}else lines.Add("Inherited content. Encounter source review is outstanding.");
  if(coverage){lines.Add("Source: "+chosen.Source);lines.Add("No complete 1:1 NPC or visual certification is claimed.");}
  var display=lines.SelectMany(t=>WrapLinesL(t,dw,12).Append("")).ToArray();int dy=w.Y+260,cap=Math.Max(1,(w.Bounds.Bottom-dy-108)/20);w.DetailScrollL=Math.Clamp(w.DetailScrollL,0,Math.Max(0,display.Length-cap));foreach(string line in display.Skip(w.DetailScrollL).Take(cap)){ui.Text(line,dx,dy,White,12);dy+=20;}
  ui.Button(new(dx,w.Bounds.Bottom-89,dw,29),chosen.ProfileM==null?"Find an agent for inherited content":"Request this local variant",()=>{if(chosen.ProfileM==null){EnsureWindow(ViewMode.Agents);return;}AgentAction38(()=>world.RequestReviewedM(chosen.Id));if(agentNotice38.Length==0||world.AgentsState.Missions.LastOrDefault() is {} m&&m.Definition==chosen.Id&&m.State=="Offered") {var offer=world.AgentsState.Missions.LastOrDefault(m=>m.Definition==chosen.Id&&m.State=="Offered");if(offer!=null)OpenConversationL(offer.Contact);}},"M / request / "+chosen.Id);
  ui.Text(ShortInfo(agentNotice38,width/7),x,w.Bounds.Bottom-29,Gold,11);
 }
 void DrawReferencesM(DesktopWindow w,int x,int y,int width,string kind)
 {
  var rows=MissionCatalogM.References.Where(r=>(r.Kind==kind||kind=="Special"&&r.Kind!="Level 5")&&r.Name.Contains(w.QueryJ,StringComparison.OrdinalIgnoreCase)).ToArray();ui.Text($"{rows.Length} reference entries / encounters not implemented",x,y,Gold,13);y+=32;int visible=Math.Max(1,(w.Bounds.Bottom-y-42)/42);w.ScrollJ=Math.Clamp(w.ScrollJ,0,Math.Max(0,rows.Length-visible));foreach(var r in rows.Skip(w.ScrollJ).Take(visible)){ui.Text(ShortInfo(r.Name,width/8),x+8,y,White,12);ui.Text("Reference / no playable offer",x+8,y+20,Muted,10);y+=42;}DrawScrollbarJ(w,new(w.Bounds.Right-14,w.Y+330,7,Math.Max(40,w.Height-370)),rows.Length,visible);
 }
 void DrawChainsM(DesktopWindow w,int x,int y,int width)
 {
  world.UpdateChainsM();int left=300;int row=y;foreach(var d in MissionCatalogM.Chains){var chain=d;ui.Button(new(x,row,left-15,56),"",()=>{chainSelectionM=chain.Id;w.DetailScrollL=0;},"M / chain / "+d.Id,chainSelectionM==d.Id);ui.Text(ShortInfo(d.Name,36),x+8,row+10,White,12);ui.Text(d.Kind+" / "+ShortInfo(world.ChainAvailabilityM(d),36),x+8,row+33,Muted,10);row+=62;}
  int dx=x+left+16,dw=width-left-28;var chosen=MissionCatalogM.Chains.First(d=>d.Id==chainSelectionM);ui.Text(ShortInfo(chosen.Name,dw/10),dx,y,Gold,18);y+=34;ui.Wrap(chosen.Briefing,dx,y,dw,White,13);y+=95;
  if(world.AgentsState.ChainsM.LastOrDefault(r=>r.Definition.Id==chosen.Id) is {} contactRun)ui.Button(new(dx+dw-165,y-34,165,26),"Contact destination",()=>world.SetDestination(Stations44.SystemOf(contactRun.Origin)),"M / chain route");
  ui.Text(chosen.Once?"One-time chain / failure closes this contact":$"Replay interval / {chosen.RepeatDays} local days",dx,y,Cyan,12);y+=28;
  var run=world.AgentsState.ChainsM.LastOrDefault(r=>r.Definition.Id==chosen.Id);var lines=new List<string>{world.ChainAvailabilityM(chosen)};if(run!=null){lines.Add("Progress: "+run.Completed.Count+" completed steps");foreach(var step in run.Definition.Steps){var def=AgentCatalog38.Missions[step.Mission];lines.Add((run.Completed.Contains(step.Id)?"Complete / ":run.Step==step.Id?"Current / ":"Pending or alternate / ")+def.Name);}foreach(var choice in run.Choices)lines.Add("Chosen path / "+choice.Value);}
  var display=lines.SelectMany(t=>WrapLinesL(t,dw,12)).ToArray();int cap=Math.Max(1,(w.Bounds.Bottom-y-112)/22);w.DetailScrollL=Math.Clamp(w.DetailScrollL,0,Math.Max(0,display.Length-cap));foreach(var line in display.Skip(w.DetailScrollL).Take(cap)){ui.Text(line,dx,y,White,12);y+=22;}
  if(run is {State:"Active"}){
   if(run.Mission.Length>0){var m=world.AgentsState.Missions.First(m=>m.Id==run.Mission);ui.Button(new(dx,w.Bounds.Bottom-89,dw,30),"Open current step",()=>OpenConversationL(m.Contact),"M / chain current");}
   else{var step=run.Definition.Steps.Single(s=>s.Id==run.Step);ui.Button(new(dx,w.Bounds.Bottom-89,dw,30),step.Branches.Count>0?"Choose next path":"Request next step",()=>{if(step.Branches.Count>0)ShowPopup("Choose your path",step.Branches.Keys.Select(branch=>(branch,(Action)(()=>ContinueChainUiM(run.Id,branch)))).ToArray());else ContinueChainUiM(run.Id,"");},"M / chain continue");}
  }else ui.Button(new(dx,w.Bounds.Bottom-89,dw,30),"Start chain",()=>{AgentAction38(()=>world.StartChainM(chosen.Id));var r=world.AgentsState.ChainsM.LastOrDefault(r=>r.Definition.Id==chosen.Id&&r.State=="Active");if(r?.Mission.Length>0)OpenConversationL(world.AgentsState.Missions.First(m=>m.Id==r.Mission).Contact);},"M / chain start");
  ui.Text(ShortInfo(agentNotice38,width/7),x,w.Bounds.Bottom-29,Gold,11);
 }
 void ContinueChainUiM(string id,string branch){AgentAction38(()=>world.ContinueChainM(id,branch));var run=world.AgentsState.ChainsM.First(r=>r.Id==id);if(run.Mission.Length>0)OpenConversationL(world.AgentsState.Missions.First(m=>m.Id==run.Mission).Contact);}
}
