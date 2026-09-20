using MashGamemodeLibrary.Phase;

namespace Chaos.Phase;

public class MatchPhase : GamePhase
{
    public override string Name { get; } = "Match";
    public override float Duration { get; } = 300f;
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        return PhaseIdentifier.Of<DowntimePhase>();
    }
}