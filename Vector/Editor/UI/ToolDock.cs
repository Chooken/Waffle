using Vector.Editor.Tools;
using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Floating vertical tool dock (Figma-style) living over the canvas.
// Icon-only ghost buttons; the selected tool gets the accent pill.
public class ToolDock : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    public const int DockWidth = 48;
    public const int ToolSize = 40;
    public const int ToolGap = 4;
    public const int DockPad = 4;

    private Button _selectBtn = null!;
    private Button _penBtn = null!;
    private Button _boneBtn = null!;
    private Button _snapBtn = null!;

    public override void OnInit()
    {
        Color = Theme.BgLight;
        BorderRadius = new Vector4(12, 12, 12, 12);
        BorderColor = Theme.Border;
        BorderSize = 1f;
        SetClipped(true);

        _selectBtn = ToolButton(MaterialIcons.Select, () => Editor.ActiveToolMode = ToolMode.Select);
        _penBtn = ToolButton(MaterialIcons.Pen, () => Editor.ActiveToolMode = ToolMode.Pen);
        _boneBtn = ToolButton(MaterialIcons.Bone, () => Editor.ActiveToolMode = ToolMode.Bone);
        _snapBtn = ToolButton(MaterialIcons.Snap, () =>
        {
            Editor.Settings.SnapEnabled = !Editor.Settings.SnapEnabled;
            Editor.Settings.Save();
        });
    }

    private Button ToolButton(string icon, Action onClick)
    {
        var btn = new Button { IconGlyph = icon, Framed = false, OnClick = onClick };
        AddNode(btn);
        return btn;
    }

    public override void OnUpdate()
    {
        // Background live: theme switches apply immediately.
        Color = Theme.BgLight;

        _selectBtn.Selected = Editor.ActiveToolMode == ToolMode.Select;
        _penBtn.Selected = Editor.ActiveToolMode == ToolMode.Pen;
        _boneBtn.Selected = Editor.ActiveToolMode == ToolMode.Bone;
        _snapBtn.Selected = Editor.Settings.SnapEnabled;

        IRect box = Rect.Inset(DockPad);
        float y = box.y;
        _selectBtn.SetRect(new IRect { x = box.x, y = (int)y, w = box.w, h = ToolSize });
        y += ToolSize + ToolGap;
        _penBtn.SetRect(new IRect { x = box.x, y = (int)y, w = box.w, h = ToolSize });
        y += ToolSize + ToolGap;
        _boneBtn.SetRect(new IRect { x = box.x, y = (int)y, w = box.w, h = ToolSize });
        y += ToolSize + ToolGap + 8;
        _snapBtn.SetRect(new IRect { x = box.x, y = (int)y, w = box.w, h = ToolSize });
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        // Divider between tools and the snap toggle.
        if (!Assets.TryGetShader("builtin", "ui-rect", out var shader))
            return;
        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;
        IRect box = Rect.Inset(DockPad);
        float y = box.y + 3 * (ToolSize + ToolGap) + 2;
        renderPass.Bind(shader);
        renderPass.SetUniforms(new Rect.UIRectData
        {
            Position = new AlignedVector3(box.x, y, 0),
            Size = new Vector2(box.w, 1),
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

    public static int DockHeight() => DockPad * 2 + ToolSize * 4 + ToolGap * 3 + 8;
}
