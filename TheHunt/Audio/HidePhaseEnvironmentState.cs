using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Hide;
using TheHunt.Phase;

namespace TheHunt.Audio;

public class HidePhaseEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public HidePhaseEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new HideMusicEffector()
    })
    {
    }

    public override int Priority => 10;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsPhase<HidePhase>();
    }
}