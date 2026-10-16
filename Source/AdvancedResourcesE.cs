namespace StarlightSyndicateDemo;
public sealed class PhaseFieldE {public string Station{get;set;}="";public DateTime Start{get;set;}public decimal Remaining{get;set;}=1000;public bool Discovered{get;set;}public bool Anchor{get;set;}public int Energy{get;set;}public decimal Harvested{get;set;}}
public sealed partial class DemoWorld
{
 public string BuyProductionSupplyE(string item)
 {
  if(!Docked||item is not("e_formula" or "e_relic" or "e_decryptor" or "e_mutator"))return "Dock to acquire a specialist supply.";string key=StationKey44+"/"+item;
  int stock=Production.Supplies.GetValueOrDefault(key,5);decimal price=ItemCatalog.Get(item).Price;if(stock<=0||Balance<price)return "Specialist opening stock exhausted or insufficient credits.";WalletChange(-price,"Finite specialist supply / "+item);Production.Supplies[key]=stock-1;Hangar[item]=Hangar.GetValueOrDefault(item)+1;return "";
 }
 public string MutateDefenseE(int slot)
 {
  if(!Docked||slot<0||slot>=Player.Defense.Length||Hangar.GetValueOrDefault("e_mutator")<1)return "Dock with a mutator and select a fitted local repairer.";
  var s=Player.Defense[slot];if(s.Definition?.Kind is not("ShieldBoost" or "ArmorRepair" or "HullRepair")||s.Active||s.Burnt||s.Damage>0||s.MutatedE)return "Only pristine, inactive and unmutated local repairers are eligible.";
  Hangar["e_mutator"]--;s.MutatedE=true;s.MutationAmountE=(float)(.8+RollProductionE()*.4);s.MutationCapE=(float)(.8+RollProductionE()*.4);s.MutationCpuE=(float)(.8+RollProductionE()*.4);s.MutationPowerE=(float)(.8+RollProductionE()*.4);s.Online=false;
  LogProductionE("Mutated repairer "+s.InstanceId+" / amount "+s.MutationAmountE.ToString("0.000")+" / capacitor "+s.MutationCapE.ToString("0.000")+" / CPU "+s.MutationCpuE.ToString("0.000")+" / power "+s.MutationPowerE.ToString("0.000")+" / left offline for fit review");return "";
 }
 public PhaseFieldE? DiscoverPhaseE()
 {
  if(!Docked||ResolveSystem43(SystemId).Security>.4)return null;var field=Production.Phases.FirstOrDefault(f=>f.Station==StationKey44);if(field!=null){field.Discovered=true;return field;}field=new(){Station=StationKey44,Start=Now,Discovered=true};Production.Phases.Add(field);return field;
 }
 public string PhaseStatusE(PhaseFieldE f){double hours=(Now-f.Start).TotalHours;return hours>=4?"Collapsed":hours<1?"Unbreached":f.Anchor&&f.Energy>=100?"Phased in / 100% efficiency":"Phased out / 10% efficiency";}
 public string AnchorPhaseE()
 {var f=Production.Phases.FirstOrDefault(f=>f.Station==StationKey44);if(f==null||!Docked||PhaseStatusE(f)=="Collapsed"||Hangar.GetValueOrDefault("e_component")<5)return "Discover a live local field and supply five advanced components.";if(f.Anchor)return "Anchor already deployed.";Hangar["e_component"]-=5;f.Anchor=true;return "";}
 public string EnergizePhaseE()
 {
  var f=Production.Phases.FirstOrDefault(f=>f.Station==StationKey44);if(f==null||!f.Anchor||!Docked||PhaseStatusE(f)=="Collapsed")return "An active local anchor is required.";
  int energy=CombatHulls.Get(Player.CombatHull).Role switch{"CapitalIndustrial"=>100,"ExpeditionCommand"=>50,"MiningTender"=>35,_=>0};if(energy==0)return "Board an industrial command hull: 100 / 50 / 35 energy.";f.Energy=energy;return "";
 }
 public string CollectPhaseE()
 {var f=Production.Phases.FirstOrDefault(f=>f.Station==StationKey44);if(f==null||!Docked||PhaseStatusE(f) is "Unbreached" or "Collapsed"||f.Remaining<=0)return "A breached, live local field is required.";decimal amount=Math.Min(f.Remaining,f.Anchor&&f.Energy>=100?100:10);f.Remaining-=amount;f.Harvested+=amount;Hangar["e_prism"]=Hangar.GetValueOrDefault("e_prism")+amount;LogProductionE("Phased mining service / "+amount+" units");return "";}
 public string CompressPhaseE()
 {if(!Docked||Hangar.GetValueOrDefault("e_prism")<=0||!Production.Facilities.Any(f=>f.Station==StationKey44&&f.Kind=="Refinery"&&f.Online&&!f.Destroyed))return "Local online refinery and prism ore required.";decimal amount=Hangar["e_prism"];Hangar["e_prism"]=0;Hangar["e_compressed_prism"]=Hangar.GetValueOrDefault("e_compressed_prism")+amount;return "";}
 public string RefinePhaseE()
 {if(!Docked||Hangar.GetValueOrDefault("e_compressed_prism")<100||!Production.Facilities.Any(f=>f.Station==StationKey44&&f.Kind=="Refinery"&&f.Online&&!f.Destroyed))return "100 compressed units and an online local refinery required.";Hangar["e_compressed_prism"]-=100;string mineral="ind_"+IndustryCatalog37.MineralTypes[Math.Min(7,(int)(RollProductionE()*8))];int amount=100+(int)(RollProductionE()*101);Hangar[mineral]=Hangar.GetValueOrDefault(mineral)+amount;LogProductionE("Erratic refining / "+mineral+" x"+amount);return "";}
}
