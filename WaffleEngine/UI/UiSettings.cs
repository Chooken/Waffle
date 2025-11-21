namespace WaffleEngine.UI;

public struct UiSettings()
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
    /// The aspect ratio of the element. -1 if disabled.
    /// </summary>
    public float AspectRatio = -1;
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
    /// The cursor to show when captured input.
    /// </summary>
    public Cursor Cursor = Cursor.Default;
    
    public static bool operator ==(UiSettings left, UiSettings right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(UiSettings left, UiSettings right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(UiSettings other)
    {
        return
            Width == other.Width &&
            Height == other.Height &&
            Padding == other.Padding &&
            Gap == other.Gap &&
            Direction == other.Direction &&
            Alignment == other.Alignment;
    }
}