using LabFusion.Player;

namespace MashGamemodeLibrary.networking.Control;

public interface ICatchup
{
    // Called when a player joins the game and needs to catch up to the current state.
    // Only called on host
    void OnCatchup(PlayerID playerId);
}