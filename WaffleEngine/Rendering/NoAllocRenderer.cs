using System.Numerics;
using System.Text.Json;

namespace WaffleEngine.Rendering;

public class NoAllocRenderer
{
    private WindowSdl _window;
    
    private Pipeline? _pipeline;
    private Shader _shader;
    private RenderTexture _renderTexture = new();

    private Buffer<Vertex> vertices;
    private Buffer<int> indices;

    private Texture texture;
    private RenderTexture text;
    
    public NoAllocRenderer(WindowSdl window)
    {
        _window = window;
        
        texture = new Texture("textures/texture.png");
        
        if (!ShaderManager.TryGetShader("BuiltinShaders/triangle", out _shader))
        {
            WLog.Error("Shader not found", "Renderer");
            return;
        }
        
        _shader.Build();
        
        //_pipeline = Pipeline.BuildRasterizer(_window, _shader);
        if (!Pipeline.TryBuild(PipelineSettings.Default, _shader, out _pipeline))
        {
            return;
        }
        
        _shader.ReleaseGpuShaders();
        
        vertices = new Buffer<Vertex>(BufferUsage.Vertex, 4);
        vertices[0] = new Vertex { Position = new Vector3(0.5f, 0.5f, 0f), Uv = new Vector2(1f, 1f)};
        vertices[1] = new Vertex { Position = new Vector3(0.5f, -0.5f, 0f), Uv = new Vector2(1f, 0f)};
        vertices[2] = new Vertex { Position = new Vector3(-0.5f, -0.5f, 0f), Uv = new Vector2(0f, 0f)};
        vertices[3] = new Vertex { Position = new Vector3(-0.5f, 0.5f, 0f), Uv = new Vector2(0f, 1f)};

        indices = new Buffer<int>(BufferUsage.Index, 6);
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;
        
        // Upload Buffer to GPU
        var queue = new NoAlloc.Queue();
        var copyPass = new NoAlloc.CopyPass();
        copyPass.Start(queue);
        copyPass.UploadToGpu(vertices);
        copyPass.UploadToGpu(indices);
        copyPass.UploadToGpu(texture);
        copyPass.End();
        queue.Submit();
        
        // Create the render Queue

        text = new RenderTexture(window);
    }
    
    internal void Render()
    {
        if (_pipeline is null)
            return;
        
        var queue = new NoAlloc.Queue();
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0.6f, 0.6f, 0.6f, 1.0f),
            RenderTexture = text,
            LoadOperation = LoadOperation.Clear,
            StoreOperation = StoreOperation.Store,
        };

        queue.TryGetSwapchainTexture(_window, ref _renderTexture);
        var renderPass = new NoAlloc.RenderPass(colorTargetSettings, _pipeline);
        renderPass.Start(queue);
        renderPass.Bind(vertices, 0);
        renderPass.Bind(indices, 0);
        renderPass.Bind(texture, 0);
        renderPass.DrawIndexedPrimatives(6, 1, 0,0,0);
        renderPass.End();
        var blitPass = new NoAlloc.BlitPass(text, _renderTexture);
        blitPass.Start(queue);
        queue.Submit();
    }
}