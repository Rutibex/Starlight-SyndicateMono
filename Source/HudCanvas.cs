using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;

namespace StarlightSyndicateDemo;
public sealed record HitArea(Rectangle Rect, Action Action, string Tip, string Owner = "", string Key = "");
public sealed class HudCanvas : IDisposable
{
    readonly SpriteBatch batch;
    readonly GraphicsDevice device;
    readonly RasterizerState clipped = new() { ScissorTestEnable=true, CullMode=CullMode.None };
    readonly Texture2D pixel, font;
    readonly float[] widths;
    public List<HitArea> Hits = new();
    public Vector2 Mouse;public bool LargeText,HighContrast;public float OpacityJ=1;
    public static readonly Color White = new(215, 224, 228), Muted = new(142, 159, 169), Cyan = new(109, 205, 216), Gold = new(205, 181, 116), Red = new(219, 105, 98), Green = new(123, 192, 145);
    public HudCanvas(GraphicsDevice device)
    {
        this.device=device; batch = new SpriteBatch(device); pixel = new Texture2D(device, 1, 1); pixel.SetData(new[] { Color.White });
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Assets", "UiFont.png"));
        font = Texture2D.FromStream(device, stream);
        widths = JsonSerializer.Deserialize<float[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Assets", "UiFontWidths.json")))!;
    }
    readonly Stack<Rectangle> clipStackJ=new();
    Matrix transformJ=Matrix.Identity;
    public void Begin() { Hits.Clear();clipStackJ.Clear();transformJ=Matrix.Identity; batch.Begin(sortMode:SpriteSortMode.Immediate,blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearClamp,depthStencilState:DepthStencilState.None,rasterizerState:RasterizerState.CullNone); }
    public void Layer(Matrix transform, Rectangle clip)
    {
        batch.End();clipStackJ.Push(clipStackJ.Count>0?Rectangle.Intersect(clip,clipStackJ.Peek()):Rectangle.Intersect(clip,device.Viewport.Bounds));device.ScissorRectangle=clipStackJ.Peek();transformJ=transform;
        batch.Begin(sortMode:SpriteSortMode.Immediate,blendState:BlendState.AlphaBlend,samplerState:SamplerState.LinearClamp,rasterizerState:clipped,depthStencilState:DepthStencilState.None,transformMatrix:transform);
    }
    public void ResetLayer(){batch.End();if(clipStackJ.Count>0)clipStackJ.Pop();transformJ=Matrix.Identity;if(clipStackJ.Count>0)device.ScissorRectangle=clipStackJ.Peek();batch.Begin(sortMode:SpriteSortMode.Immediate,blendState:BlendState.AlphaBlend,samplerState:SamplerState.LinearClamp,depthStencilState:DepthStencilState.None,rasterizerState:clipStackJ.Count>0?clipped:RasterizerState.CullNone);}
    public void End() => batch.End();
    public void Image(Texture2D texture, Rectangle destination) => batch.Draw(texture,destination,Color.White*OpacityJ);
    public void Image(Texture2D texture, Rectangle destination,Color tint) => batch.Draw(texture,destination,tint*OpacityJ);
    public void Box(Rectangle r, Color c) { if(r.Width > 0 && r.Height > 0) batch.Draw(pixel, r, c*OpacityJ); }
    public void Line(Vector2 a, Vector2 b, Color c, float width = 1)
    {
        var d = b - a; if(d.LengthSquared() < .01f) return;
        batch.Draw(pixel, a, null, c, MathF.Atan2(d.Y, d.X), Vector2.Zero, new Vector2(d.Length(), width), SpriteEffects.None, 0);
    }
    public void Border(Rectangle r, Color c)
    {
        Box(new(r.X,r.Y,r.Width,1),c);Box(new(r.X,r.Bottom-1,r.Width,1),c);Box(new(r.X,r.Y,1,r.Height),c);Box(new(r.Right-1,r.Y,1,r.Height),c);
    }
    public void Panel(Rectangle r, string title)
    {
        Box(r, new Color(11, 19, 24, 240)); Border(r, new Color(82, 101, 109, 170));
        Box(new(r.X+1,r.Y+1,r.Width-2,27),new Color(31,43,49,235));
        Text(title,r.X+10,r.Y+6,White,13);
    }
    public float Measure(string s, float size = 14) => s.Sum(c => widths[c >= 32 && c <= 126 ? c-32 : 31]) * size*(LargeText?1.1f:1) / 20;
    public void Text(string s, float x, float y, Color? color = null, float size = 14)
    {
        size*=LargeText?1.1f:1;float origin = x, scale = size / 20;
        if(HighContrast&&color.HasValue&&color.Value==Muted)color=White;
        foreach(char c in s)
        {
            if(c == '\n') { x=origin;y+=size*1.45f;continue; }
            int i = c >= 32 && c <= 126 ? c-32 : 31;
            batch.Draw(font, new Vector2(x,y), new Rectangle(i%16*32,i/16*40,32,40),(color ?? White)*OpacityJ,0,Vector2.Zero,scale,SpriteEffects.None,0);
            x += widths[i]*scale;
        }
    }
    public void Center(string s, float x, float y, Color? c = null, float size = 14) => Text(s,x-Measure(s,size)/2,y,c,size);
    public void Wrap(string s, float x, float y, float width, Color? c = null, float size = 14)
    {
        float left=x;bool first=true;
        foreach(var paragraph in s.Replace("\r","").Split('\n'))
        {
            if(!first){x=left;y+=size*1.5f;}first=false;
            foreach(var word in paragraph.Split(' ',StringSplitOptions.RemoveEmptyEntries))
            {
                if(x>left&&x+Measure(word,size)>left+width){x=left;y+=size*1.5f;}
                Text(word+" ",x,y,c,size);x+=Measure(word+" ",size);
            }
        }
    }
    public void Button(Rectangle r, string label, Action action, string tip = "", bool active = false)
    {
        bool hover=r.Contains(Mouse.ToPoint());
        Box(r,active ? new Color(54,83,83,240) : hover ? new Color(54,67,73,245) : new Color(26,37,43,235));
        Border(r,active ? Cyan : new Color(78,94,101,180));
        Center(label,r.Center.X,r.Center.Y-8,active?Cyan:White,13);
        Hits.Add(new(r,action,tip.Length>0?tip:label));
    }
    public void Arc(Vector2 p, float radius, float start, float sweep, Color color, float width = 2, int segments=70)
    {
        for(int i=0;i<segments;i++)
        {
            float a=start+sweep*i/segments,b=start+sweep*(i+1)/segments;
            Line(p+new Vector2(MathF.Cos(a),MathF.Sin(a))*radius,p+new Vector2(MathF.Cos(b),MathF.Sin(b))*radius,color,width);
        }
    }
    public void Circle(Vector2 p,float radius,Color color,float width=1) => Arc(p,radius,0,MathF.Tau,color,width);
    public void Hex(Vector2 p,float radius,Color color,float width=1)
    {
        for(int i=0;i<6;i++)
        {
            float a=i*MathF.Tau/6,b=(i+1)*MathF.Tau/6;
            Line(p+new Vector2(MathF.Cos(a),MathF.Sin(a))*radius,p+new Vector2(MathF.Cos(b),MathF.Sin(b))*radius,color,width);
        }
    }
    public void Bar(Rectangle r,float fraction,Color color)
    {
        Box(r,new Color(43,53,59));Box(new(r.X,r.Y,(int)(r.Width*Math.Clamp(fraction,0,1)),r.Height),color);
    }
    public void Tooltip(string owner = "")
    {
        var hit=Hits.LastOrDefault(h=>h.Owner==owner&&h.Rect.Contains(Mouse.ToPoint())); if(hit==null) return;
        int width=(int)Math.Min(440,Measure(hit.Tip,13)+24);
        int x=Math.Clamp((int)Mouse.X+14,6,1594-width),y=Math.Clamp((int)Mouse.Y-43,6,850);
        Box(new(x,y,width,34),new Color(4,9,12,247));Border(new(x,y,width,34),Muted);Text(hit.Tip,x+10,y+8,White,13);
    }
    public void Dispose() { batch.Dispose(); pixel.Dispose(); font.Dispose(); clipped.Dispose(); }
}
