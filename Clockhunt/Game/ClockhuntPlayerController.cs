using Clockhunt.Config;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Util;
using UnityEngine;
using Timer = MashGamemodeLibrary.Util.Timer;

namespace Clockhunt.Game;

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

    public int? GetSize()
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

internal class LivesChangedPacket : INetSerializable
{
    public int Lives;

    public int? GetSize()
    {
        return sizeof(int);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Lives);
    }
}

public class ClockhuntPlayerController : PlayerController
{
    private static readonly RemoteEvent<LivesChangedPacket> LivesChangedEvent = new(OnLivesChanged, CommonNetworkRoutes.HostToAll);
    private static readonly SyncedVariable<int> MaxLives = new("MaxLives", new IntEncoder(), Clockhunt.Config.MaxLives);
    
    private static readonly RemoteEvent<EscapeUpdatePacket> EscapeUpdateEvent = new(OnEscapeUpdate, CommonNetworkRoutes.HostToAll);
    private static readonly RemoteEvent<EscapeAvailablePacket> EscapeAvailableEvent = new(OnEscapeAvailable, CommonNetworkRoutes.HostToAll);

    private const float EscapeTime = 30f;
    
    private int _lives = 3;
    private Vector3? _escapePoint;
    private Timer _timer;

    static ClockhuntPlayerController()
    {
        ConfigManager.OnConfigChanged += config =>
        {
            if (config is not ClockhuntConfig clockhuntConfig)
                return;
            
            MaxLives.Value = clockhuntConfig.MaxLives;
        };
    }

    public ClockhuntPlayerController()
    {
        _timer = new Timer(EscapeTime, 
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

    private static int CountSurvivors()
    {
        return NetworkPlayer.Players.Count(e =>
            !e.PlayerID.IsSpectating() && !NightmareManager.IsNightmare(e.PlayerID));
    }

    private bool OnFinalLiveLost()
    {
        if (Clockhunt.Config.IsSpectatingEnabled)
        {
            var aliveSurvivorCount = CountSurvivors();
            if (aliveSurvivorCount <= 1 && !Clockhunt.Config.DebugForceSpectate)
            {
                // Last survivor died, nightmares win
                WinManager.Win<NightmareTeam>();
                return true;
            }

            Owner.PlayerID.SetSpectating(true);
            return false;
        }

        // No spectating, just end the game when lives hit 0
        WinManager.Win<NightmareTeam>();
        return true;
    }

    private void OnDeath()
    {
        if (NightmareManager.IsNightmare(Owner.PlayerID))
            return;

        if (Owner.PlayerID.IsSpectating())
            return;

        // -1 For ignoring
        _lives = Math.Max(-1, _lives - 1);
        
        LivesChangedEvent.CallFor(Owner.PlayerID, new LivesChangedPacket
        {
            Lives = _lives
        });
        
        switch (_lives)
        {
            case > 0:
            case 0 when OnFinalLiveLost():
                return;
        }
    }

    private bool IsInEscapeDistance()
    {
        if (!_escapePoint.HasValue) return false;

        var distance = Vector3.Distance(_escapePoint.Value, Owner.RigRefs.Head.position);

        return distance <= Clockhunt.Config.EscapeDistance;
    }
    
    public override void OnAttach()
    {
        MaxLives.OnValueChanged += OnMaxLivesChanged;
    }

    public override void OnDetach()
    {
        MaxLives.OnValueChanged -= OnMaxLivesChanged;
    }

    public override void OnUpdate(float delta)
    {
        if (!GamePhaseManager.IsPhase<EscapePhase>()) return;
        
        if (!IsInEscapeDistance())
        {
            _timer.Reset();
            return;
        }

        _timer.Update(delta);
    }

    public override void OnPlayerAction(PlayerActionType action, PlayerID otherPlayer)
    {
        if (action != PlayerActionType.DEATH) return;

        OnDeath();
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
    
    // Events

    private void OnMaxLivesChanged(int maxLives)
    {
        if (GamePhaseManager.IsPhase<HidePhase>()) return;
        
        // We are in hunt mode
        if (_lives <= maxLives)
            return;

        _lives = maxLives;
    }
    
    private static void OnEscapeAvailable(EscapeAvailablePacket packet)
    {
        if (!GamePhaseManager.IsPhase<EscapePhase>())
            return;
        
        if (!packet.EscapeAvailable)
            return;
        
        MarkerManager.SetMarker(packet.Position);
    }

    private static void OnLivesChanged(LivesChangedPacket packet)
    {
        var lives = packet.Lives;
        
        switch (lives)
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
                    Message = $"You have {lives - 1} lives remaining.",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
        }
    }
    
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
    
    // External

    public void ResetLives()
    {
        _lives = MaxLives;
    }

    public void SetLives(int lives)
    {
        _lives = lives;
    }
    
    public void SetEscapePoint(Vector3? escapePoint)
    {
        _escapePoint = escapePoint;
        
        EscapeAvailableEvent.Call(new EscapeAvailablePacket
        {
            EscapeAvailable = escapePoint.HasValue,
            Position = escapePoint.GetValueOrDefault()
        });
    }
}