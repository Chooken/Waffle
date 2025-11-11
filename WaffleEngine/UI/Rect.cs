using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI.Old;

namespace WaffleEngine.UI;

public class Rect : IUiElement
{
    private Rect? _parent;
    private List<Rect> _children = new ();

    public UiSizeData Width = Ui.Fit;
    public UiSizeData Height = Ui.Fit;
    public float PaddingLeft;
    public float PaddingRight;
    public float PaddingTop;
    public float PaddingBottom;
    public UiDirection Direction = UiDirection.LeftToRight;
    public UiAlignment Alignment;
    public float Gap;
    public Color Color;

    public float ContentWidth;
    public float ContentHeight;
    public float CalculatedWidth;
    public float CalculatedInnerWidth => CalculatedWidth - PaddingLeft - PaddingRight;
    public float CalculatedHeight;
    public float CalculatedInnerHeight => CalculatedHeight - PaddingTop - PaddingBottom;
    public Vector2 CalulatedPosition;
    
    /// <summary>
    /// Calculates the fit size of the element.
    /// </summary>
    /// <param name="width">Is the Width or Height calculated.</param>
    public void CalculateFitSize(bool width)
    {
        if (width)
        {
            ContentWidth = 0;
        }
        else
        {
            ContentHeight = 0;
        }
        
        // Calculate Fit Content Size
        foreach (var child in _children)
        {
            child.CalculateFitSize(width);

            switch (Direction)
            {
                case UiDirection.LeftToRight:
                    
                    if (width)
                    {
                        ContentWidth += child.CalculatedWidth;
                    }
                    else
                    {
                        ContentHeight = MathF.Max(ContentHeight, child.CalculatedHeight);
                    }
                    break;
                
                case UiDirection.TopToBottom:
                    
                    if (width)
                    {
                        ContentWidth = MathF.Max(ContentWidth, child.CalculatedWidth);
                    }
                    else
                    {
                        ContentHeight += child.CalculatedHeight;
                    }
                    break;
            }
        }
        
        // Add Gap to Content Size
        if (width)
        {
            if (Direction == UiDirection.LeftToRight)
                ContentWidth += (_children.Count - 1) * Gap;
        }
        else
        {
            if (Direction == UiDirection.TopToBottom)
                ContentHeight += (_children.Count - 1) * Gap;
        }

        // Set Calculated Size based off type.
        if (width)
        {
            switch (Width.SizeType)
            {
                case UiSizeType.Fixed:
                    CalculatedWidth = Width.Value;
                    break;
                case UiSizeType.Fit or UiSizeType.Grow:
                    CalculatedWidth = MathF.Max(ContentWidth + PaddingLeft + PaddingRight, Width.MinValue);
                    break;
                default:
                    CalculatedWidth = 0;
                    break;
            }
        }
        else
        {
            switch (Height.SizeType)
            {
                case UiSizeType.Fixed:
                    CalculatedHeight = Height.Value;
                    break;
                case UiSizeType.Fit or UiSizeType.Grow:
                    CalculatedHeight = MathF.Max(ContentHeight + PaddingTop + PaddingBottom, Height.MinValue);
                    break;
                default:
                    CalculatedHeight = 0;
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
                if (child.Width.SizeType == UiSizeType.Percentage)
                {
                    if (Direction == UiDirection.LeftToRight)
                    {
                        child.CalculatedWidth = CalculatedInnerWidth * child.Width.Value;
                        ContentWidth += child.CalculatedWidth;
                    }
                    else
                    {
                        child.CalculatedWidth = CalculatedInnerWidth * child.Width.Value;
                        ContentWidth = MathF.Max(ContentWidth, child.CalculatedWidth);
                    }
                }
            }
            else
            {
                if (child.Height.SizeType == UiSizeType.Percentage)
                {
                    if (Direction == UiDirection.LeftToRight)
                    {
                        child.CalculatedHeight = CalculatedInnerHeight * child.Height.Value;
                        ContentHeight = MathF.Max(ContentHeight, child.CalculatedHeight);
                    }
                    else
                    {
                        child.CalculatedHeight = CalculatedInnerHeight * child.Height.Value;
                        ContentHeight += child.CalculatedHeight;
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
            CalculatedInnerWidth - ContentWidth : 
            CalculatedInnerHeight - ContentHeight;

        int childGrowCount = 0;

        // Implement Shrink Later
        if (remainder < 0)
            return;
        
        foreach (var child in _children)
        {
            if (width)
            {
                if (child.Width.SizeType == UiSizeType.Grow)
                {
                    childGrowCount++;
                }
            }
            else
            {
                if (child.Height.SizeType == UiSizeType.Grow)
                {
                    childGrowCount++;
                }
            }
        }

        bool fillPass = (width && Direction == UiDirection.TopToBottom) ||
                        (!width && Direction == UiDirection.LeftToRight);

        while (remainder > 0 || fillPass)
        {
            if (_children.Count == 0)
                break;
            
            float growValue = remainder / childGrowCount;
            
            foreach (var child in _children)
            {
                if (width)
                {
                    if (child.Width.SizeType == UiSizeType.Grow)
                    {
                        if (Direction == UiDirection.LeftToRight)
                        {
                            if (child.CalculatedWidth + growValue > child.Width.MaxValue)
                            {
                                remainder -= child.Width.MaxValue - child.CalculatedWidth;
                                child.CalculatedWidth = child.Width.MaxValue;
                            }
                            else
                            {
                                child.CalculatedWidth += growValue;
                                remainder -= growValue;
                            }
                        }
                        else
                        {
                            child.CalculatedWidth +=
                                CalculatedInnerWidth - child.CalculatedWidth;
                        }
                    }
                }
                else
                {
                    if (child.Height.SizeType == UiSizeType.Grow)
                    {
                        if (Direction == UiDirection.LeftToRight)
                        {
                            child.CalculatedHeight +=
                                CalculatedInnerHeight - child.CalculatedHeight;
                        }
                        else
                        {
                            if (child.CalculatedHeight + growValue > child.Height.MaxValue)
                            {
                                remainder -= child.Height.MaxValue - child.CalculatedHeight;
                                child.CalculatedHeight = child.Height.MaxValue;
                            }
                            else
                            {
                                child.CalculatedHeight += growValue;
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
        CalulatedPosition = position;
        
        // Get Start Position and Size Without Padding
        Vector2 childStartPosition = new Vector2(
            position.x + PaddingLeft, 
            position.y + PaddingTop);
        Vector2 sizeWithoutPadding = new Vector2(
            CalculatedWidth - PaddingLeft - PaddingRight, 
            CalculatedHeight - PaddingTop - PaddingBottom);

        Vector2 childOffset = Vector2.Zero;

        foreach (var child in _children)
        {
            // Get Alignment Offset
            Vector2 alignmentOffset = Vector2.Zero;

            float horizontalRemainder = Direction == UiDirection.LeftToRight
                ? sizeWithoutPadding.x - ContentWidth
                : sizeWithoutPadding.x - child.CalculatedWidth;

            switch (Alignment.Horizontal)
            {
                case UiAlignmentHorizontal.Center:
                    alignmentOffset.x = horizontalRemainder / 2;
                    break;
                case UiAlignmentHorizontal.Right:
                    alignmentOffset.x = horizontalRemainder;
                    break;
            }
            
            float verticalRemainder = Direction == UiDirection.TopToBottom
                ? sizeWithoutPadding.y - ContentHeight
                : sizeWithoutPadding.y - child.CalculatedHeight;

            switch (Alignment.Vertical)
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
            switch (Direction)
            {
                case UiDirection.LeftToRight:
                    childOffset.x += child.CalculatedWidth + Gap;
                    break; 
                case UiDirection.TopToBottom:
                    childOffset.y += child.CalculatedHeight + Gap;
                    break;
            }
        }
    }

    public void Render(ImRenderPass renderPass, Vector2 renderSize)
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
            Position = new AlignedVector3(CalulatedPosition),
            Size = new Vector2(CalculatedWidth, CalculatedHeight),
            Color = Color,
            BorderRadius = new Vector4(),
            BorderColor = new Vector4(),
            ScreenSize = renderSize,
            BorderSize = 0f
        };
        
        if (Color.a != 0)
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(shader);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        foreach (var child in _children)
        {
            child.Render(renderPass, renderSize);
        }
    }

    public Rect Add(Rect child)
    {
        _children.Add(child);
        child._parent = this;
        return this;
    }
}