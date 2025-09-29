
using System.Diagnostics;
using SDL3;
using WaffleEngine.Rendering;
using WaffleEngine.Text;

namespace WaffleEngine;

public static class Application
{
    private static bool _isRunning;
    private static readonly IWindowEventSystem AppEventSystem = new WindowEventSystemSdl();
    public static void Run()
    {
        if (_isRunning)
            return;
        
        if (!Init())
            return;
        
        MainLoop();
        
        CleanUp();
    }
    
    private static bool Init()
    {
        if (!Device.Init())
            return false;
        
        ShaderManager.Init();
        FontLoader.Init();
        
        WLog.Info("Application Initialised");
        
        return true;
    }

    private static void MainLoop()
    {
        WLog.Info("Started Application Main Loop");
        
        ShaderManager.CompileAllShaders();
        
        _isRunning = true;
        
        Stopwatch timer = Stopwatch.StartNew();
        
        while (_isRunning)
        {
            AppEventSystem.Process();

            if (!_isRunning)
                break;
            
            SceneManager.RunActiveSceneQueries();
            
            Input.GlobalInputHandler.Update();
            
            timer.Restart();
        }
    }

    private static void CleanUp()
    {
        SceneManager.CleanUp();
        ShaderManager.CleanUp();
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