using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background.Music;

namespace Clockhunt.Audio;

public class ChaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.RiotControl",
        "Sylvie.SignalisMonodiscs.MonoDisc.Kolibri",
        "Sylvie.SignalisMonodiscs.MonoDisc.IntensiveCare",
        "Sylvie.SignalisMonodiscs.MonoDisc.BecameWholeAgain",
        "Sylvie.SignalisMonodiscs.MonoDisc.Blockwart"
    }));

    protected override string[] WeatherSpawnables => Array.Empty<string>();

    public override int Priority => 500;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsChasing;
    }

    public override bool ShouldApplyWeatherEffects(ClockhuntMusicContext context)
    {
        return false;
    }
}