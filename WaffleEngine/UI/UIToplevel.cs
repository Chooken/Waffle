using System.Numerics;
using GarbagelessSharp;
using WaffleEngine.Rendering;

namespace WaffleEngine.UI;

public class UIToplevel
{
    public readonly WindowSdl Window;
    public UIRect? Root;

    private ValueBox<uint> _childCount;
    private ValueBox<Vector2> _uniformData;

    private Queue _queue;
    private GpuTexture _uiTexture;

    public UIToplevel(WindowSdl window)
    {
        Window = window;
        _uiTexture = new GpuTexture(Window);
        _childCount = new ValueBox<uint>(0);
        _queue = new Queue();
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/ui-rect", out Shader? shader))
        {
            WLog.Error("Shader not found", "Renderer");
            return;
        }
        
        shader.SetPipeline(new PipelineSettings()
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
        });

        _uniformData = new ValueBox<Vector2>(new Vector2(window.Width, window.Height));
        
        Material material = new Material(shader);
        material.AddBuffer(UIRect.GpuData, 0);
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0f, 0f, 0f, 0f),
            GpuTexture = _uiTexture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        CopyPass copyPass = new CopyPass();
        copyPass.AddCommand(new UploadBufferToGpu(UIRect.GpuData));
        _queue.AddPass(copyPass);
        
        _queue.AddPass(new SetUniforms<Vector2>(_uniformData));

        RenderPass renderPass = new RenderPass(colorTargetSettings, material);
        renderPass.AddCommand(new DrawPrimatives(6, _childCount, 0, 0));
        _queue.AddPass(renderPass);
    }

    public GpuTexture Render()
    {
        if (Root is null)
            return _uiTexture;
        
        Root.Update();

        bool resized = _uniformData.Value != new Vector2(Window.Width, Window.Height);
        
        if (resized)
        {
            _uiTexture.Resize((uint)Window.Width, (uint)Window.Height);
        }

        if (Root.Dirty || resized)
        {
            UIRect.GpuData.Clear();
            Root.AddToBuffer(new Vector3(-1, -1, 0), new Vector2(_uiTexture.Width, _uiTexture.Height), new Vector2(_uiTexture.Width, _uiTexture.Height));
            _childCount.Value = (uint)UIRect.GpuData.Count;
        }

        _uniformData.Value = new Vector2(_uiTexture.Width, _uiTexture.Height);

        _queue.Submit();

        return _uiTexture;
    }

    public UIToplevel SetRoot(UIRect uiRect)
    {
        Root = uiRect;
        return this;
    }
}