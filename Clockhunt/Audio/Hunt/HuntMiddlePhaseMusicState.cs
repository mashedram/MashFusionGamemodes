using Clockhunt.Phase;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background.Music;

namespace Clockhunt.Audio;

public class HuntMiddlePhaseMusicState : MusicState<ClockhuntMusicContext>
{
    protected override IAudioContainer AudioContainer => new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.DoubleBack",
        "Sylvie.SignalisMonodiscs.MonoDisc.Liminality",
        "Sylvie.SignalisMonodiscs.MonoDisc.DreamDiary",
        "Sylvie.SignalisMonodiscs.MonoDisc.Bodies"
    }));

    public override int Priority => 200;
    
    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HuntPhase>() && context.PhaseProgress < 0.66f;
    }
}