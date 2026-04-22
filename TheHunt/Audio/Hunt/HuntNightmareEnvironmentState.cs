using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Hunt;
using TheHunt.Audio.Effectors.Weather;
using TheHunt.Phase;

namespace TheHunt.Audio.Hunt;

internal class HuntNightmareEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public HuntNightmareEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new HuntNightmareMusicEffector(),
        new WeatherEffector()
    })
    {
    }

    public override int Priority => 120;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsPhase<HuntPhase>() && EnvironmentContext.IsLocalNightmare;
    }
}