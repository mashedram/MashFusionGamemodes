using MashGamemodeLibrary.Player.Team;

namespace MashGamemodeLibrary.Context.Helper;

public interface IRoundEndable
{
    void EndRound(ulong winnerTeamId);
}