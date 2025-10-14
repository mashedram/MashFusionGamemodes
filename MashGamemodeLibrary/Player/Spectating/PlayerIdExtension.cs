using LabFusion.Player;

namespace MashGamemodeLibrary.Spectating;

public static class PlayerIdExtension
{
    public static bool IsSpectating(this PlayerID playerId)
    {
        return SpectatorManager.IsPlayerSpectating(playerId);
    }
}