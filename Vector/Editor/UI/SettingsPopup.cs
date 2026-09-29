using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Modal settings card. Fullscreen (dims + consumes all mouse while open);
// toggled by the toolbar button, Comma shortcut, Close, or Escape.
//
// Z-order note: the scene draws this in a second (overlay) render pass so it
// always lands on top of the asset. Event priority is separate: it stays the
// first child of the root, so it still wins mouse events while open.
public class SettingsPopup : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    private const int CardW = 360;
    private const int CardH = 336;
    private const int RowH = 36;
    private const int TitleH = 44;

    private static readonly int[] GridSteps = { 1, 2, 4, 8, 16 };

    private SegmentedControl _appearance = null!;
    private Toggle _snap = null!;
    private Toggle _controls = null!;
    private SegmentedControl _handleSize = null!;
    private SegmentedControl _gridSize = null!;
    private Button _reset = null!;
    private Button _close = null!;
    private readonly UiText _title = new();
    private readonly UiText[] _rowLabels = { new(), new(), new(), new(), new() };
    private static readonly string[] RowNames =
        { "Appearance", "Snap to pixels", "Control points", "Handle size", "Grid size" };

    public bool IsOpen { get; private set; }

    public void Open()
    {
        IsOpen = true;
        SetEnabled(true);
    }

    public void Close()
    {
        IsOpen = false;
        SetEnabled(false);
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public override void OnInit()
    {
        SetEnabled(false);

        _appearance = new SegmentedControl
        {
            Options = new List<string> { "Dark", "Light" },
            OnSelect = i =>
            {
                Editor.Settings.DarkTheme = i == 0;
                Editor.Settings.Apply();
                Editor.Settings.Save();
            },
        };
        _snap = new Toggle
        {
            OnChange = v =>
            {
                Editor.Settings.SnapEnabled = v;
                Editor.Settings.Save();
            },
        };
        _controls = new Toggle
        {
            OnChange = v =>
            {
                Editor.Settings.ShowControls = v;
                Editor.Settings.Save();
            },
        };
        _handleSize = new SegmentedControl
        {
            Options = new List<string> { "S", "M", "L" },
            OnSelect = i =>
            {
                Editor.Settings.HandleSize = i;
                Editor.Settings.Apply();
                Editor.Settings.Save();
            },
        };
        _gridSize = new SegmentedControl
        {
            Options = new List<string> { "1", "2", "4", "8", "16" },
            OnSelect = i =>
            {
                Editor.Settings.GridSquarePixels = GridSteps[Math.Clamp(i, 0, GridSteps.Length - 1)];
                Editor.Settings.Save();
            },
        };
        _reset = new Button
        {
            LabelText = "Reset to defaults",
            OnClick = () =>
            {
                Editor.Settings.Reset();
                Editor.Settings.Apply();
                Editor.Settings.Save();
            },
        };
        _close = new Button { LabelText = "Close", Selected = true, OnClick = Close };

        AddNode(_appearance);
        AddNode(_snap);
        AddNode(_controls);
        AddNode(_handleSize);
        AddNode(_gridSize);
        AddNode(_reset);
        AddNode(_close);
    }

    public override void OnUpdate()
    {
        if (!IsOpen)
            return;

        Color = Theme.Scrim;

        _appearance.Selected = Editor.Settings.DarkTheme ? 0 : 1;
        _snap.Value = Editor.Settings.SnapEnabled;
        _controls.Value = Editor.Settings.ShowControls;
        _handleSize.Selected = Math.Clamp(Editor.Settings.HandleSize, 0, 2);
        _gridSize.Selected = GridIndex(Editor.Settings.GridSquarePixels);

        float cx = Rect.x + (Rect.w - CardW) / 2;
        float cy = Rect.y + (Rect.h - CardH) / 2;
        float y = cy + TitleH + 8;

        _appearance.SetRect(new IRect { x = (int)(cx + 170), y = (int)y, w = 160, h = 30 });
        y += RowH + 4;
        _snap.SetRect(new IRect { x = (int)(cx + 170), y = (int)y, w = 160, h = 30 });
        y += RowH + 4;
        _controls.SetRect(new IRect { x = (int)(cx + 170), y = (int)y, w = 160, h = 30 });
        y += RowH + 4;
        _handleSize.SetRect(new IRect { x = (int)(cx + 170), y = (int)y, w = 160, h = 30 });
        y += RowH + 4;
        _gridSize.SetRect(new IRect { x = (int)(cx + 170), y = (int)y, w = 160, h = 30 });
        y += RowH + 10;
        _reset.SetRect(new IRect { x = (int)(cx + 20), y = (int)y, w = CardW - 40, h = 30 });
        y += 30 + 8;
        _close.SetRect(new IRect { x = (int)(cx + 20), y = (int)y, w = CardW - 40, h = 30 });

        _title.SetText("Settings");
        _title.Sync();
        for (int i = 0; i < _rowLabels.Length; i++)
        {
            _rowLabels[i].SetText(RowNames[i]);
            _rowLabels[i].Sync();
        }
    }

    private static int GridIndex(int texels)
    {
        int best = 2;
        for (int i = 0; i < GridSteps.Length; i++)
        {
            if (Math.Abs(GridSteps[i] - texels) < Math.Abs(GridSteps[best] - texels))
                best = i;
        }
        return best;
    }

    private void RowLabel(ImRenderPass renderPass, Vector2 screen, IRect clip,
        int row, float y)
    {
        var label = _rowLabels[row];
        label.Draw(renderPass,
            new Vector2(Rect.x + (Rect.w - CardW) / 2 + 20, y),
            screen, clip.Min, clip.Max);
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        // Explicit order (not base.OnDraw): scrim, then card, then children,
        // then labels on top. base would draw children before the card and
        // bury them under it.
        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        if (Assets.TryGetShader("builtin", "ui-rect", out var scrim))
        {
            renderPass.Bind(scrim);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x, Rect.y, 0),
                Size = new Vector2(Rect.w, Rect.h),
                Color = Color,
                BorderRadius = Vector4.Zero,
                BorderColor = new Vector4(0, 0, 0, 0),
                ScreenSize = screen,
                BorderSize = 0f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        float cx = Rect.x + (Rect.w - CardW) / 2;
        float cy = Rect.y + (Rect.h - CardH) / 2;

        if (Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(cx, cy, 0),
                Size = new Vector2(CardW, CardH),
                Color = Theme.BgLight,
                BorderRadius = new Vector4(12, 12, 12, 12),
                BorderColor = new Vector4(1, 1, 1, 0.12f),
                ScreenSize = screen,
                BorderSize = 1f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        _title.Draw(renderPass,
            new Vector2(cx + 20, cy + (TitleH - _title.Size.y) / 2),
            screen, clip.Min, clip.Max);

        float y = cy + TitleH + 8 + (30 - 16) / 2;
        RowLabel(renderPass, screen, clip, 0, y);
        y += RowH + 4;
        RowLabel(renderPass, screen, clip, 1, y);
        y += RowH + 4;
        RowLabel(renderPass, screen, clip, 2, y);
        y += RowH + 4;
        RowLabel(renderPass, screen, clip, 3, y);
        y += RowH + 4;
        RowLabel(renderPass, screen, clip, 4, y);

        DrawAllChildren(renderPass, screenSize);
    }
}
