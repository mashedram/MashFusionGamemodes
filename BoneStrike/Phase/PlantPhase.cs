using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike.Phase;

public class PlantPhase : GamePhase
{
    public override string Name => "Plant Phase";

    public override PhaseIdentifier GetNextPhase()
    {
        if (ElapsedTime < 10f) return PhaseIdentifier.Empty();
        
        return PhaseIdentifier.Of<DefusePhase>();
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            TeamManager.AssignAllRandom();
            
            
        });
    }
}