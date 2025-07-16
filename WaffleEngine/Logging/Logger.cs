namespace WaffleEngine;

public class Logger(string logger_name)
{
    public void Fatal(string message, params string[] tags)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [FATAL] ");
        LogMessage(message, tags);
    }

    public void Error(string message, params string[] tags)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [ERROR] ");
        LogMessage(message, tags);
    }

    public void Warning(string message, string[] tags)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [WARNING] ");
        LogMessage(message, tags);
    }

    public void Info(string message, params string[] tags)
    {
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [INFO] ");
        LogMessage(message, tags);
    }

    public void Trace(string message, params string[] tags)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;

        Console.Write($"[{DateTime.Now:HH:mm:ss}] [TRACE] ");
        LogMessage(message, tags);
    }
    
    private void LogMessage(string message, params string[] tags)
    {
        Console.ResetColor();
        if (logger_name.Length != 0) Console.Write($"[{logger_name}] ");
        foreach (var tag in tags)
        {
            Console.Write($"[{tag}] ");
        }
        Console.WriteLine(message);
    }
}