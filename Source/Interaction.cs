using Microsoft.Xna.Framework;

namespace StarlightSyndicateDemo;

public sealed class ProbePuzzle
{
    public Vector2 Center = Vector2.Zero;
    public Vector2 Estimate = Vector2.Zero;
    public float Range = 16000;
    public int Signal;
    public int Passes;
    public readonly Vector2 Secret = new(3500, -1700);
    public string Report = "Umbra Knot: an unresolved artificial signal. Position eight probes and analyze.";

    public void Reset()
    {
        Center = Vector2.Zero; Estimate = Vector2.Zero; Range = 16000; Signal = Passes = 0;
        Report = "Position probes, reduce scan range, then analyze.";
    }

    public void AdjustRange(bool narrower)
    {
        Range = Math.Clamp(narrower ? Range / 2 : Range * 2, 1000, 16000);
    }

    public void Analyze(DemoWorld world)
    {
        world.AdvanceHours(.5*world.Player.Stat("probeScanTime")/8);
        Passes++;
        if (world.SystemId != "umbra")
        {
            Signal = 0; Report = "No matching JoyOS signature here. CODEX traced the echo toward Umbra Knot.";
            world.Say(Report); return;
        }
        float distance = Vector2.Distance(Center, Secret);
        if (distance > Range * .95f) { Signal = 0; Report = "Signal lost. Widen the probes or move their center."; world.Say(Report); return; }
        Signal = Math.Clamp((int)((world.Player.Stat("probeStrength")/28)*(24 + 10 * MathF.Log2(16000 / Range) + (1 - distance / Range) * 38)), 1, 100);
        if (Range <= 2000 && distance < 1100) Signal = 100;
        float noise = MathF.Max(120, Range * .25f) * (Signal < 100 ? (float)world.Player.Stat("probeDeviation") : 0);
        Estimate = Secret + new Vector2(MathF.Sin(Passes * 2.1f), MathF.Cos(Passes * 1.7f)) * noise;
        if (Signal == 100)
        {
            world.SignalResolved = true;
            Report = "100%: JoyOS memory vault resolved. Press W to warp to the site.";
        }
        else Report = $"{Signal}%: position estimate updated. Click the contact and narrow the probe range.";
        world.Say(Report);
    }
}

public sealed class HackPuzzle
{
    public const int Columns = 6, Rows = 5;
    public readonly int[,] Nodes = new int[Columns, Rows];
    public readonly bool[,] Revealed = new bool[Columns, Rows];
    public Point Position;
    public int Coherence;public int MaxCoherence=75;
    public bool Finished, Won;
    public string Message = "Trace a path to the hidden system core. Click adjacent hex-like nodes.";

    public void Reset(Ship? ship=null)
    {
        MaxCoherence=ship==null?75:(int)ship.Stat("virusCoherence");
        Array.Clear(Nodes); Array.Clear(Revealed);
        // 0 empty, 1 defense, 2 repair, 3 core. A winnable path always exists.
        Nodes[1, 1] = 1; Nodes[2, 0] = 1; Nodes[2, 2] = 2;
        Nodes[3, 1] = 1; Nodes[4, 2] = 1; Nodes[4, 3] = 2; Nodes[5, 4] = 3;
        Nodes[3, 3] = 1; Nodes[5, 1] = 1;
        Position = new Point(0, 0); Coherence = MaxCoherence; Finished = Won = false;
        Revealed[0, 0] = true;
        Message = "Find the system core. Defender nodes damage your virus; green nodes repair it.";
    }

    public static IEnumerable<Point> Neighbors(int col, int row)
    {
        int side = row % 2 == 0 ? -1 : 1;
        Point[] neighbors = [new(col-1,row),new(col+1,row),new(col,row-1),new(col,row+1),new(col+side,row-1),new(col+side,row+1)];
        return neighbors.Where(p=>p.X>=0&&p.X<Columns&&p.Y>=0&&p.Y<Rows);
    }

    public void Click(int col, int row, DemoWorld world)
    {
        if (Finished || col < 0 || col >= Columns || row < 0 || row >= Rows) return;
        if (!Neighbors(Position.X, Position.Y).Contains(new Point(col, row)))
        {
            Message = "Select a neighboring node."; return;
        }
        Position = new Point(col, row); Revealed[col, row] = true;
        world.AdvanceHours(.12);
        int type = Nodes[col, row];
        if (type == 1) { Coherence -= 18; Nodes[col, row] = 0; Message = "Security daemon counterattacks: -18 coherence."; }
        else if (type == 2) { Coherence = Math.Min(MaxCoherence, Coherence + 25); Nodes[col, row] = 0; Message = "Recovered repair utility: +25 coherence."; }
        else if (type == 3)
        {
            Finished = Won = true; world.SignalHacked = true;
            Message = "JOYOS FRAGMENT RECOVERED. It asks for protection, custody, or a public voice.";
            world.Say(Message); world.Say("Return to any station and press 3 to decide its fate before day 9.");
        }
        else Message = "Node clear. The signal branches deeper.";
        if (Coherence <= 0)
        {
            Coherence = 0; Finished = true; Won = false;
            Message = "Virus coherence failed. The cache resets; try another route.";
            world.Say(Message);
        }
    }
}
