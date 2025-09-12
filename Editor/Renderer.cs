using System.Numerics;
using System.Text.Json;
using WaffleEngine.UI;

namespace WaffleEngine.Rendering;

public class Renderer
{
    private WindowSdl _window;
    
    private Shader _shader;
    private Queue _queue;
    private GpuTexture _renderTexture = new();

    private Buffer<Vertex> vertices;
    private Buffer<int> indices;

    private Texture texture;
    private GpuTexture text;

    private ValueBox<uint> instances = new ValueBox<uint>(1);

    private UIToplevel ui;
    
    public Renderer(WindowSdl window)
    {
        _window = window;
        _queue = new Queue();
        
        texture = new Texture("textures/texture.png");
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/triangle", out _shader))
        {
            Log.Error("Shader not found");
            return;
        }
        
        vertices = new Buffer<Vertex>(BufferUsage.Vertex, 4);
        
        vertices.Add( new Vertex { Position = new Vector3(0.5f, 0.5f, 0f), Uv = new Vector2(1f, 0f)});
        vertices.Add( new Vertex { Position = new Vector3(0.5f, -0.5f, 0f), Uv = new Vector2(1f, 1f)});
        vertices.Add( new Vertex { Position = new Vector3(-0.5f, -0.5f, 0f), Uv = new Vector2(0f, 1f)});
        vertices.Add( new Vertex { Position = new Vector3(-0.5f, 0.5f, 0f), Uv = new Vector2(0f, 0f)});
        
        indices = new Buffer<int>(BufferUsage.Index, 6);
        indices.Add(0);
        indices.Add(1);
        indices.Add(2);
        indices.Add(0);
        indices.Add(2);
        indices.Add(3);
        
        //Upload Buffer to GPU
        var copyPass = new CopyPass();
        copyPass.AddCommand(new UploadBufferToGpu(vertices));
        copyPass.AddCommand(new UploadBufferToGpu(indices));
        copyPass.AddCommand(new UploadBufferToGpu(texture));
        _queue.AddPass(copyPass);
        _queue.Submit();
        _queue.Clear();
        
        //Create the render Queue

        text = new GpuTexture(window);
        
        Material material = new Material(_shader);
        material.AddBuffer(vertices, 0);
        material.AddBuffer(indices, 0);
        material.AddTexture(texture, 0);
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = Color.RGBA255(34, 40, 49, 255),
            GpuTexture = text,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };
        
        _queue.AddPreprocess(new GetSwapchain(_window, ref _renderTexture));
        RenderPass renderPass = new RenderPass(colorTargetSettings);
        renderPass.AddCommand(new Bind(material));
        renderPass.AddCommand(new DrawIndexedPrimatives(6, instances, 0, 0, 0));
        _queue.AddPass(renderPass);

        ui = new UIToplevel(window);
        ui.BackgroundColor = Color.RGBA255(49, 54, 63, 255);
        ui.Root = new UIRect()
            .SetWidth(UISize.Percentage(100))
            .SetHeight(UISize.Percentage(100))
            .AddUIElement(new UIRect()
                .SetColor(Color.RGBA255(34, 40, 49, 255))
                .SetWidth(UISize.Percentage(50))
                .SetHeight(UISize.Percentage(100))
                .SetMarginX(UISize.Pixels(12))
                .SetMarginY(UISize.Pixels(12))
                .SetBorderRadius(new Vector4(25f, 25f, 25f, 25f), UISizeType.Pixels))
            .AddUIElement(new UIRect()
                .SetColor(Color.RGBA255(34, 40, 49, 255))
                .SetWidth(UISize.Percentage(50))
                .SetHeight(UISize.Percentage(100))
                .SetMarginX(UISize.Pixels(12))
                .SetMarginY(UISize.Pixels(12))
                .SetBorderRadius(new Vector4(25f, 25f, 25f, 25f), UISizeType.Pixels)
                .SetTexture(text));
        
        _queue.AddPass(new UIPass(ui));
        
        BlitPass blitPass2 = new BlitPass(ui.UiTexture, _renderTexture, true);
        _queue.AddPass(blitPass2);
    }
    
    internal void Render()
    {
        // instances.SetValue(instances + 1);
        _queue.Submit();
    }
}