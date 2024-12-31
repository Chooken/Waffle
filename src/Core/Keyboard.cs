using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WaffleEngine
{
    public static class Keyboard
    {
        public static bool IsPressed(int key) => Window.CurrentKeyboardState.IsKeyPressed((Keys)key);

        public static bool IsDown(int key) => Window.CurrentKeyboardState.IsKeyDown((Keys)key);
    }

    public static class Keycode
    {
        public const int Null = 0;
        public const int Apostrophe = 39;
        public const int Comma = 44;
        public const int Minus = 45;
        public const int Period = 46;
        public const int Slash = 47;
        public const int Zero = 48;
        public const int One = 49;
        public const int Two = 50;
        public const int Three = 51;
        public const int Four = 52;
        public const int Five = 53;
        public const int Six = 54;
        public const int Seven = 55;
        public const int Eight = 56;
        public const int Nine = 57;
        public const int Semicolon = 59;
        public const int Equal = 61;
        public const int A = 65;
        public const int B = 66;
        public const int C = 67;
        public const int D = 68;
        public const int E = 69;
        public const int F = 70;
        public const int G = 71;
        public const int H = 72;
        public const int I = 73;
        public const int J = 74;
        public const int K = 75;
        public const int L = 76;
        public const int M = 77;
        public const int N = 78;
        public const int O = 79;
        public const int P = 80;
        public const int Q = 81;
        public const int R = 82;
        public const int S = 83;
        public const int T = 84;
        public const int U = 85;
        public const int V = 86;
        public const int W = 87;
        public const int X = 88;
        public const int Y = 89;
        public const int Z = 90;

        // Function keys
        public const int Space = 32;
        public const int Escape = 256;
        public const int Enter = 257;
        public const int Tab = 258;
        public const int Backspace = 259;
        public const int Insert = 260;
        public const int Delete = 261;
        public const int Right = 262;
        public const int Left = 263;
        public const int Down = 264;
        public const int Up = 265;
        public const int PageUp = 266;
        public const int PageDown = 267;
        public const int Home = 268;
        public const int End = 269;
        public const int CapsLock = 280;
        public const int ScrollLock = 281;
        public const int NumLock = 282;
        public const int PrintScreen = 283;
        public const int Pause = 284;
        public const int F1 = 290;
        public const int F2 = 291;
        public const int F3 = 292;
        public const int F4 = 293;
        public const int F5 = 294;
        public const int F6 = 295;
        public const int F7 = 296;
        public const int F8 = 297;
        public const int F9 = 298;
        public const int F10 = 299;
        public const int F11 = 300;
        public const int F12 = 301;
        public const int LeftShift = 340;
        public const int LeftControl = 341;
        public const int LeftAlt = 342;
        public const int LeftSuper = 343;
        public const int RightShift = 344;
        public const int RightControl = 345;
        public const int RightAlt = 346;
        public const int RightSuper = 347;
        public const int KeyboardMenu = 348;
        public const int LeftBracket = 91;
        public const int Backslash = 92;
        public const int RightBracket = 93;
        public const int Grave = 96;

        // Keypad keys
        public const int Kp0 = 320;
        public const int Kp1 = 321;
        public const int Kp2 = 322;
        public const int Kp3 = 323;
        public const int Kp4 = 324;
        public const int Kp5 = 325;
        public const int Kp6 = 326;
        public const int Kp7 = 327;
        public const int Kp8 = 328;
        public const int Kp9 = 329;
        public const int KpDecimal = 330;
        public const int KpDivide = 331;
        public const int KpMultiply = 332;
        public const int KpSubtract = 333;
        public const int KpAdd = 334;
        public const int KpEnter = 335;
        public const int KpEqual = 336;

        // Android key buttons
        public const int Back = 4;
        public const int Menu = 82;
        public const int VolumeUp = 24;
        public const int VolumeDown = 25;
    }
}
