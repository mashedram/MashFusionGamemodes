using Clockhunt.Config;
using Clockhunt.Game.Player;
using Clockhunt.Phase;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;

namespace Clockhunt.Game.Teams;

public class SurvivorTeam : LogicTeam
{
    public override string Name => "Survivors";

    protected override void OnAssigned()
    {
        Executor.RunIfHost(() =>
        {
            Owner.AddComponent(new LimitedRespawnComponent(Clockhunt.Config.MaxRespawns));
            Owner.AddComponent(new PlayerHandTimerTag());
        });

        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            AvatarStatManager.SetStats(Clockhunt.Config.DefaultStats);
        });
    }

    protected override void OnRemoved()
    {
        Executor.RunIfHost(() =>
        {
            Owner.RemoveComponent<LimitedRespawnComponent>();
            Owner.RemoveComponent<PlayerHandTimerTag>();
            Owner.RemoveComponent<PlayerEscapeTag>();
        });
    }

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleComponent(phase is EscapePhase, () => new PlayerEscapeTag());
        });
    }
}