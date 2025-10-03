using Clockhunt.Audio.Effectors;
using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;

namespace Clockhunt.Audio.Hunt;

public class HuntStartPhaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    public HuntStartPhaseEnvironmentState() : base(new EnvironmentEffector<ClockhuntMusicContext>[]
    {
        new HuntStartMusicEffector(),
        new Rain1WeatherEffector()
    })
    {
    }

    public override int Priority => 300;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>() && context.PhaseProgress < 0.33f;
    }
}