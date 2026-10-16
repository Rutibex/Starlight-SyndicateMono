using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed record EwarHeatProfile(float Hp,float Damage,float Range,float Strength,float Cycle);
public static class StrategicHeat
{
 public static readonly Dictionary<string,EwarHeatProfile> Ewar=JsonSerializer.Deserialize<Dictionary<string,EwarHeatProfile>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","EwarHeat28.json")))!;
}
public sealed class NaniteJob{public string Kind{get;set;}="";public int Index{get;set;}public string ModuleId{get;set;}="";public string InstanceId{get;set;}="";public float DamageLeft{get;set;}}
public sealed partial class Ship
{
 public List<NaniteJob> NaniteJobs{get;set;}=[];
 public object? HeatState(string kind,int index)=>kind switch{"weapon"=>Weapons.ElementAtOrDefault(index),"utility"=>Utilities.ElementAtOrDefault(index),"enhancement"=>Enhancements.ElementAtOrDefault(index),"defense"=>Defense.ElementAtOrDefault(index),"support"=>Support.ElementAtOrDefault(index),"mobility"=>Mobility.ElementAtOrDefault(index),"ewar"=>Ewar.ElementAtOrDefault(index),_=>null};
 public EwarDefinition EwarOverload(EwarDefinition d)
 {
  var state=Ewar.FirstOrDefault(s=>s.ModuleId==d.Id);if(state?.Overheat!=true||state.Burnt||!StrategicHeat.Ewar.TryGetValue(d.Id,out var heat))return d;
  float f=ScopedOverheat("ewar",d.Kind.ToString());return d with{Optimal=d.Optimal*(1+heat.Range*f),Strength=d.Strength*(1+heat.Strength*f),Cycle=d.Cycle*Math.Max(.05f,1+heat.Cycle*f)};
 }
}
public sealed partial class DemoWorld
{
 public bool RepairWithNanites(string kind,int index)
 {
  object? state=Player.HeatState(kind,index);if(state==null||Player.NaniteJobs.Any(j=>j.Kind==kind&&j.Index==index))return false;
  var type=state.GetType();float damage=(float)(type.GetProperty("Damage")?.GetValue(state)??0f);if(damage<=0||damage>=1||(bool)(type.GetProperty("Active")?.GetValue(state)??false))return false;
  int qty=Math.Max(1,(int)Math.Ceiling(damage*20));if(ItemCount("dc_paste",false)<qty){Say("Load "+qty+" nanite paste into cargo before repair.");return false;}
  ChangeStack("dc_paste",false,-(decimal)(qty));Player.NaniteJobs.Add(new(){Kind=kind,Index=index,ModuleId=(string)type.GetProperty("ModuleId")!.GetValue(state)!,InstanceId=(string)type.GetProperty("InstanceId")!.GetValue(state)!,DamageLeft=damage});type.GetProperty("Overheat")?.SetValue(state,false);Say("Nanite repair started. Module must remain inactive; burnt modules require station repair.");return true;
 }
 public void UpdateNaniteRepairs(float dt)
 {
  foreach(var j in Player.NaniteJobs.ToArray()){
   var state=Player.HeatState(j.Kind,j.Index);if(state==null||(string?)state.GetType().GetProperty("ModuleId")!.GetValue(state)!=j.ModuleId||j.InstanceId.Length>0&&(string?)state.GetType().GetProperty("InstanceId")!.GetValue(state)!=j.InstanceId){Player.NaniteJobs.Remove(j);continue;}
   var type=state.GetType();if((bool)(type.GetProperty("Active")?.GetValue(state)??false))continue;
   float damage=(float)type.GetProperty("Damage")!.GetValue(state)!;float amount=Math.Min(j.DamageLeft,dt*.01f*Player.HullFactor("hull","pasteRepairSpeed"));type.GetProperty("Damage")!.SetValue(state,Math.Max(0,damage-amount));j.DamageLeft-=amount;if(j.DamageLeft<=.00001f)Player.NaniteJobs.Remove(j);
  }
 }
}
public sealed class EwarStored{public EwarState State{get;set;}=new();public bool Hangar{get;set;}public string System{get;set;}="";}
public sealed partial class DemoWorld
{
 public List<EwarStored> EwarStorage{get;set;}=[];
 void StoreEwarCondition(EwarState s){if(s.ModuleId.Length>0)EwarStorage.Add(new(){State=s,Hangar=true,System=StationKey44});}
 EwarState TakeEwarCondition(string id,bool hangar){var stored=EwarStorage.FirstOrDefault(s=>s.State.ModuleId==id&&s.Hangar==hangar&&(!hangar||s.System==StationKey44));if(stored==null)return EwarState.Create(id);EwarStorage.Remove(stored);stored.State.Overheat=false;return stored.State;}
 void TransferEwarCondition(string id,bool fromHangar,int quantity){foreach(var s in EwarStorage.Where(s=>s.State.ModuleId==id&&s.Hangar==fromHangar&&(!fromHangar||s.System==StationKey44)).Take(quantity)){s.Hangar=!fromHangar;s.System=StationKey44;}}
 public bool RepairEwar(int index){if(!RequireService44(13))return false;if(!Docked||index<0||index>=Player.Ewar.Length)return false;var s=Player.Ewar[index];if(s.Definition==null)return false;decimal cost=ItemCatalog.Get(s.ModuleId).Price*.12m*(decimal)Math.Max(s.Damage,s.Burnt?1:0);if(Credits<(float)cost)return false;WalletChange(-cost,"EWAR module repair");s.Damage=0;s.Burnt=false;s.Overheat=false;return true;}
}
