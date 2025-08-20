namespace WaffleEngine.UI;

public enum UISizeType
{
    Pixels,
    Points,
    Percentage
}

public struct UISize
{
    public UISizeType Type;
    public float Value;

    public static UISize Pixels(float pixels) => new UISize { Type = UISizeType.Pixels, Value = pixels };
    public static UISize Points(float points) => new UISize { Type = UISizeType.Points, Value = points };
    public static UISize Percentage(float percentage) => new UISize { Type = UISizeType.Percentage, Value = percentage };

    public float AsPixels(float parentSize)
    {
        switch (Type)
        {
            case UISizeType.Pixels:
                return Value;
            
            case UISizeType.Points:
                return Value;
            
            case UISizeType.Percentage:
                return Value / 100.0f * parentSize;
            
            default:
                return Value;
        }
    }
}