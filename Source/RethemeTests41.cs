using System.Text.Json;
namespace StarlightSyndicateDemo;
public static class RethemeTests41
{
 public static void Run()
 {
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception("RETHEME FAIL: "+label);checks++;}
  void Clean(string value,string label)=>Check(!Retheme41.SourceLorePattern.IsMatch(value),"No source setting in "+label+": "+value);
  Check(Retheme41.Index.Build==3569502&&Retheme41.Index.Version==41,"Overlay provenance");
  foreach(var expected in new[]{("faction",27),("corporation",283),("system",8490),("station",5210),("npc",11393),("culture",11),("heritage",18),("background",43),("title",45),("school",23),("school-route",12),("attribute",5),("activity",20),("division",10)})Check(Retheme41.Index.Records.Count(r=>r.Kind==expected.Item1)==expected.Item2,"Complete "+expected.Item1);
  foreach(var r in Retheme41.Index.Records){Check(r.Name.Length>0&&r.Description.Length>0,"Display identity "+r.Key);Clean(r.Name+" "+r.Description,r.Key);foreach(string key in r.Links.Concat(r.Parent.Length>0?[r.Parent]:[]))Check(Retheme41.Records.ContainsKey(key),"Link "+r.Key+" to "+key);}
  Check(Retheme41.Index.Types.Count==UnifiedCatalog40.Types.Count,"Every source type has display lore");
  foreach(var pair in Retheme41.Index.Types){Check(UnifiedCatalog40.Types.ContainsKey(pair.Key),"Lore source identity "+pair.Key);Clean(pair.Value.Name+" "+pair.Value.Description,"type "+pair.Key);}
  Check(Retheme41.Index.Types.Values.Select(t=>t.Name).Distinct().Count()==53065,"Distinct pattern display identities");
  foreach(var item in ItemCatalog.All){Clean(item.Name+" "+item.Group+" "+item.Description,"supported item "+item.Id);Check(ItemCatalog.IsSupported(item.Id),"Support preserved "+item.Id);}
  Check(ItemCatalog.Get("df_399").Name=="Small Aegis Pulse","Authored Aegis equipment name is not mistaken for a source corporation");
  foreach(var hull in FleetCatalog.All){Clean(hull.Name+" "+hull.Description+" "+hull.Quote,"hull "+hull.Id);Check(hull.SourceTypeId!=0,"Hull source provenance preserved "+hull.Id);}
  foreach(var page in InformationCatalog.ByKey.Values){Clean(page.Name+" "+page.Group+" "+page.Description,"information "+page.Key);foreach(var tab in page.Tabs.Where(t=>!t.Key.StartsWith("Source",StringComparison.Ordinal)))foreach(var row in tab.Value)Clean(row.Label+" "+row.Value,page.Key+" / "+tab.Key);}
  Check(Retheme41.Records["faction:500004"].Name=="Boreal Federation"&&Retheme41.Records["faction:500001"].Name=="Amur Cooperative Compact"&&Retheme41.Records["faction:500003"].Name=="Suryavarta Empire"&&Retheme41.Records["faction:500002"].Name=="Azadi Confederation","Four established major equivalents");
  Check(Retheme41.Records["faction:500016"].Name=="Order of the Far Lantern"&&Retheme41.Records["faction:500018"].Name=="Meridian Legion"&&Retheme41.Records["faction:500029"].Name=="The Black Relay","Established minor equivalents");
  Check(Retheme41.Text("Vesperite ore / Asteroid Ore / ORE")=="Vesperite ore / Asteroid Ore / Kepler Resource Group","Industrial acronym does not replace the ordinary resource name");
  foreach(var mission in AgentCatalog38.Missions.Values){Clean(mission.Name+" "+mission.Briefing,"mission "+mission.Id);foreach(var enemy in mission.EnemyWaves.SelectMany(w=>w))Clean(enemy.Name,"mission enemy "+mission.Id);}
  Check(Retheme41.Text("Concord of Worlds / CONCORD / Concordance of One")=="Concord of Worlds / Arbitration / Concordance of One","JoyOS bloc remains distinct from source acronym and mind collective");
  var refInfo=UnifiedCatalog40.Information(12005);Check(refInfo.Name==Retheme41.TypeName(12005)&&!refInfo.Description.Contains("CreoDron"),"Default source pattern lore is authored");Check(refInfo.Tabs["Source audit"].Any(r=>r.Label=="Source name"&&r.Value=="Ishtar"),"Original source name intentionally retained in audit tab");
  Check(!ItemCatalog.IsSupported("sde_12005")&&refInfo.MarketId=="","Retheming never enables an unsupported identity");
  foreach(var record in Retheme41.Index.Records.Where(r=>r.Kind is "faction" or "corporation" or "culture" or "heritage" or "background" or "school" or "title"))Check(Retheme41.TryInformation(Retheme41.InformationKey(record.Key),out var e)&&e.Name==record.Name&&e.MarketId=="","Lore navigation "+record.Key);
  var world=new DemoWorld();world.NewGame(0);world.Screen=ViewMode.Station;var h=Retheme41.Records["heritage:7"];var b=Retheme41.Index.Records.First(r=>r.Kind=="background"&&r.Parent==h.Key);var s=Retheme41.Index.Records.First(r=>r.Kind=="school"&&r.Links.Contains(h.Parent));var title=Retheme41.Index.Records.First(r=>r.Kind=="title");
  var before=JsonSerializer.Serialize(new{world.Credits,world.CargoItems,world.StationItems,world.AgentsState,world.Skills,world.Industry,world.Player},FittingDesignStore.Json);
  Check(world.ChooseCharacterOrigin(h.Key,b.Key,s.Key,title.Key)==""&&world.CharacterOriginValid(),"Valid background accepted");
  var after=JsonSerializer.Serialize(new{world.Credits,world.CargoItems,world.StationItems,world.AgentsState,world.Skills,world.Industry,world.Player},FittingDesignStore.Json);Check(before==after,"Background editing changes no assets, fitting, SP, industry or standings");
  string chosen=JsonSerializer.Serialize(world.CharacterOrigin);Check(world.ChooseCharacterOrigin("heritage:1",b.Key,s.Key,title.Key).Length>0&&JsonSerializer.Serialize(world.CharacterOrigin)==chosen,"Inconsistent background rejected atomically");
  string temp=Path.Combine(Path.GetTempPath(),"starlight-lore-"+Guid.NewGuid().ToString("N"));string? old=Environment.GetEnvironmentVariable("STARLIGHT_SAVE_DIR");
  try{
   Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",temp);world.Save();var saved=File.ReadAllText(DemoWorld.SavePath);var loaded=new DemoWorld();Check(loaded.Load()&&JsonSerializer.Serialize(loaded.CharacterOrigin)==chosen,"Background and title persist through actual save/load");
   using var doc=JsonDocument.Parse(saved);var legacy=doc.RootElement.EnumerateObject().Where(p=>!p.Name.Equals("CharacterOrigin",StringComparison.OrdinalIgnoreCase)).ToDictionary(p=>p.Name,p=>p.Value.Clone());File.WriteAllText(DemoWorld.SavePath,JsonSerializer.Serialize(legacy));var migrated=new DemoWorld();Check(migrated.Load()&&migrated.CharacterOrigin==null&&migrated.SystemId==world.SystemId&&migrated.Credits==world.Credits,"Legacy save retains campaign and does not invent a background");
  }finally{Environment.SetEnvironmentVariable("STARLIGHT_SAVE_DIR",old);if(Directory.Exists(temp))Directory.Delete(temp,true);}
  Console.WriteLine($"{checks} Pass 41 retheme checks passed / 27 factions / 283 corporations / {Retheme41.Index.Types.Count} type identities.");
 }
}
