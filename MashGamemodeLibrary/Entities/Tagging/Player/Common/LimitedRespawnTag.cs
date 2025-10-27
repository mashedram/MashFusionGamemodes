using Il2CppSLZ.Bonelab;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Entities.Tagging.Player.Base;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Spectating;

namespace MashGamemodeLibrary.Entities.Tagging.Player.Common;

internal class RespawnCountPacket : INetSerializable
{
    public int Respawns;

    public int? GetSize()
    {
        return sizeof(int);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Respawns);
    }
}

public class LimitedRespawnTag : PlayerTag, ITagRemoved, IPlayerActionTag
{
    private static readonly RemoteEvent<RespawnCountPacket> RespawnCountChangedEvent = new(OnRespawnsChanged, CommonNetworkRoutes.HostToAll);

    private Func<NetworkPlayer, int, bool>? _predicate;
    public int Respawns { get; private set; }

    public LimitedRespawnTag() {}
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="respawns"></param>
    /// <param name="predicate">Returns wether the life should subtract, params: The player, the new respawn count</param>
    public LimitedRespawnTag(int respawns, Func<NetworkPlayer, int, bool>? predicate = null)
    {
        Respawns = respawns;
        _predicate = predicate;
    }
    
    public void SetRespawns(int respawns)
    {
        Executor.RunIfHost(() =>
        {
            Respawns = respawns;
        });
    }
    
    public void OnRemoval(ushort entityID)
    {
        Executor.RunIfHost(() =>
        {
            Owner.PlayerID.SetSpectating(false);
        });
    }
    
    public void OnAction(PlayerActionType action, PlayerID otherPlayer)
    {
        Executor.RunIfHost(() =>
        {
            if (action != PlayerActionType.DEATH)
                return;
            if (Owner.PlayerID.IsSpectating())
                return;
            
            if (_predicate != null && !_predicate.Invoke(Owner, Respawns - 1))
                return;

            Respawns--;
            RespawnCountChangedEvent.CallFor(Owner.PlayerID, new RespawnCountPacket
            {
                Respawns = Respawns
            });
            
            if (Respawns >= 0)
                return;
            
            Owner.PlayerID.SetSpectating(true);
        });
    }
    
    private static void OnRespawnsChanged(RespawnCountPacket packet)
    {
        var respawns = packet.Respawns;
        
        switch (respawns)
        {
            case < 0: 
                Notifier.Send(new Notification
                {
                    Title = "Game over",
                    Message = "You are now spectating",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
            case 0:
                Notifier.Send(new Notification
                {
                    Title = "Player Died",
                    Message = "This is your last try.",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
            case > 0:
                Notifier.Send(new Notification
                {
                    Title = "Player Died",
                    Message = $"You have {respawns} respawns remaining.",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
        }
    }
}