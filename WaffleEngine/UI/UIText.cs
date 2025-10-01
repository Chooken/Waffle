using SDL3;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace WaffleEngine.UI;

public class UIText : UIRect
{
    private AtlasedText _text;

    public UIText()
    {
        Font font = FontLoader.GetFont("Fonts/Nunito-Regular.ttf", 24);

        _text = new AtlasedText("", font, Color.RGBA255(0,0,0,255));
    }

    public override void Update()
    {
        _text.Update();
        base.Update();
    }

    public override Vector2 Render(ImQueue queue, ImRenderPass renderPass, Vector3 position, UIAnchor anchor, Vector2 parentSize,
        Vector2 renderSize)
    {
        position = new Vector3(
            position.x + MarginX.AsPixels(parentSize.x),
            position.y + MarginY.AsPixels(parentSize.y),
            position.z + 1);
        
        _text.Render(renderPass, position, renderSize);

        int width = (int)GetSize(parentSize).x;
        
        if (width > 0)
            _text.SetWrapWidth((int)GetSize(parentSize).x);
        
        return new Vector2(MathF.Max(28 + MarginX.AsPixels(parentSize.x) * 2, parentSize.x), parentSize.y);
    }

    public UIText SetText(string text)
    {
        _text.SetText(text);
        SetDirty();
        return this;
    }

    public UIText SetTextColor(Color color)
    {
        _text.SetColor(color);
        SetDirty();
        return this;
    }
}