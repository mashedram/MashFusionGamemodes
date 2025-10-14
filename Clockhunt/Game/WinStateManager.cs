using Clockhunt.Config;
using Clockhunt.Nightmare;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Spectating;
using MelonLoader;

namespace Clockhunt.Game;

public enum GameTeam
{
    Nightmares,
    Survivors
}

internal class OverwriteLivesPacket : INetSerializable
{
    public int NewLives;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NewLives);
    }
}

internal class PlayerFinalDeathPacket : INetSerializable
{
    public byte PlayerID;

    public PlayerFinalDeathPacket()
    {
    }

    public PlayerFinalDeathPacket(PlayerID playerID)
    {
        PlayerID = playerID;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
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
    private static readonly RemoteEvent<OnGameWinPacket> OnGameWinEvent = new(OnGameWin, true);
    private static readonly RemoteEvent<OverwriteLivesPacket> OverwriteLivesEvent = new(OnOverwriteLives, true);

    private static readonly RemoteEvent<PlayerFinalDeathPacket> PlayerFinalDeathEvent =
        new(OnPlayerFinalDeath, true, CommonNetworkRoutes.ClientToHost);

    public static int Lives { get; private set; } = 3;

    public static GameTeam LocalGameTeam { get; private set; }

    public static int CountSurvivors()
    {
        return NetworkPlayer.Players.Count(e =>
            !e.PlayerID.IsSpectating() && !NightmareManager.IsNightmare(e.PlayerID));
    }

    public static void SetLocalTeam(GameTeam gameTeam)
    {
        LocalGameTeam = gameTeam;
    }

    public static void OverwriteLives(int lives)
    {
        if (!NetworkInfo.IsHost)
            return;

        OverwriteLivesEvent.Call(new OverwriteLivesPacket
        {
            NewLives = lives
        });
    }

    public static void PlayerDied(PlayerID playerID)
    {
        if (!playerID.IsMe)
            return;

        // Ignore if the player is a nightmare, they don't have lives
        if (NightmareManager.IsNightmare(playerID))
            return;

        if (SpectatorManager.IsPlayerSpectating(playerID))
            return;

        Lives = Math.Max(0, Lives - 1);

        switch (Lives)
        {
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
                    Message = $"You have {Lives - 1} lives remaining.",
                    PopupLength = 3f,
                    SaveToMenu = false,
                    ShowPopup = true,
                    Type = NotificationType.ERROR
                });
                break;
        }

        // TODO: Custom message when lives hit 0
        if (Lives > 0) return;

        Notifier.Send(new Notification
        {
            Title = "Game over",
            Message = "Wait till the next round starts.",
            PopupLength = 10f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.ERROR
        });

        PlayerFinalDeathEvent.CallFor(PlayerIDManager.GetHostID(), new PlayerFinalDeathPacket(playerID));
    }

    public static void ForceWin(GameTeam winningTeam)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Warning("Only the host can call game wins!");
            return;
        }

        OnGameWinEvent.Call(new OnGameWinPacket
        {
            WinningGameTeam = winningTeam
        });

        GamemodeManager.StopGamemode();
    }

    // Remote Events

    private static void OnOverwriteLives(OverwriteLivesPacket packet)
    {
        Lives = packet.NewLives;
    }

    private static void OnPlayerFinalDeath(PlayerFinalDeathPacket packet)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Warning("Only the host can call player final deaths!");
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(packet.PlayerID, out var player))
        {
            MelonLogger.Error("Player not found for final death packet!");
            return;
        }

        var playerID = player.PlayerID;

        if (ClockhuntConfig.IsSpectatingEnabled)
        {
            var aliveSurvivorCount = CountSurvivors();
            if (aliveSurvivorCount <= 1 && !ClockhuntConfig.DebugForceSpectate)
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

    private static void OnGameWin(OnGameWinPacket packet)
    {
        var winningTeam = packet.WinningGameTeam;
        var isWinner = winningTeam == LocalGameTeam;

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