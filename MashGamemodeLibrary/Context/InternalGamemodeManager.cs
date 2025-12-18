using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.networking.Compatiblity;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase.Rounds;
using MashGamemodeLibrary.Player.Actions;

namespace MashGamemodeLibrary.Context;

public class RoundStartPacket : INetSerializable
{
    public int Index;

    public int? GetSize()
    {
        return sizeof(int);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Index);
    }
}

public class RoundEndPacket : INetSerializable
{
    public ulong WinningTeamID;
    public bool HasNextRound;
    public float TimeUntilNextRound;

    public int? GetSize()
    {
        return sizeof(ulong) + sizeof(float);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref WinningTeamID);
        serializer.SerializeValue(ref HasNextRound);
        serializer.SerializeValue(ref TimeUntilNextRound);
    }
}

public static class InternalGamemodeManager
{
    private static readonly RemoteEvent<RoundStartPacket> RoundStartEvent = new RemoteEvent<RoundStartPacket>("OnRoundStart", OnRoundStart, CommonNetworkRoutes.HostToAll);
    private static readonly RemoteEvent<RoundEndPacket> RoundEndEvent = new("OnRoundEnd", OnRoundEnd, CommonNetworkRoutes.HostToAll);

    public static int RoundCount { get; set; } = 0;
    public static float TimeBetweenRounds => GamemodeRoundManager.Settings.TimeBetweenRounds;
    
    private static bool _inRound;
    private static int _roundIndex;
    private static float _roundCooldown;
    
    private static bool TryGetGamemode([MaybeNullWhen(false)] out IGamemode gamemode)
    {
        if (!GamemodeManager.IsGamemodeStarted)
        {
            gamemode = null;
            return false;
        }
        
        gamemode = GamemodeManager.ActiveGamemode as IGamemode;
        return gamemode != null;
    }

    public static void StartRound(int index)
    {
        if (!NetworkInfo.IsHost)
            return;
        
        if (!GamemodeManager.IsGamemodeStarted)
            return;
        
        // Validate players
        
        foreach (var networkPlayer in NetworkPlayer.Players)
        {
            GamemodeCompatibilityChecker.ValidatePlayer(networkPlayer.PlayerID.SmallID);
        }
        
        // Start remotely
        
        RoundStartEvent.Call(new RoundStartPacket
        {
            Index = index
        });
    }
    
    public static void EndRound(ulong winningTeamId)
    {
        if (!NetworkInfo.IsHost)
            return;
        
        if (!GamemodeManager.IsGamemodeStarted)
            return;

        // Reduce by 1 to see if this is the last round
        var hasNextRound = _roundIndex >= RoundCount - 1;
        if (hasNextRound)
        {
            GamemodeManager.StopGamemode();
            return;
        }
       
        RoundEndEvent.Call(new RoundEndPacket
        {
            WinningTeamID = winningTeamId,
            HasNextRound = hasNextRound,
            TimeUntilNextRound = TimeBetweenRounds
        });
    }
    
    public static void OnLateJoin(PlayerID id)
    {
        if (!TryGetGamemode(out var gamemode))
            return;
        
        GamemodeCompatibilityChecker.ValidatePlayer(id);
        gamemode.OnLateJoin(id);
    }
    
    // Events
    
    private static void OnRoundStart(RoundStartPacket packet)
    {
        if (!TryGetGamemode(out var gamemode))
            return;

        _inRound = true;
        _roundCooldown = 0f;
        _roundIndex = packet.Index;
        
        if (RoundCount > 1)
        {
            Notifier.Send(new Notification
            {
                Title = "Round Start!",
                Message = $"Round: {_roundIndex + 1} / {RoundCount}",
                PopupLength = 4f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        }
        
        // Reset the player tracker
        PlayerDamageTracker.Reset();
        
        gamemode.StartRound(packet.Index);
    }

    private static void OnRoundEnd(RoundEndPacket packet)
    {
        if (!TryGetGamemode(out var gamemode))
            return;

        _inRound = false;
        _roundCooldown = packet.TimeUntilNextRound;

        if (packet.HasNextRound)
        {
            Notifier.Send(new Notification
            {
                Title = "Cooldown",
                Message = $"The next round will start in {MathF.Round(_roundCooldown):N0} seconds",
                PopupLength = 4f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        }
        
        gamemode.EndRound(packet.WinningTeamID);
    }

    public static void Update(float delta)
    {
        if (!NetworkInfo.IsHost)
            return;
        if (!GamemodeManager.IsGamemodeStarted)
            return;
        if (_inRound)
            return;
        
        if (_roundCooldown <= 0f)
            return;

        _roundCooldown -= delta;
        
        if (_roundCooldown > 0f)
            return;
        
        StartRound(_roundIndex + 1);
    }
}