using SDL3;

namespace WaffleEngine;

internal sealed class WindowEventSystemSdl : IWindowEventSystem
{
    public void Process()
    {
        while (SDL.PollEvent(out SDL.Event sdlEvent))
        {
            switch (sdlEvent.Type)
            {
                case (uint)SDL.EventType.KeyDown or (uint)SDL.EventType.KeyUp:
                    ProcessKeyEvent(ref sdlEvent);
                    break;
                
                case (uint)SDL.EventType.WindowCloseRequested:
                        
                    string? windowHandle = WindowManager.TryGetWindowHandle(sdlEvent.Window.WindowID);

                    if (windowHandle == null)
                        break;
                        
                    WindowManager.CloseWindow(windowHandle);

                    if (WindowManager.WindowCount == 0)
                        Application.Exit();
                    
                    break;
                
                case (uint)SDL.EventType.Quit:
                    Application.Exit();
                    break;
            }
        }
    }
    
    private void ProcessKeyEvent(ref SDL.Event sdlEvent)
    {
        if (Input.GlobalInputHandler.TriggerKeyEvent((Keycode)sdlEvent.Key.Key, sdlEvent.Key.Down))
            return;

        // Trigger Window Events
        if (WindowManager.TryGetWindowWithId(sdlEvent.Window.WindowID, out var window))
            window!.WindowInput.TriggerKeyEvent((Keycode)sdlEvent.Key.Key, sdlEvent.Key.Down);
    }
}