using Microsoft.Xna.Framework;
using static StarlightSyndicateDemo.HudCanvas;

namespace StarlightSyndicateDemo;

// The first dense UI pass. These windows deliberately use Starlight names and the
// existing simulation; unavailable mechanics are displayed as specifications only.
public sealed partial class DemoGame
{
    static readonly string[] StoreNames=ItemCatalog.All.Select(i=>i.Name).ToArray();
    static readonly string[] StoreGroups=ItemCatalog.All.Select(i=>i.Group).ToArray();
    static readonly string[] StoreDetails=ItemCatalog.All.Select(i=>i.Description).ToArray();
    static readonly decimal[] StorePrices=ItemCatalog.All.Select(i=>i.Price).ToArray();
    void FitUtility(int choice)
    {
        string id=choice==1?"missile":choice==2?"miner":"analyzer";
        world.FitItem(id,world.ItemCount(id,true)>0);
    }
    void Glyph(int x,int y,int kind,Color color)
    {
        ui.Box(new(x,y,32,32),new Color(31,49,58));ui.Border(new(x,y,32,32),color);
        if(kind%4==0){ui.Hex(new(x+16,y+16),10,color,2);ui.Line(new(x+6,y+16),new(x+26,y+16),color,2);}
        if(kind%4==1){ui.Circle(new(x+16,y+16),10,color,2);ui.Line(new(x+16,y+5),new(x+16,y+27),color,2);}
        if(kind%4==2){ui.Line(new(x+5,y+26),new(x+26,y+5),color,3);ui.Line(new(x+9,y+5),new(x+27,y+23),color,2);}
        if(kind%4==3){ui.Hex(new(x+16,y+16),11,color,2);ui.Circle(new(x+16,y+16),4,color,2);}
    }
    void TinyTab(Rectangle r,string name,Action action,bool active=false) => ui.Button(r,name,action,active:active);
    void DetailedFitting()
    {
        if(fleetBrowser){FleetRegistryWindow();return;}
        if(!fittingWorkshop){FittingLayout();return;}
        if(fittingEwar)EwarFitting();else WeaponFitting();
        ui.Button(new(1080,762,130,24),"FITTING OVERVIEW",()=>{fittingWorkshop=false;fittingEwar=false;},"Return to fitting overview");
    }
    void DetailedMap()
    {
        ui.Button(new(230,731,177,27),"Full galaxy",()=>OpenGalaxy42(),"Open imported galaxy");
        DrawWindow(new(207,84,1033,690),"Cluster map / Syndicate navigation");
        ui.Box(new(218,119,202,600),new Color(14,27,34,235));
        ui.Text("REGIONS",235,137,Cyan,13);ui.Text("Aster Reach",235,171,White,14);ui.Text("Border March",235,199,White,14);
        ui.Text("SELECTED SYSTEM",235,256,Cyan,12);
        var chosen=DemoWorld.Systems[Math.Clamp(mapSelection,0,DemoWorld.Systems.Length-1)];
        ui.Text(chosen.Name,235,281,White,17);ui.Text($"Security {chosen.Security:0.0}",235,307,Gold,12);
        ui.Wrap(chosen.Region+" / "+chosen.Faction,235,338,175,Muted,12);
        ui.Wrap(chosen.Description,235,397,175,White,12);
        ui.Text("CURRENT LOCATION",235,522,Cyan,11);ui.Text(world.Current.Name,235,545,White,14);
        ui.Text("GATE CONNECTION",235,581,Cyan,11);
        ui.Text(world.Current.Links.Contains(chosen.Id)?"Direct gate available":chosen.Id==world.SystemId?"Already in system":"Route requires transfers",235,607,Gold,11);
        ui.Button(new(230,653,177,30),"JUMP / ROUTE",()=>Jump(chosen.Id));ui.Button(new(230,690,85,25),"Info",()=>ShowInformation("system/"+chosen.Id),"System Show Info");ui.Button(new(320,690,87,25),"Region",()=>ShowInformation("region/"+chosen.Region),"Region Show Info");
        ui.Box(new(428,120,797,582),new Color(5,13,21,225));
        for(int x=453;x<1200;x+=52)ui.Line(new(x,133),new(x,683),new Color(26,46,53,55));
        for(int y=140;y<690;y+=52)ui.Line(new(443,y),new(1208,y),new Color(26,46,53,55));
        foreach(var s in DemoWorld.Systems)foreach(var link in s.Links)if(string.CompareOrdinal(s.Id,link)<0)ui.Line(MapPoint(s.Id),MapPoint(link),new Color(68,124,141),2);
        for(int i=0;i<DemoWorld.Systems.Length;i++)
        {
            int ix=i;var s=DemoWorld.Systems[i];var p=MapPoint(s.Id);
            Color color=s.Id==world.SystemId?Gold:s.Security<.4f?Red:Cyan;
            ui.Circle(p,10,color,3);if(mapSelection==i)ui.Circle(p,17,White,1);
            ui.Text(s.Name,p.X+15,p.Y-18,White,13);ui.Text($"{s.Security:0.0} / {s.Region}",p.X+15,p.Y+2,Muted,10);
            ui.Hits.Add(new(new((int)p.X-14,(int)p.Y-18,122,45),()=>mapSelection=ix,"Inspect "+s.Name));
        }
        ui.Text("Connected lines are stargates. Select a node to inspect; travel only along a direct link.",440,731,Muted,12);
    }
    void Catalog()=>DrawEncyclopedia(drawingWindow!);
}
