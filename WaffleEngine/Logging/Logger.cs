namespace WaffleEngine;

public class Logger(string loggerName)
{
    public void Fatal(string message, string? tag = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [FATAL] ");
        LogMessage(message, tag);
    }

    public void Error(string message, string? tag = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [ERROR] ");
        LogMessage(message, tag);
    }

    public void Warning(string message, string? tag = null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [WARNING] ");
        LogMessage(message, tag);
    }

    public void Info(string message, string? tag = null)
    {
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [INFO] ");
        LogMessage(message, tag);
    }

    public void Trace(string message, string? tag = null)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [TRACE] ");
        LogMessage(message, tag);
    }
    
    private void LogMessage(string message, string? tag = null)
    {
        Console.ResetColor();
        if (loggerName.Length != 0) Console.Write($"[{loggerName}] ");
        if (tag is not null) Console.Write($"[{tag}] ");
        Console.WriteLine(message);
    }
}