using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public interface IPreprocess
{
    public void Run(ImQueue queue);
}

public sealed class GetSwapchain(WindowSdl window, ref GpuTexture texture) : IPreprocess
{
    private GpuTexture _texture = texture;

    public void Run(ImQueue queue)
    {
        if (!queue.TryGetSwapchainTexture(window, ref _texture))
        {
            WLog.Error("Failed to retrieve swapchain texture");
            return;
        }
    }
}

public sealed class Queue
{
    private List<IPreprocess> _preprocesses = new List<IPreprocess>();
    private List<IPass> _passes = new List<IPass>();

    public void AddPreprocess(IPreprocess preprocess)
    {
        _preprocesses.Add(preprocess);
    }
    
    public void AddPass(IPass command)
    {
        _passes.Add(command);
    }

    public void Submit()
    {
        ImQueue imQueue = new ImQueue();
        
        if (!imQueue.Active)
        {
            WLog.Error("Failed to acquire command buffer");
            return;
        }

        foreach (var preprocess in _preprocesses)
        {
            preprocess.Run(imQueue);
        }
        
        foreach (var pass in _passes)
        {
            pass.Submit(imQueue);
        }
        
        imQueue.Submit();
    }

    public void Clear()
    {
        _preprocesses.Clear();
        _passes.Clear();
    }
}