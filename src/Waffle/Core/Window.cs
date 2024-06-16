using Raylib_cs;

namespace WaffleEngine
{
    internal class Window
    {
        int _Width;
        int _Height;
        string _Title;

        public Window(int width = 960, int height = 960 / 16 * 9, string title = "Waffle Engine")
        {
            _Width = width;
            _Height = height;
            _Title = title;

            Raylib.SetWindowIcon(Raylib.LoadImage("icon.ico"));

            Raylib.InitWindow(_Width, _Height, _Title);

            //Raylib.SetTargetFPS(Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor()));
            Raylib.SetTargetFPS(60);

            Raylib.SetExitKey(KeyboardKey.Null);
        }

        public bool IsMinimised() => Raylib.IsWindowMinimized();

        public bool ShouldClose() => Raylib.WindowShouldClose();

        public void Close() => Raylib.CloseWindow();

        public void SetTargetFPS(int target) => Raylib.SetTargetFPS(target);
        public void SetVSync()
        {
            Raylib.SetTargetFPS(0);
            Raylib.SetWindowState(ConfigFlags.VSyncHint);
        }
    }
}
