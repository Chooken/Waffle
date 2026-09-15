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
            SDL.Free(vertexSpriv);
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

        NativePtr<ShaderCross.GraphicsShaderMetadata> vertexMetadata = ShaderCross.ReflectGraphicsSPIRV(vertexSpriv, vertexSize, 0);

        if (vertexMetadata.IsNull)
        {
            WLog.Error(SDL.GetError());
            SDL.Free(vertexSpriv);
            SDL.Free(fragmentSpriv);
            return false;
        }
        
        NativePtr<ShaderCross.GraphicsShaderMetadata> fragmentMetadata = ShaderCross.ReflectGraphicsSPIRV(fragmentSpriv, fragmentSize, 0);

        if (fragmentMetadata.IsNull)
        {
            WLog.Error(SDL.GetError());
            SDL.Free(vertexMetadata);
            SDL.Free(vertexSpriv);
            SDL.Free(fragmentSpriv);
            return false;
        }
        
        IntPtr vertexShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, ref vertexSprivInfo, ref vertexMetadata.Value.ResourceInfo, 0);

        if (vertexShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            SDL.Free(vertexMetadata);
            SDL.Free(fragmentMetadata);
            SDL.Free(vertexSpriv);
            SDL.Free(fragmentSpriv);
            return false;
        }

        IntPtr fragmentShader = ShaderCross.CompileGraphicsShaderFromSPIRV(Device.Handle, ref fragmentSprivInfo, ref fragmentMetadata.Value.ResourceInfo, 0);
        
        if (fragmentShader == IntPtr.Zero)
        {
            WLog.Error(SDL.GetError());
            SDL.ReleaseGPUShader(Device.Handle, vertexShader);
            SDL.Free(vertexMetadata);
            SDL.Free(fragmentMetadata);
            SDL.Free(vertexSpriv);
            SDL.Free(fragmentSpriv);
            return false;
        }
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";
        
        WLog.Info($"Shader compiled: {relPath}");

        var inputs = new NativeArray<ShaderCross.IOVarMetadata>(vertexMetadata.Value.Inputs, vertexMetadata.Value.NumInputs);

        var vertex_inputs = new VertexInput[inputs.Length];

        for (int i = 0; i < inputs.Length; i++)
        {
            ShaderCross.IOVarMetadata input =  inputs[i];

            VertexAttributeType type = input.VectorSize switch
            {
                1 => input.VectorType switch
                {
                    ShaderCross.IOVarType.Float32 => VertexAttributeType.Float,
                    ShaderCross.IOVarType.Int32 => VertexAttributeType.Int,
                    ShaderCross.IOVarType.UInt32 => VertexAttributeType.UInt,
                    _ => VertexAttributeType.Invalid,
                },

                2 => input.VectorType switch
                {
                    ShaderCross.IOVarType.Float16 => VertexAttributeType.Half2,
                    ShaderCross.IOVarType.Float32 => VertexAttributeType.Float2,
                    ShaderCross.IOVarType.Int8 => VertexAttributeType.Byte2,
                    ShaderCross.IOVarType.UInt8 => VertexAttributeType.UByte2,
                    ShaderCross.IOVarType.Int16 => VertexAttributeType.Short2,
                    ShaderCross.IOVarType.UInt16 => VertexAttributeType.UShort2,
                    ShaderCross.IOVarType.Int32 => VertexAttributeType.Int2,
                    ShaderCross.IOVarType.UInt32 => VertexAttributeType.UInt2,
                    _ => VertexAttributeType.Invalid,
                },

                3 => input.VectorType switch
                {
                    ShaderCross.IOVarType.Float32 => VertexAttributeType.Float3,
                    ShaderCross.IOVarType.Int32 => VertexAttributeType.Int3,
                    ShaderCross.IOVarType.UInt32 => VertexAttributeType.UInt3,
                    _ => VertexAttributeType.Invalid,
                },

                4 => input.VectorType switch
                {
                    ShaderCross.IOVarType.Float16 => VertexAttributeType.Half4,
                    ShaderCross.IOVarType.Float32 => VertexAttributeType.Float4,
                    ShaderCross.IOVarType.Int8 => VertexAttributeType.Byte4,
                    ShaderCross.IOVarType.UInt8 => VertexAttributeType.UByte4,
                    ShaderCross.IOVarType.Int16 => VertexAttributeType.Short4,
                    ShaderCross.IOVarType.UInt16 => VertexAttributeType.UShort4,
                    ShaderCross.IOVarType.Int32 => VertexAttributeType.Int4,
                    ShaderCross.IOVarType.UInt32 => VertexAttributeType.UInt4,
                    _ => VertexAttributeType.Invalid,
                },
                _ => VertexAttributeType.Invalid,
            };

            vertex_inputs[i] = new VertexInput
            {
                Location = input.Location,
                AttributeType = type,
            };
        }
        
        shader = new Shader(
            vertexShader, 
            fragmentShader,
            vertexMetadata.Value.ResourceInfo.NumSamplers + fragmentMetadata.Value.ResourceInfo.NumSamplers, 
            vertexMetadata.Value.ResourceInfo.NumUniformBuffers + fragmentMetadata.Value.ResourceInfo.NumUniformBuffers, 
            vertexMetadata.Value.ResourceInfo.NumStorageBuffers + fragmentMetadata.Value.ResourceInfo.NumStorageBuffers, 
            vertexMetadata.Value.ResourceInfo.NumStorageTextures + fragmentMetadata.Value.ResourceInfo.NumStorageTextures,
            vertex_inputs);
        
        SDL.Free(vertexMetadata);
        SDL.Free(fragmentMetadata);
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