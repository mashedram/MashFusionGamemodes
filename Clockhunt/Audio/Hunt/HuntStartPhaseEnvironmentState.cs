using Clockhunt.Phase;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background.Music;

namespace Clockhunt.Audio;

public class HuntStartPhaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.CasualLoop",
        "Sylvie.SignalisMonodiscs.MonoDisc.Eulenlieder",
        "Sylvie.SignalisMonodiscs.MonoDisc.DieToteninselEmptiness"
    }));

    protected override string[] WeatherSpawnables => new[]
    {
        "FirePura.BoneWeater.Spawnable.Night",
        "FirePura.BoneWeater.Spawnable.Rainy"
    };

    public override int Priority => 300;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>() && context.PhaseProgress < 0.33f;
    }
}