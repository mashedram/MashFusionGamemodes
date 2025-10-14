using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors;

public class HuntStartMusicEffector : AudioEffector<ClockhuntMusicContext>
{
    public HuntStartMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.CasualLoop",
        "Sylvie.SignalisMonodiscs.MonoDisc.Eulenlieder",
        "Sylvie.SignalisMonodiscs.MonoDisc.DieToteninselEmptiness"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}