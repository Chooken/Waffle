using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public class Image(GpuTexture texture) : Rect
{
    private Shader? _shader;
    
    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_shader is null)
        {
            if (!SetupShader())
            {
                return;
            }
        }
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Bounds.CalculatedPosition),
            Size = new Vector2(Bounds.CalculatedWidth, Bounds.CalculatedHeight),
            Color = RectSettings.Color,
            BorderRadius = new Vector4(
                RectSettings.BorderRadius.BottomLeft, 
                RectSettings.BorderRadius.TopLeft, 
                RectSettings.BorderRadius.BottomRight, 
                RectSettings.BorderRadius.TopRight),
            BorderColor = RectSettings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = RectSettings.BorderSize,
        };
        
        if (texture.Handle != IntPtr.Zero)
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(_shader);
            renderPass.Bind(texture);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }
    }
    
    private bool SetupShader()
    {
        if (!Assets.TryGetShader("builtin", "ui-rect-texture", out _shader))
        {
            Log.Error("Shader not found");
            return false;
        }
        
        _shader.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.One,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
            VertexAttributes = null,
        });

        return true;
    }
}