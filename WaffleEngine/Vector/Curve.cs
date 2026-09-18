namespace WaffleEngine.Vector;

public class Curve
{
    public List<Point> Points = new ();
    public Rect Bounds = Rect.Zero;

    public void AddPoint(Point point)
    {
        Points.Add(point);
        Bounds.Contain(point.Position);
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