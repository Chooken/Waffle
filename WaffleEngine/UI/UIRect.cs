using System.Diagnostics;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI;

public struct UIRectData
{
    public AlignedVector3 Position;
    public Vector2 Size;
    public Vector4 Color;
    public Vector4 BorderRadius;
    public Vector4 BorderColor;
    public Vector2 ScreenSize;
    public float BorderSize;
}

public class UIRect
{
    public UIRect? Parent { get; private set; }
    protected List<UIRect> _uiElements = new List<UIRect>();

    //public UISettings DefaultSettings;
    public UISettings Settings;
    private UISettings _newSettings;

    public delegate void ActionRef<T>(ref T item);

    private Func<UISettings>? _default;
    private ActionRef<UISettings>? _onHoverEvent;
    private ActionRef<UISettings>? _onClickEvent;
    private ActionRef<UISettings>? _onHoldEvent;
    public bool Dirty = true;

    protected static Material? BaseRectMaterial;
    protected static Material? BaseTexturedRectMaterial;

    public Vector2 LastSize;
    public Vector3 LastPosition;

    public virtual Vector2 GetBoundingSize(Vector2 parentSize)
    {
        var size = GetSize(parentSize);

        Vector2 contentSize = GetContentSize(parentSize, size);

        Vector2 realSize = new Vector2(MathF.Max(contentSize.x, size.x), MathF.Max(contentSize.y, size.y));

        return realSize;
    }
    
    public virtual Vector2 GetSize(Vector2 parentSize)
    {
        return new Vector2(
            Settings.Width.AsPixels(parentSize) - Settings.MarginX.AsPixels(parentSize) * 2, 
            Settings.Height.AsPixels(parentSize) - Settings.MarginY.AsPixels(parentSize) * 2);
    }

