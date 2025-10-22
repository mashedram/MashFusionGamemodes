using BoneStrike.Config;
using BoneStrike.Phase;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Util;
using Timer = System.Threading.Timer;

namespace BoneStrike.Player;

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

public class BoneStrikePlayerController : PlayerController
{
    private static readonly RemoteEvent<LivesChangedPacket> LivesChangedEvent = new(OnLivesChanged, CommonNetworkRoutes.HostToAll);
    private static readonly SyncedVariable<int> MaxRespawns = new("MaxRespawns", new IntEncoder(), BoneStrike.Config.MaxRespawns);

    private int _respawns;

    static BoneStrikePlayerController()
    {
        ConfigManager.OnConfigChanged += config =>
        {
            if (config is not BoneStrikeConfig clockhuntConfig)
                return;
            
            MaxRespawns.Value = clockhuntConfig.MaxRespawns;
        };
    }
    private void OnDeath()
    {
        if (GamePhaseManager.IsPhase<PlantPhase>())
            return;
        
        if (Owner.PlayerID.IsSpectating())
            return;

        // -1 For ignoring
        _respawns = Math.Max(0, _respawns - 1);
        
        LivesChangedEvent.CallFor(Owner.PlayerID, new LivesChangedPacket
        {
            Lives = _respawns
        });

        if (_respawns == 0)
        {
            Owner.PlayerID.SetSpectating(true);
        }
    }
    
    public override void OnAttach()
    {
        MaxRespawns.OnValueChanged += OnMaxRespawnsChanged;
        _respawns = MaxRespawns;
    }

    public override void OnDetach()
    {
        MaxRespawns.OnValueChanged -= OnMaxRespawnsChanged;
    }

    public override void OnPlayerAction(PlayerActionType action, PlayerID otherPlayer)
    {
        if (action != PlayerActionType.DEATH) return;

        OnDeath();
    }
    
    // Events

    private void OnMaxRespawnsChanged(int maxLives)
    {
        // We are in hunt mode
        if (_respawns <= maxLives)
            return;

        _respawns = maxLives;
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
    
    // External

    public void ResetLives()
    {
        _respawns = MaxRespawns;
    }

    public void SetLives(int lives)
    {
        _respawns = lives;
    }
}