namespace WaffleEngine.UI;

public enum UISizeType
{
    Pixels,
}

public struct UISize
{
    public UISizeType Type;
    public float Value;

    public static UISize Pixels(float pixels) => new UISize { Type = UISizeType.Pixels, Value = pixels };
}