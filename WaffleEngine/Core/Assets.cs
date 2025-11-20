using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using VYaml.Parser;
using WaffleEngine.Rendering;
using WaffleEngine.Serializer;
using ThreadState = System.Threading.ThreadState;

namespace WaffleEngine;

public static class Assets
{
    private static Thread? _ioThread;
    private static bool _running = true;
    
    private static ConcurrentDictionary<string, AssetBundle> _assetBundles = new ConcurrentDictionary<string, AssetBundle>();
    private static ConcurrentQueue<(string, AssetLoadRequest)> requests = new ();

    public static bool TryLoadAssetBundle(string filepath)
    {
        AssetLoadRequest loadRequest = AsyncTryLoadAssetBundle(filepath);

        while (!loadRequest.IsFinished)
        {
            Thread.Sleep(10);
        }

        return !loadRequest.Failed;
    }

    public static AssetLoadRequest AsyncTryLoadAssetBundle(string filepath)
    {
        AssetLoadRequest loadRequest = new ();
        
        requests.Enqueue((filepath, loadRequest));

        return loadRequest;
    }

    public static void StartAssetThread()
    {
        if (_ioThread is not null)
            return;

        _ioThread = new Thread(ThreadLoadAssetBundles) { IsBackground = true };
        _ioThread.Start();
    }

