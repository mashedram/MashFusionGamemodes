using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
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

    public static PlayerData? GetPlayerData(byte playerID)
    {
        return PlayerData.GetValueOrDefault(playerID);
    }
    
    public static bool TryGetPlayerData(PlayerID playerId, [MaybeNullWhen(false)] out PlayerData playerData)
    {
        return PlayerData.TryGetValue(playerId, out playerData);
    }
    
    public static PlayerData? GetLocalPlayerData()
    {
        if (!NetworkInfo.HasServer)
            return null;
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer == null)
            return null;
        
        return PlayerData.GetValueOrDefault(localPlayer.PlayerID);
    }
    
    public static void ForEachPlayerData(Action<PlayerData> action)
    {
        foreach (var playerData in PlayerData.Values)
        {
            action(playerData);
        }
    }

    public static void Reset()
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
}