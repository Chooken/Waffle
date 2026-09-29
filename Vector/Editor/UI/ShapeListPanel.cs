using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Layers panel: one row per shape. Rows are rebuilt when the shape list
// changes, laid out top-down, and scrolled with the wheel over the list.
// Fully scrolled-out rows are disabled so they neither draw nor take clicks.
public class ShapeListPanel : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    private string _builtKey = "";
    private int _scroll;
    private int _bonesHeaderY = -1;
    private readonly UiText _header = new() { TextSize = 13 };
    private readonly UiText _bonesHeader = new() { TextSize = 13 };
    private readonly UiText _canvasLabel = new();
    private Button _newBtn = null!;
    private Button _shrinkBtn = null!;
    private Button _growBtn = null!;

    public override void OnInit()
    {
        SetClipped(true);
        _newBtn = new Button
        {
            IconGlyph = MaterialIcons.Add,
            LabelText = "New Layer",
            OnClick = () => Editor.NewShape(),
        };
        _shrinkBtn = new Button { IconGlyph = MaterialIcons.Remove, OnClick = () => Editor.ShrinkCanvas() };
        _growBtn = new Button { IconGlyph = MaterialIcons.Add, OnClick = () => Editor.GrowCanvas() };
        AddNode(_newBtn);
        AddNode(_shrinkBtn);
        AddNode(_growBtn);
    }

    public override void OnUpdate()
    {
        // Background live: theme switches apply immediately.
        Color = Theme.BgLight;
        var shapes = Editor.Document.Shapes;
        var bones = Editor.Document.Bones;
        string key = string.Join("|", shapes.Select(s =>
            $"{s.Id}:{s.Name}:{s.Closed}:{s.AnchorCount}"))
            + "#"
            + string.Join("|", bones.Select(b => $"{b.Id}:{b.Name}:{b.ParentId}"));
        if (key != _builtKey)
        {
            _builtKey = key;
            _scroll = 0;
            foreach (var child in Children.ToArray())
            {
                if (child is ShapeRow || child is BoneRow)
                    RemoveNode(child);
            }
            foreach (var shape in shapes)
                AddNode(new ShapeRow { Editor = Editor, Panel = this, ShapeId = shape.Id });
            foreach (var bone in bones)
                AddNode(new BoneRow { Editor = Editor, BoneId = bone.Id });
        }

        _header.SetText($"LAYERS ({shapes.Count})");
        _header.Sync();
        _bonesHeader.SetText($"BONES ({bones.Count})");
        _bonesHeader.Sync();
        _canvasLabel.SetText($"Canvas {Editor.Document.CanvasSize.x}");
        _canvasLabel.Sync();

        int listTop = Rect.y + Theme.HeaderHeight + 4;
        int listBottom = Rect.y + Rect.h - Theme.FooterHeight;
        int units = Children.OfType<ShapeRow>().Count()
            + Children.OfType<BoneRow>().Count()
            + (bones.Count > 0 ? 1 : 0);
        int contentH = units * (Theme.RowHeight + Theme.RowGap);
        int maxScroll = Math.Max(0, contentH - (listBottom - listTop));

        if (Rect.Contains(Input.Mouse.Position)
            && Input.Mouse.Position.y < listBottom
            && Input.Mouse.MouseWheelTicksDelta != 0)
        {
            _scroll = Math.Clamp(_scroll - Input.Mouse.MouseWheelTicksDelta * 20, 0, maxScroll);
        }
        else
        {
            _scroll = Math.Clamp(_scroll, 0, maxScroll);
        }

        float y = listTop - _scroll;
        // Rows are full-bleed: no inset math here, each row pads its own
        // content once via Theme.PanelPad.
        foreach (var child in Children.OfType<ShapeRow>())
        {
            child.SetRect(new IRect { x = Rect.x, y = (int)y, w = Rect.w, h = Theme.RowHeight });
            // Never disable the armed row mid-gesture: a disabled Active node
            // would never receive its release event and stick forever.
            bool visible = y + Theme.RowHeight > listTop && y < listBottom;
            child.SetEnabled(visible || child.ReorderArmed());
            y += Theme.RowHeight + Theme.RowGap;
        }
        _bonesHeaderY = -1;
        if (Editor.Document.Bones.Count > 0)
        {
            _bonesHeaderY = (int)y;
            y += Theme.RowHeight + Theme.RowGap;
        }
        foreach (var child in Children.OfType<BoneRow>())
        {
            child.SetRect(new IRect { x = Rect.x, y = (int)y, w = Rect.w, h = Theme.RowHeight });
            bool visible = y + Theme.RowHeight > listTop && y < listBottom;
            child.SetEnabled(visible);
            y += Theme.RowHeight + Theme.RowGap;
        }

        float fy = Rect.y + Rect.h - Theme.FooterHeight;
        IRect footer = new IRect(Rect.x, (int)fy, Rect.w, Theme.FooterHeight).Inset(Theme.PanelPad);
        _newBtn.SetRect(new IRect { x = footer.x, y = (int)fy, w = footer.w, h = Theme.ButtonHeight });
        float cy = fy + Theme.ButtonHeight + Theme.RowGap;
        _shrinkBtn.SetRect(new IRect { x = footer.x, y = (int)cy, w = 30, h = Theme.ButtonHeight });
        _growBtn.SetRect(new IRect { x = footer.x + 34, y = (int)cy, w = 30, h = Theme.ButtonHeight });
    }

    // Drop target for the row currently being dragged: how many of the OTHER
    // rows sit above the cursor. Directly usable as a document index.
    public int DropIndexFor(float mouseY)
    {
        int target = 0;
        foreach (var child in Children.OfType<ShapeRow>())
        {
            if (child.ReorderDragging)
                continue;
            if (child.Rect.y + child.Rect.h / 2 < mouseY)
                target++;
        }
        return target;
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);
        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        if (Assets.TryGetShader("builtin", "ui-rect", out var edge))
        {
            renderPass.Bind(edge);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x + Rect.w - 1, Rect.y, 0),
                Size = new Vector2(1, Rect.h),
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

        // Insertion indicator while a row is being dragged.
        bool dragging = false;
        foreach (var child in Children.OfType<ShapeRow>())
        {
            if (child.ReorderDragging)
            {
                dragging = true;
                break;
            }
        }
        if (dragging && Assets.TryGetShader("builtin", "ui-rect", out var lineShader))
        {
            float mouseY = Input.Mouse.Position.y;
            float lineY = Rect.y + Rect.h;
            foreach (var child in Children.OfType<ShapeRow>())
            {
                if (child.ReorderDragging)
                    continue;
                if (child.Rect.y + child.Rect.h / 2 >= mouseY)
                {
                    lineY = child.Rect.y - Theme.RowGap / 2;
                    break;
                }
                lineY = child.Rect.y + child.Rect.h + Theme.RowGap / 2;
            }
            renderPass.Bind(lineShader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x, lineY - 1, 0),
                Size = new Vector2(Rect.w, 2),
                Color = Theme.Accent,
                BorderRadius = Vector4.Zero,
                BorderColor = new Vector4(0, 0, 0, 0),
                ScreenSize = screen,
                BorderSize = 0f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        _header.SetText($"LAYERS ({Editor.Document.Shapes.Count})");
        _header.Sync();
        _header.Draw(renderPass,
            new Vector2(Rect.x, Rect.y + (Theme.HeaderHeight - _header.Size.y) / 2),
            screen, clip.Min, clip.Max);
        if (_bonesHeaderY >= 0)
        {
            _bonesHeader.Draw(renderPass,
                new Vector2(Rect.x, _bonesHeaderY + (Theme.RowHeight - _bonesHeader.Size.y) / 2),
                screen, clip.Min, clip.Max);
        }
        _canvasLabel.SetText($"Canvas {Editor.Document.CanvasSize.x}");
        _canvasLabel.Sync();
        _canvasLabel.Draw(renderPass,
            new Vector2(_growBtn.Rect.x + _growBtn.Rect.w + Theme.RowGap, Rect.y + Rect.h - Theme.FooterHeight + Theme.ButtonHeight + Theme.RowGap + (Theme.ButtonHeight - _canvasLabel.Size.y) / 2),
            screen, clip.Min, clip.Max);
    }
}

