using Vector.Editor;
using Vector.Editor.Tools;
using Vector.Editor.UI;
using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.Serializer;
using WaffleEngine.UI;
using WaffleEngine.Vector;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Scenes;

// Thin orchestration node. All state lives in Document/Camera/Selection/History;
// all pointer math lives in ViewportCamera; all gestures live in Tools.
// Shortcuts (Phase 1, no modifier combos to stay compatible with EventSpace):
//   V select · P pen · N new shape · Del delete point · Enter close shape
//   Z undo · Y redo · G snap toggle · S save · L load · -/= canvas size
//   B bone tool · , settings · Esc clear/close · Right-drag pan · Wheel zoom
public class AssetEditor : INode
{
    public string AssetPath = "Assets/sprite.vec.yaml";

    public VectorAssetDocument Document;
    public ViewportCamera Camera = new();
    public EditorSelection Selection = new();
    public EditorHistory History = new();
    public ToolMode ActiveToolMode = ToolMode.Select;
    public EditorSettings Settings = new();
    public SettingsPopup? Popup;

    public GpuTexture AssetTexture;
    public VectorRenderer Renderer = new();

    private readonly ToolContext _toolCtx = new();
    private readonly SelectTool _selectTool;
    private readonly PenTool _penTool;
    private readonly BoneTool _boneTool;
    private ToolDock? _dock;

    // Current bone world poses, refreshed in OnUpdate for drawing. Null when
    // the document has no bones.
    private Dictionary<int, RigidPose>? _boneWorlds;

    private Vector2 _grabScreen;
    private Vector2 _grabPan;
    private Vector2 _grabWorld;
    private bool _panning;
    private bool _toolDragging;

    public AssetEditor(IVector2 size)
    {
        Settings = EditorSettings.LoadOrDefault();
        Document = VectorAssetDocument.CreateDefault();
        Document.CanvasSize = size;
        Selection.SelectShape(Document.Shapes.Count > 0 ? Document.Shapes[0].Id : -1);

        AssetTexture = CreateAssetTexture(size);

        _toolCtx.Document = Document;
        _toolCtx.Camera = Camera;
        _toolCtx.Selection = Selection;
        _toolCtx.History = History;
        _selectTool = new SelectTool(_toolCtx);
        _penTool = new PenTool(_toolCtx);
        _boneTool = new BoneTool(_toolCtx);

        if (File.Exists(AssetPath)
            && Yaml.TryDeserialize(AssetPath, out VectorAssetDocument? loaded) && loaded is not null)
            SetDocument(loaded);
    }

    public override void OnInit()
    {
        _dock = new ToolDock { Editor = this };
        AddNode(_dock);
    }

    private static GpuTexture CreateAssetTexture(IVector2 size) => new(
        GpuTextureSettings.Default((uint)Math.Max(1, size.x), (uint)Math.Max(1, size.y)) with
        {
            ColorTarget = true,
            MagFilter = FilterMode.Nearest,
            MinFilter = FilterMode.Nearest,
        });

    private void SetDocument(VectorAssetDocument doc)
    {
        Document = doc;
        _toolCtx.Document = doc;
        History.Clear();
        Selection.Clear();
        if (doc.Shapes.Count > 0)
            Selection.SelectShape(doc.Shapes[0].Id);
        if ((int)AssetTexture.Width != doc.CanvasSize.x || (int)AssetTexture.Height != doc.CanvasSize.y)
            AssetTexture = CreateAssetTexture(doc.CanvasSize);
    }

    public void Save()
    {
        Yaml.TrySerialize(AssetPath, Document);
        History.MarkSaved();
    }

    public void Load()
    {
        if (!File.Exists(AssetPath))
            return;
        if (Yaml.TryDeserialize(AssetPath, out VectorAssetDocument? loaded) && loaded is not null)
            SetDocument(loaded);
    }

