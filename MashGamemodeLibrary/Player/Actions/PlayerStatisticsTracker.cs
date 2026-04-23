using LabFusion.SDK.Points;
using LabFusion.UI.Popups;

namespace MashGamemodeLibrary.Player.Actions;

public static class PlayerStatisticsTracker
{
    private static readonly Dictionary<Enum, Func<int, int>> Awarders = new();
    private static readonly Dictionary<Enum, int> Statistics = new();

    public static void Increment(Enum key, int value = 1)
    {
        Statistics.TryAdd(key, 0);
        Statistics[key] += value;

        GlobalStatisticsCollector.SyncChangeToHost(key, Statistics[key]);
    }

    public static void Clear()
    {
        Statistics.Clear();
        GlobalStatisticsCollector.Clear();
    }

    public static void Register(Enum key, Func<int, int> mapper)
    {
        Awarders[key] = mapper;
        GlobalStatisticsCollector.RegisterStatisticKey(key);
    }

    private static int GetTotalBits(int extraBits = 0)
    {
        return Math.Max(Statistics.Sum(kvp =>
        {
            if (!Awarders.TryGetValue(kvp.Key, out var awarder))
                return 0;

            return awarder(kvp.Value);
        }) + extraBits, 0);
    }

    public static Dictionary<string, int> GetStatisticsSnapshot()
    {
        return Statistics.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
    }

    public static void AwardBits(params Enum[] keys)
    {
        var bits = GetTotalBits();

        var message = "";
        foreach (var key in keys)
        {
            var value = Statistics.GetValueOrDefault(key, 0);
            var name = key.ToString();

            message += $"{name}: {value}\n";
        }

        Notifier.Send(new Notification
        {
            Title = $"You've been awarded {bits} bits",
            Message = message,
            ShowPopup = true,
            PopupLength = 5f,
            SaveToMenu = false,
            Type = NotificationType.SUCCESS
        });
        PointItemManager.RewardBits(bits);
    }
}