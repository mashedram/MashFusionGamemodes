using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace TheHunt.Audio.Effectors.Chase;

public class ChaseMusicEffector : AudioEffector<EnvironmentContext>
{
    public ChaseMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.RiotControl",
        "Sylvie.SignalisMonodiscs.MonoDisc.Kolibri",
        "Sylvie.SignalisMonodiscs.MonoDisc.IntensiveCare",
        "Sylvie.SignalisMonodiscs.MonoDisc.BecameWholeAgain",
        "Sylvie.SignalisMonodiscs.MonoDisc.Blockwart"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}