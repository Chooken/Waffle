using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public sealed unsafe class Shader : IDisposable
{
    public byte[] Bytecode { get; private set; }
    public string VertexEntry { get; private set; }
    public string FragmentEntry { get; private set; }
    public ShaderFormat Format { get; private set; }
    public int Samplers { get; private set; }
    public int UniformBuffers { get; private set; }
    public int StorageBuffers { get; private set; }
    public int StorageTextures { get; private set; }

    public IntPtr VertexHandle { get; private set; }
    public IntPtr FragmentHandle { get; private set; }
    
    public Pipeline? Pipeline { get; private set; }

    public Shader(
        byte[] bytecode, 
        string vertexEntry, 
        string fragmentEntry, 
        ShaderFormat format, 
        uint samplers, 
        uint uniformBuffers, 
        uint storageBuffers, 
        uint storageTextures)
    {
        Bytecode = bytecode;
        VertexEntry = vertexEntry;
        FragmentEntry = fragmentEntry;
        Format = format;
        Samplers = (int) samplers;
        UniformBuffers = (int) uniformBuffers;
        StorageBuffers = (int) storageBuffers;
        StorageTextures = (int) storageTextures;
        
        Build();
    }

    private void Build()
    {
        var vertexEntryPtr = Marshal.StringToHGlobalAnsi(VertexEntry);
        var fragmentEntryPtr = Marshal.StringToHGlobalAnsi(FragmentEntry);
        
        SDL.GPUShaderCreateInfo shaderInfo;
        
        fixed (byte* bytecodePtr = Bytecode)
        {
            shaderInfo = new SDL.GPUShaderCreateInfo()
            {
                Code = (IntPtr)bytecodePtr,
                CodeSize = (uint)Bytecode.Length,
                Entrypoint = vertexEntryPtr,
                Stage = SDL.GPUShaderStage.Vertex,
                Format = (SDL.GPUShaderFormat)Format,
                NumSamplers = (uint)Samplers,
                NumUniformBuffers = (uint)UniformBuffers,
                NumStorageBuffers = (uint)StorageBuffers,
                NumStorageTextures = (uint)StorageTextures
            };
        }
        
        VertexHandle = SDL.CreateGPUShader(Device._gpuDevicePtr, shaderInfo);

        shaderInfo.Entrypoint = fragmentEntryPtr;
        shaderInfo.Stage = SDL.GPUShaderStage.Fragment;
        
        FragmentHandle = SDL.CreateGPUShader(Device._gpuDevicePtr, shaderInfo);
        
        Marshal.FreeHGlobal(vertexEntryPtr);
        Marshal.FreeHGlobal(fragmentEntryPtr);
        
        if (!Pipeline.TryBuild(PipelineSettings.Default, this, out Pipeline pipeline))
            WLog.Error("Failed to Build Pipeline", "Shader");

        Pipeline = pipeline;
    }

    public void SetPipeline(PipelineSettings settings)
    {
        if (!Pipeline.TryBuild(settings, this, out Pipeline pipeline))
            WLog.Error("Failed to Build Pipeline", "Shader");
        
        Pipeline?.Dispose();

        Pipeline = pipeline;
    }
    
    public void ReleaseGpuShaders()
    {
        if (VertexHandle != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(Device._gpuDevicePtr, VertexHandle);
            VertexHandle = IntPtr.Zero;
        }
        
        if (FragmentHandle != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(Device._gpuDevicePtr, FragmentHandle);
            FragmentHandle = IntPtr.Zero;
        }
        
        Pipeline?.Dispose();
    }

    public void Dispose()
    {
        ReleaseGpuShaders();
    }

    public void Bind(IntPtr pass)
    {
        if (Pipeline is null)
        {
            WLog.Error("Unable to Bind Shader as pipeline is not created.", "Shader");
            return;
        }
        
        Pipeline.Bind(pass);
    }
}