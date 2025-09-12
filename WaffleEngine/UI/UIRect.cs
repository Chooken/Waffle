using System.Runtime.InteropServices;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public struct UIRectData
{
    public Vector3 Position;
    public Vector2 Size;
    public Vector4 Color;
    public Vector4 BorderRadius;
    public Vector2 ScreenSize;
}

public class UIRect
{
    private UIRect? _parent;
    private List<UIRect> _uiElements = new List<UIRect>();

    public static Buffer<UIRectData> GpuData = new Buffer<UIRectData>(BufferUsage.GraphicsStorageRead);

    private UIRectData _data;
    
    public UISize Width;
    public UISize Height;
    public UISize MarginX;
    public UISize MarginY;
    public UISize PaddingX;
    public UISize PaddingY;
    public UISize BorderRadiusTL;
    public UISize BorderRadiusBL;
    public UISize BorderRadiusTR;
    public UISize BorderRadiusBR;
    public UIDirection ChildDirection;
    public Color Color;
    public GpuTexture? Texture;
    public bool Dirty;

    private Material? _material;
    private Material? _materialText;

    public virtual Vector2 AddToBuffer(ImQueue queue, ColorTargetSettings colorTargetSettings, Vector3 position, Vector2 parentSize, Vector2 renderSize)
    {
        var size = new Vector2(
            Width.AsPixels(parentSize.x) - MarginX.AsPixels(parentSize.x) * 2, 
            Height.AsPixels(parentSize.y) - MarginY.AsPixels(parentSize.y) * 2);

        position = new Vector3(
            position.x + MarginX.AsPixels(parentSize.x),
            position.y + MarginY.AsPixels(parentSize.y),
            position.z + 1);
        
        if (_material is null || _materialText is null)
            SetupMaterials();

        _data = new UIRectData()
        {
            Position = position,
            Size = size,
            Color = Color,
            BorderRadius = new Vector4(BorderRadiusTL.Value, BorderRadiusBL.Value, BorderRadiusTR.Value,
                BorderRadiusBR.Value),
            ScreenSize = renderSize
        };
        
        Dirty = false;
        
        queue.SetUniforms(_data);

        ImRenderPass renderPass = queue.AddRenderPass(colorTargetSettings);

        renderPass.Bind(Texture is null ? _material : _materialText);

        renderPass.DrawPrimatives(6, 1, 0, 0);
        renderPass.End();

        Vector3 adjustedPos = new Vector3(position.x + PaddingX.AsPixels(parentSize.x),
            position.y + PaddingY.AsPixels(parentSize.y), position.z + 1);
        
        foreach (var child in _uiElements)
        {
            Vector2 childSize = child.AddToBuffer( 
                queue,
                colorTargetSettings,
                adjustedPos, 
                new Vector2(size.x - PaddingX.AsPixels(parentSize.x) * 2, size.y - PaddingY.AsPixels(parentSize.y) * 2), renderSize);
            adjustedPos.x += childSize.x;
        }

        return new Vector2(Width.AsPixels(parentSize.x), Height.AsPixels(parentSize.y));
    }

    private void SetupMaterials()
    {
        if (!ShaderManager.TryGetShader("BuiltinShaders/ui-rect", out var _shader))
        {
            Log.Error("Shader not found");
            return;
        }
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/ui-rect-texture", out var _shaderTex))
        {
            Log.Error("Shader not found");
            return;
        }
        
        _shader.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
        });
        
        _shaderTex.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
        });

        _material = new Material(_shader);
        _materialText = new Material(_shaderTex);
        _materialText.AddTexture(Texture, 0);
    }

    public virtual void Update() {}

    public UIRect AddUIElement(UIRect uiRect)
    {
        _uiElements.Add(uiRect);
        SetDirty();
        return this;
    }

    public UIRect SetWidth(UISize size)
    {
        Width = size;
        SetDirty();
        return this;
    }

    public UIRect SetHeight(UISize size)
    {
        Height = size;
        SetDirty();
        return this;
    }

    public UIRect SetMarginX(UISize size)
    {
        MarginX = size;
        SetDirty();
        return this;
    }

    public UIRect SetMarginY(UISize size)
    {
        MarginY = size;
        SetDirty();
        return this;
    }

    public UIRect SetPaddingX(UISize size)
    {
        PaddingX = size;
        SetDirty();
        return this;
    }

    public UIRect SetPaddingY(UISize size)
    {
        PaddingY = size;
        SetDirty();
        return this;
    }

    public UIRect SetBorderRadius(Vector4 borderRadius, UISizeType type)
    {
        BorderRadiusTL.Value = borderRadius.x;
        BorderRadiusTL.Type = type;
        BorderRadiusBL.Value = borderRadius.y;
        BorderRadiusBL.Type = type;
        BorderRadiusTR.Value = borderRadius.z;
        BorderRadiusTR.Type = type;
        BorderRadiusBR.Value = borderRadius.w;
        BorderRadiusBR.Type = type;
        SetDirty();
        return this;
    }

    public UIRect SetChildDirection(UIDirection direction)
    {
        ChildDirection = direction;
        SetDirty();
        return this;
    }

    public UIRect SetColor(Color color)
    {
        Color = color;
        SetDirty();
        return this;
    }

    public UIRect SetTexture(GpuTexture? texture)
    {
        Texture = texture;
        SetDirty();
        return this;
    }

    public virtual void SetDirty()
    {
        Dirty = true;
        _parent?.SetDirty();
    }
}