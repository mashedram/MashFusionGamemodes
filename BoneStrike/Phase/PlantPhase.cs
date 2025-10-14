using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;

namespace BoneStrike.Phase;

public class PlantPhase : GamePhase
{
    public override string Name => "Plant Phase";
    public override float Duration => 20;

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() => { BoneStrikeContext.TeamManager.AssignToRandomTeams(); });
    }
}