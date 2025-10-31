namespace WaffleEngine;

public static class Input
{
    internal static readonly InputHandler GlobalInputHandler = new();

    public static EventSpace GetDefaultEventSpace => GlobalInputHandler.DefaultEventSpace;
    public static EventSpace GetEventSpace(Modifier modifier) => GlobalInputHandler.GetEventSpace(modifier);
    public static void AddEventSpace(Modifier modifier) => GlobalInputHandler.AddEventSpace(modifier);

    public static MouseData Mouse => GlobalInputHandler.MouseData;
}