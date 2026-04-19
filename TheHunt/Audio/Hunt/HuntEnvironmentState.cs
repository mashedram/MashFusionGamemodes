using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Hunt;
using TheHunt.Audio.Effectors.Weather;
using TheHunt.Phase;

namespace TheHunt.Audio.Hunt;

public class HuntEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public HuntEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new HuntMusicEffector(),
        new WeatherEffector()
    })
    {
    }

    public override int Priority => 100;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsPhase<HuntPhase>();
    }
}