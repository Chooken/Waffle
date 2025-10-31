namespace WaffleEngine;

public interface ILogger
{
    public void Fatal(string message, string? tag = null);

    public void Error(string message, string? tag = null);

    public void Warning(string message, string? tag = null);

    public void Info(string message, string? tag = null);

    public void Trace(string message, string? tag = null);
}