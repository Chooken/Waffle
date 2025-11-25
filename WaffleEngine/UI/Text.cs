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
        public string? Text;
        public Color Color = Color.White;
        public float Size = 16;
    }
    
    private Func<TextSettings>? _default;

    public Text(Font font)
    {
        _font = font.Copy();
        _atlasedText = new AtlasedText("", _font, new Color(0, 0, 0, 1));
    }
    
    public override void Render(ImRenderPass renderPass, Vector2 renderSize)
    {
        if (_textSettings.Text is null)
            return;
        
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
        
        if (_textSettings.Text is null)
            return;
        
        if (_font.Size != _textSettings.Size * Bounds.Scale * Window.GetDensity())
            _font.SetFontSize(_textSettings.Size * Bounds.Scale * Window.GetDensity());
        
        _atlasedText.SetText(_textSettings.Text);
        
        _atlasedText.Update();
        Vector2 size = _atlasedText.GetSize();
        Settings.Width = Ui.Pixels(size.x / Bounds.Scale / Window.GetDensity());
        Settings.Height = Ui.Pixels(size.y / Bounds.Scale / Window.GetDensity());
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