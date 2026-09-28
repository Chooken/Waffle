using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public class Mesh : IGpuUploadable, IRenderBindable
{
    public Buffer<Vertex> Vertexes = new Buffer<Vertex>(BufferUsage.Vertex);
    public Buffer<int> Indices = new Buffer<int>(BufferUsage.Index);

    public static Mesh Quad(Vector3 min, Vector3 max, Color color)
    {
        Mesh mesh = new Mesh();
        
        mesh.Vertexes.Add(new Vertex { Color = color, Position = new Vector4(max.x, max.y, max.z, 1), Uv = new Vector2(1f, 0f)});
        mesh.Vertexes.Add(new Vertex { Color = color, Position = new Vector4(max.x, min.y, min.z + (max.z - min.z) * 0.5f, 1), Uv = new Vector2(1f, 1f)});
        mesh.Vertexes.Add(new Vertex { Color = color, Position = new Vector4(min.x, min.y, min.z, 1), Uv = new Vector2(0f, 1f)});
        mesh.Vertexes.Add(new Vertex { Color = color, Position = new Vector4(min.x, max.y, min.z + (max.z - min.z) * 0.5f, 1), Uv = new Vector2(0f, 0f)});
        
        mesh.Indices.Add(0);
        mesh.Indices.Add(1);
        mesh.Indices.Add(2);
        mesh.Indices.Add(0);
        mesh.Indices.Add(2);
        mesh.Indices.Add(3);

        return mesh;
    }


    public void UploadToGpu(ImCopyPass copyPass)
    {
        if (Vertexes.Count > 0)
            copyPass.Upload(Vertexes);
        
        if (Indices.Count > 0)
            copyPass.Upload(Indices);
    }

    public void Bind(ImRenderPass renderPass, uint slot)
    {
        if (Vertexes.Count > 0)
            renderPass.Bind(Vertexes);
        
        if (Indices.Count > 0)
            renderPass.Bind(Indices);
    }
}