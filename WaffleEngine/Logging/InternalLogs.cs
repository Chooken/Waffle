namespace WaffleEngine;

internal static class WLog
{
    private static readonly Logger Logger = new ("Waffle Engine");
        
    public static void Fatal(string message, params string[] tags)
    {
        Logger.Fatal(message, tags);
    }

    public static void Error(string message, params string[] tags)
    {
        Logger.Error(message, tags);
    }

    public static void Warning(string message, params string[] tags)
    {
        Logger.Warning(message, tags);
    }

    public static void Info(string message, params string[] tags)
    {
        Logger.Info(message, tags);
    }

    public static void Trace(string message, params string[] tags)
    {
        Logger.Trace(message, tags);
    }
}