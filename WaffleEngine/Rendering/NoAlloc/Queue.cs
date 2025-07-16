using System.Diagnostics.CodeAnalysis;
using SDL3;

namespace WaffleEngine.Rendering.NoAlloc;

public struct Queue
{
    internal IntPtr Handle;

    public Queue()
    {
        Handle = SDL.AcquireGPUCommandBuffer(Device._gpuDevicePtr);
    }

    public bool TryGetSwapchainTexture(WindowSdl window, ref RenderTexture swapchainTexture)
    {
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Command buffer wasn't acquired", "Queue");
            return false;
        }
        
        IntPtr texturePtr;

        if (!SDL.WaitAndAcquireGPUSwapchainTexture(Handle, ((WindowSdl)window).WindowPtr, out texturePtr, out uint width, out uint height))
        {
            WLog.Error("Failed to acquire a swapchain texture", "SDL");
            return false;
        }

        swapchainTexture.Texture = texturePtr;
        swapchainTexture.Width = width;
        swapchainTexture.Height = height;
        
        return true;
    }

    public void Submit()
    {
        SDL.SubmitGPUCommandBuffer(Handle);
        Handle = IntPtr.Zero;
    }
}