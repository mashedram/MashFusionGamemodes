using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Gamemode;

namespace TheHunt.Audio.Effectors.Chase;

public class ChaseMusicEffector : AudioEffector<EnvironmentContext>
{
    public ChaseMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new AudioBinLoader(TheHuntContext.ChaseAudioBin))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}