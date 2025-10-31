using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using WaffleEngine.Rendering;

namespace WaffleEngine.UI;

public struct UISettings()
{
    public UISize Width;
    public UISize Height;
    public UISize MarginX;
    public UISize MarginY;
    public UISize PaddingX;
    public UISize PaddingY;
    public UIBorderRadius BorderRadius;
    public UISize Gap;
    public UIDirection ChildDirection = UIDirection.Right;
    public UIAnchor ChildAnchor;
    public Color Color;
    public Color BorderColor;
    public UISize BorderSize;
    public Color TextColor;
    public GpuTexture? Texture;

    public static bool operator ==(UISettings left, UISettings right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(UISettings left, UISettings right)
    {
        return !left.Equals(right);
    }

    public bool Equals(UISettings other)
    {
        return 
            Width == other.Width &&
            Height == other.Height &&
            MarginX == other.MarginX &&
            MarginY == other.MarginY &&
            PaddingX == other.PaddingX &&
            PaddingY == other.PaddingY &&
            BorderRadius == other.BorderRadius &&
            Gap == other.Gap &&
            ChildDirection == other.ChildDirection &&
            ChildAnchor == other.ChildAnchor &&
            Color == other.Color &&
            BorderColor == other.BorderColor &&
            BorderSize == other.BorderSize &&
            TextColor == other.TextColor &&
            ReferenceEquals(Texture, other.Texture);
    }
}