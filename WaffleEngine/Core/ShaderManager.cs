using System.Diagnostics.CodeAnalysis;
using SDL3;
using WaffleEngine.Native;
using WaffleEngine.Rendering;

namespace WaffleEngine;

public static class ShaderManager
{
    private static ShaderFormat _format;
    private static string _formatString = "";
    
    private static Dictionary<string, Shader> _shaders = new ();

    private static string[] _slangcArgs =
    [
        "",
        "-target", ""
    ];

    public static bool Init()
    {
        if (!ShaderCross.Init())
        {
            WLog.Error("Failed to initialise the shader compiler.");
            return false;
        }

        return true;
    }
    
    public static void CompileAllShaders()
    {
        var shaderFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.hlsl", SearchOption.AllDirectories);
        
        // Parallel.ForEach(shaderFiles, CompileShader);

        foreach (var shader in shaderFiles)
        {
            CompileShader(shader);
        }
    }

    public static void CompileShader(string shaderPath)
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

        lock (_shaders)
        {
            _shaders.Add(relPath, new Shader(
                vertexShader, 
                fragmentShader,
                metadata.Value.NumSamplers, 
                metadata.Value.NumUniformBuffers, 
                metadata.Value.NumStorageBuffers, 
                metadata.Value.NumStorageTextures));
        }
        
        WLog.Info($"Shader compiled: {relPath}");
    }
    
    
    public static bool TryGetShader(string shaderPath, [NotNullWhen(true)] out Shader? shader)
    {
        return _shaders.TryGetValue(shaderPath, out shader);
    }

    internal static void CleanUp()
    {
        foreach (var shader in _shaders.Values)
        {
            shader.Dispose();
        }
        WLog.Info("All Shaders Disposed");
    }
}