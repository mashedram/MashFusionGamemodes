using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;
using TheHunt.Gamemode;

namespace TheHunt.Audio.Effectors.Hide;

public class HideMusicEffector : AudioEffector<EnvironmentContext>
{
    public HideMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new AudioBinLoader(TheHuntContext.HideAudioBin))))
    {
    }

    public override Enum Track => EffectorTracks.Music;
}