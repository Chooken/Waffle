using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI.Old;

namespace WaffleEngine.UI;

public class Rect : IUiElement
{
    public ref IUiElement? Parent => ref _parent;
    private IUiElement? _parent;
    private List<IUiElement> _children = new ();

    public ref UiSettings Settings => ref _settings;
    private UiSettings _settings = new UiSettings();
    
    public ref UiLayout Layout => ref _layout;
    private UiLayout _layout;
    
    public delegate void ActionRef<T>(ref T item);
    
    private Func<UiSettings>? _default;
    private ActionRef<UiSettings>? _onHoverEvent;
    private ActionRef<UiSettings>? _onClickEvent;
    private ActionRef<UiSettings>? _onHoldEvent;
    
    /// <summary>
    /// Calculates the fit size of the element.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void CalculateFitSize(bool width)
    {
        if (width)
        {
            Layout.ContentWidth = 0;
        }
        else
        {
            Layout.ContentHeight = 0;
        }
        
        // Calculate Fit Content Size
        foreach (var child in _children)
        {
            child.CalculateFitSize(width);

            switch (Settings.Direction)
            {
                case UiDirection.LeftToRight:
                    
                    if (width)
                    {
                        Layout.ContentWidth += child.Layout.CalculatedWidth;
                    }
                    else
                    {
                        Layout.ContentHeight = MathF.Max(Layout.ContentHeight, child.Layout.CalculatedHeight);
                    }
                    break;
                
                case UiDirection.TopToBottom:
                    
                    if (width)
                    {
                        Layout.ContentWidth = MathF.Max(Layout.ContentWidth, child.Layout.CalculatedWidth);
                    }
                    else
                    {
                        Layout.ContentHeight += child.Layout.CalculatedHeight;
                    }
                    break;
            }
        }
        
        // Add Gap to Content Size
        if (width)
        {
            if (Settings.Direction == UiDirection.LeftToRight)
                Layout.ContentWidth += (_children.Count - 1) * Settings.Gap;
        }
        else
        {
            if (Settings.Direction == UiDirection.TopToBottom)
                Layout.ContentHeight += (_children.Count - 1) * Settings.Gap;
        }

        // Set Calculated Size based off type.
        if (width)
        {
            switch (Settings.Width.SizeType)
            {
                case UiSizeType.Fixed:
                    Layout.CalculatedWidth = Settings.Width.Value;
                    break;
                case UiSizeType.Fit or UiSizeType.Grow:
                    Layout.CalculatedWidth = MathF.Max(Layout.ContentWidth + Settings.Padding.TotalHorizontal, Settings.Width.MinValue);
                    break;
                default:
                    Layout.CalculatedWidth = 0;
                    break;
            }
        }
        else
        {
            switch (Settings.Height.SizeType)
            {
                case UiSizeType.Fixed:
                    Layout.CalculatedHeight = Settings.Height.Value;
                    break;
                case UiSizeType.Fit or UiSizeType.Grow:
                    Layout.CalculatedHeight = MathF.Max(Layout.ContentHeight + Settings.Padding.TotalVertical, Settings.Height.MinValue);
                    break;
                default:
                    Layout.CalculatedHeight = 0;
                    break;
            }
        }
    }

    public void CalculatePercentages(bool width)
    {
        foreach (var child in _children)
        {
            if (width)
            {
                if (child.Settings.Width.SizeType == UiSizeType.Percentage)
                {
                    if (Settings.Direction == UiDirection.LeftToRight)
                    {
                        child.Layout.CalculatedWidth = (Layout.CalculatedWidth - Settings.Padding.TotalHorizontal) * child.Settings.Width.Value;
                        Layout.ContentWidth += child.Layout.CalculatedWidth;
                    }
                    else
                    {
                        child.Layout.CalculatedWidth = (Layout.CalculatedWidth - Settings.Padding.TotalHorizontal) * child.Settings.Width.Value;
                        Layout.ContentWidth = MathF.Max(Layout.ContentWidth, child.Layout.CalculatedWidth);
                    }
                }
            }
            else
            {
                if (child.Settings.Height.SizeType == UiSizeType.Percentage)
                {
                    if (Settings.Direction == UiDirection.LeftToRight)
                    {
                        child.Layout.CalculatedHeight = (Layout.CalculatedHeight - Settings.Padding.TotalVertical) * child.Settings.Height.Value;
                        Layout.ContentHeight = MathF.Max(Layout.ContentHeight, child.Layout.CalculatedHeight);
                    }
                    else
                    {
                        child.Layout.CalculatedHeight = (Layout.CalculatedHeight - Settings.Padding.TotalVertical) * child.Settings.Height.Value;
                        Layout.ContentHeight += child.Layout.CalculatedHeight;
                    }
                }
            }
            
            child.CalculatePercentages(width);
        }
    }

