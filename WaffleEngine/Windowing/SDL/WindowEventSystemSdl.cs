using SDL3;

namespace WaffleEngine;

internal sealed class WindowEventSystemSdl : IWindowEventSystem
{
    public void Process()
    {
        while (SDL.PollEvent(out SDL.Event sdlEvent))
        {
            switch ((SDL.EventType)sdlEvent.Type)
            {
                case SDL.EventType.KeyDown or SDL.EventType.KeyUp:
                    ProcessKeyEvent(ref sdlEvent.Key);
                    break;
                
                case SDL.EventType.MouseMotion:
                    ProcessMouseMotion(ref sdlEvent.Motion);
                    break;
                
                case SDL.EventType.MouseWheel:
                    ProcessMouseWheel(ref sdlEvent.Wheel);
                    break;
                
                case SDL.EventType.MouseButtonDown or SDL.EventType.MouseButtonUp:
                    ProcessMouseButton(ref sdlEvent.Button);
                    break;
                
                case SDL.EventType.WindowCloseRequested:
                    string? windowHandle = WindowManager.TryGetWindowHandle(sdlEvent.Window.WindowID);

                    if (windowHandle == null)
                        break;
                        
                    WindowManager.CloseWindow(windowHandle);

                    if (WindowManager.WindowCount == 0)
                        Application.Exit();
                    
                    break;
                
                case SDL.EventType.Quit:
                    Application.Exit();
                    break;
            }
        }
    }
    
    private void ProcessKeyEvent(ref SDL.KeyboardEvent sdlEvent)
    {
        if (Input.GlobalInputHandler.TriggerKeyEvent((Keycode)sdlEvent.Key, sdlEvent.Down))
            return;

        // Trigger Window Events
        if (WindowManager.TryGetWindowWithId(sdlEvent.WindowID, out var window))
            window!.WindowInput.TriggerKeyEvent((Keycode)sdlEvent.Key, sdlEvent.Down);
    }

    private void ProcessMouseMotion(ref SDL.MouseMotionEvent sdlEvent)
    {
        Input.GlobalInputHandler.UpdateMouseMotion(sdlEvent.X, sdlEvent.Y, sdlEvent.XRel, sdlEvent.YRel);

        if (WindowManager.TryGetWindowWithId(sdlEvent.WindowID, out var window))
        {
            float density = window.GetDensity();
            
            window.WindowInput.UpdateMouseMotion(
                sdlEvent.X * density, 
                sdlEvent.Y * density, 
                sdlEvent.XRel * density, 
                sdlEvent.YRel * density);
        }
    }
    
    private void ProcessMouseWheel(ref SDL.MouseWheelEvent sdlEvent)
    {
        Input.GlobalInputHandler.SetMouseWheelTicksDelta(sdlEvent.IntegerY);
        WLog.Info($"{sdlEvent.X} {sdlEvent.Y} {sdlEvent.Direction} {sdlEvent.IntegerX} {sdlEvent.IntegerY}");

        if (WindowManager.TryGetWindowWithId(sdlEvent.WindowID, out var window))
        {
            window.WindowInput.SetMouseWheelTicksDelta(sdlEvent.IntegerY);
        }
    }

    private void ProcessMouseButton(ref SDL.MouseButtonEvent sdlEvent)
    {
        if (sdlEvent.Button == 1)
        {
            Input.GlobalInputHandler.SetMouseLeftDown(sdlEvent.Down);
            
            if (WindowManager.TryGetWindowWithId(sdlEvent.WindowID, out var window))
                window.WindowInput.SetMouseLeftDown(sdlEvent.Down);
        }
        else if (sdlEvent.Button == 3)
        {
            Input.GlobalInputHandler.SetMouseRightDown(sdlEvent.Down);
            
            if (WindowManager.TryGetWindowWithId(sdlEvent.WindowID, out var window))
                window.WindowInput.SetMouseRightDown(sdlEvent.Down);
        }
    }
}