using Clockhunt.Config;
using Clockhunt.Nightmare;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Spectating;
using MelonLoader;

namespace Clockhunt.Game;

public enum GameTeam
{
    Nightmares,
    Survivors
}

internal class SetLivesPacket : INetSerializable
{
    public int NewLives;
    public bool ShouldDisplayMessage;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NewLives);
        serializer.SerializeValue(ref ShouldDisplayMessage);
    }
}

internal class OnGameWinPacket : INetSerializable
{
    public GameTeam WinningGameTeam;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref WinningGameTeam);
    }
}

public class WinStateManager
{
    private static GameTeam _localGameTeam;
    private static readonly RemoteEvent<OnGameWinPacket> OnGameWinEvent = new(OnGameWin);
    private static readonly RemoteEvent<SetLivesPacket> OnLivesChangedEvent = new(OnLivesChanged, false);
    private static int _lives = 3;
    public static int Lives => _lives;
    
    public static GameTeam LocalGameTeam => _localGameTeam;
    
    public static void SetLocalTeam(GameTeam gameTeam)
    {
        _localGameTeam = gameTeam;
    }

    public static void SetLives(int lives, bool shouldDisplayMessage)
    {
        if (!NetworkInfo.IsHost)
            return;
        
        _lives = lives;
        
        OnLivesChangedEvent.Call(new SetLivesPacket()
        {
            NewLives = _lives,
            ShouldDisplayMessage = shouldDisplayMessage
        });
    }
    
    public static void PlayerDied(PlayerID playerID)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Warning("Only the host can call player deaths!");
            return;
        }
        
        // Ignore if the player is a nightmare, they don't have lives
        if (NightmareManager.IsNightmare(playerID))
            return;
        
        _lives = Math.Max(0, _lives - 1);
        
        OnLivesChangedEvent.Call(new SetLivesPacket()
        {
            NewLives = _lives,
            ShouldDisplayMessage = true
        });
        
        // TODO: Custom message when lives hit 0
        if (_lives > 0) return;

        if (ClockhuntConfig.IsSpectatingEnabled)
        {
            var aliveSurvivorCount = NetworkPlayer.Players.Count(e => !e.PlayerID.IsSpectating() && !NightmareManager.IsNightmare(e.PlayerID));
            if (aliveSurvivorCount <= 1)
            {
                // Last survivor died, nightmares win
                ForceWin(GameTeam.Nightmares);
                return;
            }
            
            playerID.SetSpectating(true);
        }
        else
        {
            // No spectating, just end the game when lives hit 0
            ForceWin(GameTeam.Nightmares);
        }
    }
    
    public static void ForceWin(GameTeam winningTeam)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Warning("Only the host can call game wins!");
            return;
        }
        
        OnGameWinEvent.Call(new OnGameWinPacket()
        {
            WinningGameTeam = winningTeam
        });
        
        GamemodeManager.StopGamemode();
    }
    
    // Remote Events
    
    private static void OnLivesChanged(SetLivesPacket packet)
    {
        _lives = packet.NewLives;
        
        if (!packet.ShouldDisplayMessage)
            return;
        
        Notifier.Send(new Notification
        {
            Title = "Player Died",
            Message = $"There are now {packet.NewLives} lives remaining.",
            PopupLength = 3f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.ERROR
        });
    }

    private static void OnGameWin(OnGameWinPacket packet)
    {
        var winningTeam = packet.WinningGameTeam;
        var isWinner = winningTeam == _localGameTeam;
        
        Notifier.Send(new Notification
        {
            Title = "Game Over",
            Message = isWinner ? "You Win!" : "You Lose!",
            PopupLength = 5f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = isWinner ? NotificationType.SUCCESS : NotificationType.ERROR
        });
    }
}