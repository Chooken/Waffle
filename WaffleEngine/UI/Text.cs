using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace WaffleEngine.UI;

public class Text : UiElement
{
    private AtlasedText _atlasedText;
    private Font _font;
    private float _fontSize;

    public Text(string text, Font font)
    {
        _font = font.Copy();
        _fontSize = _font.Size;
        _atlasedText = new AtlasedText(text, _font, new Color(0, 0, 0, 1));
    }
    
    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        _atlasedText.Render(renderPass, Bounds.CalculatedPosition * Bounds.Scale, renderSize * Bounds.Scale);
    }

    public override void Update()
    {
        if (_font.Size != _fontSize * Bounds.Scale)
            _font.SetFontSize(_fontSize * Bounds.Scale);
        
        _atlasedText.Update();
        Vector2 size = _atlasedText.GetSize();
        Settings.Width = Ui.Fixed(size.x / Bounds.Scale);
        Settings.Height = Ui.Fixed(size.y / Bounds.Scale);
    }

    public override bool OnHover() { return false; }

    public override bool OnClick() { return false; }

    public override bool OnHold() { return false; }
}