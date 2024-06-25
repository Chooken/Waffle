using System.Numerics;

namespace WaffleEngine
{
    public struct Edge
    {
        public int A;
        public int B;

        public static bool operator ==(Edge left, Edge right)
        {
            return (left.A == right.A && left.B == right.B) ||
                (left.A == right.B && right.A == left.B);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !((left.A == right.A && left.B == right.B) ||
                (left.A == right.B && right.A == left.B));
        }
    }

    public struct Triangle
    {
        public int A; 
        public int B; 
        public int C;

        public void SortCounterClockwise(ref List<Vector2> vertices)
        {
            Vector2 centroid = new Vector2((vertices[A].X + vertices[B].X + vertices[B].X) / 3, (vertices[A].Y + vertices[B].Y + vertices[B].Y) / 3);

            float pseudo_angle_a = MathF.Atan2(vertices[A].Y - centroid.Y, vertices[A].X - centroid.X);
            float pseudo_angle_b = MathF.Atan2(vertices[B].Y - centroid.Y, vertices[B].X - centroid.X);
            float pseudo_angle_c = MathF.Atan2(vertices[C].Y - centroid.Y, vertices[C].X - centroid.X);

            List<(int, float)> verts = new List<(int, float)>();
            verts.Add((A, pseudo_angle_a));
            verts.Add((B, pseudo_angle_b));
            verts.Add((C, pseudo_angle_c));

            verts.Sort((vert1, vert2) =>
            {
                if (vert1.Item2 > vert2.Item2)
                    return 1;

                return -1;
            });

            A = verts[0].Item1;
            B = verts[1].Item1;
            C = verts[2].Item1;
        }

        public bool InCircumcircle(Vector2 vertex, List<Vector2> vertices)
        {
            return QuickInCircumcircle(vertices[A], vertices[B], vertices[C], vertex);
        }
        public static bool QuickInCircumcircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float[] mat2 = new float[3 * 3];

            mat2[0 + 0 * 3] = a.X - d.X;
            mat2[1 + 0 * 3] = b.X - d.X;
            mat2[2 + 0 * 3] = c.X - d.X;

            mat2[0 + 1 * 3] = a.Y - d.Y;
            mat2[1 + 1 * 3] = b.Y - d.Y;
            mat2[2 + 1 * 3] = c.Y - d.Y;

            mat2[0 + 2 * 3] = (a.X - d.X) * (a.X - d.X) + (a.Y - d.Y) * (a.Y - d.Y);
            mat2[1 + 2 * 3] = (b.X - d.X) * (b.X - d.X) + (b.Y - d.Y) * (b.Y - d.Y);
            mat2[2 + 2 * 3] = (c.X - d.X) * (c.X - d.X) + (c.Y - d.Y) * (c.Y - d.Y);

            float determinate_mat2 =
                mat2[0 + 0 * 3] * mat2[1 + 1 * 3] * mat2[2 + 2 * 3] +
                mat2[0 + 1 * 3] * mat2[1 + 2 * 3] * mat2[2 + 0 * 3] +
                mat2[0 + 2 * 3] * mat2[1 + 0 * 3] * mat2[2 + 1 * 3] -

                mat2[0 + 2 * 3] * mat2[1 + 1 * 3] * mat2[2 + 0 * 3] -
                mat2[0 + 1 * 3] * mat2[1 + 0 * 3] * mat2[2 + 2 * 3] -
                mat2[0 + 0 * 3] * mat2[1 + 2 * 3] * mat2[2 + 1 * 3];

            return determinate_mat2 > 0;
        }
    }

    public static class BowyerWatson
    {
        public static List<Triangle> Triangulate(ref List<Vector2> vertices)
        {

            // Create bounding super triangle
            Triangle super_triangle = GenSuperTriangle(ref vertices);
            super_triangle.SortCounterClockwise(ref vertices);

            // Initialise triangles
            List<Triangle> triangles = new List<Triangle>();

            // Add super triangle to array
            triangles.Add(super_triangle);

            // Triangulate each vertex
            for (int i = 0; i < vertices.Count - 3; i++)
            {
                AddVertex(i, ref triangles, vertices);

                //if (i == 8)
                //    break;
            }

            // Remove triangles that share edges with super triangle
            triangles = triangles.Where(triangle =>
            {
                return !(triangle.A == super_triangle.A || triangle.A == super_triangle.B || triangle.A == super_triangle.C ||
                    triangle.B == super_triangle.A || triangle.B == super_triangle.B || triangle.B == super_triangle.C ||
                    triangle.C == super_triangle.A || triangle.C == super_triangle.B || triangle.C == super_triangle.C);
            }).ToList();
            
            vertices.RemoveRange(vertices.Count - 3, 3);

            return triangles;
        }

        private static void AddVertex(int index, ref List<Triangle> triangles, List<Vector2> vertices)
        {
            List<Edge> edges = new List<Edge>();

            // Remove triangles with circumcircles containing vertex.
            triangles = triangles.Where(triangle =>
            {
                if (triangle.InCircumcircle(vertices[index], vertices))
                {
                    edges.Add(new Edge { A = triangle.A, B = triangle.B });
                    edges.Add(new Edge { A = triangle.B, B = triangle.C });
                    edges.Add(new Edge { A = triangle.C, B = triangle.A });
                    return false;
                }
                return true;
            }).ToList();

            // Get Unique Edges.
            UniqueEdges(ref edges);

            // Create new Triangle from the unique edges and new vertex.
            foreach (Edge edge in edges) 
            {
                Triangle newTri = new Triangle { A = edge.A, B = edge.B, C = index };
                newTri.SortCounterClockwise(ref vertices);
                triangles.Add(newTri);
            }
        }

        private static void UniqueEdges(ref List<Edge> edges)
        {
            List<Edge> unique_edges = new List<Edge>();

            for (int i = 0; i < edges.Count; i++)
            {
                bool isUnique = true;

                for (int j = 0; j < edges.Count; j++)
                {
                    if (i != j && edges[i] == edges[j])
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                    unique_edges.Add(edges[i]);
            }

            edges = unique_edges;
        }

        public static Triangle GenSuperTriangle(ref List<Vector2> vertices)
        {
            float minx = float.PositiveInfinity;
            float miny = float.PositiveInfinity;

            float maxx = float.NegativeInfinity; 
            float maxy = float.NegativeInfinity;

            foreach (Vector2 vertex in vertices)
            {
                minx = Math.Min(minx, vertex.X);
                miny = Math.Min(miny, vertex.X);
                maxx = Math.Max(maxx, vertex.X);
                maxy = Math.Max(maxy, vertex.X);
            }

            float dx = (maxx - minx);
            float dy = (maxy - miny);

            vertices.Add(new Vector2(minx + dx * 0.5f, maxy + dy));
            vertices.Add(new Vector2(minx - dx, miny - dy * 0.5f));
            vertices.Add(new Vector2(maxx + dx, miny - dy * 0.5f));

            return new Triangle
            {
                A = vertices.Count - 3,
                B = vertices.Count - 2,
                C = vertices.Count - 1,
            };
        }
    }
}
