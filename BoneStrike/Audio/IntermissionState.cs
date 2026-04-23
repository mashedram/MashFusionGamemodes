using BoneStrike.Audio.Effectors;
using BoneStrike.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Phase;

namespace BoneStrike.Audio;

public class IntermissionState : EnvironmentState<EnvironmentContext>
{

    public IntermissionState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new IntermissionMusicEffector()
    })
    {
    }

    public override int Priority => 15;

    public override bool CanPlay(EnvironmentContext context)
    {
        return GamePhaseManager.IsPhase<TeamAssignmentPhase>();
    }
}