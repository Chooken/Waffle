
namespace WaffleEngine
{
    public static class SceneManager
    {
        private static Scene _current_scene;
        public static Scene CurrentScene => _current_scene;

        private static Scene? _requested_scene;

        public static void ChangeScene(Scene scene)
        {
            _requested_scene = scene;
        }

        public static void Update()
        {
            if (_requested_scene == null)
                return;

            if (_current_scene != null) 
                _current_scene.Deinit();

            Log.Info("Changing Scene To {0}", _requested_scene.GetType().Name);

            _current_scene = _requested_scene;

            _current_scene.Init();

            _requested_scene = null;
        }
    }
}
