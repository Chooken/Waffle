using System.Runtime.InteropServices;
using GarbagelessSharp;
using SDL3;
using StbImageSharp;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public unsafe class Texture : IGpuUploadable
{
    public int Width => _surface.Value.Width;
    public int Height => _surface.Value.Height;
    public Span<byte> Data => new Span<byte>((void *)_surface.Value.Pixels, Width * Height * 4);
    
    private Unmanaged<SDL.Surface> _surface;
    private GpuTexture _gpuTexture;
    
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
        
        //var surface = SDL.PointerToStructure<SDL.Surface>(_surface) ?? default;
        
        SDL.DestroySurface(startSurface);

        _gpuTexture = new GpuTexture((uint)Width, (uint)Height);
    }

    public void Bind(IntPtr renderPass, uint slot)
    {
        _gpuTexture.Bind(renderPass, slot);
    }

    public void UploadToGpu(IntPtr copyPass)
    {
        SDL.GPUTransferBufferCreateInfo transferCreateInfo = new SDL.GPUTransferBufferCreateInfo();
        transferCreateInfo.Size = (uint)Width * (uint)Height * 4;
        transferCreateInfo.Usage = SDL.GPUTransferBufferUsage.Upload;

        IntPtr transferBuffer = SDL.CreateGPUTransferBuffer(Device._gpuDevicePtr, transferCreateInfo);

        var dataPtr = SDL.MapGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer, false);

        Span<byte> transferData = new(dataPtr.ToPointer(), (int)(Width * Height * 4));
        
        Data.CopyTo(transferData);

        SDL.UnmapGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer);

        SDL.GPUTextureTransferInfo info = new SDL.GPUTextureTransferInfo();
        info.TransferBuffer = transferBuffer;
        info.Offset = 0;

        SDL.GPUTextureRegion region = new SDL.GPUTextureRegion();
        region.Texture = _gpuTexture.Handle;
        region.W = (uint)Width;
        region.H = (uint)Height;
        region.D = 1;

        SDL.UploadToGPUTexture(copyPass, info, region, true);

        SDL.ReleaseGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer);
    }

    public static implicit operator GpuTexture(Texture value) => value._gpuTexture;
}