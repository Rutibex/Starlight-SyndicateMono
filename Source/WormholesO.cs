using Microsoft.Xna.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace StarlightSyndicateDemo;
public sealed class SystemProfileO{public int Id{get;set;}public int Class{get;set;}public string InheritedFrom{get;set;}="";public int Region{get;set;}public int Constellation{get;set;}public double Security{get;set;}public int EffectType{get;set;}public int Beacon{get;set;}public Dictionary<string,double> Modifiers{get;set;}=[];}
public sealed class WormholeTypeO{public int Id{get;set;}public string Code{get;set;}="";public int DestinationClass{get;set;}public double Minutes{get;set;}public double TotalMass{get;set;}public double Regeneration{get;set;}public double JumpMass{get;set;}public int[] Regions{get;set;}=[];public int[] Constellations{get;set;}=[];public int[] Systems{get;set;}=[];}
public sealed class FilamentO{public int Id{get;set;}public int Tier{get;set;}public string Weather{get;set;}="";public string SourceName{get;set;}="";public float Volume{get;set;}public string Item=>"sde_"+Id;public string Name=>Retheme41.TypeName(Id)+" / T"+Tier+" / "+Weather;}
public sealed class SourceDataO{public SystemProfileO[] Systems{get;set;}=[];public WormholeTypeO[] Types{get;set;}=[];public FilamentO[] Filaments{get;set;}=[];}
public static class SourceCatalogO
{
 public static readonly SourceDataO Data=JsonSerializer.Deserialize<SourceDataO>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Assets","ExplorationO","source.json")))!;
 static readonly Dictionary<int,SystemProfileO> systems=Data.Systems.ToDictionary(s=>s.Id);
 public static SystemProfileO? System(string id)=>id.StartsWith("system:")&&int.TryParse(id[7..],out int n)?systems.GetValueOrDefault(n):null;
 public static bool Matches(WormholeTypeO t,SystemProfileO s){bool ordinary=s.Id>=30000000&&s.Id<30100000&&!new[]{10000004,10000017,10000019,10000070}.Contains(s.Region);bool match=t.DestinationClass switch{7=>ordinary&&s.Security>=.45,8=>ordinary&&s.Security>0&&s.Security<.45,9=>ordinary&&s.Security<=0,-1=>t.Code=="C729"&&s.Class==25,_=>s.Class==t.DestinationClass};return match&&(t.Regions.Length==0||t.Regions.Contains(s.Region))&&(t.Constellations.Length==0||t.Constellations.Contains(s.Constellation))&&(t.Systems.Length==0||t.Systems.Contains(s.Id));}
 public static readonly ItemDefinition[] Items=Data.Filaments.Select(f=>new ItemDefinition(f.Item,f.Name,"Exploration / Filaments","Three-room Abyss entry. Physical matching filaments and admitted hulls required.",f.Volume,200*(f.Tier+1),"")).Concat(new ItemDefinition[]{new("o_blue","Resident neural archive","Exploration / Recoverables","Physical resident salvage; authored local economy value.",.1f,100,""),new("o_fabricator_data","Fabricator research data","Exploration / Recoverables","Physical wave research; authored reward quantities.",.1f,50,""),new("o_bio","Abyssal biological sample","Exploration / Recoverables","Physical cache contents; authored reward quantities.",.1f,30,"")}).ToArray();
}
public sealed partial class ConnectionF
{
 public int SourceTypeO{get;set;}public string SlotO{get;set;}="";public int GenerationO{get;set;}public string SuccessorO{get;set;}="";public bool BackVisibleO{get;set;}public double InitialMassO{get;set;}public DateTime LastMassO{get;set;}public DateTime NextNpcO{get;set;}public Dictionary<string,string> ObservationsO{get;set;}=[];
}
public sealed partial class DemoWorld
{
 public ConnectionF? AddConnectionO(string a,WormholeTypeO type,string slot="",int generation=0)
 {var possible=SourceCatalogO.Data.Systems.Where(s=>"system:"+s.Id!=a&&SourceCatalogO.Matches(type,s)).OrderBy(s=>s.Id).ToArray();if(possible.Length==0||type.TotalMass<=0||type.JumpMass<=0||type.Minutes<=0)return null;var dest=possible[(StableC(a+slot)+(uint)generation)%(uint)possible.Length];var h=new ConnectionF{A=a,B="system:"+dest.Id,Type=type.Code,SourceTypeO=type.Id,SlotO=slot,GenerationO=generation,InitialMassO=type.TotalMass,RemainingMass=type.TotalMass,MaxShipMass=type.JumpMass,Expires=Now.AddMinutes(type.Minutes),LastMassO=Now,NextNpcO=Now.AddMinutes(30)};Exploration.Connections.Add(h);RegisterConnectionF(h);return h;}
 public void EnsureWormholesO()
 {var p=SourceCatalogO.System(SystemId);if(p is not{Class:>=1 and <=6})return;string slot=SystemId+"/staticO";if(!Exploration.Connections.Any(h=>h.SlotO==slot)){var type=SourceCatalogO.Data.Types.First(t=>t.DestinationClass==7&&t.Minutes>0&&t.TotalMass>0);AddConnectionO(SystemId,type,slot);}if(!Exploration.Connections.Any(h=>h.A==SystemId&&h.SourceTypeO>0&&h.SlotO==""&&!h.Collapsed&&h.Expires>Now)){var types=SourceCatalogO.Data.Types.Where(t=>t.DestinationClass is >=1 and <=6&&t.Minutes>0&&t.TotalMass>0).ToArray();AddConnectionO(SystemId,types[StableC(SystemId)%(uint)types.Length]);}EnsureResidentsO();ApplyEnvironmentO();}
 public bool ConnectionAvailableO(ConnectionF h)=>!h.Collapsed&&h.RemainingMass>0&&h.Expires>Now&&(h.A==SystemId||h.B==SystemId&&(h.SourceTypeO==0||h.BackVisibleO));
 public bool DebitConnectionO(ConnectionF h,string ship,double mass)
 {if(h.Collapsed||h.Expires<=Now||!double.IsFinite(mass)||mass<=0||mass>h.MaxShipMass||h.RemainingMass<=0)return false;var history=h.Transits.GetValueOrDefault(ship)??[];history.RemoveAll(t=>t<=Now.AddMinutes(-5));if(history.Count>=2)return false;history.Add(Now);h.Transits[ship]=history;h.RemainingMass=Math.Max(0,h.RemainingMass-mass);h.BackVisibleO=true;RegisterConnectionF(h);if(h.RemainingMass<=0)h.Collapsed=true;return true;}
 public bool TransitNpcO(ConnectionF h,PilotC p)
 {if(p.Contracted||p.Fit.Hull<=0||p.System!=h.A&&p.System!=h.B||!p.Knowledge.ContainsKey("wormhole:"+h.Id)||!DebitConnectionO(h,p.Fit.InstanceId,p.Fit.Stat("mass")))return false;p.System=p.System==h.A?h.B:h.A;p.Grid="wf:"+h.Id;p.Position=new(2400,0,0);RememberC(p,"Observed wormhole transit / "+h.Id);return true;}
 public void TickWormholesO()
 {foreach(var h in Exploration.Connections.Where(h=>h.SourceTypeO>0).ToArray()){
  var t=SourceCatalogO.Data.Types.Single(t=>t.Id==h.SourceTypeO);if(!h.Collapsed&&h.Expires>Now&&t.Regeneration>0){int cycles=(int)Math.Max(0,Math.Floor((Now-h.LastMassO).TotalHours));h.RemainingMass=Math.Min(h.InitialMassO,h.RemainingMass+cycles*t.Regeneration);h.LastMassO=h.LastMassO.AddHours(cycles);}
  if(h.Expires<=Now)h.Collapsed=true;
  if(h.Collapsed){foreach(var s in UtilitySites.Where(s=>s.Id=="wf:"+h.Id)){s.Completed=true;s.Signal=0;}Targets.RemoveAll(t=>t.Id=="wf:"+h.Id);if(h.SlotO!=""&&h.SuccessorO==""){var n=AddConnectionO(h.A,t,h.SlotO,h.GenerationO+1);if(n!=null)h.SuccessorO=n.Id;}continue;}
  if(Now>=h.NextNpcO){h.NextNpcO=Now.AddMinutes(30);var scout=Population.Pilots.FirstOrDefault(p=>!p.Contracted&&p.Role=="Scout"&&(p.System==h.A||h.BackVisibleO&&p.System==h.B));if(scout!=null){if(scout.Knowledge.ContainsKey("wormhole:"+h.Id))TransitNpcO(h,scout);else scout.Knowledge["wormhole:"+h.Id]=Now;}}
 }ApplyEnvironmentO();}
 public string ConnectionCodeO(ConnectionF h,string system)=>h.SourceTypeO>0&&system==h.B?"K162":h.Type;
 public string ConnectionStatusO(ConnectionF h)=>h.SourceTypeO>0&&Grid!="wf:"+h.Id?"Connection report":h.Collapsed?"Collapsed":"Open";
 public string DescribeConnectionO(ConnectionF h)
 {if(Grid!="wf:"+h.Id)return h.ObservationsO.GetValueOrDefault(SystemId,"Not inspected on grid");string mass=h.Collapsed?"Collapsed":h.RemainingMass<h.InitialMassO*.1?"Critically disrupted":h.RemainingMass<h.InitialMassO*.5?"Reduced mass":"Stable mass";string report=ConnectionCodeO(h,SystemId)+" / "+mass+" / "+((h.Expires-Now).TotalHours<4?"End of life":"Not near end of life")+" / observed "+Now.ToString("HH:mm:ss");h.ObservationsO[SystemId]=report;return report;}
 public void ApplyEnvironmentO()
 {var modifiers=InAbyssF?new Dictionary<string,double>():SourceCatalogO.System(SystemId)?.Modifiers??[];Player.WormholeModifiersO=new(modifiers);if(ActiveEncounterF is {State:"Active",ProfileO.Family:"Abyss"} abyss)ApplyCloudO(abyss);else Player.CloudO="";foreach(var t in Targets)if(t.Profile!=null)t.Profile.WormholeModifiersO=new(modifiers);if(ActiveEncounterF?.ProfileO?.Family!="Fabricator"){Player.NanoswarmRangeO=Player.NanoswarmFlightO=Player.NanoswarmFighterO=1;}else ApplyNanoswarmsO(ActiveEncounterF!);}
}
public sealed partial class Ship
{
 [JsonIgnore]public Dictionary<string,double> WormholeModifiersO{get;set;}=[];
 [JsonIgnore]public string CloudO{get;set;}="";
 [JsonIgnore]public float WeatherPenaltyO{get;set;}=.5f;
 [JsonIgnore]public float NanoswarmRangeO{get;set;}=1;
 [JsonIgnore]public float NanoswarmFighterO{get;set;}=1;
 [JsonIgnore]public float NanoswarmFlightO{get;set;}=1;
 public double StatO(string key,double value)
 {string? modifier=key switch{"shieldCapacity"=>"shieldCapacityMultiplier","signatureRadius"=>"signatureRadiusMultiplier","rechargeRate"=>"rechargeRateMultiplier","agility"=>"agilityMultiplier","maxTargetRange"=>"maxTargetRangeMultiplier","maxVelocity"=>"maxVelocityMultiplier","armorHP"=>"armorHPMultiplier","capacitorCapacity"=>"capacitorCapacityMultiplierSystem",_=>null};if(modifier!=null&&WormholeModifiersO.TryGetValue(modifier,out double mult))value*=mult;foreach(string layer in new[]{"armor","shield"})if(key.StartsWith(layer)&&key.EndsWith("DamageResonance")&&WormholeModifiersO.TryGetValue(layer+"DamageResonanceMultiplier",out double res))value=Math.Min(1,value*res);foreach(string layer in new[]{"armor","shield"})foreach(string damage in new[]{"Em","Kinetic","Thermal","Explosive"})if(key==layer+damage+"DamageResonance"&&WormholeModifiersO.TryGetValue(layer+damage+"DamageResistanceBonus",out double percent))value=Math.Min(1,value*(1+percent/100));if(CloudO=="Velocity"&&key=="maxVelocity"||CloudO=="Signature"&&key=="signatureRadius")value*=3;return value;}
}
