using System.Diagnostics.CodeAnalysis;
using SDL3;
using WaffleEngine.Native;
using WaffleEngine.Rendering;
using WaffleEngine.Serializer;

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

    public struct ShaderProgram
    {
        public IntPtr Handle;
    }

    public static bool CompileVertexShader(string shaderPath, string entrypoint, out ShaderProgram compiledShader)
    {
        compiledShader = new ();

        if (!File.Exists(shaderPath))
        {
            WLog.Error($"Shader failed to compile: file not found at {shaderPath}");
            return false;
        }
        
        string vertSource = File.ReadAllText(shaderPath);

        return CompileHlslToShader(vertSource, entrypoint, ShaderCross.ShaderStage.Vertex, true, out compiledShader);
    }
    
    public static bool CompileFragmentShader(string shaderPath, string entrypoint, out ShaderProgram compiledShader)
    {
        compiledShader = new ();

        if (!File.Exists(shaderPath))
        {
            WLog.Error($"Shader failed to compile: file not found at {shaderPath}");
            return false;
        }
        
        string vertSource = File.ReadAllText(shaderPath);

        return CompileHlslToShader(vertSource, entrypoint, ShaderCross.ShaderStage.Fragment, true, out compiledShader);
    }

    private static bool CompileHlslToShader(string source, string entrypoint, ShaderCross.ShaderStage stage, bool debug, out ShaderProgram compiledShader)
    {
        compiledShader = new();
        
        var hlslInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = debug,
            Entrypoint = entrypoint,
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = stage,
            Source = source,
            Defines = IntPtr.Zero,
        };
        
        IntPtr spirv = ShaderCross.CompileSPIRVFromHLSL(in hlslInfo, out UIntPtr size);

        if (spirv == IntPtr.Zero)
        {
            WLog.Error($"Failed to compile shader: {SDL.GetError()}");
            return false;
        }
        
        NativePtr<ShaderCross.GraphicsShaderMetadata> metadata = ShaderCross.ReflectGraphicsSPIRV(spirv, size, 0);

        if (metadata.IsNull)
        {
            WLog.Error($"Failed shader reflection: {SDL.GetError()}");
            return false;
        }
        
        var spirvInfo = new ShaderCross.SPIRVInfo()
        {
            ByteCode = spirv,
            ByteCodeSize = size,
            Entrypoint = hlslInfo.Entrypoint,
            Name = hlslInfo.Name,
            Props = hlslInfo.Props,
            ShaderStage = hlslInfo.ShaderStage,
        };

        compiledShader.Handle = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, in spirvInfo, metadata.Value, 0);
        
        SDL.Free(metadata);
        SDL.Free(spirv);
        
        return true;
    }

    public static bool CompileComputeShader(string shaderPath, string entrypoint, [NotNullWhen(true)] out ComputeShader? shader)
    {
        shader = null;
        
        string source = File.ReadAllText(shaderPath);
        
        var computeInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = true,
            Entrypoint = entrypoint,
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = ShaderCross.ShaderStage.Compute,
            Source = source,
            Defines = IntPtr.Zero,
        };

        IntPtr computeSpriv = ShaderCross.CompileSPIRVFromHLSL(in computeInfo, out UIntPtr computeSize);

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
            Name = computeInfo.Name,
            Props = computeInfo.Props,
            ShaderStage = computeInfo.ShaderStage,
        };

        var metadata = ShaderCross.ReflectComputeSPIRV(computeSpriv, computeSize, 0);

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
            metadata.Value.ThreadCountX,
            metadata.Value.ThreadCountY,
            metadata.Value.ThreadCountZ);
        
        SDL.Free(metadata);
        SDL.Free(computeSpriv);

        return true;
    }
}