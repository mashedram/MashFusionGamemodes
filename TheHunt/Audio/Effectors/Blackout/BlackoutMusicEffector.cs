using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Gamemode;

namespace TheHunt.Audio.Effectors.Blackout;

public class BlackoutMusicEffector : AudioEffector<EnvironmentContext>
{
    public BlackoutMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new AudioBinLoader(TheHuntContext.BlackoutMusicAudioBin))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}