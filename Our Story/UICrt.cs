using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI.Old;

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

public class UICrt(Vector2 resolution, float chromaticAberration) : UIRect
{
    public Vector2 Resolution = resolution;
    public float ChromaticAberration = chromaticAberration;

    private static Material? _crtMaterial;
    
    public override Vector2 GetSize(Vector2 parentSize)
    {
        bool widthBound = parentSize.x < parentSize.y;

        Vector2 shrunkSize;
        Vector2 scaling = new Vector2(1, 1);

        Vector2 aspectRatio = new Vector2(Resolution.x / Resolution.y, 1);

        scaling.x = Resolution.x != 0 ? Resolution.x : 1;
        scaling.y = Resolution.y != 0 ? Resolution.y : 1;

        if (widthBound)
        {
            float width = Settings.Width.AsPixels(parentSize);

            float scaledWidth = MathF.Floor(width / scaling.x) * scaling.x;
            float scaledHeight = MathF.Floor(width / aspectRatio.x * aspectRatio.y / scaling.x) * scaling.x;
            
            shrunkSize = new Vector2(
                scaledWidth != 0 ? scaledWidth : width, 
                scaledHeight != 0 ? scaledHeight : width / aspectRatio.x * aspectRatio.y);
        }
        else
        {
            float height = Settings.Height.AsPixels(parentSize);

            float scaledWidth = MathF.Floor(height / aspectRatio.y * aspectRatio.x / scaling.x) * scaling.x;
            float scaledHeight = MathF.Floor(height / scaling.x) * scaling.x;
            
            shrunkSize = new Vector2(
                scaledWidth != 0 ? scaledWidth : height / aspectRatio.y * aspectRatio.x,
                scaledHeight != 0 ? scaledHeight : height);
        }
        
        return shrunkSize;
    }

    public override void Update()
    {
        SetDirty();
    }

    public override Vector2 Render(ImQueue queue, ImRenderPass renderPass, Vector3 position, Vector2 parentSize,
        Vector2 grow,
        Vector2 renderSize)
    {
        if (Settings.Texture is null)
        {
            Log.Error("UI Crt requires a texture to render.");
            return Vector2.Zero;
        }
        
        Vector2 elementGrow = Settings.Grow ? grow : Vector2.Zero;
        
        var size = GetSize(parentSize) + elementGrow;

        Vector2 contentSize = GetContentSize(parentSize, size);

        Vector2 realSize = new Vector2(MathF.Max(contentSize.x, size.x + elementGrow.x), MathF.Max(contentSize.y, size.y + elementGrow.y));

        position = new Vector3(
            position.x + Settings.MarginX.AsPixels(parentSize),
            position.y + Settings.MarginY.AsPixels(parentSize),
            position.z + 1);

        if (_crtMaterial is null)
        {
            if (!SetupCrtMaterials())
                return Vector2.Zero;
        }

        UICrtData data = new UICrtData()
        {
            Position = position,
            Size = realSize,
            Color = Settings.Color,
            BorderRadius = Settings.BorderRadius.AsPixels(parentSize),
            BorderColor = Settings.BorderColor,
            ScreenSize = renderSize,
            RefRes = Resolution,
            ChromaticAberration = ChromaticAberration,
            BorderSize = Settings.BorderSize.AsPixels(parentSize)
        };

        renderPass.SetUniforms(data);

        renderPass.Bind(_crtMaterial);
        renderPass.Bind(Settings.Texture);

        renderPass.DrawPrimatives(6, 1, 0, 0);
        
        LastSize = realSize;
        LastPosition = position;
        
        return realSize;
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

    public UICrt SetPixelMultiple(Vector2 referenceSize)
    {
        Resolution = referenceSize;
        SetDirty();
        return this;
    }
    
    public UICrt SetChromaticAberration(float chromaticAberration)
    {
        ChromaticAberration = chromaticAberration;
        SetDirty();
        return this;
    }
}