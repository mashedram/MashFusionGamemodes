using LabFusion.Player;

namespace MashGamemodeLibrary.networking.Control;

public enum CatchupMoment
{
    Join,
    LevelLoad
}

public interface ICatchup
{
    CatchupMoment Moment { get; }
    // Called when a player joins the game and needs to catch up to the current state.
    // Only called on host
    void OnCatchup(PlayerID playerId);
}