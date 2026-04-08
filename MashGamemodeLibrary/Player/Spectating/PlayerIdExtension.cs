using LabFusion.Player;

namespace MashGamemodeLibrary.Player.Spectating;

public static class PlayerIdExtension
{
    public static bool IsSpectatingAndHidden(this PlayerID playerID)
    {
        return playerID.IsHidden();
    }
}