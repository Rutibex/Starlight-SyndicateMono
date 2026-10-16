using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int galaxyUiChecks42;
 void RunGalaxyUiChecks42(int stage)
 {
  if(stage>17)return;void Check(bool ok,string message){UiCheck(ok,"Pass 42: "+message);galaxyUiChecks42++;}void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  switch(stage)
  {
   case 0:Check(desktop.Last().Kind==ViewMode.Galaxy,"imported galaxy window opens");Check(galaxyVisible42.Length==5268&&galaxyDrawNodes42>4000,"full gated space is drawn");Click("Galaxy search");foreach(char c in "30000002")GalaxyText42(c);Check(galaxyFocus42&&galaxyQuery42=="30000002","search text entry");break;
   case 1:Check(galaxyResults42.Length==1,"ID search returns exact system");Click("Galaxy result / system:30000002");Check(Plan42.Selected==30000002,"system selection via real UI");Click("Galaxy route destination");break;
   case 2:Check(galaxyRoute42.Found&&galaxyRoute42.Systems[0]==30000001&&galaxyRoute42.Systems[^1]==30000002,"shortest route is populated");Click("Galaxy route mode / Safer");break;
   case 3:Check(Plan42.Mode==GalaxyRouteMode42.Safer&&galaxyRoute42.Found,"safer route button recalculates");Click("Galaxy projection");break;
   case 4:Check(galaxyThreeD42&&galaxyDrawNodes42>0,"3D projection draws full graph");TestClick(galaxyPoints42[30000002]);Check(Plan42.Selected==30000002,"map node click selects exact system");var center=galaxyViewport42.Center.ToVector2();double before=galaxyCenterX42;TestDrag(center,center+new Vector2(28,14));Check(galaxyCenterX42!=before,"map drag pans camera");double zoom=galaxyZoom42;mouse=center;var wheel=new Microsoft.Xna.Framework.Input.MouseState((int)center.X,(int)center.Y,120,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released,Microsoft.Xna.Framework.Input.ButtonState.Released);DesktopInput(wheel,new Microsoft.Xna.Framework.Input.KeyboardState());oldMouse=wheel;Check(galaxyZoom42>zoom,"wheel zoom changes camera scale");double yaw=galaxyYaw42;TestPointer(center);TestPointer(center,right:Microsoft.Xna.Framework.Input.ButtonState.Pressed);TestPointer(center+new Vector2(20,10),right:Microsoft.Xna.Framework.Input.ButtonState.Pressed);TestPointer(center+new Vector2(20,10));Check(galaxyYaw42!=yaw,"right drag orbits 3D projection");Click("Galaxy focus region");break;
   case 5:Check(galaxyRegion42==Galaxy42.ById[30000002].RegionId&&galaxyVisible42.All(s=>s.RegionId==galaxyRegion42),"region scope filters geometry");Click("Galaxy selected info");break;
   case 6:Check(desktop.Last().InfoKey=="lore/system:30000002","selected system information links to rethemed biography");Close();Click("Galaxy all regions");Click("Galaxy clear search");Click("Galaxy connections / No gates");break;
   case 7:Check(galaxyVisible42.Length==3222&&galaxyVisible42.All(s=>s.Neighbours.Length==0),"isolated systems remain browsable");var target=galaxyResults42.First();Click("Galaxy result / system:"+target.Id);Click("Galaxy route destination");break;
   case 8:Check(!galaxyRoute42.Found&&galaxyRoute42.Reason.Contains("separate"),"disconnected destination explains absent route");Click("Galaxy connections / With gates");Click("Galaxy security / High");break;
   case 9:Check(galaxyVisible42.Length>0&&galaxyVisible42.All(s=>s.Security>=.45),"security filter applies");Click("Galaxy security / All security");Click("Galaxy kind / Regions");break;
   case 10:Check(galaxyResults42.Length==114,"region search includes all regions");Click("Galaxy result / region:"+galaxyResults42.First().Id);break;
   case 11:Check(galaxyRegion42!=0&&galaxyKind42==0,"region result opens scoped system map");var win=desktop.Last(w=>w.Kind==ViewMode.Galaxy);int width=win.Width;TestDrag(new Vector2(win.Bounds.Right-5,win.Bounds.Bottom-5),new Vector2(win.Bounds.Right-185,win.Bounds.Bottom-95));Check(win.Width<width,"map window resizes through native input");Check(world.SystemId=="aster"&&world.WorldDefinitionId==DemoWorld.LegacyWorld42,"chart planning never relocates pilot");break;
   case 12:Click("Galaxy save plan");Check(world.GalaxyPlan?.Valid()==true&&System.IO.File.Exists(DemoWorld.SavePath),"galaxy plan saved with valid stable IDs");Click("Galaxy kind / Constellations");break;
   case 13:Check(galaxyResults42.Length==Galaxy42.Data.Constellations.Count(c=>c.ParentId==galaxyRegion42),"constellation search respects region scope");Click("Galaxy result / constellation:"+galaxyResults42.First().Id);break;
   case 14:Check(galaxyConstellation42!=0&&galaxyVisible42.All(s=>s.ConstellationId==galaxyConstellation42),"constellation selection scopes its own systems");Click("Galaxy selected info");break;
   case 15:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs.ContainsKey("Galaxy chart"),"system information exposes graph and coordinate metadata");Close();Click("Galaxy legacy campaign");break;
   case 16:Check(desktop.Last().Kind==ViewMode.Map&&DemoWorld.Systems.Length==9,"legacy campaign map remains selectable");Click("Open imported galaxy");break;
   case 17:Check(desktop.Last().Kind==ViewMode.Galaxy,"legacy map links back to full galaxy");Console.WriteLine($"Pass 42 galaxy UI: {galaxyUiChecks42} checks passed.");break;
  }
 }
}
