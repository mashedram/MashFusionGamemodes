namespace MashGamemodeLibrary.Player.Actions;

public static class PlayerStatisticsTracker
{
    private static readonly Dictionary<Enum, int> Statistics = new();

    public static void Increment(Enum index)
    {
        Statistics.TryAdd(index, 0);
        Statistics[index] += 1;
    }
    
    public static void Clear()
    {
        Statistics.Clear();
    }
}