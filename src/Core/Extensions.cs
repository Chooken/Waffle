namespace WaffleEngine;

public static class Extensions
{
    public static unsafe int ToInt(this bool boolean)
    {
        return *(Byte*)&boolean;
    }
}