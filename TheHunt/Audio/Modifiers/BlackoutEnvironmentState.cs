using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Blackout;
using TheHunt.Audio.Effectors.Weather;

namespace TheHunt.Audio.Modifiers;

public class BlackoutEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public BlackoutEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new BlackoutMusicEffector(),
        new WeatherEffector()
    })
    {
    }

    public override int Priority => 150;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsBlackout();
    }
}