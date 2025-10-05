using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors;

public class HuntEndMusicEffector : AudioEffector<ClockhuntMusicContext>
{
    public HuntEndMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.TurnedAround",
        "Sylvie.SignalisMonodiscs.MonoDisc.Misremembered",
        "Sylvie.SignalisMonodiscs.MonoDisc.NearDarkbythePond"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}