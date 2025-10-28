using Clockhunt.Config;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;

namespace Clockhunt.Game.Teams;

public class SurvivorTeam : Team
{
    public override string Name => "Survivors";

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID,() =>
        {
            Owner.AddTag(new LimitedRespawnTag(Clockhunt.Config.MaxRespawns));
            PlayerStatManager.SetStats(Clockhunt.Config.DefaultStats);
        });
    }
}