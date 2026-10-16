using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 int spatialUiChecks43;string spatialLegacyBytes43="",spatialBelt43="",spatialSourceBytes43="";
 void RunSpatialUiChecks43(int stage)
 {
  void Check(bool value,string why){UiCheck(value,"Pass43: "+why);spatialUiChecks43++;}
  void Click(string tip){var h=ui.Hits.Last(h=>h.Tip==tip);if(h.Owner.Length==0)h.Action();else TestClick(h.Rect.Center.ToVector2());}
  switch(stage)
  {
   case 0:Check(!world.ImportedWorld43&&desktop.Last().Kind==ViewMode.Galaxy,"explicit source entry offered from legacy chart");world.Save();spatialLegacyBytes43=File.ReadAllText(DemoWorld.SavePath);Click("Pass43 new imported exploration; separate save");break;
   case 1:Check(world.ImportedWorld43&&world.Screen==ViewMode.Space&&desktop.Count==0,"native entry launches source flight");Check(File.ReadAllText(DemoWorld.SavePath)==spatialLegacyBytes43,"legacy bytes untouched");Click("Overview / All");Check(SpaceRows().Any(r=>r.Id.StartsWith("moon:"))&&SpaceRows().Any(r=>r.Id.StartsWith("stargate:")),"source overview includes moons and gates");spatialBelt43=world.SolarCelestial;world.SelectedId=spatialBelt43;Click("Show object information");break;
   case 2:Check(desktop.Last().InfoKey=="celestial/"+world.SystemId+"/"+spatialBelt43,"source body native Show Info");Click("Info tab / Attributes");break;
   case 3:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs["Attributes"].Any(r=>r.Label=="Local metres"),"exact local coordinate information available");Click("Info tab / Source audit");break;
   case 4:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs["Source audit"].Any(r=>r.Value=="2019-11-28"),"historical belt provenance accessible");Click("Info tab / Relationships");break;
   case 5:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs["Relationships"].Any(r=>r.Link.Contains("/planet:")),"orbit hierarchy links to imported parent");Close();Click("Open solar system map");break;
   case 6:Check(desktop.Last().Kind==ViewMode.SolarMap&&ui.Hits.Any(h=>h.Tip=="Solar system Show Info"),"source map and information control drawn");world.SelectedId=world.Celestials.First(c=>c.Kind==CelestialKind.Planet).Id;Click("WARP TO SELECTED");break;
   case 7:Check(world.Transit!=null,"real map warp button launches source transit");world.Transit=null;Close();OpenGalaxy42();break;
   case 8:Click("Galaxy kind / Landmarks");break;
   case 9:Check(galaxyResults42.Length==45,"all galactic landmarks browsable");Click("Galaxy result / landmark:"+galaxyResults42.First().Id);break;
   case 10:Check(desktop.Last().InfoKey.StartsWith("landmark43/"),"native landmark information opens");Click("Info tab / Attributes");break;
   case 11:Check(ResolveInformation(desktop.Last().InfoKey)!.Tabs["Attributes"].Any(r=>r.Label=="Local warp position"&&r.Value.Contains("Not supplied")),"galactic positions never become invented local destinations");Close();world.Save();Click("Pass43 resume imported exploration");break;
   case 12:Check(world.ImportedWorld43&&world.SolarCelestial==spatialBelt43&&desktop.Count==0,"resume restores exact original belt");Check(File.ReadAllText(DemoWorld.SavePath)==spatialLegacyBytes43,"source save and resume preserve legacy campaign");spatialSourceBytes43=File.ReadAllText(DemoWorld.ImportedSavePath43);world.Screen=ViewMode.Title;break;
   case 13:Click("Pass43 imported exploration selection");break;
   case 14:Check(world.ImportedWorld43&&desktop.Last().Kind==ViewMode.Galaxy,"title exploration button opens a usable source chart");Click("Pass43 resume imported exploration");Check(File.ReadAllText(DemoWorld.ImportedSavePath43)==spatialSourceBytes43,"resume from title preview does not overwrite prior source save");Console.WriteLine($"Pass43 spatial UI: {spatialUiChecks43} checks passed.");break;
  }
 }
}
