using WaffleEngine;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace OurStory.Editor;

public class Canvas
{
    private Texture _texture;
    private GpuTexture _canvasTexture;
    private List<(Color color, Vector2 position)> _tempPixels = new ();
    
    private struct CanvasBlitUniforms
    {
        public Color SelectedColor;
        public Vector2 CursorPosition;
        public Vector2 TextureSize;
    }

    private Shader _canvasBlitShader;

    public Canvas(uint width, uint height)
    {
        Assert.True(
            Assets.TryGetShader("Core", "texture_canvas", out _canvasBlitShader),
            "Canvas Blit Shader not found."
        );
        
        _canvasBlitShader.SetPipeline(new PipelineSettings()
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
        
        _texture = new Texture(width, height);
        _canvasTexture = new GpuTexture(GpuTextureSettings.Default(width, height) with
        {
            ColorTarget = true,
        });
    }

    public Color GetColor(uint x, uint y)
    {
        var data = _texture.GetAs<(byte r, byte g, byte b, byte a)>();

        var color = data[(int)x + (int)y * _texture.Width];

        return Color.RGBA255(color.r, color.g, color.b, color.a);
    }

    public void SetPixel(Color color, uint x, uint y)
    {
        _texture.Data[(int)(x + y * _texture.Width) * 4] =
            color.r255;
        _texture.Data[(int)(x + y * _texture.Width) * 4 + 1] =
            color.g255;
        _texture.Data[(int)(x + y * _texture.Width) * 4 + 2] =
            color.b255;
        _texture.Data[(int)(x + y * _texture.Width) * 4 + 3] =
            color.a255;

        ImQueue queue = new ImQueue();
        ImCopyPass copyPass = queue.AddCopyPass();
        copyPass.Upload(_texture);
        copyPass.End();
        queue.Submit();
    }

    public void SetTempPixel(Color color, Vector2 position)
    {
        _tempPixels.Add((color, position));
    }

    public void Render(ref ImQueue queue)
    {
        queue.AddBlitPass(_texture, _canvasTexture, true);
        
        ImRenderPass renderPass = queue.AddRenderPass(new ColorTargetSettings()
        {
            GpuTexture = _canvasTexture,
            ClearColor = Color.RGBA255(24, 24, 24, 255),
            LoadOperation = LoadOperation.Load,
            StoreOperation = StoreOperation.Store,
        });
        
        renderPass.Bind(_canvasBlitShader);

        foreach (var temp in _tempPixels)
        {
            renderPass.SetUniforms(new CanvasBlitUniforms
            {
                SelectedColor = temp.color,
                CursorPosition = temp.position,
                TextureSize = new Vector2(_canvasTexture.Width, _canvasTexture.Width),
            });
            renderPass.DrawPrimatives(3, 1, 0, 0);
        }
        _tempPixels.Clear();
        
        renderPass.End();
    }

    public GpuTexture GetCanvas()
    {
        return _canvasTexture;
    }
}