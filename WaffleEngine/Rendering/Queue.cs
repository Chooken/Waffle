using SDL3;

namespace WaffleEngine.Rendering;

public interface IPreprocess
{
    public void Run(Queue queue);
}

public sealed class GetSwapchain(WindowSdl window, ref GpuTexture texture) : IPreprocess
{
    private GpuTexture _texture = texture;

    public void Run(Queue queue)
    {
        if (!queue.TryGetSwapchainTexture(window, ref _texture))
        {
            WLog.Error("Failed to retrieve swapchain texture", "Renderer");
            return;
        }
    }
}

public sealed class Queue
{
    private List<IPreprocess> _preprocesses = new List<IPreprocess>();
    private List<IPass> _passes = new List<IPass>();
    private IntPtr CommandBuffer;

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
        CommandBuffer = SDL.AcquireGPUCommandBuffer(Device._gpuDevicePtr);
        
        if (CommandBuffer == IntPtr.Zero)
        {
            WLog.Error("Failed to acquire command buffer", "SDL");
            return;
        }

        foreach (var preprocess in _preprocesses)
        {
            preprocess.Run(this);
        }
        
        foreach (var pass in _passes)
        {
            pass.Submit(CommandBuffer);
        }
        
        if (!SDL.SubmitGPUCommandBuffer(CommandBuffer))
        {
            WLog.Info("Failed submitting GPU CommandBuffer", "SDL");
        }
    }
    
    internal bool TryGetSwapchainTexture(Window window, ref GpuTexture texture)
    {
        if (CommandBuffer == IntPtr.Zero)
        {
            WLog.Error("Command buffer wasn't acquired", "Queue");
            return false;
        }
        
        IntPtr handle;

        if (!SDL.WaitAndAcquireGPUSwapchainTexture(CommandBuffer, ((WindowSdl)window).WindowPtr, out handle, out uint width, out uint height))
        {
            WLog.Error("Failed to acquire a swapchain texture", "SDL");
            return false;
        }

        texture.Set(width, height, handle);
        
        return true;
    }

    public void Clear()
    {
        _preprocesses.Clear();
        _passes.Clear();
    }
}