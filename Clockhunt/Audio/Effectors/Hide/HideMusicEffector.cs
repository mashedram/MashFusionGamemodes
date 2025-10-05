using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Environment.Effector;

namespace Clockhunt.Audio.Effectors.Hide;

public class HideMusicEffector : AudioEffector<ClockhuntMusicContext>
{
    public HideMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(new []
    {
        "Sylvie.SignalisMonodiscs.MonoDisc.3000Cycles",
        "Sylvie.SignalisMonodiscs.MonoDisc.FalkesTheme",
        "Sylvie.SignalisMonodiscs.MonoDisc.TrainRide",
        "Sylvie.SignalisMonodiscs.MonoDisc.Home"
    }))))
    {
        
    }
    
    public override Enum Track => EffectorTracks.Music;
}