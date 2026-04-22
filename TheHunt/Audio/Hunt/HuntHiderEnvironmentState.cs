using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Hunt;
using TheHunt.Audio.Effectors.Weather;
using TheHunt.Phase;

namespace TheHunt.Audio.Hunt;

internal class HuntHiderEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public HuntHiderEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new HuntHiderMusicEffector(),
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