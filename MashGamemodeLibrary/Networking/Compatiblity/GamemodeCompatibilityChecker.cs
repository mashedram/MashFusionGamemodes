using System.Reflection;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation.Routes;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Compatiblity;

internal readonly record struct GamemodeCompatibilityInfo(string GamemodeId, string Version)
{
    public ulong Hash { get; } = GamemodeId.GetStableHash() + Version.GetStableHash();
}

public class GamemodeHashPacket : INetSerializable, IKnownSenderPacket
{
    public byte SenderPlayerID { get; set; }
    public List<ulong> Hashes = new();
    
    public void Serialize(INetSerializer serializer)
    {
        ulong value = 0;
        // If we are a reader, it will overwrite the count
        // If we are a writer, the current count will be written
        var count = Hashes.Count;
        serializer.SerializeValue(ref count);
        // Diverging logic
        if (serializer.IsReader)
        {
            Hashes.Clear();
            for (var i = 0; i < count; i++)
            {
                serializer.SerializeValue(ref value);
                Hashes.Add(value);
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
            {
                value = Hashes[i];
                serializer.SerializeValue(ref value);
            }
        }
    }
}

public static class GamemodeCompatibilityChecker
{
    private static readonly RemoteEvent<GamemodeHashPacket> GamemodeHashRemoteEvent =
        new("GlobalGamemodeHashEvent", OnGamemodeHashesReceived, new ClientToHostNetworkRoute());

    private static Gamemode? _activeGamemode;
    private static readonly HashSet<byte> ValidatedPlayers = new();
    private static readonly Dictionary<Type, GamemodeCompatibilityInfo> LocalGamemodeInfo = new ();
    private static readonly Dictionary<byte, List<ulong>> RemoteGamemodeHashes = new();

    static GamemodeCompatibilityChecker()
    {
        MultiplayerHooking.OnJoinedServer += SendGamemodeHashes;
    }

    public static void SetActiveGamemode(Gamemode? gamemode)
    {
        _activeGamemode = gamemode;
        ValidatedPlayers.Clear();
    }

    private static void KickPlayer(byte smallId)
    {
        if (_activeGamemode == null)
            return;

        var info = LocalGamemodeInfo[_activeGamemode.GetType()];
        
        ConnectionSender.SendDisconnect(smallId, $"The server is running a gamemode that you don't have: {info.GamemodeId} - {info.Version}");
        
        RemoteGamemodeHashes.Remove(smallId);
    }
    
    public static void ValidatePlayer(byte smallId)
    {
        if (_activeGamemode == null)
            return;
        
        if (ValidatedPlayers.Contains(smallId))
            return;

        if (!RemoteGamemodeHashes.TryGetValue(smallId, out var remoteHashes))
        {
            KickPlayer(smallId);
            return;
        }
        
        var requiredHash = LocalGamemodeInfo[_activeGamemode.GetType()];
        if (!remoteHashes.Contains(requiredHash.Hash))
        {
            KickPlayer(smallId);
            return;
        }

        ValidatedPlayers.Add(smallId);
    }
    
    public static void RegisterGamemodeInfo(Gamemode gamemode)
    {
        var assembly = gamemode.GetType().Assembly;
        var attribute = assembly.GetCustomAttribute<MelonInfoAttribute>();
        var version = attribute?.Version ?? "1.0.0";

        var info = new GamemodeCompatibilityInfo(gamemode.Title, version);
        LocalGamemodeInfo.Add(gamemode.GetType(), info);
    }

    public static void SendGamemodeHashes()
    {
        Executor.RunIfNotHost(() =>
        {
            GamemodeHashRemoteEvent.Call(new GamemodeHashPacket
            {
                Hashes = LocalGamemodeInfo.Values.Select(v => v.Hash).ToList()
            });
        });
    }

    public static void ClearRemoteHashes()
    {
        RemoteGamemodeHashes.Clear();
    }
    
    // Static callbacks
    private static void OnGamemodeHashesReceived(GamemodeHashPacket packet)
    {
        RemoteGamemodeHashes[packet.SenderPlayerID] = packet.Hashes;
    }
}