using System.Collections.Concurrent;
using StbImageSharp;

namespace WaffleEngine
{
    public static partial class AssetLoader
    {
        private static bool _initialized = false;

        private static Queue<Task> _tasks = new();

        private interface IAssetResult
        {
            public void Complete();
        }

        private static ConcurrentQueue<IAssetResult> _task_results = new();

        public static bool IsAsyncFinished => _tasks.Count == 0 && _task_results.IsEmpty;

        public static FileSystemWatcher _watcher = new();

        public static void LoadFolderAsync(string folder)
        {
            Task new_task = Task.Factory.StartNew(() =>
            {
                LoadAllTexturesFromFolderAsync(folder);
                LoadAllShadersFromFolderAsync(folder);
            });

            _tasks.Enqueue(new_task);
        }

        public static void Update()
        {
            if (!_initialized)
                InitFileWatcher();

            if (_tasks.Count != 0)
            {
                if (_tasks.Peek().IsCompleted)
                    _tasks.Dequeue();
            }

            if (_task_results.IsEmpty)
                return;

            _task_results.TryDequeue(out var result);

            result?.Complete();
        }

        private static void InitFileWatcher()
        {
            _initialized = true;

#if DEBUG 
            _watcher.Path = $"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}";
#else
            _watcher.Path = $"{Environment.CurrentDirectory}/assets";
#endif

            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.IncludeSubdirectories = true;
            _watcher.Filters.Add("*.wshader");
            _watcher.Filters.Add("*.png");
            _watcher.Filters.Add("*.jpeg");
            _watcher.Filters.Add("*.jpg");

            _watcher.Changed += FileUpdated;

            _watcher.EnableRaisingEvents = true;
        }

        private static void FileUpdated(object source, FileSystemEventArgs e)
        {
            string folder = Path.GetFileName(Path.GetDirectoryName(e.FullPath));
            string file = Path.GetFileNameWithoutExtension(e.FullPath);
            string extension = Path.GetExtension(e.FullPath);

            Log.Info("File Updated: folder={0} file={1} extension={2}", folder, file, extension);
        }

        /// <summary>
        /// Loads all textures from folder in textures folder.
        /// </summary>
        /// <param name="folder">The folder you want to load textures from in the textures folder.</param>
        /// <returns>whether it was successful.</returns>
        public static bool LoadFolder(string folder)
        {
            LoadAllTexturesFromFolder(folder);
            LoadAllShadersFromFolder(folder);

            return true;
        }

        public static void UnloadFolder(string folder)
        {
            UnloadTextureFolder(folder);
            UnloadShaderFolder(folder);
        }

        public static void UnloadAll()
        {
            UnloadAllTextures();
            UnloadAllShaders();
        }
    }
}
