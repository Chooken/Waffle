using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Oklch fill picker for the active shape. Slider positions live in a
// ColorIntent, decoupled from the model color: sRGB clipping would otherwise
// yank them back to "what's allowed" mid-drag. Writes go through
// Oklch.ToRGB, which arrives gamma-encoded from the source.
public class ColorPanel : Rect
{
    public Vector.Scenes.AssetEditor Editor = null!;

    public const int PanelH = 162;
    private const int TitleH = 24;
    private const int SwatchH = 28;
    private const int SliderH = 28;

    private Slider _hue = null!;
    private Slider _light = null!;
    private Slider _chroma = null!;
    private readonly UiText _title = new();
    private readonly ColorIntent _intent = new();
    private bool _checkpointArmed;

    public override void OnInit()
    {
        Color = Theme.BarBackground;
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

        AddNode(_hue);
        AddNode(_light);
        AddNode(_chroma);
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
        var shape = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
        _intent.SyncTo(shape);

        string title = shape is null ? "Fill" : $"Fill · {shape.Name}";
        _title.SetText(title);
        _title.Sync();

        _hue.Label = $"H {_intent.H * 360f:0}";
        _light.Label = $"L {_intent.L:0.00}";
        _chroma.Label = $"C {_intent.C * ColorIntent.MaxChroma:0.00}";
        _hue.Value = _intent.H;
        _light.Value = _intent.L;
        _chroma.Value = _intent.C;

        float y = Rect.y + TitleH + 2;
        _hue.SetRect(new IRect { x = Rect.x + 6, y = (int)y, w = Rect.w - 12, h = SliderH });
        y += SliderH + 2;
        _light.SetRect(new IRect { x = Rect.x + 6, y = (int)y, w = Rect.w - 12, h = SliderH });
        y += SliderH + 2;
        _chroma.SetRect(new IRect { x = Rect.x + 6, y = (int)y, w = Rect.w - 12, h = SliderH });
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        _title.Draw(renderPass,
            new Vector2(Rect.x + 10, Rect.y + (TitleH - _title.Size.y) / 2),
            screen, clip.Min, clip.Max);

        var shape = Editor.Document.FindShape(Editor.Selection.ActiveShapeId);
        if (shape is not null && Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            float y = Rect.y + Rect.h - SwatchH - 4;
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x + 6, y, 0),
                Size = new Vector2(Rect.w - 12, SwatchH),
                Color = shape.Color,
                BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius),
                BorderColor = Theme.HandleBorderColor,
                ScreenSize = screen,
                BorderSize = Theme.SwatchBorder,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }
    }
}
