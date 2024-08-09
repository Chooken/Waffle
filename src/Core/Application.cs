using System.Diagnostics;
using OpenTK.Core;
using OpenTK.Graphics.OpenGL;

namespace WaffleEngine
{
    public static class Application
    {
        private static Stopwatch _update_timer = new Stopwatch();
        
        public static void StartingScene(Scene scene)
        {
            Init();

            SceneManager.ChangeScene(new InitScene(scene));
            //SceneManager.ChangeScene(scene);

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

            Window.Init();
            
            NoAlloc.Init();
            
            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Log.Error("OpenGL Error: {0}", err);
            }
        }

        public static void Run()
        {
            DateTime application_start_time = DateTime.Now;

            _update_timer.Start();
            while (!Window.ShouldClose())
            {
                double elapsed_time = _update_timer.Elapsed.TotalSeconds;

                if (elapsed_time > Window.UpdateFrequency)
                {
                    Time.UpdateTimes();
                    Time.DeltaTime = elapsed_time;
                    Time.AddLastFrameTime(elapsed_time);
                    
                    _update_timer.Restart();
                    
                    Window.ProcessEvents();
                    
                    SceneManager.Update();

                    AssetLoader.UpdateQueue();

                    Window.StartFrame();

                    // Run Update
                    SceneManager.CurrentScene.Update();

                    Window.EndFrame();
                }
                
                // The time we have left to the next update.
                double timeToNextUpdate = Window.UpdateFrequency - _update_timer.Elapsed.TotalSeconds;

                if (timeToNextUpdate > 0)
                {
                    Utils.AccurateSleep(timeToNextUpdate, 8);
                }
            }

            Exit();
        }
    }
}
