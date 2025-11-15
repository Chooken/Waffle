using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory;

public struct UICrtData
{
    public AlignedVector3 Position;
    public Vector2 Size;
    public Vector4 Color;
    public Vector4 BorderRadius;
    public Vector4 BorderColor;
    public Vector2 ScreenSize;
    public Vector2 RefRes;
    public float ChromaticAberration;
    public float BorderSize;
}

public class RectCrt(GpuTexture texture, Vector2 resolution, float chromaticAberration) : Rect
{
    public Vector2 Resolution = resolution;
    public float ChromaticAberration = chromaticAberration;
    public GpuTexture Texture = texture;

    private static Shader? _shader;

    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_shader is null)
        {
            if (!SetupCrtShader())
                return;
        }

        UICrtData data = new UICrtData()
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
            RefRes = Resolution,
            ChromaticAberration = ChromaticAberration,
            BorderSize = RectSettings.BorderSize
        };

        renderPass.SetUniforms(data);

        renderPass.Bind(_shader);
        renderPass.Bind(Texture);

        renderPass.DrawPrimatives(6, 1, 0, 0);
    }

    private bool SetupCrtShader()
    {
        if (!Assets.TryGetShader("Core", "crt", out _shader))
        {
            //Log.Error("Shader not found");
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