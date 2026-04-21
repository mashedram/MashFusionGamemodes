using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Music;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace BoneStrike.Audio.Effectors;

public class IntermissionMusicEffector : AudioEffector<EnvironmentContext>
{

    public IntermissionMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MusicPackLoader(MusicPackTags.Intermission))))
    {
    }

    public override Enum Track => EffectorTrack.Music;
}