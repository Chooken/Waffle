using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Rendering.Immediate;

namespace WaffleEngine;

public sealed unsafe class Shader : IRenderBindable, IDisposable
{
    public IntPtr VertexHandle { get; private set; }
    public IntPtr FragmentHandle { get; private set; }

    public Pipeline Pipeline { get; private set; } = new Pipeline();

    public Shader(
        IntPtr vertexHandle,
        IntPtr fragmentHandle,
        PipelineSettings settings)
    {
        VertexHandle = vertexHandle;
        FragmentHandle = fragmentHandle;
        
        if (!Pipeline.TryBuild(settings, this))
            WLog.Error("Failed to Build Pipeline");
    }

    public void SetPipeline(PipelineSettings settings)
    {
        if (!Pipeline.TryBuild(settings, this))
            WLog.Error("Failed to Build Pipeline");
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

        Pipeline.Dispose();
    }

    public void Dispose()
    {
        ReleaseGpuShaders();
    }

    public void Bind(ImRenderPass pass, uint _ = 0)
    {
        if (Pipeline.Handle == IntPtr.Zero)
        {
            WLog.Error("Unable to Bind Shader as pipeline is not created.");
            return;
        }
        
        Pipeline.Bind(pass);
    }
}