using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules;
using MashGamemodeLibrary.Player.Data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Helpers;

public static class SpectatorExtender
{

    private static PlayerRuleInstance<PlayerSpectatingRule> GetOrCreateSpectatingModifier(PlayerID playerId)
    {
        var playerData = PlayerDataManager.GetPlayerData(playerId);
        if (playerData == null) throw new Exception($"Player data not found for player ID: {playerId}");

        return playerData.GetRuleInstance<PlayerSpectatingRule>();
    }

    public static bool IsSpectating(this NetworkPlayer player)
    {
        return PlayerDataManager.GetPlayerData(player)?.CheckRule<PlayerSpectatingRule>(r => r.IsSpectating) ?? false;
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
        PlayerDataManager.ForEachPlayerData(data =>
        {
            var modifier = data.GetRuleInstance<PlayerSpectatingRule>();
            modifier.Modify(rule => rule.IsSpectating = false);
        });
    }
}