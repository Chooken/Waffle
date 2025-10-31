using SDL3;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine.Rendering;

public interface IGpuRenderCommand
{
    public unsafe void Add(ImRenderPass renderPass);
}

public class DrawPrimatives(ValueBox<uint> numberOfVertices, ValueBox<uint> numberOfInstances, ValueBox<uint> firstVertex, ValueBox<uint> firstInstance) : IGpuRenderCommand
{
    public unsafe void Add(ImRenderPass renderPass)
    {
        renderPass.DrawPrimatives(numberOfVertices, numberOfInstances, firstVertex, firstInstance);
    }
}

public class DrawIndexedPrimatives(ValueBox<uint> numberOfIndices, ValueBox<uint> numberOfInstances, ValueBox<uint> firstIndex, ValueBox<short> vertexOffset, ValueBox<uint> firstInstance) : IGpuRenderCommand
{
    public unsafe void Add(ImRenderPass renderPass)
    {
        renderPass.DrawIndexedPrimatives(numberOfIndices, numberOfInstances, firstIndex, vertexOffset, firstInstance);
    }
}

public interface IGpuCopyCommand
{
    public unsafe void Add(ImCopyPass copyPass);
}

public class Bind(IRenderBindable value, uint slot = 0) : IGpuRenderCommand
{
    public unsafe void Add(ImRenderPass renderPass)
    {
        renderPass.Bind(value, slot);
    }
}

public class UploadToGpu(IGpuUploadable value): IGpuCopyCommand
{
    public unsafe void Add(ImCopyPass copyPass)
    {
        copyPass.Upload(value);
    }
}