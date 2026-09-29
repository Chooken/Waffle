using WaffleEngine;
using WaffleEngine.Vector;

namespace Vector.Editor.Tools;

public enum ToolMode
{
    Select,
    Pen,
    Bone,
}

public sealed class ToolContext
{
    public VectorAssetDocument Document = null!;
    public ViewportCamera Camera = null!;
    public EditorSelection Selection = null!;
    public EditorHistory History = null!;
    public Vector2 ViewportCenter;
    public float BaseSize;
    public bool SnapEnabled = true;

    // Marquee overlay (screen space); drawn by the viewport while active.
    public bool MarqueeActive;
    public Vector2 MarqueeStartScreen;
    public Vector2 MarqueeEndScreen;

    public Dictionary<int, RigidPose> BoneWorlds() =>
        Skinning.ComputeWorlds(Document.Bones);
}

public interface IEditorTool
{
    ToolMode Mode { get; }
    // Returns true if the event was consumed and a checkpoint is needed handled internally.
    void OnPress(Vector2 mouseScreen, Vector2 mouseWorld);
    void OnDrag(Vector2 mouseScreen, Vector2 mouseWorld, Vector2 grabWorld);
    void OnRelease();
    void OnKeyShortcut(Keycode key);
}

public sealed class SelectTool : IEditorTool
{
    public ToolMode Mode => ToolMode.Select;

    private enum DragMode
    {
        None,
        Points,
        Shape,
        Marquee,
    }

    private readonly struct Grab
    {
        public readonly Shape Shape;
        public readonly int Index;
        public readonly Vector2 Start;

        public Grab(Shape shape, int index, Vector2 start)
        {
            Shape = shape;
            Index = index;
            Start = start;
        }
    }

    private readonly ToolContext _ctx;
    private DragMode _mode = DragMode.None;
    private Vector2 _pressWorld;
    private Vector2 _pressScreen;
    private bool _moved;
    private bool _marqueeUnion;
    private bool _collapsePending;
    private PointSelection _pressedSelection = PointSelection.None;
    private readonly List<Grab> _grabs = new();
    private bool _checkpointTaken;

    public SelectTool(ToolContext ctx) => _ctx = ctx;

    private static bool ShiftHeld()
    {
        var keys = Input.GetDefaultEventSpace;
        return keys.KeyDown(Keycode.LeftShift) || keys.KeyDown(Keycode.RightShift);
    }

    public void OnPress(Vector2 mouseScreen, Vector2 mouseWorld)
    {
        _mode = DragMode.None;
        _grabs.Clear();
        _checkpointTaken = false;
        _moved = false;
        _collapsePending = false;
        _pressedSelection = PointSelection.None;
        _pressWorld = mouseWorld;
        _pressScreen = mouseScreen;
        _ctx.MarqueeActive = false;

        float tolWorld = 10f / MathF.Max(1f, _ctx.Camera.PixelsPerWorldUnit(_ctx.BaseSize));

        Shape? anchorShape = null;
        int anchorIndex = -1;
        float anchorDist = tolWorld;
        Shape? controlShape = null;
        int controlIndex = -1;
        float controlDist = tolWorld;

        foreach (var shape in _ctx.Document.Shapes)
        {
            var pts = shape.Curve.Points;
            for (int i = 0; i < pts.Count; i++)
            {
                float d = (pts[i].Position - mouseWorld).Length();
                if (i % 2 == 0)
                {
                    if (d < anchorDist)
                    {
                        anchorDist = d;
                        anchorShape = shape;
                        anchorIndex = i;
                    }
                }
                else if (d < controlDist)
                {
                    controlDist = d;
                    controlShape = shape;
                    controlIndex = i;
                }
            }
        }

        // Anchors win ties so grabs stay stable where handles overlap.
        bool useAnchor = anchorShape is not null
            && (controlShape is null || anchorDist <= controlDist);
        var hitShape = useAnchor ? anchorShape : controlShape;
        int hitIndex = useAnchor ? anchorIndex : controlIndex;

        bool shift = ShiftHeld();

        if (hitShape is not null && hitIndex >= 0)
        {
            var kind = useAnchor ? HandleKind.Anchor : HandleKind.Control;
            if (shift)
            {
                // Toggle membership, no drag.
                _ctx.Selection.TogglePoint(hitShape.Id, hitIndex, kind);
                return;
            }
            if (_ctx.Selection.IsPointActive(hitShape.Id, hitIndex)
                && _ctx.Selection.Points.Count > 1)
            {
                // Pressed a member of a multi-selection: drag it all,
                // collapse to just this point if released without moving.
                _collapsePending = true;
                _pressedSelection = new PointSelection
                {
                    ShapeId = hitShape.Id, PointIndex = hitIndex, Handle = kind,
                };
            }
            else
            {
                _ctx.Selection.SelectPoint(hitShape.Id, hitIndex, kind);
            }
            _mode = DragMode.Points;
            return;
        }

        if (shift)
        {
            BeginMarquee(mouseScreen, union: true);
            return;
        }

        // Whole-curve drag when pressing the filled area / stroke, front
        // shape first (document order matches the layers panel, top = front).
        foreach (var shape in _ctx.Document.Shapes)
        {
            if (shape.HitTest(mouseWorld, tolWorld))
            {
                _ctx.Selection.SelectShape(shape.Id);
                _mode = DragMode.Shape;
                return;
            }
        }

        BeginMarquee(mouseScreen, union: false);
    }

