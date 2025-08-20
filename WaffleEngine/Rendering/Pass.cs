using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using SDL3;

namespace WaffleEngine.Rendering;

public struct ColorTargetSettings
{
    public GpuTexture GpuTexture;
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

public sealed class SetUniforms<T>(ValueBox<T> data) : IPass
{
    public unsafe void Submit(IntPtr commandBuffer)
    {
        SDL.PushGPUFragmentUniformData(commandBuffer, 0, (IntPtr)Unsafe.AsPointer(ref data.Value), (uint) Unsafe.SizeOf<T>());
    }
}

public sealed class RenderListPass(ColorTargetSettings colorTargetSettings, List<Material> materials) : PassCommands<IGpuRenderCommand>, IPass
{
    public unsafe void Submit(IntPtr commandBuffer)
    {
        if (colorTargetSettings.GpuTexture.Handle == IntPtr.Zero)
        {
            WLog.Error("RenderPass was given a null texture");
            return;
        }

        SDL.GPUColorTargetInfo colorTargetInfo = new SDL.GPUColorTargetInfo();
        colorTargetInfo.Texture = colorTargetSettings.GpuTexture.Handle;
        colorTargetInfo.ClearColor = new SDL.FColor
        {
            R = colorTargetSettings.ClearColor.r,
            G = colorTargetSettings.ClearColor.g,
            B = colorTargetSettings.ClearColor.b,
            A = colorTargetSettings.ClearColor.a
        };
        colorTargetInfo.LoadOp = (SDL.GPULoadOp)colorTargetSettings.LoadOperation;
        colorTargetInfo.StoreOp = (SDL.GPUStoreOp)colorTargetSettings.StoreOperation;

        IntPtr renderPass = SDL.BeginGPURenderPass(commandBuffer, (IntPtr)(&colorTargetInfo), 1, IntPtr.Zero);

        foreach (var material in materials)
        {
            material.Bind(renderPass);

            foreach (var command in commands)
            {
                command.Add(renderPass);
            }
        }

        SDL.EndGPURenderPass(renderPass);
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
        if (colorTargetSettings.GpuTexture.Handle == IntPtr.Zero)
        {
            WLog.Error("RenderPass was given a null texture");
            return;
        }
        
        SDL.GPUColorTargetInfo colorTargetInfo = new SDL.GPUColorTargetInfo();
        colorTargetInfo.Texture = colorTargetSettings.GpuTexture.Handle;
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
    
    public BlitPass(GpuTexture source, GpuTexture destination, bool clear)
    {
        if (!ShaderManager.TryGetShader("BuiltinShaders/blit", out Shader? shader))
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
        
        Material material = new Material(shader);
        material.AddTexture(source, 0);
        
        ColorTargetSettings colorTargetSettings = new ColorTargetSettings
        {
            ClearColor = new Color(0.0f, 0.0f, 0.0f, 0.0f),
            GpuTexture = destination,
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