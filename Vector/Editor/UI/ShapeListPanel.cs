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
    private readonly UiText _header = new();
    private readonly UiText _canvasLabel = new();
    private Button _shrinkBtn = null!;
    private Button _growBtn = null!;

    public override void OnInit()
    {
        Color = Theme.PanelBackground;
        SetClipped(true);
        _shrinkBtn = new Button { LabelText = "-", OnClick = () => Editor.ShrinkCanvas() };
        _growBtn = new Button { LabelText = "+", OnClick = () => Editor.GrowCanvas() };
        AddNode(_shrinkBtn);
        AddNode(_growBtn);
    }

    public override void OnUpdate()
    {
        var shapes = Editor.Document.Shapes;
        string key = string.Join("|", shapes.Select(s =>
            $"{s.Id}:{s.Name}:{s.Closed}:{s.AnchorCount}"));
        if (key != _builtKey)
        {
            _builtKey = key;
            _scroll = 0;
            foreach (var child in Children.OfType<ShapeRow>().ToArray())
                RemoveNode(child);
            foreach (var shape in shapes)
                AddNode(new ShapeRow { Editor = Editor, Panel = this, ShapeId = shape.Id });
        }

        _header.SetText($"Shapes ({shapes.Count})");
        _header.Sync();
        _canvasLabel.SetText($"Canvas {Editor.Document.CanvasSize.x}");
        _canvasLabel.Sync();

        int rows = Children.OfType<ShapeRow>().Count();
        int listTop = Rect.y + Theme.HeaderHeight + 4;
        int listBottom = Rect.y + Rect.h - Theme.FooterHeight;
        int contentH = rows * (Theme.RowHeight + Theme.RowGap);
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
        foreach (var child in Children.OfType<ShapeRow>())
        {
            child.SetRect(new IRect { x = Rect.x + Theme.PanelPad, y = (int)y, w = Rect.w - Theme.PanelPad * 2, h = Theme.RowHeight });
            // Never disable the armed row mid-gesture: a disabled Active node
            // would never receive its release event and stick forever.
            bool visible = y + Theme.RowHeight > listTop && y < listBottom;
            child.SetEnabled(visible || child.ReorderArmed());
            y += Theme.RowHeight + Theme.RowGap;
        }

        float fy = Rect.y + Rect.h - Theme.FooterHeight;
        _shrinkBtn.SetRect(new IRect { x = Rect.x + Theme.PanelPad, y = (int)fy, w = 30, h = Theme.ButtonHeight });
        _growBtn.SetRect(new IRect { x = Rect.x + Theme.PanelPad + 34, y = (int)fy, w = 30, h = Theme.ButtonHeight });
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
                Position = new AlignedVector3(Rect.x + Theme.PanelPad, lineY - 1, 0),
                Size = new Vector2(Rect.w - Theme.PanelPad * 2, 2),
                Color = Theme.CanvasBorder,
                BorderRadius = Vector4.Zero,
                BorderColor = new Vector4(0, 0, 0, 0),
                ScreenSize = screen,
                BorderSize = 0f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        _header.SetText($"Shapes ({Editor.Document.Shapes.Count})");
        _header.Sync();
        _header.Draw(renderPass,
            new Vector2(Rect.x + 10, Rect.y + (Theme.HeaderHeight - _header.Size.y) / 2),
            screen, clip.Min, clip.Max);
        _canvasLabel.SetText($"Canvas {Editor.Document.CanvasSize.x}");
        _canvasLabel.Sync();
        _canvasLabel.Draw(renderPass,
            new Vector2(Rect.x + 82, Rect.y + Rect.h - Theme.FooterHeight + (Theme.ButtonHeight - _canvasLabel.Size.y) / 2),
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
    private readonly UiText _del = new();
    private float _pressY;
    private bool _armed;
    private bool _pressDeleteZone;

    public bool ReorderArmed() => _armed;

    public bool ReorderDragging =>
        _armed && Math.Abs(Input.Mouse.Position.y - _pressY) > DragThreshold;

    public override void OnUpdate()
    {
        var shape = Editor.Document.FindShape(ShapeId);
        bool selected = Editor.Selection.IsShapeActive(ShapeId);
        Color = selected
            ? Theme.ControlSelected
            : IsHovered
                ? Theme.RowHover
                : Theme.RowNormal;
        BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius);

        _label.SetText(shape is null
            ? "?"
            : $"{shape.Name} · {shape.AnchorCount}{(shape.Closed ? "" : " ○")}");
        _label.Sync();
        _del.SetText("X");
        _del.Sync();
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
            float chip = 14;
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(
                    Rect.x + 10, Rect.y + (Rect.h - chip) / 2, 0),
                Size = new Vector2(chip, chip),
                Color = shape?.Color ?? Theme.ChipFallback,
                BorderRadius = new Vector4(Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius),
                BorderColor = Theme.HandleBorderColor,
                ScreenSize = screen,
                BorderSize = Theme.ChipBorder,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        _label.Draw(renderPass,
            new Vector2(Rect.x + 32, Rect.y + (Rect.h - _label.Size.y) / 2),
            screen, clip.Min, clip.Max);

        var del = _del;
        del.Draw(renderPass,
            new Vector2(Rect.x + Rect.w - DeleteW / 2 - 4, Rect.y + (Rect.h - del.Size.y) / 2),
            screen, clip.Min, clip.Max);
    }
}