    /// <summary>
    /// Grows the children to fill the remainder.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void GrowOrShrink(bool width)
    {
        float remainder = width ? 
            Layout.CalculatedWidth - Settings.Padding.TotalHorizontal - Layout.ContentWidth : 
            Layout.CalculatedHeight - Settings.Padding.TotalVertical - Layout.ContentHeight;

        int childGrowCount = 0;

        // Implement Shrink Later
        if (remainder < 0)
            return;
        
        foreach (var child in _children)
        {
            if (width)
            {
                if (child.Settings.Width.SizeType == UiSizeType.Grow)
                {
                    childGrowCount++;
                }
            }
            else
            {
                if (child.Settings.Height.SizeType == UiSizeType.Grow)
                {
                    childGrowCount++;
                }
            }
        }

        bool fillPass = (width && Settings.Direction == UiDirection.TopToBottom) ||
                        (!width && Settings.Direction == UiDirection.LeftToRight);

        while (remainder > 0 || fillPass)
        {
            if (_children.Count == 0)
                break;
            
            float growValue = remainder / childGrowCount;
            
            foreach (var child in _children)
            {
                if (width)
                {
                    if (child.Settings.Width.SizeType == UiSizeType.Grow)
                    {
                        if (Settings.Direction == UiDirection.LeftToRight)
                        {
                            if (child.Layout.CalculatedWidth + growValue > child.Settings.Width.MaxValue)
                            {
                                remainder -= child.Settings.Width.MaxValue - child.Layout.CalculatedWidth;
                                child.Layout.CalculatedWidth = child.Settings.Width.MaxValue;
                            }
                            else
                            {
                                child.Layout.CalculatedWidth += growValue;
                                remainder -= growValue;
                            }
                        }
                        else
                        {
                            child.Layout.CalculatedWidth +=
                                Layout.CalculatedWidth - Settings.Padding.TotalHorizontal - child.Layout.CalculatedWidth;
                        }
                    }
                }
                else
                {
                    if (child.Settings.Height.SizeType == UiSizeType.Grow)
                    {
                        if (Settings.Direction == UiDirection.LeftToRight)
                        {
                            child.Layout.CalculatedHeight +=
                                Layout.CalculatedHeight - Settings.Padding.TotalVertical - child.Layout.CalculatedHeight;
                        }
                        else
                        {
                            if (child.Layout.CalculatedHeight + growValue > child.Settings.Height.MaxValue)
                            {
                                remainder -= child.Settings.Height.MaxValue - child.Layout.CalculatedHeight;
                                child.Layout.CalculatedHeight = child.Settings.Height.MaxValue;
                            }
                            else
                            {
                                child.Layout.CalculatedHeight += growValue;
                                remainder -= growValue;
                            }
                        }
                    }
                }

                child.GrowOrShrink(width);
            }

            if (fillPass)
                break;
        }
    }
    
