using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
// Stage D objects use stable identities, decimal quantities and the shared simulation clock.
public sealed class CommanderStateD
{
 public string MarketProfile{get;set;}="Starlight";public string PilotName{get;set;}="Independent capsuleer";public string Biography{get;set;}="";public bool Capsule{get;set;}
 public Dictionary<int,ImplantD> Implants{get;set;}=[];public Dictionary<string,Dictionary<int,ImplantD>> CloneImplants{get;set;}=[];
 public List<AssetContainerD> Containers{get;set;}=[];public List<InsuranceD> Insurance{get;set;}=[];public List<LossD> Losses{get;set;}=[];public List<WreckD> Wrecks{get;set;}=[];
 public List<ContractD> Contracts{get;set;}=[];public List<ResearchD> Research{get;set;}=[];public List<string> History{get;set;}=[];
 public Dictionary<string,int> TraderStock{get;set;}=[];public HashSet<string> TraderStations{get;set;}=[];public HashSet<string> LostFacilities{get;set;}=[];
 public List<MasteryD> Masteries{get;set;}=[];public decimal ServiceSinks{get;set;}public decimal InsurancePaid{get;set;}
}
public sealed class ImplantD {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Item{get;set;}="";public int Slot{get;set;}public int Bonus{get;set;}public string Set{get;set;}="";}
public sealed class AssetContainerD {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Name{get;set;}="Container";public string Station{get;set;}="";public string Parent{get;set;}="";public string Owner{get;set;}="player";public string Division{get;set;}="Personal";public bool Access{get;set;}=true;public bool Locked{get;set;}public decimal Capacity{get;set;}=10000;public Dictionary<string,decimal> Items{get;set;}=[];public List<UniqueAssetD> Instances{get;set;}=[];}
public sealed class UniqueAssetD {public string Id{get;set;}="";public string Item{get;set;}="";public string Bank{get;set;}="";public string Json{get;set;}="";}
public sealed class InsuranceD {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Ship{get;set;}="";public string Tier{get;set;}="Basic";public decimal Premium{get;set;}public decimal Payout{get;set;}public DateTime Expires{get;set;}public bool Claimed{get;set;}public string Loss{get;set;}="";}
public sealed class LossD {public string Id{get;set;}=Guid.NewGuid().ToString("N");public DateTime At{get;set;}public string System{get;set;}="";public string Ship{get;set;}="";public string Hull{get;set;}="";public bool Pod{get;set;}public string Wreck{get;set;}="";public decimal Insurance{get;set;}public Dictionary<string,decimal> Destroyed{get;set;}=[];public List<string> DestroyedInstances{get;set;}=[];public Dictionary<string,decimal> Dropped{get;set;}=[];public string Attacker{get;set;}="Unknown";}
public sealed partial class WreckD {public string Id{get;set;}="";public string System{get;set;}="";public string Grid{get;set;}="";public Vector3 Position{get;set;}public DateTime Expires{get;set;}public Dictionary<string,decimal> Items{get;set;}=[];public List<UniqueAssetD> Instances{get;set;}=[];public bool Despawned{get;set;}}
public sealed class MasteryD {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Name{get;set;}="";public string Plan{get;set;}="";public Dictionary<string,SkillRequirement[]> Milestones{get;set;}=[];}
public sealed class ResearchD {public string Id{get;set;}=Guid.NewGuid().ToString("N");public string Corporation{get;set;}="";public string Station{get;set;}="";public DateTime Finish{get;set;}public int Quantity{get;set;}public bool Claimed{get;set;}}
public enum ContractKindD {Exchange,Auction,Courier}
public sealed partial class ContractD
{
 public string Id{get;set;}=Guid.NewGuid().ToString("N");public ContractKindD Kind{get;set;}public string DeliveryOwner{get;set;}="";public string Issuer{get;set;}="player";public string Audience{get;set;}="Public";public string Recipient{get;set;}="";public string Title{get;set;}="";public string Origin{get;set;}="";public string Destination{get;set;}="";public string State{get;set;}="Outstanding";
 public Dictionary<string,decimal> Items{get;set;}=[];public Dictionary<string,decimal> Requested{get;set;}=[];public List<UniqueAssetD> Instances{get;set;}=[];
 public decimal Price{get;set;}public decimal Buyout{get;set;}public decimal Reward{get;set;}public decimal Collateral{get;set;}public decimal MoneyEscrow{get;set;}public decimal CollateralEscrow{get;set;}public decimal BidEscrow{get;set;}public string Bidder{get;set;}="";public string Carrier{get;set;}="";public DateTime Expires{get;set;}public DateTime Deadline{get;set;}public double DeliveryHours{get;set;}=24;public DateTime NextLeg{get;set;}public List<string> Route{get;set;}=[];public int Leg{get;set;}public bool PackageBroken{get;set;}public bool MoneyClaimed{get;set;}public bool ItemsClaimed{get;set;}
}
public sealed record AssetRowD(string Item,decimal Quantity,string Station,string Container,string Kind,string Instance,bool Accessible,decimal Value);
public sealed record BuyLineD(string Item,int Quantity,string Station);
public sealed record HistoryBarD(int Day,int Volume,decimal Low,decimal High,decimal Average);
public static class CharacterItemsD
{
 public static readonly ItemDefinition[] All=new ItemDefinition[]{
  new("d_extractor","Neural extractor","Character / SP items","Removes 500,000 trained SP into one physical injector. Docked capsule required.",.1f,10000,""),new("d_injector","Large neural injector","Character / SP items","One physical item; diminishing SP yield by total allocated and unallocated points.",.1f,25000,""),
  new("d_accelerator","Neural accelerator","Character / Consumables","Adds 10 neural attributes for 24 simulated hours.",.1f,3000,""),new("d_expert","Fit qualification lease","Character / Consumables","Temporary fit qualifications for seven simulated days; grants no trained SP.",.1f,2000,""),
  new("d_tag","Security service tag","Rewards / Tags","Earned from completed security work; some LP offers require this tag.",.1f,50,""),new("d_datacore","Applied engineering datacore","Rewards / Research","Finite research output used for specialist acquisition.",.1f,100,""),new("d_faction_token","Corporation equipment voucher","Rewards / Special acquisition","Corporation reward, unavailable through ordinary manufacturing.",.1f,500,""),
  new("d_set_shield","Aegis shield implant","Character / Implant sets","Slot 6 or 7. Two implants provide 5% and 10% shield respectively; complete set adds 5%.",.1f,4500,"")}
  .Concat(SkillCatalog.AttributeNames.Keys.SelectMany(a=>Enumerable.Range(1,5).Select(l=>new ItemDefinition($"d_implant_{a}_{l}",$"{SkillCatalog.AttributeNames[a]} learning implant +{l}","Character / Learning implants","Clone-bound implant. Removal and pod loss destroy this instance.",.1f,500*l*l,"")))).ToArray();
 public static bool AcquisitionOnly(string id)=>id.StartsWith("d_")||id.StartsWith("skill_");
}
