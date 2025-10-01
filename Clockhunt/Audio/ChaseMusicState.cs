using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background.Music;

namespace Clockhunt.Audio;

public class ChaseMusicState : MusicState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.RiotControl",
        "Sylvie.SignalisMonodiscs.MonoDisc.Kolibri",
        "Sylvie.SignalisMonodiscs.MonoDisc.IntensiveCare",
        "Sylvie.SignalisMonodiscs.MonoDisc.BecameWholeAgain",
        "Sylvie.SignalisMonodiscs.MonoDisc.Blockwart"
    }));

    public override int Priority => 500;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsChasing;
    }
}