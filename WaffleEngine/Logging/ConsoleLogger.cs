using System.Diagnostics;

namespace WaffleEngine;

public class ConsoleLogger(string? loggerName, int skipFrames) : ILogger
{
    public void Fatal(string message, string? tag = null)
    {
        StackTrace trace = new StackTrace(skipFrames + 1);
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [FATAL] ");
        LogMessage(message, tag);
        LogTrace(trace);
    }

    public void Error(string message, string? tag = null)
    {
        StackTrace trace = new StackTrace(skipFrames + 1);
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [ERROR] ");
        LogMessage(message, tag);
        LogTrace(trace);
    }

    public void Warning(string message, string? tag = null)
    {
        StackTrace trace = new StackTrace(skipFrames + 1);
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [WARNING] ");
        LogMessage(message, tag);
        LogTrace(trace);
    }

    public void Info(string message, string? tag = null)
    {
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [INFO] ");
        LogMessage(message, tag);
    }

    public void Trace(string message, string? tag = null)
    {
        StackTrace trace = new StackTrace(skipFrames + 1);
        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [TRACE] ");
        LogMessage(message, tag);
        LogTrace(trace);
    }
    
    private void LogMessage(string message, string? tag = null)
    {
        Console.ResetColor();
        if (loggerName is not null) Console.Write($"[{loggerName}] ");
        if (tag is not null) Console.Write($"[{tag}] ");
        Console.WriteLine(message);
    }

    private void LogTrace(StackTrace trace)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(trace);
        Console.ResetColor();
    }
}