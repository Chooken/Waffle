using System.Numerics;

namespace WaffleEngine;

public static class ConvexHull
{
    private const int INT_MAXIMUM = 100000;

    public enum Orientation
    {
        Colinear,
        Clockwise,
        CounterClockwise
    }

    public static Orientation GetOrientation(Vector2 point_1, Vector2 point_2, Vector2 point_3)
    {
        float value = (point_2.X - point_1.X) * (point_3.Y - point_1.Y) -
                                  (point_2.Y - point_1.Y) * (point_3.X - point_1.X);

        if (value == 0)
            return Orientation.Colinear;

        return (value < 0) ? Orientation.Clockwise : Orientation.CounterClockwise;
    }

    public static List<int> GrahamScan(ref List<Vector2> points)
    {
        List<int> hull_points = new List<int>();

        if (points.Count() == 0)
            return hull_points;

        int left_most_point = 0;

        for (int i = 0; i < points.Count(); i++)
        {
            if (points[i].X < points[left_most_point].X)
                left_most_point = i;
        }

        int point = left_most_point;
        int search_point = 0;
        
        do
        {
            hull_points.Add(point);
            
            search_point = (point + 1) % points.Count();

            for (int i = 0; i < points.Count(); i++)
            {
                if (GetOrientation(points[point], points[i], points[search_point]) == Orientation.CounterClockwise)
                    search_point = i;
            }

            point = search_point;

        } while (point != left_most_point);

        return hull_points;
    }
}