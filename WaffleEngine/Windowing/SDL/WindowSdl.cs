using System.Runtime.CompilerServices;
using SDL3;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public sealed class WindowSdl : Window
{
    internal IntPtr WindowPtr;

    public string? WindowHandle => WindowManager.TryGetWindowHandle(SDL.GetWindowID(WindowPtr));
    
    public static bool TryCreate(string title, int width, int height, out Window? window)
    {
        window = null;

        WindowSdl windowSdl = new WindowSdl();

        windowSdl.Resizeable = true;
        
        if (SDL.WasInit(SDL.InitFlags.Video) != SDL.InitFlags.Video)
        {
            if (!SDL.InitSubSystem(SDL.InitFlags.Video))
            {
                WLog.Error($"Failed to initialise video sub system -> {SDL.GetError()}", "SDL");
                return false;
            }
            
            WLog.Info("Initialised Video Subsystem.", "SDL");
        }

        SDL.WindowFlags flags = SDL.WindowFlags.HighPixelDensity;

        if (windowSdl.Resizeable) flags |= SDL.WindowFlags.Resizable;
        
        windowSdl.WindowPtr = SDL.CreateWindow(title, width, height,
            flags);

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

        unsafe
        {
            SDL.AddEventWatch(HandleWindowResize, (IntPtr)Unsafe.AsPointer(ref window));
        }

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

    private static unsafe bool HandleWindowResize(IntPtr userdata, ref SDL.Event sdlEvent)
    {
        switch ((SDL.EventType)sdlEvent.Type)
        {
            case SDL.EventType.WindowExposed:
                if (((WindowSdl*)userdata)->Resizeable)
                {
                    SceneManager.RunActiveSceneQueries();
                }

                break;
            
            case SDL.EventType.WindowPixelSizeChanged:
                ((WindowSdl*)userdata)->Width = sdlEvent.Window.Data1;
                ((WindowSdl*)userdata)->Height = sdlEvent.Window.Data2;
                
                // I'm pretty sure I shouldn't be doing this but idk how to fix it otherwise.
                SceneManager.RunActiveSceneQueries();
                
                break;
        }
        
        return true;
    }
}