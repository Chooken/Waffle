using Raylib_cs;

namespace WaffleEngine
{
    public class Application
    {
        private static Application? Instance;

        private Scene _CurrentScene;
        private Window _Window;

        public Application(Scene scene)
        {
            _CurrentScene = scene;
        }

        public static void StartingScene(Scene scene)
        {
            if (Instance != null)
                return;

            // Creates new Application
            // 
            // Will eventually add a World to the app where
            // Waffle can call Game World code.

            Instance = new Application(scene);

            Instance.Init();

            Instance.Run();
        }

        public void Exit()
        {
            // Run Exit code.
        }

        public void Init()
        {
            // Init Window and stuff

            _Window = new Window();
        }

        public void Run()
        {
            while (!_Window.ShouldClose())
            {
                // Update Game Logic
                _CurrentScene.InternalUpdate();

                Raylib.ClearBackground(Color.Black);

                Raylib.BeginDrawing();

                // Render Stuff
                _CurrentScene.Render();

                Raylib.EndDrawing();
            }
        }
    }
}
