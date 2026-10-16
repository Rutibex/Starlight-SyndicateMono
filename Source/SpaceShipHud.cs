using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
 sealed record FlightModule(string Bank,string Name,bool Active,float Progress,Action Action,string Tip);
 void SolarShipHud(){var s=world.Player;var center=new Vector2(733,790);ui.Center(world.InWarp?"WARP DRIVE ACTIVE":s.Order=="stop"?"SHIP STOPPED":s.Order.ToUpper(),733,696,Cyan,13);
 for(int i=0;i<3;i++){float value=i==0?s.Shield/s.MaxShield:i==1?s.Armor/s.MaxArmor:s.Hull/s.MaxHull;ui.Arc(center,87-i*10,MathF.PI*.83f,MathF.PI*1.34f,new Color(54,65,71),7);ui.Arc(center,87-i*10,MathF.PI*.83f,MathF.PI*1.34f*Math.Clamp(value,0,1),i==0?White:i==1?Muted:Gold,7);}ui.Circle(center,52,new Color(105,130,139),2);
 for(int i=0;i<30;i++){float a=i*MathF.Tau/30;ui.Line(center+new Vector2(MathF.Cos(a),MathF.Sin(a))*39,center+new Vector2(MathF.Cos(a),MathF.Sin(a))*46,i<30*s.Cap/s.MaxCap?Gold:new Color(57,59,55),5);}ui.Center($"{s.Cap/s.MaxCap:P0}",733,777,Gold,17);ui.Center("CAP",733,801,Muted,10);ui.Center(world.InWarp?$"{world.Transit!.Curve.Sample(world.Transit.Elapsed).Velocity/SolarCatalog.AU:0.00} AU/s":$"{s.Velocity.Length():0} m/s",733,860,White,14);ui.Button(new(620,834,38,30),"0",world.StopShip,"Stop ship [Space]");ui.Button(new(806,834,42,30),"TAC",()=>renderer.Tactical=!renderer.Tactical,"Tactical ranges [T]",renderer.Tactical);
 DrawFlightRackK();
 ui.Text($"Shield {s.Shield/s.MaxShield:P0}  Armor {s.Armor/s.MaxArmor:P0}  Hull {s.Hull/s.MaxHull:P0}  Cargo {world.CargoUsed:0}/{s.CargoMax:0}",857,678,Muted,10);
 if(s.IsFleetHull){ui.Button(new(857,704,110,24),"ALL WEAPONS",world.ToggleFleetRack,"Toggle fitted weapon rack");if(ShipBatch21.ById[s.CombatHull].Effects.Any(e=>e.Mode.Length>0))ui.Button(new(974,704,130,24),s.TacticalModeLabel(s.HullTacticalMode).ToUpper(),()=>world.SetHullTacticalMode(s.HullTacticalMode=="Primary"?"Secondary":s.HullTacticalMode=="Secondary"?"Tertiary":"Primary"),"Cycle tactical mode / "+s.TacticalModeDelay+" second switch delay");}
 }
}
