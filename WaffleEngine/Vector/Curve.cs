namespace WaffleEngine.Vector;

public class Curve
{
    public List<Point> Points = new ();
    public Rect Bounds = Rect.Zero;

    public Curve(Point start_point)
    {
        AddPoint(start_point);
    }
    
    private void AddPoint(Point point)
    {
        Points.Add(point);
        Bounds.Contain(point.Position);
    }

    public void AddLinear(Point point)
    {
        Point start = Points[^1];
        
        AddPoint(CalculateLinear(start, point));
        AddPoint(point);
    }

    public void AddSmooth(Point point)
    {
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
        CalculateBounds();
    }
    
    public void CalculateBounds()
    {
        Bounds = Rect.Zero;
        
        foreach (Point p in Points)
        {
            Bounds.Contain(p.Position);
        }
    }
}