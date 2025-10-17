using System.Diagnostics.CodeAnalysis;
using SDL3;
using WaffleEngine.Native;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public static class ShaderCompiler
{
    public static bool Init()
    {
        if (!ShaderCross.Init())
        {
            WLog.Error("Failed to initialise the shader compiler.");
            return false;
        }

        return true;
    }

    public static Shader CompileShader(string shaderPath)
    {
        string source = File.ReadAllText(shaderPath);
        
        var vertexInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = true,
            Entrypoint = "vsMain",
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = ShaderCross.ShaderStage.Vertex,
            Source = source,
            Defines = IntPtr.Zero,
        };
        
        var fragmentInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = true,
            Entrypoint = "fsMain",
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = ShaderCross.ShaderStage.Fragment,
            Source = source,
            Defines = IntPtr.Zero,
        };

        IntPtr vertexSpriv = ShaderCross.CompileSPIRVFromHLSL(in vertexInfo, out UIntPtr vertexSize);

        var vertexSprivInfo = new ShaderCross.SPIRVInfo()
        {
            ByteCode = vertexSpriv,
            ByteCodeSize = vertexSize,
            Entrypoint = vertexInfo.Entrypoint,
            Name = vertexInfo.Name,
            Props = vertexInfo.Props,
            ShaderStage = vertexInfo.ShaderStage,
        };
        
        IntPtr fragmentSpriv = ShaderCross.CompileSPIRVFromHLSL(in fragmentInfo, out UIntPtr fragmentSize);

        var fragmentSprivInfo = new ShaderCross.SPIRVInfo()
        {
            ByteCode = fragmentSpriv,
            ByteCodeSize = fragmentSize,
            Entrypoint = fragmentInfo.Entrypoint,
            Name = fragmentInfo.Name,
            Props = fragmentInfo.Props,
            ShaderStage = fragmentInfo.ShaderStage
        };

        NativePtr<ShaderCross.GraphicsShaderMetadata> metadata = ShaderCross.ReflectGraphicsSPIRV(fragmentSpriv, fragmentSize, 0);
        
        IntPtr vertexShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, in vertexSprivInfo, in metadata.Value, 0);
        
        if (vertexShader == IntPtr.Zero)
            WLog.Error(SDL.GetError());
        
        IntPtr fragmentShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, in fragmentSprivInfo, in metadata.Value, 0);
        if (fragmentShader == IntPtr.Zero)
            WLog.Error(SDL.GetError());
        
        SDL.Free(metadata);
        SDL.Free(vertexSpriv);
        SDL.Free(fragmentSpriv);
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";
        
        WLog.Info($"Shader compiled: {relPath}");
        
        return new Shader(
            vertexShader, 
            fragmentShader,
            metadata.Value.NumSamplers, 
            metadata.Value.NumUniformBuffers, 
            metadata.Value.NumStorageBuffers, 
            metadata.Value.NumStorageTextures);
    }
}