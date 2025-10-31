using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public class ComputeShader : IComputeBindable
{
    /// <summary>
    /// The number of samplers defined in the shader.
    /// </summary>
    public uint Samplers;
        
    /// <summary>
    /// The number of readonly storage textures defined in the shader. 
    /// </summary>
    public uint ReadOnlyStorageTextures;
        
    /// <summary>
    /// The number of readonly storage buffers defined in the shader.
    /// </summary>
    public uint ReadOnlyStorageBuffers;
        
    /// <summary>
    /// The number of read-write storage textures defined in the shader.
    /// </summary>
    public uint ReadWriteStorageTextures;
        
    /// <summary>
    /// The number of read-write storage buffers defined in the shader.
    /// </summary>
    public uint ReadWriteStorageBuffers;
        
    /// <summary>
    /// The number of uniform buffers defined in the shader.
    /// </summary>
    public uint UniformBuffers;
        
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
        uint samplers, 
        uint readOnlyStorageTextures,
        uint readWriteStorageTextures,
        uint uniformBuffers, 
        uint readOnlyStorageBuffers, 
        uint readWriteStorageBuffers,
        uint threadCountX,
        uint threadCountY,
        uint threadCountZ)
    {
        Handle = handle;
        Samplers = samplers;
        ReadOnlyStorageTextures = readOnlyStorageTextures;
        ReadWriteStorageTextures = readWriteStorageTextures;
        UniformBuffers = uniformBuffers;
        ReadOnlyStorageBuffers = readOnlyStorageBuffers;
        ReadWriteStorageBuffers = readWriteStorageBuffers;
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