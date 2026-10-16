namespace StarlightSyndicateDemo;
public sealed partial class DemoWorld
{
 public CommanderStateD Commander{get;set;}=new();
 public void HistoryD(string text){Commander.History.Add(Now.ToString("u")+" / "+text);if(Commander.History.Count>300)Commander.History.RemoveAt(0);}
 public string BuyConsumableD(string item)
 {
  if(!Docked||!CharacterItemsD.All.Any(i=>i.Id==item))return "Dock at a specialist trader.";if(item=="d_tag"||item=="d_datacore"||item=="d_faction_token")return "Earn this acquisition item through rewards or research.";
  EnsureTraderD();string key=StationKey44+":"+item;int available=Commander.TraderStock.GetValueOrDefault(key);decimal price=ItemCatalog.Get(item).Price;
  if(available<1)return "The specialist trader's finite stock is exhausted.";if(Balance<price)return "Insufficient wallet balance.";Commander.TraderStock[key]--;WalletChange(-price,"Specialist purchase / "+ItemName(item));Commander.ServiceSinks+=price;ChangeStack(item,true,1);HistoryD("Purchased physical "+ItemName(item));return "";
 }
 public void EnsureTraderD(){if(!Docked||!Commander.TraderStations.Add(StationKey44))return;foreach(var item in CharacterItemsD.All.Where(i=>i.Id!="d_tag"&&i.Id!="d_datacore"&&i.Id!="d_faction_token"))Commander.TraderStock[StationKey44+":"+item.Id]=6;HistoryD("Specialist trader registered: six opening units per consumable at "+LocationName(StationKey44));}
 public string ConsumeInjectorD()
 {
  if(!Docked)return "Dock before using SP items.";Skills.Advance(Now);if(StackCount("d_injector",true)<1)return "A physical injector is required in this station hangar.";
  double total=Skills.TotalPoints+Skills.Unallocated;int amount=total<5000000?500000:total<50000000?400000:total<80000000?300000:150000;ChangeStack("d_injector",true,-1);Skills.Unallocated+=amount;HistoryD("Injector consumed / "+amount+" SP credited");return "";
 }
 public string ExtractD(int id)
 {
  Skills.Advance(Now);if(!Docked||!Commander.Capsule)return "Extraction requires a docked capsule; leave the active ship first.";if(StackCount("d_extractor",true)<1)return "A physical extractor is required.";
  if(Skills.TotalPoints<5500000||!SkillCatalog.ById.ContainsKey(id)||Skills.Points.GetValueOrDefault(id)<500000)return "Requires 5.5 million trained SP and 500,000 extractable points in this skill.";
  if(PointsLocked(id)||Skills.Queue.Any(q=>q.Skill==id||SkillCatalog.ById[q.Skill].Requirements.Any(r=>r.Skill==id)))return "A trained or queued skill depends on these points. Remove queued levels and dependent training first.";
  Skills.Points[id]-=500000;ChangeStack("d_extractor",true,-1);ChangeStack("d_injector",true,1);HistoryD("Extractor consumed / 500,000 trained SP converted to one injector");SyncSkills();return "";
 }
 public string UseAcceleratorD(){if(!Docked||StackCount("d_accelerator",true)<1)return "Dock with a physical accelerator in the station hangar.";Skills.Advance(Now);if(Skills.AcceleratorExpiresUtc>Now)return "An accelerator is already active.";ChangeStack("d_accelerator",true,-1);Skills.AcceleratorBonus=10;Skills.AcceleratorExpiresUtc=Now.AddHours(24);HistoryD("Accelerator consumed; expires "+Skills.AcceleratorExpiresUtc.ToString("u"));return "";}
 public SkillRequirement[] FitGoalsD(Ship? fit=null){fit??=Player;return FittingDesignStore.Equipment(fit).Keys.Append(fit.CombatHull).SelectMany(SkillCatalog.Required).GroupBy(r=>r.Skill).Select(g=>new SkillRequirement(g.Key,g.Max(r=>r.Level))).ToArray();}
 public string LeaseFitD(){if(!Docked||Commander.Capsule||StackCount("d_expert",true)<1)return "Dock in a fitted ship with a qualification lease item.";var goals=FitGoalsD();var expanded=new PilotSkills().Expand(goals,false);ChangeStack("d_expert",true,-1);Skills.Leases.Add(new(){Name="Fit: "+Player.HullName,ExpiresUtc=Now.AddDays(7),Levels=expanded.GroupBy(e=>e.Skill).ToDictionary(g=>g.Key,g=>g.Max(e=>e.Level))});HistoryD("Seven-day fit qualification lease consumed; no SP granted");SyncSkills();return "";}
 public SkillPlan CreateFitPlanD(){var goals=FitGoalsD();var plan=new SkillPlan{Name=Player.HullName+" fit qualifications",Description="Permanent hull and fitted equipment access; expert leases do not count.",Entries=Skills.Expand(goals.Concat(new[]{"CPU Management","Power Grid Management","Navigation","Mechanics"}.Select(s=>new SkillRequirement(SkillCatalog.Id(s),3))),false),Milestones=["Hull access","Fitted equipment","Core skills III"]};Skills.Plans.Add(plan);Skills.TrackedPlan=plan.Id;Commander.Masteries.Add(new(){Name=plan.Name,Plan=plan.Id,Milestones=new(){["Hull access"]=SkillCatalog.Required(Player.CombatHull),["Fitted equipment"]=goals,["Core skills III"]=new[]{"CPU Management","Power Grid Management","Navigation","Mechanics"}.Select(s=>new SkillRequirement(SkillCatalog.Id(s),3)).ToArray()}});HistoryD("Fit skill plan created with permanent qualification milestones");return plan;}
 public bool MilestoneCompleteD(MasteryD mastery,string name)=>mastery.Milestones[name].All(r=>Skills.Trained(r.Skill)>=r.Level);
 public string InstallImplantD(string item,int slot=0)
 {
  if(!Docked||StackCount(item,true)<1)return "Dock with the physical implant in this hangar.";int bonus=0;string set="";
  if(item.StartsWith("d_implant_")&&item.Split('_') is {Length:4} parts&&int.TryParse(parts[2],out int attr)&&int.TryParse(parts[3],out bonus)&&SkillCatalog.AttributeNames.ContainsKey(attr)&&bonus is >=1 and <=5){slot=attr;}
  else if(item=="d_set_shield"&&slot is 6 or 7){bonus=slot==6?5:10;set="Aegis";}else return "Unsupported implant or slot.";
  int required=set.Length>0?3:bonus>=4?5:bonus==3?3:1;if(Skills.Trained(SkillCatalog.Id("Cybernetics"))<required)return "Train Cybernetics "+required+" first.";
  Skills.Advance(Now);ChangeStack(item,true,-1);Commander.Implants[slot]=new(){Item=item,Slot=slot,Bonus=bonus,Set=set};if(SkillCatalog.AttributeNames.ContainsKey(slot))Skills.Implants[slot]=bonus;HistoryD("Installed clone-bound "+ItemName(item)+"; previous implant destroyed");SyncSkills();return "";
 }
 public string RemoveImplantD(int slot){if(!Docked)return "Dock to remove implants.";Skills.Advance(Now);Commander.Implants.Remove(slot);Skills.Implants.Remove(slot);HistoryD("Implant in slot "+slot+" destroyed");SyncSkills();return "";}
 public float ImplantShieldD=>1+Commander.Implants.Values.Where(i=>i.Set=="Aegis").Sum(i=>i.Bonus)/100f+(Commander.Implants.Values.Count(i=>i.Set=="Aegis")==2?.05f:0);
 public string LeaveShipD(){if(Commander.Contracts.Any(c=>c.Carrier=="player"&&c.Kind==ContractKindD.Courier&&c.State=="In progress"))return "Deliver or break your sealed courier package before leaving this ship.";if(!Docked||Commander.Capsule)return "Dock in a ship first.";var cargo=new Dictionary<string,decimal>(CargoItems);if(Player.Ore>0)cargo["ore"]=cargo.GetValueOrDefault("ore")+(decimal)Player.Ore;if(Player.Missiles>0)cargo["ammo"]=cargo.GetValueOrDefault("ammo")+Player.Missiles;Player.Ore=0;Player.Missiles=0;ParkedShips.Add(new(){System=StationKey44,Ship=Player,Cargo=cargo});CargoItems.Clear();MakeCapsuleD();HistoryD("Ship parked at "+LocationName(StationKey44));return "";}
 public void MakeCapsuleD(){Player=new Ship();Player.SetCombatHull("frigate");FittingSimulation.ClearEquipment(Player);Player.CapsuleD=true;Player.Missiles=0;Player.Ore=0;Player.Shield=Player.Armor=Player.Hull=30;Player.Cap=100;Commander.Capsule=true;SyncSkills();}
 public string BoardShipD(string id){if(!Docked)return "Dock to board a stored ship.";var ship=ParkedShips.FirstOrDefault(s=>s.Id==id&&s.System==StationKey44);if(ship==null)return "Stored ship is not at this station.";if(!SkillAllowed(ship.Ship.CombatHull))return "Hull qualification required.";if(!Commander.Capsule){var error=LeaveShipD();if(error.Length>0)return error;}Player=ship.Ship;CargoItems=new(ship.Cargo);Player.Ore=(float)CargoItems.GetValueOrDefault("ore");Player.Missiles=(int)CargoItems.GetValueOrDefault("ammo");CargoItems.Remove("ore");CargoItems.Remove("ammo");ParkedShips.Remove(ship);Commander.Capsule=false;SyncSkills();RecomputeWeaponFit();return "";}
 public void EnforceQualificationsD(){if(IgnoreSkillRequirements||SkillCatalog.VerificationAllV)return;foreach(var module in FittingLayoutModel.Installed(Player)){if(SkillCatalog.Required(module.Id).All(r=>Skills.Effective(r.Skill)>=r.Level))continue;var state=Player.TacticalModule(module);if(state==null)continue;state.GetType().GetProperty("Online")?.SetValue(state,false);state.GetType().GetProperty("Active")?.SetValue(state,false);state.GetType().GetMethod("Stop",Type.EmptyTypes)?.Invoke(state,null);}}
 public void AdvanceCommanderD()
 {
  foreach(var c in Commander.Contracts.Where(c=>c.State=="Outstanding"&&c.Expires<=Now).ToArray()){if(c.Kind==ContractKindD.Auction&&c.Bidder.Length>0)c.State="Finished";else CancelContractD(c.Id);}
  foreach(var c in Commander.Contracts.Where(c=>c.Kind==ContractKindD.Courier&&c.State=="In progress"&&c.Carrier!="player").ToArray())AdvanceCourierD(c);
  foreach(var wreck in Commander.Wrecks.Where(w=>!w.Despawned&&w.Expires<=Now)){wreck.Items.Clear();wreck.Instances.Clear();wreck.Despawned=true;Targets.RemoveAll(t=>t.Id==wreck.Id);HistoryD("Wreck expired / "+wreck.Id);}
  Player.CapsuleD=Commander.Capsule;Player.PilotShieldD=Commander.Capsule?1:ImplantShieldD;
 }
}
public sealed partial class Ship {public float PilotShieldD{get;set;}=1;public bool CapsuleD{get;set;}public double CapsuleStatD(string key)=>key switch{"hp" or "shieldCapacity" or "armorHP"=>30,"capacity"=>0,"signatureRadius"=>25,"maxVelocity"=>400,"mass"=>32000,"agility"=>1,"capacitorCapacity"=>100,"rechargeRate" or "shieldRechargeRate"=>100000,"warpSpeedMultiplier"=>3,"warpCapacitorNeed"=>.0000045,"shieldEmDamageResonance" or "shieldExplosiveDamageResonance" or "shieldKineticDamageResonance" or "shieldThermalDamageResonance" or "armorEmDamageResonance" or "armorExplosiveDamageResonance" or "armorKineticDamageResonance" or "armorThermalDamageResonance" or "hullEmDamageResonance" or "hullExplosiveDamageResonance" or "hullKineticDamageResonance" or "hullThermalDamageResonance"=>1,_=>0};}
