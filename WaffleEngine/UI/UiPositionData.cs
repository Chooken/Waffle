namespace WaffleEngine.UI;

public enum UiPositionType
{
    None,
    Offset,
    Relative,
    Fixed,
}

public struct UiPositionData
{
    public UiPositionType Type;
    public float X;
    public float Y;
    
    public static bool operator ==(UiPositionData left, UiPositionData right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(UiPositionData left, UiPositionData right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(UiPositionData other)
    {
        return
            Type == other.Type &&
            X == other.X &&
            Y == other.Y;
    }
}

public static partial class Ui
{
    public static UiPositionData None => new UiPositionData();

    public static UiPositionData Offset(float xOffset, float yOffset) => new UiPositionData
    {
        Type = UiPositionType.Offset,
        X = xOffset,
        Y = yOffset,
    };
    
    public static UiPositionData Relative(float xPercentage, float yPercentage) => new UiPositionData
    {
        Type = UiPositionType.Relative,
        X = xPercentage,
        Y = yPercentage,
    };
    
    public static UiPositionData Fixed(float x, float y) => new UiPositionData
    {
        Type = UiPositionType.Fixed,
        X = x,
        Y = y,
    };
}