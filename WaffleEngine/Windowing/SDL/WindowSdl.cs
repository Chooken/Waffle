using SDL3;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public sealed class WindowSdl : Window
{
    internal IntPtr WindowPtr;
    internal Renderer? Renderer;

    public string? WindowHandle => WindowManager.TryGetWindowHandle(SDL.GetWindowID(WindowPtr));
    
    public static bool TryCreate(string title, int width, int height, out Window? window)
    {
        window = null;

        WindowSdl windowSdl = new WindowSdl();
        
        if (SDL.WasInit(SDL.InitFlags.Video) != SDL.InitFlags.Video)
        {
            if (!SDL.InitSubSystem(SDL.InitFlags.Video))
            {
                WLog.Error($"Failed to initialise video sub system -> {SDL.GetError()}", "SDL");
                return false;
            }
            
            WLog.Info("Initialised Video Subsystem.", "SDL");
        }
        
        windowSdl.WindowPtr = SDL.CreateWindow(title, width, height,
            SDL.WindowFlags.Resizable | SDL.WindowFlags.HighPixelDensity);

        if (windowSdl.WindowPtr == IntPtr.Zero)
            return false;
        
        Device.Attach(windowSdl);
        
        if (SDL.WindowSupportsGPUPresentMode(Device._gpuDevicePtr, windowSdl.WindowPtr, SDL.GPUPresentMode.VSync)) WLog.Info("Window supports VSync", "WindowSDL");
        if (SDL.WindowSupportsGPUPresentMode(Device._gpuDevicePtr, windowSdl.WindowPtr, SDL.GPUPresentMode.Mailbox)) WLog.Info("Window supports Mailbox", "WindowSDL");
        if (SDL.WindowSupportsGPUPresentMode(Device._gpuDevicePtr, windowSdl.WindowPtr, SDL.GPUPresentMode.Immediate)) WLog.Info("Window supports Immediate", "WindowSDL");
        
        SDL.SetGPUSwapchainParameters(
            Device._gpuDevicePtr, 
            windowSdl.WindowPtr, 
            SDL.GPUSwapchainComposition.SDR, 
            SDL.GPUPresentMode.VSync);
        
        if (!SDL.GetWindowSizeInPixels(windowSdl.WindowPtr, out width, out height))
        {
            WLog.Info($"Failed to get window size", "SDL");
        }
        
        windowSdl.Width = width;
        windowSdl.Height = height;
        window = windowSdl;

        
        
        return true;
    }

    public override uint GetId()
    {
        return SDL.GetWindowID(WindowPtr);
    }

    public override void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override void Focus()
    {
        SDL.RaiseWindow(WindowPtr);
    }

    public override void Dispose()
    {
        WLog.Info("Window Disposed", "SDL");
        SDL.DestroyWindow(WindowPtr);
    }
}