using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

            if (!Directory.Exists(request.Item1))
            {
                WLog.Warning($"Tried to load an asset bundle which doesn't exist: {request.Item1}");
                request.Item2.Failed = true;
                request.Item2.IsFinished = true;
                continue;
            }

            var textures = Directory.EnumerateFiles(request.Item1, "*.png", SearchOption.AllDirectories);
            var shaders = Directory.EnumerateFiles(request.Item1, "*.hlsl", SearchOption.AllDirectories);

            AssetBundle bundle = new AssetBundle();
            
            foreach (var texturePath in textures)
            {
                string name = Path.GetFileNameWithoutExtension(texturePath);

                if (bundle.Textures.ContainsKey(name))
                {
                    WLog.Warning($"Bundle \"{request.Item1}\" contains duplicate texture with name: {name}");
                    continue;
                }
                
                Texture texture = new Texture(texturePath);
                bundle.Textures.Add(
                    Path.GetFileNameWithoutExtension(texturePath), 
                    texture);
            }
            
            foreach (var shaderPath in shaders)
            {
                string name = Path.GetFileNameWithoutExtension(shaderPath);

                if (bundle.Shaders.ContainsKey(name))
                {
                    WLog.Warning($"Bundle \"{request.Item1}\" contains duplicate texture with name: {name}");
                    continue;
                }
                
                Shader shader = ShaderCompiler.CompileShader(shaderPath);
                bundle.Shaders.Add(
                    name,
                    shader);
            }

            _assetBundles.TryAdd(request.Item1, bundle);

            request.Item2.Failed = false;
            request.Item2.IsFinished = true;
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
    public Dictionary<string, Shader> Shaders = new ();

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