    private static void ThreadLoadAssetBundles()
    {
        while (_running)
        {
            if (!requests.TryDequeue(out var request))
            {
                Thread.Sleep(100);
                continue;
            }

            if (_assetBundles.ContainsKey(request.Item1))
            {
                WLog.Warning($"Tried to load an asset bundle which has already been loaded: {request.Item1}");
                request.Item2.Failed = true;
                request.Item2.IsFinished = true;
                continue;
            }
            
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}/Assets/{request.Item1}";

            if (!Directory.Exists(path))
            {
                WLog.Warning($"Tried to load an asset bundle which doesn't exist: {request.Item1}");
                request.Item2.Failed = true;
                request.Item2.IsFinished = true;
                continue;
            }
            
            AssetBundle bundle = new AssetBundle();

            LoadTextureFiles(path, ref bundle);
            LoadShaderFiles(path, ref bundle);

            _assetBundles.TryAdd(request.Item1, bundle);

            request.Item2.Failed = false;
            request.Item2.IsFinished = true;
        }
    }

    private static void LoadTextureFiles(string path, ref AssetBundle bundle)
    {
        var textures = Directory.EnumerateFiles(path, "*.png", SearchOption.AllDirectories);
            
        foreach (var texturePath in textures)
        {
            string name = Path.GetFileNameWithoutExtension(texturePath);

            if (bundle.Textures.ContainsKey(name))
            {
                WLog.Warning($"Bundle contains duplicate texture with name: {name}");
                continue;
            }
                
            Texture texture = new Texture(texturePath);
            bundle.Textures.Add(
                Path.GetFileNameWithoutExtension(texturePath), 
                texture);
        }
    }

    private static void LoadShaderFiles(string path, ref AssetBundle bundle)
    {
        var vertexShaders = Directory.EnumerateFiles(path, "*.vert.hlsl", SearchOption.AllDirectories);
        var fragmentShaders = Directory.EnumerateFiles(path, "*.frag.hlsl", SearchOption.AllDirectories);
        var shaders = Directory.EnumerateFiles(path, "*.shader.yaml", SearchOption.AllDirectories);
        var compute = Directory.EnumerateFiles(path, "*.comp.hlsl", SearchOption.AllDirectories);
        
        foreach (var vertexShaderPath in vertexShaders)
        {
            // Gets rid of .vert.hlsl and then gets the filename in the path.
            string name = Path.GetFileName(vertexShaderPath[..^10]);

            if (bundle.Shaders.ContainsKey(name))
            {
                WLog.Warning($"Bundle contains duplicate texture with name: {name}");
                continue;
            }

            if (!ShaderCompiler.CompileVertexShader(vertexShaderPath, "main", out var vertexShader))
            {
                continue;
            }
                
            bundle.VertexPrograms.Add(
                name,
                vertexShader);
        }
        
        foreach (var fragmentShaderPath in fragmentShaders)
        {
            // Gets rid of .frag.hlsl and then gets the filename in the path.
            string name = Path.GetFileName(fragmentShaderPath[..^10]);

            if (bundle.Shaders.ContainsKey(name))
            {
                WLog.Warning($"Bundle contains duplicate texture with name: {name}");
                continue;
            }

            if (!ShaderCompiler.CompileFragmentShader(fragmentShaderPath, "main", out var fragmentShader))
            {
                continue;
            }
                
            bundle.FragmentPrograms.Add(
                name,
                fragmentShader);
        }

        foreach (var shaderPath in shaders)
        {
            // Gets rid of .shader.yaml and then gets the filename in the path.
            string name = Path.GetFileName(shaderPath[..^12]);

            if (bundle.Shaders.ContainsKey(name))
            {
                WLog.Warning($"Bundle contains duplicate texture with name: {name}");
                continue;
            }

            if (!Yaml.TryDeserialize(shaderPath, out ShaderInfo shaderInfo))
            {
                WLog.Error($"Failed to deserialize: {shaderPath} as ShaderInfo");
                continue;
            }

            if (!bundle.VertexPrograms.TryGetValue(shaderInfo.VertexName!, out var vertexProgram))
            {
                WLog.Error($"Failed to find vertex program: {shaderInfo.VertexName} for shader {name}");
                continue;
            }
            
            if (!bundle.FragmentPrograms.TryGetValue(shaderInfo.FragmentName!, out var fragmentProgram))
            {
                WLog.Error($"Failed to find fragment program: {shaderInfo.FragmentName} for shader {name}");
                continue;
            }

            var shader = new Shader(vertexProgram.Handle, fragmentProgram.Handle, shaderInfo.PipelineSettings);
            
            bundle.Shaders.Add(name, shader);
        }
        
        foreach (var computeShaderPath in compute)
        {
            // Gets rid of .frag.hlsl and then gets the filename in the path.
            string name = Path.GetFileName(computeShaderPath[..^10]);

            if (bundle.Shaders.ContainsKey(name))
            {
                WLog.Warning($"Bundle contains duplicate texture with name: {name}");
                continue;
            }

            if (!ShaderCompiler.CompileComputeShader(computeShaderPath, "main", out var computeShader))
            {
                continue;
            }
                
            bundle.ComputeShaders.Add(
                name,
                computeShader);
        }
    }

    public static void UnloadAssetBundle(string bundleName)
    {
        if (bundleName == "builtin")
        {
            WLog.Warning($"Tried to unload the builtin asset bundle.");
            return;
        }
        
        if (!_assetBundles.TryGetValue(bundleName, out var bundle))
        {
            WLog.Warning($"Tried to unload an asset bundle that wasn't loaded: {bundleName}");
            return;
        }
        
        bundle.Dispose();
    }

    public static void Dispose()
    {
        _running = false;
        
        while (_ioThread.IsAlive)
        {
            Thread.Sleep(100);
        }

        foreach (AssetBundle bundle in _assetBundles.Values)
        {
            bundle.Dispose();
        }
    }

    public static bool TryGetTexture(string bundleName, string textureName, [NotNullWhen(true)] out Texture? texture)
    {
        texture = null;

        if (!_assetBundles.TryGetValue(bundleName, out var bundle))
        {
            WLog.Error($"Asset Bundle not loaded: {bundleName}");
            return false;
        }

        if (!bundle.TryGetTexture(textureName, out texture))
        {
            return false;
        }

        return true;
    }
    
    public static bool TryGetShader(string bundleName, string shaderName, [NotNullWhen(true)] out Shader? shader)
    {
        shader = null;

        if (!_assetBundles.TryGetValue(bundleName, out var bundle))
        {
            WLog.Error($"Asset Bundle not loaded: {bundleName}");
            return false;
        }

        if (!bundle.TryGetShader(shaderName, out shader))
        {
            return false;
        }

        return true;
    }
    
    public static bool TryGetComputeShader(string bundleName, string shaderName, [NotNullWhen(true)] out ComputeShader? shader)
    {
        shader = null;

        if (!_assetBundles.TryGetValue(bundleName, out var bundle))
        {
            WLog.Error($"Asset Bundle not loaded: {bundleName}");
            return false;
        }

        if (!bundle.TryGetComputeShader(shaderName, out shader))
        {
            return false;
        }

        return true;
    }
    
    public static bool TryGetBundle(string bundleName, out AssetBundle bundle)
    {
        if (!_assetBundles.TryGetValue(bundleName, out bundle))
        {
            WLog.Error($"Asset Bundle not loaded: {bundleName}");
            return false;
        }

        return true;
    }
}

