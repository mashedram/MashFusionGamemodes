using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating;

namespace MashGamemodeLibrary.Player.Stats;

public static class PlayerStatManager
{
    internal static PlayerStats? LocalStatOverride;

    private static void SetVitality(float? value)
    {
        if (SpectatorManager.IsLocalPlayerSpectating())
        {
            value = 100f;
        }
        
        if (LocalHealth.VitalityOverride.Equals(value))
            return;

        LocalHealth.VitalityOverride = value;
    }
    
    public static void RefreshVitality()
    {
        SetVitality(LocalStatOverride?.Vitality);
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