using SDL3;

namespace WaffleEngine;

public struct MouseData
{
    public Vector2 Position;
    public Vector2 Delta;
    public int MouseWheelTicksDelta;
    public bool IsLeftPressed;
    public bool IsLeftDown;
    public bool IsRightPressed;
    public bool IsRightDown;
    
    public enum SystemCursor {
        Default,
        Pointer,
        Text,
        Wait,
        Progress,
        ResizeHorizontal,
        ResizeVertical,
        Move,
    };

    private static Dictionary<SDL.SystemCursor, IntPtr> _cursors = new Dictionary<SDL.SystemCursor, IntPtr>();

    public void SetCursor(SystemCursor cursor)
    {
        SDL.SystemCursor systemCursor = cursor switch
        {
            SystemCursor.Default => SDL.SystemCursor.Default,
            SystemCursor.Pointer => SDL.SystemCursor.Pointer,
            SystemCursor.Text => SDL.SystemCursor.Text,
            SystemCursor.Wait => SDL.SystemCursor.Wait,
            SystemCursor.Progress => SDL.SystemCursor.Progress,
            SystemCursor.ResizeHorizontal => SDL.SystemCursor.EWResize,
            SystemCursor.ResizeVertical => SDL.SystemCursor.NWResize,
            SystemCursor.Move => SDL.SystemCursor.Move,
        };

        if (!_cursors.TryGetValue(systemCursor, out IntPtr cursor_ptr))
        {
            cursor_ptr = SDL.CreateSystemCursor(systemCursor);

            if (cursor_ptr == IntPtr.Zero)
            {
                Log.Error($"Failed to create a cursor from system cursor: {Enum.GetName(systemCursor)}");
                throw new NullReferenceException();
            }
            
            _cursors.Add(systemCursor, cursor_ptr);
        }


        SDL.SetCursor(cursor_ptr);
    }
}