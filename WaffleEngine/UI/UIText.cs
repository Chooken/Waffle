using SDL3;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace WaffleEngine.UI;

public class UIText : UIRect
{
    private AtlasedText? _text = null;
    public bool TextWrapping;

    public UIText()
    {
        if (FontLoader.TryGetFont("builtin/fonts/Nunito-Regular.ttf", 24, out var font))
        {
            _text = new AtlasedText("", font, WaffleEngine.Color.RGBA255(0, 0, 0, 255));
        }
    }

    public override Vector2 GetSize(Vector2 parentSize)
    {
        Vector2 baseSize = base.GetSize(parentSize);
        Vector2 textSize = _text?.GetSize() ?? Vector2.Zero;

        return TextWrapping ? baseSize : new Vector2(
            MathF.Max(baseSize.x, textSize.x),
            MathF.Max(baseSize.y, textSize.y));
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

        Vector2 size = GetSize(parentSize);
        
        position = new Vector3(
            position.x + Settings.MarginX.AsPixels(parentSize),
            position.y + Settings.MarginY.AsPixels(parentSize),
            position.z + 1);
        
        _text.Render(renderPass, position, renderSize);
        
        Vector2 elementGrow = Settings.Grow ? grow : Vector2.Zero;

        int width = (int)size.x + (int)elementGrow.x;
        
        if (TextWrapping)
            _text.SetWrapWidth((int)size.x);
        
        return _text.GetSize();
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

    public UIText SetTextWrapping(bool wrapping)
    {
        TextWrapping = wrapping;
        SetDirty();
        return this;
    }
}