    private void BeginMarquee(Vector2 mouseScreen, bool union)
    {
        _mode = DragMode.Marquee;
        _marqueeUnion = union;
        _ctx.MarqueeActive = true;
        _ctx.MarqueeStartScreen = mouseScreen;
        _ctx.MarqueeEndScreen = mouseScreen;
    }

    public void OnDrag(Vector2 mouseScreen, Vector2 mouseWorld, Vector2 grabWorld)
    {
        if ((mouseScreen - _pressScreen).Length() > 4f)
            _moved = true;

        switch (_mode)
        {
            case DragMode.Points:
            case DragMode.Shape:
                DragGrabs(mouseWorld);
                break;
            case DragMode.Marquee:
                _ctx.MarqueeEndScreen = mouseScreen;
                if (_moved)
                    ApplyMarquee();
                break;
            case DragMode.None:
                break;
        }
    }

    private void DragGrabs(Vector2 mouseWorld)
    {
        if (!_checkpointTaken)
        {
            _ctx.History.Checkpoint(_ctx.Document);
            _checkpointTaken = true;

            _grabs.Clear();
            if (_mode == DragMode.Shape)
            {
                var shape = _ctx.Document.FindShape(_ctx.Selection.ActiveShapeId);
                if (shape is null)
                    return;
                for (int i = 0; i < shape.Curve.Points.Count; i++)
                    _grabs.Add(new Grab(shape, i, shape.Curve.Points[i].Position));
            }
            else
            {
                foreach (var sel in _ctx.Selection.Points)
                {
                    var shape = _ctx.Document.FindShape(sel.ShapeId);
                    if (shape is not null && sel.PointIndex < shape.Curve.Points.Count)
                        _grabs.Add(new Grab(shape, sel.PointIndex,
                            shape.Curve.Points[sel.PointIndex].Position));
                }
            }
        }

        if (_grabs.Count == 0)
            return;

        // Rigid quantized translation: snap the delta, not each point.
        Vector2 delta = mouseWorld - _pressWorld;
        if (_ctx.SnapEnabled)
        {
            Vector2 snappedMouse = _ctx.Camera.SnapToPixel(
                _pressWorld + delta, _ctx.BaseSize, _ctx.Document.CanvasSize.x);
            Vector2 snappedPress = _ctx.Camera.SnapToPixel(
                _pressWorld, _ctx.BaseSize, _ctx.Document.CanvasSize.x);
            delta = snappedMouse - snappedPress;
        }

        foreach (var grab in _grabs)
            grab.Shape.SetPoint(grab.Index, grab.Start + delta);
    }

