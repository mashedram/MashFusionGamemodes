using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Gamemode;

namespace TheHunt.Audio.Effectors.Finally;

public class FinallyMusicEffector : AudioEffector<EnvironmentContext>
{
    public FinallyMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new AudioBinLoader(TheHuntContext.FinallyAudioBin))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}