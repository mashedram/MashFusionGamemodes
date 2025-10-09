using LabFusion.Player;
using LabFusion.Utilities;
using MelonLoader;

namespace MashGamemodeLibrary.Player;

public static class PlayerStatManager
{
    internal static PlayerStats? LocalStatOverride;

    private static void SetVitality(float? value)
    {
        if (LocalHealth.VitalityOverride.Equals(value))
            return;

        LocalHealth.VitalityOverride = value;
    }
    
    public static void SetAvatarAndStats(string barcode, PlayerStats stats)
    {
        LocalStatOverride = stats;
        SetVitality(stats.Vitality);
        LocalAvatar.AvatarOverride = barcode;
    }
    
    public static void SetStats(PlayerStats stats)
    {
        LocalStatOverride = stats;
        SetVitality(stats.Vitality);
        LocalAvatar.RefreshAvatar();
    }

    public static void ResetStats()
    {
        LocalStatOverride = null;
        SetVitality(null);
    }
}