    public void NewShape()
    {
        History.Checkpoint(Document);
        var shape = new Shape { Id = Document.NextShapeId++, Name = $"Shape {Document.Shapes.Count + 1}", Closed = false };
        // New layers go on top (front) of the stack.
        Document.Shapes.Insert(0, shape);
        Selection.SelectShape(shape.Id);
        ActiveToolMode = ToolMode.Pen;
    }

    public void DeleteShape(int shapeId)
    {
        var shape = Document.FindShape(shapeId);
        if (shape is null)
            return;
        History.Checkpoint(Document);
        Document.Shapes.Remove(shape);
        Selection.Clear();
        if (Document.Shapes.Count > 0)
            Selection.SelectShape(Document.Shapes[0].Id);
    }

    public void DeleteActiveShape() => DeleteShape(Selection.ActiveShapeId);

    public void CloseActiveShape()
    {
        var shape = Document.FindShape(Selection.ActiveShapeId);
        if (shape is null || shape.Closed || shape.AnchorCount < 2)
            return;
        History.Checkpoint(Document);
        shape.CloseLoop(Document.Bones, Skinning.ComputeWorlds(Document.Bones));
        shape.Closed = true;
    }

    public void MoveShapeTo(int shapeId, int toIndex)
    {
        int from = Document.Shapes.FindIndex(s => s.Id == shapeId);
        if (from < 0)
            return;
        toIndex = Math.Clamp(toIndex, 0, Document.Shapes.Count - 1);
        if (toIndex == from)
            return;
        History.Checkpoint(Document);
        var shape = Document.Shapes[from];
        Document.Shapes.RemoveAt(from);
        Document.Shapes.Insert(Math.Clamp(toIndex, 0, Document.Shapes.Count), shape);
    }

    public void Undo()
    {
        if (History.TryUndo(Document, out var restored))
            SetDocumentKeepHistory(restored);
    }

    public void Redo()
    {
        if (History.TryRedo(Document, out var restored))
            SetDocumentKeepHistory(restored);
    }

    public const int MinCanvasSize = 8;
    public const int MaxCanvasSize = 128;

    public void SetCanvasSize(int pixels)
    {
        int clamped = Math.Clamp(pixels, MinCanvasSize, MaxCanvasSize);
        if (clamped == Document.CanvasSize.x && clamped == Document.CanvasSize.y)
            return;
        History.Checkpoint(Document);
        Document.CanvasSize = new IVector2(clamped, clamped);
        AssetTexture.Dispose();
        AssetTexture = CreateAssetTexture(Document.CanvasSize);
    }

    public void GrowCanvas() => SetCanvasSize(Document.CanvasSize.x * 2);
    public void ShrinkCanvas() => SetCanvasSize(Document.CanvasSize.x / 2);

    public void DeleteBone(int boneId) =>
        Rigging.DeleteBone(Document, History, Selection, boneId);

    public void BindSkin() => Rigging.AutoBindAll(Document, History);

    private IEditorTool ActiveTool => ActiveToolMode switch
    {
        ToolMode.Pen => _penTool,
        ToolMode.Bone => _boneTool,
        _ => _selectTool,
    };

    public override void OnUpdate()
    {
        PollShortcuts();

        // Wheel zoom around cursor, only when hovering the viewport so the
        // side panels keep the wheel free for future scrolling.
        float wheel = Input.Mouse.MouseWheelTicksDelta;
        if (wheel != 0 && Rect.Contains(Input.Mouse.Position))
        {
            var (center, baseSize) = ComputeLayout();
            Camera.ApplyZoom(wheel, Input.Mouse.Position, center, baseSize);
        }

        SyncToolContext();

        // Reversed: the layers panel reads top-down, so the first shape is
        // fed last and blends front-most. (No depth buffer exists in this
        // pass — overlap order is purely instance order.)
        // Posed bones deform into scratch curves; untouched rigs feed the
        // live curves with zero extra cost.
        _boneWorlds = null;
        bool posed = false;
        if (Document.Bones.Count > 0)
        {
            _boneWorlds = Skinning.ComputeWorlds(Document.Bones);
            foreach (var bone in Document.Bones)
            {
                if (_boneWorlds.TryGetValue(bone.Id, out var world)
                    && (world.Position != bone.BindPosition || world.AngleDeg != bone.BindAngle))
                {
                    posed = true;
                    break;
                }
            }
        }
        for (int i = Document.Shapes.Count - 1; i >= 0; i--)
        {
            var shape = Document.Shapes[i];
            Curve curve = shape.Curve;
            if (posed && _boneWorlds is not null)
                curve = DeformedCurve(shape, _boneWorlds);
            if (curve.Points.Count > 0)
                Renderer.AddCurve(curve, shape.Color);
        }
    }

