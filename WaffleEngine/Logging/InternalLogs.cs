using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WaffleEngine;

internal static class WLog
{
    private static readonly Logger Logger = new ("Waffle Engine");
        
    public static void Fatal(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Logger.Fatal(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }

    public static void Error(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Logger.Error(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }

    public static void Warning(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Logger.Warning(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }

    public static void Info(string message, [CallerFilePath] string? callerFileName = null)
    {
        Logger.Info(message, Path.GetFileNameWithoutExtension(callerFileName));
    }

    public static void Trace(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        Logger.Trace(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }
}