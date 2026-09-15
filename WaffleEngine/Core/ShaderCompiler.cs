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

    public static bool CompileRasterShader(string shaderPath, [NotNullWhen(true)] out Shader? shader)
    {
        shader = null;
        
        string source = File.ReadAllText(shaderPath);
        
        var vertexInfo = new ShaderCross.HLSLInfo()
        {
            ManagedEntrypoint = "vsMain",
            ShaderStage = ShaderCross.ShaderStage.Vertex,
            ManagedSource = source,
            IncludeDir = IntPtr.Zero,
            Defines = IntPtr.Zero,
            Props = 0,
        };
        
        var fragmentInfo = new ShaderCross.HLSLInfo()
        {
            ManagedEntrypoint = "fsMain",
            ShaderStage = ShaderCross.ShaderStage.Fragment,
            ManagedSource = source,
            IncludeDir = IntPtr.Zero,
            Defines = IntPtr.Zero,
            Props = 0,
        };

        IntPtr vertexSpriv = ShaderCross.CompileSPIRVFromHLSL(ref vertexInfo, out UIntPtr vertexSize);

        if (vertexSpriv == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }

        var vertexSprivInfo = new ShaderCross.SPIRVInfo()
        {
            ByteCode = vertexSpriv,
            ByteCodeSize = vertexSize,
            Entrypoint = vertexInfo.Entrypoint,
            ShaderStage = vertexInfo.ShaderStage,
            Props = vertexInfo.Props,
        };
        
        IntPtr fragmentSpriv = ShaderCross.CompileSPIRVFromHLSL(ref fragmentInfo, out UIntPtr fragmentSize);

        if (fragmentSpriv == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }
        
        var fragmentSprivInfo = new ShaderCross.SPIRVInfo()
        {
            ByteCode = fragmentSpriv,
            ByteCodeSize = fragmentSize,
            Entrypoint = fragmentInfo.Entrypoint,
            ShaderStage = fragmentInfo.ShaderStage,
            Props = fragmentInfo.Props,
        };

        NativePtr<ShaderCross.GraphicsShaderMetadata> metadata = ShaderCross.ReflectGraphicsSPIRV(fragmentSpriv, fragmentSize, 0);
        
        IntPtr vertexShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, ref vertexSprivInfo, ref metadata.Value.ResourceInfo, 0);

        if (vertexShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }

        IntPtr fragmentShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, ref fragmentSprivInfo, ref metadata.Value.ResourceInfo, 0);
        
        if (fragmentShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";
        
        WLog.Info($"Shader compiled: {relPath}");
        
        shader = new Shader(
            vertexShader, 
            fragmentShader,
            metadata.Value.ResourceInfo.NumSamplers, 
            metadata.Value.ResourceInfo.NumUniformBuffers, 
            metadata.Value.ResourceInfo.NumStorageBuffers, 
            metadata.Value.ResourceInfo.NumStorageTextures);
        
        SDL.Free(metadata);
        SDL.Free(vertexSpriv);
        SDL.Free(fragmentSpriv);

        return true;
    }

    public static bool CompileComputeShader(string shaderPath, [NotNullWhen(true)] out ComputeShader? shader)
    {
        shader = null;
        
        string source = File.ReadAllText(shaderPath);
        
        var computeInfo = new ShaderCross.HLSLInfo()
        {
            ManagedEntrypoint = "main",
            ShaderStage = ShaderCross.ShaderStage.Compute,
            ManagedSource = source,
            IncludeDir = IntPtr.Zero,
            Defines = IntPtr.Zero,
            Props = 0,
        };

        IntPtr computeSpriv = ShaderCross.CompileSPIRVFromHLSL(ref computeInfo, out UIntPtr computeSize);

        if (computeSpriv == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }

        var computeSprivInfo = new ShaderCross.SPIRVInfo()
        {
            ByteCode = computeSpriv,
            ByteCodeSize = computeSize,
            Entrypoint = computeInfo.Entrypoint,
            ShaderStage = computeInfo.ShaderStage,
            Props = computeInfo.Props,
        };

        NativePtr<ShaderCross.ComputePipelineMetadata> metadata = ShaderCross.ReflectComputeSPIRV(computeSpriv, computeSize, 0);

        IntPtr computeShader =
            ShaderCross.CompileComputePipelineFromSPIRV(Device.Handle, in computeSprivInfo, in metadata.Value, 0);
        
        if (computeShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }
        
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";
        
        WLog.Info($"Shader compiled: {relPath}");
        
        shader = new ComputeShader(
            computeShader, 
            metadata.Value.NumSamplers, 
            metadata.Value.NumReadOnlyStorageTextures, 
            metadata.Value.NumReadWriteStorageTextures, 
            metadata.Value.NumUniformBuffers,
            metadata.Value.NumReadOnlyStorageBuffers,
            metadata.Value.NumReadWriteStorageBuffers,
            metadata.Value.ThreadCountX,
            metadata.Value.ThreadCountY,
            metadata.Value.ThreadCountZ);
        
        SDL.Free(metadata);
        SDL.Free(computeSpriv);

        return true;
    }
}