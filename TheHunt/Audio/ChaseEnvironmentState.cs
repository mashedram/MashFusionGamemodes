using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Chase;
using TheHunt.Audio.Effectors.Stinger;

namespace TheHunt.Audio;

public class ChaseEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public ChaseEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new ChaseMusicEffector(),
        new StingerAudioEffector()
    })
    {
    }

    public override int Priority => 500;
    public override int Layer => 1;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsChasing;
    }
}