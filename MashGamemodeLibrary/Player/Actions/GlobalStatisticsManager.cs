using System.Text.Json;
using LabFusion.SDK.Gamemodes;
using MelonLoader;
using MelonLoader.Utils;

namespace MashGamemodeLibrary.Player.Actions;

public static class GlobalStatisticsManager
{
    private static readonly string StatisticsFolder = MelonEnvironment.UnityGameDataDirectory + "/statistics/";
    private static readonly Dictionary<Gamemode, Dictionary<string, int>> ScopedStatistics = new();

    private static Dictionary<string, int> LoadStatistics(Gamemode gamemode)
    {
        if (ScopedStatistics.TryGetValue(gamemode, out var existingValue))
            return existingValue;
        
        var filePath = StatisticsFolder + $"{gamemode.Title}.json";

        Dictionary<string, int>? data = null;
        try
        {
            Directory.CreateDirectory(filePath);

            if (File.Exists(filePath))
            {
                using var stream = File.OpenRead(filePath);
                data = JsonSerializer.Deserialize<Dictionary<string, int>>(stream);
            }
            
            data ??= new Dictionary<string, int>();
        }
        catch (Exception exception)
        {
            MelonLogger.Error("Failed to load global statistics", exception);
            data = new Dictionary<string, int>();
        } 
        
        ScopedStatistics.Add(gamemode, data);
        return data;
    }

    public static void SaveStatistics(Gamemode gamemode)
    {
        var statistics = PlayerStatisticsTracker.GetStatisticsSnapshot();
        var globalStatistics = LoadStatistics(gamemode);
        
        foreach (var (key, value) in statistics)
        {
            // If the key doesn't exist, add it. Otherwise, increment it.
            if (!globalStatistics.TryAdd(key, value))
                globalStatistics[key] += value;
        }
    }
}