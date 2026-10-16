using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 void RunFleetUiChecks(int stage)
 {
  void Click(string tip){var hit=ui.Hits.Last(h=>h.Tip.StartsWith(tip,StringComparison.Ordinal));TestClick(hit.Rect.Center.ToVector2());}
  var kestrel=FleetCatalog.Source("Kestrel");
  switch(stage)
  {
   case 0:UiCheck(fleetBrowser&&world.Docked,"Fleet registry opens in docked fitting");Click("Board fleet / "+kestrel.Id);break;
   case 1:UiCheck(world.Player.CombatHull==kestrel.Id&&world.Player.Weapons.Length==4,"Starter button boards four-launcher hull");Click("BACK TO FITTING");break;
   case 2:UiCheck(!fleetBrowser,"Back button returns to weapon fitting");Click("HIGH 4");break;
   case 3:UiCheck(world.FittingWeaponSlot==3,"Fourth high socket selectable");Click("UNFIT SELECTED WEAPON");break;
   case 4:UiCheck(world.Player.WeaponAt(3)==null,"UI unfit empties fourth socket");weaponFamily=Array.IndexOf(WeaponFamilies,"Missile Launcher Light");weaponPage=0;break;
   case 5:Click(WeaponCatalog.All.First(w=>w.Family=="Missile Launcher Light").Name+" /");break;
   case 6:UiCheck(world.Player.WeaponAt(3)!=null,"UI refits returned launcher in fourth socket");Click(ChargeCatalog.Default(world.Player.WeaponAt(3)!.Definition)!.Name+" / Load");break;
   case 7:UiCheck(world.Player.WeaponAt(3)!.Loaded>0,"Fourth magazine loads through charge button");Click("FLEET REGISTRY / 569");break;
   case 8:UiCheck(fleetBrowser,"Fleet browser reachable from fitting");Click("Board fleet / "+FleetCatalog.Source("Punisher").Id);break;
   case 9:UiCheck(world.Player.HullName=="Samudraprabha","Board contrasting laser hull from UI");world.Save();var loaded=new DemoWorld();UiCheck(loaded.Load()&&loaded.FleetBerths.Any(b=>b.Ship.CombatHull==kestrel.Id&&b.Ship.Weapons.Length==4),"UI changes persist with berthed full racks");break;
   case 10:Click("Board fleet / "+kestrel.Id);break;
   case 11:UiCheck(world.Player.WeaponAt(3)!.Loaded>0,"Board button restores loaded fourth magazine");Click("BACK TO FITTING");break;
   case 12:Click("HIGH 4");break;
   case 13:UiCheck(world.FittingWeaponSlot==3,"Restored hull's fourth socket usable");Console.WriteLine("12 fleet UI interaction checks passed.");break;
  }
  oldMouse=Mouse.GetState();
 }
}
