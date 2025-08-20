using System.Runtime.InteropServices;
using SDL3;

namespace WaffleEngine;

public static class Tray
{
    private static IntPtr _tray;
    private static IntPtr _trayMenu;
    private static int _currentPos = 0;
    
    public static void Create(string name)
    {
        _tray = SDL.CreateTray(IntPtr.Zero, name);
        _trayMenu = SDL.CreateTrayMenu(_tray);
    }

    public static void AddButton(string name, SDL.TrayCallback callback)
    {
        var openWindowTrayEntry = SDL.InsertTrayEntryAt(_trayMenu, _currentPos, name, SDL.TrayEntryFlags.Button);
        SDL.SetTrayEntryCallback(openWindowTrayEntry, callback, IntPtr.Zero);

        _currentPos++;
    }
}