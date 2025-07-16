using System.Runtime.InteropServices;
using SDL3;
using StbImageSharp;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public unsafe class Texture : IGpuUploadable, IGpuBindable
{
    private IntPtr _gpuHandle;
    private IntPtr _sampler;
    
    private IntPtr _surface;
    
    public readonly uint Width;
    public readonly uint Height;

    private IntPtr _data;
    
    public Texture(string path)
    {
        IntPtr startSurface = Image.Load(path);
        if (startSurface == IntPtr.Zero)
        {
            WLog.Error($"Failed to load image {path}: {SDL.GetError()}", "SDL Image");
            return;
        }

        _surface = SDL.ConvertSurface(startSurface, SDL.PixelFormat.ABGR8888);
        SDL.FlipSurface(_surface, SDL.FlipMode.Vertical);
        
        var surface = SDL.PointerToStructure<SDL.Surface>(_surface) ?? default;
        
        Width = (uint)surface.Width;
        Height = (uint)surface.Height;
        
        _data = surface.Pixels;
        
        SDL.DestroySurface(startSurface);
        
        var samplerCreateInfo = new SDL.GPUSamplerCreateInfo
        {
            MinFilter = SDL.GPUFilter.Linear,
            MagFilter = SDL.GPUFilter.Linear,
            MipmapMode = SDL.GPUSamplerMipmapMode.Linear,
            AddressModeU = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeV = SDL.GPUSamplerAddressMode.Repeat,
            AddressModeW = SDL.GPUSamplerAddressMode.Repeat,
        };

        _sampler = SDL.CreateGPUSampler(Device._gpuDevicePtr, samplerCreateInfo);
        
        var textureCreateInfo = new SDL.GPUTextureCreateInfo
        {
            Type = SDL.GPUTextureType.Texturetype2D,
            Format = SDL.GPUTextureFormat.R8G8B8A8Unorm,
            Width = Width,
            Height = Height,
            LayerCountOrDepth = 1,
            NumLevels = 1,
            Usage = SDL.GPUTextureUsageFlags.Sampler
        };

        _gpuHandle = SDL.CreateGPUTexture(Device._gpuDevicePtr, textureCreateInfo);
    }

    public void Bind(IntPtr renderPass, uint slot)
    {
        SDL.GPUTextureSamplerBinding binding = new SDL.GPUTextureSamplerBinding
        {
            Texture = _gpuHandle,
            Sampler = _sampler
        };
        
        IntPtr ptr = (IntPtr)(&binding);
        
        SDL.BindGPUVertexSamplers(renderPass, slot, ptr, 1);
        SDL.BindGPUFragmentSamplers(renderPass, slot, ptr, 1);
    }

    public void UploadToGpu(IntPtr copyPass)
    {
        SDL.GPUTransferBufferCreateInfo transferCreateInfo = new SDL.GPUTransferBufferCreateInfo();
        transferCreateInfo.Size = Width * Height * 4;
        transferCreateInfo.Usage = SDL.GPUTransferBufferUsage.Upload;

        IntPtr transferBuffer = SDL.CreateGPUTransferBuffer(Device._gpuDevicePtr, transferCreateInfo);

        var dataPtr = SDL.MapGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer, false);

        Span<byte> transferData = new(dataPtr.ToPointer(), (int)(Width * Height * 4));
        Span<byte> data = new((void*)_data, (int)(Width * Height * 4));
        
        data.CopyTo(transferData);

        SDL.UnmapGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer);

        SDL.GPUTextureTransferInfo info = new SDL.GPUTextureTransferInfo();
        info.TransferBuffer = transferBuffer;
        info.Offset = 0;

        SDL.GPUTextureRegion region = new SDL.GPUTextureRegion();
        region.Texture = _gpuHandle;
        region.W = Width;
        region.H = Height;
        region.D = 1;

        SDL.UploadToGPUTexture(copyPass, info, region, true);

        SDL.ReleaseGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer);
    }
}