    public virtual Vector2 Render(ImQueue queue, ImRenderPass renderPass, Vector3 position, Vector2 parentSize, Vector2 grow, Vector2 renderSize)
    {
        Vector2 elementGrow = Settings.Grow ? grow : Vector2.Zero;
        
        var size = GetSize(parentSize) + elementGrow;

        Vector2 contentSize = GetContentSize(parentSize, size);

        Vector2 realSize = new Vector2(MathF.Max(contentSize.x, size.x), MathF.Max(contentSize.y, size.y));

        position = new Vector3(
            position.x + Settings.MarginX.AsPixels(parentSize),
            position.y + Settings.MarginY.AsPixels(parentSize),
            position.z + 1);

        if (BaseRectMaterial is null || BaseTexturedRectMaterial is null)
        {
            if (!SetupMaterials())
                return Vector2.Zero;
        }

        UIRectData data = new UIRectData()
        {
            Position = position,
            Size = realSize,
            Color = Settings.Color,
            BorderRadius = Settings.BorderRadius.AsPixels(realSize),
            BorderColor = Settings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = Settings.BorderSize.AsPixels(parentSize)
        };
        
        Dirty = false;

        if (Settings.Color.a != 0 || Settings.Texture is not null)
        {
            renderPass.SetUniforms(data);

            if (Settings.Texture is null)
            {
                renderPass.Bind(BaseRectMaterial);
            }
            else
            {
                renderPass.Bind(BaseTexturedRectMaterial);
                renderPass.Bind(Settings.Texture);
            }

            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        Vector2 adjustedSize = new Vector2(
            size.x - Settings.PaddingX.AsPixels(parentSize) * 2,
            size.y - Settings.PaddingY.AsPixels(parentSize) * 2);

        Vector3 adjustedPos = new Vector3(
            position.x + Settings.PaddingX.AsPixels(parentSize) + realSize.x * Settings.ChildAnchor.Position.x,
            position.y + Settings.PaddingY.AsPixels(parentSize) + realSize.y * Settings.ChildAnchor.Position.y, 
            position.z + 1);
        
        int growCount = GetGrowCount();
        
        Vector2 growSize;
        
        switch (Settings.ChildDirection)
        {
            case UIDirection.Right or UIDirection.Left:
                growSize = new Vector2((adjustedSize.x - contentSize.x) / growCount, adjustedSize.y);
                break;
            case UIDirection.Up or UIDirection.Down:
                growSize = new Vector2(adjustedSize.x, (adjustedSize.y - contentSize.y) / growCount);
                break;
            default:
                growSize = Vector2.Zero;
                break;
        }

        if (growCount != 0)
        {
            contentSize.x = MathF.Max(contentSize.x, realSize.x);
            contentSize.y = MathF.Max(contentSize.y, realSize.y);
        }
        
        adjustedPos.x -= contentSize.x * Settings.ChildAnchor.Position.x;
        adjustedPos.y -= contentSize.y * Settings.ChildAnchor.Position.y;
        
        foreach (var child in _uiElements)
        {
            Vector2 childSize = child.Render( 
                queue,
                renderPass,
                adjustedPos, 
                adjustedSize, 
                growSize,
                renderSize);
                
            switch (Settings.ChildDirection)
            {
                case UIDirection.Right:
                    adjustedPos.x += childSize.x + Settings.Gap.AsPixels(parentSize);
                    break;
                case UIDirection.Left:
                    adjustedPos.x -= childSize.x + Settings.Gap.AsPixels(parentSize);
                    break;
                case UIDirection.Up:
                    adjustedPos.y -= childSize.y + Settings.Gap.AsPixels(parentSize);
                    break;
                case UIDirection.Down:
                    adjustedPos.y += childSize.y + Settings.Gap.AsPixels(parentSize);
                    break;
                default:
                    break;
            }
        }

        LastSize = realSize;
        LastPosition = position;
        
        return realSize;
    }

    protected int GetGrowCount()
    {
        int count = 0;
        
        foreach (var element in _uiElements)
        {
            if (element.Settings.Grow)
                count++;
        }

        return count;
    }

    protected Vector2 GetContentSize(Vector2 parentSize, Vector2 size)
    {
        Vector2 contentSize = Vector2.Zero;
        
        foreach (var uiElement in _uiElements)
        {
            Vector2 childSize = uiElement.GetBoundingSize(
                new Vector2(
                    size.x - Settings.PaddingX.AsPixels(parentSize) * 2, 
                    size.y - Settings.PaddingY.AsPixels(parentSize) * 2));

            switch (Settings.ChildDirection)
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
        
        switch (Settings.ChildDirection)
        {
            case UIDirection.Right:
                contentSize.x += MathF.Max(_uiElements.Count - 1, 0) * Settings.Gap.AsPixels(parentSize);
                break;
            case UIDirection.Left:
                contentSize.x += MathF.Max(_uiElements.Count - 1, 0) * Settings.Gap.AsPixels(parentSize);
                break;
            case UIDirection.Up:
                contentSize.y += MathF.Max(_uiElements.Count - 1, 0) * Settings.Gap.AsPixels(parentSize);
                break;
            case UIDirection.Down:
                contentSize.y += MathF.Max(_uiElements.Count - 1, 0) * Settings.Gap.AsPixels(parentSize);
                break;
            default:
                break;
        }
        
        contentSize = new Vector2(
            contentSize.x + Settings.PaddingX.AsPixels(parentSize) * 2, 
            contentSize.y + Settings.PaddingY.AsPixels(parentSize) * 2);
        
        return contentSize;
    }

    protected bool SetupMaterials()
    {
        if (!Assets.TryGetShader("builtin", "ui-rect", out var _shader))
        {
            WLog.Error("Shader not found");
            return false;
        }
        
        if (!Assets.TryGetShader("builtin", "ui-rect-texture", out var _shaderTex))
        {
            WLog.Error("Shader not found");
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
            VertexAttributes = null,
        });

        BaseRectMaterial = new Material(_shader);
        BaseTexturedRectMaterial = new Material(_shaderTex);

        return true;
    }

    public bool PropagateUpdate(Window window, bool propagateEvents)
    {
        Settings = _default.Invoke();
        
        for (int i = _uiElements.Count - 1; i >= 0; i--)
        {
            propagateEvents = _uiElements[i].PropagateUpdate(window, propagateEvents);
        }
        
        Update();
        
        if (propagateEvents)
        {
            propagateEvents = !ProcessEvents(window);
        }

        return propagateEvents;
    }

    public virtual void Update() {}

    public bool ProcessEvents(Window window)
    {
        Vector2 b = new Vector2(LastPosition.x + LastSize.x, LastPosition.y + LastSize.y);
        Vector2 mousePos = window.WindowInput.MouseData.Position;
        
        UISettings settings = Settings;

        bool captured = false;

        if (MathF.Min(LastPosition.x, b.x) < mousePos.x && MathF.Min(LastPosition.y, b.y) < mousePos.y &&
            MathF.Max(LastPosition.x, b.x) > mousePos.x && MathF.Max(LastPosition.y, b.y) > mousePos.y)
        {
            if (window.WindowInput.MouseData.IsLeftPressed && _onClickEvent is not null)
            {
                _onClickEvent.Invoke(ref settings);
            }
            else if (window.WindowInput.MouseData.IsLeftDown && _onHoldEvent is not null)
            {
                _onHoldEvent.Invoke(ref settings);
            }
            else if (_onHoverEvent is not null)
            {
                _onHoverEvent.Invoke(ref settings);
            }

            captured = true;
        }

        if (!settings.Equals(Settings))
        {
            SetDirty();
            Settings = settings;
        }

        return captured;
    }

    public UIRect AddUIElement(UIRect uiRect)
    {
        uiRect.Parent = this;
        _uiElements.Add(uiRect);
        SetDirty();
        return this;
    }

    public UIRect Default(Func<UISettings> defaultSettings)
    {
        _default += defaultSettings;
        return this;
    }

    public UIRect OnHover(ActionRef<UISettings> hover)
    {
        _onHoverEvent += hover;
        return this;
    }

    public UIRect OnClick(ActionRef<UISettings> click)
    {
        _onClickEvent += click;
        return this;
    }
    
    public UIRect OnHold(ActionRef<UISettings> hold)
    {
        _onHoldEvent += hold;
        return this;
    }

    public virtual void SetDirty()
    {
        Dirty = true;
        Parent?.SetDirty();
    }
}