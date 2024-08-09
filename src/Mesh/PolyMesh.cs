using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine;

public class PolyMesh : Mesh
{
    private int _element_buffer_object;
    
    public static Mesh TriangulatePoly(List<Vector2> outer, params List<Vector2>[] inners)
    {
        List<Triangle> triangles = Earclipping.Triangulate(ref outer);

        return TrianglesToMesh(ref triangles, ref outer);
    }

    private static Mesh TrianglesToMesh(ref List<Triangle> triangles, ref List<Vector2> vertices_list)
    {
        PolyMesh mesh = new PolyMesh();
            
        Log.Info("Building Mesh with: {0} triangles", triangles.Count);

        Vector3[] vertices = new Vector3[vertices_list.Count];
        ushort[] indices = new ushort[triangles.Count * 3];

        for (int i = 0; i < vertices_list.Count; i++)
        {
            vertices[i] = new Vector3(vertices_list[i].X, vertices_list[i].Y, 0);
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            indices[i * 3 + 0] = (ushort)triangles[i].A;
            indices[i * 3 + 1] = (ushort)triangles[i].B;
            indices[i * 3 + 2] = (ushort)triangles[i].C;
        }
            
        mesh._vertex_buffer_object = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._vertex_buffer_object);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices, BufferUsage.StaticDraw);

        mesh._vertex_array_object = GL.GenVertexArray();
        GL.BindVertexArray(mesh._vertex_array_object);
        
        mesh._element_buffer_object = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._element_buffer_object);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices, BufferUsage.StaticDraw);

        return mesh;
    }
        
    public static ConvexHull.Orientation PolygonOrientation(ref List<Vector2> points)
    {
        float sum = 0f;
            
        for (int i = 0; i < points.Count; i++)
        {
            sum += (points[(i + 1) % points.Count].X - points[i].X) *
                   (points[(i + 1) % points.Count].Y + points[i].Y);
        }

        return sum < 0 ? ConvexHull.Orientation.CounterClockwise : ConvexHull.Orientation.Clockwise;
    }
    
    public override void Unload()
    {
        GL.DeleteBuffer(_element_buffer_object);
        
        base.Unload();
    }
}