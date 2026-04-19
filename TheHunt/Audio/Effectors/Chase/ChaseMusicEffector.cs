using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace TheHunt.Audio.Effectors.Chase;

public class ChaseMusicEffector : AudioEffector<EnvironmentContext>
{
    public ChaseMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Mash.SignalisMonodiscs.MonoDisc.RiotControl",
        "Mash.SignalisMonodiscs.MonoDisc.Kolibri",
        "Mash.SignalisMonodiscs.MonoDisc.IntensiveCare",
        "Mash.SignalisMonodiscs.MonoDisc.BecameWholeAgain",
        "Mash.SignalisMonodiscs.MonoDisc.Blockwart"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}