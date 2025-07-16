using System.Numerics;
using WaffleEngine.Rendering;

namespace WaffleEngine.UI;

public class UIToplevel
{
    public readonly WindowSdl Window;
    public UIElement? Root;

    private Value<uint> _childCount;

    private Queue _queue;
    private RenderTexture _uiTexture;

    public UIToplevel(WindowSdl window)
    {
        Window = window;
        _uiTexture = new RenderTexture(Window);
        _childCount = new Value<uint>(0);
        _queue = new Queue();
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/uielement", out Shader? shader))
        {
            WLog.Error("Shader not found", "Renderer");
            return;
        }
        
        shader.SetPipelineSettings(new PipelineSettings()
        {
            ColorBlendOp = BlendOp.Add,
            AlphaBlendOp = BlendOp.Add,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            SrcAlphaBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorTargetFormat = TextureFormat.B8G8R8A8Unorm,
            PrimitiveType = PrimitiveType.TriangleList,
            FillMode = FillMode.Fill,
            VertexInputRate = VertexInputRate.Vertex,
        });
        
        shader.Build();
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0.6f, 0.6f, 0.6f, 1.0f),
            RenderTexture = _uiTexture,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        CopyPass copyPass = new CopyPass();
        copyPass.AddCommand(new UploadBufferToGpu(UIElement.GpuData));
        _queue.AddPass(copyPass);

        Material material = new Material(shader);
        material.AddBuffer(UIElement.GpuData, 0);

        RenderPass renderPass = new RenderPass(colorTargetSettings, material);
        renderPass.AddCommand(new DrawPrimatives(6, _childCount, 0, 0));
        _queue.AddPass(renderPass);
        
        Root?.AddToBuffer(Vector3.Zero);
    }

    public RenderTexture Render()
    {
        if (Root is null)
            return _uiTexture;
        
        Root.Update();
        
        if (Root.Dirty)
            Root.AddToBuffer(Vector3.Zero);

        _childCount.SetValue((uint)UIElement.GpuData.Count);
        
        _queue.Submit();

        return _uiTexture;
    }

    public UIToplevel SetRoot(UIElement uiElement)
    {
        Root = uiElement;
        return this;
    }
}