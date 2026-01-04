using Clockhunt.Game.Teams;
using Clockhunt.Phase;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util.Timer;
using UnityEngine;

namespace Clockhunt.Game.Player;

internal class EscapeUpdatePacket : INetSerializable
{
    public bool IsEscaping;
    public float Time;

    public int? GetSize()
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

public class PlayerEscapeTag : IComponentPlayerReady, IComponentUpdate, IComponentRemoved
{
    // Config
    private const float EscapeTime = 30f;
    public static readonly SyncedVariable<Vector3?> EscapePosition = new ("EscapePosition", new NullableValueEncoder<Vector3>(new Vector3Encoder()), null);
    
    // Remotes
    private static readonly RemoteEvent<EscapeUpdatePacket> EscapeUpdateEvent = new(OnEscapeUpdate, CommonNetworkRoutes.HostToAll);

    private NetworkPlayer _owner = null!;
    private readonly MarkableTimer _timer;

    static PlayerEscapeTag()
    {
        EscapePosition.OnValueChanged += OnEscapeAvailable;
    }

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
        if (!EscapePosition.Value.HasValue) return false;

        var distance = Vector3.Distance(EscapePosition.Value.Value, _owner.RigRefs.Head.position);

        return distance <= Clockhunt.Config.EscapeDistance;
    }
        
    public void Update(float delta)
    {
        if (!EscapePosition.Value.HasValue)
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
            EscapeUpdateEvent.CallFor(_owner.PlayerID, new EscapeUpdatePacket
            {
                IsEscaping = false,
                Time = 0f
            });
            return;
        }
        
        EscapeUpdateEvent.CallFor(_owner.PlayerID, new EscapeUpdatePacket
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

        if (packet.Time >= EscapeTime)
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
    
    private static void OnEscapeAvailable(Vector3? escape)
    {
        if (!GamePhaseManager.IsPhase<EscapePhase>())
            return;
        
        if (!escape.HasValue)
            return;
        
        MarkerManager.SetMarker(escape.Value);
    }
    
    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        _owner = networkPlayer;
    }

    public void OnRemoved(NetworkEntity networkEntity)
    {
        MarkerManager.ClearMarker();
    }
}