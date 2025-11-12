using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI.Old;

namespace WaffleEngine.UI;

public class Rect : UiElement
{
    public override void Render(ImRenderPass renderPass, Vector2 renderSize, float scale)
    {
        if (!Assets.TryGetShader("builtin", "ui-rect", out var shader))
        {
            WLog.Error("Shader not found");
            return;
        }
        
        shader.SetPipeline(new PipelineSettings
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.One,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
            VertexAttributes = null,
        });
        
        UIRectData data = new UIRectData()
        {
            Position = new AlignedVector3(Bounds.CalulatedPosition * scale),
            Size = new Vector2(Bounds.CalculatedWidth * scale, Bounds.CalculatedHeight * scale),
            Color = Settings.Color,
            BorderRadius = new Vector4(
                Settings.BorderRadius.BottomLeft * scale, 
                Settings.BorderRadius.TopLeft * scale, 
                Settings.BorderRadius.BottomRight * scale, 
                Settings.BorderRadius.TopRight * scale),
            BorderColor = Settings.BorderColor,
            ScreenSize = renderSize,
            BorderSize = Settings.BorderSize * scale,
        };
        
        if (Settings.Color.a != 0)
        {
            renderPass.SetUniforms(data);
            renderPass.Bind(shader);
            renderPass.DrawPrimatives(6, 1, 0, 0);
        }

        foreach (var child in Children)
        {
            child.Render(renderPass, renderSize, scale);
        }
    }
}