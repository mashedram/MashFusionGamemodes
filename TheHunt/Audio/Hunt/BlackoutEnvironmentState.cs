using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Blackout;
using TheHunt.Audio.Effectors.Finally;
using TheHunt.Audio.Effectors.Weather;
using TheHunt.Phase;
using TheHunt.Scene;

namespace TheHunt.Audio.Hunt;

public class BlackoutEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public BlackoutEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new BlackoutMusicEffector(),
        new WeatherEffector()
    })
    {
    }

    public override int Priority => 900;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsBlackout();
    }
}