using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WaffleEngine;

internal static class WLog
{
    public static readonly ILogger ConsoleLogger = new ConsoleLogger ("Waffle Engine", 1);
        
    public static void Fatal(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        ConsoleLogger.Fatal(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }

    public static void Error(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        ConsoleLogger.Error(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }

    public static void Warning(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        ConsoleLogger.Warning(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }

    public static void Info(string message, [CallerFilePath] string? callerFileName = null)
    {
        ConsoleLogger.Info(message, Path.GetFileNameWithoutExtension(callerFileName));
    }

    public static void Trace(string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        ConsoleLogger.Trace(message, $"{Path.GetFileNameWithoutExtension(callerFileName)}: line {lineNumber}");
    }
}