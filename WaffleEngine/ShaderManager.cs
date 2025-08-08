using System.Diagnostics.CodeAnalysis;
using Slangc.NET;
using Slangc.NET.Enums;
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

    public static void Init()
    {
        _format = Device.GetShaderFormat();

        if (_format.HasFlag(ShaderFormat.SpirV))
        {
            _format = ShaderFormat.SpirV;
            _formatString = "spirv";
        }
        else if (_format.HasFlag(ShaderFormat.Msl))
        {
            _format = ShaderFormat.Msl;
            _formatString = "metal";
        }
        else if (_format.HasFlag(ShaderFormat.Dxil))
        {
            _format = ShaderFormat.Dxil;
            _formatString = "dxil";
        }
        else
        {
            WLog.Error("Failed to find a suiteable shader format", "ShaderManager");
        }
    }
    
    public static void CompileAllShaders()
    {
        var shaderFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.slang", SearchOption.AllDirectories);
        
        if (_formatString == "")
        {
            WLog.Error("Shader format not initialised", "ShaderManager");
            return;
        }
        
        //Parallel.ForEach(shaderFiles, CompileShader);

        foreach (var shader in shaderFiles)
        {
            CompileShader(shader);
        }
    }

    public static void CompileShader(string shaderPath)
    {
        if (_formatString == "")
        {
            WLog.Error("Shader format not initialised", "ShaderManager");
            return;
        }

        _slangcArgs[0] = shaderPath;
        _slangcArgs[2] = _formatString;

        // This Uses a SHHIIITT ton of memory on first call probably won't
        // include in Engine instead use in Editor in the future.
        byte[] bytecode = SlangCompiler.CompileWithReflection(_slangcArgs, out var reflection);

        string? vertexEntry = null;
        string? fragmentEntry = null;
        
        uint samplerCount = 0;
        uint uniformBufferCount = 0;
        uint storageBufferCount = 0;
        uint storageTextureCount = 0;

        foreach (var entry in reflection.EntryPoints)
        {
            if (entry.Stage == SlangStage.Vertex)
            {
                vertexEntry = entry.Name;
                continue;
            }

            if (entry.Stage == SlangStage.Fragment)
            {
                fragmentEntry = entry.Name;
                continue;
            }
        }
        
        foreach (var parameter in reflection.Parameters)
        {
            switch (parameter.Type.Kind)
            {
                case SlangTypeKind.Resource:
                    switch (parameter.Type.Resource.BaseShape)
                    {
                        case SlangResourceShape.StructuredBuffer:
                            storageBufferCount++;
                            break;
                    }
                    break;
                case SlangTypeKind.ConstantBuffer:
                    uniformBufferCount++;
                    break;
                case SlangTypeKind.SamplerState:
                    samplerCount++;
                    break;
            }
        }

        bool failed = false;
        
        if (vertexEntry == null)
        {
            WLog.Error("Failed to compile shader, missing a vertex entry point", "ShaderManager");
            failed = true;
        }

        if (fragmentEntry == null)
        {
            WLog.Error("Failed to compile shader, missing a fragment entry point", "ShaderManager");
            failed = true;
        }

        if (failed)
            return;
        
        string relPath = $"{Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(shaderPath) ?? string.Empty)}/{Path.GetFileNameWithoutExtension(shaderPath)}";

        lock (_shaders)
        {
            _shaders.Add(relPath, new Shader(
                bytecode, 
                vertexEntry!, 
                fragmentEntry!, 
                _format, 
                samplerCount, 
                uniformBufferCount, 
                storageBufferCount, 
                storageTextureCount));
        }
        
        WLog.Info($"Shader compiled: {relPath}", "ShaderManager");
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
        WLog.Info("All Shaders Disposed", "ShaderManager");
    }
}