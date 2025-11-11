namespace WaffleEngine.UI;

public enum UiSizeType
{
    Fit,
    Grow,
    Fixed,
}

public struct UiSizeData
{
    public float Value;
    public UiSizeType SizeType;
    public float Min;
    public float Max;
}

public struct Range()
{
    public float Min = float.MinValue;
    public float Max = float.MaxValue;
}

public static partial class Ui
{
    public static UiSizeData Fit() => new UiSizeData
    {
        Value = 0,
        SizeType = UiSizeType.Fit,
        Min = float.MinValue,
        Max = float.MaxValue,
    };
    
    public static UiSizeData Fit(Range range) => new UiSizeData
    {
        Value = 0,
        SizeType = UiSizeType.Fit,
        Min = range.Min,
        Max = range.Max,
    };
    
    public static UiSizeData Grow() => new UiSizeData
    {
        Value = 0,
        SizeType = UiSizeType.Grow,
        Min = float.MinValue,
        Max = float.MaxValue,
    };
    
    public static UiSizeData Grow(Range range) => new UiSizeData
    {
        Value = 0,
        SizeType = UiSizeType.Grow,
        Min = range.Min,
        Max = range.Max,
    };
    
    public static UiSizeData Fixed(float value) => new UiSizeData
    {
        Value = value,
        SizeType = UiSizeType.Fixed,
        Min = float.MinValue,
        Max = float.MaxValue,
    };
}