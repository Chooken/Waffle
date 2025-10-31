using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using SDL3;
using WaffleEngine.Rendering.Immediate;

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
    public void Submit(ImQueue queue);
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
    public unsafe void Submit(ImQueue queue)
    {
        ImCopyPass copyPass = queue.AddCopyPass();
        
        foreach (var command in commands)
        {
            command.Add(copyPass);
        }
        
        copyPass.End();
    }
}

public sealed class RenderPass(ColorTargetSettings colorTargetSettings) : PassCommands<IGpuRenderCommand>, IPass
{
    public unsafe void Submit(ImQueue queue)
    {
        ImRenderPass renderPass = queue.AddRenderPass(colorTargetSettings);
        
        foreach (var command in commands)
        {
            command.Add(renderPass);
        }
        
        renderPass.End();
    }
}

public sealed class BlitPass(GpuTexture source, GpuTexture destination, bool clear) : IPass
{
    public void Submit(ImQueue queue)
    {
        queue.AddBlitPass(source, destination, clear);
    }
}