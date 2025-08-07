using SDL3;

namespace WaffleEngine.Rendering;

public class GpuTexture : IGpuBindable
{
    public IntPtr Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    private IntPtr _sampler;

    public GpuTexture() => Handle = IntPtr.Zero;
    
    public GpuTexture(WindowSdl window) => 
        Init((uint)window.Width, (uint)window.Height, SDL.GetGPUSwapchainTextureFormat(Device._gpuDevicePtr, window.WindowPtr));

    public GpuTexture(uint width, uint height, WindowSdl window) => 
        Init(width, height, SDL.GetGPUSwapchainTextureFormat(Device._gpuDevicePtr, window.WindowPtr));

    public GpuTexture(uint width, uint height) => Init(width, height, SDL.GPUTextureFormat.R8G8B8A8Unorm);

    public GpuTexture(uint width, uint height, IntPtr handle) => Set(width, height, handle);
    
    private void Init(uint width, uint height, SDL.GPUTextureFormat format)
    {
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Format = SDL.GPUTextureFormat.R8G8B8A8Unorm,
            Width = width,
            Height = height,
            Usage = SDL.GPUTextureUsageFlags.ColorTarget | SDL.GPUTextureUsageFlags.Sampler,
            Type = SDL.GPUTextureType.Texturetype2D,
            LayerCountOrDepth = 1,
            NumLevels = 1,
        };

        Width = (int)width;
        Height = (int)height;

        Handle = SDL.CreateGPUTexture(Device._gpuDevicePtr, createInfo);
        
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

    public void Set(uint width, uint height, IntPtr handle)
    {
        Width = (int)width;
        Height = (int)height;
        Handle = handle;

        if (_sampler != IntPtr.Zero)
        {
            SDL.ReleaseGPUSampler(Device._gpuDevicePtr, _sampler);
        }
        
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
        if (Handle == IntPtr.Zero)
        {
            WLog.Error("Tried to bind a gpu texture which has a null reference.");
            return;
        }
        
        SDL.GPUTextureSamplerBinding binding = new SDL.GPUTextureSamplerBinding
        {
            Texture = Handle,
            Sampler = _sampler
        };
        
        IntPtr ptr = (IntPtr)(&binding);
        
        SDL.BindGPUVertexSamplers(renderPass, slot, ptr, 1);
        SDL.BindGPUFragmentSamplers(renderPass, slot, ptr, 1);
    }
}