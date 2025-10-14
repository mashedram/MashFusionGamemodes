using Clockhunt.Audio.Effectors;
using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;

namespace Clockhunt.Audio.Hunt;

public class HuntMiddlePhaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    public HuntMiddlePhaseEnvironmentState() : base(new EnvironmentEffector<ClockhuntMusicContext>[]
    {
        new HuntMiddleMusicEffector(),
        new Weather2Effector()
    })
    {
    }

    public override int Priority => 200;

    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>() && context.PhaseProgress < 0.66f;
    }
}