    private Curve DeformedCurve(Shape shape, Dictionary<int, RigidPose> worlds)
    {
        var curve = shape.Curve.Clone();
        for (int i = 0; i < curve.Points.Count; i++)
        {
            if (i >= shape.BoneBinding.Count || i >= shape.BindPose.Count)
                continue;
            int boneId = shape.BoneBinding[i];
            if (boneId < 0)
                continue;
            var bone = Document.FindBone(boneId);
            if (bone is null || !worlds.TryGetValue(boneId, out var current))
                continue;
            var bind = new RigidPose(bone.BindPosition, bone.BindAngle);
            if (current.Position == bind.Position && current.AngleDeg == bind.AngleDeg)
                continue;
            curve.Points[i] = new Point(Skinning.DeformPoint(shape.BindPose[i], bind, current));
        }
        return curve;
    }

    private void SyncToolContext()
    {
        var (center, baseSize) = ComputeLayout();
        _toolCtx.ViewportCenter = center;
        _toolCtx.BaseSize = baseSize;
        _toolCtx.SnapEnabled = Settings.SnapEnabled;

        // Floating dock, left edge of the viewport.
        if (_dock is not null)
        {
            int h = ToolDock.DockHeight();
            _dock.SetRect(new IRect
            {
                x = Rect.x + 12,
                y = Rect.y + (Rect.h - h) / 2,
                w = ToolDock.DockWidth,
                h = h,
            });
        }
    }

    private void ToggleSnap()
    {
        Settings.SnapEnabled = !Settings.SnapEnabled;
        Settings.Save();
    }

    private void PollShortcuts()
    {
        var keys = Input.GetDefaultEventSpace;
        if (Popup is not null && Popup.IsOpen)
        {
            if (keys.KeyPressed(Keycode.Escape) || keys.KeyPressed(Keycode.Comma))
                Popup.Close();
            return;
        }
        if (keys.KeyPressed(Keycode.V)) ActiveToolMode = ToolMode.Select;
        else if (keys.KeyPressed(Keycode.P)) ActiveToolMode = ToolMode.Pen;
        else if (keys.KeyPressed(Keycode.B)) ActiveToolMode = ToolMode.Bone;
        else if (keys.KeyPressed(Keycode.G)) ToggleSnap();
        else if (keys.KeyPressed(Keycode.Z)) Undo();
        else if (keys.KeyPressed(Keycode.Y)) Redo();
        else if (keys.KeyPressed(Keycode.S)) Save();
        else if (keys.KeyPressed(Keycode.L)) Load();
        else if (keys.KeyPressed(Keycode.N)) NewShape();
        else if (keys.KeyPressed(Keycode.Minus)) ShrinkCanvas();
        else if (keys.KeyPressed(Keycode.Equals)) GrowCanvas();
        else if (keys.KeyPressed(Keycode.Comma)) Popup?.Toggle();
        else if (keys.KeyPressed(Keycode.Return) || keys.KeyPressed(Keycode.Escape)
                 || keys.KeyPressed(Keycode.Delete) || keys.KeyPressed(Keycode.Backspace))
        {
            SyncToolContext();
            ActiveTool.OnKeyShortcut(
                keys.KeyPressed(Keycode.Return) ? Keycode.Return :
                keys.KeyPressed(Keycode.Escape) ? Keycode.Escape : Keycode.Delete);
        }
        else if (keys.KeyPressed(Keycode.Tab))
        {
            if (Document.Shapes.Count > 0)
            {
                int idx = Document.Shapes.FindIndex(s => s.Id == Selection.ActiveShapeId);
                int next = (idx + 1) % Document.Shapes.Count;
                Selection.SelectShape(Document.Shapes[next].Id);
            }
        }
    }

