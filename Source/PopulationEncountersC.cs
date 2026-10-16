using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public void ResolvePopulationAmbushC(ShipmentC op,PilotC attacker)
 {
  if(op.State is "Delivered" or "Lost")return;var defender=PopulationPilot(op.Pilot)!;if(attacker.Fit.Hull<=0||defender.Fit.Hull<=0)return;RememberC(attacker,"Observed convoy "+defender.Name+" on local route.");RememberC(defender,"Intercepted by "+attacker.Name);PopulationLogC("Abstract local encounter",attacker.Id,detail:op.Id);
  for(int round=0;round<3&&attacker.Fit.Hull>0&&defender.Fit.Hull>0;round++){
   Fire(attacker,defender);if(defender.Fit.Hull>0)Fire(defender,attacker);
  }
  if(defender.Fit.Hull<=0)ResolveShipmentC(op.Id,true);if(attacker.Fit.Hull<=0)LosePilotC(attacker);
  void Fire(PilotC from,PilotC to){var gun=from.Fit.Weapons[0];var d=gun.Definition;if(gun.Burnt||!gun.Online)return;var q=TacticalRulesB.Weapon(from.Fit,gun);if(from.Fit.Cap<q.Capacitor)return;if(d.Magazine>0&&gun.Loaded<=0){int available=(int)from.Cargo.GetValueOrDefault(PopulationAmmoC);int load=Math.Min(d.Magazine,available);if(load==0)return;from.Cargo[PopulationAmmoC]-=load;gun.Loaded=load;return;}
   from.Fit.Cap-=q.Capacitor;if(d.Magazine>0){gun.Loaded--;Population.GoodsConsumed++;}float chance=TacticalRulesB.TurretProbability(q.Gun.Optimal,q.Gun.Falloff,q.Gun.Tracking,q.Gun.SignatureResolution,3500,0,to.Fit.Signature);float quality=TacticalRulesB.HitQuality(chance,CombatRoll());var damage=TacticalRulesB.Damage(to.Fit,q.Packet.Scale(quality),to.Fit.Shield,to.Fit.Armor,to.Fit.Hull);to.Fit.Shield=damage.Shield;to.Fit.Armor=damage.Armor;to.Fit.Hull=damage.Hull;from.Experience++;
  }
 }
 void PreparePopulationDepartureC(){SyncPopulationGridC();foreach(var p in Population.Pilots.Where(p=>p.Contracted)){if(Targets.Any(t=>t.Id==p.Id)&&WarpBlocked(p.Id)){p.Order="Hold";RememberC(p,"Fleet departed; tackle prevented following.");}}foreach(var op in Population.Shipments.Where(o=>o.State=="On grid"))op.State="Transit";}
}
