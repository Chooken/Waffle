namespace WaffleEngine.UI;

public struct UiBorderRadius
{
    public float TopLeft;
    public float TopRight;
    public float BottomLeft;
    public float BottomRight;

    public UiBorderRadius(float value)
    {
        TopLeft = value;
        TopRight = value;
        BottomLeft = value;
        BottomRight = value;
    }

    public static implicit operator UiBorderRadius(float value) => new UiBorderRadius(value);
    public static implicit operator UiBorderRadius((float topleft, float topright, float bottomleft, float bottomright) value) => new UiBorderRadius
    {
        TopLeft = value.topleft,
        TopRight = value.topright,
        BottomLeft = value.bottomleft,
        BottomRight = value.bottomright,
    };
}