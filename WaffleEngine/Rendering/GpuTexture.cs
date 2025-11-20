using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public sealed class GpuTexture : IRenderBindable
{
    public IntPtr Handle { get; private set; }
    public uint Width { get => _settings.Width; }
    public uint Height { get => _settings.Height; }

    public TextureFormat Format { get => _settings.Format; }
    
    public bool ColorTarget { get => _settings.ColorTarget; }
    
    public bool RandomWrites { get => _settings.RandomWrites; }

    private GpuTextureSettings _settings;
    

    private IntPtr _sampler;

    public GpuTexture() => Handle = IntPtr.Zero;
    
    public GpuTexture(Window window) => 
        Init(GpuTextureSettings.FromWindow(window));

    public GpuTexture(GpuTextureSettings settings) => 
        Init(settings);

    private void Init(GpuTextureSettings settings)
    {
        _settings = settings;
        
        var createInfo = new SDL.GPUTextureCreateInfo
        {
            Format = (SDL.GPUTextureFormat)settings.Format,
            Width = settings.Width,
            Height = settings.Height,
            Usage = SDL.GPUTextureUsageFlags.Sampler,
            Type = SDL.GPUTextureType.Texturetype2D,
            LayerCountOrDepth = 1,
            NumLevels = 1,
        };

        if (settings.ColorTarget)
            createInfo.Usage |= SDL.GPUTextureUsageFlags.ColorTarget;

        if (settings.RandomWrites)
            createInfo.Usage |= SDL.GPUTextureUsageFlags.ComputeStorageSimultaneousReadWrite;
        

        Handle = SDL.CreateGPUTexture(Device.Handle, createInfo);
        
        var samplerCreateInfo = new SDL.GPUSamplerCreateInfo
        {
            MinFilter = (SDL.GPUFilter)settings.MinFilter,
            MagFilter = (SDL.GPUFilter)settings.MagFilter,
            MipmapMode = (SDL.GPUSamplerMipmapMode)settings.MipsFilter,
            AddressModeU = (SDL.GPUSamplerAddressMode)settings.SamplerMode,
            AddressModeV = (SDL.GPUSamplerAddressMode)settings.SamplerMode,
            AddressModeW = (SDL.GPUSamplerAddressMode)settings.SamplerMode,
        };
        
        _sampler = SDL.CreateGPUSampler(Device.Handle, samplerCreateInfo);
    }

    public void Resize(uint width, uint height)
    {
        _settings.Width = width;
        _settings.Height = height;
        
        Dispose();
        
        Init(_settings);
    }

    public void Set(GpuTextureSettings settings, IntPtr handle)
    {
        _settings = settings;
        Handle = handle;
    }

    public bool TryGetReadWriteBinding(uint mipLevel, [NotNullWhen(true)] out ReadWriteTextureBinding? binding)
    {
        binding = null;

        if (!RandomWrites)
            return false;

        binding = new ReadWriteTextureBinding()
        {
            TextureHandle = Handle,
            Layer = 0,
            MipLevel = mipLevel,
        };
        
        return true;
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

        Handle = 0x0;
        _sampler = 0x0;
    }
}

/// <summary>
/// A structure specifying parameters related to binding textures in a compute
/// pass.
/// </summary>
/// <since>This struct is available since SDL 3.2.0</since>
/// <seealso cref="BeginGPUComputePass"/>
[StructLayout(LayoutKind.Sequential)]
public struct ReadWriteTextureBinding
{
    /// <summary>
    /// The texture to bind. Must have been created with <see cref="GPUTextureUsageFlags.ComputeStorageWrite"/> or <see cref="GPUTextureUsageFlags.ComputeStorageSimultaneousReadWrite"/>.
    /// </summary>
    public IntPtr TextureHandle;
        
    /// <summary>
    /// The mip level index to bind.
    /// </summary>
    public UInt32 MipLevel;
        
    /// <summary>
    /// The layer index to bind.
    /// </summary>
    public UInt32 Layer;
        
    /// <summary>
    /// true cycles the texture if it is already bound.
    /// </summary>
    public Byte Cycle;
        
    private Byte _padding1;
        
    private Byte _padding2;
        
    private Byte _padding3;
}