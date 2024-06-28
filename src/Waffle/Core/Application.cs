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

            SceneManager.CurrentScene.Deinit();

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
        }

        public void Run()
        {
            while (!Window.ShouldClose())
            {
                SceneManager.Update();

                AssetLoader.UpdateQueue();

                Raylib.ClearBackground(Color.DarkGray);

                Raylib.BeginDrawing();

                // Run Ecs
                SceneManager.CurrentScene.World.Progress();

                Raylib.EndDrawing();
            }

            Exit();
        }
    }
}
