using SDL3;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace WaffleEngine.UI;

public class UIText : UIRect
{
    private AtlasedText? _text = null;

    public UIText()
    {
        if (FontLoader.TryGetFont("builtin/fonts/Nunito-Regular.ttf", 24, out var font))
        {
            _text = new AtlasedText("", font, WaffleEngine.Color.RGBA255(0, 0, 0, 255));
        }
    }

    public override void Update()
    {
        _text?.Update();
    }

    public override Vector2 Render(ImQueue queue, ImRenderPass renderPass, Vector3 position, Vector2 parentSize,
        Vector2 grow,
        Vector2 renderSize)
    {
        if (_text is null)
        {
            return Vector2.Zero;
        }
        
        position = new Vector3(
            position.x + Settings.MarginX.AsPixels(parentSize),
            position.y + Settings.MarginY.AsPixels(parentSize),
            position.z + 1);
        
        _text.Render(renderPass, position, renderSize);

        int width = (int)GetSize(parentSize).x;
        
        if (width > 0)
            _text.SetWrapWidth((int)GetSize(parentSize).x);
        
        return new Vector2(MathF.Max(28 + Settings.MarginX.AsPixels(parentSize) * 2, parentSize.x), parentSize.y);
    }

    public UIText SetText(string text)
    {
        _text?.SetText(text);
        SetDirty();
        return this;
    }

    public UIText SetFont(Font font)
    {
        _text?.SetFont(font);
        SetDirty();
        return this;
    }

    public UIText SetTextColor(Color color)
    {
        _text?.SetColor(color);
        SetDirty();
        return this;
    }
}