    /// <summary>
    /// Sets the Rects Position and Calculates Child Positions.
    /// </summary>
    /// <param name="position">The position of the element.</param>
    public void CalculatePositions(Vector2 position)
    {
        Layout.CalulatedPosition = position;
        
        // Get Start Position and Size Without Padding
        Vector2 childStartPosition = new Vector2(
            position.x + Settings.Padding.Left, 
            position.y + Settings.Padding.Top);
        Vector2 sizeWithoutPadding = new Vector2(
            Layout.CalculatedWidth - Settings.Padding.TotalHorizontal, 
            Layout.CalculatedHeight - Settings.Padding.TotalVertical);

        Vector2 childOffset = Vector2.Zero;

        foreach (var child in _children)
        {
            // Get Alignment Offset
            Vector2 alignmentOffset = Vector2.Zero;

            float horizontalRemainder = Settings.Direction == UiDirection.LeftToRight
                ? sizeWithoutPadding.x - Layout.ContentWidth
                : sizeWithoutPadding.x - child.Layout.CalculatedWidth;

            switch (Settings.Alignment.Horizontal)
            {
                case UiAlignmentHorizontal.Center:
                    alignmentOffset.x = horizontalRemainder / 2;
                    break;
                case UiAlignmentHorizontal.Right:
                    alignmentOffset.x = horizontalRemainder;
                    break;
            }
            
            float verticalRemainder = Settings.Direction == UiDirection.TopToBottom
                ? sizeWithoutPadding.y - Layout.ContentHeight
                : sizeWithoutPadding.y - child.Layout.CalculatedHeight;

            switch (Settings.Alignment.Vertical)
            {
                case UiAlignmentVertical.Center:
                    alignmentOffset.y = verticalRemainder / 2;
                    break;
                case UiAlignmentVertical.Bottom:
                    alignmentOffset.y = verticalRemainder;
                    break;
            }

            // Propagate to child.
            child.CalculatePositions(childStartPosition + alignmentOffset + childOffset);

            
            // Offset next child.
            switch (Settings.Direction)
            {
                case UiDirection.LeftToRight:
                    childOffset.x += child.Layout.CalculatedWidth + Settings.Gap;
                    break; 
                case UiDirection.TopToBottom:
                    childOffset.y += child.Layout.CalculatedHeight + Settings.Gap;
                    break;
            }
        }
    }

    public virtual void Render(ImRenderPass renderPass, Vector2 renderSize, float scale)
    {
        if (!Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            WLog.Error("Shader not found");
            return;
        }
        
        shader.SetPipeline(new PipelineSettings()
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
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Layout.CalulatedPosition * scale),
            Size = new Vector2(Layout.CalculatedWidth * scale, Layout.CalculatedHeight * scale),
            Color = Settings.Color,
            BorderRadius = new Vector4(
                Settings.BorderRadius.BottomLeft * scale, 
                Settings.BorderRadius.TopLeft * scale, 
                Settings.BorderRadius.BottomRight * scale, 
                Settings.BorderRadius.TopRight * scale),
            BorderColor = Settings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = Settings.BorderSize * scale,
        };
        
        if (Settings.Color.a != 0)
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(shader);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        foreach (var child in _children)
        {
            child.Render(renderPass, renderSize, scale);
        }
    }

    public bool PropagateUpdate(Window window, bool propagateEvents)
    {
        Settings = _default?.Invoke() ?? default;
        
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            propagateEvents = _children[i].PropagateUpdate(window, propagateEvents);
        }
        
        if (propagateEvents)
        {
            propagateEvents = !ProcessEvents(window);
        }

        return propagateEvents;
    }
    
    public bool ProcessEvents(Window window)
    {
        Vector2 b = new Vector2(Layout.CalulatedPosition.x + Layout.CalculatedWidth, Layout.CalulatedPosition.y + Layout.CalculatedHeight);
        Vector2 mousePos = window.WindowInput.MouseData.Position;
        
        UiSettings settings = Settings;

        bool captured = false;

        if (MathF.Min(Layout.CalulatedPosition.x, b.x) < mousePos.x && MathF.Min(Layout.CalulatedPosition.y, b.y) < mousePos.y &&
            MathF.Max(Layout.CalulatedPosition.x, b.x) > mousePos.x && MathF.Max(Layout.CalulatedPosition.y, b.y) > mousePos.y)
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
            Settings = settings;
        }

        return captured;
    }
    
    public Rect Default(Func<UiSettings> defaultSettings)
    {
        _default += defaultSettings;
        return this;
    }

    public Rect OnHover(ActionRef<UiSettings> hover)
    {
        _onHoverEvent += hover;
        return this;
    }

    public Rect OnClick(ActionRef<UiSettings> click)
    {
        _onClickEvent += click;
        return this;
    }
    
    public Rect OnHold(ActionRef<UiSettings> hold)
    {
        _onHoldEvent += hold;
        return this;
    }

    public Rect Add(IUiElement child)
    {
        _children.Add(child);
        child.Parent = this;
        return this;
    }
}