using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public class ComputeShader : IComputeBindable
{
    /// <summary>
    /// The number of threads in the X dimension.
    /// </summary>
    public uint ThreadCountX;
        
    /// <summary>
    /// The number of threads in the Y dimension.
    /// </summary>
    public uint ThreadCountY;
        
    /// <summary>
    /// The number of threads in the Z dimension.
    /// </summary>
    public uint ThreadCountZ;

    public IntPtr Handle { get; private set; }
    

    public ComputeShader(
        IntPtr handle,
        uint threadCountX,
        uint threadCountY,
        uint threadCountZ)
    {
        Handle = handle;
        ThreadCountX = threadCountX;
        ThreadCountY = threadCountY;
        ThreadCountZ = threadCountZ;
    }
    
    public void ReleaseGpuShaders()
    {
        if (Handle != IntPtr.Zero)
        {
            SDL.ReleaseGPUComputePipeline(Device.Handle, Handle);
            Handle = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        ReleaseGpuShaders();
    }

    public void Bind(ImComputePass pass, uint slot = 0)
    {
        SDL.BindGPUComputePipeline(pass.Handle, Handle);
    }
}