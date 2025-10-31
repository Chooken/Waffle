using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public class Mesh : IGpuUploadable, IRenderBindable
{
    private Buffer<Vertex> _vertexBuffer = new Buffer<Vertex>(BufferUsage.Vertex);
    private Buffer<int> _indexBuffer = new Buffer<int>(BufferUsage.Index);

    public void AddVertex(Vertex vertex) => _vertexBuffer.Add(vertex);

    public void AddIndex(int index) => _indexBuffer.Add(index);

    public static Mesh Quad(Vector3 min, Vector3 max)
    {
        Mesh mesh = new Mesh();
        
        mesh._vertexBuffer.Add(new Vertex { Position = new Vector3(max.x, max.y, max.z), Uv = new Vector2(1f, 0f)});
        mesh._vertexBuffer.Add(new Vertex { Position = new Vector3(max.x, min.y, min.z + max.z * 0.5f), Uv = new Vector2(1f, 1f)});
        mesh._vertexBuffer.Add(new Vertex { Position = new Vector3(min.x, min.y, min.z), Uv = new Vector2(0f, 1f)});
        mesh._vertexBuffer.Add(new Vertex { Position = new Vector3(min.x, max.y, min.z + max.z * 0.5f), Uv = new Vector2(0f, 0f)});
        
        mesh._indexBuffer.Add(0);
        mesh._indexBuffer.Add(1);
        mesh._indexBuffer.Add(2);
        mesh._indexBuffer.Add(0);
        mesh._indexBuffer.Add(2);
        mesh._indexBuffer.Add(3);

        return mesh;
    }


    public void UploadToGpu(ImCopyPass copyPass)
    {
        if (_vertexBuffer.Count > 0)
            copyPass.Upload(_vertexBuffer);
        
        if (_indexBuffer.Count > 0)
            copyPass.Upload(_indexBuffer);
    }

    public void Bind(ImRenderPass renderPass, uint slot)
    {
        if (_vertexBuffer.Count > 0)
            renderPass.Bind(_vertexBuffer);
        
        if (_indexBuffer.Count > 0)
            renderPass.Bind(_indexBuffer);
    }
}