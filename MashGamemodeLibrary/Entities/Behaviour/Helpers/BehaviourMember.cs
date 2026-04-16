using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.Behaviour.Helpers;

// Helper class to automatically unregister on GC
public class BehaviourMember
{
    internal bool InCache = true;
    internal Guid ID { get; }
    internal IBehaviour Behaviour { get; }
    internal IBehaviourCache Cache { get; }

    internal BehaviourMember(Guid id, IBehaviour behaviour, IBehaviourCache cache)
    {
        ID = id;
        Behaviour = behaviour;
        Cache = cache;
    }

    ~BehaviourMember()
    {
        if (!InCache)
            return;

        BehaviourManager.Remove(this);
    }

    public void Remove()
    {
        Cache.TryRemove(this);
    }
}