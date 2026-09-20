using MashGamemodeLibrary.Phase;

namespace Chaos.Phase;

public class DowntimePhase : GamePhase
{
    public override string Name => "Downtime";
    public override float Duration => 30f;
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();
        
        return PhaseIdentifier.Of<MatchPhase>();
    }
}