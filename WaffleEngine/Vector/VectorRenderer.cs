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
            PointBuffer.Add(curve.Points[0]);

            for (int i = 0; i + 2 < curve.Points.Count; i += 2)
            {
                SplitAndAddBezier(curve.Points[i].Position, curve.Points[i + 1].Position, curve.Points[i + 2].Position);
                PointBuffer.Add(curve.Points[i + 2]);
            }
            
            Vector2 start = (curve.Points.Count % 2 == 1) ? 
                curve.Points[^1].Position : 
                curve.Points[^2].Position;
            Vector2 control = (curve.Points.Count % 2 == 1)
                ? (PointBuffer[^1].Position + PointBuffer[0].Position) / 2
                : curve.Points[^1].Position;
            Vector2 end = curve.Points[0].Position;

            SplitAndAddBezier(start, control, end);
            
            InstanceBuffer.Add(new Instance()
            {
                Min = curve.Bounds.Min,
                Max = curve.Bounds.Max,
                Offset = offset,
                Length = PointBuffer.Count,
            });
            
            offset += PointBuffer.Count;
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

    private void SplitAndAddBezier(System.Numerics.Vector2 start, System.Numerics.Vector2 control, System.Numerics.Vector2 end)
    {
        float tE = (start.Y - control.Y) / (start.Y - 2 * control.Y + end.Y);

        if (tE > 0 && tE < 1)
        {
            System.Numerics.Vector2 p01 = System.Numerics.Vector2.Lerp(start, control, tE);
            System.Numerics.Vector2 p12 = System.Numerics.Vector2.Lerp(control, end, tE);
            System.Numerics.Vector2 pMid = System.Numerics.Vector2.Lerp(p01, p12, tE);

            PointBuffer.Add(new Point(p01));
            PointBuffer.Add(new Point(pMid));
            PointBuffer.Add(new Point(p12));
        }
        else
        {
            PointBuffer.Add(new Point(control));
        }
    }
}