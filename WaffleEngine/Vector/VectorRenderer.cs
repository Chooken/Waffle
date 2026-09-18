using CommunityToolkit.HighPerformance;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Vector;

public class VectorRenderer
{
    public List<Curve> Curves = new();
    
    public struct Instance
    {
        public Vector2 Min;
        public Vector2 Max;
        public int Offset;
        public int Length;
    }

    public Buffer<Instance> InstanceBuffer;
    public Buffer<Point> PointBuffer;

    public void AddCurve(Curve curve)
    {
        Curves.Add(curve);
    }

    public VectorRenderer()
    {
        InstanceBuffer = new Buffer<Instance>(BufferUsage.GraphicsStorageRead);
        PointBuffer = new Buffer<Point>(BufferUsage.GraphicsStorageRead);
    }

    public void Render(ImQueue queue, GpuTexture target)
    {
        if (!Assets.TryGetShader("Core", "vector", out Shader? shader))
        {
            return;
        }
        
        InstanceBuffer.Clear();
        PointBuffer.Clear();

        int offset = 0;
        
        foreach (var curve in Curves)
        {
            InstanceBuffer.Add(new Instance()
            {
                Min = curve.Bounds.Min,
                Max = curve.Bounds.Max,
                Offset = offset,
                Length = curve.Points.Count,
            });
            PointBuffer.Add(curve.Points.AsSpan());

            offset += curve.Points.Count;
        }
        
        var copypass = queue.AddCopyPass();
        copypass.Upload(InstanceBuffer);
        copypass.Upload(PointBuffer);
        copypass.End();
        
        ColorTargetSettings bgColorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0,0,0,0),
            GpuTexture = target,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        var renderPass = queue.AddRenderPass(bgColorTargetSettings);
        
        renderPass.Bind(shader);
        
        renderPass.Bind(InstanceBuffer, 0);
        renderPass.Bind(PointBuffer, 1);
        
        renderPass.DrawPrimatives(6, (uint)InstanceBuffer.Count, 0, 0);
        
        renderPass.End();
        
        Curves.Clear();
    }
}