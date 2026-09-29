using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Right inspector: FILL (swatch + Oklch sliders for the active layer),
// LAYER (close/delete), RIG (bind + bone hint). Writes go through
// Oklch.ToRGB, which arrives gamma-encoded from the source.
public class InspectorPanel : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    public const int PanelH = 296;
    private const int TitleH = 24;
    private const int SwatchH = 30;
    private const int SliderH = 28;
    private const int ButtonH = 28;

    private Slider _hue = null!;
    private Slider _light = null!;
    private Slider _chroma = null!;
    private Button _closeBtn = null!;
    private Button _deleteBtn = null!;
    private Button _bindBtn = null!;
    private readonly UiText _fillTitle = new() { TextSize = 13 };
    private readonly UiText _layerTitle = new() { TextSize = 13 };
    private readonly UiText _rigTitle = new() { TextSize = 13 };
    private readonly UiText _hint = new() { TextSize = 13 };
    private readonly ColorIntent _intent = new();
    private bool _checkpointArmed;

    public override void OnInit()
    {
        SetClipped(true);

        _hue = MakeSlider((intent, v) =>
        {
            intent.H = v;
            return new OklchColor(intent.L, intent.C * ColorIntent.MaxChroma, intent.H * 360f);
        });
        _light = MakeSlider((intent, v) =>
        {
            intent.L = v;
            return new OklchColor(intent.L, intent.C * ColorIntent.MaxChroma, intent.H * 360f);
        });
        _chroma = MakeSlider((intent, v) =>
        {
            intent.C = v;
            return new OklchColor(intent.L, intent.C * ColorIntent.MaxChroma, intent.H * 360f);
        });
        _closeBtn = new Button { OnClick = () => Editor.CloseActiveShape() };
        _deleteBtn = new Button { LabelText = "Delete Layer", OnClick = () => Editor.DeleteActiveShape() };
        _bindBtn = new Button { LabelText = "Bind Skin", OnClick = () => Editor.BindSkin() };

        AddNode(_hue);
        AddNode(_light);
        AddNode(_chroma);
        AddNode(_closeBtn);
        AddNode(_deleteBtn);
        AddNode(_bindBtn);
    }

    private Slider MakeSlider(Func<ColorIntent, float, OklchColor> replace)
    {
        var slider = new Slider();
        slider.OnGrab = () =>
        {
            var shape = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
            if (shape is not null)
            {
                Editor.History.Checkpoint(Editor.Document);
                _checkpointArmed = true;
            }
        };
        slider.OnRelease = () => _checkpointArmed = false;
        slider.OnChange = v =>
        {
            var shape = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
            if (shape is null)
                return;
            if (!_checkpointArmed)
            {
                Editor.History.Checkpoint(Editor.Document);
                _checkpointArmed = true;
            }
            _intent.Wrote(shape, replace(_intent, v).ToRGB());
        };
        return slider;
    }

    public override void OnUpdate()
    {
        // Background live: theme switches apply immediately.
        Color = Theme.BgLight;
        var shape = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
        _intent.SyncTo(shape);

        bool hasShape = shape is not null;
        bool openShape = hasShape && !shape!.Closed;
        bool hasBones = Editor.Document.Bones.Count > 0;

        _fillTitle.SetText(shape is null ? "FILL" : $"FILL · {shape.Name.ToUpperInvariant()}");
        _layerTitle.SetText("LAYER");
        _rigTitle.SetText(hasBones ? $"RIG · {Editor.Document.Bones.Count}" : "RIG");
        _hint.SetText(hasBones ? "" : "No bones — press B");
        _fillTitle.Sync();
        _layerTitle.Sync();
        _rigTitle.Sync();
        _hint.Sync();

        _hue.Label = $"H";
        _light.Label = $"L";
        _chroma.Label = $"C";
        _hue.Value = _intent.H;
        _light.Value = _intent.L;
        _chroma.Value = _intent.C;

        // Gradient tracks mirror the intent: full-saturation hue ring at the
        // current lightness, lightness/chroma ramps through the current hue.
        var oklch = _intent.ToOklch();
        _hue.GradientTrack = true;
        _hue.Gradient = UiGradientData.Ramp(Vector2.Zero, Vector2.Zero, Vector2.Zero,
            Vector2.Zero, Vector2.Zero, 0, oklch.Lightness, oklch.Chroma, 0f);
        _light.GradientTrack = true;
        _light.Gradient = UiGradientData.Ramp(Vector2.Zero, Vector2.Zero, Vector2.Zero,
            Vector2.Zero, Vector2.Zero, 1, 0f, oklch.Chroma, oklch.Hue);
        _chroma.GradientTrack = true;
        _chroma.Gradient = UiGradientData.Ramp(Vector2.Zero, Vector2.Zero, Vector2.Zero,
            Vector2.Zero, Vector2.Zero, 2, oklch.Lightness, 0f, oklch.Hue);

        _closeBtn.LabelText = openShape ? "Close Shape" : "Closed";
        _closeBtn.Dimmed = !openShape;
        _deleteBtn.Dimmed = !hasShape;
        _bindBtn.Dimmed = !hasBones;

        // One inset box feeds every child x/w below; only y is custom.
        IRect box = Rect.Inset(Theme.PanelPad);
        int w = box.w;
        // Canonical vertical rhythm (mirrored in OnDraw):
        // title 4/24, sliders 28/58/88, swatch 124/30, layer 160/24,
        // buttons 184/28, rig 218/24, bind 242/28, hint 274.
        _hue.SetRect(new IRect { x = box.x, y = (int)(Rect.y + 28), w = w, h = SliderH });
        _light.SetRect(new IRect { x = box.x, y = (int)(Rect.y + 58), w = w, h = SliderH });
        _chroma.SetRect(new IRect { x = box.x, y = (int)(Rect.y + 88), w = w, h = SliderH });
        int half = (w - Theme.ButtonGap) / 2;
        _closeBtn.SetRect(new IRect { x = box.x, y = (int)(Rect.y + 184), w = half, h = ButtonH });
        _deleteBtn.SetRect(new IRect { x = box.x + half + Theme.ButtonGap, y = (int)(Rect.y + 184), w = w - half - Theme.ButtonGap, h = ButtonH });
        _bindBtn.SetRect(new IRect { x = box.x, y = (int)(Rect.y + 242), w = w, h = ButtonH });
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        // Left-edge separator (inspector docks right).
        if (Assets.TryGetShader("builtin", "ui-rect", out var edge))
        {
            renderPass.Bind(edge);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x, Rect.y, 0),
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

        IRect box = Rect.Inset(Theme.PanelPad);
        float x = box.x;
        _fillTitle.Draw(renderPass, new Vector2(x, Rect.y + 4 + (TitleH - _fillTitle.Size.y) / 2),
            screen, clip.Min, clip.Max);

        var shape = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
        if (shape is not null && Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(box.x, Rect.y + 124, 0),
                Size = new Vector2(box.w, SwatchH),
                Color = shape.Color,
                BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius),
                BorderColor = Theme.Text,
                ScreenSize = screen,
                BorderSize = Theme.SwatchBorder,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        _layerTitle.Draw(renderPass, new Vector2(x, Rect.y + 160 + (TitleH - _layerTitle.Size.y) / 2),
            screen, clip.Min, clip.Max);
        _rigTitle.Draw(renderPass, new Vector2(x, Rect.y + 218 + (TitleH - _rigTitle.Size.y) / 2),
            screen, clip.Min, clip.Max);
        if (Editor.Document.Bones.Count == 0)
        {
            _hint.Draw(renderPass, new Vector2(x, Rect.y + 274),
                screen, clip.Min, clip.Max);
        }
    }
}
