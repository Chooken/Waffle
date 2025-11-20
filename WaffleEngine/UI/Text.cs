using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace WaffleEngine.UI;

public class Text : UiElement
{
    private AtlasedText _atlasedText;
    private Font _font;
    private TextSettings _textSettings;

    public struct TextSettings()
    {
        public Color Color;
        public float Size = 16;
    }
    
    private Func<TextSettings>? _default;

    public Text(string text, Font font)
    {
        _font = font.Copy();
        _atlasedText = new AtlasedText(text, _font, new Color(0, 0, 0, 1));
    }
    
    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        _atlasedText.Render(
            renderPass, 
            Bounds.CalculatedPosition * Bounds.Scale * Window.GetDensity(), 
            renderSize * Bounds.Scale * Window.GetDensity(),
            _textSettings.Color
            );
    }

    public override void Update()
    {
        _textSettings = _default?.Invoke() ?? new TextSettings
        {
            Color = new Color(1,1,1,1),
            Size = 16,
        };
        
        if (_font.Size != _textSettings.Size * Bounds.Scale * Window.GetDensity())
            _font.SetFontSize(_textSettings.Size * Bounds.Scale * Window.GetDensity());
        
        _atlasedText.Update();
        Vector2 size = _atlasedText.GetSize();
        Settings.Width = Ui.Fixed(size.x / Bounds.Scale / Window.GetDensity());
        Settings.Height = Ui.Fixed(size.y / Bounds.Scale / Window.GetDensity());
    }

    public override bool OnHover() { return false; }
    public override bool OnMouseDown() { return false; }
    public override bool OnHold() { return false; }
    public override bool OnMouseUp() { return false; }

    public Text Default(Func<TextSettings> defaultSettings)
    {
        _default += defaultSettings;
        _textSettings = _default.Invoke();
        return this;
    }
}