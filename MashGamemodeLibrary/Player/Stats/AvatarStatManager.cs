using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Stats;

public static class AvatarStatManager
{
    private static AvatarStats? _localStatOverride;

    public static bool BalanceStats { get; set; } = false;

    private const float TargetHeight = 1.8f;
    private const float SafeMargin = 0.2f;
    private const float MaxMargin = 0.8f;
    private const float Multiplier = 0.5f;

    private const float TeamUnbalancedSteps = 1f;
    private const float TeamUnbalancedMultiplier = 0.5f;

    private static void SetVitality(float? value)
    {
        if (SpectatorExtender.IsLocalPlayerSpectating())
        {
            value = 100f;
        }

        if (LocalHealth.VitalityOverride.Equals(value))
            return;

        LocalHealth.VitalityOverride = value;
    }

    public static void RefreshVitality()
    {
        var stats = GetLocalStats(BoneLib.Player.Avatar);
        SetVitality(stats?.Vitality);
    }

    public static void SetAvatarAndStats(string barcode, AvatarStats stats)
    {
        _localStatOverride = stats;
        SetVitality(stats.Vitality);
        LocalAvatar.AvatarOverride = barcode;
    }

    public static void SetStats(AvatarStats stats)
    {
        _localStatOverride = stats;
        SetVitality(stats.Vitality);
        LocalAvatar.RefreshAvatar();
    }

    public static void ResetStats()
    {
        _localStatOverride = null;
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

    private static float GetTeamBalanceModifier()
    {
        var localTeamId = LogicTeamManager.GetLocalTeamID();
        if (!localTeamId.HasValue)
            return 1f;

        var validPlayers = NetworkPlayer.Players
            .Where(p => p.HasRig)
            .ToList();

        var teamMemberCount = validPlayers.Count(p => LogicTeamManager.GetPlayerTeamID(p.PlayerID) == localTeamId.Value);
        var totalPlayers = validPlayers.Count;
        var enemyCount = totalPlayers - teamMemberCount;

        // Health should not change if the player team has the advantage of numbers, but should be reduced if they are outnumbered
        if (enemyCount <= teamMemberCount)
            return 1f;

        var difference = enemyCount - teamMemberCount;
        if (difference >= TeamUnbalancedSteps)
            return 1f + TeamUnbalancedMultiplier;

        var factor = difference / TeamUnbalancedSteps;
        return 1f + factor * TeamUnbalancedMultiplier;
    }

    public static AvatarStats? GetLocalStats(Avatar? avatar)
    {
        if (!TryGetLocalStats(avatar, out var stats))
            return null;

        return stats;
    }

    public static bool TryGetLocalStats(Avatar? avatar, out AvatarStats stats)
    {
        if (!_localStatOverride.HasValue)
        {
            stats = default;
            return false;
        }

        if (BalanceStats)
        {
            var heightModifier = avatar != null ? GetBalancedModifier(avatar) : 1f;
            var teamBalanceModifier = GetTeamBalanceModifier();
            stats = _localStatOverride.Value.MultiplyHealth(heightModifier * teamBalanceModifier);
        }
        else
        {
            stats = _localStatOverride.Value;
        }
        return true;
    }
}