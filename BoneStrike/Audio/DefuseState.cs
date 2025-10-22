using BoneStrike.Audio.Effectors;
using BoneStrike.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Phase;

namespace BoneStrike.Audio;

public class DefuseState : EnvironmentState<EnvironmentContext>
{

    public DefuseState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new DefuseMusicEffector()
    })
    {
    }

    public override int Priority => 5;
    
    public override bool CanPlay(EnvironmentContext context)
    {
        return GamePhaseManager.IsPhase<DefusePhase>();
    }
}