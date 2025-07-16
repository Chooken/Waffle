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

public sealed class BlitPass(RenderTexture source, RenderTexture destination) : IPass
{
    public unsafe void Submit(IntPtr commandBuffer)
    {
        var blitInfo = new SDL.GPUBlitInfo
        {
            ClearColor = new SDL.FColor(0, 0, 0, 0),
            Source = new SDL.GPUBlitRegion
            {
                W = source.Width,
                H = source.Height,
                X = 0,
                Y = 0,
                Texture = source.Texture,
            },
            Destination = new SDL.GPUBlitRegion
            {
                W = destination.Width,
                H = destination.Height,
                X = 0,
                Y = 0,
                Texture = destination.Texture,
            },
            Filter = SDL.GPUFilter.Nearest,
            LoadOp = SDL.GPULoadOp.Clear,
            Cycle = 1,
        };
        
        SDL.BlitGPUTexture(commandBuffer, blitInfo);
    }
}