using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public sealed unsafe class RenderBuffer<T> : IRenderBindable, IComputeBindable where T : unmanaged
{
    private IntPtr _gpuBuffer;
    private BufferUsage _usage;
    private int _gpuBufferSize;
    
    public BufferUsage Usage => _usage;
    public int Length => _gpuBufferSize;

    public RenderBuffer(BufferUsage usage, int startSize = 16)
    {
        _usage = usage;
        _gpuBufferSize = startSize;
        AllocateBuffer(startSize);
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

    private static IntPtr _transferBuffer;
    private static int _transferBufferSize;

    public void UploadData(Span<T> cpuData, ImCopyPass copyPass)
    {
        if (cpuData.Length > _gpuBufferSize)
        {
            Resize(cpuData.Length);
        }

        var dataSize = sizeof(T) * cpuData.Length;

        if (_transferBufferSize < dataSize)
        {
            if (_transferBuffer != IntPtr.Zero)
            {
                SDL.ReleaseGPUTransferBuffer(Device.Handle, _transferBuffer);
            }
            
            SDL.GPUTransferBufferCreateInfo transferCreateInfo = new SDL.GPUTransferBufferCreateInfo();
            transferCreateInfo.Size = (uint)dataSize;
            transferCreateInfo.Usage = SDL.GPUTransferBufferUsage.Upload;

            _transferBuffer = SDL.CreateGPUTransferBuffer(Device.Handle, transferCreateInfo);
            _transferBufferSize = dataSize;
        }

        var dataPtr = SDL.MapGPUTransferBuffer(Device.Handle, _transferBuffer, true);

        Span<T> data = new (dataPtr.ToPointer(), sizeof(T) * cpuData.Length);
        
        cpuData.CopyTo(data);
        
        SDL.UnmapGPUTransferBuffer(Device.Handle, _transferBuffer);
        
        SDL.GPUTransferBufferLocation location = new SDL.GPUTransferBufferLocation();
        location.TransferBuffer = _transferBuffer;
        location.Offset = 0;
        
        SDL.GPUBufferRegion region = new SDL.GPUBufferRegion();
        region.Buffer = _gpuBuffer;
        region.Size = (uint)(sizeof(T) * cpuData.Length);
        region.Offset = 0;
        
        SDL.UploadToGPUBuffer(copyPass.Handle, location, region, false);
        
        // SDL.ReleaseGPUTransferBuffer(Device.Handle, _transferBuffer);
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
    
    public void Bind(ImComputePass computePass, uint slot)
    {
        if (_usage != BufferUsage.ComputeStorageRead)
        {
            return;
        }
        
        IntPtr ptr = _gpuBuffer;
            
        SDL.BindGPUComputeStorageBuffers(computePass.Handle, slot, (IntPtr)(&ptr), 1);
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
        
        SDL.BindGPUVertexStorageBuffers(renderPass.Handle, slot, (IntPtr)(&ptr), 1);
        SDL.BindGPUFragmentStorageBuffers(renderPass.Handle, slot, (IntPtr)(&ptr), 1);
    }

    
}