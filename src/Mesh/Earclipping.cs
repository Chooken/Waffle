using System.Numerics;
using System.Security.Cryptography;

namespace WaffleEngine
{
    public static class Earclipping
    {
        public class EarPoint
        {
            public int VertexIndex;
            public bool IsEar;

            public EarPoint Left;
            public EarPoint Right;

            public bool Update(ref List<Vector2> vertices, int from_index = 0)
            {
                if (ConvexHull.GetOrientation(vertices[Right.VertexIndex], vertices[VertexIndex], vertices[Left.VertexIndex])
                    == ConvexHull.Orientation.CounterClockwise)
                {
                    bool point_in_triangle = false;

                    EarPoint point_to_process = Right.Right;

                    for (int i = from_index; i < vertices.Count; i++)
                    {
                        if (!Triangle.PointIn(
                                vertices[Left.VertexIndex],
                                vertices[VertexIndex],
                                vertices[Right.VertexIndex],
                                vertices[i]))
                        {
                            point_to_process = point_to_process.Right;
                            continue;
                        }
                    
                        point_in_triangle = true;
                        break;
                    }

                    IsEar = !point_in_triangle;
                }

                return IsEar;
            }

            public void Remove()
            {
                Left.Right = Right;
                Right.Left = Left;
            }
        }

        public static List<Triangle> Triangulate(ref List<Vector2> vertices, int from_index = 0)
        {
            List<Triangle> triangles = new();
            
            // Generate Doubley Linked List of Ears.
            
            List<EarPoint> ear_points = new List<EarPoint>();
            
            ear_points.Add(new EarPoint { VertexIndex = from_index });

            for (int i = from_index + 1; i < vertices.Count(); i++)
            {
                ear_points.Add(new EarPoint { VertexIndex = i, Left = ear_points[i - from_index - 1] });
                ear_points[i - from_index - 1].Right = ear_points[i - from_index];
            }

            ear_points[ear_points.Count() - 1].Right = ear_points[0];
            ear_points[0].Left = ear_points[ear_points.Count() - 1];
            
            // Update the IsEar property and select the last ear point.

            EarPoint selected_ear_point = null;
            
            foreach (var ear_point in ear_points)
            {
                if (ear_point.Update(ref vertices, from_index))
                    selected_ear_point = ear_point;
            }
            
            // Earclipping
            
            for (int i = from_index; i < vertices.Count() - 2; i++)
            {
                triangles.Add(new Triangle
                {
                    A = selected_ear_point.Right.VertexIndex, 
                    B = selected_ear_point.VertexIndex, 
                    C = selected_ear_point.Left.VertexIndex
                });
                
                selected_ear_point.Remove();

                selected_ear_point.Left.Update(ref vertices);
                selected_ear_point.Right.Update(ref vertices);

                // Find Next Ear.
                
                for (int loop = from_index; loop < vertices.Count(); loop++)
                {
                    selected_ear_point = selected_ear_point.Right;

                    if (selected_ear_point.IsEar)
                        break;
                }
            }

            return triangles;
        }

        public static float AngleBetweenPoints(Vector2 point_1, Vector2 point_2, Vector2 point_3)
        {
            
            
            float length_1 = MathF.Pow(point_2.X - point_1.X, 2) + MathF.Pow(point_2.Y - point_1.Y, 2);
            float length_2 = MathF.Pow(point_3.X - point_2.X, 2) + MathF.Pow(point_3.Y - point_2.Y, 2);
            float length_3 = MathF.Pow(point_3.X - point_1.X, 2) + MathF.Pow(point_3.Y - point_1.Y, 2);

            float angle = MathF.Acos((length_1 + length_2 - length_3) / (2 * MathF.Sqrt(length_1) * MathF.Sqrt(length_2))) * 180 / Single.Pi;

            float new_angle = MathF.Atan2(point_1.Y - point_2.Y, point_1.X - point_2.X) -
                              MathF.Atan2(point_3.Y - point_2.Y, point_3.X - point_2.X);

            new_angle = new_angle * 180 / Single.Pi;
            
            return angle;
        }
    }   
}