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
    }

    private static PlayerData GetDataOrCreate(NetworkPlayer networkPlayer)
    {
        return PlayerData.GetValueOrCreate(networkPlayer.PlayerID, () => new PlayerData(networkPlayer));
    }
    
    private static void OnPlayerJoined(PlayerID playerID)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerID, out var networkPlayer))
            return;
        
        var data = GetDataOrCreate(networkPlayer);
        data.OnRigChanged(networkPlayer.RigRefs.RigManager);
    }
    
    private static void OnPlayerLeft(PlayerID playerId)
    {
        PlayerData.Remove(playerId);
    }
    
    // Accessors
    
    public static PlayerData GetPlayerData(NetworkPlayer networkPlayer)
    {
        return GetDataOrCreate(networkPlayer);
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
        
        return GetDataOrCreate(LocalPlayer.GetNetworkPlayer()!);
    }
    
    public static void Reset()
    {
        // TODO
    }
}