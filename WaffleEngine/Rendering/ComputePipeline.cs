using SDL3;

namespace WaffleEngine.Rendering;

public struct ComputePipeline
{
    public IntPtr Handle;

    public void TryBuild()
    {
        var createInfo = new SDL.GPUComputePipelineCreateInfo()
        {
            
        };

        Handle = SDL.CreateGPUComputePipeline(Device.Handle, createInfo);
    }
}