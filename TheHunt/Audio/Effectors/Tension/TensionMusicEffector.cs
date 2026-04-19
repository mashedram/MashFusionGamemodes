using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Gamemode;

namespace TheHunt.Audio.Effectors.Tension;

public class TensionMusicEffector : AudioEffector<EnvironmentContext>
{
    public TensionMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new AudioBinLoader(TheHuntContext.TensionAudioBin))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}