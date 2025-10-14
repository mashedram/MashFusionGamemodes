using MashGamemodeLibrary.Phase;

namespace BoneStrike.Phase;

public class DefusePhase : GamePhase
{
    public override string Name => "Defuse Phase";
    public override PhaseIdentifier GetNextPhase()
    {
        if (ElapsedTime > 120f)
        {
            // TODO: Make defenders win
        }
        
        return PhaseIdentifier.Empty();
    }
}