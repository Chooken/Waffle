using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Clickable rect with optional text label and/or Material icon glyph. Draws
// its own content (see UiText) so no child node steals its mouse events.
// Fires OnClick on release-inside.
public class Button : Rect
{
    public string LabelText = "";
    public string IconGlyph = "";
    public bool Selected;
    public bool Dimmed;
    public bool NewGroup;
    // Frameless icon buttons (toolbar, dock) show their border only on
    // hover/selection; framed buttons always do so they read on any surface.
    public bool Framed = true;
    public Action? OnClick;

    private readonly UiText _label = new();
    private readonly UiText _labelAccent = new() { TextColorOverride = Color.White };
    private readonly UiText _icon = new()
    {
        FontFile = MaterialIcons.FontFile,
        TextSize = MaterialIcons.IconSize,
    };
    private readonly UiText _iconAccent = new()
    {
        FontFile = MaterialIcons.FontFile,
        TextSize = MaterialIcons.IconSize,
        TextColorOverride = Color.White,
    };

    public bool HasIcon => IconGlyph.Length > 0;
    public bool HasLabel => LabelText.Length > 0;

    public float PreferredWidth()
    {
        float w = 20f;
        if (HasIcon)
        {
            _icon.SetText(IconGlyph);
            _icon.Sync();
            w += _icon.Size.x > 0 ? _icon.Size.x + 6f : 0f;
        }
        if (HasLabel)
        {
            _label.SetText(LabelText);
            _label.Sync();
            w += _label.Size.x > 0 ? _label.Size.x : LabelText.Length * 8f;
        }
        return w;
    }

    public override void OnUpdate()
    {
        _label.SetText(LabelText);
        _label.Sync();
        _icon.SetText(IconGlyph);
        _icon.Sync();
        // Ghost by default: transparent base, hairline border, wash on
        // hover, accent fill when selected.
        Color bg = Selected ? Theme.Accent
            : IsHovered ? Theme.Highlight
            : new Color(0, 0, 0, 0);
        if (Dimmed)
            bg = bg.WithAlpha(bg.a * Theme.DisabledAlpha);
        Color = bg;
        BorderColor = Selected ? Theme.Accent : Theme.Border;
        BorderSize = (Framed || Selected) ? 1f : 0f;
        BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius);
    }

    public override void OnEvent(NodeEvent node_event)
    {
        if (!Dimmed && node_event == NodeEvent.MouseClick
            && Rect.Contains(Input.Mouse.Position))
        {
            OnClick?.Invoke();
        }
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        float totalW = 0f;
        float iconW = 0f;
        float labelW = 0f;
        if (HasIcon)
        {
            var icon = Selected ? _iconAccent : _icon;
            icon.SetText(IconGlyph);
            icon.Sync();
            iconW = icon.Size.x;
            totalW += iconW;
        }
        if (HasLabel)
        {
            var label = Selected ? _labelAccent : _label;
            label.SetText(LabelText);
            label.Sync();
            labelW = label.Size.x;
            totalW += (HasIcon ? 6f : 0f) + labelW;
        }

        float x = Rect.x + Math.Max(0, (Rect.w - totalW) / 2);
        float cy = Rect.y + Rect.h / 2;
        if (HasIcon)
        {
            var icon = Selected ? _iconAccent : _icon;
            icon.Draw(renderPass, new Vector2(x, cy - icon.Size.y / 2),
                screen, clip.Min, clip.Max);
            x += iconW + (HasLabel ? 6f : 0f);
        }
        if (HasLabel)
        {
            var label = Selected ? _labelAccent : _label;
            label.Draw(renderPass, new Vector2(x, cy - label.Size.y / 2),
                screen, clip.Min, clip.Max);
        }
    }
}
