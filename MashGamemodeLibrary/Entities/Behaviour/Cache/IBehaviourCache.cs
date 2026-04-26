using MashGamemodeLibrary.Entities.Association;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Behaviour.Cache;

public delegate void OnAddedDelegate<in TBehaviour>(TBehaviour behaviour);
public delegate void OnRemovedDelegate<in TBehaviour>(TBehaviour behaviour);

public interface IBehaviourCache
{
    Type Target { get; }
    BehaviourMember? TryAdd(IBehaviourHolder holder, IBehaviour behaviour);
    void TryRemove(BehaviourMember member);
}

public interface IBehaviourCache<TBehaviour> : IBehaviourCache
    where TBehaviour : IBehaviour
{
    
    void ForEach(Action<TBehaviour> onEach);
    public IBehaviourHolder? GetHolder(TBehaviour behaviour);

    event OnAddedDelegate<TBehaviour>? OnAdded;
    event OnRemovedDelegate<TBehaviour>? OnRemoved;

    void ForEach(Action<IBehaviourHolder, TBehaviour> onEach);
}

public delegate void OnAssociatedAddedDelegate<in TAssociation, in TBehaviour>(TAssociation association, TBehaviour behaviour);
public delegate void OnAssociatedRemovedDelegate<in TAssociation, in TBehaviour>(TAssociation association, TBehaviour behaviour);

public interface IAssociatedBehaviourCache<out TAssociation, TBehaviour> : IBehaviourCache<TBehaviour>
    where TBehaviour : IBehaviour
    where TAssociation : IEcsAssociation
{
    new event OnAssociatedAddedDelegate<TAssociation, TBehaviour>? OnAdded;
    new event OnAssociatedRemovedDelegate<TAssociation, TBehaviour>? OnRemoved;
    
    public bool Contains(int id);
    public void ForEach(int id, Action<TBehaviour> onEach);
    public IEnumerable<TBehaviour> GetAll(int id);
}