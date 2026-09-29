namespace WaffleEngine.Vector;

public class Curve
{
    public List<Point> Points = new ();

    public Curve(Point start_point)
    {
        AddPoint(start_point);
    }

    public Curve Clone()
    {
        var copy = new Curve(Points[0]);
        for (int i = 1; i < Points.Count; i++)
            copy.Points.Add(Points[i]);
        return copy;
    }
    
    private void AddPoint(Point point)
    {
        Points.Add(point);
    }

    public void AddLinear(Point point)
    {
        Point start = Points[^1];
        
        AddPoint(CalculateLinear(start, point));
        AddPoint(point);
    }

    public void AddSmooth(Point point)
    {
        // NOTE: incremental helper for in-progress drawing. It can only see
        // previous points, so it assumes the rest (e.g. linear fallback for
        // the first segment). Fine while drawing; for a finished shape with
        // all anchors known, prefer FromAnchors, which makes no such guesses.
        if (Points.Count <= 1)
        {
            AddLinear(point);
            return;
        }
        
        Point start = Points[^1];
        Point start_ctrl = Points[^2];
        
        AddPoint(CalculateSmooth(start_ctrl, start, point));
        AddPoint(point);
    }

    public void Close()
    {
        if (Points.Count <= 1)
        {
            return;
        }
        
        Point start = Points[^1];
        
        AddPoint(CalculateLinear(start, Points[0]));
    }
    
    public void CloseSmooth()
    {
        if (Points.Count <= 1)
        {
            return;
        }
        
        Point start = Points[^1];
        Point start_ctrl = Points[^2];

        Point a = CalculateSmooth(start_ctrl, start, Points[0]);
        Point c = CalculateSmooth(Points[1], Points[0], start);
        Point b = CalculateLinear(a, c);
        
        AddPoint(a);
        AddPoint(b);
        AddPoint(c);
    }

    private Point CalculateLinear(Point start, Point end)
    {
        return new Point()
        {
            Position = new Vector2(
                (start.Position.x + end.Position.x) / 2,
                (start.Position.y + end.Position.y) / 2
            ),
        };
    }

    private Point CalculateSmooth(Point start_ctrl, Point start, Point end)
    {
        // Using ray from start control point to start gets the closest point on the ray to the end point.
        System.Numerics.Vector2 direction = System.Numerics.Vector2.Normalize(start.Position - start_ctrl.Position);

        Vector2 origin_point = end.Position - start.Position;

        float t = System.Numerics.Vector2.Dot(origin_point, direction);

        t = Math.Abs(t);

        return new Point((System.Numerics.Vector2)start.Position + direction * t);
    }

    public void RemovePoint(int index)
    {
        Points.RemoveAt(index);
    }

