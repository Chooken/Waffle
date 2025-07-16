using SDL3;

namespace WaffleEngine.Rendering;

public static unsafe class Device
{
    internal static IntPtr _gpuDevicePtr;
    private static bool _initialized = false;

    public static bool Init()
    {
        if (_initialized)
            return true;
        
        if (SDL.WasInit(SDL.InitFlags.Video) != SDL.InitFlags.Video)
        {
            if (!SDL.InitSubSystem(SDL.InitFlags.Video))
            {
                WLog.Error($"Failed to initialise video sub system -> {SDL.GetError()}", "SDL");
                return false;
            }
            
            WLog.Info("Initialised Video Subsystem.", "SDL");
        }
        
        _gpuDevicePtr = SDL.CreateGPUDevice(
            SDL.GPUShaderFormat.SPIRV | 
            SDL.GPUShaderFormat.MSL |
            SDL.GPUShaderFormat.DXIL, false, null);

        if (_gpuDevicePtr == IntPtr.Zero)
        {
            WLog.Error("Failed to create GPU Device", "SDL");
            return false;
        }
        
        WLog.Info($"Created GPU Device with backend {GetDriver()}.", "SDL");
        _initialized = true;
        
        return true;
    }

    public static bool Attach(WindowSdl window)
    {
        if (!_initialized)
            return false;
        
        return TryClaimWindow(window);
    }

    private static bool TryClaimWindow(WindowSdl window)
    {
        if (!SDL.ClaimWindowForGPUDevice(_gpuDevicePtr, window.WindowPtr))
        {
            WLog.Error("Failed to claim window for GPU Device", "SDL");
            return false;
        }
        
        return true;
    }

    public static string GetDriver()
    {
        return SDL.GetGPUDeviceDriver(_gpuDevicePtr) ?? "No GPU Driver Found";
    }

    public static ShaderFormat GetShaderFormat()
    {
        return (ShaderFormat)SDL.GetGPUShaderFormats(_gpuDevicePtr);
    }
}

[Flags]
public enum ShaderFormat : uint
{
    Private = 1,
    SpirV = 2,
    Dxbc = 4,
    Dxil = 8,
    Msl = 16, // 0x00000010
    MetalLib = 32, // 0x00000020
}