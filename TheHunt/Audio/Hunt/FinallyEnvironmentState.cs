using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Finally;
using TheHunt.Audio.Effectors.Weather;
using TheHunt.Phase;

namespace TheHunt.Audio.Hunt;

public class FinallyEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public FinallyEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new FinallyMusicEffector(),
        new WeatherEffector()
    })
    {
    }

    public override int Priority => 1000;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsPhase<FinallyPhase>();
    }
}