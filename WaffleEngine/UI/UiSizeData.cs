namespace WaffleEngine.UI;

public enum UiSizeType
{
    Fit,
    Grow,
    Percentage,
    Fixed,
}

public struct UiSizeData
{
    public float Value;
    public UiSizeType SizeType;
    public float MinValue;
    public float MaxValue;

    public UiSizeData Min(float value)
    {
        MinValue = value;
        return this;
    }

    public UiSizeData Max(float value)
    {
        MaxValue = value;
        return this;
    }
    
    public static bool operator ==(UiSizeData left, UiSizeData right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(UiSizeData left, UiSizeData right)
    {
        return !left.Equals(right);
    }
    
    public bool Equals(UiSizeData other)
    {
        return
            Value == other.Value &&
            SizeType == other.SizeType &&
            MinValue == other.MinValue &&
            MaxValue == other.MaxValue;
    }
}

public struct Range()
{
    public float Min = float.MinValue;
    public float Max = float.MaxValue;
}

public static partial class Ui
{
    public static UiSizeData Fit => new UiSizeData
    {
        Value = 0,
        SizeType = UiSizeType.Fit,
        MinValue = float.MinValue,
        MaxValue = float.MaxValue,
    };
    
    public static UiSizeData Grow => new UiSizeData
    {
        Value = 0,
        SizeType = UiSizeType.Grow,
        MinValue = float.MinValue,
        MaxValue = float.MaxValue,
    };

    public static UiSizeData Percentage(float value) => new UiSizeData
    {
        Value = value / 100,
        SizeType = UiSizeType.Percentage,
        MinValue = float.MinValue,
        MaxValue = float.MaxValue,
    };
    
    public static UiSizeData Fixed(float value) => new UiSizeData
    {
        Value = value,
        SizeType = UiSizeType.Fixed,
        MinValue = float.MinValue,
        MaxValue = float.MaxValue,
    };
}