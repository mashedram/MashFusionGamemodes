using MashGamemodeLibrary.Entities.Association;

namespace MashGamemodeLibrary.Entities.Behaviour.Cache;

public interface IBehaviourHolder
{
    EcsIndex Index { get; }
    bool IsReady { get; }
}