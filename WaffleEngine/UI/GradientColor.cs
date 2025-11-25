namespace WaffleEngine.UI;

public struct GradientColor
{
    public Color TopLeft;
    public Color TopRight;
    public Color BottomLeft;
    public Color BottomRight;

    public GradientColor(Color color)
    {
        TopLeft = color;
        TopRight = color;
        BottomLeft = color;
        BottomRight = color;
    }
    
    public GradientColor(Color topLeft, Color topRight, Color bottomLeft, Color bottomRight)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    public bool Visible()
    {
        return TopLeft.a != 0 || TopRight.a != 0 || BottomLeft.a != 0 || BottomRight.a != 0;
    }

    public static implicit operator GradientColor(Color color) => new GradientColor(color);
    public static implicit operator GradientColor((Color left, Color right) value) => new GradientColor(value.left, value.right, value.left, value.right);
    
    public static bool operator ==(GradientColor left, GradientColor right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(GradientColor left, GradientColor right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(GradientColor other)
    {
        return
            TopLeft == other.TopLeft &&
            TopRight == other.TopRight &&
            BottomLeft == other.BottomLeft &&
            BottomRight == other.BottomRight;
    }
}