    // Global smoothing for finished shapes. Unlike the incremental AddSmooth
    // (which bakes each control from incomplete history and gets the first
    // joints wrong), this derives every tangent from final neighbor anchors
    // — the same idea CloseSmooth applies to the closing joint, extended to
    // the whole loop — then builds one quadratic per edge:
    //   smooth+smooth -> control at tangent-ray intersection (C1 at both ends),
    //   mixed         -> projection along the smooth end's tangent,
    //   sharp+sharp   -> midpoint (straight line).
    // Open endpoints ignore the smooth flag (no second neighbor to continue).
    public static Curve FromAnchors(IReadOnlyList<Vector2> anchors, IReadOnlyList<bool> smooth, bool closed)
    {
        if (anchors.Count == 0)
            throw new ArgumentException("At least one anchor is required.", nameof(anchors));
        if (smooth.Count != anchors.Count)
            throw new ArgumentException("Smooth flags must match anchors one-to-one.", nameof(smooth));

        int n = anchors.Count;

        if (n == 1)
            return new Curve(new Point(anchors[0]));

        if (n == 2)
        {
            var segment = new Curve(new Point(anchors[0]));
            segment.AddLinear(new Point(anchors[1]));
            if (closed)
                segment.Close();
            return segment;
        }

        // Unit tangents from final neighbors (central differences). Zero
        // vector marks degenerate/sharp — the edge builder falls back.
        var tangents = new Vector2[n];
        var tangentValid = new bool[n];
        for (int i = 0; i < n; i++)
        {
            Vector2 prev = anchors[closed ? (i - 1 + n) % n : Math.Max(0, i - 1)];
            Vector2 next = anchors[closed ? (i + 1) % n : Math.Min(n - 1, i + 1)];
            Vector2 delta = next - prev;
            float len = MathF.Sqrt(delta.x * delta.x + delta.y * delta.y);
            if (len > 1e-6f)
            {
                tangents[i] = new Vector2(delta.x / len, delta.y / len);
                tangentValid[i] = true;
            }
        }

        var curve = new Curve(new Point(anchors[0]));

        int edgeCount = closed ? n : n - 1;
        for (int i = 0; i < edgeCount; i++)
        {
            int j = (i + 1) % n;
            bool sa = smooth[i] && tangentValid[i] && (closed || i != 0);
            bool sb = smooth[j] && tangentValid[j] && (closed || j != n - 1);
            curve.AddPoint(BuildEdgeControl(anchors[i], anchors[j], tangents[i], tangents[j], sa, sb));
            // Closed loops wrap implicitly in the renderer — don't duplicate
            // the first anchor at the end (that would add a degenerate segment).
            if (!(closed && j == 0))
                curve.AddPoint(new Point(anchors[j]));
        }

        return curve;
    }

    private static Point BuildEdgeControl(
        Vector2 a, Vector2 b, Vector2 tangentA, Vector2 tangentB, bool smoothA, bool smoothB)
    {
        float edgeLen = MathF.Sqrt(
            (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
        if (edgeLen < 1e-6f)
            return new Point(a);

        if (!smoothA && !smoothB)
            return CalculateLinearStatic(a, b);

        float projA = Math.Abs((b.x - a.x) * tangentA.x + (b.y - a.y) * tangentA.y);
        float projB = Math.Abs((b.x - a.x) * tangentB.x + (b.y - a.y) * tangentB.y);

        if (smoothA && !smoothB)
            return new Point(new Vector2(a.x + tangentA.x * projA, a.y + tangentA.y * projA));

        if (!smoothA && smoothB)
            return new Point(new Vector2(b.x - tangentB.x * projB, b.y - tangentB.y * projB));

        // Both smooth: intersect outgoing ray of A with incoming ray of B.
        float dBx = -tangentB.x;
        float dBy = -tangentB.y;
        float denom = tangentA.x * dBy - tangentA.y * dBx;
        if (Math.Abs(denom) > 1e-4f)
        {
            float abx = b.x - a.x;
            float aby = b.y - a.y;
            float s = (abx * dBy - aby * dBx) / denom;
            float t = (abx * tangentA.y - aby * tangentA.x) / denom;
            float maxT = edgeLen * 3f;
            if (s >= 0f && t >= 0f && s <= maxT && t <= maxT)
                return new Point(new Vector2(a.x + tangentA.x * s, a.y + tangentA.y * s));
        }

        // Parallel, behind, or runaway intersection: balanced compromise that
        // stays inside the edge diamond instead of spiking.
        var fallbackA = new Vector2(a.x + tangentA.x * projA, a.y + tangentA.y * projA);
        var fallbackB = new Vector2(b.x - tangentB.x * projB, b.y - tangentB.y * projB);
        return new Point(new Vector2(
            (fallbackA.x + fallbackB.x) / 2f,
            (fallbackA.y + fallbackB.y) / 2f));
    }

    private static Point CalculateLinearStatic(Vector2 start, Vector2 end)
    {
        return new Point(new Vector2(
            (start.x + end.x) / 2f,
            (start.y + end.y) / 2f));
    }
}