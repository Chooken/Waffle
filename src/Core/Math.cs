using System.Numerics;

namespace WaffleEngine;

public static class Math
{
    public static int Mod(int value, int mod)
    {
        return (value % mod + mod) % mod;
    }

    public static float Mod(float value, float mod)
    {
        return value - mod * MathF.Floor(value / mod);
    }

    public static Vector2 ClosestPointOnLine(Vector2 line_start, Vector2 line_end, Vector2 point)
    {
        Vector2 line_localised = line_end - line_start;
        Vector2 point_localised = point - line_start;

        Vector2 closest_point = line_localised * Vector2.Dot(line_localised, point_localised) /
                                Vector2.Dot(line_localised, line_localised) + line_start;

        return closest_point;
    }

    public static bool RayLineIntersection(Vector2 line_start, Vector2 line_end, Vector2 ray_start, Vector2 ray_end, out Vector2? intersection)
    {
        intersection = null;

        Vector2 s1 = ray_end - ray_start;
        Vector2 s2 = line_end - line_start;

        float s = (-s1.Y * (ray_start.X - line_start.X) +
                   s1.X * (ray_start.X - line_start.X)) /
                  (-s2.X * s1.Y + s1.X * s2.Y);

        float t = (s2.X * (ray_start.Y - line_start.Y) -
                   s2.Y * (ray_start.X - line_start.X)) / 
                  (-s2.X * s1.Y + s1.X * s2.Y);

        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
        {
            intersection = new Vector2(ray_start.X + (t * s1.X), ray_start.Y + (t * s1.Y));
            return true;
        }
        
        return false;
    }
}

public static class MatrixExtensions
{
    public static Matrix4x4 ConvertToColumnMajor(this Matrix4x4 mat4)
    {
        return new Matrix4x4(
            mat4.M11, mat4.M21, mat4.M31, mat4.M41,
            mat4.M12, mat4.M22, mat4.M32, mat4.M42,
            mat4.M13, mat4.M23, mat4.M33, mat4.M43,
            mat4.M14, mat4.M24, mat4.M34, mat4.M44
        );
    }
}