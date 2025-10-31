using System.Diagnostics;
using System.Runtime.CompilerServices;
using SDL3;

namespace WaffleEngine;

public static class Assert
{
    public static void True(bool condition, string message, [CallerFilePath] string? callerFileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        if (condition) return;
        
        Log.ConsoleLogger.Fatal(message, $"{callerFileName}: line {lineNumber}");
        Application.Exit();
    }

    public static void False(bool condition, string message, [CallerFilePath] string? callerFileName = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!condition) return;
        
        Log.ConsoleLogger.Fatal(message, $"{callerFileName}: line {lineNumber}");
        Application.Exit();
    }
}