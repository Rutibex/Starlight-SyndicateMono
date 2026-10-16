using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public void InitializeSpecialCluster(){foreach(var s in Systems.Where(s=>s.Security<.5f))RelayNodes.Add(new(){Id="relay_"+s.Id,System=s.Id});var eligible=Systems.Where(s=>s.Security<.5f).ToArray();if(eligible.Length>=2)Wormholes.Add(new(){Id="fracture_1",A=eligible[0].Id,B=eligible[^1].Id,RemainingMass=150000000,MaxShipMass=5000000});}
 public void CloakObservatoryPulse(){if(Docked)return;foreach(var s in Player.Special.Where(s=>s.Active&&s.Definition!.Cloak&&s.ActiveSeconds>=900).ToArray())if(_rng.NextDouble()<.5){StopSpecial(s);Say("Observatory pulse disrupted your unstabilized cloak.");}}
 public bool ConfigureTitanAffinity(string affinity){if(!Docked||CombatHulls.Get(Player.CombatHull).Role!="Titan"||!new[]{"Aurora","Boreal","Verdant","Ember"}.Contains(affinity)||Player.Special.Any(s=>s.Definition?.Kind=="Phenomena"))return false;TitanAffinity=affinity;return true;}
 public void RefreshCapitalDrones()
 {
  foreach(var u in Player.Drones.Where(u=>u.Owner=="player"&&u.Location=="Space")){
   var t=Targets.FirstOrDefault(t=>t.DroneId==u.Id);if(t?.Profile==null)continue;var d=EffectiveDrone(u);var p=t.Profile;float sh=t.Shield/Math.Max(1,p.MaxShield),ar=t.Armor/Math.Max(1,p.MaxArmor),hu=t.Hull/Math.Max(1,p.MaxHull);p.SetStat("shieldCapacity",Math.Max(1,d.Shield));p.SetStat("armorHP",Math.Max(1,d.Armor));p.SetStat("hp",Math.Max(1,d.Hull));t.Shield=sh*p.MaxShield;t.Armor=ar*p.MaxArmor;t.Hull=hu*p.MaxHull;u.Shield=t.Shield;u.Armor=t.Armor;u.Hull=t.Hull;
  }
 }
 public bool JumpFleetClone(string id){var c=JumpClones.FirstOrDefault(c=>c.Id==id&&c.Mobile&&c.Pilot!="player");var w=Wingmates.FirstOrDefault(w=>w.Id==c?.Pilot&&w.Fleet);if(c==null||w==null||!Player.HasSpecial("Vat")||c.System!=SystemId||CloneJumpCooldown>0)return false;float cost=100;if(Credits<cost)return false;WalletChange(-(decimal)(cost),"Gameplay transaction");w.System=SystemId;w.Hull=0;w.Order="Hold";CloneJumpCooldown=86400;Say(w.Name+" transferred to the clone nursery; dock and buy a replacement vessel.");return true;}
 public bool AssignFleetHull(string id,string hull){if(!Docked)return false;var w=Wingmates.FirstOrDefault(w=>w.Id==id&&w.Fleet);var h=CombatHulls.All.FirstOrDefault(h=>h.Id==hull&&h.Size<4);if(w==null||h==null||w.UtilityCargo.Sum(x=>x.Value*ItemVolume(x.Key))>h.Cargo)return false;float cost=WeaponTestMode?0:h.Hull*.4f;if(Credits<cost)return false;WalletChange(-(decimal)(cost),"Gameplay transaction");var p=new Ship();p.SetCombatHull(hull);w.HullId=hull;w.System=SystemId;w.Shield=p.MaxShield;w.Armor=p.MaxArmor;w.Hull=p.MaxHull;w.Cap=p.MaxCap;w.Order="Follow";if(h.Role is "MiningBarge" or "MiningTender" or "ExpeditionFrigate")w.Role="Miner";Say("Fleet hull assigned: "+h.Name+(WeaponTestMode?" / LOAN":" / purchased"));return true;}

}
