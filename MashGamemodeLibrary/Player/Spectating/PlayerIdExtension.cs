using LabFusion.Player;

namespace MashGamemodeLibrary.Player.Spectating;

public static class PlayerIdExtension
{
    public static bool IsSpectating(this PlayerID playerId)
    {
        return SpectatorManager.IsSpectating(playerId);
    }

    public static bool IsSpectatingAndHidden(this PlayerID playerID)
    {
        return SpectatorManager.IsHidden(playerID);
    }
}