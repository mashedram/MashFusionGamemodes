using Clockhunt.Config;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
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
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Spectating;

namespace Clockhunt.Game;

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
    private static readonly IntSyncedVariable MaxLives = new("MaxLives", Clockhunt.Config.MaxLives);
    private static readonly Vector3SyncedVariable EscapePoint = new Vector3SyncedVariable()
    
    private int _lives = 3;

    static ClockhuntPlayerController()
    {
        ConfigManager.OnConfigChanged += config =>
        {
            if (config is ClockhuntConfig clockhuntConfig)
                MaxLives.Value = clockhuntConfig.MaxLives;
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
    
    public override void OnAttach()
    {
        MaxLives.OnValueChanged += OnMaxLivesChanged;
    }

    public override void OnDetach()
    {
        MaxLives.OnValueChanged -= OnMaxLivesChanged;
    }

    public override void OnPlayerAction(PlayerActionType action, PlayerID otherPlayer)
    {
        if (action != PlayerActionType.DEATH) return;

        OnDeath();
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

    public void ResetLives()
    {
        _lives = MaxLives;
    }

    public void SetLives(int lives)
    {
        _lives = lives;
    }
}