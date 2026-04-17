using MashGamemodeLibrary.Phase;
using TheHunt.Teams;

namespace TheHunt.Phase;

/// <summary>
/// Last player standing, epic chase scene ensues
/// Also triggers on the last minute of the round
/// </summary>
public class FinallyPhase : GamePhase
{
    public override string Name => "Finally";
    public override float Duration => Gamemode.TheHunt.Config.FinallyDuration;
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        WinManager.Win<HiderTeam>();
        
        return PhaseIdentifier.Empty();
    }
}