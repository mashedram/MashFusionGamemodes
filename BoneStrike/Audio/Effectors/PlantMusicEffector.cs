using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Environment.Effector;

namespace BoneStrike.Audio.Effectors;

public class PlantMusicEffector : AudioEffector<EnvironmentContext>
{

    public PlantMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.3000Cycles",
        "Sylvie.SignalisMonodiscs.MonoDisc.FalkesTheme",
        "Sylvie.SignalisMonodiscs.MonoDisc.TrainRide",
        "Sylvie.SignalisMonodiscs.MonoDisc.Home"
    }))))
    {
    }

    public override Enum Track => EffectorTrack.Music;
}