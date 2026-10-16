using Microsoft.Xna.Framework;
using System.Text.Json;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed class MarketGroup
{
 public int? SourceId; public string Path="",Name="";public List<MarketGroup> Children=[];public List<int> Items=[];
 public IEnumerable<int> Descendants()=>Items.Concat(Children.SelectMany(c=>c.Descendants()));
}
public static class MarketGroups
{
 public static readonly MarketGroup Root=Build();
 public static string PathFor(ItemDefinition item)=>item.SourceType?.MarketGroupId is int m?UnifiedCatalog40.MarketPaths[m]:AuthoredPathFor(item);
 public static string AuthoredPathFor(ItemDefinition item)=>item.Slot=="ship"?item.Group:item.Group.StartsWith("Ammunition")?"Ammunition & Charges / "+item.Group[10..].Trim(' ','/'):item.Group.StartsWith("Materials")?item.Group:item.Group.StartsWith("Drones")?item.Group:item.Group.StartsWith("Rigs")?"Ship and Module Modifications / "+item.Group:item.Group.StartsWith("Consumables")?item.Group:"Ship Equipment / "+item.Group;
 static MarketGroup Build(){
  var root=new MarketGroup();var source=UnifiedCatalog40.Index.Markets.ToDictionary(n=>n.Id,n=>new MarketGroup{SourceId=n.Id,Name=Retheme41.Taxonomy("market",n.Id),Path=UnifiedCatalog40.MarketPaths[n.Id]});
  foreach(var n in UnifiedCatalog40.Index.Markets){if(n.ParentId is int parent)source[parent].Children.Add(source[n.Id]);else root.Children.Add(source[n.Id]);}
  for(int i=0;i<ItemCatalog.All.Length;i++){
   var item=ItemCatalog.All[i];if(item.SourceType?.MarketGroupId is int market){source[market].Items.Add(i);continue;}
   var node=root;string path="";foreach(var name in AuthoredPathFor(item).Split(" / ",StringSplitOptions.RemoveEmptyEntries)){path=path.Length==0?name:path+" / "+name;var child=node.Children.FirstOrDefault(c=>c.Name==name);if(child==null){child=new(){Name=name,Path=path};node.Children.Add(child);}node=child;}node.Items.Add(i);
  }Sort(root);return root;
 }
 static void Sort(MarketGroup n){n.Children.Sort((a,b)=>string.Compare(a.Name,b.Name,StringComparison.Ordinal));n.Items.Sort((a,b)=>string.Compare(ItemCatalog.All[a].Name,ItemCatalog.All[b].Name,StringComparison.Ordinal));foreach(var c in n.Children)Sort(c);}
}
public sealed partial class DemoGame
{
 readonly HashSet<string> marketExpanded=[];
 bool marketQuickbar,marketGroupsView,marketPrefsLoaded,marketHistoryGraph=true;
 string marketGroupPath="";int marketTreeScroll,marketGroupScroll,marketScope;
 Rectangle marketTreeRect,marketSellersRect,marketBuyersRect;
 string MarketPrefsPath=>Path.Combine(Path.GetDirectoryName(DemoWorld.SavePath)!,"market-browser.json");
 void LoadMarketPrefs(){if(marketPrefsLoaded)return;marketPrefsLoaded=true;string current="";foreach(var part in MarketGroups.PathFor(ItemCatalog.Get(MarketItem)).Split(" / ")){current=current.Length==0?part:current+" / "+part;marketExpanded.Add(current);}try{if(File.Exists(MarketPrefsPath))foreach(var id in JsonSerializer.Deserialize<string[]>(File.ReadAllText(MarketPrefsPath))??[])marketFavorites.Add(id);}catch(IOException){}catch(JsonException){}}
 void ToggleMarketFavorite(string id){if(!marketFavorites.Add(id))marketFavorites.Remove(id);try{Directory.CreateDirectory(Path.GetDirectoryName(MarketPrefsPath)!);File.WriteAllText(MarketPrefsPath+".tmp",JsonSerializer.Serialize(marketFavorites));File.Move(MarketPrefsPath+".tmp",MarketPrefsPath,true);}catch(IOException){marketNotice="Could not save Quickbar.";}}
 void SelectMarketItem(int index){catalogSelection=index;marketGroupsView=false;marketTab=0;selectedMarketOrder="";askScroll=bidScroll=0;}
 void DrawMarketBrowser(Rectangle r,int sidebar)
 {
  LoadMarketPrefs();ui.Box(new(r.X+1,r.Y+29,sidebar,r.Height-30),new Color(19,20,22));
  ui.Button(new(r.X+8,r.Y+37,94,25),"Browse",()=>{marketQuickbar=false;marketTreeScroll=0;},active:!marketQuickbar);
  ui.Button(new(r.X+105,r.Y+37,94,25),"Quickbar",()=>{marketQuickbar=true;marketTreeScroll=0;},active:marketQuickbar);
  searchRect=new(r.X+9,r.Y+70,sidebar-42,27);ui.Box(searchRect,new Color(7,8,10));ui.Border(searchRect,marketTextFocus=="search"?Cyan:new Color(44,46,49));ui.Text(marketQuery.Length==0?"Search":marketQuery,searchRect.X+7,searchRect.Y+7,Muted,12);
  ui.Button(new(r.X+sidebar-30,r.Y+70,22,27),"x",()=>{marketQuery="";marketTreeScroll=0;});
  ui.Button(new(r.X+9,r.Y+103,sidebar-18,24),"Range: "+new[]{"Region","System","Station"}[marketScope],()=>{marketScope=(marketScope+1)%3;marketLocal=marketScope!=0;askScroll=bidScroll=0;});
  marketTreeRect=new(r.X+8,r.Y+135,sidebar-16,r.Height-166);
  var rows=new List<(MarketGroup? Group,int Index,int Depth)>();
  bool Matches(int i)=>ItemCatalog.All[i].Name.Contains(marketQuery,StringComparison.OrdinalIgnoreCase)&&(!marketQuickbar||marketFavorites.Contains(ItemCatalog.All[i].Id));
  void Visit(MarketGroup group,int depth){if(!group.Descendants().Any(Matches))return;rows.Add((group,-1,depth));if(marketQuery.Length>0||marketQuickbar||marketExpanded.Contains(group.Path)){foreach(var child in group.Children)Visit(child,depth+1);foreach(int i in group.Items.Where(Matches))rows.Add((null,i,depth+1));}}
  foreach(var group in MarketGroups.Root.Children)Visit(group,0);
  int visible=Math.Max(1,marketTreeRect.Height/23);marketTreeScroll=Math.Clamp(marketTreeScroll,0,Math.Max(0,rows.Count-visible));
  for(int i=0;i<visible&&i+marketTreeScroll<rows.Count;i++){
   var row=rows[i+marketTreeScroll];int y=marketTreeRect.Y+i*23,x=marketTreeRect.X+row.Depth*13;var rect=new Rectangle(x,y,marketTreeRect.Right-x,22);
   if(row.Group is {} g){bool open=marketQuery.Length>0||marketQuickbar||marketExpanded.Contains(g.Path);ui.Box(rect,marketGroupPath==g.Path&&marketGroupsView?new Color(39,51,56):new Color(25,26,28));ui.Text((open?"v ":"> ")+g.Name,x+4,y+5,White,11);ui.Hits.Add(new(rect,()=>{if(!marketExpanded.Add(g.Path))marketExpanded.Remove(g.Path);marketGroupPath=g.Path;marketGroupsView=true;marketGroupScroll=0;},g.Name));}
   else{int ix=row.Index;var item=ItemCatalog.All[ix];if(ix==catalogSelection&&!marketGroupsView)ui.Box(rect,new Color(35,51,58));string name=item.Name;int max=Math.Max(8,(rect.Width-28)/6);ui.Text(name.Length>max?name[..(max-3)]+"...":name,x+5,y+5,White,11);ui.Hits.Add(new(new Rectangle(x,y,rect.Width-23,22),()=>SelectMarketItem(ix),item.Name){Key="market-item:"+item.Id});ui.Button(new(rect.Right-22,y,21,21),"i",()=>MarketShowInfo(ix),"Show information: "+item.Name);}
  }
  if(rows.Count==0)ui.Text(marketQuickbar?"Quickbar is empty":"No matching items",marketTreeRect.X+4,marketTreeRect.Y+5,Muted,12);
  ui.Text($"{rows.Count} entries  |  Scroll to browse",r.X+12,r.Bottom-23,Muted,10);
 }
 void MarketShowInfo(int ix){if(ix>=0&&ix<ItemCatalog.All.Length)ExecuteCommandJ(new(ItemCatalog.All[ix].Id,true),"info");}
 void DrawMarketGroupCards(Rectangle r)
 {
  MarketGroup? Find(MarketGroup n)=>n.Path==marketGroupPath?n:n.Children.Select(Find).FirstOrDefault(g=>g!=null);
  var group=Find(MarketGroups.Root);var items=(group?.Descendants()??[]).Where(i=>ItemCatalog.All[i].Name.Contains(marketQuery,StringComparison.OrdinalIgnoreCase)).ToArray();
  int visible=Math.Max(1,r.Height/159);marketGroupScroll=Math.Clamp(marketGroupScroll,0,Math.Max(0,items.Length-visible));
  for(int row=0;row<visible&&row+marketGroupScroll<items.Length;row++){int ix=items[row+marketGroupScroll];var item=ItemCatalog.All[ix];int y=r.Y+row*159;DrawMarketIcon(item,new(r.X,y,48,48));ui.Text(item.Name,r.X+60,y+3,White,18);ui.Text(MarketGroups.PathFor(item),r.X+60,y+28,Cyan,11);string description=item.Slot=="ship"?string.Join(" | ",ShipBatch21.ById[item.Id].Traits.Select(t=>(t.Skill==0?"Role: ":"Per level: ")+(t.Bonus==0?"":t.Bonus.ToString("0.##")+(t.Unit==105?"% ":" "))+t.Text)):item.Description;if(description.Length==0)description=item.Description;ui.Wrap(description.Length>220?description[..217]+"...":description,r.X+60,y+49,r.Width-72,Muted,11);var best=world.MarketOrders.Where(o=>o.Item==item.Id&&!o.Buy&&o.Station==world.StationKey44&&o.Remaining>0&&o.Expires>world.Day).Select(o=>(decimal?)o.Price).Min();ui.Text("Best price at station: "+(best.HasValue?$"{best:N2} CR":"None available"),r.X+60,y+105,White,12);ui.Button(new(r.X+60,y+125,110,24),"View details",()=>SelectMarketItem(ix));ui.Button(new(r.X+176,y+125,105,24),"Show info",()=>MarketShowInfo(ix));ui.Button(new(r.X+287,y+125,137,24),"Place buy order",()=>{SelectMarketItem(ix);NewMarketOrder(true);});ui.Line(new(r.X,y+155),new(r.Right,y+155),new Color(40,42,44));}
 }
 void DrawMarketIcon(ItemDefinition item,Rectangle r){if(item.Slot=="ship"&&HullThumbnail(item.Id) is {} texture)ui.Image(texture,r);else Glyph(r.X+5,r.Y+5,Array.FindIndex(ItemCatalog.All,i=>i.Id==item.Id),Cyan);}
 void AssembleMarketHull(string id){marketNotice=world.AssembleMarketHull(id)?"Hull assembled in Local berths. Open Fleet to board and fit it.":"A packaged hull in this station's item hangar is required.";}
}
public sealed partial class DemoWorld
{
 public bool AssembleMarketHull(string id){if(!RequireService44(17))return false;if(!Docked||!FleetCatalog.ById.TryGetValue(id,out var hull)||!hull.Playable||ItemCount(id,true)<1)return false;var ship=new Ship();ship.ApplyFleetHull(hull);ChangeStack(id,true,-(decimal)(1));FleetBerths.Add(new(){System=StationKey44,Ship=ship,Cargo=[]});return true;}
}
