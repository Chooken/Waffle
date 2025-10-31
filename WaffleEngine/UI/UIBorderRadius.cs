namespace WaffleEngine.UI;

public struct UIBorderRadius
{
    public Vector4 Corners;
    public UISizeType Type;

    public UIBorderRadius(float topLeft, float topRight, float bottomLeft, float bottomRight, UISizeType type)
    {
        Corners = new Vector4(bottomLeft, topLeft, bottomRight, topRight);
        Type = type;
    }

    public Vector4 AsPixels(Vector2 parentSize)
    {
        switch (Type)
        {
            case UISizeType.Pixels:
                return Corners * UISize.Scale;
            
            case UISizeType.Points:
                return Corners * 1.33f * UISize.Scale;
            
            case UISizeType.PercentageWidth:
                return Corners / 100.0f * parentSize.x;
            
            case UISizeType.PercentageHeight:
                return Corners / 100.0f * parentSize.y;
            
            default:
                return Corners;
        }
    }
    
    public static bool operator ==(UIBorderRadius left, UIBorderRadius right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(UIBorderRadius left, UIBorderRadius right)
    {
        return !left.Equals(right);
    }

    public bool Equals(UIBorderRadius other)
    {
        return Corners == other.Corners && Type == other.Type;
    }
}