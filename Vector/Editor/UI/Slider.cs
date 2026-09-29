using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Horizontal 0..1 slider in the macOS idiom: thin track, accent fill,
// round white knob. Owner sets Label/Value; drag calls OnChange.
// Grab/release actions let owners checkpoint once per gesture.
public class Slider : Rect
{
    public string Label = "";
    public float Value;
    public Action<float>? OnChange;
    public Action? OnGrab;
    public Action? OnRelease;

    // Gradient track (color pickers): when true the bar renders Gradient
    // through ui-gradient instead of the flat rest/fill look.
    public bool GradientTrack;
    public UiGradientData Gradient;

    private const int Pad = 8;
    private const int LabelW = 52;
    private const float TrackH = 10f;
    private const float KnobD = 16f;
    private bool _drag;
    private readonly UiText _label = new();

    public override void OnInit()
    {
        Color = new Color(0, 0, 0, 0);
    }

    public override void OnUpdate()
    {
        _label.SetText(Label);
        _label.Sync();
    }

    private float ValueFromMouse(float mouseX)
    {
        float track = Math.Max(1, Rect.w - LabelW - Pad * 3);
        return Math.Clamp((mouseX - Rect.x - LabelW - Pad * 2) / track, 0f, 1f);
    }

    private void SetFromMouse(float mouseX)
    {
        float next = ValueFromMouse(mouseX);
        if (next != Value)
        {
            Value = next;
            OnChange?.Invoke(next);
        }
    }

    public override void OnEvent(NodeEvent node_event)
    {
        switch (node_event)
        {
            case NodeEvent.MouseHold:
                if (Tree.Input.Mouse.IsLeftPressed)
                {
                    _drag = true;
                    OnGrab?.Invoke();
                    SetFromMouse(Tree.Input.Mouse.Position.x);
                }
                else if (_drag && Tree.Input.Mouse.IsLeftDown)
                {
                    SetFromMouse(Tree.Input.Mouse.Position.x);
                }
                break;
            case NodeEvent.MouseClick when _drag:
                _drag = false;
                OnRelease?.Invoke();
                break;
        }
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        if (Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            float trackX = Rect.x + LabelW + Pad * 2;
            float trackW = Math.Max(0, Rect.w - LabelW - Pad * 3);
            float barY = Rect.y + (Rect.h - TrackH) / 2;
            float knobX = trackX + trackW * Value;
            renderPass.Bind(shader);
            bool gradientDrawn = false;
            if (GradientTrack
                && Assets.TryGetShader("builtin", "ui-gradient", out var gradient))
            {
                var g = Gradient;
                g.Position = new Vector4(trackX, barY, 0, 0);
                g.Size = new Vector4(trackW, TrackH, 0, 0);
                g.RenderSize = new Vector4(screen.x, screen.y, 0, 0);
                g.Clip = new Vector4(clip.Min.x, clip.Min.y, clip.Max.x, clip.Max.y);
                renderPass.Bind(gradient);
                renderPass.SetUniforms(g);
                renderPass.DrawPrimatives(6, 1, 0, 0);
                renderPass.Bind(shader);
                gradientDrawn = true;
            }
            if (!gradientDrawn && trackW > 0.5f)
            {
                renderPass.SetUniforms(new Rect.UIRectData
                {
                    Position = new AlignedVector3(trackX, barY, 0),
                    Size = new Vector2(trackW, TrackH),
                    Color = Theme.Highlight,
                    BorderRadius = new Vector4(TrackH, TrackH, TrackH, TrackH),
                    BorderColor = new Vector4(0, 0, 0, 0),
                    ScreenSize = screen,
                    BorderSize = 0f,
                    ClipMin = clip.Min,
                    ClipMax = clip.Max,
                });
                renderPass.DrawPrimatives(6, 1, 0, 0);
            }
            float fillW = trackW * Value;
            if (!gradientDrawn && fillW > 0.5f)
            {
                renderPass.SetUniforms(new Rect.UIRectData
                {
                    Position = new AlignedVector3(trackX, barY, 0),
                    Size = new Vector2(fillW, TrackH),
                    Color = Theme.Accent,
                    BorderRadius = new Vector4(2, 2, 2, 2),
                    BorderColor = new Vector4(0, 0, 0, 0),
                    ScreenSize = screen,
                    BorderSize = 0f,
                    ClipMin = clip.Min,
                    ClipMax = clip.Max,
                });
                renderPass.DrawPrimatives(6, 1, 0, 0);
            }
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(knobX - KnobD / 2, Rect.y + (Rect.h - KnobD) / 2, 0),
                Size = new Vector2(KnobD, KnobD),
                Color = Color.White,
                BorderRadius = new Vector4(KnobD / 2, KnobD / 2, KnobD / 2, KnobD / 2),
                BorderColor = Theme.Border,
                ScreenSize = screen,
                BorderSize = 2f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        _label.Draw(renderPass,
            new Vector2(Rect.x + Pad, Rect.y + (Rect.h - _label.Size.y) / 2),
            screen, clip.Min, clip.Max);
    }
}
