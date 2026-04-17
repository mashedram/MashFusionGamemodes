using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace TheHunt.Audio.Effectors.Hunt;

public class HuntMusicEffector : AudioEffector<EnvironmentContext>
{
    public HuntMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new[]
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.TurnedAround",
        "Sylvie.SignalisMonodiscs.MonoDisc.Misremembered",
        "Sylvie.SignalisMonodiscs.MonoDisc.NearDarkbythePond"
    }))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}