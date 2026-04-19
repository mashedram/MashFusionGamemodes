using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace TheHunt.Audio.Effectors.Hide;

public class HideMusicEffector : AudioEffector<EnvironmentContext>
{
    public HideMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Mash.TheHuntAssets.MonoDisc.3000Cycles",
        "Mash.TheHuntAssets.MonoDisc.FalkesTheme",
        "Mash.TheHuntAssets.MonoDisc.TrainRide",
        "Mash.TheHuntAssets.MonoDisc.Home"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}