namespace WaffleEngine.UI;

public struct UiAlignment
{
    public UiAlignmentVertical Vertical;
    public UiAlignmentHorizontal Horizontal;
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