using Clockhunt.Phase;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background.Music;

namespace Clockhunt.Audio;

public class HuntEndPhaseMusicState : MusicState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.TurnedAround",
        "Sylvie.SignalisMonodiscs.MonoDisc.Misremembered",
        "Sylvie.SignalisMonodiscs.MonoDisc.NearDarkbythePond"
    }));

    public override int Priority => 100;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>();
    }
}