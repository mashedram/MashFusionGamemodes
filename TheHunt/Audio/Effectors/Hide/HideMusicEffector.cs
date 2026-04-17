using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace TheHunt.Audio.Effectors.Hide;

public class HideMusicEffector : AudioEffector<EnvironmentContext>
{
    public HideMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.TheHuntAssets.MonoDisc.3000Cycles",
        "Sylvie.TheHuntAssets.MonoDisc.FalkesTheme",
        "Sylvie.TheHuntAssets.MonoDisc.TrainRide",
        "Sylvie.TheHuntAssets.MonoDisc.Home"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}