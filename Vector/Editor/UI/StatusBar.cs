using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Bottom status bar: document stats left, contextual tool hint right.
public class StatusBar : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    private readonly UiText _left = new() { TextSize = 13 };
    private readonly UiText _right = new() { TextSize = 13 };

    public override void OnInit()
    {
        SetClipped(true);
    }

    public override void OnUpdate()
    {
        // Background live: theme switches apply immediately.
        Color = Theme.BgLight;
        int points = 0;
        foreach (var shape in Editor.Document.Shapes)
            points += shape.Curve.Points.Count;

        _left.SetText(
            $"{Editor.Document.CanvasSize.x}×{Editor.Document.CanvasSize.y} · " +
            $"{Editor.Document.Shapes.Count} layers · {points} pts · " +
            $"{Editor.Camera.Zoom * 100f:0}%");
        _left.Sync();

        string hint = Editor.ActiveToolMode switch
        {
            Tools.ToolMode.Pen => "Click to add · Enter closes shape · Del removes",
            Tools.ToolMode.Bone => "Click empty for bone · drag joints · Del removes",
            _ => "Drag points/fills · Shift toggles · wheel zooms · right-drag pans",
        };
        _right.SetText(hint);
        _right.Sync();
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;
        IRect box = Rect.Inset(Theme.PanelPad);

        _left.Draw(renderPass,
            new Vector2(box.x, Rect.y + (Rect.h - _left.Size.y) / 2),
            screen, clip.Min, clip.Max);
        _right.Draw(renderPass,
            new Vector2(box.x + box.w - _right.Size.x,
                Rect.y + (Rect.h - _right.Size.y) / 2),
            screen, clip.Min, clip.Max);

        if (Assets.TryGetShader("builtin", "ui-rect", out var edge))
        {
            renderPass.Bind(edge);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x, Rect.y, 0),
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
