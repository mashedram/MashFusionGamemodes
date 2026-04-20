using MashGamemodeLibrary.Phase;
using TheHunt.Teams;

namespace TheHunt.Phase;

/// <summary>
/// The hunt begins
/// </summary>
public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => Gamemode.TheHunt.Config.HuntDuration;
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Of<HuntPhase>();
        
        if (Gamemode.TheHunt.Config.FinallyAlwaysPlays)
            return PhaseIdentifier.Of<FinallyPhase>();

        WinManager.Win<HiderTeam>();
        
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        Gamemode.TheHunt.Context.RandomAmbienceAudioPlayer.Start();
    }

    protected override void OnPhaseExit()
    {
        Gamemode.TheHunt.Context.RandomAmbienceAudioPlayer.Stop();
    }
}