using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public sealed unsafe class Shader : IRenderBindable, IDisposable
{
    public int Samplers { get; private set; }
    public int UniformBuffers { get; private set; }
    public int StorageBuffers { get; private set; }
    public int StorageTextures { get; private set; }

    public IntPtr VertexHandle { get; private set; }
    public IntPtr FragmentHandle { get; private set; }
    
    public Pipeline? Pipeline { get; private set; }

    public Shader(
        IntPtr vertexHandle,
        IntPtr fragmentHandle,
        uint samplers, 
        uint uniformBuffers, 
        uint storageBuffers, 
        uint storageTextures)
    {
        VertexHandle = vertexHandle;
        FragmentHandle = fragmentHandle;
        Samplers = (int) samplers;
        UniformBuffers = (int) uniformBuffers;
        StorageBuffers = (int) storageBuffers;
        StorageTextures = (int) storageTextures;
        
        Build();
    }

    private void Build()
    {
        if (!Pipeline.TryBuild(PipelineSettings.Default, this, out Pipeline? pipeline))
            WLog.Error("Failed to Build Pipeline");

        Pipeline = pipeline;
    }

    public void SetPipeline(PipelineSettings settings)
    {
        if (!Pipeline.TryBuild(settings, this, out Pipeline? pipeline))
            WLog.Error("Failed to Build Pipeline");
        
        Pipeline?.Dispose();

        Pipeline = pipeline;
    }
    
    public void ReleaseGpuShaders()
    {
        if (VertexHandle != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(Device.Handle, VertexHandle);
            VertexHandle = IntPtr.Zero;
        }
        
        if (FragmentHandle != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(Device.Handle, FragmentHandle);
            FragmentHandle = IntPtr.Zero;
        }
        
        Pipeline?.Dispose();
    }

    public void Dispose()
    {
        ReleaseGpuShaders();
    }

    public void Bind(ImRenderPass pass, uint _ = 0)
    {
        if (Pipeline is null)
        {
            WLog.Error("Unable to Bind Shader as pipeline is not created.");
            return;
        }
        
        Pipeline.Bind(pass);
    }
}