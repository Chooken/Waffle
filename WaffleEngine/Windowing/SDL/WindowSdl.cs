using System.Runtime.CompilerServices;
using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public sealed class WindowSdl : Window
{
    internal IntPtr WindowPtr;

    public static bool TryCreate(string handle, string title, int width, int height, out Window? window)
    {
        window = null;

        WindowSdl windowSdl = new WindowSdl();

        windowSdl.WindowHandle = handle;

        windowSdl.Resizeable = true;
        
        if (SDL.WasInit(SDL.InitFlags.Video) != SDL.InitFlags.Video)
        {
            if (!SDL.InitSubSystem(SDL.InitFlags.Video))
            {
                WLog.Error($"Failed to initialise video sub system -> {SDL.GetError()}");
                return false;
            }
            
            WLog.Info("Initialised Video Subsystem.");
        }

        SDL.WindowFlags flags = SDL.WindowFlags.HighPixelDensity;

        if (windowSdl.Resizeable) flags |= SDL.WindowFlags.Resizable;
        
        windowSdl.WindowPtr = SDL.CreateWindow(title, width, height,
            flags);

        if (windowSdl.WindowPtr == IntPtr.Zero)
            return false;
        
        Device.Attach(windowSdl);
        
        if (SDL.WindowSupportsGPUPresentMode(Device.Handle, windowSdl.WindowPtr, SDL.GPUPresentMode.VSync)) WLog.Info("Window supports VSync");
        if (SDL.WindowSupportsGPUPresentMode(Device.Handle, windowSdl.WindowPtr, SDL.GPUPresentMode.Mailbox)) WLog.Info("Window supports Mailbox");
        if (SDL.WindowSupportsGPUPresentMode(Device.Handle, windowSdl.WindowPtr, SDL.GPUPresentMode.Immediate)) WLog.Info("Window supports Immediate");
        
        SDL.SetGPUSwapchainParameters(
            Device.Handle, 
            windowSdl.WindowPtr, 
            SDL.GPUSwapchainComposition.SDR, 
            SDL.GPUPresentMode.VSync);
        
        if (!SDL.GetWindowSizeInPixels(windowSdl.WindowPtr, out width, out height))
        {
            WLog.Info($"Failed to get window size");
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

    public override void SetTitle(string title)
    {
        SDL.SetWindowTitle(WindowPtr, title);
    }

    public override float GetDisplayScale() => SDL.GetWindowDisplayScale(WindowPtr);

    public override TextureFormat GetSwapchainTextureFormat()
    {
        return (TextureFormat)SDL.GetGPUSwapchainTextureFormat(Device.Handle, WindowPtr);
    }

    public override void Dispose()
    {
        WLog.Info("Window Disposed");
        SDL.DestroyWindow(WindowPtr);
        WindowPtr = IntPtr.Zero;
    }

    public override bool IsOpen()
    {
        return WindowPtr != IntPtr.Zero;
    }

    public override bool TryGetSwapchainTexture(ImQueue queue, ref GpuTexture texture)
    {
        if (!SDL.WaitAndAcquireGPUSwapchainTexture(queue.Handle, WindowPtr, out IntPtr handle, out uint width, out uint height))
        {
            WLog.Error("Failed to acquire a swapchain texture");
            return false;
        }

        texture.Set(GpuTextureSettings.Default(width, height) with
        {
            ColorTarget = true,
        }, handle);
        return true;
    }

    private static unsafe bool HandleWindowResize(IntPtr userdata, ref SDL.Event sdlEvent)
    {
        switch ((SDL.EventType)sdlEvent.Type)
        {
            case SDL.EventType.WindowPixelSizeChanged:

                if (!WindowManager.TryGetWindowWithId(sdlEvent.Window.WindowID, out var window))
                {
                    return true;
                }
                
                window.Width = sdlEvent.Window.Data1;
                window.Height = sdlEvent.Window.Data2;
                
                window.OnWindowResized.Invoke(new Vector2(window.Width, window.Height));
                
                // I'm pretty sure I shouldn't be doing this but idk how to fix it otherwise.
                SceneManager.UpdateScenes();
                
                break;
        }
        
        return true;
    }
}