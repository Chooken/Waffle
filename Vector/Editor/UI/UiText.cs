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

    private static readonly Dictionary<string, Font?> _fonts = new();

    private AtlasedText? _text;
    private string _current = "";
    private bool _dirty = true;
    private int _gen = -1;
    private int _builtSize = -1;
    private string _builtFont = "";
    private Vector2 _size;

    // Font file for this label (UiText.FontPath default, MaterialIcons.Font
    // for glyphs). Point size for this label (headers use smaller type).
    public string FontFile = FontPath;
    public int TextSize = FontSize;

    // Fixed color, ignoring theme switches. Null follows Theme.Text.
    public Color? TextColorOverride;

    public Vector2 Size => _size;

    private static bool TryFont(string path, int size, out Font? font)
    {
        string key = $"{path}_{size}";
        if (_fonts.TryGetValue(key, out font))
            return font is not null;
        font = FontLoader.TryGetFont(path, size, out var loaded) ? loaded : null;
        _fonts[key] = font;
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
        // Theme switches bake a new text color; rebuild then (old native
        // object is dropped — switches are rare). Fixed overrides skip this.
        if (TextColorOverride is null && _gen != Theme.Generation)
        {
            _gen = Theme.Generation;
            _text = null;
            _dirty = true;
        }
        if (TextSize != _builtSize || FontFile != _builtFont)
        {
            _text = null;
            _dirty = true;
            _builtSize = TextSize;
            _builtFont = FontFile;
        }
        if (!_dirty)
            return;
        // Empty strings have no geometry: stay textless instead of building
        // an empty native object (which also logs every time).
        if (_current.Length == 0)
        {
            _text = null;
            _size = Vector2.Zero;
            _dirty = false;
            return;
        }
        if (!TryFont(FontFile, TextSize, out var font) || font is null)
            return;
        Color color = TextColorOverride ?? Theme.Text;
        if (_text is null)
            _text = new AtlasedText(_current, font, color);
        else
        {
            _text.SetText(_current);
            _text.SetColor(color);
        }
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
