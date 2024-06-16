using Raylib_cs;

namespace WaffleEngine
{
    public class Mouse
    {
        private static int x, y;

        public static int X => x;
        public static int Y => y;

        public static bool IsLeft() => Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Left);
        public static bool IsRight() => Raylib.IsMouseButtonDown(Raylib_cs.MouseButton.Right);

        public static bool IsPressed(int button) => Raylib.IsMouseButtonPressed((Raylib_cs.MouseButton)button);
        public static bool IsPressed(MouseButton button) => Raylib.IsMouseButtonPressed((Raylib_cs.MouseButton)button);

        public static bool IsDown(int button) => Raylib.IsMouseButtonDown((Raylib_cs.MouseButton)button);
        public static bool IsDown(MouseButton button) => Raylib.IsMouseButtonDown((Raylib_cs.MouseButton)button);

        public static void SetCursor(int cursor) => Raylib.SetMouseCursor((Raylib_cs.MouseCursor)cursor);
        public static void SetCursor(MouseCursor cursor) => Raylib.SetMouseCursor((Raylib_cs.MouseCursor)cursor);
    }

    public enum MouseButton
    {
        // From Raylib

        /// <summary>
        /// Mouse button left
        /// </summary>
        Left = 0,

        /// <summary>
        /// Mouse button right
        /// </summary>
        Right = 1,

        /// <summary>
        /// Mouse button middle (pressed wheel)
        /// </summary>
        Middle = 2,

        /// <summary>
        /// Mouse button side (advanced mouse device)
        /// </summary>
        Side = 3,

        /// <summary>
        /// Mouse button extra (advanced mouse device)
        /// </summary>
        Extra = 4,

        /// <summary>
        /// Mouse button forward (advanced mouse device)
        /// </summary>
        Forward = 5,

        /// <summary>
        /// Mouse button back (advanced mouse device)
        /// </summary>
        Back = 6
    }

    public enum MouseCursor
    {
        // From Raylib

        /// <summary>
        /// Default pointer shape
        /// </summary>
        Default = 0,

        /// <summary>
        /// Arrow shape
        /// </summary>
        Arrow = 1,

        /// <summary>
        /// Text writing cursor shape
        /// </summary>
        IBeam = 2,

        /// <summary>
        /// Cross shape
        /// </summary>
        Crosshair = 3,

        /// <summary>
        /// Pointing hand cursor
        /// </summary>
        PointingHand = 4,

        /// <summary>
        /// Horizontal resize/move arrow shape
        /// </summary>
        ResizeEw = 5,

        /// <summary>
        /// Vertical resize/move arrow shape
        /// </summary>
        ResizeNs = 6,

        /// <summary>
        /// Top-left to bottom-right diagonal resize/move arrow shape
        /// </summary>
        ResizeNwse = 7,

        /// <summary>
        /// The top-right to bottom-left diagonal resize/move arrow shape
        /// </summary>
        ResizeNesw = 8,

        /// <summary>
        /// The omnidirectional resize/move cursor shape
        /// </summary>
        ResizeAll = 9,

        /// <summary>
        /// The operation-not-allowed shape
        /// </summary>
        NotAllowed = 10
    }
}
