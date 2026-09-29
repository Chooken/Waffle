using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// Horizontal 0..1 slider. Owner sets Label/Value; drag calls OnChange.
// Grab/release actions let owners checkpoint once per gesture.
public class Slider : Rect
{
    public string Label = "";
    public float Value;
    public Action<float>? OnChange;
    public Action? OnGrab;
    public Action? OnRelease;

    private const int Pad = 8;
    private const int LabelW = 52;
    private bool _drag;
    private readonly UiText _label = new();

    public override void OnInit()
    {
        Color = Theme.TrackBackground;
        BorderRadius = new Vector4(Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius, Theme.CornerRadius);
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
                if (Input.Mouse.IsLeftPressed && Rect.Contains(Input.Mouse.Position))
                {
                    _drag = true;
                    OnGrab?.Invoke();
                    SetFromMouse(Input.Mouse.Position.x);
                }
                else if (_drag && Input.Mouse.IsLeftDown)
                {
                    SetFromMouse(Input.Mouse.Position.x);
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
            float barH = 8;
            float barY = Rect.y + (Rect.h - barH) / 2;
            renderPass.Bind(shader);
            float restW = trackW * (1 - Value);
            if (restW > 0.5f)
            {
                renderPass.SetUniforms(new Rect.UIRectData
                {
                    Position = new AlignedVector3(trackX + trackW * Value, barY, 0),
                    Size = new Vector2(restW, barH),
                    Color = Theme.TrackRest,
                    BorderRadius = new Vector4(Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius),
                    BorderColor = new Vector4(0, 0, 0, 0),
                    ScreenSize = screen,
                    BorderSize = 0f,
                    ClipMin = clip.Min,
                    ClipMax = clip.Max,
                });
                renderPass.DrawPrimatives(6, 1, 0, 0);
            }
            float fillW = trackW * Value;
            if (fillW > 0.5f)
            {
                renderPass.SetUniforms(new Rect.UIRectData
                {
                    Position = new AlignedVector3(trackX, barY, 0),
                    Size = new Vector2(fillW, barH),
                    Color = Theme.TrackFill,
                    BorderRadius = new Vector4(Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius, Theme.ChipRadius),
                    BorderColor = new Vector4(0, 0, 0, 0),
                    ScreenSize = screen,
                    BorderSize = 0f,
                    ClipMin = clip.Min,
                    ClipMax = clip.Max,
                });
                renderPass.DrawPrimatives(6, 1, 0, 0);
            }
        }

        _label.Draw(renderPass,
            new Vector2(Rect.x + Pad, Rect.y + (Rect.h - _label.Size.y) / 2),
            screen, clip.Min, clip.Max);
    }
}
