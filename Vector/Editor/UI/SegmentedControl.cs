using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// macOS-style segmented control. Owner sets Options/Selected; tap picks.
public class SegmentedControl : Rect
{
    public List<string> Options = new();
    public int Selected;
    public Action<int>? OnSelect;

    private readonly UiText _label = new();
    private readonly UiText _labelAccent = new() { TextColorOverride = Color.White };

    public override void OnInit()
    {
        BorderRadius = new Vector4(8, 8, 8, 8);
    }

    public override void OnUpdate()
    {
        // Background live: theme switches apply immediately.
        Color = new Color(0, 0, 0, 0);
        BorderColor = Theme.Border;
        BorderSize = 1f;
    }

    public override void OnEvent(NodeEvent node_event)
    {
        if (node_event != NodeEvent.MouseClick
            || Options.Count == 0)
            return;

        int index = Math.Clamp(
            (int)((Tree.Input.Mouse.Position.x - Rect.x) / Rect.w * Options.Count),
            0, Options.Count - 1);
        if (index != Selected)
        {
            Selected = index;
            OnSelect?.Invoke(index);
        }
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        if (Options.Count == 0)
            return;

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        float segW = (float)Rect.w / Options.Count;

        if (Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            renderPass.Bind(shader);
            renderPass.SetUniforms(new Rect.UIRectData
            {
                Position = new AlignedVector3(Rect.x + segW * Selected + 2, Rect.y + 2, 0),
                Size = new Vector2(segW - 4, Rect.h - 4),
                Color = Theme.Accent,
                BorderRadius = new Vector4(6, 6, 6, 6),
                BorderColor = new Vector4(0, 0, 0, 0),
                ScreenSize = screen,
                BorderSize = 0f,
                ClipMin = clip.Min,
                ClipMax = clip.Max,
            });
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        for (int i = 0; i < Options.Count; i++)
        {
            var label = i == Selected ? _labelAccent : _label;
            label.SetText(Options[i]);
            label.Sync();
            float cx = Rect.x + segW * i + segW / 2;
            label.Draw(renderPass,
                new Vector2(cx - label.Size.x / 2, Rect.y + (Rect.h - label.Size.y) / 2),
                screen, clip.Min, clip.Max);
        }
    }
}
