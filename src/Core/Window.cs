using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics.CodeAnalysis;

namespace WaffleEngine
{
    public static class Window
    {
        private static NativeWindow _window;
        
        public static int Width => _window.ClientSize.X;
        public static int Height => _window.ClientSize.Y;
        public static int RenderWidth 
        { 
            get 
            {
                int[] viewport = new int[4];

                unsafe
                {
                    fixed (int* p = viewport)
                    {
                        GL.GetIntegerv(GetPName.Viewport, p);
                    }
                }

                return viewport[2]; 
            } 
        }
        public static int RenderHeight
        {
            get
            {
                int[] viewport = new int[4];

                unsafe
                {
                    fixed (int* p = viewport)
                    {
                        GL.GetIntegerv(GetPName.Viewport, p);
                    }
                }

                return viewport[3];
            }
        }

        public static double UpdateFrequency = 0;

        public static KeyboardState CurrentKeyboardState;
        public static MouseState CurrentMouseState;

        public static Action WindowResizeEvent;
        
        #region Win32 Function for timing

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

        [DllImport("kernel32")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("winmm")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm")]
        private static extern uint timeEndPeriod(uint uPeriod);

        #endregion

        public static void Init(int width = 960, int height = 960 / 16 * 9, string title = "Waffle Engine")
        {
            SetThreadAffinityMask(GetCurrentThread(), new IntPtr(1));
            
            timeBeginPeriod(8);

            NativeWindowSettings window_settings = new NativeWindowSettings
            {
                ClientSize = (width, height),
                Title = title,
                AutoLoadBindings = false,
                Vsync = VSyncMode.On
            };
            
            _window = new NativeWindow(window_settings);

            var provider = new GLFWBindingsContext();

            OpenTK.Graphics.GLLoader.LoadBindings(provider);
            
            _window.Context?.MakeCurrent();
            
            Log.Info("Window Started:");
            Log.Info("Window: [{0}x{1}]", Width, Height);
            Log.Info("Window Render: [{0}x{1}]", RenderWidth, RenderHeight);
            Log.Info("OpenGL: {0}", GL.GetString(StringName.Version));
            Log.Info("GLSL: {0}", GL.GetString(StringName.ShadingLanguageVersion));

            unsafe
            {
                GLFW.SetWindowAttrib(_window.WindowPtr, WindowAttribute.Resizable, false);
            }

            GL.ClearColor(34f / 255, 147f / 255, 76f / 255, 1);
        }

        public static bool IsMinimised => _window.WindowState == WindowState.Minimized;

        public static bool IsFocused => _window.IsFocused;

        public static unsafe bool ShouldClose => GLFW.WindowShouldClose(_window.WindowPtr);

        public static void Close() => _window.Close();

        public static void SetTargetFPS(int target)
        {
            UpdateFrequency = (target == 0) ? 0 : 1.0 / target;
        }
        
        public static void SetVSync(bool value)
        {
            _window.VSync = (value) ? VSyncMode.On : VSyncMode.Off;
        }

        public static void Resize(int  width, int height)
        {
            unsafe
            {
                GLFW.SetWindowSize(_window.WindowPtr, width, height);
                GL.Viewport(0, 0, width, height);
            }

            if (WindowResizeEvent == null)
                return;
            
            WindowResizeEvent.Invoke();
        }

        public static void ProcessEvents()
        {
            _window.NewInputFrame();
            
            GLFW.PollEvents();

            CurrentKeyboardState = _window.KeyboardState;
            CurrentMouseState = _window.MouseState;
        }

        public static void StartFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void EndFrame()
        {
            if (_window.Context == null)
            {
                throw new InvalidOperationException("Cannot use SwapBuffers when running with ContextAPI.NoAPI.");
            }

            _window.Context.SwapBuffers();
        }
    }
}
