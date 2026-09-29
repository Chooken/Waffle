using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Clickable labeled rect. Draws its own text (see UiText) so no child node
// steals its mouse events. Fires OnClick on release-inside.
public class Button : Rect
{
    public string LabelText = "";
    public bool Selected;
    public bool Dimmed;
    public bool NewGroup;
    public Action? OnClick;

    private readonly UiText _label = new();

    public float PreferredWidth()
    {
        _label.SetText(LabelText);
        _label.Sync();
        float textW = _label.Size.x > 0 ? _label.Size.x : LabelText.Length * 8f;
        return textW + 20f;
    }

    public override void OnUpdate()
    {
        _label.SetText(LabelText);
        _label.Sync();
        Color bg = Selected ? Theme.ControlSelected : IsHovered ? Theme.ControlHover : Theme.ControlNormal;
        if (Dimmed)
            bg = bg.WithAlpha(bg.a * Theme.DisabledAlpha);
        Color = bg;
        BorderColor = Selected ? Theme.ControlBorderColor : new Color(0, 0, 0, 0);
        BorderSize = Selected ? Theme.ControlBorderWidth : 0f;
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

        _label.SetText(LabelText);
        _label.Sync();
        Vector2 size = _label.Size;
        var pos = new Vector2(
            Rect.x + Math.Max(0, (Rect.w - size.x) / 2),
            Rect.y + Math.Max(0, (Rect.h - size.y) / 2));
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;
        _label.Draw(renderPass, pos, new Vector2(screenSize.w, screenSize.h),
            clip.Min, clip.Max);
    }
}
