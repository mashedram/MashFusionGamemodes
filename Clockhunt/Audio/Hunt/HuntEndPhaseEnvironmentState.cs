using Clockhunt.Phase;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background.Music;

namespace Clockhunt.Audio;

public class HuntEndPhaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.TurnedAround",
        "Sylvie.SignalisMonodiscs.MonoDisc.Misremembered",
        "Sylvie.SignalisMonodiscs.MonoDisc.NearDarkbythePond"
    }));

    protected override string[] WeatherSpawnables => new[]
    {
        "FirePura.BoneWeater.Spawnable.Night",
        "FirePura.BoneWeater.Spawnable.Fog",
        "FirePura.BoneWeater.Spawnable.ThunderStorm"
    };

    public override int Priority => 100;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>() || context.IsPhase<EscapePhase>();
    }

    public override bool ShouldApplyWeatherEffects(ClockhuntMusicContext context)
    {
        return !context.IsLocalNightmare;
    }
}