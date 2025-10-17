using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public unsafe interface IGpuUploadable
{
    internal void UploadToGpu(ImCopyPass copyPass);
}

public unsafe interface IGpuBindable
{
    internal void Bind(ImRenderPass renderPass, uint slot);
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

public sealed class Buffer<T>: IGpuBindable, IGpuUploadable where T : unmanaged
{
    private T[] _data;
    private GpuBuffer<T> _gpuBuffer;
    
    private bool _updated = false;
    private int _count;

    public BufferUsage Usage => _gpuBuffer.Usage;
    public int Count => _count;
    public bool IsReadOnly => false;

    public Buffer(BufferUsage usage, int startSize = 16)
    {
        _data = new T[startSize];
        _gpuBuffer = new GpuBuffer<T>(usage);
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

    public void UploadToGpu(ImCopyPass copyPass)
    {
        if (!_updated)
            return;

        _gpuBuffer.UploadData(_data, copyPass);
        _updated = false;
    }

    public void Bind(ImRenderPass renderPass, uint slot) => _gpuBuffer.Bind(renderPass, slot);
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