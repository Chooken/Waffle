
namespace WaffleEngine
{
    public class Log
    {
        public static readonly ILogger ConsoleLogger = new ConsoleLogger(null, 1);
        
        public static void Fatal(string message)
        {
            ConsoleLogger.Fatal(message);
        }

        public static void Error(string message)
        {
            ConsoleLogger.Error(message);
        }

        public static void Warning(string message)
        {
            ConsoleLogger.Warning(message);
        }

        public static void Info(string message)
        {
            ConsoleLogger.Info(message);
        }

        public static void Trace(string message)
        {
            ConsoleLogger.Trace(message);
        }
    }
}
