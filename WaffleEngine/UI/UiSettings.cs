namespace WaffleEngine.UI;

public struct UiSettings()
{
    public UiSizeData Width = Ui.Fit;
    public UiSizeData Height = Ui.Fit;
    public UiPadding Padding;
    public UiDirection Direction = UiDirection.LeftToRight;
    public UiAlignment Alignment;
    public float Gap;
    public UiBorderRadius BorderRadius;
    public float BorderSize;
    public Color BorderColor;
    public Color BackgroundColor;
    public Color Color;
}