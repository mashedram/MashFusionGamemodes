using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Spectating.data;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Helpers;

public static class SpectatorExtender
{
    private static readonly Dictionary<PlayerID, PlayerRuleModifier<PlayerSpectatingRule>> SpectatingModifiers = new();
    
    private static PlayerRuleModifier<PlayerSpectatingRule> GetOrCreateSpectatingModifier(PlayerID playerId)
    {
        if (SpectatingModifiers.TryGetValue(playerId, out var modifier))
            return modifier;
        
        var playerData = PlayerDataManager.GetPlayerData(playerId);
        if (playerData == null) throw new Exception($"Player data not found for player ID: {playerId}");

        var newModifier = playerData.CreateModifier<PlayerSpectatingRule>(RuleModifierPriority.Highest);
        SpectatingModifiers[playerId] = newModifier;
        return newModifier;
    }
    
    public static bool IsSpectating(this NetworkPlayer player)
    {
        return PlayerDataManager.GetPlayerData(player).CheckRule<PlayerSpectatingRule>(r => r.IsSpectating);
    }
    
    public static bool IsSpectating(this PlayerID playerId)
    {
        return PlayerDataManager.GetPlayerData(playerId)?.CheckRule<PlayerSpectatingRule>(r => r.IsSpectating) ?? false;
    }
    
    public static bool IsLocalPlayerSpectating()
    {
        return PlayerDataManager.GetLocalPlayerData()?.CheckRule<PlayerSpectatingRule>(r => r.IsSpectating) ?? false;
    }
    
    public static void SetSpectating(this NetworkPlayer player, bool isSpectating)
    {
        player.PlayerID.SetSpectating(isSpectating);
    }
    
    public static void SetSpectating(this PlayerID playerID, bool isSpectating)
    {
        var modifier = GetOrCreateSpectatingModifier(playerID);
        modifier.Modify(rule => rule.IsSpectating = isSpectating);
    }
    
    public static void StopSpectatingAll()
    {
        foreach (var modifier in SpectatingModifiers.Values)
        {
            modifier.Modify(rule => rule.IsSpectating = false);
        }
    }
}