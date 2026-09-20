using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.UI.Nodes;

public class Rect : INode
{
    public Color Color;
    public Vector4 BorderRadius;
    public float BorderSize;
    public Color BorderColor;
    
    public struct UIRectData
    {
        public AlignedVector3 Position;
        public Vector2 Size;
        public Vector4 Color;
        public Vector4 BorderRadius;
        public Vector4 BorderColor;
        public Vector2 ScreenSize;
        public Vector2 ClipMin;
        public Vector2 ClipMax;
        public float BorderSize;
    }

    public override void OnDraw(ImRenderPass renderPass, IRect screenSize)
    {
        if (!Assets.TryGetShader("builtin", "ui-rect", out Shader? shader))
        {
            return;
        }
        
        IRect clip = Tree.Clipstack.TryPeek(out IRect clip_top)
            ? clip_top
            : screenSize;
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Rect.x, Rect.y, 0),
            Size = new Vector2(Rect.Width, Rect.Height),
            Color = Color,
            BorderRadius = BorderRadius,
            BorderColor = BorderColor,
            ScreenSize = new Vector2(screenSize.x, screenSize.y),
            BorderSize = BorderSize,
            ClipMin = clip.Min,
            ClipMax = clip.Max,
        };
        
        renderPass.Bind(shader);
        renderPass.SetUniforms(data);
        renderPass.DrawPrimatives(6, 1, 0, 0);
        
        base.OnDraw(renderPass, screenSize);
    }
}