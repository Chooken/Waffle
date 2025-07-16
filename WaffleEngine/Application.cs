
using System.Diagnostics;
using SDL3;
using WaffleEngine.Rendering;

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
        
        // if (WindowManager.OpenWindow("Waffle Engine - Second", "waffle_engine_second", 800, 600) == null)
        //     return false;
        
        // Tray.Create("Waffle Engine");
        // Tray.AddButton("Open Window 2", (ptr, entry) => WindowManager.OpenWindow("Waffle Engine - Second", "waffle_engine_second", 800, 600));
        // Tray.AddButton("Open Window 3", (ptr, entry) => WindowManager.OpenWindow("Waffle Engine - Third", "waffle_engine_third", 800, 600));
        //
        
        WLog.Info("Application Initialised");
        
        return true;
    }

    private static void MainLoop()
    {
        WLog.Info("Started Application Main Loop");
        
        ShaderManager.CompileAllShaders();
        
        _isRunning = true;
        
        while (_isRunning)
        {
            AppEventSystem.Process();

            if (!_isRunning)
                break;
            
            SceneManager.RunActiveSceneQueries();
            
            Input.GlobalInputHandler.Update();
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
        
        _isRunning = false;
        WLog.Info("Application Exit Requested.");
    }
}