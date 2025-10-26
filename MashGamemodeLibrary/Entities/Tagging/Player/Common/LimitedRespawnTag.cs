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
using MashGamemodeLibrary.Spectating;

namespace MashGamemodeLibrary.Entities.Tagging.Player.Common;

internal struct RespawnCountPacket : INetSerializable
{
    public int Respawns;

    public readonly int? GetSize()
    {
        return sizeof(int);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Respawns);
    }
}

public class LimitedRespawnTag : PlayerTag, INetSerializable, ITagRemoved, IPlayerActionTag
{
    private static readonly RemoteEvent<RespawnCountPacket> RespawnCountChangedEvent = new(OnRespawnsChanged, CommonNetworkRoutes.HostToAll);
    
    private int _respawns;
    private Func<NetworkPlayer, bool>? _predicate;
    
    public LimitedRespawnTag() {}
    
    public LimitedRespawnTag(int respawns, Func<NetworkPlayer, bool>? predicate = null)
    {
        _respawns = respawns;
        _predicate = predicate;
    }
    
    public void SetRespawns(int respawns)
    {
        Executor.RunIfHost(() =>
        {
            _respawns = respawns;
        });
    }
    
    public void OnRemoval(ushort entityID)
    {
        Owner.PlayerID.SetSpectating(false);
    }
    
    public void OnAction(PlayerActionType action, PlayerID otherPlayer)
    {
        Executor.RunIfHost(() =>
        {
            if (action != PlayerActionType.DEATH)
                return;
            if (Owner.PlayerID.IsSpectating())
                return;
            
            if (_predicate != null && !_predicate.Invoke(Owner))
                return;

            _respawns--;
            RespawnCountChangedEvent.CallFor(Owner.PlayerID, new RespawnCountPacket
            {
                Respawns = _respawns
            });
            
            if (_respawns >= 0)
                return;
            
            Owner.PlayerID.SetSpectating(true);
        });
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _respawns);
    }
    
    private static void OnRespawnsChanged(RespawnCountPacket packet)
    {
        var respawns = packet.Respawns;
        
        switch (respawns)
        {
            case 0: 
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
            case 1:
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
            case > 1:
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