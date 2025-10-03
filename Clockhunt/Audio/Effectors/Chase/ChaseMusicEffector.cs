using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors;

public class ChaseMusicEffector : AudioEffector<ClockhuntMusicContext>
{
    public ChaseMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new []
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.RiotControl",
        "Sylvie.SignalisMonodiscs.MonoDisc.Kolibri",
        "Sylvie.SignalisMonodiscs.MonoDisc.IntensiveCare",
        "Sylvie.SignalisMonodiscs.MonoDisc.BecameWholeAgain",
        "Sylvie.SignalisMonodiscs.MonoDisc.Blockwart"
    }))))
    {
        
    }
}