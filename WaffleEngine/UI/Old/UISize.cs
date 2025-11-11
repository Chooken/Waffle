namespace WaffleEngine.UI.Old;

public enum UISizeType
{
    Pixels,
    Points,
    PercentageWidth,
    PercentageHeight,
}

public struct UISize
{
    public static float Scale { get; private set; } = 1f;

    public UISizeType Type;
    public float Value;

    public static UISize Pixels(float pixels) => new UISize { Type = UISizeType.Pixels, Value = pixels };
    public static UISize Points(float points) => new UISize { Type = UISizeType.Points, Value = points };
    public static UISize PercentageWidth(float percentage) => new UISize { Type = UISizeType.PercentageWidth, Value = percentage };
    public static UISize PercentageHeight(float percentage) => new UISize { Type = UISizeType.PercentageHeight, Value = percentage };

    public float AsPixels(Vector2 parentSize)
    {
        switch (Type)
        {
            case UISizeType.Pixels:
                return Value * Scale;
            
            case UISizeType.Points:
                return Value * 1.33f * Scale;
            
            case UISizeType.PercentageWidth:
                return Value / 100.0f * parentSize.x;
            
            case UISizeType.PercentageHeight:
                return Value / 100.0f * parentSize.y;
            
            default:
                return Value;
        }
    }

    public static void SetScale(float scale) => Scale = scale;
    
    public static bool operator ==(UISize left, UISize right)
    {
        return left.Value == right.Value && left.Type == right.Type;
    }
    
    public static bool operator !=(UISize left, UISize right)
    {
        return left.Value != right.Value || left.Type != right.Type;
    }
}