// One layer row: color chip + name on the left, delete zone ("X") on the
// right. Click selects; click in the delete zone removes the shape; press
// and drag vertically reorders the layer on release.
public class ShapeRow : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;
    public ShapeListPanel Panel = null!;
    public int ShapeId;

    private const int DeleteW = 30;
    private const float DragThreshold = 10f;
    private readonly UiText _label = new();
    private readonly UiText _labelAccent = new() { TextColorOverride = Color.White };
    private readonly UiText _del = new() { FontFile = MaterialIcons.FontFile, TextSize = 18 };
    private readonly UiText _delAccent = new() { FontFile = MaterialIcons.FontFile, TextSize = 18, TextColorOverride = Color.White };
    private float _pressY;
    private bool _armed;
    private bool _pressDeleteZone;
    private bool _selected;

    public bool ReorderArmed() => _armed;

    public bool ReorderDragging =>
        _armed && Math.Abs(Input.Mouse.Position.y - _pressY) > DragThreshold;

    public override void OnUpdate()
    {
        var shape = Editor.Document.FindShape(ShapeId);
        _selected = Editor.Selection.IsShapeActive(ShapeId);
        Color = _selected
            ? Theme.Accent
            : IsHovered
                ? Theme.Highlight
                : new Color(0, 0, 0, 0);
        BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius);

        string text = shape is null
            ? "?"
            : $"{shape.Name} · {shape.AnchorCount}{(shape.Closed ? "" : " ○")}";
        _label.SetText(text);
        _label.Sync();
        _labelAccent.SetText(text);
        _labelAccent.Sync();
        _del.SetText(MaterialIcons.Close);
        _del.Sync();
        _delAccent.SetText(MaterialIcons.Close);
        _delAccent.Sync();
    }

    public override void OnEvent(NodeEvent node_event)
    {
        if (node_event == NodeEvent.MouseHold)
        {
            if (Input.Mouse.IsLeftPressed && Rect.Contains(Input.Mouse.Position))
            {
                _pressDeleteZone = Input.Mouse.Position.x >= Rect.x + Rect.w - DeleteW;
                _pressY = Input.Mouse.Position.y;
                _armed = !_pressDeleteZone;
                Editor.Selection.SelectShape(ShapeId);
            }
            return;
        }

        if (node_event != NodeEvent.MouseClick)
            return;

        if (_armed && ReorderDragging && Panel.Rect.Contains(Input.Mouse.Position))
            Editor.MoveShapeTo(ShapeId, Panel.DropIndexFor(Input.Mouse.Position.y));
        else if (_pressDeleteZone && Rect.Contains(Input.Mouse.Position)
            && Input.Mouse.Position.x >= Rect.x + Rect.w - DeleteW)
            Editor.DeleteShape(ShapeId);

        _armed = false;
        _pressDeleteZone = false;
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var shape = Editor.Document.FindShape(ShapeId);
        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        if (Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            float chip = Theme.ChipSize;
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(
                    Rect.x + Theme.PanelPad, Rect.y + (Rect.h - chip) / 2, 0),
                Size = new Vector2(chip, chip),
                Color = shape?.Color ?? Theme.TextDim,
                BorderRadius = new Vector4(Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius),
                BorderColor = Theme.Text,
                ScreenSize = screen,
                BorderSize = Theme.ChipBorder,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        var label = _selected ? _labelAccent : _label;
        label.Draw(renderPass,
            new Vector2(Rect.x + Theme.PanelPad + Theme.ChipSize + Theme.ChipGap, Rect.y + (Rect.h - label.Size.y) / 2),
            screen, clip.Min, clip.Max);

        var del = _selected ? _delAccent : _del;
        del.Draw(renderPass,
            new Vector2(Rect.x + Rect.w - DeleteW / 2 - 4, Rect.y + (Rect.h - del.Size.y) / 2),
            screen, clip.Min, clip.Max);
    }
}


