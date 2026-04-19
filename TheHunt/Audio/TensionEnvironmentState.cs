using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using TheHunt.Audio.Effectors.Chase;
using TheHunt.Audio.Effectors.Tension;

namespace TheHunt.Audio;

public class TensionEnvironmentState : EnvironmentState<EnvironmentContext>
{
    public TensionEnvironmentState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new TensionMusicEffector()
    })
    {
    }

    public override int Priority => 400;
    public override int Layer => 1;

    public override bool CanPlay(EnvironmentContext context)
    {
        return context.IsTension;
    }
}