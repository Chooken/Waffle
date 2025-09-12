using System.Runtime.CompilerServices;

namespace WaffleEngine;

internal static class WLog
{
    private static readonly Logger Logger = new ("Waffle Engine");
        
    public static void Fatal(string message, [CallerMemberName] string callerName = "")
    {
        Logger.Fatal(message, callerName);
    }

    public static void Error(string message, [CallerMemberName] string callerName = "")
    {
        Logger.Error(message, callerName);
    }

    public static void Warning(string message, [CallerMemberName] string callerName = "")
    {
        Logger.Warning(message, callerName);
    }

    public static void Info(string message, [CallerMemberName] string callerName = "")
    {
        Logger.Info(message, callerName);
    }

    public static void Trace(string message, [CallerMemberName] string callerName = "")
    {
        Logger.Trace(message, callerName);
    }
}