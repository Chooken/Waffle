
using System.Numerics;

namespace WaffleEngine
{
    public class Mouse
    {
        public static float X => Window.CurrentMouseState.X;
        public static float Y => Window.CurrentMouseState.Y;

        public static float DeltaX => Window.CurrentMouseState.X - Window.CurrentMouseState.PreviousX;
        public static float DeltaY => Window.CurrentMouseState.Y - Window.CurrentMouseState.PreviousY;

        public static Vector2 Position => new Vector2(X, Y);
        public static Vector2 Delta => new Vector2(DeltaX, DeltaY);

        public static bool IsLeft() => Window.CurrentMouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)MouseButton.Left);
        public static bool IsRight() => Window.CurrentMouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)MouseButton.Right);

        public static bool IsPressed(int button) => Window.CurrentMouseState.IsButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
        public static bool IsPressed(MouseButton button) => Window.CurrentMouseState.IsButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);

        public static bool IsDown(int button) => Window.CurrentMouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
        public static bool IsDown(MouseButton button) => Window.CurrentMouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
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
}
