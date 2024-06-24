using Raylib_cs;

namespace WaffleEngine
{
    public class Application
    {
        private static Application _instance;

        public static void StartingScene(Scene scene)
        {
            if (_instance != null)
                return;

            _instance = new Application();

            _instance.Init();

            SceneManager.ChangeScene(new InitScene(scene));

            _instance.Run();
        }

        public void Exit()
        {
            // Run Exit code.

            Log.Info("Closing Application.");

            AssetLoader.UnloadAllTextures();

            SceneManager.CurrentScene.End();
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
        }

        public void Run()
        {
            while (!Window.ShouldClose())
            {
                SceneManager.Update();

                AssetLoader.UpdateQueue();

                // Update Game Logic
                SceneManager.CurrentScene.InternalUpdate();

                Raylib.ClearBackground(Color.DarkGray);

                Raylib.BeginDrawing();

                // Render Stuff
                SceneManager.CurrentScene.Render();

                Raylib.EndDrawing();
            }

            Exit();
        }
    }
}
