using System.Runtime.InteropServices;
using SDL3;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public sealed unsafe class Shader
{
    internal byte[] _bytecode;
    internal string _vertexEntry;
    private IntPtr _vertexEntryPtr;
    internal string _fragmentEntry;
    private IntPtr _fragmentEntryPtr;
    internal ShaderFormat _format;
    internal uint _samplerCount = 0;
    internal uint _uniformBufferCount = 0;
    internal uint _storageBufferCount = 0;
    internal uint _storageTextureCount = 0;
    
    private IntPtr _gpuVertexShader;
    private IntPtr _gpuFragmentShader;

    private PipelineSettings _pipelineSettings;
    private Pipeline? _pipeline;
    
    public Shader(byte[] bytecode, string vertexEntry, string fragmentEntry, ShaderFormat format)
    {
        _bytecode = bytecode;
        _vertexEntry = vertexEntry;
        _vertexEntryPtr = Marshal.StringToHGlobalAnsi(vertexEntry);
        _fragmentEntry = fragmentEntry;
        _fragmentEntryPtr = Marshal.StringToHGlobalAnsi(fragmentEntry);
        _format = format;
        _pipelineSettings = PipelineSettings.Default;
    }
    
    public Shader(byte[] bytecode, string vertexEntry, string fragmentEntry, ShaderFormat format, PipelineSettings pipelineSettings)
    {
        _bytecode = bytecode;
        _vertexEntry = vertexEntry;
        _vertexEntryPtr = Marshal.StringToHGlobalAnsi(vertexEntry);
        _fragmentEntry = fragmentEntry;
        _fragmentEntryPtr = Marshal.StringToHGlobalAnsi(fragmentEntry);
        _format = format;
        _pipelineSettings = pipelineSettings;
    }

    public void SetPipelineSettings(PipelineSettings pipelineSettings) => _pipelineSettings = pipelineSettings;

    public void Build()
    {
        if (_gpuVertexShader != IntPtr.Zero || _gpuFragmentShader != IntPtr.Zero)
            return;
        
        var vertexInfo = BuildShaderInfo(_vertexEntryPtr, SDL.GPUShaderStage.Vertex);
        var fragmentInfo = BuildShaderInfo(_fragmentEntryPtr, SDL.GPUShaderStage.Fragment);
        
        _gpuVertexShader = SDL.CreateGPUShader(Device._gpuDevicePtr, vertexInfo);
        _gpuFragmentShader = SDL.CreateGPUShader(Device._gpuDevicePtr, fragmentInfo);

        if (!Pipeline.TryBuild(_pipelineSettings, this, out _pipeline))
            WLog.Error("Failed to Build Pipeline", "Shader");
    }

    private SDL.GPUShaderCreateInfo BuildShaderInfo(IntPtr entryPoint, SDL.GPUShaderStage stage)
    {
        SDL.GPUShaderCreateInfo shaderInfo = new SDL.GPUShaderCreateInfo();
        
        fixed (byte* bytecodePtr = _bytecode)
        {
            shaderInfo.Code = (IntPtr)bytecodePtr;
        }
        
        shaderInfo.CodeSize = (UIntPtr)_bytecode.Length;
        
        shaderInfo.Entrypoint = entryPoint;
        shaderInfo.Stage = stage;

        shaderInfo.Format = (SDL.GPUShaderFormat)_format;
        shaderInfo.NumSamplers = _samplerCount;
        shaderInfo.NumUniformBuffers = _uniformBufferCount;
        shaderInfo.NumStorageBuffers = _storageBufferCount;
        shaderInfo.NumStorageTextures = _storageTextureCount;
        return shaderInfo;
    }

    public bool TryGetShaders(out IntPtr vertexShader, out IntPtr fragmentShader)
    {
        vertexShader = _gpuVertexShader;
        fragmentShader = _gpuFragmentShader;
        
        if (vertexShader == IntPtr.Zero)
        {
            WLog.Error("Gpu Vertex Shader was not built", "Shader");
            return false;
        }
        
        if (fragmentShader == IntPtr.Zero)
        {
            WLog.Error("Gpu Fragment Shader was not built", "Shader");
            return false;
        }
        
        return true;
    }

    public void ReleaseGpuShaders()
    {
        if (_gpuVertexShader != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(Device._gpuDevicePtr, _gpuVertexShader);
            _gpuVertexShader = IntPtr.Zero;
        }
        
        if (_gpuFragmentShader != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(Device._gpuDevicePtr, _gpuFragmentShader);
            _gpuFragmentShader = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        ReleaseGpuShaders();
    }

    public void Bind(IntPtr renderPass)
    {
        if (_pipeline is null)
        {
            WLog.Error("Pipeline is not created.", "Shader");
            return;
        }
        
        _pipeline.Bind(renderPass);
    }
}