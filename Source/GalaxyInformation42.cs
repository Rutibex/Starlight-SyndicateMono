using System.Globalization;
namespace StarlightSyndicateDemo;
public static class GalaxyInformation42
{
 static void PlacePosition(InformationEntry entry,GalaxyPlace42 p){for(int axis=0;axis<3;axis++)entry.Add("Galaxy chart","3D "+"XYZ"[axis],(p.Position[axis]/Galaxy42.LightYear).ToString("0.000000",CultureInfo.InvariantCulture)+" light years");entry.Add("Galaxy chart","Flat coordinates",p.FlatSource);}
 public static void Attach(InformationEntry entry)
 {
  if(entry.Tabs.ContainsKey("Galaxy chart"))return;
  if(entry.Id.StartsWith("system:")&&int.TryParse(entry.Id[7..],out int id)&&Galaxy42.ById.TryGetValue(id,out var s))
  {
   entry.Add("Galaxy chart","Security",s.Security.ToString("0.000000",CultureInfo.InvariantCulture));entry.Add("Galaxy chart","Region",Galaxy42.Regions[s.RegionId].Name,"lore/region:"+s.RegionId);entry.Add("Galaxy chart","Constellation",Galaxy42.Constellations[s.ConstellationId].Name,"lore/constellation:"+s.ConstellationId);entry.Add("Galaxy chart","Static component",s.Component.ToString());entry.Add("Galaxy chart","Gate links",s.Neighbours.Length.ToString());entry.Add("Galaxy chart","Flat coordinates",s.FlatSource);
   for(int axis=0;axis<3;axis++)entry.Add("Galaxy chart","3D "+"XYZ"[axis],(s.Position[axis]/Galaxy42.LightYear).ToString("0.000000",CultureInfo.InvariantCulture)+" light years");
   for(int axis=0;axis<2;axis++)entry.Add("Galaxy chart","2D "+"XY"[axis],(s.Flat[axis]/Galaxy42.LightYear).ToString("0.000000",CultureInfo.InvariantCulture)+" light years");
   foreach(int n in s.Neighbours)entry.Add("Gate network",Galaxy42.ById[n].Name,"Connected system / "+n,"lore/system:"+n);
  }
  else if(entry.Id.StartsWith("region:")&&int.TryParse(entry.Id[7..],out int region)&&Galaxy42.Regions.TryGetValue(region,out var r))
  {PlacePosition(entry,r);entry.Add("Galaxy chart","Constellations",r.Members.Length.ToString());foreach(int member in r.Members)entry.Add("Galaxy chart",Galaxy42.Constellations[member].Name,"Constellation / "+member,"lore/constellation:"+member);}
  else if(entry.Id.StartsWith("constellation:")&&int.TryParse(entry.Id[14..],out int constellation)&&Galaxy42.Constellations.TryGetValue(constellation,out var c))
  {PlacePosition(entry,c);entry.Add("Galaxy chart","Region",Galaxy42.Regions[c.ParentId].Name,"lore/region:"+c.ParentId);foreach(int member in c.Members)entry.Add("Galaxy chart",Galaxy42.ById[member].Name,"Solar system / "+member,"lore/system:"+member);}
 }
}
