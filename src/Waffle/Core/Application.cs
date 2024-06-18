using Raylib_cs;
using WaffleEngine.src.Waffle.Core;

namespace WaffleEngine
{
    public class Application
    {
        private static Application? Instance;

        private Scene _CurrentScene;

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

            Log.Info("Closing Application.");

            AssetLoader.UnloadAllTextures();
        }

        public void Init()
        {
            // Init Window and stuff

            unsafe
            {
                Log.Info("Setting Raylib Logging to internal logging tools.");
                Raylib.SetTraceLogCallback(&Log.RaylibLog);
            }

            Window.Init();

            AssetLoader.LoadFolder("core");
        }

        public void Run()
        {
            while (!Window.ShouldClose())
            {
                // Update Game Logic
                _CurrentScene.InternalUpdate();

                Raylib.ClearBackground(Color.Black);

                Raylib.BeginDrawing();

                // Render Stuff
                _CurrentScene.Render();

                Raylib.EndDrawing();
            }

            Exit();
        }
    }
}
