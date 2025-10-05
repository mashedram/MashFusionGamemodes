using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors;

public class HuntMiddleMusicEffector : AudioEffector<ClockhuntMusicContext>
{
    public HuntMiddleMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new []
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.DoubleBack",
        "Sylvie.SignalisMonodiscs.MonoDisc.Liminality",
        "Sylvie.SignalisMonodiscs.MonoDisc.DreamDiary",
        "Sylvie.SignalisMonodiscs.MonoDisc.Bodies"
    }))))
    {
        
    }
    
    public override Enum Track => EffectorTracks.Music;
}