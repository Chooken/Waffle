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

    public static bool CompileRasterShader(string shaderPath, [NotNullWhen(true)] out Shader? shader)
    {
        shader = null;

        if (!File.Exists(shaderPath + "/frag.hlsl"))
        {
            WLog.Error($"Shader failed to compile: No frag.hlsl found in {shaderPath}");
            return false;
        }
        
        if (!File.Exists(shaderPath + "/vert.hlsl"))
        {
            WLog.Error($"Shader failed to compile: No frag.hlsl found in {shaderPath}");
            return false;
        }

        PipelineSettings settings = PipelineSettings.Default;
        
        if (File.Exists(shaderPath + "/pipeline.yaml"))
        {
            if (!Yaml.TryDeserialize(shaderPath + "/pipeline.yaml", out settings))
            {
                WLog.Error($"Failed to deserialize pipeline in: {shaderPath}");
                return false;
            }
        }
        
        string vertSource = File.ReadAllText(shaderPath + "/vert.hlsl");
        string fragSource = File.ReadAllText(shaderPath + "/frag.hlsl");
        
        var vertexInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = true,
            Entrypoint = "main",
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = ShaderCross.ShaderStage.Vertex,
            Source = vertSource,
            Defines = IntPtr.Zero,
        };
        
        var fragmentInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = true,
            Entrypoint = "main",
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = ShaderCross.ShaderStage.Fragment,
            Source = fragSource,
            Defines = IntPtr.Zero,
        };

        IntPtr vertexSpriv = ShaderCross.CompileSPIRVFromHLSL(in vertexInfo, out UIntPtr vertexSize);

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
            Name = vertexInfo.Name,
            Props = vertexInfo.Props,
            ShaderStage = vertexInfo.ShaderStage,
        };
        
        IntPtr fragmentSpriv = ShaderCross.CompileSPIRVFromHLSL(in fragmentInfo, out UIntPtr fragmentSize);

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
            Name = fragmentInfo.Name,
            Props = fragmentInfo.Props,
            ShaderStage = fragmentInfo.ShaderStage
        };

        NativePtr<ShaderCross.GraphicsShaderMetadata> metadata = ShaderCross.ReflectGraphicsSPIRV(fragmentSpriv, fragmentSize, 0);
        
        IntPtr vertexShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, in vertexSprivInfo, in metadata.Value, 0);

        if (vertexShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }

        IntPtr fragmentShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, in fragmentSprivInfo, in metadata.Value, 0);
        if (fragmentShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            return false;
        }

        SDL.Free(metadata);
        SDL.Free(vertexSpriv);
        SDL.Free(fragmentSpriv);
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";
        
        WLog.Info($"Shader compiled: {relPath}");
        
        shader = new Shader(
            vertexShader, 
            fragmentShader,
            settings,
            metadata.Value.NumSamplers, 
            metadata.Value.NumUniformBuffers, 
            metadata.Value.NumStorageBuffers, 
            metadata.Value.NumStorageTextures);;

        return true;
    }

    public static bool CompileComputeShader(string shaderPath, [NotNullWhen(true)] out ComputeShader? shader)
    {
        shader = null;
        
        string source = File.ReadAllText(shaderPath);
        
        var computeInfo = new ShaderCross.HLSLInfo()
        {
            EnableDebug = true,
            Entrypoint = "main",
            IncludeDir = null,
            Name = null,
            Props = 0,
            ShaderStage = ShaderCross.ShaderStage.Vertex,
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
        
        SDL.Free(metadata);
        SDL.Free(computeSpriv);
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";
        
        WLog.Info($"Shader compiled: {relPath}");
        
        shader = new ComputeShader(
            computeShader, 
            metadata.Value.NumSamplers, 
            metadata.Value.NumReadOnlyStorageTextures, 
            metadata.Value.NumReadWriteStorageTextures, 
            metadata.Value.NumUniformBuffers,
            metadata.Value.NumReadOnlyStorageBuffers,
            metadata.Value.NumReadwriteStorageBuffers,
            metadata.Value.ThreadCountX,
            metadata.Value.ThreadCountY,
            metadata.Value.ThreadCountZ);

        return true;
    }
}