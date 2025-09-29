namespace WaffleEngine.UI;

public struct UIAnchor
{
    private Vector2 _position;

    public Vector2 Position => _position;

    public Vector2 AsOneToMinusOne => -(_position * 2 - 1);

    public static UIAnchor TopLeft => new UIAnchor() { _position = new Vector2(0, 0) };
    public static UIAnchor Top => new UIAnchor() { _position = new Vector2(0.5f, 0) };
    public static UIAnchor TopRight => new UIAnchor() { _position = new Vector2(1f, 0) };
    public static UIAnchor CenterLeft => new UIAnchor() { _position = new Vector2(0f, 0.5f) };
    public static UIAnchor Center => new UIAnchor() { _position = new Vector2(0.5f, 0.5f) };
    public static UIAnchor CenterRight => new UIAnchor() { _position = new Vector2(1, 0.5f) };
    public static UIAnchor BottomLeft => new UIAnchor() { _position = new Vector2(0, 1f) };
    public static UIAnchor Bottom => new UIAnchor() { _position = new Vector2(0.5f, 1f) };
    public static UIAnchor BottomRight => new UIAnchor() { _position = new Vector2(1, 1f) };
}