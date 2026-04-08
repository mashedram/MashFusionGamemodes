using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Stats;

public static class PlayerStatManager
{
    private static PlayerStats? LocalStatOverride;

    public static bool BalanceStats { get; set; } = false;

    private const float TargetHeight = 1.8f;
    private const float SafeMargin = 0.2f;
    private const float MaxMargin = 0.8f;
    private const float Multiplier = 0.5f;

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
        LocalAvatar.RefreshAvatar();
    }
    
    // Getter

    private static float GetBalancedModifier(Avatar avatar)
    {
        var difference = Mathf.Abs(avatar.height - TargetHeight);
        
        if (difference <= SafeMargin) 
            return 1f;
        
        if (difference >= MaxMargin) 
            return 1f - Multiplier;
        
        var factor = (difference - SafeMargin) / (MaxMargin - SafeMargin);
        return 1f - factor * Multiplier;
    }
    
    public static bool TryGetLocalStats(Avatar avatar, out PlayerStats stats)
    {
        if (!LocalStatOverride.HasValue)
        {
            stats = default;
            return false;
        }

        if (BalanceStats)
        {
            var modifier = GetBalancedModifier(avatar);
            stats = LocalStatOverride.Value.MultiplyHealth(modifier);
        }
        else
        {
            stats = LocalStatOverride.Value;
        }
        return true;
    }
}