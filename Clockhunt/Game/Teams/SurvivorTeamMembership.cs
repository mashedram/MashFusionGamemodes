using Clockhunt.Config;
using Clockhunt.Game.Player;
using Clockhunt.Phase;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;

namespace Clockhunt.Game.Teams;

public class SurvivorTeam : Team
{
    public override string Name => "Survivors";

    protected override void OnAssigned()
    {
        Executor.RunIfHost(() =>
        {
            Owner.AddTag(new LimitedRespawnComponent(Clockhunt.Config.MaxRespawns));
            Owner.AddTag(new PlayerHandTimerTag());
        });
        
        Executor.RunIfMe(Owner.PlayerID,() =>
        {
            PlayerStatManager.SetStats(Clockhunt.Config.DefaultStats);
        });
    }

    protected override void OnRemoved()
    {
        Executor.RunIfHost(() =>
        {
            Owner.RemoveTag<LimitedRespawnComponent>();
            Owner.RemoveTag<PlayerHandTimerTag>();
            Owner.RemoveTag<PlayerEscapeTag>();
        });
    }

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleTag(phase is EscapePhase, () => new PlayerEscapeTag());
        });
    }
}