    private void SetDocumentKeepHistory(VectorAssetDocument restored)
    {
        // Undo/redo already pushed the counterpart; just swap content.
        Document = restored;
        _toolCtx.Document = restored;
        Selection.Clear();
        if (restored.Shapes.Count > 0)
            Selection.SelectShape(restored.Shapes[0].Id);
    }

    public override void OnEvent(NodeEvent node_event)
    {
        // The tree dispatches release as MouseClick, not MouseHold — without
        // this branch tool OnRelease (marquee clear, click-collapse) would
        // never run in-app.
        if (node_event == NodeEvent.MouseClick)
        {
            if (_toolDragging)
            {
                ActiveTool.OnRelease();
                _toolDragging = false;
            }
            _panning = false;
            return;
        }

        if (node_event != NodeEvent.MouseHold)
            return;

        SyncToolContext();
        var (center, baseSize) = ComputeLayout();
        Vector2 mouseScreen = Input.Mouse.Position;
        Vector2 mouseWorld = Camera.ScreenToWorld(mouseScreen, center, baseSize);

        if (Input.Mouse.IsRightPressed)
        {
            _panning = true;
            _grabScreen = mouseScreen;
            _grabPan = Camera.Pan;
        }
        else if (Input.Mouse.IsRightDown && _panning)
        {
            Vector2 delta = mouseScreen - _grabScreen;
            Camera.Pan = new Vector2(_grabPan.x + delta.x, _grabPan.y + delta.y);
            return;
        }
        else if (!Input.Mouse.IsRightDown)
        {
            _panning = false;
        }

        if (Input.Mouse.IsLeftPressed)
        {
            _grabWorld = mouseWorld;
            _toolDragging = true;
            ActiveTool.OnPress(mouseScreen, mouseWorld);
        }
        else if (Input.Mouse.IsLeftDown && _toolDragging)
        {
            ActiveTool.OnDrag(mouseScreen, mouseWorld, _grabWorld);
        }
    }

    // Crisp-pixel base size (snapped to integer texture scale), before zoom.
    private (Vector2 Center, float BaseSize) ComputeLayout()
    {
        var center = new Vector2(
            Rect.x + (float)Rect.w / 2,
            Rect.y + (float)Rect.h / 2);
        float baseSize = (float)Rect.h / 2;
        if (AssetTexture.Width > 0)
            baseSize -= baseSize % AssetTexture.Width;
        return (center, Math.Max(8f, baseSize));
    }

    public void RenderAsset(ImQueue queue) => Renderer.Render(queue, AssetTexture);

    private struct TexturedQuad
    {
        public Vector4 Position;
        public Vector4 Size;
        public Vector4 RenderSize;
        public Vector4 Clip;
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        var (center, baseSize) = ComputeLayout();
        float drawSize = baseSize * Camera.Zoom;
        float quadX = center.x - drawSize / 2 + Camera.Pan.x;
        float quadY = center.y - drawSize / 2 + Camera.Pan.y;

        if (!Assets.TryGetShader("builtin", "ui-rect", out var handleShader))
            throw new NullReferenceException();

        // Clip everything to the viewport so panning never paints over panels.
        IRect stackClip = Tree.Clipstack.TryPeek(out IRect clipTop) ? clipTop : screenSize;
        IRect clip = Rect.GetOverlap(stackClip) ?? IRect.Zero;
        var screen = new Vector2(screenSize.Width, screenSize.Height);
        Vector2 clipMin = clip.Min;
        Vector2 clipMax = clip.Max;

