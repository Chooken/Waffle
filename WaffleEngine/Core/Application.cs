using System.Diagnostics;
using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Text;

namespace WaffleEngine;

public static class Application
{
    private static ulong _updateRate = 0;
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
        _isRunning = true;
        
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

        ulong ns_timer = SDL.GetTicksNS();
        ulong elapsed;
        
        while (_isRunning)
        {
            AppEventSystem.Process();

            if (!_isRunning)
                break;
            
            SceneManager.UpdateScenes();
            
            Input.GlobalInputHandler.Update();
            WindowManager.UpdateWindowInput();

            elapsed = SDL.GetTicksNS() - ns_timer;

            if (elapsed < _updateRate)
            {
                SDL.DelayPrecise(_updateRate - elapsed);
            }
            
            ns_timer = SDL.GetTicksNS();
        }
    }

    public static void SetUpdateRate(float updates_per_second)
    {
        float seconds_per_update = 1 / updates_per_second;
        _updateRate = (ulong)(seconds_per_update * 1_000_000_000);
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