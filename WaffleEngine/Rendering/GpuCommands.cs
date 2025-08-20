using SDL3;

namespace WaffleEngine.Rendering;

public interface IGpuRenderCommand
{
    public unsafe void Add(IntPtr renderPass);
}

public class DrawPrimatives(ValueBox<uint> numberOfVertices, ValueBox<uint> numberOfInstances, ValueBox<uint> firstVertex, ValueBox<uint> firstInstance) : IGpuRenderCommand
{
    public unsafe void Add(IntPtr renderPass)
    {
        SDL.DrawGPUPrimitives(renderPass, numberOfVertices, numberOfInstances, firstVertex, firstInstance);
    }
}

public class DrawIndexedPrimatives(ValueBox<uint> numberOfIndices, ValueBox<uint> numberOfInstances, ValueBox<uint> firstIndex, ValueBox<short> vertexOffset, ValueBox<uint> firstInstance) : IGpuRenderCommand
{
    public unsafe void Add(IntPtr renderPass)
    {
        SDL.DrawGPUIndexedPrimitives(renderPass, numberOfIndices, numberOfInstances, firstIndex, vertexOffset, firstInstance);
    }
}

public interface IGpuCopyCommand
{
    public unsafe void Add(IntPtr copyPass);
}

public class Bind(IGpuBindable value, uint slot = 0) : IGpuRenderCommand
{
    public unsafe void Add(IntPtr renderPass)
    {
        value.Bind(renderPass, slot);
    }
}

public class UploadBufferToGpu(IGpuUploadable value): IGpuCopyCommand
{
    public unsafe void Add(IntPtr copyPass)
    {
        value.UploadToGpu(copyPass);
    }
}