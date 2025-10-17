using System.Diagnostics;
using WaffleEngine.Rendering;
using WaffleEngine.Text;

namespace WaffleEngine;

public static class Application
{
    private static bool _isRunning;
    private static readonly IWindowEventSystem AppEventSystem = new WindowEventSystemSdl();
    public static void Run(IScene startScene)
    {
        if (_isRunning)
            return;
        
        if (!Init())
            return;

        SceneManager.SetScene(startScene, "_start_scene");
        
        MainLoop();
        
        CleanUp();
    }
    
    private static bool Init()
    {
        if (!Device.Init())
            return false;

        if (!ShaderCompiler.Init())
            return false;
        
        FontLoader.Init();
        
        Assets.StartAssetThread();

        if (!Assets.TryLoadAssetBundle("builtin"))
            return false;
        
        WLog.Info("Application Initialised");
        
        return true;
    }

    private static void MainLoop()
    {
        WLog.Info("Started Application Main Loop");
        
        _isRunning = true;
        
        Stopwatch timer = Stopwatch.StartNew();
        
        while (_isRunning)
        {
            AppEventSystem.Process();

            if (!_isRunning)
                break;
            
            SceneManager.UpdateScenes();
            
            Input.GlobalInputHandler.Update();
            
            timer.Restart();
        }
    }

    private static void CleanUp()
    {
        SceneManager.CleanUp();
        Assets.Dispose();
        WindowManager.CloseAllWindows();
        WLog.Info("Clean up finished.");
    }

    public static void Exit()
    {
        if (!_isRunning)
            return;
        
        FontLoader.Dispose();
        
        _isRunning = false;
        WLog.Info("Application Exit Requested.");
    }
}