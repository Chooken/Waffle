using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Native;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public unsafe class Texture : IGpuUploadable, IRenderBindable, IDisposable
{
    public int Width => _surface.Value.Width;
    public int Height => _surface.Value.Height;
    public Span<byte> Data => new Span<byte>((void *)_surface.Value.Pixels, Width * Height * 4);
    
    private NativePtr<SDL.Surface> _surface;
    private GpuTexture _gpuTexture;

    public Texture(uint width, uint height)
    {
        _surface = SDL.CreateSurface((int)width, (int)height, SDL.PixelFormat.ABGR8888);
        
        _gpuTexture = new GpuTexture(GpuTextureSettings.Default((uint)Width, (uint)Height) with
        {
            Format = TextureFormat.R8G8B8A8Unorm,
        });
        
        ImQueue queue = new ImQueue();
        ImCopyPass copyPass = queue.AddCopyPass();
        UploadToGpu(copyPass);
        copyPass.End();
        queue.Submit();
    }
    
    public Texture(string path)
    {
        NativePtr<SDL.Surface> startSurface = Image.Load(path);
        
        if (startSurface.IsNull)
        {
            WLog.Error($"Failed to load image {path}: {SDL.GetError()}");
            return;
        }

        if (startSurface.Value.Format != SDL.PixelFormat.ABGR8888)
        {
            _surface = SDL.ConvertSurface(startSurface, SDL.PixelFormat.ABGR8888);
            SDL.DestroySurface(startSurface);
        }
        else
        {
            _surface = startSurface;
        }

        if (_surface.IsNull)
        {
            WLog.Error(SDL.GetError());
            return;
        }

        _gpuTexture = new GpuTexture(GpuTextureSettings.Default((uint)Width, (uint)Height) with
        {
            Format = TextureFormat.R8G8B8A8Unorm,
        });

        ImQueue queue = new ImQueue();
        ImCopyPass copyPass = queue.AddCopyPass();
        UploadToGpu(copyPass);
        copyPass.End();
        queue.Submit();
    }

    public Texture(IntPtr surface)
    {
        _surface= SDL.ConvertSurface(surface, SDL.PixelFormat.ABGR8888);
        
        SDL.DestroySurface(surface);
        
        _gpuTexture = new GpuTexture(GpuTextureSettings.Default((uint)Width, (uint)Height));
    }

    public void SetSurface(IntPtr surface)
    {
        SDL.DestroySurface(_surface);
        
        _surface= SDL.ConvertSurface(surface, SDL.PixelFormat.ABGR8888);
        
        SDL.DestroySurface(surface);
        
        _gpuTexture.Resize((uint)Width, (uint)Height);
    }

    public void Bind(ImRenderPass renderPass, uint slot)
    {
        _gpuTexture.Bind(renderPass, slot);
    }

    public void UploadToGpu(ImCopyPass copyPass)
    {
        SDL.GPUTransferBufferCreateInfo transferCreateInfo = new SDL.GPUTransferBufferCreateInfo();
        transferCreateInfo.Size = (uint)Width * (uint)Height * 4;
        transferCreateInfo.Usage = SDL.GPUTransferBufferUsage.Upload;

        IntPtr transferBuffer = SDL.CreateGPUTransferBuffer(Device.Handle, transferCreateInfo);

        var dataPtr = SDL.MapGPUTransferBuffer(Device.Handle, transferBuffer, false);

        Span<byte> transferData = new(dataPtr.ToPointer(), (int)(Width * Height * 4));
        
        Data.CopyTo(transferData);

        SDL.UnmapGPUTransferBuffer(Device.Handle, transferBuffer);

        SDL.GPUTextureTransferInfo info = new SDL.GPUTextureTransferInfo();
        info.TransferBuffer = transferBuffer;
        info.Offset = 0;

        SDL.GPUTextureRegion region = new SDL.GPUTextureRegion();
        region.Texture = _gpuTexture.Handle;
        region.W = (uint)Width;
        region.H = (uint)Height;
        region.D = 1;

        SDL.UploadToGPUTexture(copyPass.Handle, info, region, true);

        SDL.ReleaseGPUTransferBuffer(Device.Handle, transferBuffer);
    }

    public Span<T> GetAs<T>() => new Span<T>((void*)_surface.Value.Pixels, Width * Height);
    

    public void Dispose()
    {
        _gpuTexture.Dispose();
        SDL.DestroySurface(_surface);
    }

    public static implicit operator GpuTexture(Texture value) => value._gpuTexture;
}