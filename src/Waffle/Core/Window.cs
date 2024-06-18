using Raylib_cs;

namespace WaffleEngine
{
    public static class Window
    {
        public static void Init(int width = 960, int height = 960 / 16 * 9, string title = "Waffle Engine")
        {
            Raylib.InitWindow(width, height, title);

            Raylib.SetTargetFPS(60);

            Raylib.SetExitKey(KeyboardKey.Null);
        }

        public static bool IsMinimised() => Raylib.IsWindowMinimized();

        public static bool ShouldClose() => Raylib.WindowShouldClose();

        public static void Close() => Raylib.CloseWindow();

        public static void SetTargetFPS(int target) => Raylib.SetTargetFPS(target);
        public static void SetVSync()
        {
            Raylib.SetTargetFPS(0);
            Raylib.SetWindowState(ConfigFlags.VSyncHint);
        }
    }
}
