using LabFusion.Player;
using MashGamemodeLibrary.Phase.Rounds;
using MashGamemodeLibrary.Player.Team;

namespace MashGamemodeLibrary.Context.Helper;

public interface IGamemode
{
    void StartRound(int index);
    void EndRound(ulong winnerTeamId);
    void OnLateJoin(PlayerID playerID);
}