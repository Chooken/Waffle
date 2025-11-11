using System.Security.Cryptography;

namespace WaffleEngine.UI;

public struct UiPadding
{
    public float Left;
    public float Right;
    public float Top;
    public float Bottom;

    public float TotalVertical => Top + Bottom;
    public float TotalHorizontal => Left + Right;

    public UiPadding(float padding)
    {
        Left = padding;
        Right = padding;
        Top = padding;
        Bottom = padding;
    }

    public UiPadding(float horizontal, float vertical)
    {
        Left = horizontal;
        Right = horizontal;
        Top = vertical;
        Bottom = vertical;
    }

    public static implicit operator UiPadding(float value) => new UiPadding(value);
    public static implicit operator UiPadding((float x, float y) value) => new UiPadding(value.x, value.y);
    public static implicit operator UiPadding((float left, float right, float top, float bottom) value) => new UiPadding()
    {
        Left = value.left,
        Right = value.right,
        Top = value.top,
        Bottom = value.bottom
    };
}