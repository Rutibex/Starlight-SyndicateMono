using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class Ship
{
 public bool MiningFocusLoaded{get;set;}public float MiningFocusWear{get;set;}
}
public sealed partial class DemoWorld
{
 public bool SalvagerOn{get;set;}public float SalvagerLeft{get;set;}public string SalvagerTarget{get;set;}="";
 public bool LoadMiningFocus(bool hangar)
 {if(!Docked||Player.MiningFocusLoaded||ItemCount("mining_focus",hangar)<1)return false;ChangeStack("mining_focus",hangar,-(decimal)(1));Player.MiningFocusLoaded=true;Player.MiningFocusWear=0;Say("Calibrated mining prism loaded. Preservation pulses reduce its wear probability.");return true;}
 public float MiningHarvest(SpaceTarget asteroid,float yield,bool useFocus=true)
 {
  if(asteroid.Id.StartsWith("f:")){float mined=HarvestEncounterF(asteroid,yield);RecordExperienceI("mine",(decimal)mined);return mined;}
  if(asteroid.Id.StartsWith("mission:")){if(asteroid.Resource!="ore")return 0;float mined=HarvestMission38(asteroid,yield);RecordExperienceI("mine",(decimal)mined);return mined;}
  float amount=Math.Min(yield*(useFocus&&Player.MiningFocusLoaded?1.12f:1),Math.Min(asteroid.Ore,Math.Max(0,Player.CargoMax-CargoUsed)));
  asteroid.Ore=Math.Max(0,asteroid.Ore-amount);float baseAmount=amount;
  if(RollProductionE()<.01*Player.SupportFactor("miningCrit"))amount+=Math.Max(0,Math.Min(baseAmount*2,Player.CargoMax-CargoUsed-baseAmount));
  Player.Ore+=amount;
  if(RollProductionE()<.12*Player.SupportFactor("miningResidue"))asteroid.Ore=Math.Max(0,asteroid.Ore-baseAmount*.35f);
  if(useFocus&&Player.MiningFocusLoaded&&RollProductionE()<.1*Player.SupportFactor("crystalWear")){Player.MiningFocusWear=Math.Min(1,Player.MiningFocusWear+.12f);if(Player.MiningFocusWear>=1){Player.MiningFocusLoaded=false;Say("Mining prism exhausted; base mining continues.");}}
  RecordExperienceI("mine",(decimal)amount);return amount;
 }
 public bool ToggleSalvager()
 {if(Docked||Player.UtilityFit!="salvager"||Locked?.Kind!=TargetKind.Wreck){Say("Fit a salvager and lock a wreck.");return false;}SalvagerOn=!SalvagerOn;if(!SalvagerOn){SalvagerLeft=0;SalvagerTarget="";}return true;}
 void UpdateSupportSalvage(float dt)
 {
  if(!SalvagerOn)return;var t=Locked;if(t?.Kind!=TargetKind.Wreck||Vector3.Distance(Player.Position,t.Position)>5000*Player.SupportFactor("salvageRange")){SalvagerOn=false;SalvagerLeft=0;return;}
  if(SalvagerLeft<=0)
  {if(Player.Cap<5){SalvagerOn=false;return;}Player.Cap-=5;SalvagerTarget=t.Id;SalvagerLeft=10*Player.SupportFactor("salvageCycle");return;}
  SalvagerLeft=Math.Max(0,SalvagerLeft-dt);if(SalvagerLeft==0&&t.Id==SalvagerTarget&&RollProductionE()<Math.Clamp(.6f-t.SalvageDifficulty,0,1))
  {int q=Math.Min((int)t.Salvage,(int)Math.Max(0,(Player.CargoMax-CargoUsed)/.2f));if(q>0){CargoItems["salvage"]=CargoItems.GetValueOrDefault("salvage")+q;t.Salvage-=q;Say("Recovery beam extracted "+q+" components.");}else SalvagerOn=false;}
 }
}
