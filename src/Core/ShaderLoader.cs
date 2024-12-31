using StbImageSharp;

namespace WaffleEngine;

public static partial class AssetLoader
{
    /// <summary>
    /// Dictionary of Loaded Textures indexed by (Folder, TextureName)
    /// </summary>
    private static Dictionary<(string, string), Shader> _shader_dictionary = new();

    private struct ShaderResult : IAssetResult
    {
        public string Folder;
        public string File;
        public string VertexSource;
        public string FragmentSource;

        public void Complete()
        {
            _shader_dictionary.Add((Folder, File), new Shader(VertexSource, FragmentSource));
        }
    }

    private static void LoadAllShadersFromFolderAsync(string folder)
    {
        string full_path = $"{Environment.CurrentDirectory}/assets/shaders/{folder}";

        if (!Directory.Exists(full_path))
        {
            Log.Warning("Tried to load shaders from \"{0}\" but it doesn't exist.", full_path);
            return;
        }

        var files = Directory.EnumerateFiles(full_path, "*.wshader", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string source = File.ReadAllText(file);

            string[] sources = source.Split("~frag");

            if (sources.Length != 2)
            {
                Log.Error("Invalid Shader {0}: Shader has {1} sources", Path.GetFileNameWithoutExtension(file), sources.Length);
                return;
            }

            _task_results.Enqueue(new ShaderResult
            {
                Folder = folder,
                File = Path.GetFileNameWithoutExtension(file),
                VertexSource = sources[0],
                FragmentSource = sources[1],
            });
        }
    }

    private static void LoadAllShadersFromFolder(string folder)
    {
        string full_path = $"{Environment.CurrentDirectory}/assets/shaders/{folder}";

        if (!Directory.Exists(full_path))
        {
            Log.Warning("Tried to load shaders from \"{0}\" but it doesn't exist.", full_path);
            return;
        }

        var files = Directory.EnumerateFiles(full_path, "*.wshader", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string source = File.ReadAllText(file);

            string[] sources = source.Split("~frag");

            if (sources.Length != 2)
            {
                Log.Error("Invalid Shader {0}: Shader has {1} sources", Path.GetFileNameWithoutExtension(file), sources.Length);
                return;
            }

            _shader_dictionary.Add((folder, Path.GetFileNameWithoutExtension(file)), new Shader(sources[0], sources[1]));
        }
    }

    private static void UnloadShaderFolder(string folder)
    {
        foreach (((string key_folder, string key_file), Shader shader) in _shader_dictionary)
        {
            if (folder != key_folder)
                continue;

            shader.Dispose();
            _shader_dictionary.Remove((key_folder, key_file));
        }
    }

    private static void UnloadAllShaders()
    {
        foreach (var shader in _shader_dictionary.Values)
        {
            shader.Dispose();
        }

        _shader_dictionary.Clear();
    }

    public static Shader GetShader(string folder, string shader)
    {
        if (_shader_dictionary.TryGetValue((folder, shader), out Shader loaded_shader))
            return loaded_shader;

        Log.Fatal("Couldn't load shader {0} from {1}. Currently loaded shaders are:", shader, folder);

        foreach ((string key_folder, string key_file) in _shader_dictionary.Keys)
        {
            Log.Fatal(" - {0}: {1}", key_folder, key_file);
        }

        throw new Exception();
    }
}