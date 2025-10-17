
namespace WaffleEngine
{
    public class Log
    {
        private static readonly Logger Logger = new ("");
        
        public static void Fatal(string message)
        {
            Logger.Fatal(message);
        }

        public static void Error(string message)
        {
            Logger.Error(message);
        }

        public static void Warning(string message)
        {
            Logger.Warning(message);
        }

        public static void Info(string message)
        {
            Logger.Info(message);
        }

        public static void Trace(string message)
        {
            Logger.Trace(message);
        }
    }
}
