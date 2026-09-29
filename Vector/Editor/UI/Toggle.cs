using WaffleEngine;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;
using Rect = WaffleEngine.UI.Nodes.Rect;

namespace Vector.Editor.UI;

// iOS-style switch. Owner draws the row label; the toggle draws itself.
public class Toggle : Rect
{
    public bool Value;
    public Action<bool>? OnChange;

    public override void OnInit()
    {
        Color = new Color(0, 0, 0, 0);
    }

    public override void OnEvent(NodeEvent node_event)
    {
        if (node_event == NodeEvent.MouseClick)
        {
            Value = !Value;
            OnChange?.Invoke(Value);
        }
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        base.OnDraw(renderPass, screenSize);

        if (!Assets.TryGetShader("builtin", "ui-rect", out var shader))
            return;

        var screen = new Vector2(screenSize.w, screenSize.h);
        IRect clip = Tree.Clipstack.TryPeek(out IRect top) ? top : screenSize;

        float w = 46f;
        float h = 26f;
        float x = Rect.x + Rect.w - w;
        float y = Rect.y + (Rect.h - h) / 2;

        renderPass.Bind(shader);
        renderPass.SetUniforms(new Rect.UIRectData
        {
            Position = new AlignedVector3(x, y, 0),
            Size = new Vector2(w, h),
            Color = Value ? Theme.Accent : Theme.Highlight,
            BorderRadius = new Vector4(h / 2, h / 2, h / 2, h / 2),
            BorderColor = new Vector4(0, 0, 0, 0),
            ScreenSize = screen,
            BorderSize = 0f,
            ClipMin = clip.Min,
            ClipMax = clip.Max,
        });
        renderPass.DrawPrimatives(6, 1, 0, 0);

        float knob = h - 6f;
        float knobX = Value ? x + w - knob - 3f : x + 3f;
        renderPass.SetUniforms(new Rect.UIRectData
        {
            Position = new AlignedVector3(knobX, y + 3f, 0),
            Size = new Vector2(knob, knob),
            Color = Color.White,
            BorderRadius = new Vector4(knob / 2, knob / 2, knob / 2, knob / 2),
            BorderColor = new Vector4(0, 0, 0, 0),
            ScreenSize = screen,
            BorderSize = 0f,
            ClipMin = clip.Min,
            ClipMax = clip.Max,
        });
        renderPass.DrawPrimatives(6, 1, 0, 0);
    }
}
