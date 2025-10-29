using MashGamemodeLibrary.Player.Team;

namespace MashGamemodeLibrary.Context.Helper;

public interface IRoundBasedGamemode
{
    void StartRound();
    void EndRound(ulong winnerTeamId);
}