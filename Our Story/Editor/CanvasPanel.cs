using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;
using WaffleEngine.UI;

namespace OurStory.Editor;

public class CanvasPanel
{
    public UIRect Panel;
    public UICrt Canvas;
    public Texture Texture;
    public GpuTexture CanvasTexture;
    public Color CursorColor;
    public Vector2 CursorPosition;
    public Shader CanvasBlitShader;
    
    private struct CanvasBlitUniforms
    {
        public Color SelectedColor;
        public Vector2 CursorPosition;
        public Vector2 TextureSize;
    }
    
    public CanvasPanel(Window window, uint width, uint height)
    {
        Assert.True(
            Assets.TryGetShader("Core", "texture_canvas", out CanvasBlitShader),
            "Canvas Blit Shader not found."
        );
        
        CanvasBlitShader.SetPipeline(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
            VertexAttributes = null
        });
        
        Texture = new Texture(width, height);
        CanvasTexture = new GpuTexture(GpuTextureSettings.Default(width, height) with
        {
            ColorTarget = true,
        });
        Canvas = new UICrt(new Vector2(width, height), 0.25f);
        Panel = new UIRect()
            .Default(() => new UISettings()
            {
                Grow = true,
                ChildAnchor = UIAnchor.Center
            })
            .AddUIElement(Canvas
                .Default(() =>
                {
                    CursorPosition = new Vector2(-1, -1);
                    
                    return new UISettings()
                    {
                        Width = UISize.PercentageWidth(100),
                        Height = UISize.PercentageHeight(100),
                        BorderRadius = new UIBorderRadius(5, 5, 5, 5, UISizeType.PercentageWidth),
                        BorderSize = UISize.Pixels(4),
                        BorderColor = Color.RGBA255(22, 22, 22, 255),
                        Texture = CanvasTexture,
                    };
                })
                .OnHover((ref UISettings settings) =>
                {
                    Vector2 position = window.WindowInput.MouseData.Position;
                    Vector3 uiPos = Canvas.LastPosition;
                    Vector2 uiSize = Canvas.LastSize;

                    CursorPosition = new Vector2(
                        (int)((position.x - uiPos.x) / uiSize.x * Canvas.Resolution.x),
                        (int)((position.y - uiPos.y) / uiSize.y * Canvas.Resolution.y));
                })
                .OnHold((ref UISettings settings) =>
                {
                    Vector2 position = window.WindowInput.MouseData.Position;
                    Vector3 uiPos = Canvas.LastPosition;
                    Vector2 uiSize = Canvas.LastSize;

                    Vector2 texturePos = new Vector2(
                        (position.x - uiPos.x) / uiSize.x * Canvas.Resolution.x,
                        (position.y - uiPos.y) / uiSize.y * Canvas.Resolution.y);

                    Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4] =
                        CursorColor.r255;
                    Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4 + 1] =
                        CursorColor.g255;
                    Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4 + 2] =
                        CursorColor.b255;
                    Texture.Data[((int)texturePos.x + (int)texturePos.y * Texture.Width) * 4 + 3] =
                        CursorColor.a255;

                    ImQueue queue = new ImQueue();
                    ImCopyPass copyPass = queue.AddCopyPass();
                    copyPass.Upload(Texture);
                    copyPass.End();
                    queue.Submit();
                })
            );
    }

    public void RenderCanvas(ImQueue queue)
    {
        ImRenderPass renderPass = queue.AddRenderPass(new ColorTargetSettings()
        {
            GpuTexture = CanvasTexture,
            ClearColor = Color.RGBA255(24, 24, 24, 255),
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        });
        
        renderPass.Bind(CanvasBlitShader);
        renderPass.Bind(Texture);
        renderPass.SetUniforms(new CanvasBlitUniforms
        {
            SelectedColor = CursorColor,
            CursorPosition = CursorPosition,
            TextureSize = new Vector2(Texture.Width, Texture.Height),
        });
        renderPass.DrawPrimatives(3, 1, 0, 0);
        renderPass.End();
    }

    public static implicit operator UIRect(CanvasPanel panel) => panel.Panel;
}