using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace WaffleEngine.Rendering;

public unsafe interface IGpuUploadable
{
    internal void UploadToGpu(IntPtr copyPass);
}

public unsafe interface IGpuBindable
{
    internal void Bind(IntPtr renderPass, uint slot);
}

[Flags]
public enum BufferUsage : uint
{
    Vertex = 1,
    Index = 2,
    Indirect = 4,
    GraphicsStorageRead = 8,
    ComputeStorageRead = 16,
    ComputeStorageWrite = 32,
}

public unsafe class Buffer<T>: IGpuUploadable, IGpuBindable where T : unmanaged
{
    private T[] _data;
    private BufferUsage _usage;
    
    private IntPtr _gpuBuffer;
    private int _gpuBufferSize;
    private bool _updated = false;
    private int _count;

    public BufferUsage Usage => _usage;
    public int Count => _count;
    public bool IsReadOnly => false;

    public Buffer(BufferUsage usage, int startSize = 16)
    {
        _data = new T[startSize];
        _usage = usage;
        
        SDL.GPUBufferCreateInfo bufferCreateInfo = new SDL.GPUBufferCreateInfo();
        bufferCreateInfo.Usage = (SDL.GPUBufferUsageFlags)_usage;
        bufferCreateInfo.Size = (uint)(sizeof(T) * startSize);
        
        _gpuBufferSize = startSize;
        _gpuBuffer = SDL.CreateGPUBuffer(Device._gpuDevicePtr, bufferCreateInfo);
            
        _count = 0;
    }
    
    public T this[int index]
    {
        get => _data[index]; 
        set
        {
            _updated = true;
            _data[index] = value;
        }
    }

    public void Add(T item)
    {
        _count++;
        
        if (_count > _data.Length)
            Array.Resize(ref _data, _data.Length * 2);

        _data[_count - 1] = item;
        _updated = true;
    }

    public void Sort(IComparer<T> comparer)
    {
        Array.Sort(_data, 0, _count, comparer);
    }

    public void Clear()
    {
        _count = 0;
    }

    public void UploadToGpu(IntPtr copyPass)
    {
        if (!_updated)
            return;

        if (_data.Length > _gpuBufferSize)
        {
            SDL.ReleaseGPUBuffer(Device._gpuDevicePtr, _gpuBuffer);
            
            SDL.GPUBufferCreateInfo bufferCreateInfo = new SDL.GPUBufferCreateInfo();
            bufferCreateInfo.Usage = (SDL.GPUBufferUsageFlags)_usage;
            bufferCreateInfo.Size = (uint)(sizeof(T) * _data.Length);
        
            _gpuBufferSize = _data.Length;
            _gpuBuffer = SDL.CreateGPUBuffer(Device._gpuDevicePtr, bufferCreateInfo);
        }
        
        SDL.GPUTransferBufferCreateInfo transferCreateInfo = new SDL.GPUTransferBufferCreateInfo();
        transferCreateInfo.Size = (uint)(sizeof(T) * _data.Length);
        transferCreateInfo.Usage = SDL.GPUTransferBufferUsage.Upload;
        
        IntPtr transferBuffer = SDL.CreateGPUTransferBuffer(Device._gpuDevicePtr, transferCreateInfo);

        var dataPtr = SDL.MapGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer, false);

        Span<T> data = new (dataPtr.ToPointer(), sizeof(T) * _data.Length);
        
        _data.CopyTo(data);
        
        SDL.UnmapGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer);
        
        SDL.GPUTransferBufferLocation location = new SDL.GPUTransferBufferLocation();
        location.TransferBuffer = transferBuffer;
        location.Offset = 0;
        
        SDL.GPUBufferRegion region = new SDL.GPUBufferRegion();
        region.Buffer = _gpuBuffer;
        region.Size = (uint)(sizeof(T) * _data.Length);
        region.Offset = 0;
        
        SDL.UploadToGPUBuffer(copyPass, location, region, true);
        
        SDL.ReleaseGPUTransferBuffer(Device._gpuDevicePtr, transferBuffer);

        _updated = false;
    }

    public void Bind(IntPtr renderPass, uint slot)
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
        
        WLog.Error($"Can't bind a buffer with the usage: {_usage}", "Buffer");
    }

    private void BindAsVertexBuffer(IntPtr renderPass, uint slot)
    {
        SDL.GPUBufferBinding bufferBinding = new SDL.GPUBufferBinding
        {
            Buffer = _gpuBuffer,
            Offset = 0
        };

        SDL.GPUBufferBinding* ptr = &bufferBinding;
        
        SDL.BindGPUVertexBuffers(renderPass, slot, (IntPtr)ptr, 1);
    }

    private void BindAsIndexBuffer(IntPtr renderPass)
    {
        SDL.GPUBufferBinding bufferBinding = new SDL.GPUBufferBinding
        {
            Buffer = _gpuBuffer,
            Offset = 0
        };

        SDL.BindGPUIndexBuffer(renderPass, bufferBinding, SDL.GPUIndexElementSize.IndexElementSize32Bit);
    }
    
    private void BindAsStorageBuffer(IntPtr renderPass, uint slot)
    {
        // IntPtr[] ptrs = [_gpuBuffer];
        //
        // SDL.BindGPUVertexStorageBuffers(renderPass, slot, ptrs, 1);
        
        IntPtr ptr = _gpuBuffer;
        
        SDLExtra.BindGPUVertexStorageBuffers(renderPass, slot, (IntPtr)(&ptr), 1);
        SDL.BindGPUFragmentStorageBuffers(renderPass, slot, (IntPtr)(&ptr), 1);
        // WLog.Info(SDL.GetError());
    }
}

public static partial class SDLExtra
{
    [LibraryImport("SDL3", EntryPoint = "SDL_BindGPUVertexStorageBuffers"), UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void BindGPUVertexStorageBuffers(
        IntPtr renderPass,
        uint firstSlot,
        IntPtr storageBuffers,
        uint numBindings);
}