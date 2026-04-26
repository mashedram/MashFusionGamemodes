using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Rules;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data;

public static class PlayerDataManager
{
    private static readonly Dictionary<byte, PlayerData> PlayerData = new();

    static PlayerDataManager()
    {
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
        NetworkPlayer.OnNetworkRigCreated += OnNetworkRigCreated;
    }

    private static void OnPlayerJoined(PlayerID playerID)
    {
        PlayerData.GetValueOrCreate(playerID, () => new PlayerData(playerID));
    }

    private static void OnNetworkRigCreated(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        var playerID = networkPlayer.PlayerID;
        var data = PlayerData.GetValueOrCreate(playerID, () => new PlayerData(playerID));

        data.OnRigCreated(networkPlayer, rigManager);
    }

    private static void OnPlayerLeft(PlayerID playerId)
    {
        PlayerData.Remove(playerId);
    }

    // Accessors

    public static PlayerData? GetPlayerData(NetworkPlayer networkPlayer)
    {
        if (!networkPlayer.HasRig)
            return null;

        return PlayerData.GetValueOrDefault(networkPlayer.PlayerID);
    }

    public static PlayerData? GetOrCreatePlayerData(byte playerID)
    {
        if (PlayerData.TryGetValue(playerID, out var data))
            return data;

        if (!NetworkPlayerManager.TryGetPlayer(playerID, out var networkPlayer))
            return null;

        var newData = new PlayerData(networkPlayer.PlayerID);
        PlayerData[playerID] = newData;
        if (networkPlayer.HasRig)
            newData.OnRigCreated(networkPlayer, networkPlayer.RigRefs.RigManager);
        return newData;
    }

    public static PlayerData? GetPlayerData(byte playerID)
    {
        return GetOrCreatePlayerData(playerID);
    }

    public static bool TryGetPlayerData(PlayerID playerId, [MaybeNullWhen(false)] out PlayerData playerData)
    {
        playerData = GetOrCreatePlayerData(playerId);
        return playerData != null;
    }

    public static PlayerData? GetLocalPlayerData()
    {
        if (!NetworkInfo.HasServer)
            return null;
        
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer == null)
            return null;

        return GetOrCreatePlayerData(localPlayer.PlayerID);
    }

    public static void ForEachPlayerData(Action<PlayerData> action)
    {
        foreach (var playerData in PlayerData.Values)
        {
            action(playerData);
        }
    }

    public static void ModifyAll<TRule>(PlayerRuleInstance<TRule>.ModifyRuleDelegate modifier) where TRule : class, IPlayerRule, new()
    {
        ForEachPlayerData(playerData =>
        {
            playerData.ModifyRule(modifier);
        });
    }
    
    public static void ResetRules()
    {
        foreach (var playerRuleInstance in PlayerData.Values)
        {
            playerRuleInstance.ResetRules();
        }
    }

    public static void Clear()
    {
        PlayerData.Clear();
    }

    internal static void SendCatchup(PlayerID playerID)
    {
        ForEachPlayerData(playerData =>
        {
            playerData.SendCatchup(playerID);
        });
    }

    internal static void CallEventOnAll(IPlayerEvent playerEvent)
    {
        ForEachPlayerData(playerData =>
        {
            playerData.ReceiveEvent(playerEvent);
        });
    }
}