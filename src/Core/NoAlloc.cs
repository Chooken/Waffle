namespace WaffleEngine;

public static class NoAlloc
{
    private static string[] negative_ints;
    private static string[] positive_ints;
    
    private const int MAX_PREGENNED_INTS = 10000;

    public static void Init()
    {
        negative_ints = new string[MAX_PREGENNED_INTS + 1];
        positive_ints = new string[MAX_PREGENNED_INTS + 1];
        
        for (int i = 0; i <= MAX_PREGENNED_INTS; i++)
        {
            negative_ints[i] = (-i).ToString();
            positive_ints[i] = i.ToString();
        }
    }

    public static string ToStringNoAlloc(this int value)
    {
        if (System.Math.Abs(value) > MAX_PREGENNED_INTS)
        {
            Log.Warning("Tried to call ToStringNoAlloc with the value {0} and the max is {1}", value, MAX_PREGENNED_INTS);
            return int.IsPositive(value) ? positive_ints[MAX_PREGENNED_INTS] : negative_ints[MAX_PREGENNED_INTS];
        }

        return int.IsPositive(value) ? positive_ints[value] : negative_ints[System.Math.Abs(value)];
    }
}