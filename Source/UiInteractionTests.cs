using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace StarlightSyndicateDemo;
public sealed partial class DemoGame
{
    void TestPointer(Vector2 at,ButtonState left=ButtonState.Released,ButtonState right=ButtonState.Released)
    {
        mouse=at;var state=new MouseState((int)at.X,(int)at.Y,0,left,ButtonState.Released,right,ButtonState.Released,ButtonState.Released);
        DesktopInput(state,new KeyboardState());oldMouse=state;
    }
    void TestClick(Vector2 at){TestPointer(at);TestPointer(at,ButtonState.Pressed);TestPointer(at);}
    void TestDrag(Vector2 from,Vector2 to){TestPointer(from);TestPointer(from,ButtonState.Pressed);TestPointer(to,ButtonState.Pressed);TestPointer(to);}
    void UiCheck(bool ok,string label){if(!ok)throw new Exception("UI FAIL: "+label);Console.WriteLine("UI PASS: "+label);}
    void RunUiChecks(int stage)
    {
        if(stage>8)return;
        var inv=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Inventory);var fit=desktop.FirstOrDefault(w=>w.Kind==ViewMode.Fitting);
        if(inv==null||fit==null)throw new Exception("UI test requires --scene desktop");
        switch(stage)
        {
            case 0:
                var before=inv.Bounds;TestDrag(new(inv.X+80,inv.Y+12),new(inv.X+120,inv.Y+37));
                UiCheck(inv.X==before.X+40&&inv.Y==before.Y+25,"Title dragging moves the window");break;
            case 1:
                int width=inv.Width,height=inv.Height;TestDrag(new(inv.Bounds.Right-5,inv.Bounds.Bottom-5),new(inv.Bounds.Right+55,inv.Bounds.Bottom+25));
                UiCheck(inv.Width==width+60&&inv.Height==height+30,"Resize grip changes client dimensions");break;
            case 2:
                TestClick(new(inv.X+50,inv.Y+130));UiCheck(showHangar,"Location tree receives clicks after moving and resizing");break;
            case 3:
                var ammo=itemHits.Single(h=>h.Id=="ammo");float original=world.ItemCount("ammo",false);
                TestDrag(ammo.Rect.Center.ToVector2(),new(inv.X+50,inv.Y+86));
                UiCheck(world.ItemCount("ammo",false)==original+12&&world.ItemCount("ammo",true)==0,"Drag transfers a real stack to cargo");break;
            case 4:
                var miner=itemHits.Single(h=>h.Id=="miner");var moduleSocket=fitSockets.Single(s=>s.Module?.Kind=="legacy");var dest=ui.Hits.Last(h=>h.Owner==ViewMode.Fitting.ToString()&&h.Tip==FitSocketTip(moduleSocket)).Rect.Center.ToVector2();
                TestDrag(miner.Rect.Center.ToVector2(),dest);UiCheck(world.Player.UtilityFit=="miner"&&world.ItemCount("analyzer",true)==1,"Cross-window drag fits and returns the previous module");break;
            case 5:
                var item=itemHits.First();TestPointer(item.Rect.Center.ToVector2(),right:ButtonState.Pressed);TestPointer(item.Rect.Center.ToVector2());
                UiCheck(context&&popupActions.Any(a=>a.Label.Contains("Move stack")),"Right-click offers location-specific inventory actions");context=false;break;
            case 6:
                TestClick(new(fit.X+70,fit.Y+12));UiCheck(desktop.Last()==fit,"Click brings a window to the front");
                TestClick(new(fit.Bounds.Right-37,fit.Y+12));UiCheck(fit.Minimized&&fit.Bounds.Height==29,"Minimize preserves the workspace");break;
            case 7:
                TestClick(new(fit.Bounds.Right-37,fit.Y+12));UiCheck(!fit.Minimized,"Restore reopens the client area");
                SaveDesktop();placements.Clear();LoadDesktop();UiCheck(placements[ViewMode.Inventory].X==inv.X&&placements[ViewMode.Inventory].Width==inv.Width,"Window geometry persists to disk");break;
            case 8:
                desktop.Remove(inv);desktop.Add(inv);Console.WriteLine("10 UI interaction checks passed.");break;
        }
        oldMouse=Mouse.GetState();
    }
}
