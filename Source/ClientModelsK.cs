using System.Text.Json;
using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed class OverviewTabK
{
 public string Name{get;set;}="All";public List<string> Types{get;set;}=[];public List<string> Exceptions{get;set;}=["friendly:hide","hostile:show"];public string Brackets{get;set;}="Same as tab";
 public OverviewTabK Copy()=>JsonSerializer.Deserialize<OverviewTabK>(JsonSerializer.Serialize(this))!;
 public bool Allows(string type,bool friendly,bool hostile,bool locked,bool selected){foreach(var rule in Exceptions){var parts=rule.Split(':');if(parts.Length!=2)continue;bool match=parts[0] switch{"friendly"=>friendly,"hostile"=>hostile,"locked"=>locked,"selected"=>selected,_=>false};if(match)return parts[1]=="show";}return Types.Count==0||Types.Contains(type);}
 public static List<OverviewTabK> Defaults()=>[new(){Name="All",Exceptions=[]},new(){Name="Hostiles",Types=["Raider"],Exceptions=["friendly:hide","hostile:show"]},new(){Name="Resources",Types=["Asteroid","AsteroidBelt","IceField","GasCloud"],Exceptions=[]},new(){Name="Containers",Types=["Container","Wreck"],Exceptions=[]},new(){Name="Travel",Types=["Station","Stargate","Wormhole","Planet","Sun"],Exceptions=[]}];
}
public sealed record InventoryLineK(string Item,decimal Quantity,int Segment=0){public string Key=>Item+"#"+Segment;}
public static class StackPresentationK
{
 // Segments are an allocation of the authoritative aggregate, never extra stock.
 public static List<decimal> Reconcile(decimal total,List<decimal>? portions){var result=(portions??[]).Where(p=>p>0).ToList();decimal remaining=total;for(int i=0;i<result.Count;i++){result[i]=Math.Min(result[i],remaining);remaining-=result[i];}result.RemoveAll(p=>p<=0);if(remaining>0)result.Add(remaining);return result;}
 public static bool Split(List<decimal> portions,int index,decimal quantity){if(index<0||index>=portions.Count||quantity<=0||quantity>=portions[index])return false;portions[index]-=quantity;portions.Insert(index+1,quantity);return true;}
}
public sealed class RackLayoutK
{
 public Dictionary<string,int> Positions{get;set;}=[];
 public void Reconcile(IEnumerable<FittingModule> modules){var list=modules.Where(m=>m.Bank!="rig").ToArray();var keys=list.Select(m=>m.Bank+"/"+m.Key).ToHashSet();foreach(var key in Positions.Keys.Where(k=>!keys.Contains(k)).ToArray())Positions.Remove(key);foreach(var m in list){string key=m.Bank+"/"+m.Key;if(Positions.ContainsKey(key))continue;var used=Positions.Where(p=>p.Key.StartsWith(m.Bank+"/")).Select(p=>p.Value).ToHashSet();int position=0;while(used.Contains(position))position++;Positions[key]=position;}}
 public int Position(FittingModule m)=>Positions.GetValueOrDefault(m.Bank+"/"+m.Key,-1);
}
public sealed partial class DemoWorld
{
 public string TransferBatchK(IEnumerable<(string Item,decimal Quantity)> rows,bool fromHangar){var lines=rows.GroupBy(r=>r.Item).Select(g=>(Item:g.Key,Quantity:g.Sum(r=>r.Quantity))).ToArray();if(!Docked||lines.Length==0)return "Dock and select local inventory first.";if(lines.Any(l=>l.Quantity<=0||l.Quantity>100000000||InventoryDisplayCount(l.Item,fromHangar)<l.Quantity||InventoryMission(l.Item)==null&&l.Item!="ore"&&l.Quantity!=decimal.Truncate(l.Quantity)))return "Invalid or changed selection; nothing moved.";
 var stage=JsonSerializer.Deserialize<DemoWorld>(JsonSerializer.Serialize(this,FittingDesignStore.Json),FittingDesignStore.Json)!;stage.Player=FittingSimulation.Copy(Player);
 bool Move(DemoWorld w,(string Item,decimal Quantity) line)=>w.InventoryMission(line.Item)!=null?w.TransferInventoryMission(line.Item,fromHangar,(float)line.Quantity):w.TransferItem(line.Item,fromHangar,(float)line.Quantity);
 foreach(var line in lines)if(!Move(stage,line))return "The entire selection must fit; nothing moved.";
 foreach(var line in lines)if(!Move(this,line))throw new InvalidOperationException("Inventory changed after successful atomic preflight.");return "";
 }
}

public sealed partial class DemoGame
{
 public static void ValidateLayoutK(DesktopLayoutJ state){if(state.BracketOwnerK==null||state.InformationSnapshotsK==null||state.InformationSnapshotsK.Count>100||state.InformationSnapshotsK.Any(p=>!p.Key.StartsWith("tactical/fitted/")||p.Value.ValueKind!=JsonValueKind.Object)||state.Windows==null||state.Windows.Any(w=>w.TabsK==null||w.TabsK.Count>20||w.TabsK.Any(t=>t.Name==null||t.Name.Length is <1 or >40||t.Types==null||t.Exceptions==null||t.Exceptions.Count>20||t.Brackets==null)||w.SplitStacksK==null||w.SplitStacksK.Count>10000||w.SplitStacksK.Any(p=>p.Value==null||p.Value.Count>1000||p.Value.Any(v=>v<=0))||w.CompareKeysK==null||w.CompareKeysK.Count>10||w.LocationK==null||w.HoldK==null||w.SelectionK==null))throw new InvalidDataException("Invalid Stage K desktop view state.");}
}
