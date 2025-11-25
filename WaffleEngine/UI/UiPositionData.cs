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
    public float AnchorX;
    public float AnchorY;

    public UiPositionData Left(float value)
    {
        X = value;
        AnchorX = 0;
        return this;
    }
    
    public UiPositionData Right(float value)
    {
        X = -value;
        AnchorX = 1;
        return this;
    }
    
    public UiPositionData Top(float value)
    {
        Y = value;
        AnchorY = 0;
        return this;
    }
    
    public UiPositionData Bottom(float value)
    {
        Y = -value;
        AnchorY = 1;
        return this;
    }
    
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

    public static UiPositionData Offset => new UiPositionData
    {
        Type = UiPositionType.Offset,
    };
    
    public static UiPositionData Relative => new UiPositionData
    {
        Type = UiPositionType.Relative,
    };
    
    public static UiPositionData Fixed => new UiPositionData
    {
        Type = UiPositionType.Fixed,
    };
}