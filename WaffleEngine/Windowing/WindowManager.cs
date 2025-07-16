using System.Diagnostics.CodeAnalysis;

namespace WaffleEngine;

public static class WindowManager
{
    private static Dictionary<string, Window> _windows = new ();
    private static Dictionary<uint, string> _windowIdToWindowHandleDict = new();

    public static int WindowCount => _windows.Count;

    internal static void UpdateWindowInput()
    {
        foreach (var window in _windows.Values)
        {
            window.WindowInput.Update();
        }
    }
    
    public static bool TryOpenWindow(string windowName, string windowHandle, int width, int height, [NotNullWhen(true)] out Window? window)
    {
        window = null;
        
        if (_windows.TryGetValue(windowHandle, out window))
        {
            window.Focus();
            
            WLog.Info($"Window already existed focused instead [name: {windowName}, handle: {windowHandle}]");
            return true;
        }
        
        if (!WindowSdl.TryCreate(windowName, 800, 600, out window))
            return false;
        
        _windows.Add(windowHandle, window!);
        _windowIdToWindowHandleDict.Add(window!.GetId(), windowHandle);

        WLog.Info($"Window created [name: {windowName}, handle: {windowHandle}]");
        
        return true;
    }

    public static bool TryGetWindow(string windowName, [NotNullWhen(true)] out Window? window)
    {
        if (_windows.TryGetValue(windowName, out window))
        {
            return true;
        }
        
        return false;
    }

    public static bool TryGetWindowWithId(uint windowId, out Window? window)
    {
        if (!_windowIdToWindowHandleDict.TryGetValue(windowId, out string? windowName))
        {
            //WLog.Error($"Tried to get [Window ID: {windowId}] but it doesn't exist.", "Window Manager");
            window = null;
            return false;
        }

        return TryGetWindow(windowName, out window);
    }

    public static string? TryGetWindowHandle(uint windowId)
    {
        if (!_windowIdToWindowHandleDict.TryGetValue(windowId, out string? windowName))
        {
            WLog.Error($"Tried to get [Window ID: {windowId}] but it doesn't exist.", "Window Manager");
            return null;
        }

        return windowName;
    }

    public static void CloseWindow(string windowName)
    {
        if (!_windows.TryGetValue(windowName, out Window? window))
        {
            WLog.Error($"Tried to close {windowName} but it doesn't exist.", "Window Manager");
            return;
        }

        _windowIdToWindowHandleDict.Remove(window.GetId());
        _windows.Remove(windowName);
        window.Dispose();
    }

    public static void CloseAllWindows()
    {
        foreach (var window in _windows.Values)
        {
            window.Dispose();
        }
        
        _windows.Clear();
        _windowIdToWindowHandleDict.Clear();
        
        WLog.Info("All Windows closed.");
    }
}