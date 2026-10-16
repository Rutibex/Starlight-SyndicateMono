using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunShipStatUiChecks(int stage)
 {
  void Click(string label)=>TestClick(ui.Hits.Last(h=>h.Tip==label).Rect.Center.ToVector2());
  switch(stage)
  {
   case 0:UiCheck(InfoTabs.All(t=>ui.Hits.Any(h=>h.Tip==t)),"All seven ship information tabs are reachable");Click("Attributes");UiCheck(shipInfoTab==1,"Attributes opens");break;
   case 1:Click("Source fields");UiCheck(shipInfoRaw,"Source fields toggles");Click("Next page");UiCheck(shipInfoPage==1,"Attribute paging works");break;
   case 2:Click("Fitting");UiCheck(shipInfoTab==2&&shipInfoPage==0,"Fitting opens and resets page");Click("Rotate right");UiCheck(shipPreviewYaw>.7f,"Preview rotates independently");break;
   case 3:Click("Skills");UiCheck(shipInfoTab==3,"Skills opens");break;
   case 4:Click("Variations");UiCheck(shipInfoTab==4,"Variations opens");break;
   case 10:Click("Compare all");UiCheck(shipInfoCompare,"Variation comparison opens");break;
   case 5:Click("Industry");UiCheck(shipInfoTab==5,"Industry opens");break;
   case 6:Click("SKINs");UiCheck(shipInfoTab==6,"SKIN collection opens");ShowShipInformation(FleetCatalog.Source("Kestrel").Id);UiCheck(shipInfoId==FleetCatalog.Source("Kestrel").Id,"Other registry hull opens");break;
   case 7:Click("Back");UiCheck(shipInfoId!=FleetCatalog.Source("Kestrel").Id,"History returns previous hull");Click("Forward");UiCheck(shipInfoId==FleetCatalog.Source("Kestrel").Id,"History advances");break;
   case 8:var w=desktop.Single(w=>w.Kind==ViewMode.ShipInfo);var before=w.Bounds;TestDrag(new(w.X+100,w.Y+12),new(w.X+125,w.Y+24));UiCheck(w.X==before.X+25,"Info window remains movable");break;
   case 11:ShowCurrentShipInfo();Click("Fitting");break;
   case 12:Click("Use current fit");UiCheck(shipInfoFitted,"Current fit toggle responds");var preview=ui.Hits.Last(h=>h.Tip=="Drag to rotate / wheel to zoom").Rect;float cameraYaw=renderer.Yaw,previewYaw=shipPreviewYaw;TestDrag(preview.Center.ToVector2(),preview.Center.ToVector2()+new Vector2(32,18));UiCheck(shipPreviewYaw!=previewYaw&&renderer.Yaw==cameraYaw,"Dragging preview leaves world camera unchanged");Console.WriteLine("18 ship information UI checks passed.");break;
   case 9:Click("Overview");UiCheck(shipInfoTab==0,"Tabs work after moving window");Click("Variations");break;
  }
  oldMouse=Microsoft.Xna.Framework.Input.Mouse.GetState();
 }
}
