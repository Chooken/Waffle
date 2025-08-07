using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using SDL3;

namespace WaffleEngine.Rendering;

public struct ColorTargetSettings
{
    public RenderTexture RenderTexture;
    public Color ClearColor;
    public LoadOperation LoadOperation;
    public StoreOperation StoreOperation;
}

public enum LoadOperation
{
    Load,
    Clear,
    DontCare,
}

public enum StoreOperation
{
    Store,
    DontCare,
    Resolve,
    ResolveAndStore,
}

public interface IPass
{
    public void Submit(IntPtr commandBuffer);
}

public abstract class PassCommands<T>
{
    protected List<T> commands = new List<T>();

    public void AddCommand(T command)
    {
        commands.Add(command);
    }
}

public sealed class CopyPass : PassCommands<IGpuCopyCommand>, IPass
{
    public unsafe void Submit(IntPtr commandBuffer)
    {
        IntPtr copyPass = SDL.BeginGPUCopyPass(commandBuffer);
        
        foreach (var command in commands)
        {
            command.Add(copyPass);
        }
        
        SDL.EndGPUCopyPass(copyPass);
    }
}

public sealed class RenderPass(ColorTargetSettings colorTargetSettings, Material material) : PassCommands<IGpuRenderCommand>, IPass
{
    public unsafe void Submit(IntPtr commandBuffer)
    {
        if (colorTargetSettings.RenderTexture.Texture == IntPtr.Zero)
        {
            WLog.Error("RenderPass was given a null texture");
            return;
        }
        
        SDL.GPUColorTargetInfo colorTargetInfo = new SDL.GPUColorTargetInfo();
        colorTargetInfo.Texture = colorTargetSettings.RenderTexture.Texture;
        colorTargetInfo.ClearColor = new SDL.FColor
        {
            R = colorTargetSettings.ClearColor.r, 
            G = colorTargetSettings.ClearColor.g, 
            B = colorTargetSettings.ClearColor.b, 
            A = colorTargetSettings.ClearColor.a
        };
        colorTargetInfo.LoadOp = (SDL.GPULoadOp) colorTargetSettings.LoadOperation;
        colorTargetInfo.StoreOp = (SDL.GPUStoreOp) colorTargetSettings.StoreOperation;
        
        IntPtr renderPass = SDL.BeginGPURenderPass(commandBuffer, (IntPtr)(&colorTargetInfo), 1, IntPtr.Zero);
        
        material.Bind(renderPass);

        foreach (var command in commands)
        {
            command.Add(renderPass);
        }
        
        SDL.EndGPURenderPass(renderPass);
    }
}

public sealed class BlitPass : IPass
{
    private readonly RenderPass? _renderPass;
    
    public BlitPass(RenderTexture source, RenderTexture destination, bool clear)
    {
        if (!ShaderManager.TryGetShader("BuiltinShaders/blit", out Shader? shader))
        {
            WLog.Error("Shader not found", "Renderer");
            return;
        }
        
        shader.SetPipelineSettings(new PipelineSettings()
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
        
        Material material = new Material(shader);
        material.AddTexture(source, 0);
        material.Build();
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0.0f, 0.0f, 0.0f, 0.0f),
            RenderTexture = destination,
            LoadOperation = clear ? LoadOperation.Clear : LoadOperation.Load,
            StoreOperation = StoreOperation.Store,
        };

        _renderPass = new RenderPass(colorTargetSettings, material);
        _renderPass.AddCommand(new DrawPrimatives(3, 1, 0, 0));
    }
    
    public void Submit(IntPtr commandBuffer)
    {
        _renderPass?.Submit(commandBuffer);
    }
}