// One bone row: joint chip + name (with parent) on the left, delete zone on
// the right. Click selects the bone for the Bone tool.
public class BoneRow : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;
    public int BoneId;

    private const int DeleteW = 30;
    private readonly UiText _label = new();
    private readonly UiText _labelAccent = new() { TextColorOverride = Color.White };
    private readonly UiText _del = new() { FontFile = MaterialIcons.FontFile, TextSize = 18 };
    private readonly UiText _delAccent = new() { FontFile = MaterialIcons.FontFile, TextSize = 18, TextColorOverride = Color.White };
    private bool _selected;

    public override void OnUpdate()
    {
        var bone = Editor.Document.FindBone(BoneId);
        _selected = Editor.Selection.IsBoneActive(BoneId);
        Color = _selected
            ? Theme.Accent
            : IsHovered
                ? Theme.Highlight
                : new Color(0, 0, 0, 0);
        BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius);

        string parent = "";
        if (bone is not null && bone.ParentId >= 0
            && Editor.Document.FindBone(bone.ParentId) is { } parentBone)
        {
            parent = $" ‹ {parentBone.Name}";
        }
        string text = bone is null ? "?" : $"{bone.Name}{parent}";
        _label.SetText(text);
        _label.Sync();
        _labelAccent.SetText(text);
        _labelAccent.Sync();
        _del.SetText(MaterialIcons.Close);
        _del.Sync();
        _delAccent.SetText(MaterialIcons.Close);
        _delAccent.Sync();
    }

    public override void OnEvent(NodeEvent node_event)
    {
        if (node_event != NodeEvent.MouseClick
            || !Rect.Contains(Input.Mouse.Position))
            return;

        if (Input.Mouse.Position.x >= Rect.x + Rect.w - DeleteW)
            Editor.DeleteBone(BoneId);
        else
            Editor.Selection.SelectBone(BoneId);
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        if (Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            float chip = Theme.ChipSize;
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(
                    Rect.x + Theme.PanelPad, Rect.y + (Rect.h - chip) / 2, 0),
                Size = new Vector2(chip, chip),
                Color = Theme.HandleBone,
                BorderRadius = new Vector4(Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius),
                BorderColor = Theme.Text,
                ScreenSize = screen,
                BorderSize = Theme.ChipBorder,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        var label = _selected ? _labelAccent : _label;
        label.Draw(renderPass,
            new Vector2(Rect.x + Theme.PanelPad + Theme.ChipSize + Theme.ChipGap, Rect.y + (Rect.h - label.Size.y) / 2),
            screen, clip.Min, clip.Max);

        var del = _selected ? _delAccent : _del;
        del.Draw(renderPass,
            new Vector2(Rect.x + Rect.w - DeleteW / 2 - 4, Rect.y + (Rect.h - del.Size.y) / 2),
            screen, clip.Min, clip.Max);
    }
}
