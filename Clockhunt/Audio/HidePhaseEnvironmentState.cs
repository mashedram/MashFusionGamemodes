using Clockhunt.Phase;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Background.Music;
using MashGamemodeLibrary.Context;

namespace Clockhunt.Audio;

public class HidePhaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.3000Cycles",
        "Sylvie.SignalisMonodiscs.MonoDisc.FalkesTheme",
        "Sylvie.SignalisMonodiscs.MonoDisc.TrainRide",
        "Sylvie.SignalisMonodiscs.MonoDisc.Home"
    }));

    protected override string[] WeatherSpawnables => Array.Empty<string>();

    public override int Priority => 10;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HidePhase>();
    }
}