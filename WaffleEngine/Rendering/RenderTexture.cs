using SDL3;

namespace WaffleEngine.Rendering;

public class RenderTexture
{
    internal IntPtr Texture;

    public uint Width;
    public uint Height;
    
    public RenderTexture() {}

    public RenderTexture(uint width, uint height, WindowSdl window)
    {
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Format = SDL.GetGPUSwapchainTextureFormat(Device._gpuDevicePtr, window.WindowPtr),
            Width = width,
            Height = height,
            Usage = SDL.GPUTextureUsageFlags.ColorTarget | SDL.GPUTextureUsageFlags.Sampler,
            Type = SDL.GPUTextureType.Texturetype2D,
            LayerCountOrDepth = 1,
            NumLevels = 1,
        };

        Width = width;
        Height = height;

        Texture = SDL.CreateGPUTexture(Device._gpuDevicePtr, createInfo);
    }
    
    public RenderTexture(WindowSdl window)
    {
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Format = SDL.GetGPUSwapchainTextureFormat(Device._gpuDevicePtr, window.WindowPtr),
            Width = (uint)window.Width,
            Height = (uint)window.Height,
            Usage = SDL.GPUTextureUsageFlags.ColorTarget | SDL.GPUTextureUsageFlags.Sampler,
            Type = SDL.GPUTextureType.Texturetype2D,
            LayerCountOrDepth = 1,
            NumLevels = 1,
        };

        Width = (uint)window.Width;
        Height = (uint)window.Height;

        Texture = SDL.CreateGPUTexture(Device._gpuDevicePtr, createInfo);
    }
}