public class AssetLoadRequest
{
    public bool IsFinished;
    public bool Failed;
}

public struct AssetBundle()
{
    public Dictionary<string, Texture> Textures = new ();
    public Dictionary<string, ShaderCompiler.ShaderProgram> VertexPrograms = new ();
    public Dictionary<string, ShaderCompiler.ShaderProgram> FragmentPrograms = new ();
    public Dictionary<string, Shader> Shaders = new ();
    public Dictionary<string, ComputeShader> ComputeShaders = new ();

    public bool TryGetTexture(string textureName, [NotNullWhen(true)] out Texture? texture)
    {
        if (!Textures.TryGetValue(textureName, out texture))
        {
            WLog.Error($"Texture by name \"{textureName}\" not in bundle");
            return false;
        }

        return true;
    }
    
    public bool TryGetShader(string shaderName, [NotNullWhen(true)] out Shader? shader)
    {
        if (!Shaders.TryGetValue(shaderName, out shader))
        {
            WLog.Error($"Shader by name \"{shaderName}\" not in bundle");
            return false;
        }

        return true;
    }
    
    public bool TryGetComputeShader(string shaderName, [NotNullWhen(true)] out ComputeShader? shader)
    {
        if (!ComputeShaders.TryGetValue(shaderName, out shader))
        {
            WLog.Error($"Shader by name \"{shaderName}\" not in bundle");
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        foreach (var texture in Textures.Values)
        {
            texture.Dispose();
        }
            
        foreach (var shader in Shaders.Values)
        {
            shader.Dispose();
        }
    }
}

public struct ShaderInfo : IDeserializable<ShaderInfo>
{
    public string? VertexName;
    public string? FragmentName;
    public PipelineSettings PipelineSettings;
    
    public static bool TryDeserialize(ref YamlParser parser, out ShaderInfo shaderInfo)
    {
        shaderInfo = new();
        
        if (parser.CurrentEventType != ParseEventType.MappingStart)
        {
            WLog.Error("ShaderInfo deserializer didn't start with a mapping.");
            return false;
        }

        parser.Read();

        while (parser.CurrentEventType != ParseEventType.MappingEnd)
        {
            if (!parser.TryReadScalarAsString(out var label))
            {
                return false;
            }
            
            switch (label)
            {
                case "Vertex":
                    if (!parser.TryReadScalarAsString(out var vertexName))
                    {
                        return false;
                    }
                    shaderInfo.VertexName = vertexName;
                    break;
                
                case "Fragment":
                    if (!parser.TryReadScalarAsString(out var fragmentName))
                    {
                        return false;
                    }
                    shaderInfo.FragmentName = fragmentName;
                    break;
                
                case "Pipeline":
                    if (!PipelineSettings.TryDeserialize(ref parser, out var pipelineSettings))
                    {
                        return false;
                    }
                    shaderInfo.PipelineSettings = pipelineSettings;
                    break;
                default:
                    WLog.Error($"Shader Invalid Parameter: {label}");
                    return false;
            }
        }
        
        // Read the mapping end.
        parser.Read();

        return true;
    }
}