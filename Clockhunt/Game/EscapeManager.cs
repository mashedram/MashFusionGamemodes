using Clockhunt.Config;
using Clockhunt.Entities.Tags;
using Clockhunt.Nightmare;
using Clockhunt.Vision;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using UnityEngine;

namespace Clockhunt.Game;

internal class OnEscapePointActivatedPacket : INetSerializable
{
    public Vector3 EscapePoint;

    public int? GetSize()
    {
        return sizeof(float) * 3;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EscapePoint);
    }
}

internal class OnEscapeRequestPacket : INetSerializable
{
    public byte PlayerID;

    public int? GetSize()
    {
        return 1;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
    }
}

public static class EscapeManager
{
    private static readonly List<Vector3> EscapePoints = new();

    private static readonly RemoteEvent<OnEscapePointActivatedPacket> OnEscapePointActivatedEvent =
        new(OnEscapePointActivated, true);

    private static readonly RemoteEvent<OnEscapeRequestPacket> OnEscapeRequestEvent =
        new(OnEscapeRequest, false, CommonNetworkRoutes.ClientToHost);

    private static bool _isEscaping;
    private static bool _hasEscaped;
    private static float _localEscapeTime;

    public static Vector3 ActiveEscapePoint { get; private set; } = Vector3.zero;

    public static void ActivateRandomEscapePoint()
    {
        Executor.RunIfHost(() =>
        {
            OnEscapePointActivatedEvent.Call(new OnEscapePointActivatedPacket
            {
                EscapePoint = EscapePoints.GetRandom()
            });

            var context = Clockhunt.Context;
            var name = context.EscapeAudioPlayer.GetRandomAudioName();
            context.EscapeAudioPlayer.Play(name, ActiveEscapePoint);
        });
    }

    public static void CollectEscapePoints()
    {
        Executor.RunIfHost(() =>
        {
            EscapePoints.Clear();

            foreach (var networkEntity in EntityTagManager.GetAllWithTag<ClockMarker>())
            {
                var position = networkEntity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity?.transform.position;
                if (position == null)
                    continue;

                EscapePoints.Add(position.Value);
            }
        });
    }

    public static void Update(float delta)
    {
        var context = Clockhunt.Context;
        var localPlayer = context.LocalPlayer;

        if (NightmareManager.IsNightmare(localPlayer.PlayerID)) return;

        var distance = Vector3.Distance(localPlayer.RigRefs.Head.position, ActiveEscapePoint);

        if (distance > ClockhuntConfig.EscapeDistance)
        {
            _localEscapeTime = 0f;
            if (_isEscaping)
                Notifier.Send(new Notification
                {
                    Title = "Too Far!",
                    Message = "You have left the escape zone. Return to the area to continue escaping.",
                    PopupLength = 2f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.WARNING
                });
            _isEscaping = false;
            return;
        }

        if (!_isEscaping)
        {
            var remainingTime = Math.Floor(ClockhuntConfig.EscapeDuration - _localEscapeTime);
            Notifier.Send(new Notification
            {
                Title = "Stay Here!",
                Message = $"You are in the escape zone! Stay here for {remainingTime} more seconds to escape.",
                PopupLength = 2f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });

            _isEscaping = true;
        }

        _localEscapeTime += delta;
        if (_localEscapeTime < ClockhuntConfig.EscapeDuration || _hasEscaped) return;
        _hasEscaped = true;

        OnEscapeRequestEvent.CallFor(context.HostPlayer.PlayerID, new OnEscapeRequestPacket
        {
            PlayerID = context.LocalPlayer.PlayerID.SmallID
        });

        Notifier.Send(new Notification
        {
            Title = "You Escaped!",
            Message = "You have successfully escaped the area.",
            PopupLength = 2f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.SUCCESS
        });
    }

    // Remote Events

    private static void OnEscapePointActivated(OnEscapePointActivatedPacket packet)
    {
        _isEscaping = false;
        _hasEscaped = false;
        _localEscapeTime = 0.0f;
        ActiveEscapePoint = packet.EscapePoint;

        MarkerManager.SetMarker(ActiveEscapePoint);
    }

    private static void OnEscapeRequest(OnEscapeRequestPacket requestPacket)
    {
        Executor.RunIfHost(() => { WinStateManager.ForceWin(GameTeam.Survivors); });
    }
}