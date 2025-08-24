using System.Runtime.InteropServices;
using WaffleEngine.Rendering;

namespace WaffleEngine.UI;

public struct UIRectData
{
    public Vector3 Position;
    public Vector2 Size;
    public Vector4 Color;
    public Vector4 BorderRadius;
    public Vector2 ScreenSize;
}

public class UIRect
{
    private UIRect? _parent;
    private List<UIRect> _uiElements = new List<UIRect>();

    public static Buffer<UIRectData> GpuData = new Buffer<UIRectData>(BufferUsage.GraphicsStorageRead);

    public UISize Width;
    public UISize Height;
    public UISize MarginX;
    public UISize MarginY;
    public UISize PaddingX;
    public UISize PaddingY;
    public UISize BorderRadiusTL;
    public UISize BorderRadiusBL;
    public UISize BorderRadiusTR;
    public UISize BorderRadiusBR;
    public UIDirection ChildDirection;
    public Color Color;
    public bool Dirty;

    public virtual void AddToBuffer(Vector3 position, Vector2 parentSize, Vector2 renderSize)
    {
        var size = new Vector2(
            Width.AsPixels(parentSize.x) - MarginX.AsPixels(parentSize.x) * 2, 
            Height.AsPixels(parentSize.y) - MarginY.AsPixels(parentSize.y) * 2);

        position = new Vector3(
            position.x + MarginX.AsPixels(parentSize.x),
            position.y + MarginY.AsPixels(parentSize.y),
            position.z + 1);
        
        GpuData.Add(new UIRectData()
        {
            Position = position,
            Size = size,
            Color = Color,
            BorderRadius = new Vector4(BorderRadiusTL.Value, BorderRadiusBL.Value, BorderRadiusTR.Value, BorderRadiusBR.Value),
            ScreenSize = renderSize
        });
        
        Dirty = false;

        foreach (var child in _uiElements)
        {
            child.AddToBuffer( 
                new Vector3(position.x + PaddingX.AsPixels(parentSize.x), position.y + PaddingY.AsPixels(parentSize.y), position.z + 1), 
                new Vector2(size.x - PaddingX.AsPixels(parentSize.x) * 2, size.y - PaddingY.AsPixels(parentSize.y) * 2), renderSize);
        }
    }

    public virtual void Update() {}

    public UIRect AddUIElement(UIRect uiRect)
    {
        _uiElements.Add(uiRect);
        Dirty = true;
        return this;
    }

    public UIRect SetWidth(UISize size)
    {
        Width = size;
        Dirty = true;
        return this;
    }

    public UIRect SetHeight(UISize size)
    {
        Height = size;
        Dirty = true;
        return this;
    }

    public UIRect SetMarginX(UISize size)
    {
        MarginX = size;
        Dirty = true;
        return this;
    }

    public UIRect SetMarginY(UISize size)
    {
        MarginY = size;
        Dirty = true;
        return this;
    }

    public UIRect SetPaddingX(UISize size)
    {
        PaddingX = size;
        Dirty = true;
        return this;
    }

    public UIRect SetPaddingY(UISize size)
    {
        PaddingY = size;
        Dirty = true;
        return this;
    }

    public UIRect SetBorderRadius(Vector4 borderRadius, UISizeType type)
    {
        BorderRadiusTL.Value = borderRadius.x;
        BorderRadiusTL.Type = type;
        BorderRadiusBL.Value = borderRadius.y;
        BorderRadiusBL.Type = type;
        BorderRadiusTR.Value = borderRadius.z;
        BorderRadiusTR.Type = type;
        BorderRadiusBR.Value = borderRadius.w;
        BorderRadiusBR.Type = type;
        Dirty = true;
        return this;
    }

    public UIRect SetChildDirection(UIDirection direction)
    {
        ChildDirection = direction;
        Dirty = true;
        return this;
    }

    public UIRect SetColor(Color color)
    {
        Color = color;
        Dirty = true;
        return this;
    }

    public virtual void SetDirty()
    {
        Dirty = true;
        _parent?.SetDirty();
    }
}