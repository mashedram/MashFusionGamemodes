using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Finally;
using TheHunt.Phase;

namespace TheHunt.Audio.Hunt;

public class FinallyEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public FinallyEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new FinallyMusicEffector(),
    })
    {
    }

    public override int Priority => 1000;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsPhase<FinallyPhase>();
    }
}