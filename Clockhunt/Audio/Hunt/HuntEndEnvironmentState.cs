using Clockhunt.Audio.Effectors;
using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;

namespace Clockhunt.Audio.Hunt;

public class HuntEndEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    public HuntEndEnvironmentState() : base(new EnvironmentEffector<ClockhuntMusicContext>[]
    {
        new HuntEndMusicEffector(),
        new Weather3Effector()
    })
    {
    }

    public override int Priority => 100;

    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>() || context.IsPhase<EscapePhase>();
    }
}