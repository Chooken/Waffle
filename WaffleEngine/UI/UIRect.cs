using System.Runtime.InteropServices;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public struct UIRectData
{
    public AlignedVector3 Position;
    public Vector2 Size;
    public Vector2 VertexOffset;
    public Vector4 Color;
    public Vector4 BorderRadius;
    public Vector4 BorderColor;
    public Vector2 ScreenSize;
    public float BorderSize;
}

public class UIRect
{
    public UIRect? Parent { get; private set; }
    private List<UIRect> _uiElements = new List<UIRect>();
    
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
    public UISize Gap;
    public UIDirection ChildDirection = UIDirection.Right;
    public UIAnchor ChildAnchor;
    public Color Color;
    public Color BorderColor;
    public UISize BorderSize;
    public GpuTexture? Texture;
    public bool Dirty = true;

    protected static Material? BaseRectMaterial;
    protected static Material? BaseTexturedRectMaterial;

    public virtual Vector2 GetBoundingSize(Vector2 parentSize)
    {
        var size = GetSize(parentSize);
        
        Vector2 contentSize = Vector2.Zero;

        foreach (var uiElement in _uiElements)
        {
            Vector2 childSize = uiElement.GetBoundingSize(
                new Vector2(
                    size.x - PaddingX.AsPixels(parentSize.x) * 2, 
                    size.y - PaddingY.AsPixels(parentSize.y) * 2));

            switch (ChildDirection)
            {
                case UIDirection.Right:
                    contentSize = new Vector2(
                        contentSize.x + childSize.x, 
                        MathF.Max(contentSize.y, childSize.y));
                    break;
                case UIDirection.Left:
                    contentSize = new Vector2(
                        contentSize.x + childSize.x, 
                        MathF.Max(contentSize.y, childSize.y));
                    break;
                case UIDirection.Up:
                    contentSize = new Vector2(
                        MathF.Max(contentSize.x, childSize.x),
                        contentSize.y + childSize.y);
                    break;
                case UIDirection.Down:
                    contentSize = new Vector2(
                        MathF.Max(contentSize.x, childSize.x),
                        contentSize.y + childSize.y);
                    break;
                default:
                    contentSize = new Vector2(
                        MathF.Max(contentSize.x, childSize.x),
                        MathF.Max(contentSize.y, childSize.y));
                    break;
            }
        }
        
        switch (ChildDirection)
        {
            case UIDirection.Right:
                contentSize.x += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.x);
                break;
            case UIDirection.Left:
                contentSize.x += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.x);
                break;
            case UIDirection.Up:
                contentSize.y += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.y);
                break;
            case UIDirection.Down:
                contentSize.y += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.y);
                break;
            default:
                break;
        }

        contentSize = new Vector2(
            contentSize.x + PaddingX.AsPixels(parentSize.x) * 2, 
            contentSize.y + PaddingY.AsPixels(parentSize.y) * 2);

        Vector2 realSize = new Vector2(MathF.Max(contentSize.x, size.x), MathF.Max(contentSize.y, size.y));

        return realSize;
    }
    
    public virtual Vector2 GetSize(Vector2 parentSize)
    {
        return new Vector2(
            Width.AsPixels(parentSize.x) - MarginX.AsPixels(parentSize.x) * 2, 
            Height.AsPixels(parentSize.y) - MarginY.AsPixels(parentSize.y) * 2);
    }

    public virtual Vector2 Render(ImQueue queue, ImRenderPass renderPass, Vector3 position, UIAnchor anchor, Vector2 parentSize, Vector2 renderSize)
    {
        var size = GetSize(parentSize);
        
        Vector2 contentSize = Vector2.Zero;

        foreach (var uiElement in _uiElements)
        {
            Vector2 childSize = uiElement.GetBoundingSize(
                new Vector2(
                    size.x - PaddingX.AsPixels(parentSize.x) * 2, 
                    size.y - PaddingY.AsPixels(parentSize.y) * 2));

            switch (ChildDirection)
            {
                case UIDirection.Right:
                    contentSize = new Vector2(
                        contentSize.x + childSize.x, 
                        MathF.Max(contentSize.y, childSize.y));
                    break;
                case UIDirection.Left:
                    contentSize = new Vector2(
                        contentSize.x + childSize.x, 
                        MathF.Max(contentSize.y, childSize.y));
                    break;
                case UIDirection.Up:
                    contentSize = new Vector2(
                        MathF.Max(contentSize.x, childSize.x),
                        contentSize.y + childSize.y);
                    break;
                case UIDirection.Down:
                    contentSize = new Vector2(
                        MathF.Max(contentSize.x, childSize.x),
                        contentSize.y + childSize.y);
                    break;
                default:
                    contentSize = new Vector2(
                        MathF.Max(contentSize.x, childSize.x),
                        MathF.Max(contentSize.y, childSize.y));
                    break;
            }
        }
        
        switch (ChildDirection)
        {
            case UIDirection.Right:
                contentSize.x += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.x);
                break;
            case UIDirection.Left:
                contentSize.x += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.x);
                break;
            case UIDirection.Up:
                contentSize.y += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.y);
                break;
            case UIDirection.Down:
                contentSize.y += MathF.Max(_uiElements.Count - 1, 0) * Gap.AsPixels(parentSize.y);
                break;
            default:
                break;
        }
        
        contentSize = new Vector2(
            contentSize.x + PaddingX.AsPixels(parentSize.x) * 2, 
            contentSize.y + PaddingY.AsPixels(parentSize.y) * 2);

        Vector2 realSize = new Vector2(MathF.Max(contentSize.x, size.x), MathF.Max(contentSize.y, size.y));

        position = new Vector3(
            position.x + MarginX.AsPixels(parentSize.x),
            position.y + MarginY.AsPixels(parentSize.y),
            position.z + 1);
        
        if (BaseRectMaterial is null || BaseTexturedRectMaterial is null)
            SetupMaterials();

        UIRectData data = new UIRectData()
        {
            Position = position,
            Size = realSize,
            VertexOffset = anchor.Position,
            Color = Color,
            BorderRadius = new Vector4(BorderRadiusTL.Value, BorderRadiusBL.Value, BorderRadiusTR.Value,
                BorderRadiusBR.Value),
            BorderColor = BorderColor,
            ScreenSize = renderSize,
            BorderSize = BorderSize.AsPixels(parentSize.x)
        };
        
        Dirty = false;

        if (Color.a != 0 || Texture is not null)
        {
            queue.SetUniforms(data);

            if (Texture is null)
            {
                renderPass.Bind(BaseRectMaterial);
            }
            else
            {
                renderPass.Bind(BaseTexturedRectMaterial);
                renderPass.Bind(Texture);
            }

            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        Vector2 adjustedSize = new Vector2(
            size.x - PaddingX.AsPixels(parentSize.x) * 2,
            size.y - PaddingY.AsPixels(parentSize.y) * 2);

        Vector3 adjustedPos = new Vector3(
            position.x + PaddingX.AsPixels(parentSize.x) + realSize.x * ChildAnchor.Position.x,
            position.y + PaddingY.AsPixels(parentSize.y) + realSize.y * ChildAnchor.Position.y, 
            position.z + 1);

        adjustedPos.x -= contentSize.x * ChildAnchor.Position.x;
        adjustedPos.y -= contentSize.y * ChildAnchor.Position.y;
        
        foreach (var child in _uiElements)
        {
            Vector2 childSize = child.Render( 
                queue,
                renderPass,
                adjustedPos, 
                ChildAnchor,
                adjustedSize, 
                renderSize);

            switch (ChildDirection)
            {
                case UIDirection.Right:
                    adjustedPos.x += childSize.x + Gap.AsPixels(parentSize.x);
                    break;
                case UIDirection.Left:
                    adjustedPos.x -= childSize.x + Gap.AsPixels(parentSize.x);
                    break;
                case UIDirection.Up:
                    adjustedPos.y -= childSize.y + Gap.AsPixels(parentSize.y);
                    break;
                case UIDirection.Down:
                    adjustedPos.y += childSize.y + Gap.AsPixels(parentSize.y);
                    break;
                default:
                    break;
            }
        }
        
        return realSize;
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
            DstAlphaBlendFactor = BlendFactor.One,
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
            DstAlphaBlendFactor = BlendFactor.One,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
        });

        BaseRectMaterial = new Material(_shader);
        BaseTexturedRectMaterial = new Material(_shaderTex);
    }

    public virtual void Update()
    {
        foreach (var uiElement in _uiElements)
        {
            uiElement.Update();
        }
    }

    public UIRect AddUIElement(UIRect uiRect)
    {
        uiRect.Parent = this;
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

    public UIRect SetChildAnchor(UIAnchor anchor)
    {
        ChildAnchor = anchor;
        SetDirty();
        return this;
    }

    public UIRect SetColor(Color color)
    {
        Color = color;
        SetDirty();
        return this;
    }

    public UIRect SetBorderColor(Color color)
    {
        BorderColor = color;
        SetDirty();
        return this;
    }
    
    public UIRect SetBorderSize(UISize size)
    {
        BorderSize = size;
        SetDirty();
        return this;
    }

    public UIRect SetTexture(GpuTexture? texture)
    {
        Texture = texture;
        SetDirty();
        return this;
    }

    public UIRect SetGap(UISize size)
    {
        Gap = size;
        SetDirty();
        return this;
    }

    public virtual void SetDirty()
    {
        Dirty = true;
        Parent?.SetDirty();
    }
}