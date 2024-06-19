using Raylib_cs;

namespace WaffleEngine
{
    public class Application
    {
        private static Application? Instance;

        public static void StartingScene(Scene scene)
        {
            if (Instance != null)
                return;

            // Creates new Application
            // 
            // Will eventually add a World to the app where
            // Waffle can call Game World code.

            Instance = new Application();

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

            SceneManager.ChangeScene(new InitScene());
        }

        public void Run()
        {
            while (!Window.ShouldClose())
            {
                SceneManager.Update();

                AssetLoader.UpdateQueue();

                // Update Game Logic
                SceneManager.CurrentScene.InternalUpdate();

                Raylib.ClearBackground(Color.White);

                Raylib.BeginDrawing();

                // Render Stuff
                SceneManager.CurrentScene.Render();

                Raylib.EndDrawing();
            }

            Exit();
        }
    }
}
