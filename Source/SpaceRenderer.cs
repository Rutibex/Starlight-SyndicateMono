using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightSyndicateDemo;

// Native MonoGame 3D renderer. Meshes, camera and picking all use world-space Vector3 positions.
public sealed partial class SpaceRenderer : IDisposable
{
    readonly GraphicsDevice device;
    readonly BasicEffect effect;
    readonly VertexPositionNormalTexture[] hull, sphere, cube, rock;
    readonly Dictionary<string,VertexPositionNormalTexture[]> fleetMeshes=[];
    readonly Dictionary<string,ShipRenderModel[]> shipModels;
    readonly RasterizerState previewWire=new(){CullMode=CullMode.None,FillMode=FillMode.WireFrame};
    readonly Dictionary<string,(VertexPositionNormalTexture[] Primary,VertexPositionNormalTexture[] Accent)> paintMeshesI=[];CosmeticI? activePaintI;Ship? activePaintShipI;public bool PreviewWireframe; public int PreviewLod;
    public string PreviewSummary(string id)=>shipModels.TryGetValue(id,out var m)?$"LOD {PreviewLod} / {m[PreviewLod].Triangles} triangles / {m[PreviewLod].Parts.Length} materials / +Z nose": "Legacy model fallback / dedicated model unavailable";
    readonly Texture2D sky, planet;
    readonly Random random = new(753);
    readonly (Vector3 position, float scale, float spin)[] stones;
    public Matrix View, Projection;
    public Vector3 CameraPosition;
    public float Yaw = -.9f, Pitch = .25f, Distance = 210, DesiredDistance = 210;
    public bool LookAtTarget, Tactical;
    Vector3 forward = Vector3.Forward;
    public SpaceRenderer(GraphicsDevice graphics)
    {
        device = graphics;
        effect = new BasicEffect(device);
        hull = ReadGltf(Path.Combine(AppContext.BaseDirectory, "Assets", "ConcordHull.gltf"));
        shipModels=ShipModelAssets.LoadAll(Path.Combine(AppContext.BaseDirectory,"Assets"));
        foreach(var h in FleetCatalog.Playable){if(shipModels.TryGetValue(h.Id,out var models))fleetMeshes[h.Id]=models[0].Flat;else{string path=Path.Combine(AppContext.BaseDirectory,"Assets","FleetModels",h.Id+".gltf");try{fleetMeshes[h.Id]=ReadGltf(path);}catch(Exception ex){Console.Error.WriteLine("MODEL WARNING: "+h.Name+" legacy fallback: "+ex.Message);fleetMeshes[h.Id]=hull;}}}
        sphere = Sphere(40, 64); rock = Sphere(9, 13, true); cube = Cube();
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", "SpaceSky.png"));
        sky = Texture2D.FromStream(device, stream);
        planet = PlanetTexture();
        stones = Enumerable.Range(0, 60).Select(i => (new Vector3(random.Next(-3200, 3900), random.Next(-900, 900), random.Next(-3600, 3400)), (float)random.Next(35, 175), (float)random.NextDouble() * 6)).ToArray();
    }
    public bool HasPreview(string id) => id.Length==0 || shipModels.ContainsKey(id) || fleetMeshes.ContainsKey(id) || FleetCatalog.ById.TryGetValue(id,out var h)&&(shipModels.ContainsKey(h.BaseHullId)||fleetMeshes.ContainsKey(h.BaseHullId));
    public void DrawPreview(string id,float yaw,float pitch,float zoom,Ship? configuration=null)
    {
        var h=FleetCatalog.ById.GetValueOrDefault(id);
        var mesh=fleetMeshes.GetValueOrDefault(id)??fleetMeshes.GetValueOrDefault(h?.BaseHullId??"")??hull;
        var previewLods=shipModels.GetValueOrDefault(id)??shipModels.GetValueOrDefault(h?.BaseHullId??"");
        var min=previewLods?[0].Bounds.Min??mesh.Select(v=>v.Position).Aggregate(Vector3.Min);var max=previewLods?[0].Bounds.Max??mesh.Select(v=>v.Position).Aggregate(Vector3.Max);
        var center=(min+max)/2;float radius=Math.Max(1,(max-min).Length()/2);
        device.DepthStencilState=DepthStencilState.Default;device.RasterizerState=PreviewWireframe?previewWire:RasterizerState.CullNone;device.BlendState=BlendState.Opaque;
        effect.View=Matrix.CreateLookAt(new Vector3(0,0,radius*3.1f*zoom),Vector3.Zero,Vector3.Up);
        effect.Projection=Matrix.CreatePerspectiveFieldOfView(.65f,400f/420f,.1f,radius*20);
        effect.TextureEnabled=false;effect.EnableDefaultLighting();effect.PreferPerPixelLighting=true;effect.AmbientLightColor=new Vector3(.2f);effect.DirectionalLight0.DiffuseColor=new Vector3(.7f);effect.DirectionalLight1.DiffuseColor=new Vector3(.18f);effect.DirectionalLight2.DiffuseColor=new Vector3(.12f);effect.SpecularColor=new Vector3(.16f);effect.SpecularPower=24;
        var color=h?.SourceHull switch{"Tristan"=>new Color(128,170,158),"Kestrel"=>new Color(116,149,184),"Punisher"=>new Color(203,174,100),"Rifter"=>new Color(160,119,90),_=>new Color(160,180,190)};
        var matrix=Matrix.CreateTranslation(-center)*Matrix.CreateRotationY(yaw)*Matrix.CreateRotationX(pitch);
        if(previewLods!=null)DrawShipModel(previewLods[Math.Clamp(PreviewLod,0,2)],matrix);else Mesh(mesh,matrix,color);
        if(configuration?.EliteBattleship==true)DrawEliteDeployment(configuration,matrix,PreviewLod);
        if(ShipBatch21.ById.TryGetValue(id,out var profile)&&profile.Role=="StrategicCruiser"){configuration??=new Ship();if(configuration.CombatHull!=id)configuration.ApplyFleetHull(FleetCatalog.ById[id]);DrawSubsystemGeometry(configuration,matrix,PreviewLod);}
    }
    public void Reset() { Yaw = -.9f; Pitch = .25f; Distance = DesiredDistance = 210; LookAtTarget = false; forward = Vector3.Forward; }
    public void Orbit(float x, float y) { Yaw -= x * .006f; Pitch = Math.Clamp(Pitch + y * .006f, -1.48f, 1.48f); }
    public void Zoom(int delta) => DesiredDistance = Math.Clamp(DesiredDistance * MathF.Pow(.84f, delta / 120f), 70, 22000);
    public void Update(DemoWorld world, float dt)
    {
        if(world.Docked)DesiredDistance=Math.Clamp(DesiredDistance,110,410);
        Distance = MathHelper.Lerp(Distance, DesiredDistance, 1 - MathF.Exp(-dt * 12));
        var center = LookAtTarget && world.Selected != null ? world.Selected.Position : world.Player.Position;
        if (world.Screen == ViewMode.Title) center = world.Player.Position;
        var offset = new Vector3(MathF.Sin(Yaw) * MathF.Cos(Pitch), MathF.Sin(Pitch), MathF.Cos(Yaw) * MathF.Cos(Pitch));
        CameraPosition = center + offset * Distance;
        View = Matrix.CreateLookAt(CameraPosition, center, Vector3.Up);
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(58), 1600f / 900f, 1f, 12000000f);
        if (world.Player.Velocity.LengthSquared() > 10)
        {
            var next = Vector3.Lerp(forward, Vector3.Normalize(world.Player.Velocity), 1 - MathF.Exp(-dt * 2));
            if (next.LengthSquared() > .01f) forward = Vector3.Normalize(next);
        }
    }
    public Vector3 Project(Vector3 p) => new Viewport(0, 0, 1600, 900).Project(p, Projection, View, Matrix.Identity);
    public Ray PickRay(Vector2 p)
    {
        var vp = new Viewport(0, 0, 1600, 900);
        var near = vp.Unproject(new Vector3(p, 0), Projection, View, Matrix.Identity);
        var far = vp.Unproject(new Vector3(p, 1), Projection, View, Matrix.Identity);
        return new Ray(near, Vector3.Normalize(far - near));
    }
    public void Draw(DemoWorld world, float time, bool fitting = false)
    {
        activePaintI=world.ActiveCosmeticI;activePaintShipI=world.Player;
        device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;
        device.BlendState = BlendState.Opaque;
        effect.View = View; effect.Projection = Projection;
        effect.TextureEnabled = true; effect.Texture = sky; effect.LightingEnabled = false;
        effect.DiffuseColor = new Vector3(.48f, .56f, .68f); effect.EmissiveColor = Vector3.Zero;
        effect.World = Matrix.CreateScale(90000) * Matrix.CreateRotationY(.4f) * Matrix.CreateTranslation(CameraPosition);
        DrawMesh(sphere);
        effect.TextureEnabled = false;
        device.DepthStencilState = DepthStencilState.Default;
        effect.EnableDefaultLighting(); effect.PreferPerPixelLighting = true;
        effect.AmbientLightColor = new Vector3(.22f, .27f, .34f);
        effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -.35f, -.6f));
        effect.DirectionalLight0.DiffuseColor = new Vector3(.95f, .88f, .74f);
        effect.DirectionalLight1.DiffuseColor = new Vector3(.14f, .34f, .46f);
        effect.SpecularColor = new Vector3(.4f); effect.SpecularPower = 32;
        if(world.Docked) HangarInterior(world.Player.Position);
        if (!fitting)
        {
            DrawSolarBackdrop(world);
            if (world.Grid == "belt") foreach (var stone in stones)
                Mesh(rock, Matrix.CreateScale(stone.scale) * Matrix.CreateRotationY(stone.spin + time * .005f) * Matrix.CreateTranslation(stone.position + new Vector3(-1000, 0, -1400)), new Color(91, 86, 77));
            foreach (var target in world.Targets.Where(t => t.Alive&&!world.InWarp).OrderBy(t=>t.Id==world.SelectedId?0:1).ThenBy(t=>Vector3.DistanceSquared(t.Position,world.Player.Position)).Take(world.Experience.Settings.RenderBudget))
            {
                switch (target.Kind)
                {
                    case TargetKind.Asteroid:
                        Mesh(rock, Matrix.CreateScale(250, 180, 210) * Matrix.CreateRotationY(.8f) * Matrix.CreateTranslation(target.Position), new Color(142, 129, 98)); break;
                    case TargetKind.Wreck:Mesh(rock,Matrix.CreateScale(50,25,70)*Matrix.CreateTranslation(target.Position),new Color(60,65,68));break;
                    case TargetKind.Raider:
                        if(target.DroneId.Length>0){var u=world.Player.Drones.FirstOrDefault(u=>u.Id==target.DroneId);for(int i=0;i<Math.Min(12,u?.Count??1);i++)Ship(target.Position+new Vector3((i%3)*55,(i/3)*30,(i%2)*40),target.Velocity.LengthSquared()>1?Vector3.Normalize(target.Velocity):Vector3.Forward,target.Friendly?new Color(90,170,160):new Color(160,85,70),u?.Definition.Fighter==true?1.8f:.7f,time);break;}
                        Ship(target.Position, target.Velocity.LengthSquared() > 1 ? Vector3.Normalize(target.Velocity) : Vector3.Forward, target.Friendly?new Color(78,155,160):new Color(136, 78, 66), 4.5f, time); break;
                    case TargetKind.Container:
                        Mesh(cube, Matrix.CreateScale(32, 21, 32) * Matrix.CreateRotationY(time * .05f) * Matrix.CreateTranslation(target.Position), new Color(95, 176, 169)); break;
                    case TargetKind.Stargate: DrawSolarGate(target.Position,time); break;
                    case TargetKind.Wormhole: DrawSolarGate(target.Position,time,true); break;
                    case TargetKind.Celestial: Mesh(cube,Matrix.CreateScale(25)*Matrix.CreateTranslation(target.Position),Color.Cyan);break;
                    case TargetKind.Station: Station(target.Position); break;
                }
            }
        }
        if(!world.Cloaked)Ship(world.Player.Position, forward, new Color(177, 189, 196), 6, time,world.Player.CombatHull,world.Player);
        var lines = new List<VertexPositionColor>();
        if (Tactical)
        {
            for (int r = 1000; r <= 10000; r += 1000) for (int i = 0; i < 100; i++)
            {
                float a = i * MathF.Tau / 100, b = (i + 1) * MathF.Tau / 100;
                Add(lines, world.Player.Position + new Vector3(MathF.Cos(a) * r, -80, MathF.Sin(a) * r), world.Player.Position + new Vector3(MathF.Cos(b) * r, -80, MathF.Sin(b) * r), new Color(65, 118, 139, 100));
            }
        }
        if (world.Locked is {} targetLocked)
        {
            if (world.Player.MinerOn && Vector3.Distance(world.Player.Position, targetLocked.Position) < 6500)
            {
                Add(lines, world.Player.Position + new Vector3(15, 0, 0), targetLocked.Position, new Color(85, 232, 186));
                Add(lines, world.Player.Position - new Vector3(15, 0, 0), targetLocked.Position, new Color(202, 244, 213));
            }
        }
        foreach(var zone in world.EwarZones)
        {for(int i=0;i<80;i++){float a=i*MathF.Tau/80,b=(i+1)*MathF.Tau/80;Add(lines,zone.Center+new Vector3(MathF.Cos(a)*zone.Radius,0,MathF.Sin(a)*zone.Radius),zone.Center+new Vector3(MathF.Cos(b)*zone.Radius,0,MathF.Sin(b)*zone.Radius),Color.Violet);Add(lines,zone.Center+new Vector3(0,MathF.Cos(a)*zone.Radius,MathF.Sin(a)*zone.Radius),zone.Center+new Vector3(0,MathF.Cos(b)*zone.Radius,MathF.Sin(b)*zone.Radius),Color.Violet);}}
        foreach(var s in world.Player.Ewar.Where(e=>e.WarmupLeft>0&&e.Definition!=null))
        {var c=new Vector3(s.AimX,s.AimY,s.AimZ);for(int i=0;i<40;i++){float a=i*MathF.Tau/40,b=(i+1)*MathF.Tau/40;Add(lines,c+new Vector3(MathF.Cos(a)*s.Definition!.Radius,0,MathF.Sin(a)*s.Definition.Radius),c+new Vector3(MathF.Cos(b)*s.Definition.Radius,0,MathF.Sin(b)*s.Definition.Radius),Color.OrangeRed);}}
        foreach(var v in world.WeaponVisuals)
        {
            Color tint=v.DamageType switch {DamageType.EM=>Color.Cyan,DamageType.Thermal=>Color.OrangeRed,DamageType.Kinetic=>Color.LightBlue,_=>Color.Gold};
            if(v.Radius>0){for(int i=0;i<80;i++){float a=i*MathF.Tau/80,b=(i+1)*MathF.Tau/80;float radius=v.Radius*(1-v.Remaining);Add(lines,v.Start+new Vector3(MathF.Cos(a)*radius,0,MathF.Sin(a)*radius),v.Start+new Vector3(MathF.Cos(b)*radius,0,MathF.Sin(b)*radius),tint);}}
            else {Add(lines,v.Start,v.End,tint);if(v.Kind==WeaponKind.Arc){var mid=Vector3.Lerp(v.Start,v.End,.5f)+new Vector3(0,160,80);Add(lines,v.Start,mid,tint);Add(lines,mid,v.End,tint);}}
        }
        foreach (var m in world.MissilesInFlight)
        {
            Mesh(sphere, Matrix.CreateScale(5) * Matrix.CreateTranslation(m.Position), m.Bomb?Color.Red:m.Interceptor?Color.LightGreen:Color.Orange, true);
            Add(lines, m.Position, m.Position - forward * 85, Color.Orange);
        }
        Lines(lines);
    }
    void HangarInterior(Vector3 at)
    {
        Mesh(cube,Matrix.CreateScale(520,8,640)*Matrix.CreateTranslation(at+new Vector3(0,-68,0)),new Color(34,47,58));
        Mesh(cube,Matrix.CreateScale(520,10,640)*Matrix.CreateTranslation(at+new Vector3(0,310,0)),new Color(20,29,38));
        Mesh(cube,Matrix.CreateScale(12,200,640)*Matrix.CreateTranslation(at+new Vector3(-520,110,0)),new Color(34,45,53));
        Mesh(cube,Matrix.CreateScale(12,200,640)*Matrix.CreateTranslation(at+new Vector3(520,110,0)),new Color(34,45,53));
        Mesh(cube,Matrix.CreateScale(520,200,12)*Matrix.CreateTranslation(at+new Vector3(0,110,-640)),new Color(23,33,44));
        Mesh(cube,Matrix.CreateScale(520,200,12)*Matrix.CreateTranslation(at+new Vector3(0,110,640)),new Color(23,33,44));
        for(int i=-4;i<=4;i++)
        {
            Mesh(cube,Matrix.CreateScale(4,2,600)*Matrix.CreateTranslation(at+new Vector3(i*105,-57,0)),new Color(34,70,82),true);
            Mesh(cube,Matrix.CreateScale(495,2,3)*Matrix.CreateTranslation(at+new Vector3(0,-57,i*125)),new Color(34,70,82),true);
            foreach(int side in new[]{-1,1})
            {
                Mesh(cube,Matrix.CreateScale(20,180,16)*Matrix.CreateTranslation(at+new Vector3(side*490,110,i*140)),new Color(64,77,84));
                Mesh(cube,Matrix.CreateScale(4,92,7)*Matrix.CreateTranslation(at+new Vector3(side*467,140,i*140)),new Color(51,124,141),true);
            }
        }
        Mesh(cube,Matrix.CreateScale(110,7,155)*Matrix.CreateTranslation(at+new Vector3(0,-54,0)),new Color(72,82,86));
        for(int i=-1;i<=1;i+=2)Mesh(cube,Matrix.CreateScale(4,2,145)*Matrix.CreateTranslation(at+new Vector3(i*112,-45,0)),new Color(135,103,40),true);
    }
    void Station(Vector3 at)
    {
        Mesh(cube, Matrix.CreateScale(95, 850, 95) * Matrix.CreateTranslation(at), new Color(76, 85, 92));
        for (int i = 0; i < 16; i++)
        {
            float a = i * MathF.Tau / 16;
            var pos = at + new Vector3(MathF.Cos(a) * 600, 0, MathF.Sin(a) * 600);
            Mesh(cube, Matrix.CreateScale(115, 95, 80) * Matrix.CreateRotationY(-a) * Matrix.CreateTranslation(pos), new Color(122, 131, 134));
            Mesh(cube, Matrix.CreateScale(26, 18, 16) * Matrix.CreateTranslation(pos + new Vector3(0, 100, 0)), new Color(91, 198, 212), true);
        }
        Mesh(cube, Matrix.CreateScale(680, 25, 22) * Matrix.CreateTranslation(at), new Color(102, 113, 120));
        Mesh(cube, Matrix.CreateScale(22, 25, 680) * Matrix.CreateTranslation(at), new Color(102, 113, 120));
    }
    void Ship(Vector3 position, Vector3 heading, Color color, float scale, float time,string hullId="",Ship? configuration=null)
    {
        var up = MathF.Abs(heading.Y) > .98f ? Vector3.UnitZ : Vector3.Up;
        var basis = Matrix.CreateWorld(position, -heading, up);
        if(shipModels.TryGetValue(hullId,out var models)){float distance=Vector3.Distance(CameraPosition,position);int lod=ShipModelAssets.SelectLod(distance);DrawShipModel(models[lod],Matrix.CreateScale(scale)*basis,configuration==activePaintShipI?activePaintI:null);if(configuration?.StrategicCruiser==true)DrawSubsystemGeometry(configuration,Matrix.CreateScale(scale)*basis,lod);if(configuration?.EliteBattleship==true)DrawEliteDeployment(configuration,Matrix.CreateScale(scale)*basis,lod);return;}
        var selected=fleetMeshes.GetValueOrDefault(hullId)??hull;
        if(FleetCatalog.ById.TryGetValue(hullId,out var fh))color=fh.SourceHull switch{"Tristan"=>new Color(128,170,158),"Kestrel"=>new Color(116,149,184),"Punisher"=>new Color(203,174,100),_=>new Color(160,119,90)};
        if(configuration==activePaintShipI&&activePaintI!=null){var painted=PaintMeshI(hullId,selected,activePaintI.Pattern);Mesh(painted.Primary,Matrix.CreateScale(scale)*basis,ExperienceCatalogI.Paints[activePaintI.Primary]);if(painted.Accent.Length>0)Mesh(painted.Accent,Matrix.CreateScale(scale)*basis,ExperienceCatalogI.Paints[activePaintI.Accent]);}else Mesh(selected, Matrix.CreateScale(scale) * basis, color);
        // Reuse the supplied hull; engine bells and running lights are small additions.
        for (int side = -1; side <= 1; side += 2)
        {
            Mesh(cube, Matrix.CreateScale(3.6f, 3, 8) * Matrix.CreateTranslation(side * 12, -2, -14) * basis, new Color(52, 64, 76));
            Mesh(sphere, Matrix.CreateScale(2.8f, 2.2f, 7 + MathF.Sin(time * 18) * 1.3f) * Matrix.CreateTranslation(side * 12, -2, -22) * basis, new Color(78, 197, 250), true);
            Mesh(sphere, Matrix.CreateScale(1.4f) * Matrix.CreateTranslation(side * 28, 1, -1) * basis, side < 0 ? Color.IndianRed : Color.LightGreen, true);
        }
        Mesh(cube, Matrix.CreateScale(2.2f, .8f, 5) * Matrix.CreateTranslation(0, 10, 6) * basis, new Color(70, 152, 174), true);
    }
    (VertexPositionNormalTexture[] Primary,VertexPositionNormalTexture[] Accent) PaintMeshI(string hullId,VertexPositionNormalTexture[] vertices,string pattern)
    {
        string key=hullId+"/"+pattern;if(paintMeshesI.TryGetValue(key,out var cached))return cached;
        var primary=new List<VertexPositionNormalTexture>();var accent=new List<VertexPositionNormalTexture>();
        for(int i=0;i+2<vertices.Length;i+=3){int section=i/3;bool useAccent=pattern switch{"Alternating"=>section%2==1,"Bands"=>section%3==0,_=>section%4==1};var list=useAccent?accent:primary;list.Add(vertices[i]);list.Add(vertices[i+1]);list.Add(vertices[i+2]);}
        if(paintMeshesI.Count>=12)paintMeshesI.Remove(paintMeshesI.Keys.First());var result=(primary.ToArray(),accent.ToArray());paintMeshesI[key]=result;return result;
    }
    // Four connected authored sections, each driven by its actual installed subsystem.
    void DrawEliteDeployment(Ship ship,Matrix basis,int lod)
    {
        if(lod==2)return;
        void Box(Vector3 pos,Vector3 size,Color color,bool glow=false)=>Mesh(cube,Matrix.CreateScale(size)*Matrix.CreateTranslation(pos)*basis,color,glow);
        if(ship.HasSpecial("Bastion")){
            var state=ship.Special.First(s=>s.Active&&s.Definition?.Kind=="Bastion");float open=Math.Clamp(state.ActiveSeconds/1.2f,0,1);
            foreach(int sign in new[]{-1,1}){Box(new(sign*(2+open*1.7f),.7f,.8f),new(.7f,.2f,2.4f),new(122,155,163));Box(new(sign*(2.2f+open*1.7f),.95f,.8f),new(.2f,.1f,2),new(77,221,233),true);Box(new(sign*2.6f,-.5f,-2),new(.2f,.2f,.5f),new(245,177,82),true);}
        }else if(ship.HasSpecial("CovertPortal")){
            foreach(int sign in new[]{-1,1}){Box(new(sign*2.2f,1,-1.2f),new(.2f,.1f,2),new(162,96,241),true);Box(new(sign*3,1,-1.2f),new(.15f,1.2f,.2f),new(97,210,237),true);}Box(new(0,2.2f,-1.2f),new(3,.15f,.2f),new(139,91,225),true);
        }
    }
    void DrawSubsystemGeometry(Ship ship,Matrix matrix,int lod)
    {
        Color hullColor=FleetCatalog.ById[ship.CombatHull].Faction switch{"Boreal Federation"=>new(100,155,133),"Amur Cooperative Compact"=>new(90,130,166),"Suryavarta Empire"=>new(195,160,90),_=>new(157,101,66)};
        void Box(Vector3 at,Vector3 size,Color tint,bool glow=false)=>Mesh(FleetCatalog.ById[ship.CombatHull].Faction is "Boreal Federation" or "Suryavarta Empire"?subsystemPrism:cube,Matrix.CreateScale(size*.65f)*Matrix.CreateTranslation(at)*matrix,tint,glow);
        foreach(var sub in ship.InstalledSubsystems){int variant=Array.IndexOf(ShipBatch28.For(ship.CombatHull,sub.Category),sub);float z=sub.Category switch{"Core"=>-1.2f,"Defensive"=>1.1f,"Offensive"=>4.3f,_=>-4.2f};
            if(sub.Category=="Core"){
                Box(new(0,.8f,z),new(1.4f+variant*.35f,.5f,1.5f),hullColor);
                for(int i=0;i<=variant;i++)Box(new((i-variant*.5f)*.7f,1.4f,z),new(.18f,.55f,.65f),new(85,185,198));
            }else if(sub.Category=="Defensive"){
                foreach(int sign in new[]{-1,1}){Box(new(sign*1.65f,.4f,z),new(.7f+variant*.22f,.55f+variant*.18f,1.8f),hullColor);if(lod<2)Box(new(sign*2.3f,.65f,z),new(.16f,.5f,sub.Name.Contains("Covert")?2.5f:1),new(43,66,77));}
            }else if(sub.Category=="Offensive"){
                foreach(int sign in new[]{-1,1}){Box(new(sign*.9f,.4f,z),new(.5f,.45f,1.3f),hullColor);Box(new(sign*.9f,.8f,z+.9f),new(variant==1?.4f:.18f,.2f,variant==2?.5f:1.2f),new(60,87,101));}
            }else{
                foreach(int sign in new[]{-1,1}){Box(new(sign*(1.1f+variant*.45f),0,z),new(.55f,.6f,1.4f),hullColor);Box(new(sign*(1.1f+variant*.45f),0,z-1.45f),new(.35f,.35f,.2f),new(75,189,228),true);}
                Box(new(0,0,z),new(1.4f+variant*.4f,.2f,.3f),new(52,76,88));
            }
        }
    }
    static readonly VertexPositionNormalTexture[] subsystemPrism=SubsystemPrism();
    static VertexPositionNormalTexture[] SubsystemPrism()
    {
        var result=new List<VertexPositionNormalTexture>();
        void Tri(Vector3 a,Vector3 b,Vector3 c){var n=Vector3.Normalize(Vector3.Cross(b-a,c-a));foreach(var v in new[]{a,b,c})result.Add(new(v,n,Vector2.Zero));}
        for(int i=0;i<8;i++){float a=i*MathF.Tau/8,b=(i+1)*MathF.Tau/8;var p=new Vector3(MathF.Cos(a),MathF.Sin(a),-1);var q=new Vector3(MathF.Cos(b),MathF.Sin(b),-1);var u=new Vector3(MathF.Cos(a)*.65f,MathF.Sin(a)*.65f,1);var v=new Vector3(MathF.Cos(b)*.65f,MathF.Sin(b)*.65f,1);Tri(p,q,u);Tri(q,v,u);Tri(new(0,0,-1),q,p);Tri(new(0,0,1),u,v);}
        return result.ToArray();
    }
    void DrawShipModel(ShipRenderModel model,Matrix matrix,CosmeticI? cosmetic=null)
    {
        effect.World=matrix;effect.TextureEnabled=false;effect.LightingEnabled=true;
        int partIndex=0;foreach(var part in model.Parts){int index=partIndex++;bool accent=cosmetic?.Pattern switch{"Alternating"=>index%2==1,"Bands"=>index%3==0,_=>index%4==1};effect.DiffuseColor=cosmetic==null?part.Color:ExperienceCatalogI.Paints[accent?cosmetic.Accent:cosmetic.Primary].ToVector3();effect.EmissiveColor=part.Emission;effect.SpecularPower=8+(1-part.Roughness)*56;foreach(var pass in effect.CurrentTechnique.Passes){pass.Apply();device.DrawUserPrimitives(PrimitiveType.TriangleList,part.Vertices,0,part.Vertices.Length/3);}}
        effect.EmissiveColor=Vector3.Zero;effect.SpecularPower=32;
    }
    void Mesh(VertexPositionNormalTexture[] mesh, Matrix matrix, Color color, bool glow = false)
    {
        effect.World = matrix; effect.DiffuseColor = color.ToVector3();
        effect.EmissiveColor = glow ? color.ToVector3() * .8f : Vector3.Zero;
        DrawMesh(mesh);
    }
    void DrawMesh(VertexPositionNormalTexture[] mesh)
    {
        foreach (var pass in effect.CurrentTechnique.Passes) { pass.Apply(); device.DrawUserPrimitives(PrimitiveType.TriangleList, mesh, 0, mesh.Length / 3); }
    }
    static void Add(List<VertexPositionColor> list, Vector3 a, Vector3 b, Color c) { list.Add(new(a, c)); list.Add(new(b, c)); }
    void Lines(List<VertexPositionColor> list)
    {
        if (list.Count == 0) return;
        effect.LightingEnabled = false; effect.VertexColorEnabled = true; effect.DiffuseColor = Vector3.One; effect.EmissiveColor = Vector3.Zero; effect.World = Matrix.Identity;
        device.BlendState = BlendState.AlphaBlend;
        var array = list.ToArray();
        foreach (var pass in effect.CurrentTechnique.Passes) { pass.Apply(); device.DrawUserPrimitives(PrimitiveType.LineList, array, 0, array.Length / 2); }
        effect.VertexColorEnabled = false;
    }
    static VertexPositionNormalTexture[] Cube()
    {
        var output = new List<VertexPositionNormalTexture>();
        Vector3[] normals = [Vector3.Up, Vector3.Down, Vector3.Right, Vector3.Left, Vector3.Forward, Vector3.Backward];
        foreach (var n in normals)
        {
            var u = MathF.Abs(n.Y) > .5f ? Vector3.Right : Vector3.Up;
            var v = Vector3.Cross(n, u);
            Vector3[] p = [n - u - v, n + u - v, n + u + v, n - u + v];
            foreach (int i in new[] { 0, 1, 2, 0, 2, 3 }) output.Add(new(p[i], n, Vector2.Zero));
        }
        return output.ToArray();
    }
    static VertexPositionNormalTexture[] Sphere(int rows, int columns, bool rough = false)
    {
        var list = new List<VertexPositionNormalTexture>();
        VertexPositionNormalTexture V(int y, int x)
        {
            float u = x / (float)columns, v = y / (float)rows;
            float a = u * MathF.Tau, b = v * MathF.PI;
            var n = new Vector3(MathF.Sin(b) * MathF.Cos(a), MathF.Cos(b), MathF.Sin(b) * MathF.Sin(a));
            float radius = rough ? 1 + .17f * MathF.Sin(a * 5 + b * 4) + .1f * MathF.Cos(a * 9 - b * 3) : 1;
            return new(n * radius, n, new Vector2(u, v));
        }
        for (int y = 0; y < rows; y++) for (int x = 0; x < columns; x++)
        { list.Add(V(y, x)); list.Add(V(y + 1, x)); list.Add(V(y + 1, x + 1)); list.Add(V(y, x)); list.Add(V(y + 1, x + 1)); list.Add(V(y, x + 1)); }
        return list.ToArray();
    }
    static VertexPositionNormalTexture[] ReadGltf(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path)); var root = doc.RootElement;
        var uri = root.GetProperty("buffers")[0].GetProperty("uri").GetString()!;
        var data = Convert.FromBase64String(uri[(uri.IndexOf(',') + 1)..]);
        var primitive = root.GetProperty("meshes")[0].GetProperty("primitives")[0];
        (int offset, int stride, int count, int type) Access(int index, int components)
        {
            var a = root.GetProperty("accessors")[index]; var v = root.GetProperty("bufferViews")[a.GetProperty("bufferView").GetInt32()];
            int type = a.GetProperty("componentType").GetInt32();
            int offset = (v.TryGetProperty("byteOffset", out var vo) ? vo.GetInt32() : 0) + (a.TryGetProperty("byteOffset", out var ao) ? ao.GetInt32() : 0);
            return (offset, v.TryGetProperty("byteStride", out var stride) ? stride.GetInt32() : components * (type == 5123 ? 2 : 4), a.GetProperty("count").GetInt32(), type);
        }
        var attrs = primitive.GetProperty("attributes"); var p = Access(attrs.GetProperty("POSITION").GetInt32(), 3); var n = Access(attrs.GetProperty("NORMAL").GetInt32(), 3);
        var indices = Access(primitive.GetProperty("indices").GetInt32(), 1);
        Vector3 Read((int offset, int stride, int count, int type) a, int i) => new(BitConverter.ToSingle(data, a.offset + i * a.stride), BitConverter.ToSingle(data, a.offset + i * a.stride + 4), BitConverter.ToSingle(data, a.offset + i * a.stride + 8));
        var vertices = new VertexPositionNormalTexture[indices.count];
        for (int i = 0; i < indices.count; i++)
        {
            int at = indices.offset + i * indices.stride;
            int index = indices.type == 5123 ? BitConverter.ToUInt16(data, at) : (int)BitConverter.ToUInt32(data, at);
            vertices[i] = new(Read(p, index), Read(n, index), Vector2.Zero);
        }
        return vertices;
    }
    Texture2D PlanetTexture()
    {
        const int width = 512, height = 256;
        var pixels = new Color[width*height];
        for(int y=0;y<height;y++) for(int x=0;x<width;x++)
        {
            float u=x/(float)width*MathF.Tau,v=y/(float)height*MathF.PI;
            float a=MathF.Sin(u*3+MathF.Sin(v*5)*2)+.5f*MathF.Sin(u*7-v*9)+.25f*MathF.Cos(u*17+v*23);
            float cloud=MathF.Sin(u*5+v*13+MathF.Sin(u*3-v*7)*2)+.45f*MathF.Cos(u*13-v*21);
            Color baseColor=a>.55f ? new Color(75,96,77) : a>.15f ? new Color(55,83,91) : new Color(27,63,91);
            float polar=MathF.Pow(MathF.Abs(MathF.Cos(v)),18);
            float cover=Math.Clamp((cloud-.65f)*.7f+polar,0,.88f);
            pixels[y*width+x]=Color.Lerp(baseColor,new Color(190,206,210),cover);
        }
        var texture=new Texture2D(device,width,height);texture.SetData(pixels);return texture;
    }
    public void Dispose() { previewWire.Dispose();effect.Dispose(); sky.Dispose(); planet.Dispose(); }
}
