using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Music;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace BoneStrike.Audio.Effectors;

public class PlantMusicEffector : AudioEffector<EnvironmentContext>
{

    public PlantMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MusicPackLoader(MusicPackTags.Ambient))))
    {
    }

    public override Enum Track => EffectorTrack.Music;
}