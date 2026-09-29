using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Text;

namespace Vector.Editor.UI;

// Non-node text helper. Nodes own a UiText and draw it in OnDraw instead of
// adding text children — child nodes would steal mouse events from their
// parent (INode gives activation to the topmost node under the cursor).
// Silently draws nothing if the font file is missing (e.g. unexpected CWD).
// Sync() uploads glyph geometry (own command buffer) and must be called from
// OnUpdate; Draw() only records into the active render pass.
public sealed class UiText
{
    public const string FontPath = "builtin/fonts/Nunito-Regular.ttf";
    public const int FontSize = 16;

    private static Font? _font;
    private static bool _fontTried;

    private AtlasedText? _text;
    private string _current = "";
    private bool _dirty;
    private Vector2 _size;

    public Vector2 Size => _size;

    private static bool TryFont(out Font? font)
    {
        if (_fontTried)
        {
            font = _font;
            return font is not null;
        }
        _fontTried = true;
        font = FontLoader.TryGetFont(FontPath, FontSize, out var loaded) ? loaded : null;
        _font = font;
        return font is not null;
    }

    public void SetText(string value)
    {
        if (value == _current)
            return;
        _current = value;
        _dirty = true;
    }

    public void Sync()
    {
        if (!_dirty)
            return;
        if (!TryFont(out var font) || font is null)
            return;
        if (_text is null)
            _text = new AtlasedText(_current, font, Theme.Text);
        else
            _text.SetText(_current);
        _text.Update();
        _size = _text.GetSize();
        _dirty = false;
    }

    public void Draw(ImRenderPass renderPass, Vector2 position, Vector2 renderSize,
        Vector2 clipMin, Vector2 clipMax)
    {
        _text?.Render(renderPass, position, renderSize, clipMin, clipMax);
    }
}