        // Canvas backdrop + boundary so the asset edges read against the scene.
        renderPass.Bind(handleShader);
        renderPass.SetUniforms(new Rect.UIRectData()
        {
            Position = new AlignedVector3(quadX - Theme.CanvasPad, quadY - Theme.CanvasPad, 0),
            Size = new Vector2(drawSize + Theme.CanvasPad * 2, drawSize + Theme.CanvasPad * 2),
            Color = Theme.Highlight,
            BorderRadius = new Vector4(Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius),
            BorderColor = new Vector4(0, 0, 0, 0),
            ScreenSize = screen,
            BorderSize = 0f,
            ClipMin = clipMin,
            ClipMax = clipMax,
        });
        renderPass.DrawPrimatives(6, 1, 0, 0);

        if (!Assets.TryGetShader("builtin", "textured-quad", out var quadShader))
            throw new NullReferenceException();

        // Transparency grid in one procedural draw, locked to canvas texels
        // so squares stay glued to asset pixels at any zoom.
        if (Assets.TryGetShader("builtin", "ui-checker", out var gridShader))
        {
            renderPass.Bind(gridShader);
            renderPass.SetUniforms(UiCheckerData.GridQuad(
                new Vector2(quadX, quadY),
                new Vector2(drawSize, drawSize),
                new Vector2(screenSize.Width, screenSize.Height),
                clipMin, clipMax,
                Document.CanvasSize.x,
                Settings.GridSquarePixels,
                Theme.TextDim.WithAlpha(0.4f),
                Theme.TextDim.WithAlpha(0.15f)));
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        renderPass.Bind(quadShader);
        renderPass.Bind(AssetTexture, 0);
        renderPass.SetUniforms(new TexturedQuad
        {
            Position = new Vector4(quadX, quadY, 0, 0),
            Size = new Vector4(drawSize, drawSize, 0, 0),
            RenderSize = new Vector4(screenSize.Width, screenSize.Height, 0, 0),
            Clip = new Vector4(clipMin.x, clipMin.y, clipMax.x, clipMax.y),
        });
        renderPass.DrawPrimatives(6, 1, 0, 0);
        
        renderPass.Bind(handleShader);

        // Child nodes (floating tool dock). Drawn here — after the canvas,
        // before selection feedback — rather than via base.OnDraw, which this
        // override replaces.
        DrawAllChildren(renderPass, screenSize);

        // Marquee overlay while rubber-banding.
        if (_toolCtx.MarqueeActive)
        {
            Vector2 m0 = _toolCtx.MarqueeStartScreen;
            Vector2 m1 = _toolCtx.MarqueeEndScreen;
            Vector2 mMin = new(Math.Min(m0.x, m1.x), Math.Min(m0.y, m1.y));
            Vector2 mMax = new(Math.Max(m0.x, m1.x), Math.Max(m0.y, m1.y));
            renderPass.SetUniforms(new Rect.UIRectData()
            {
                Position = new AlignedVector3(mMin.x, mMin.y, 0),
                Size = new Vector2(Math.Max(1, mMax.x - mMin.x), Math.Max(1, mMax.y - mMin.y)),
                Color = Theme.Accent.WithAlpha(0.15f),
                BorderRadius = Vector4.Zero,
                BorderColor = Theme.Accent,
                ScreenSize = new Vector2(screenSize.w, screenSize.h),
                BorderSize = 1.5f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        foreach (var shape in Document.Shapes)
        {
            // In Bone mode the rig is the interface: hide curve handles so
            // the joints read clearly (deformed art still renders underneath).
            if (ActiveToolMode == ToolMode.Bone)
                break;

            bool isActive = Selection.IsShapeActive(shape.Id);
            bool showAll = isActive || Document.Shapes.Count == 1;

            var pts = shape.Curve.Points;
            for (int i = 0; i < pts.Count; i++)
            {
                // Non-active shapes only show their selected points.
                if (!showAll && !Selection.IsPointActive(shape.Id, i))
                    continue;
                bool isAnchor = i % 2 == 0;
                if (!isAnchor && !Settings.ShowControls)
                    continue;
                Vector2 sp = Camera.WorldToScreen(pts[i].Position, center, baseSize);
                bool selected = Selection.IsPointActive(shape.Id, i);
                float size = (isAnchor ? Theme.HandleAnchorSize : Theme.HandleControlSize)
                    * Theme.HandleScale;
                if (selected)
                    size += Theme.HandleSelectedGrow * Theme.HandleScale;
                Vector4 color = selected ? Theme.HandleSelected
                    : isAnchor
                        ? i == 0 && !shape.Closed ? Theme.HandleOpenStart : Theme.HandleAnchor
                        : Theme.HandleControl;

                renderPass.SetUniforms(new Rect.UIRectData()
                {
                    Position = new AlignedVector3(sp.x - size * 0.5f, sp.y - size * 0.5f, 0),
                    Size = new Vector2(size, size),
                    Color = color,
                    BorderRadius = new Vector4(5, 5, 5, 5),
                    BorderColor = Theme.Text,
                    ScreenSize = new Vector2(screenSize.w, screenSize.h),
                    BorderSize = Theme.HandleBorderWidth,
                    ClipMin = clip.Min,
                    ClipMax = clip.Max,
                });
                renderPass.DrawPrimatives(6, 1, 0, 0);
            }
        }

        DrawBones(renderPass, screenSize, center, baseSize, clipMin, clipMax);
    }

    private void DrawBones(ImRenderPass renderPass, IRect screenSize,
        Vector2 center, float baseSize, Vector2 clipMin, Vector2 clipMax)
    {
        if (Document.Bones.Count == 0)
            return;

        var worlds = _boneWorlds ?? Skinning.ComputeWorlds(Document.Bones);
        var screen = new Vector2(screenSize.w, screenSize.h);

        foreach (var bone in Document.Bones)
        {
            if (!worlds.TryGetValue(bone.Id, out var world))
                continue;
            Vector2 joint = Camera.WorldToScreen(world.Position, center, baseSize);
            bool selected = Selection.IsBoneActive(bone.Id);

            if (bone.ParentId >= 0 && worlds.TryGetValue(bone.ParentId, out var parentWorld))
            {
                Vector2 parent = Camera.WorldToScreen(parentWorld.Position, center, baseSize);
                for (int d = 1; d <= 3; d++)
                {
                    float t = d / 4f;
                    Vector2 dot = new(
                        parent.x + (joint.x - parent.x) * t,
                        parent.y + (joint.y - parent.y) * t);
                    DrawHandle(renderPass, screen, clipMin, clipMax, dot,
                        5f * Theme.HandleScale, Theme.HandleBone, 2f);
                }
            }

            float size = (selected ? 12f : 10f) * Theme.HandleScale;
            DrawHandle(renderPass, screen, clipMin, clipMax, joint, size,
                selected ? Theme.HandleSelected : Theme.HandleBone, 4f);

            if (selected)
            {
                Vector2 rotate = joint + new Vector2(
                    BoneTool.RotateHandleDX, BoneTool.RotateHandleDY);
                DrawHandle(renderPass, screen, clipMin, clipMax, rotate,
                    8f * Theme.HandleScale, Theme.HandleControl, 4f);
            }
        }
    }

    private static void DrawHandle(ImRenderPass renderPass, Vector2 screen,
        Vector2 clipMin, Vector2 clipMax, Vector2 pos, float size, Color color,
        float radius)
    {
        renderPass.SetUniforms(new Rect.UIRectData()
        {
            Position = new AlignedVector3(pos.x - size * 0.5f, pos.y - size * 0.5f, 0),
            Size = new Vector2(size, size),
            Color = color,
            BorderRadius = new Vector4(radius, radius, radius, radius),
            BorderColor = Theme.Text,
            ScreenSize = screen,
            BorderSize = Theme.HandleBorderWidth,
            ClipMin = clipMin,
            ClipMax = clipMax,
        });
        renderPass.DrawPrimatives(6, 1, 0, 0);
    }
}
