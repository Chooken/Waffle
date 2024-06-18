using Raylib_cs;
using System.Runtime.InteropServices;

namespace WaffleEngine
{
    public class Log
    {
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public unsafe static void RaylibLog(int logLevel, sbyte* text, sbyte* args)
        {
            var message = Logging.GetLogMessage(new IntPtr(text), new IntPtr(args));

            switch ((TraceLogLevel)logLevel)
            {
                case TraceLogLevel.Fatal:
                    Fatal(message);
                    break;

                case TraceLogLevel.Error:
                    Error(message);
                    break;

                case TraceLogLevel.Warning:
                    Warning(message);
                    break;

                case TraceLogLevel.Info:
                    Info(message);
                    break;

                case TraceLogLevel.Debug:
                    Info(message);
                    break;

                case TraceLogLevel.Trace:
                    Trace(message);
                    break;

                case TraceLogLevel.All:
                    Info(message);
                    break;

                default:
                    return;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fatal(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [FATAL] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Error(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [ERROR] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Warning(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [WARNING] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Info(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [INFO] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Trace(string format, params object?[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [TRACE] ");
            Console.WriteLine(format, args);

            Console.ResetColor();
        }
    }
}
