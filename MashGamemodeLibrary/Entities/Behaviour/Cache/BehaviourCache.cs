using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Behaviour;

public delegate void OnAddedDelegate<in TBehaviour>(IBehaviourHolder holder, TBehaviour behaviour);
public delegate void OnRemovedDelegate<in TBehaviour>(IBehaviourHolder holder, TBehaviour behaviour);

public interface IBehaviourCache : IGuaranteeStaticConstructor
{
    Type Target { get; }
    BehaviourMember? TryAdd(IBehaviourHolder holder, IBehaviour behaviour);
    void TryRemove(BehaviourMember member);
}

public interface IBehaviourCache<TBehaviour> : IBehaviourCache
    where TBehaviour : IBehaviour
{
    public bool Contains(ushort entityId);
    public void ForEach(ushort entityId, Action<TBehaviour> onEach);
    public IEnumerable<TBehaviour> GetAll(ushort entityId);
    void ForEach(Action<TBehaviour> onEach);
    public IBehaviourHolder? GetHolder(TBehaviour behaviour);

    event OnAddedDelegate<TBehaviour>? OnAdded;
    event OnRemovedDelegate<TBehaviour>? OnRemoved;

    void ForEach(Action<IBehaviourHolder, TBehaviour> onEach);
}

public record HeldBehaviour<TValue>(IBehaviourHolder Holder, TValue Behaviour)
    where TValue : IBehaviour;

public class BehaviourCache<TBehaviour> : IBehaviourCache<TBehaviour>
    where TBehaviour : IBehaviour
{
    public event OnAddedDelegate<TBehaviour>? OnAdded;
    public event OnRemovedDelegate<TBehaviour>? OnRemoved;

    public Type Target { get; } = typeof(TBehaviour);

    private readonly Dictionary<ushort, Dictionary<Type, HeldBehaviour<TBehaviour>>> _entityToBehaviourMap = new();
    private readonly Dictionary<TBehaviour, IBehaviourHolder> _holderLookup = new();
    private readonly Dictionary<Guid, HeldBehaviour<TBehaviour>> _behaviours = new();

    public Guid Add(IBehaviourHolder holder, TBehaviour value)
    {
        var entry = new HeldBehaviour<TBehaviour>(holder, value);

        var key = Guid.NewGuid();
        _behaviours[key] = entry;
        _entityToBehaviourMap.GetValueOrCreate(holder.EntityId)[entry.Behaviour.GetType()] = entry;
        _holderLookup[value] = holder;

        OnAdded?.Try(v => v.Invoke(holder, value));

        return key;
    }

    public bool Remove(Guid id)
    {
        if (!_behaviours.Remove(id, out var entry))
            return false;

        if (_entityToBehaviourMap.TryGetValue(entry.Holder.EntityId, out var behaviours))
        {
            behaviours.Remove(entry.Behaviour.GetType());
            if (behaviours.Count == 0)
                _entityToBehaviourMap.Remove(entry.Holder.EntityId);
        }
        _holderLookup.Remove(entry.Behaviour);

        OnRemoved?.Try(v => v.Invoke(entry.Holder, entry.Behaviour));

        return true;
    }

    public void Clear()
    {
        if (OnRemoved == null)
        {
            _behaviours.Clear();
            return;
        }

        _behaviours.Clear(kvp => OnRemoved?.Try(v => v.Invoke(kvp.Value.Holder, kvp.Value.Behaviour)));
        _entityToBehaviourMap.Clear();
        _holderLookup.Clear();
    }

    private IEnumerable<HeldBehaviour<TBehaviour>> GetReadyEntries()
    {
        return _behaviours.Values.Where(kvp => kvp.Holder.IsReady);
    }

    public IEnumerable<TBehaviour> GetAll(ushort entityId)
    {
        return _entityToBehaviourMap.TryGetValue(entityId, out var set)
            ? set
                .Where(kvp => kvp.Value.Holder.IsReady)
                .Select(kvp => kvp.Value.Behaviour)
            : Array.Empty<TBehaviour>();
    }

    public bool Contains(ushort entityId)
    {
        return _entityToBehaviourMap.ContainsKey(entityId);
    }

    public void ForEach(ushort entityId, Action<TBehaviour> onEach)
    {
        foreach (var componentsValue in GetAll(entityId))
        {
            componentsValue.Try(onEach.Invoke);
        }
    }

    public void ForEach(Action<TBehaviour> onEach)
    {
        foreach (var componentsValue in GetReadyEntries())
        {
            componentsValue.Try(value => onEach.Invoke(value.Behaviour));
        }
    }

    public void ForEach(Action<IBehaviourHolder, TBehaviour> onEach)
    {
        foreach (var componentsValue in GetReadyEntries())
        {
            componentsValue.Try(value => onEach.Invoke(value.Holder, value.Behaviour));
        }
    }

    public IBehaviourHolder? GetHolder(TBehaviour behaviour)
    {
        return _holderLookup.GetValueOrDefault(behaviour);
    }

    public BehaviourMember? TryAdd(IBehaviourHolder holder, IBehaviour behaviour)
    {
        if (behaviour is not TBehaviour typedBehaviour)
        {
            InternalLogger.Debug("Tried to add behaviour of type: " + behaviour.GetType().FullName + " to cache of type: " + typeof(TBehaviour).FullName);
            return null;
        }

        var key = Add(holder, typedBehaviour);
        return new BehaviourMember(key, typedBehaviour, this);
    }

    public void TryRemove(BehaviourMember member)
    {
        if (member.Cache.Target != Target)
            return;

        member.InCache = false;
        Remove(member.ID);
    }
}