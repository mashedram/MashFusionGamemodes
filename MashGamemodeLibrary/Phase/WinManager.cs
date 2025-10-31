using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase.Rounds;
using Team = MashGamemodeLibrary.Player.Team.Team;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

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

public static class WinManager
{
    private static readonly RemoteEvent<WinPacket> WinEvent = new(OnWinEvent, CommonNetworkRoutes.HostToAll);
    
    public static void Win<T>() where T : Team
    {
        Executor.RunIfHost(() =>
        {
            var id = TeamManager.Registry.CreateID<T>();
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
        var localTeam = TeamManager.GetLocalTeamID();
        if (localTeam == packet.TeamID)
            Notifier.Send(new Notification
            {
                Title = "You won!",
                Message = "Game over",
                ShowPopup = true,
                SaveToMenu = false,
                Type = NotificationType.SUCCESS,
                PopupLength = 5f
            });
        else
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