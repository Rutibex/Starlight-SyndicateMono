using System.Text.Json;
namespace StarlightSyndicateDemo;
internal static class ShipStatExport
{
 public static void Write(string path)
 {
  var ships=new List<Ship>();for(int i=0;i<3;i++){var w=new DemoWorld();w.NewGame(i);ships.Add(w.Player);}ships.Add(Ship.RaiderProfile());
  var data=ships.Select(s=>new{Hull=s.HullName,Attributes=ShipAttributeCatalog.All.Select(a=>new{a.Key,a.Label,a.Group,a.Unit,a.ReferenceId,Value=s.Attributes[a.Key]!=null?(double?)s.Stat(a.Key):null,Status=s.SimulationStatus(a.Key)})});
  File.WriteAllText(path,JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));
 }
}
