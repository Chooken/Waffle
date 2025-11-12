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

    private static Material? _crtMaterial;

    public override void Render(ImRenderPass renderPass, Vector2 renderSize, float scale)
    {
        if (_crtMaterial is null)
        {
            if (!SetupCrtMaterials())
                return;
        }

        UICrtData data = new UICrtData()
        {
            Position = new AlignedVector3(Bounds.CalulatedPosition * scale),
            Size = new Vector2(Bounds.CalculatedWidth * scale, Bounds.CalculatedHeight * scale),
            Color = RectSettings.Color,
            BorderRadius = new Vector4(
                RectSettings.BorderRadius.BottomLeft * scale, 
                RectSettings.BorderRadius.TopLeft * scale, 
                RectSettings.BorderRadius.BottomRight * scale, 
                RectSettings.BorderRadius.TopRight * scale),
            BorderColor = RectSettings.BorderColor,
            ScreenSize = renderSize,
            RefRes = Resolution,
            ChromaticAberration = ChromaticAberration,
            BorderSize = RectSettings.BorderSize * scale
        };

        renderPass.SetUniforms(data);

        renderPass.Bind(_crtMaterial);
        renderPass.Bind(Texture);

        renderPass.DrawPrimatives(6, 1, 0, 0);
    }

    private bool SetupCrtMaterials()
    {
        if (!Assets.TryGetShader("Core", "crt", out var _crtShader))
        {
            Log.Error("Shader not found");
            return false;
        }
        
        _crtShader.SetPipeline(new PipelineSettings()
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

         _crtMaterial = new Material(_crtShader);

        return true;
    }

    public RectCrt SetResolution(Vector2 referenceSize)
    {
        Resolution = referenceSize;
        // SetDirty();
        return this;
    }
    
    public RectCrt SetChromaticAberration(float chromaticAberration)
    {
        ChromaticAberration = chromaticAberration;
        // SetDirty();
        return this;
    }
}