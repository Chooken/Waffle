
namespace WaffleEngine
{
    public class Log
    {
        [System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("RELEASE")]
        public static void Fatal(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [FATAL] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("RELEASE")]
        public static void Error(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [ERROR] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("RELEASE")]
        public static void Warning(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [WARNING] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("RELEASE")]
        public static void Info(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [INFO] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("RELEASE")]
        public static void Trace(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [TRACE] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }
    }
}
