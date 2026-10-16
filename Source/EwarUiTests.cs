using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunEwarUiChecks(int stage)
 {
  if(stage>10)return;
  void Click(string prefix){var h=ui.Hits.Last(h=>h.Tip.StartsWith(prefix,StringComparison.Ordinal));TestClick(h.Rect.Center.ToVector2());}
  switch(stage)
  {
   case 0:UiCheck(fittingEwar&&world.Docked&&world.WeaponTestMode,"EWAR fitting opens with trial stock");ewarFamily=Array.IndexOf(EwarFamilies,"Sensor Dampener");ewarPage=0;break;
   case 1:Click("Fog sensor suppressor /");break;
   case 2:UiCheck(world.Player.Ewar[1].Definition?.Kind==EwarKind.Damp,"EWAR browser fits sensor suppressor");Click("Fog range-focus script / Load");break;
   case 3:UiCheck(world.Player.Ewar[1].Mode=="Range"&&world.Player.Ewar[1].Loaded==1,"Reusable focus script loads through UI");Click("EW HIGH / F5");break;
   case 4:UiCheck(world.FittingEwarSlot==0,"EWAR socket selector changes destination");ewarFamily=Array.IndexOf(EwarFamilies,"Energy Neutralizer");ewarPage=0;break;
   case 5:Click("Small Null capacitive drain /");break;
   case 6:UiCheck(world.Player.Ewar[0].Definition?.Kind==EwarKind.Neutralizer,"High-power neutralizer fits through UI");Click("EW RANGE");break;
   case 7:UiCheck(world.Grid=="ewarrange"&&world.Targets.Count==6,"EWAR range launches all sensor-profile targets");world.SelectedId=world.LockedId="range_0";world.WeaponSafety=false;Click("ACTIVATE EWAR");break;
   case 8:UiCheck(world.Player.Ewar[0].Active&&world.Targets[0].Cap<world.Targets[0].Profile!.MaxCap,"UI activation drains real target capacitor");Click("EW MID 1 / F4");break;
   case 9:UiCheck(world.FittingEwarSlot==1,"EWAR activation selector returns to scripted mid slot");Click("ACTIVATE EWAR");break;
   case 10:var effect=world.EwarFor("range_0");UiCheck(effect.LockRange<1&&effect.ScanResolution==1,"Scripted sensor suppression affects live targeting");Console.WriteLine("9 EWAR graphical interaction checks passed.");break;
  }
  oldMouse=Microsoft.Xna.Framework.Input.Mouse.GetState();
 }
}