    private void ApplyMarquee()
    {
        Vector2 a = _ctx.Camera.ScreenToWorld(
            _ctx.MarqueeStartScreen, _ctx.ViewportCenter, _ctx.BaseSize);
        Vector2 b = _ctx.Camera.ScreenToWorld(
            _ctx.MarqueeEndScreen, _ctx.ViewportCenter, _ctx.BaseSize);
        Vector2 min = new(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        Vector2 max = new(Math.Max(a.x, b.x), Math.Max(a.y, b.y));

        if (!_marqueeUnion)
            _ctx.Selection.ClearPoints();

        foreach (var shape in _ctx.Document.Shapes)
        {
            var pts = shape.Curve.Points;
            for (int i = 0; i < pts.Count; i++)
            {
                Vector2 p = pts[i].Position;
                if (p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y)
                {
                    _ctx.Selection.AddPoint(shape.Id, i,
                        i % 2 == 0 ? HandleKind.Anchor : HandleKind.Control);
                }
            }
        }
        if (!_marqueeUnion && _ctx.Selection.Points.Count == 0)
            _ctx.Selection.Clear();
    }

    public void OnRelease()
    {
        if (_mode == DragMode.Marquee)
        {
            if (!_moved)
                _ctx.Selection.Clear();
            _ctx.MarqueeActive = false;
        }
        else if (_mode == DragMode.Points && _collapsePending && !_moved)
        {
            _ctx.Selection.SelectPoint(
                _pressedSelection.ShapeId,
                _pressedSelection.PointIndex,
                _pressedSelection.Handle);
        }
        _mode = DragMode.None;
    }

    public void OnKeyShortcut(Keycode key)
    {
        if ((key == Keycode.Delete || key == Keycode.Backspace)
            && _ctx.Selection.ActivePoint.HasValue)
        {
            // Delete every selected anchor, highest index first per shape so
            // earlier removals never invalidate later ones.
            var byShape = new Dictionary<int, List<int>>();
            foreach (var sel in _ctx.Selection.Points)
            {
                if (sel.Handle != HandleKind.Anchor)
                    continue;
                if (!byShape.TryGetValue(sel.ShapeId, out var list))
                {
                    list = new List<int>();
                    byShape[sel.ShapeId] = list;
                }
                list.Add(sel.PointIndex);
            }
            if (byShape.Count == 0)
                return;

            int primaryShape = _ctx.Selection.ActivePoint.ShapeId;
            _ctx.History.Checkpoint(_ctx.Document);
            foreach (var (shapeId, indices) in byShape)
            {
                var shape = _ctx.Document.FindShape(shapeId);
                if (shape is null)
                    continue;
                indices.Sort((x, y) => y.CompareTo(x));
                foreach (int index in indices)
                    shape.RemoveAnchor(index);
            }
            _ctx.Selection.ClearPoints();
            if (_ctx.Document.FindShape(primaryShape) is not null)
                _ctx.Selection.SelectShape(primaryShape);
            else
                _ctx.Selection.Clear();
        }
    }
}

public sealed class PenTool : IEditorTool
{
    public ToolMode Mode => ToolMode.Pen;
    private readonly ToolContext _ctx;

    public PenTool(ToolContext ctx) => _ctx = ctx;

