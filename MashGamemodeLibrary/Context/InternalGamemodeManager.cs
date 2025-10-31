using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase.Rounds;

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
    public float TimeUntilNextRound;

    public int? GetSize()
    {
        return sizeof(ulong) + sizeof(float);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref WinningTeamID);
        serializer.SerializeValue(ref TimeUntilNextRound);
    }
}

public static class InternalGamemodeManager
{
    private static readonly RemoteEvent<RoundStartPacket> RoundStartEvent = new RemoteEvent<RoundStartPacket>("OnRoundStart", OnRoundStart, CommonNetworkRoutes.HostToAll);
    private static readonly RemoteEvent<RoundEndPacket> RoundEndEvent = new("OnRoundEnd", OnRoundEnd, CommonNetworkRoutes.HostToAll);

    public static int RoundCount { get; set; } = 0;
    public static float TimeBetweenRounds { get; set; } = 0.0f;
    
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

        if (_roundIndex >= RoundCount)
        {
            GamemodeManager.StopGamemode();
            return;
        }
       
        RoundEndEvent.Call(new RoundEndPacket
        {
            WinningTeamID = winningTeamId,
            TimeUntilNextRound = TimeBetweenRounds
        });
    }
    
    public static void OnLateJoin(PlayerID id)
    {
        if (!TryGetGamemode(out var gamemode))
            return;
        
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

        
        gamemode.StartRound(packet.Index);
    }

    private static void OnRoundEnd(RoundEndPacket packet)
    {
        if (!TryGetGamemode(out var gamemode))
            return;

        _inRound = false;
        _roundCooldown = packet.TimeUntilNextRound;
        
        Notifier.Send(new Notification
        {
            Title = "Cooldown",
            Message = $"The next round will start in {MathF.Round(_roundCooldown):N0} seconds",
            PopupLength = 4f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.INFORMATION
        });
        
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