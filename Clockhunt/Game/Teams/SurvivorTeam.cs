using Clockhunt.Config;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;

namespace Clockhunt.Game.Teams;

public class SurvivorTeam : Team
{
    public override string Name => "Survivors";

    public override void OnAssigned(PlayerID player)
    {
        Executor.RunIfMe(player, () =>
        {
            PlayerStatManager.SetStats(ClockhuntConfig.DefaultStats);
        });
    }
}