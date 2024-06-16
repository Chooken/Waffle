namespace WaffleEngine
{
    public class Log
    {
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
    }
}
