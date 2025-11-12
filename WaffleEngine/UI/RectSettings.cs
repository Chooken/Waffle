namespace WaffleEngine.UI;

public struct RectSettings()
{
    /// <summary>
    /// Width of the element.
    /// Default to Ui.Fit.
    /// </summary>
    public UiSizeData Width = Ui.Fit;
    /// <summary>
    /// Height of the element.
    /// Defaults to Ui.Fit.
    /// </summary>
    public UiSizeData Height = Ui.Fit;
    /// <summary>
    /// Padding around the children on the element.
    /// </summary>
    public UiPadding Padding;
    /// <summary>
    /// The direction the children will be laid out.
    /// Defaults to UiDirection.LeftToRight.
    /// </summary>
    public UiDirection Direction = UiDirection.LeftToRight;
    /// <summary>
    /// The position of the children in the parent.
    /// </summary>
    public UiAlignment Alignment;
    /// <summary>
    /// The gap between children.
    /// </summary>
    public float Gap;
    /// <summary>
    /// The radius of the corners of the element.
    /// </summary>
    public UiBorderRadius BorderRadius;
    /// <summary>
    /// The size of the border around the element.
    /// </summary>
    public float BorderSize;
    /// <summary>
    /// The color of the border around the element.
    /// </summary>
    public Color BorderColor;
    /// <summary>
    /// The color of the rect.
    /// </summary>
    public Color Color;
    
    public static bool operator ==(RectSettings left, RectSettings right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(RectSettings left, RectSettings right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(RectSettings other)
    {
        return
            Width == other.Width &&
            Height == other.Height &&
            Padding == other.Padding &&
            BorderRadius == other.BorderRadius &&
            Gap == other.Gap &&
            Direction == other.Direction &&
            Alignment == other.Alignment &&
            Color == other.Color &&
            BorderColor == other.BorderColor &&
            BorderSize == other.BorderSize;
    }

    public void MoveTowards(RectSettings other)
    {
        this = other;
    }

    public UiSettings ToUiSettings() => new UiSettings()
    {
        Width = Width,
        Height = Height,
        Alignment = Alignment,
        Direction = Direction,
        Gap = Gap,
        Padding = Padding,
    };
}