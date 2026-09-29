using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Unified toolbar: centered title, icon actions right. Tools live in the
// floating dock; shape actions in the sidebar/inspector.
public class Toolbar : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    private readonly List<Button> _actions = new();
    private Button _undoBtn = null!;
    private Button _redoBtn = null!;
    private Button _saveBtn = null!;
    private readonly UiText _title = new();

    public override void OnInit()
    {
        SetClipped(true);

        _undoBtn = Add(MaterialIcons.Undo, null, () => Editor.Undo(), newGroup: true);
        _redoBtn = Add(MaterialIcons.Redo, null, () => Editor.Redo());
        _saveBtn = Add(MaterialIcons.Save, null, () => Editor.Save());
        Add(MaterialIcons.Settings, null, () => Editor.Popup?.Toggle());
    }

    private Button Add(string icon, string? label, Action onClick, bool newGroup = false)
    {
        var btn = new Button { IconGlyph = icon, Framed = false, OnClick = onClick, NewGroup = newGroup };
        if (label is not null)
            btn.LabelText = label;
        AddNode(btn);
        _actions.Add(btn);
        return btn;
    }

    public override void OnUpdate()
    {
        // Background live: theme switches apply immediately.
        Color = Theme.BgLight;

        _undoBtn.Dimmed = !Editor.History.CanUndo;
        _redoBtn.Dimmed = !Editor.History.CanRedo;
        _saveBtn.Selected = Editor.History.IsDirty;

        bool dirty = Editor.History.IsDirty;
        _title.SetText(dirty ? "Vector — Unsaved" : "Vector");
        _title.Sync();

        // Right-aligned icon actions (fixed 36px slots) inside one inset box.
        IRect box = Rect.Inset(Theme.PanelPad);
        float x = box.x + box.w;
        foreach (var btn in ((IEnumerable<Button>)_actions).Reverse())
        {
            x -= 36;
            btn.SetRect(new IRect { x = (int)x, y = (int)(Rect.y + (Rect.h - 36) / 2), w = 36, h = 36 });
            x -= Theme.ButtonGap + (btn.NewGroup ? Theme.GroupGap : 0);
        }
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        // Centered title — skipped when the actions would collide with it.
        float actionsLeft = Rect.x + Rect.w;
        foreach (var btn in _actions)
            actionsLeft = Math.Min(actionsLeft, (float)btn.Rect.x);
        float halfTitle = _title.Size.x / 2 + 12;
        float cx = Rect.x + Rect.w / 2;
        if (cx - halfTitle > Rect.x + 12 && cx + halfTitle < actionsLeft)
        {
            _title.Draw(renderPass,
                new Vector2(cx - _title.Size.x / 2, Rect.y + (Rect.h - _title.Size.y) / 2),
                screen, clip.Min, clip.Max);

            if (Editor.History.IsDirty && Assets.TryGetShader("builtin", "ui-rect", out var dot))
            {
                renderPass.Bind(dot);
                renderPass.SetUniforms(new Rect.UIRectData
                {
                    Position = new AlignedVector3(cx + _title.Size.x / 2 + 6, Rect.y + Rect.h / 2 - 3, 0),
                    Size = new Vector2(7, 7),
                    Color = Theme.Accent,
                    BorderRadius = new Vector4(4, 4, 4, 4),
                    BorderColor = new Vector4(0, 0, 0, 0),
                    ScreenSize = screen,
                    BorderSize = 0f,
                    ClipMin = clip.Min,
                    ClipMax = clip.Max,
                });
                renderPass.DrawPrimatives(6, 1, 0, 0);
            }
        }

        // Hairline separator under the bar.
        if (Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x, Rect.y + Rect.h - 1, 0),
                Size = new Vector2(Rect.w, 1),
                Color = Theme.Border,
                BorderRadius = Vector4.Zero,
                BorderColor = new Vector4(0, 0, 0, 0),
                ScreenSize = screen,
                BorderSize = 0f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }
    }
}
