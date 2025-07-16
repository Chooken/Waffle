using System.Numerics;
using System.Text.Json;
using WaffleEngine.UI;

namespace WaffleEngine.Rendering;

public class Renderer
{
    private WindowSdl _window;
    
    private Shader _shader;
    private Queue _queue;
    private RenderTexture _renderTexture = new();

    private Buffer<Vertex> vertices;
    private Buffer<int> indices;

    private Texture texture;
    private RenderTexture text;

    private Value<uint> instances = new Value<uint>(1);

    private UIToplevel ui;
    
    public Renderer(WindowSdl window)
    {
        _window = window;
        
        texture = new Texture("textures/texture.png");
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/triangle", out _shader))
        {
            WLog.Error("Shader not found", "Renderer");
            return;
        }
        
        vertices = new Buffer<Vertex>(BufferUsage.Vertex, 4);
        
        vertices.Add( new Vertex { Position = new Vector3(0.5f, 0.5f, 0f), Uv = new Vector2(1f, 1f)});
        vertices.Add( new Vertex { Position = new Vector3(0.5f, -0.5f, 0f), Uv = new Vector2(1f, 0f)});
        vertices.Add( new Vertex { Position = new Vector3(-0.5f, -0.5f, 0f), Uv = new Vector2(0f, 0f)});
        vertices.Add( new Vertex { Position = new Vector3(-0.5f, 0.5f, 0f), Uv = new Vector2(0f, 1f)});
        
        indices = new Buffer<int>(BufferUsage.Index, 6);
        indices.Add(0);
        indices.Add(1);
        indices.Add(2);
        indices.Add(0);
        indices.Add(2);
        indices.Add(3);
        
        // Upload Buffer to GPU
        _queue = new Queue();
        var copyPass = new CopyPass();
        copyPass.AddCommand(new UploadBufferToGpu(vertices));
        copyPass.AddCommand(new UploadBufferToGpu(indices));
        copyPass.AddCommand(new UploadBufferToGpu(texture));
        _queue.AddPass(copyPass);
        _queue.Submit();
        _queue.Clear();
        
        // Create the render Queue

        text = new RenderTexture(128, 128, window);
        
        Material material = new Material(_shader);
        material.AddBuffer(vertices, 0);
        material.AddBuffer(indices, 0);
        material.AddTexture(texture, 0);
        
        _shader.Build();
        
        _shader.ReleaseGpuShaders();
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0.6f, 0.6f, 0.6f, 1.0f),
            RenderTexture = text,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };
        
        _queue.AddPreprocess(new GetSwapchain(_window, _renderTexture));
        RenderPass renderPass = new RenderPass(colorTargetSettings, material);
        renderPass.AddCommand(new DrawIndexedPrimatives(6, instances, 0, 0, 0));
        _queue.AddPass(renderPass);
        BlitPass blitPass = new BlitPass(text, _renderTexture);
        _queue.AddPass(blitPass);

        ui = new UIToplevel(window);
        ui.Root = new UIElement()
            .SetColor(new Color(0, 1, 1, 1));

        RenderTexture uiTexture = ui.Render();
        BlitPass blitPass2 = new BlitPass(uiTexture, _renderTexture);
        _queue.AddPass(blitPass2);
    }
    
    internal void Render()
    {
        instances.SetValue(instances + 1);
        ui.Render();
        _queue.Submit();
    }
}