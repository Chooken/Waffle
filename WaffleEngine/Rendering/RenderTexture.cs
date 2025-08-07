using SDL3;

namespace WaffleEngine.Rendering;

public class RenderTexture : IGpuBindable
{
    internal IntPtr Texture;

    private IntPtr _sampler;

    public uint Width;
    public uint Height;
    
    public RenderTexture() {}

    public RenderTexture(uint width, uint height, WindowSdl window) => Init(width, height, window);

    public RenderTexture(WindowSdl window) => Init((uint)window.Width, (uint)window.Height, window);

    private void Init(uint width, uint height, WindowSdl window)
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
        
        var samplerCreateInfo = new SDL.GPUSamplerCreateInfo
        {
            MinFilter = SDL.GPUFilter.Nearest,
            MagFilter = SDL.GPUFilter.Nearest,
            MipmapMode = SDL.GPUSamplerMipmapMode.Nearest,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeW = SDL.GPUSamplerAddressMode.Repeat,
        };

        _sampler = SDL.CreateGPUSampler(Device._gpuDevicePtr, samplerCreateInfo);
    }

    public unsafe void Bind(IntPtr renderPass, uint slot)
    {
        SDL.GPUTextureSamplerBinding binding = new SDL.GPUTextureSamplerBinding
        {
            Texture = Texture,
            Sampler = _sampler
        };
        
        IntPtr ptr = (IntPtr)(&binding);
        
        SDL.BindGPUVertexSamplers(renderPass, slot, ptr, 1);
        SDL.BindGPUFragmentSamplers(renderPass, slot, ptr, 1);
    }
}