using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public sealed unsafe class GpuBuffer<T> : IGpuBindable where T : unmanaged
{
    private IntPtr _gpuBuffer;
    private BufferUsage _usage;
    private int _gpuBufferSize;
    
    public BufferUsage Usage => _usage;
    public int Length => _gpuBufferSize;

    public GpuBuffer(BufferUsage usage)
    {
        _usage = usage;
        _gpuBufferSize = 0;
    }

    private void AllocateBuffer(int size)
    {
        SDL.GPUBufferCreateInfo bufferCreateInfo = new SDL.GPUBufferCreateInfo();
        bufferCreateInfo.Usage = (SDL.GPUBufferUsageFlags)_usage;
        bufferCreateInfo.Size = (uint)(sizeof(T) * size);
        
        _gpuBufferSize = size;
        _gpuBuffer = SDL.CreateGPUBuffer(Device.Handle, bufferCreateInfo);
    }

    private void ReleaseBuffer()
    {
        if (_gpuBuffer == IntPtr.Zero)
            return;
        
        SDL.ReleaseGPUBuffer(Device.Handle, _gpuBuffer);
    }

    private void Resize(int size)
    {
        ReleaseBuffer();
        AllocateBuffer(size);
    }

    public void UploadData(Span<T> cpuData, ImCopyPass copyPass)
    {
        if (cpuData.Length > _gpuBufferSize)
        {
            Resize(cpuData.Length);
        }
        
        SDL.GPUTransferBufferCreateInfo transferCreateInfo = new SDL.GPUTransferBufferCreateInfo();
        transferCreateInfo.Size = (uint)(sizeof(T) * cpuData.Length);
        transferCreateInfo.Usage = SDL.GPUTransferBufferUsage.Upload;
        
        IntPtr transferBuffer = SDL.CreateGPUTransferBuffer(Device.Handle, transferCreateInfo);

        var dataPtr = SDL.MapGPUTransferBuffer(Device.Handle, transferBuffer, false);

        Span<T> data = new (dataPtr.ToPointer(), sizeof(T) * cpuData.Length);
        
        cpuData.CopyTo(data);
        
        SDL.UnmapGPUTransferBuffer(Device.Handle, transferBuffer);
        
        SDL.GPUTransferBufferLocation location = new SDL.GPUTransferBufferLocation();
        location.TransferBuffer = transferBuffer;
        location.Offset = 0;
        
        SDL.GPUBufferRegion region = new SDL.GPUBufferRegion();
        region.Buffer = _gpuBuffer;
        region.Size = (uint)(sizeof(T) * cpuData.Length);
        region.Offset = 0;
        
        SDL.UploadToGPUBuffer(copyPass.Handle, location, region, true);
        
        SDL.ReleaseGPUTransferBuffer(Device.Handle, transferBuffer);
    }

    public void Bind(ImRenderPass renderPass, uint slot)
    {
        if (_usage == BufferUsage.Vertex)
        {
            BindAsVertexBuffer(renderPass, slot);
            return;
        }
        
        if (_usage == BufferUsage.Index)
        {
            BindAsIndexBuffer(renderPass);
            return;
        }

        if (_usage == BufferUsage.GraphicsStorageRead)
        {
            BindAsStorageBuffer(renderPass, slot);
            return;
        }
        
        WLog.Error($"Can't bind a buffer with the usage: {_usage}");
    }

    private void BindAsVertexBuffer(ImRenderPass renderPass, uint slot)
    {
        SDL.GPUBufferBinding bufferBinding = new SDL.GPUBufferBinding
        {
            Buffer = _gpuBuffer,
            Offset = 0
        };

        SDL.GPUBufferBinding* ptr = &bufferBinding;
        
        SDL.BindGPUVertexBuffers(renderPass.Handle, slot, (IntPtr)ptr, 1);
    }

    private void BindAsIndexBuffer(ImRenderPass renderPass)
    {
        SDL.GPUBufferBinding bufferBinding = new SDL.GPUBufferBinding
        {
            Buffer = _gpuBuffer,
            Offset = 0
        };

        SDL.BindGPUIndexBuffer(renderPass.Handle, bufferBinding, SDL.GPUIndexElementSize.IndexElementSize32Bit);
    }
    
    private void BindAsStorageBuffer(ImRenderPass renderPass, uint slot)
    {
        IntPtr ptr = _gpuBuffer;
        
        SDLExtra.BindGPUVertexStorageBuffers(renderPass.Handle, slot, (IntPtr)(&ptr), 1);
        SDL.BindGPUFragmentStorageBuffers(renderPass.Handle, slot, (IntPtr)(&ptr), 1);
    }
}