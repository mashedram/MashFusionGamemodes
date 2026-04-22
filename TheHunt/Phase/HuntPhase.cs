using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using TheHunt.Teams;

namespace TheHunt.Phase;

/// <summary>
/// The hunt begins
/// </summary>
public class HuntPhase : GamePhase
{
    private static readonly SyncedVariable<float> ExtendTime = new("HuntPhase.ExtendTime", new FloatEncoder(), 0f);
    public override string Name => "Hunt";
    public override float Duration => Gamemode.TheHunt.Config.HuntDuration + ExtendTime;
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();
        
        if (Gamemode.TheHunt.Config.FinallyAlwaysPlays)
            return PhaseIdentifier.Of<FinallyPhase>();

        WinManager.Win<HiderTeam>();
        
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            ExtendTime.Value = 0f;
        });
        
        Gamemode.TheHunt.Context.RandomAmbienceAudioPlayer.Start();
    }

    protected override void OnPhaseExit()
    {
        Gamemode.TheHunt.Context.RandomAmbienceAudioPlayer.Stop();
    }
    
    public static void Extend(float time)
    {
        Executor.RunIfHost(() =>
        {
            ExtendTime.Value += time;
        });
    }
}