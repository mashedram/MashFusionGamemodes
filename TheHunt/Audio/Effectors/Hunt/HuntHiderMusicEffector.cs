using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Gamemode;

namespace TheHunt.Audio.Effectors.Hunt;

public class HuntHiderMusicEffector : AudioEffector<EnvironmentContext>
{
    public HuntHiderMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new AudioBinLoader(TheHuntContext.HuntHiderAudioBin))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}