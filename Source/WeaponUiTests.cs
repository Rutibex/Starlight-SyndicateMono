using Microsoft.Xna.Framework;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunWeaponUiChecks(int stage)
 {
  if(stage>8)return;
  void Click(string tip){var hit=ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}
  switch(stage)
  {
   case 0:UiCheck(world.WeaponTestMode&&world.Docked,"Trial service opens docked fitting with stock");Click(WeaponCatalog.All[0].Name+" /");break;
   case 1:UiCheck(world.Player.Weapons[0].ModuleId==WeaponCatalog.All[0].Id,"Weapon browser fits owned artillery through UI");Click(ChargeCatalog.For(world.Player.Weapons[0].Definition)[0].Name+" / Load");break;
   case 2:UiCheck(world.Player.Weapons[0].Loaded>0,"Charge button fills a real magazine");Click("HIGH 2 / UTILITY");break;
   case 3:UiCheck(world.FittingWeaponSlot==1,"High-slot selector changes fitting destination");weaponFamily=Array.IndexOf(WeaponFamilies,"Missile Launcher Defender");weaponPage=0;break;
   case 4:Click(WeaponCatalog.All.First(w=>w.Kind==WeaponKind.Defender).Name+" /");break;
   case 5:UiCheck(world.Player.Weapons[1].Definition.Kind==WeaponKind.Defender,"Defender fits in selected high slot");Click(ChargeCatalog.Default(world.Player.Weapons[1].Definition)!.Name+" / Load");break;
   case 6:UiCheck(world.Player.Weapons[1].Loaded==8,"Interceptor charges load through UI");Click("RANGE");break;
   case 7:UiCheck(world.Grid=="range"&&!world.Docked&&world.Targets.Count==6,"Range button launches real test encounter");world.WeaponSafety=false;world.MissilesInFlight.Add(new(){Enemy=true,Bomb=true,Position=world.Player.Position+new Vector3(800,0,0),Hp=70,Speed=0,Life=100});Click("ACTIVATE");break;
   case 8:UiCheck(world.Player.Weapons[1].Active,"Activation button runs defender without a ship lock");world.Update(.2f);UiCheck(!world.MissilesInFlight.Any(m=>m.Enemy&&m.Bomb),"UI-activated defender intercepts range ordnance");Console.WriteLine("9 weapon UI interaction checks passed.");break;
  }
  oldMouse=Microsoft.Xna.Framework.Input.Mouse.GetState();
 }
}
