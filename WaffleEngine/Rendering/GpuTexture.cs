using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public sealed class GpuTexture : IGpuBindable
{
    public IntPtr Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TextureFormat Format { get; private set; }

    private IntPtr _sampler;

    public GpuTexture() => Handle = IntPtr.Zero;
    
    public GpuTexture(WindowSdl window) => 
        Init((uint)window.Width, (uint)window.Height, (TextureFormat)SDL.GetGPUSwapchainTextureFormat(Device.Handle, window.WindowPtr));

    public GpuTexture(uint width, uint height, WindowSdl window) => 
        Init(width, height, (TextureFormat)SDL.GetGPUSwapchainTextureFormat(Device.Handle, window.WindowPtr));

    public GpuTexture(uint width, uint height) => Init(width, height, TextureFormat.B8G8R8A8Unorm);

    public GpuTexture(uint width, uint height, IntPtr handle) => Set(width, height, handle);
    public GpuTexture(uint width, uint height, TextureFormat format) => Init(width, height, format);
    
    private void Init(uint width, uint height, TextureFormat format)
    {
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Format = (SDL.GPUTextureFormat)format,
            Width = width,
            Height = height,
            Usage = SDL.GPUTextureUsageFlags.ColorTarget | SDL.GPUTextureUsageFlags.Sampler,
            Type = SDL.GPUTextureType.Texturetype2D,
            LayerCountOrDepth = 1,
            NumLevels = 1,
        };

        Width = (int)width;
        Height = (int)height;
        Format = format;

        Handle = SDL.CreateGPUTexture(Device.Handle, createInfo);
        
        var samplerCreateInfo = new SDL.GPUSamplerCreateInfo
        {
            MinFilter = SDL.GPUFilter.Nearest,
            MagFilter = SDL.GPUFilter.Nearest,
            MipmapMode = SDL.GPUSamplerMipmapMode.Nearest,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeW = SDL.GPUSamplerAddressMode.Repeat,
        };

        _sampler = SDL.CreateGPUSampler(Device.Handle, samplerCreateInfo);
    }

    public void Resize(uint width, uint height)
    {
        Dispose();
        Init(width, height, Format);
    }

    public void Set(uint width, uint height, IntPtr handle)
    {
        Width = (int)width;
        Height = (int)height;
        Handle = handle;
    }
    
    public unsafe void Bind(ImRenderPass renderPass, uint slot)
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
        
        SDL.BindGPUVertexSamplers(renderPass.Handle, slot, ptr, 1);
        SDL.BindGPUFragmentSamplers(renderPass.Handle, slot, ptr, 1);
    }

    public void Dispose()
    {
        if (_sampler != 0x0)
            SDL.ReleaseGPUSampler(Device.Handle, _sampler);
        if (Handle != 0x0)
            SDL.ReleaseGPUTexture(Device.Handle, Handle);
    }
}