using BoneStrike.Audio.Effectors;
using BoneStrike.Phase;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Phase;

namespace BoneStrike.Audio;

public class PlantState : EnvironmentState<EnvironmentContext>
{

    public PlantState() : base(new EnvironmentEffector<EnvironmentContext>[]
    {
        new PlantMusicEffector()
    })
    {
    }

    public override int Priority => 10;

    public override bool CanPlay(EnvironmentContext context)
    {
        return GamePhaseManager.IsPhase<PlantPhase>();
    }
}