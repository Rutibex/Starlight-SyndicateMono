using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace StarlightSyndicateDemo;
public sealed partial class SpaceRenderer
{
 void DrawSolarBackdrop(DemoWorld world){if(world.Docked)return;device.DepthStencilState=DepthStencilState.None;foreach(var c in world.Celestials.Where(c=>c.Kind is CelestialKind.Sun or CelestialKind.Planet or CelestialKind.Moon)){
 var delta=c.Position-world.SystemPosition-SpacePosition.From(CameraPosition-world.Player.Position);double distance=delta.Length;if(distance<c.Radius)continue;float radius=(float)(c.Radius/distance*70000);if(radius<2)continue;var position=CameraPosition+delta.Unit.Local*70000;
 effect.TextureEnabled=c.Kind!=CelestialKind.Sun;effect.Texture=planet;effect.LightingEnabled=c.Kind!=CelestialKind.Sun;effect.EmissiveColor=c.Kind==CelestialKind.Sun?new Vector3(.95f,.7f,.25f):Vector3.Zero;
 Color color=c.Kind==CelestialKind.Sun?new(255,215,130):c.Kind==CelestialKind.Moon?new(143,140,132):c.Type switch{"Lava"=>new(190,105,70),"Gas giant"=>new(191,164,135),"Ice"=>new(160,194,222),"Barren"=>new(168,145,116),_=>new(157,195,198)};
 Mesh(sphere,Matrix.CreateScale(radius)*Matrix.CreateRotationY(.6f)*Matrix.CreateTranslation(position),color);
 }effect.EmissiveColor=Vector3.Zero;effect.TextureEnabled=false;effect.LightingEnabled=true;device.DepthStencilState=DepthStencilState.Default;}
 void DrawSolarGate(Vector3 at,float time,bool wormhole=false){for(int i=0;i<24;i++){float a=i*MathF.Tau/24;var p=at+new Vector3(MathF.Cos(a)*900,MathF.Sin(a)*900,0);Mesh(cube,Matrix.CreateScale(wormhole?70:105,145,120)*Matrix.CreateRotationZ(a)*Matrix.CreateTranslation(p),wormhole?new Color(102,73,148):new Color(100,128,141));}effect.LightingEnabled=false;effect.EmissiveColor=wormhole?new(.3f,.1f,.5f):new(.05f,.4f,.65f);Mesh(sphere,Matrix.CreateScale(650,650,25)*Matrix.CreateTranslation(at),wormhole?new Color(72,35,110):new Color(24,104,142));effect.EmissiveColor=Vector3.Zero;effect.LightingEnabled=true;}
}