    public void OnPress(Vector2 mouseScreen, Vector2 mouseWorld)
    {
        Vector2 target = mouseWorld;
        if (_ctx.SnapEnabled)
            target = _ctx.Camera.SnapToPixel(target, _ctx.BaseSize, _ctx.Document.CanvasSize.x);

        Shape? shape = _ctx.Document.FindShape(_ctx.Selection.ActiveShapeId);
        var boneWorlds = _ctx.BoneWorlds();
        if (shape is null || shape.Closed)
        {
            _ctx.History.Checkpoint(_ctx.Document);
            shape = new Shape
            {
                Id = _ctx.Document.NextShapeId++,
                Name = $"Shape {_ctx.Document.Shapes.Count + 1}",
                Closed = false,
                Curve = new Curve(new Point(target)),
            };
            shape.SyncNewPoints(_ctx.Document.Bones, boneWorlds);
            // New layers go on top (front) of the stack.
            _ctx.Document.Shapes.Insert(0, shape);
            _ctx.Selection.SelectPoint(shape.Id, 0, HandleKind.Anchor);
            return;
        }

        _ctx.History.Checkpoint(_ctx.Document);

        var pts = shape.Curve.Points;

        // Click near first point of open shape -> close it linearly.
        if (shape.AnchorCount >= 3)
        {
            float tolWorld = 10f / MathF.Max(1f, _ctx.Camera.PixelsPerWorldUnit(_ctx.BaseSize));
            if ((pts[0].Position - target).Length() < tolWorld)
            {
                shape.CloseLoop(_ctx.Document.Bones, boneWorlds);
                shape.Closed = true;
                return;
            }
        }

        shape.AddCurvePoint(target, _ctx.Document.Bones, boneWorlds);
        _ctx.Selection.SelectPoint(shape.Id, shape.Curve.Points.Count - 1, HandleKind.Anchor);
    }

    public void OnDrag(Vector2 mouseScreen, Vector2 mouseWorld, Vector2 grabWorld) { }
    public void OnRelease() { }

    public void OnKeyShortcut(Keycode key)
    {
        if (key == Keycode.Return)
        {
            var shape = _ctx.Document.FindShape(_ctx.Selection.ActiveShapeId);
            if (shape is not null && !shape.Closed && shape.AnchorCount >= 3)
            {
                _ctx.History.Checkpoint(_ctx.Document);
                shape.CloseLoop(_ctx.Document.Bones, _ctx.BoneWorlds());
                shape.Closed = true;
            }
        }
        else if (key == Keycode.Escape)
        {
            _ctx.Selection.Clear();
        }
    }
}


public sealed class BoneTool : IEditorTool
{
    public ToolMode Mode => ToolMode.Bone;

    // Screen-space offset of the rotate handle from its joint. AssetEditor
    // draws it with the same constants — keep them in sync.
    public const int RotateHandleDX = 20;
    public const int RotateHandleDY = -20;
    private const float GrabRadiusPx = 10f;

    private enum DragMode
    {
        None,
        Move,
        Rotate,
    }

    private readonly ToolContext _ctx;
    private DragMode _drag;
    private int _boneId = -1;
    private Vector2 _grabOffset;
    private Vector2 _jointWorld;
    private float _grabAngleDeg;
    private bool _checkpointTaken;

    public BoneTool(ToolContext ctx) => _ctx = ctx;

    public void OnPress(Vector2 mouseScreen, Vector2 mouseWorld)
    {
        _drag = DragMode.None;
        _checkpointTaken = false;
        _boneId = -1;

        var worlds = _ctx.BoneWorlds();

        // Rotate handle of the selected bone first.
        var selected = _ctx.Document.FindBone(_ctx.Selection.ActiveBoneId);
        if (selected is not null && worlds.TryGetValue(selected.Id, out var selWorld))
        {
            Vector2 jointScreen = _ctx.Camera.WorldToScreen(
                selWorld.Position, _ctx.ViewportCenter, _ctx.BaseSize);
            Vector2 rotateScreen = jointScreen + new Vector2(RotateHandleDX, RotateHandleDY);
            if ((mouseScreen - rotateScreen).Length() < GrabRadiusPx)
            {
                _ctx.Selection.SelectBone(selected.Id);
                _drag = DragMode.Rotate;
                _boneId = selected.Id;
                _jointWorld = selWorld.Position;
                _grabAngleDeg = MouseAngleDeg(mouseWorld, _jointWorld);
                return;
            }
        }

        // Joints, nearest wins.
        Bone? best = null;
        float bestDist = GrabRadiusPx;
        Vector2 bestWorld = Vector2.Zero;
        foreach (var candidate in _ctx.Document.Bones)
        {
            if (!worlds.TryGetValue(candidate.Id, out var world))
                continue;
            Vector2 screen = _ctx.Camera.WorldToScreen(
                world.Position, _ctx.ViewportCenter, _ctx.BaseSize);
            float d = (mouseScreen - screen).Length();
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
                bestWorld = world.Position;
            }
        }

