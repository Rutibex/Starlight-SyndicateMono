using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public string InsureShipD(string tier)
 {
  if(!Docked||Commander.Capsule)return "Dock in the ship to insure.";decimal cover=tier=="Platinum"?1m:tier=="Standard"?.6m:tier=="Basic"?.5m:tier=="Bronze"?.7m:tier=="Silver"?.8m:tier=="Gold"?.9m:0;if(cover==0)return "Select Basic, Standard or Platinum.";
  if(Commander.Insurance.Any(p=>p.Ship==Player.InstanceId&&!p.Claimed&&p.Expires>Now))return "This ship instance already has an active policy.";
  decimal basis=InsuranceBasisD(Player),premium=Math.Round(basis*cover*.3m,2);if(Balance<premium)return "Insufficient premium funds.";WalletChange(-premium,"Insurance premium / "+tier);Commander.ServiceSinks+=premium;Commander.Insurance.Add(new(){Ship=Player.InstanceId,Tier=tier,Premium=premium,Payout=Math.Round(basis*cover,2),Expires=Now.AddDays(84)});HistoryD("Insured ship instance "+Player.InstanceId);return "";
 }
 public decimal InsuranceBasisD(Ship ship)=>IndustryCatalog37.Recipes.TryGetValue(ship.CombatHull,out var recipe)?Math.Max(100,recipe.Minerals.Sum(i=>i.Value*ItemCatalog.Get(i.Key).Price)):Math.Max(100,(decimal)(ship.MaxHull+ship.MaxArmor+ship.MaxShield)*.65m);
 public decimal ClaimInsuranceD(LossD loss)
 {
  if(!Commander.Losses.Contains(loss)||loss.Pod||loss.Insurance>0||loss.Attacker.StartsWith("concord",StringComparison.OrdinalIgnoreCase))return 0;var p=Commander.Insurance.FirstOrDefault(p=>p.Ship==loss.Ship&&!p.Claimed&&p.Expires>loss.At);if(p==null){p=new(){Ship=loss.Ship,Tier="Uninsured",Payout=Math.Round(InsuranceBasisD(Player)*.4m,2),Expires=Now.AddSeconds(1)};Commander.Insurance.Add(p);}
  p.Claimed=true;p.Loss=loss.Id;loss.Insurance=p.Payout;Commander.InsurancePaid+=p.Payout;WalletChange(p.Payout,"Insurance claim / "+loss.Id);return p.Payout;
 }
 public void LosePlayerD()
 {
  SyncWrecksD();foreach(var contract in Commander.Contracts.Where(c=>c.Kind==ContractKindD.Courier&&c.Carrier=="player"&&c.State=="In progress").ToArray())FailCourierD(contract.Id,false,true);var ship=Player;var loss=new LossD{At=Now,System=SystemId,Ship=ship.InstanceId,Hull=Commander.Capsule?"Capsule":ship.HullName,Pod=Commander.Capsule,Attacker=CombatEvents.LastOrDefault(e=>e.Target=="player")?.Source??"Unknown"};Commander.Losses.Add(loss);
  if(Commander.Capsule){foreach(var implant in Commander.Implants.Values)loss.DestroyedInstances.Add(implant.Id);foreach(var implant in Skills.Implants)loss.Destroyed["learning-implant-"+implant.Key]=1;Commander.Implants.Clear();Skills.Implants.Clear();RecoverPodD();HistoryD("Capsule lost; clone implants destroyed; medical home recovery");return;}
  var wreck=new WreckD{Id="wreck-d:"+loss.Id,System=SystemId,Grid=Grid,Position=ship.Position,Expires=Now.AddHours(2)};loss.Wreck=wreck.Id;
  void Drop(string item,decimal quantity){if(quantity<=0)return;decimal q=item=="ore"?quantity*.5m:decimal.Floor(quantity*.5m);if(q>0){wreck.Items[item]=wreck.Items.GetValueOrDefault(item)+q;loss.Dropped[item]=loss.Dropped.GetValueOrDefault(item)+q;}loss.Destroyed[item]=loss.Destroyed.GetValueOrDefault(item)+quantity-q;}
  foreach(var item in CargoItems)Drop(item.Key,item.Value);Drop("ore",(decimal)ship.Ore);Drop("ammo",ship.Missiles);
  foreach(var module in FittingLayoutModel.Installed(ship)){var state=ship.TacticalModule(module);if(state==null){Drop(module.Id,1);continue;}var saved=CaptureInstanceD(module.Kind,module.Id,state);bool survives=StableC(saved.Id+loss.Id)%2==0;if(survives){wreck.Instances.Add(saved);wreck.Items[module.Id]=wreck.Items.GetValueOrDefault(module.Id)+1;loss.Dropped[module.Id]=loss.Dropped.GetValueOrDefault(module.Id)+1;}else{loss.Destroyed[module.Id]=loss.Destroyed.GetValueOrDefault(module.Id)+1;loss.DestroyedInstances.Add(saved.Id);}}
  // Cargo metadata follows the surviving quantity, never both its lost and recovered stack.
  foreach(var bank in ConditionBanksD())foreach(var entry in bank.List.Cast<object>().Where(e=>PropertyD(e,"Hangar") is false).ToArray()){object state=PropertyD(entry,"State")!;string item=(string?)PropertyD(state,"ModuleId")??"";if(wreck.Instances.Count(i=>i.Item==item)<wreck.Items.GetValueOrDefault(item))wreck.Instances.Add(CaptureInstanceD(bank.Bank,item,state));else loss.DestroyedInstances.Add((string?)PropertyD(state,"InstanceId")??"");bank.List.Remove(entry);}
  foreach(var bp in Industry.Blueprints.Where(b=>b.Station=="cargo").ToArray()){if(StableC(bp.Id+loss.Id)%2==0)wreck.Instances.Add(new(){Bank="blueprint",Item=bp.Item,Id=bp.Id,Json=System.Text.Json.JsonSerializer.Serialize(bp)});else loss.DestroyedInstances.Add(bp.Id);Industry.Blueprints.Remove(bp);}
  ResolveCapitalLossQ(wreck);Commander.Wrecks.Add(wreck);ClaimInsuranceD(loss);LoseMissionCargo38();CargoItems.Clear();MissilesInFlight.Clear();Parasites.Clear();WeaponVisuals.Clear();EwarEffects.Clear();EwarZones.Clear();LeaveSpecialGrid(true);LeaveSupportGrid(true);LeaveMobilityGrid();MakeCapsuleD();Player.Position=ship.Position;Screen=ViewMode.Space;LockedId=SelectedId="";BindWrecksD();HistoryD("Ship lost / "+loss.Id+"; insurance "+loss.Insurance+"; capsule survives");Say("Ship destroyed. Your capsule survives. Wreck expires in two simulated hours.");
 }
 void RecoverPodD()
 {
  CargoItems.Clear();MissilesInFlight.Clear();Parasites.Clear();WeaponVisuals.Clear();EwarEffects.Clear();EwarZones.Clear();Targets.Clear();LockedId=SelectedId="";
  if(ImportedWorld43){RecoverImported();Commander.Capsule=false;}
  else{string home=LocationExists(RecoveryStationId)?RecoveryStationId:SystemId;SystemId=LocationSystem(home);StationId="";Grid="station";Screen=ViewMode.Station;Player=new Ship();Commander.Capsule=false;SyncSkills();WalletChange(-Math.Min(Balance,150m),"Medical recovery service");AdvanceHours(1);}
  HistoryD("One emergency starter vessel issued at medical home / "+StationKey44);Say("Medical home recovery complete. Active clone implants were lost. Stored ships and assets remain available.");
 }
 public void SyncWrecksD(){foreach(var w in Commander.Wrecks.Where(w=>!w.Despawned)){var target=Targets.FirstOrDefault(t=>t.Id==w.Id);if(target==null)continue;w.Position=target.Position;if(w.AbsoluteQ!=null)w.AbsoluteQ=SolarOrigin+SpacePosition.From(target.Position);foreach(var item in w.Items.Keys.ToArray())if(target.Contents.GetValueOrDefault(item)<(float)w.Items[item])w.Items[item]=(decimal)Math.Max(0,target.Contents.GetValueOrDefault(item));}}
 public void BindWrecksD(){foreach(var w in Commander.Wrecks.Where(w=>!w.Despawned&&w.Expires>Now&&w.System==SystemId&&w.Grid==Grid&&(w.CellQ==""||w.CellQ==SolarGridKey))){var t=Targets.FirstOrDefault(t=>t.Id==w.Id);if(t==null){t=new(){Id=w.Id,Name="Owned ship wreck",Kind=TargetKind.Wreck,Position=w.AbsoluteQ is {} absoluteQ?(absoluteQ-SolarOrigin).Local:w.Position,OwnerId=w.Id.StartsWith("wreck-d:q-safety:")?"abandoned":"player",Salvage=0};Targets.Add(t);}t.Contents=w.Items.Where(i=>i.Value>0).ToDictionary(i=>i.Key,i=>(float)i.Value);}}
 public bool LootWreckD(string id,string itemFilter="")
 {
  var w=Commander.Wrecks.FirstOrDefault(w=>w.Id==id);if(w==null||w.Despawned||w.Expires<=Now||Commander.Capsule||Docked||w.System!=SystemId||w.Grid!=Grid||Vector3.Distance(Player.Position,w.Position)>2500)return false;bool any=false;
  foreach(var item in w.Items.Where(i=>i.Value>0&&(itemFilter.Length==0||i.Key==itemFilter)).ToArray()){decimal room=(decimal)Math.Max(0,CargoRoom(item.Key)/(float)ItemVolume(item.Key));decimal q=item.Key=="ore"?Math.Min(item.Value,room):Math.Min(item.Value,decimal.Floor(room));if(q<=0)continue;ChangeStack(item.Key,false,q);w.Items[item.Key]-=q;foreach(var unique in w.Instances.Where(i=>i.Item==item.Key).Take((int)Math.Min(int.MaxValue,decimal.Floor(q))).ToArray()){RestoreInstanceD(unique,false,"");w.Instances.Remove(unique);}any=true;}
  foreach(var u in w.Instances.Where(u=>u.Bank=="blueprint"&&(itemFilter.Length==0||u.Item==itemFilter)).ToArray()){if(CargoRoom(u.Item)<.01f)break;if(RestoreInstanceD(u,false,"")){w.Instances.Remove(u);any=true;}}
  BindWrecksD();return any;
 }
}
