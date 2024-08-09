using System.Collections.Concurrent;
using StbImageSharp;

namespace WaffleEngine
{
    public static class AssetLoader
    {
        /// <summary>
        /// Dictionary of Loaded Textures indexed by (Folder, TextureName)
        /// </summary>
        private static Dictionary<(string, string), Texture> _texture_dictionary = new();


        private static int _async_loading;
        private static ConcurrentQueue<(string, string, ImageResult)> _waiting_for_load = new();

        public static bool IsAsyncFinished => _async_loading == 0 && _waiting_for_load.IsEmpty;

        public static void LoadFolderAsync(string folder)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            
            Interlocked.Increment(ref _async_loading);

            string full_path = $"{Environment.CurrentDirectory}/assets/textures/{folder}";

            if (!Directory.Exists(full_path))
            {
                Log.Error("Tried to load files from \"{0}\" but it doesn't exist.", full_path);
                return;
            }

            Task.Factory.StartNew(() => {

                var files = Directory.EnumerateFiles(full_path, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".png") || s.EndsWith(".jpg"));

                foreach (var file in files)
                {
                    ImageResult image = ImageResult.FromStream(File.OpenRead(file), ColorComponents.RedGreenBlueAlpha);

                    _waiting_for_load.Enqueue((folder, Path.GetFileNameWithoutExtension(file), image));
                }

                Interlocked.Decrement(ref _async_loading);
            });
        }

        public static void UpdateQueue()
        {
            if (_waiting_for_load.IsEmpty)
                return;

            _waiting_for_load.TryDequeue(out (string, string, ImageResult) result);

            _texture_dictionary.Add((result.Item1, result.Item2), new Texture(ref result.Item3));
        }

        public static bool LoadFile(string folder, string file)
        {
            string full_path = $"{Environment.CurrentDirectory}/assets/textures/{folder}/{file}";

            if (!File.Exists(full_path))
            {
                Log.Error("Tried to load file from \"{0}\" but it doesn't exist.", full_path);
                return false;
            }

            _texture_dictionary.Add((folder, Path.GetFileNameWithoutExtension(file)), new Texture(file));

            return true;
        }

        /// <summary>
        /// Loads all textures from folder in textures folder.
        /// </summary>
        /// <param name="folder">The folder you want to load textures from in the textures folder.</param>
        /// <returns>whether it was successful.</returns>
        public static bool LoadFolder(string folder)
        {
            string full_path = $"{Environment.CurrentDirectory}/assets/textures/{folder}";

            if (!Directory.Exists(full_path))
            {
                Log.Error("Tried to load files from \"{0}\" but it doesn't exist.", full_path);
                return false;
            }

            var files = Directory.EnumerateFiles(full_path, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".jpeg") || s.EndsWith(".png"));

            foreach (var file in files)
            {
                _texture_dictionary.Add((folder, Path.GetFileNameWithoutExtension(file)), new Texture(file));
            }

            return true;
        }

        public static void UnloadFile(string folder, string file)
        {
            if (_texture_dictionary.TryGetValue((folder, file), out Texture texture))
            {
                texture.Unload();
                _texture_dictionary.Remove((folder, file));
                return;
            }

            Log.Error("Tried to unload file from {0} - {1} but it doesn't exist.", folder, file);
        }

        public static void UnloadFolder(string folder)
        {
            foreach (((string key_folder, string key_file), Texture texture) in _texture_dictionary)
            {
                if (folder != key_folder)
                    continue;

                texture.Unload();
                _texture_dictionary.Remove((key_folder, key_file));
            }
        }

        public static void UnloadAllTextures()
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
}