        if (best is not null)
        {
            _ctx.Selection.SelectBone(best.Id);
            _ctx.Selection.ClearPoints();
            _drag = DragMode.Move;
            _boneId = best.Id;
            _grabOffset = mouseWorld - bestWorld;
            _jointWorld = bestWorld;
            return;
        }

        // Empty space: create a bone parented to the selection (or a root).
        _ctx.History.Checkpoint(_ctx.Document);
        Vector2 target = mouseWorld;
        if (_ctx.SnapEnabled)
            target = _ctx.Camera.SnapToPixel(target, _ctx.BaseSize, _ctx.Document.CanvasSize.x);

        int parentId = -1;
        RigidPose parentWorld = new(Vector2.Zero, 0f);
        var parent = _ctx.Document.FindBone(_ctx.Selection.ActiveBoneId);
        if (parent is not null && worlds.TryGetValue(parent.Id, out var pw))
        {
            parentId = parent.Id;
            parentWorld = pw;
        }

        var bone = new Bone
        {
            Id = _ctx.Document.NextBoneId++,
            Name = $"Bone {_ctx.Document.Bones.Count + 1}",
            ParentId = parentId,
        };
        var local = Skinning.RelativeTo(new RigidPose(target, parentWorld.AngleDeg), parentWorld);
        bone.LocalPosition = local.Position;
        bone.LocalAngle = 0f;
        bone.BindPosition = target;
        bone.BindAngle = parentWorld.AngleDeg;
        _ctx.Document.Bones.Add(bone);
        _ctx.Selection.SelectBone(bone.Id);
        _ctx.Selection.ClearPoints();
    }

    public void OnDrag(Vector2 mouseScreen, Vector2 mouseWorld, Vector2 grabWorld)
    {
        if (_drag == DragMode.None)
            return;

        var bone = _ctx.Document.FindBone(_boneId);
        if (bone is null)
            return;

        if (!_checkpointTaken)
        {
            _ctx.History.Checkpoint(_ctx.Document);
            _checkpointTaken = true;
        }

        if (_drag == DragMode.Move)
        {
            Vector2 target = mouseWorld - _grabOffset;
            if (_ctx.SnapEnabled)
                target = _ctx.Camera.SnapToPixel(target, _ctx.BaseSize, _ctx.Document.CanvasSize.x);

            var worlds = _ctx.BoneWorlds();
            RigidPose parentWorld = new(Vector2.Zero, 0f);
            if (bone.ParentId >= 0
                && _ctx.Document.FindBone(bone.ParentId) is not null
                && worlds.TryGetValue(bone.ParentId, out var pw))
            {
                parentWorld = pw;
            }
            // Keep the current world angle; only the joint translates.
            var current = worlds.TryGetValue(bone.Id, out var w)
                ? w
                : new RigidPose(bone.LocalPosition, bone.LocalAngle);
            var local = Skinning.RelativeTo(
                new RigidPose(target, current.AngleDeg), parentWorld);
            bone.LocalPosition = local.Position;
        }
        else
        {
            float angle = MouseAngleDeg(mouseWorld, _jointWorld);
            bone.LocalAngle += angle - _grabAngleDeg;
            _grabAngleDeg = angle;
        }
    }

    public void OnRelease() => _drag = DragMode.None;

    public void OnKeyShortcut(Keycode key)
    {
        if (key == Keycode.Delete || key == Keycode.Backspace)
        {
            if (_ctx.Selection.ActiveBoneId >= 0)
                Rigging.DeleteBone(_ctx.Document, _ctx.History, _ctx.Selection,
                    _ctx.Selection.ActiveBoneId);
        }
        else if (key == Keycode.Escape)
        {
            _ctx.Selection.ClearBone();
        }
    }

    private static float MouseAngleDeg(Vector2 mouseWorld, Vector2 jointWorld) =>
        MathF.Atan2(mouseWorld.y - jointWorld.y, mouseWorld.x - jointWorld.x)
            * 180f / MathF.PI;
}
