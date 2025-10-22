using LabFusion.Marrow;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Players.Background;
using MashGamemodeLibrary.Audio.Players.Extensions;
using MashGamemodeLibrary.Environment.Effector;

namespace BoneStrike.Audio.Effectors;

public class DefuseMusicEffector : AudioEffector<EnvironmentContext>
{

    public DefuseMusicEffector() : base(new MusicPlayer(new LoadOnDemandContainer(new MonoDiscLoader(FusionMonoDiscReferences.CombatSongReferences))))
    {
    }
    
    public override Enum Track => EffectorTrack.Music;
}