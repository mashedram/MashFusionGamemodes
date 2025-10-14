using Clockhunt.Audio.Effectors;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;

namespace Clockhunt.Audio;

public class ChaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    public ChaseEnvironmentState() : base(new EnvironmentEffector<ClockhuntMusicContext>[]
    {
        new ChaseMusicEffector()
    })
    {
    }

    public override int Priority => 500;
    public override int Layer => 1;

    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsChasing;
    }
}