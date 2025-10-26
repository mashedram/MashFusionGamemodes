using Clockhunt.Game.Teams;
using Clockhunt.Phase;
using Clockhunt.Vision;
using LabFusion.Network.Serialization;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Entities.Tagging.Player;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util.Timer;
using UnityEngine;

namespace Clockhunt.Game.Player;

internal struct EscapeUpdatePacket : INetSerializable
{
    public bool IsEscaping;
    public float Time;

    public readonly int? GetSize()
    {
        return sizeof(bool) + (IsEscaping ? sizeof(float) : 0);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref IsEscaping);
        if (IsEscaping)
            serializer.SerializeValue(ref Time);
    }
}

internal struct EscapeAvailablePacket : INetSerializable
{
    public bool EscapeAvailable;
    public Vector3 Position;

    public readonly int? GetSize()
    {
        return sizeof(bool) + (EscapeAvailable ? sizeof(float) * 3 : 0);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EscapeAvailable);
        if (EscapeAvailable)
            serializer.SerializeValue(ref Position);
    }
}

public class PlayerEscapeTag : PlayerTag, ITagUpdate
{
    // Config
    private const float EscapeTime = 30f;
    public static Vector3? EscapePosition = null;
    
    // Remotes
    private static readonly RemoteEvent<EscapeUpdatePacket> EscapeUpdateEvent = new(OnEscapeUpdate, CommonNetworkRoutes.HostToAll);
    private static readonly RemoteEvent<EscapeAvailablePacket> EscapeAvailableEvent = new(OnEscapeAvailable, CommonNetworkRoutes.HostToAll);

    private readonly MarkableTimer _timer;

    public PlayerEscapeTag()
    {
        _timer = new MarkableTimer(EscapeTime, 
            new TimeMarker(MarkerType.Interval, 10f, timer => CallEscapeTimer(timer)),
            new TimeMarker(MarkerType.AfterStart, 0f, timer => CallEscapeTimer(timer))
        );

        _timer.OnReset += () =>
        {
            CallEscapeTimer(null);
        };
        _timer.OnTimeout += timer =>
        {
            CallEscapeTimer(timer);
            WinManager.Win<SurvivorTeam>();
        };
    }
    
    private bool IsInEscapeDistance()
    {
        if (!EscapePosition.HasValue) return false;

        var distance = Vector3.Distance(EscapePosition.Value, Owner.RigRefs.Head.position);

        return distance <= Clockhunt.Config.EscapeDistance;
    }
        
    public void Update(float delta)
    {
        if (EscapePosition == null)
            return;
        
        if (!IsInEscapeDistance())
        {
            _timer.Reset();
            return;
        }

        _timer.Update(delta);
    }
    
    private void CallEscapeTimer(float? time)
    {
        if (!time.HasValue)
        {
            EscapeUpdateEvent.CallFor(Owner.PlayerID, new EscapeUpdatePacket
            {
                IsEscaping = false,
                Time = 0f
            });
            return;
        }
        
        EscapeUpdateEvent.CallFor(Owner.PlayerID, new EscapeUpdatePacket
        {
            IsEscaping = true,
            Time = time.Value
        });
    }
    
    // Remote Listeners
    
    private static void OnEscapeUpdate(EscapeUpdatePacket packet)
    {
        if (!packet.IsEscaping)
        {
            Notifier.Send(new Notification
            {
                Title = "Too Far!",
                Message = "You have left the escape zone. Return to the area to continue escaping.",
                PopupLength = 2f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.WARNING
            });
            return;
        }

        if (packet.Time > 29.5f)
        {
            Notifier.Send(new Notification
            {
                Title = "You Escaped!",
                Message = "You have successfully escaped the area.",
                PopupLength = 2f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.SUCCESS
            });
            return;
        }
        
        Notifier.Send(new Notification
        {
            Title = "Stay Here!",
            Message = $"You are in the escape zone! Stay here for {EscapeTime - packet.Time} more seconds to escape.",
            PopupLength = 2f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.INFORMATION
        });
    }
    
    private static void OnEscapeAvailable(EscapeAvailablePacket packet)
    {
        if (!GamePhaseManager.IsPhase<EscapePhase>())
            return;
        
        if (!packet.EscapeAvailable)
            return;
        
        MarkerManager.SetMarker(packet.Position);
    }
}