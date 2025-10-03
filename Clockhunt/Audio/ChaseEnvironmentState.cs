using Clockhunt.Audio.Effectors;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
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
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsChasing;
    }
}