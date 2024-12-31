using StbImageSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WaffleEngine;

public static partial class AssetLoader
{
    /// <summary>
    /// Dictionary of Loaded Textures indexed by (Folder, TextureName)
    /// </summary>
    private static Dictionary<(string, string), Texture> _texture_dictionary = new();

    private struct TextureResult : IAssetResult
    {
        public string folder;
        public string file;
        public ImageResult data;

        public void Complete()
        {
            _texture_dictionary.Add((folder, file), new Texture(ref data));
        }
    }

    private static void LoadAllTexturesFromFolderAsync(string folder)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);

        string full_path = $"{Environment.CurrentDirectory}/assets/textures/{folder}";

        if (!Directory.Exists(full_path))
        {
            Log.Warning("Tried to load textures from \"{0}\" but it doesn't exist.", full_path);
            return;
        }

        var files = Directory.EnumerateFiles(full_path, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".png") || s.EndsWith(".jpg"));

        foreach (var file in files)
        {
            ImageResult image = ImageResult.FromStream(File.OpenRead(file), ColorComponents.RedGreenBlueAlpha);

            _task_results.Enqueue(new TextureResult
            {
                folder = folder,
                file = Path.GetFileNameWithoutExtension(file),
                data = image
            });
        }
    }

    private static void LoadAllTexturesFromFolder(string folder)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);

        string full_path = $"{Environment.CurrentDirectory}/assets/textures/{folder}";

        if (!Directory.Exists(full_path))
        {
            Log.Warning("Tried to load files from \"{0}\" but it doesn't exist.", full_path);
            return;
        }

        var files = Directory.EnumerateFiles(full_path, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".png") || s.EndsWith(".jpg"));

        foreach (var file in files)
        {
            _texture_dictionary.Add((folder, Path.GetFileNameWithoutExtension(file)), new Texture(file));
        }
    }

    private static void UnloadTextureFolder(string folder)
    {
        foreach (((string key_folder, string key_file), Texture texture) in _texture_dictionary)
        {
            if (folder != key_folder)
                continue;

            texture.Unload();
            _texture_dictionary.Remove((key_folder, key_file));
        }
    }

    private static void UnloadAllTextures()
    {
        foreach (var texture in _texture_dictionary.Values)
        {
            texture.Unload();
        }

        _texture_dictionary.Clear();
    }

    public static Texture GetTexture(string folder, string texture)
    {
        if (_texture_dictionary.TryGetValue((folder, texture), out Texture loaded_texture))
            return loaded_texture;

        Log.Fatal("Couldn't load texture {0} from {1}. Currently loaded textures are:", texture, folder);

        foreach ((string key_folder, string key_file) in _texture_dictionary.Keys)
        {
            Log.Fatal(" - {0}: {1}", key_folder, key_file);
        }

        throw new Exception();
    }
}