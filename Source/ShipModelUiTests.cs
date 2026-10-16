using System.Text.Json;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 string modelUiBefore="";
 void RunShipModelUiChecks(int stage)
 {
  void Click(string tip)=>TestClick(ui.Hits.Last(h=>h.Tip==tip).Rect.Center.ToVector2());
  string State()=>JsonSerializer.Serialize(new{world.Player,world.Credits,world.Day,world.CargoItems,world.StationItems},WeaponCatalog.JsonOptions);
  switch(stage){
   case 0:modelUiBefore=State();UiCheck(ui.Hits.Any(h=>h.Tip=="Toggle model wireframe")&&ui.Hits.Any(h=>h.Tip=="Cycle model preview LOD"),"Model review controls appear in fitting");break;
   case 1:Click("Toggle model wireframe");UiCheck(renderer.PreviewWireframe,"Wireframe control changes preview render mode");break;
   case 2:Click("Cycle model preview LOD");UiCheck(renderer.PreviewLod==1&&fitNotice.Contains("triangles"),"LOD control selects medium detail and reports triangles");break;
   case 3:Click("Cycle model preview LOD");UiCheck(renderer.PreviewLod==2,"LOD control selects low detail");break;
   case 4:Click("Cycle model preview LOD");Click("Toggle model wireframe");UiCheck(renderer.PreviewLod==0&&!renderer.PreviewWireframe,"Model controls cycle back to solid hero detail");UiCheck(State()==modelUiBefore,"Model review changes no campaign ship or inventory state");break;
  }
 }
}
