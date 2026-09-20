using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Phase;

internal class WinPacket : INetSerializable
{
    public ulong TeamID;

    public int? GetSize()
    {
        return sizeof(ulong);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref TeamID);
    }
}

[RequireStaticConstructor]
public static class WinManager
{
    private static readonly RemoteEvent<WinPacket> WinEvent = new(OnWinEvent, CommonNetworkRoutes.HostToAll);

    static WinManager()
    {
        PlayerStatisticsTracker.Register(TeamStatisticKeys.RoundsWon, v => v * 50);
    }

    public static void Win(NetworkPlayer? player)
    {
        if (player == null)
            return;
        
        Win(player.PlayerID);
    }

    public static void Win(PlayerID? playerID)
    {
        if (playerID == null)
            return;
        
        var team = LogicTeamManager.GetPlayerTeam(playerID);
        if (team == null)
            return;
        
        Win(team.GetType());
    }

    public static void Win<T>() where T : LogicTeam
    {
        Win(typeof(T));
    }

    public static void Win(Type teamType)
    {
        Executor.RunIfHost(() =>
        {
            if (!InternalGamemodeManager.InRound)
                return;

            var id = LogicTeamManager.Registry.CreateID(teamType);
            WinEvent.Call(new WinPacket
            {
                TeamID = id
            });

            InternalGamemodeManager.EndRound(id);
        }, "Sending win state");
    }

    // Handelers

    private static void OnWinEvent(WinPacket packet)
    {
        var localTeam = LogicTeamManager.GetLocalTeamID();
        if (localTeam == packet.TeamID)
        {
            PlayerStatisticsTracker.Increment(TeamStatisticKeys.RoundsWon);

            Notifier.Send(new Notification
            {
                Title = "You won!",
                Message = "Game over",
                ShowPopup = true,
                SaveToMenu = false,
                Type = NotificationType.SUCCESS,
                PopupLength = 5f
            });
        }
        else
        {
            Notifier.Send(new Notification
            {
                Title = "You lost!",
                Message = "Game over",
                ShowPopup = true,
                SaveToMenu = false,
                Type = NotificationType.ERROR,
                PopupLength = 5f
            });
        }
    }
}