using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 static FleetHull[] OrderedBatch22(IEnumerable<FleetHull> hulls)=>hulls.OrderBy(h=>Array.IndexOf(new[]{"Boreal Federation","Amur Cooperative Compact","Suryavarta Empire","Azadi Confederation"},h.Faction)).ThenBy(h=>h.Class=="Cruiser"?0:h.Class=="Combat Battlecruiser"?1:2).ThenBy(h=>h.Name,StringComparer.Ordinal).ToArray();
 bool fleetElite=true;bool fleetStrategic=true;bool fleetCruisers=true;bool fleetDestroyers=true;bool fleetAdvanced=true;bool fleetSpecialists=true;int fleetFactionPage;int fleetBerthPageStageA;bool fleetBattleships=true;BatchHull[] FleetVisibleBatch=>fleetElite?ShipBatch21.EliteBattleships:fleetStrategic?ShipBatch21.StrategicCommand:fleetCruisers?ShipBatch21.AdvancedCruisers:fleetDestroyers?ShipBatch21.AdvancedDestroyers:fleetAdvanced?ShipBatch21.AdvancedFrigates:fleetSpecialists?ShipBatch21.Specialists:fleetBattleships?ShipBatch21.Battleships:ShipBatch21.Current;
 bool fleetCurrentBatch;bool fleetBatchOnly=true;bool fleetBrowser;int fleetPage,fleetFaction,fleetClass,fleetFieldPage;string fleetSelected="";
 void FleetRegistryWindow()
 {
  DrawWindow(new(142,93,1085,695),"Fleet registry / 569 hulls");
  ui.Box(new(142,121,1085,667),new Color(11,19,24,255));
  ui.Button(new(1010,132,190,28),"BACK TO FITTING",()=>fleetBrowser=false);
  var factions=new[]{"All factions"}.Concat(FleetCatalog.All.Select(h=>h.Faction).Distinct()).ToArray();
  var classes=new[]{"All classes"}.Concat(FleetCatalog.All.Select(h=>h.Class).Distinct().Order()).ToArray();
  ui.Button(new(160,137,36,26),"<",()=>{fleetFaction=(fleetFaction+factions.Length-1)%factions.Length;fleetPage=0;});
  ui.Text(factions[fleetFaction],205,145,Cyan,13);ui.Button(new(591,137,36,26),">",()=>{fleetFaction=(fleetFaction+1)%factions.Length;fleetPage=0;});
  ui.Button(new(160,174,36,26),"<",()=>{fleetClass=(fleetClass+classes.Length-1)%classes.Length;fleetPage=0;});
  ui.Text(classes[fleetClass],205,182,White,13);ui.Button(new(591,174,36,26),">",()=>{fleetClass=(fleetClass+1)%classes.Length;fleetPage=0;});
  ui.Button(new(874,132,125,28),fleetBatchOnly?FleetCatalog.Playable.Length+" PLAYABLE":"ALL 569",()=>{fleetBatchOnly=!fleetBatchOnly;fleetPage=0;});
  ui.Button(new(651,174,344,27),fleetCurrentBatch?(fleetElite?"MARAUDERS / BLACK OPS":fleetStrategic?"STRATEGIC / COMMAND SHIPS":fleetCruisers?"ADVANCED CRUISERS / FACTIONS":fleetDestroyers?"ADVANCED DESTROYERS / FACTIONS":fleetAdvanced?"ADVANCED FRIGATES / FACTIONS":fleetSpecialists?"SPECIALISTS / BY FACTION":fleetBattleships?"BATTLESHIPS / BY FACTION":"CRUISERS / BATTLECRUISERS"):"SHOW NEW "+(fleetElite?"MARAUDERS / BLACK OPS":fleetStrategic?"STRATEGIC / COMMAND":fleetCruisers?"ADVANCED CRUISERS":fleetDestroyers?"ADVANCED DESTROYERS":fleetAdvanced?"ADVANCED FRIGATES":fleetSpecialists?"SPECIALISTS":fleetBattleships?"BATTLESHIPS":"CRUISERS"),()=>{fleetCurrentBatch=!fleetCurrentBatch;fleetPage=0;fleetClass=0;if(fleetCurrentBatch){fleetFaction=0;fleetSelected=OrderedBatch22(FleetVisibleBatch.Select(p=>FleetCatalog.ById[p.ShipId])).First().Id;}});
  ui.Button(new(1003,174,195,27),"NEXT SHIP BATCH",()=>{if(fleetElite){fleetElite=false;fleetStrategic=true;}else if(fleetStrategic){fleetStrategic=false;fleetCruisers=true;}else if(fleetCruisers){fleetCruisers=false;fleetDestroyers=true;}else if(fleetDestroyers){fleetDestroyers=false;fleetAdvanced=true;}else if(fleetAdvanced){fleetAdvanced=false;fleetSpecialists=true;}else if(fleetSpecialists){fleetSpecialists=false;fleetBattleships=true;}else if(fleetBattleships){fleetBattleships=false;}else {fleetElite=true;fleetStrategic=false;fleetCruisers=false;fleetDestroyers=false;fleetAdvanced=false;fleetSpecialists=false;}fleetFactionPage=0;fleetCurrentBatch=true;fleetFaction=fleetClass=fleetPage=0;fleetSelected=OrderedBatch22(FleetVisibleBatch.Select(p=>FleetCatalog.ById[p.ShipId])).First().Id;},"Switch active ship batch");
  var list=FleetCatalog.All.Where(h=>(!fleetCurrentBatch||FleetVisibleBatch.Any(p=>p.ShipId==h.Id))&&(!fleetBatchOnly||h.Playable)&&(fleetFaction==0||h.Faction==factions[fleetFaction])&&(fleetClass==0||h.Class==classes[fleetClass])).ToArray();
  if(fleetCurrentBatch){list=OrderedBatch22(list);if(list.Length>0&&!list.Any(h=>h.Id==fleetSelected))fleetSelected=list[0].Id;if((captureScene.StartsWith("batch22")||captureScene.StartsWith("batch23")||captureScene.StartsWith("batch24")||(captureScene.StartsWith("batch25")||captureScene.StartsWith("batch26")))&&captureScene.Contains("registry")&&frame==0)fleetPage=Math.Max(0,Array.FindIndex(list,h=>h.Id==fleetSelected))/9;}
  int pages=Math.Max(1,(list.Length+8)/9);fleetPage=Math.Clamp(fleetPage,0,pages-1);
  int y=219;foreach(var h in list.Skip(fleetPage*9).Take(9)){var hull=h;if(HullThumbnail(h.Id) is {} thumbnail)ui.Image(thumbnail,new(160,y,38,33));ui.Button(new(202,y,418,33),(h.Playable?"[FLY] ":"")+h.Name,()=>{fleetSelected=hull.Id;fleetFieldPage=0;},h.Faction+" / "+h.Class,fleetSelected==h.Id);y+=39;}
  ui.Button(new(160,580,97,27),"Previous",()=>fleetPage=Math.Max(0,fleetPage-1));ui.Text($"{fleetPage+1}/{pages} / {list.Length} hulls",272,589,Muted,12);ui.Button(new(519,580,101,27),"Next",()=>fleetPage=Math.Min(pages-1,fleetPage+1));
  var selected=FleetCatalog.ById.GetValueOrDefault(fleetSelected)??list.FirstOrDefault()??FleetCatalog.Playable[0];
  ui.Button(new(651,132,210,28),"SHOW SHIP INFORMATION",()=>ShowShipInformation(selected.Id));
  ui.Text(selected.Name,651,213,Gold,20);ui.Text(selected.Faction+" / "+selected.Class,651,242,Cyan,12);
  ui.Wrap(selected.Description,651,270,547,White,12);ui.Wrap(selected.Quote,651,360,547,Muted,11);
  var fields=selected.RawFields.ToArray();int fp=Math.Max(1,(fields.Length+6)/7);fleetFieldPage=Math.Clamp(fleetFieldPage,0,fp-1);
  ui.Text("SOURCE ATTRIBUTES / RAW VALUES",651,406,Cyan,11);
  y=430;foreach(var (name,value) in fields.Skip(fleetFieldPage*7).Take(7)){ui.Text(name,651,y,White,11);ui.Text(value,1084,y,Gold,11);y+=20;}
  ui.Button(new(651,584,90,26),"Fields <",()=>fleetFieldPage=Math.Max(0,fleetFieldPage-1));ui.Text($"{fleetFieldPage+1}/{fp}",767,593,Muted,11);ui.Button(new(821,584,90,26),"Fields >",()=>fleetFieldPage=Math.Min(fp-1,fleetFieldPage+1));
  ui.Text(selected.Playable?world.ImportedWorld43?"Assemble a purchased or manufactured hull at this station.":"Home station: "+ShipBatch21.ById[selected.Id].HomeSystem:"Lineage: "+selected.SourceLineage,651,636,Muted,11);
  ui.Text(selected.Playable?"Integrated hull / traits active":"Imported reference / mechanics and model awaiting integration",651,616,selected.Playable?Green:Muted,11);
  if(selected.Playable)ui.Button(new(940,584,259,27),world.ImportedWorld43?"BOARD OWNED LOCAL HULL":"BOARD / "+(ShipBatch21.ById[selected.Id].Starter||ShipBatch21.ById[selected.Id].LegacyCharter?"FREE CHARTER":ShipBatch21.ById[selected.Id].Price+" CR"),()=>world.BoardFleetHull(selected.Id),"One starter vessel per hull per campaign. Full old ship and cargo remain at this station.");
  if(!world.ImportedWorld43&&selected.Playable&&!ShipBatch21.ById[selected.Id].Starter)ui.Button(new(940,618,259,26),"BUY ADDITIONAL / "+ShipBatch21.ById[selected.Id].Price+" CR",()=>world.BuyAdditionalFleetHull(selected.Id),"Purchase another fitted hull at its home station; board it through Local berths.");
  int x=160;
  if(fleetCurrentBatch){ui.Text("NEW SHIPS / GROUPED BY FACTION",160,632,Gold,11);if(fleetElite||fleetCruisers||fleetSpecialists||fleetAdvanced||fleetDestroyers)ui.Button(new(404,624,218,24),"MORE FACTIONS >",()=>fleetFactionPage=(fleetFactionPage+1)%((FleetVisibleBatch.Select(p=>FleetCatalog.ById[p.ShipId].Faction).Distinct().Count()+3)/4));foreach(string faction in FleetVisibleBatch.Select(p=>FleetCatalog.ById[p.ShipId].Faction).Distinct().Skip(fleetFactionPage*4).Take(4)){string group=faction;ui.Button(new(x,657,250,28),group,()=>{fleetFaction=Array.IndexOf(factions,group);fleetClass=0;fleetPage=0;fleetSelected=OrderedBatch22(FleetVisibleBatch.Select(p=>FleetCatalog.ById[p.ShipId]).Where(h=>h.Faction==group)).First().Id;},(fleetElite?"Show batch29 faction / ":fleetStrategic?"Show batch28 faction / ":fleetCruisers?"Show batch27 faction / ":fleetDestroyers?"Show batch26 faction / ":fleetAdvanced?"Show batch25 faction / ":fleetSpecialists?"Show batch24 faction / ":fleetBattleships?"Show batch23 faction / ":"Show batch22 faction / ")+group,factions[fleetFaction]==group);x+=261;}}
  else{ui.Text("STARTER CHARTER / choose any of the four ships once; no wallet charge",160,632,Gold,11);foreach(var h in FleetCatalog.Playable.Where(h=>ShipBatch21.ById[h.Id].LegacyCharter)){var hull=h;ui.Button(new(x,657,250,28),h.Name,()=>world.BoardFleetHull(hull.Id),"Board fleet / "+h.Id,world.Player.CombatHull==h.Id);x+=261;}}
  var berth=world.FleetBerths.Select((b,i)=>(b,i)).Where(p=>p.b.System==world.StationKey44).ToArray();
  ui.Text("LOCAL BERTHS / fittings, drones and cargo retained",160,698,Cyan,11);
  fleetBerthPageStageA=Math.Clamp(fleetBerthPageStageA,0,Math.Max(0,(berth.Length-1)/4));ui.Button(new(1050,690,70,23),"Prev",()=>fleetBerthPageStageA=Math.Max(0,fleetBerthPageStageA-1));ui.Button(new(1130,690,70,23),"Next",()=>fleetBerthPageStageA++);x=160;foreach(var (b,i) in berth.Skip(fleetBerthPageStageA*4).Take(4)){int index=i;ui.Button(new(x,718,250,27),b.Ship.HullName,()=>world.BoardFleetBerth(index));x+=261;}
  if(world.Player.IsFleetHull)ui.Text("SPACE: G toggles the full weapon rack; F1/F2 operate the first two. Drone bay controls launch and attack.",160,758,Muted,11);
 }
}
public sealed partial class DemoGame
{
 void FleetCombatHud()
 {
  var ship=world.Player;
  for(int i=0;i<ship.Weapons.Length;i++){
   int slot=i;var state=ship.WeaponAt(i);string label=state==null?$"HIGH {i+1} / EMPTY":$"H{i+1} {state.Definition.Kind} / {state.Loaded}";
   ui.Button(new(420+i*(ship.Weapons.Length>4?97:155),612,ship.Weapons.Length>4?91:148,28),label,()=>world.ToggleWeapon(slot),state?.Definition.Description??"Empty weapon socket",active:state?.Active==true);
  }
  ui.Button(new(420,651,148,27),"ALL WEAPONS [G]",world.ToggleFleetRack);
  ui.Button(new(575,651,148,27),"REPAIR [F3]",()=>world.ToggleModule(3));
  ui.Button(new(730,651,148,27),"FIT / MODULES [F]",()=>Open(ViewMode.Fitting));
  ui.Button(new(885,651,148,27),"DRONES [N]",()=>Open(ViewMode.Drones));
  if(ShipBatch21.ById[ship.CombatHull].Effects.Any(e=>e.Mode.Length>0))ui.Button(new(1040,651,150,27),ship.TacticalModeLabel(ship.HullTacticalMode).ToUpper(),()=>world.SetHullTacticalMode(ship.HullTacticalMode=="Primary"?"Secondary":ship.HullTacticalMode=="Secondary"?"Tertiary":"Primary"),"Cycle tactical mode / "+ship.TacticalModeDelay+" second switch delay");
 }
}
