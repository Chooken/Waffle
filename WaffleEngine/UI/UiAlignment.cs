namespace WaffleEngine.UI;

public struct UiAlignment
{
    public UiAlignmentVertical Vertical;
    public UiAlignmentHorizontal Horizontal;
    
    public static bool operator ==(UiAlignment left, UiAlignment right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(UiAlignment left, UiAlignment right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(UiAlignment other)
    {
        return
            Vertical == other.Vertical &&
            Horizontal == other.Horizontal;
    }
}

public enum UiAlignmentVertical
{
    Top,
    Center,
    Bottom
}

public enum UiAlignmentHorizontal
{
    Left,
    Center,
    Right
}