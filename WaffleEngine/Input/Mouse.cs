using SDL3;

namespace WaffleEngine;

public static class Mouse
{
    public static List<IntPtr> Cursors = new();
    public static Dictionary<Cursor, int> SystemCursorIndexes = new();
    public static Dictionary<string, int> CursorIndexes = new();

    public static void LoadSystemCursor(Cursor cursor)
    {
        var ptr = SDL.CreateSystemCursor((SDL.SystemCursor)cursor);

        if (ptr == IntPtr.Zero)
        {
            WLog.Error($"Failed creating system cursor: {SDL.GetError()}");
            return;
        }

        var index = Cursors.Count;
        
        Cursors.Add(ptr);
        SystemCursorIndexes.Add(cursor, index);
    }

    public static void SetCursor(Cursor cursor)
    {
        if (!SystemCursorIndexes.ContainsKey(cursor))
        {
            LoadSystemCursor(cursor);
        }

        SDL.SetCursor(Cursors[SystemCursorIndexes[cursor]]);
    }
    
    public static void SetCursor(string name)
    {
        if (!CursorIndexes.ContainsKey(name))
        {
            return;
        }

        SDL.SetCursor(Cursors[CursorIndexes[name]]);
    }

    public static void UnloadCursors()
    {
        foreach (var cursor in Cursors)
        {
            SDL.DestroyCursor(cursor);
        }

        Cursors.Clear();
        SystemCursorIndexes.Clear();
        CursorIndexes.Clear();
    }
}

public enum Cursor
{
    /// <summary>
    /// Default cursor. Usually an arrow.
    /// </summary>
    Default,
    
    /// <summary>
    /// Text selection. Usually an I-beam.
    /// </summary>
    Text,
    
    /// <summary>
    /// Wait. Usually an hourglass or watch or spinning ball.
    /// </summary>
    Wait,
    
    /// <summary>
    /// Crosshair.
    /// </summary>
    Crosshair,
    
    /// <summary>
    /// Program is busy but still interactive. Usually it's WAIT with an arrow.
    /// </summary>
    Progress,
    
    /// <summary>
    /// Double arrow pointing northwest and southeast.
    /// </summary>
    NWSEResize,
    
    /// <summary>
    /// Double arrow pointing northeast and southwest.
    /// </summary>
    NESWResize,
    
    /// <summary>
    /// Double arrow pointing west and east.
    /// </summary>
    EWResize,
    
    /// <summary>
    /// Double arrow pointing north and south.
    /// </summary>
    NSResize,
    
    /// <summary>
    /// Four pointed arrow pointing north, south, east, and west.
    /// </summary>
    Move,
    
    /// <summary>
    /// Not permitted. Usually a slashed circle or crossbones.
    /// </summary>
    NotAllowed,
    
    /// <summary>
    /// Pointer that indicates a link. Usually a pointing hand.
    /// </summary>
    Pointer,
    
    /// <summary>
    /// Window resize top-left. This may be a single arrow or a double arrow  like <see cref="NWSEResize"/>.
    /// </summary>
    NWResize,
    
    /// <summary>
    /// Window resize top. May be <see cref="NSResize"/>.
    /// </summary>
    NResize,
    
    /// <summary>
    /// Window resize top-right. May be <see cref="NESWResize"/>.
    /// </summary>
    NEResize,
    
    /// <summary>
    /// Window resize right. May be <see cref="EWResize"/>.
    /// </summary>
    EResize,
    
    /// <summary>
    /// Window resize bottom-right. May be <see cref="NWSEResize"/>.
    /// </summary>
    SEResize,
    
    /// <summary>
    /// Window resize bottom. May be <see cref="NSResize"/>.
    /// </summary>
    SResize,
    
    /// <summary>
    /// Window resize bottom-left. May be <see cref="NESWResize"/>.
    /// </summary>
    SWResize,
    
    /// <summary>
    /// Window resize left. May be <see cref="EWResize"/>.
    /// </summary>
    WResize,
    
    SDLNumSystemCursors
}