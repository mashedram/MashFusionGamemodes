using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Chase;

namespace TheHunt.Audio;

public class ChaseEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public ChaseEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new ChaseMusicEffector()
    })
    {
    }

    public override int Priority => 500;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsChasing;
    }
}