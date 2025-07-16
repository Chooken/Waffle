using System.Numerics;
using WaffleEngine.Rendering;

namespace WaffleEngine.UI;

public struct UIElementData
{
    public Vector3 Position;
    public Vector2 Size;
    public Vector4 Color;
}

public class UIElement
{
    private UIElement? _parent;
    private List<UIElement> _uiElements = new List<UIElement>();

    public static Buffer<UIElementData> GpuData = new Buffer<UIElementData>(BufferUsage.GraphicsStorageRead);

    public UISize Width;
    public UISize Height;
    public UISize MarginX;
    public UISize MarginY;
    public UISize PaddingX;
    public UISize PaddingY;
    public UIDirection ChildDirection;
    public Color Color;
    public bool Dirty;

    public virtual void AddToBuffer(Vector3 position)
    {
        GpuData.Add(new UIElementData()
        {
            Position = position,
            Size = new Vector2(Width.Value, Height.Value),
            Color = Color,
        });
        Dirty = false;

        foreach (var child in _uiElements)
        {
            child.AddToBuffer(position with { Z = position.Z + 1 });
        }
    }

    public virtual void Update() {}

    public UIElement AddUIElement(UIElement uiElement)
    {
        _uiElements.Add(uiElement);
        Dirty = true;
        return this;
    }

    public UIElement SetWidth(UISize size)
    {
        Width = size;
        Dirty = true;
        return this;
    }

    public UIElement SetHeight(UISize size)
    {
        Height = size;
        Dirty = true;
        return this;
    }

    public UIElement SetMarginX(UISize size)
    {
        MarginX = size;
        Dirty = true;
        return this;
    }

    public UIElement SetMarginY(UISize size)
    {
        MarginY = size;
        Dirty = true;
        return this;
    }

    public UIElement SetPaddingX(UISize size)
    {
        PaddingX = size;
        Dirty = true;
        return this;
    }

    public UIElement SetPaddingY(UISize size)
    {
        PaddingY = size;
        Dirty = true;
        return this;
    }

    public UIElement SetChildDirection(UIDirection direction)
    {
        ChildDirection = direction;
        Dirty = true;
        return this;
    }

    public UIElement SetColor(Color color)
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