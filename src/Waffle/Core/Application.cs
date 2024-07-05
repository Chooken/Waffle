using Raylib_cs;

namespace WaffleEngine
{
    public static class Application
    {
        public static void StartingScene(Scene scene)
        {
            Init();

            SceneManager.ChangeScene(new InitScene(scene));

            Run();
        }

        public static void Exit()
        {
            // Run Exit code.

            Log.Info("Closing Application.");

            SceneManager.CurrentScene.Deinit();

            AssetLoader.UnloadAllTextures();
        }

        public static void Init()
        {
            // Init Window and stuff

            unsafe
            {
                Log.Info("Setting Raylib Logging to internal logging tools.");
                Raylib.SetTraceLogCallback(&Log.RaylibLog);
            }

            Window.Init();
        }

        public static void Run()
        {
            while (!Window.ShouldClose())
            {
                SceneManager.Update();

                AssetLoader.UpdateQueue();

                //SceneManager.CurrentScene.Update();

                Raylib.ClearBackground(Color.DarkGray);

                Raylib.BeginDrawing();

                // Run Ecs
                SceneManager.CurrentScene.Update();

                Raylib.DrawFPS(10, 10);

                Raylib.EndDrawing();
            }

            Exit();
        }
    }
}
