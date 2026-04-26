using MashGamemodeLibrary.Entities.Association;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Behaviour.Cache;

public record HeldBehaviour<TValue>(IBehaviourHolder Holder, TValue Behaviour)
    where TValue : IBehaviour;

public class BehaviourCache<TBehaviour> : IBehaviourCache<TBehaviour>
    where TBehaviour : IBehaviour
{
    public event OnAddedDelegate<TBehaviour>? OnAdded;
    public event OnRemovedDelegate<TBehaviour>? OnRemoved;

    public Type Target { get; } = typeof(TBehaviour);

    // Maps an associations hash to the entities behaviour
    private readonly Dictionary<TBehaviour, IBehaviourHolder> _holderLookup = new();
    private readonly Dictionary<Guid, HeldBehaviour<TBehaviour>> _behaviours = new();
    
    // For internal extensions
    protected virtual void OnAddedInternal(HeldBehaviour<TBehaviour> entry)
    {
        OnAdded?.Try(v => v.Invoke(entry.Behaviour));
    }
    protected virtual void OnRemovedInternal(HeldBehaviour<TBehaviour> entry)
    {
        OnRemoved?.Try(v => v.Invoke(entry.Behaviour));
    }
    protected virtual void OnClear()
    {
        _behaviours.Clear(kvp => OnRemoved?.Try(v => v.Invoke(kvp.Value.Behaviour)));
    }

    public Guid Add(IBehaviourHolder holder, TBehaviour value)
    {
        var entry = new HeldBehaviour<TBehaviour>(holder, value);

        var key = Guid.NewGuid();
        _behaviours[key] = entry;
        // _associationToBehaviourMap.GetValueOrCreate(holder.Guid)[entry.Behaviour.GetType()] = entry;
        _holderLookup[value] = holder;

        OnAddedInternal(entry);

        return key;
    }

    public bool Remove(Guid id)
    {
        if (!_behaviours.Remove(id, out var entry))
            return false;
        
        _holderLookup.Remove(entry.Behaviour);

        OnRemovedInternal(entry);

        return true;
    }

    public void Clear()
    {
        if (OnRemoved == null)
        {
            _behaviours.Clear();
            return;
        }
        
        OnClear();
        
        _holderLookup.Clear();
    }

    private IEnumerable<HeldBehaviour<TBehaviour>> GetReadyEntries()
    {
        return _behaviours.Values.Where(kvp => kvp.Holder.IsReady);
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