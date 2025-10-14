using Clockhunt.Audio.Effectors.Hide;
using Clockhunt.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;

namespace Clockhunt.Audio;

public class HidePhaseEnvironmentState : EnvironmentState<ClockhuntMusicContext>
{
    public HidePhaseEnvironmentState() : base(new EnvironmentEffector<ClockhuntMusicContext>[]
    {
        new HideMusicEffector()
    })
    {
    }

    public override int Priority => 10;

    public override bool CanPlay(ClockhuntMusicContext context)
    {
        return context.IsPhase<HidePhase>();
    }
}