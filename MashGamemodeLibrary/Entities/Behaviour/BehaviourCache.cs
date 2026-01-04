using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Behaviour;

public delegate void OnAddedDelegate<in THolder, in TBehaviour>(THolder holder, TBehaviour behaviour);
public delegate void OnRemovedDelegate<in THolder, in TBehaviour>(THolder holder, TBehaviour behaviour);

public interface IBehaviourCache<out TBehaviour>
    where TBehaviour : IBehaviour
{
    public bool Contains(ushort entityId);
    public void ForEach(ushort entityId, Action<TBehaviour> onEach);
    public IEnumerable<TBehaviour> GetAll(ushort entityId);
    void ForEach(Action<TBehaviour> onEach);
}

public interface IBehaviourCache<out THolder, out TBehaviour> : IBehaviourCache<TBehaviour>
    where THolder : class, IBehaviourHolder
    where TBehaviour : IBehaviour 
{
    event OnAddedDelegate<THolder, TBehaviour>? OnAdded;
    event OnRemovedDelegate<THolder, TBehaviour>? OnRemoved;

    void ForEach(Action<THolder, TBehaviour> onEach);
}

public record HeldBehaviour<THolder, TValue>(THolder Holder, TValue Behaviour)
    where THolder : class, IBehaviourHolder
    where TValue : IBehaviour;

public class BehaviourCache<TKey, THolder, TBehaviour> : IBehaviourCache<THolder, TBehaviour>
    where TKey : notnull
    where THolder : class, IBehaviourHolder
    where TBehaviour : IBehaviour 
{
    public event OnAddedDelegate<THolder, TBehaviour>? OnAdded;
    public event OnRemovedDelegate<THolder, TBehaviour>? OnRemoved;

    public Type Target { get; } = typeof(TBehaviour);

    private readonly Dictionary<ushort, Dictionary<Type, HeldBehaviour<THolder, TBehaviour>>> _entityToBehaviourMap = new();
    private readonly Dictionary<TBehaviour, THolder> _holderLookup = new();
    private readonly Dictionary<TKey, HeldBehaviour<THolder, TBehaviour>> _behaviours = new();

    public void Add(TKey key, THolder holder, TBehaviour value)
    {
        var entry = new HeldBehaviour<THolder, TBehaviour>(holder, value);
        
        _behaviours.Add(key, entry);
        _entityToBehaviourMap
            .GetValueOrCreate(holder.EntityId)
            .Add(entry.Behaviour.GetType(), entry);
        _holderLookup.Add(value, holder);
        
        OnAdded?.Try(v => v.Invoke(holder, value));
    }

    public void Remove(TKey key)
    {
        if (!_behaviours.Remove(key, out var entry))
            return;

        if (_entityToBehaviourMap.TryGetValue(entry.Holder.EntityId, out var behaviours))
        {
            behaviours.Remove(entry.Behaviour.GetType());
            if (behaviours.Count == 0)
                _entityToBehaviourMap.Remove(entry.Holder.EntityId);
        }
        _holderLookup.Remove(entry.Behaviour);
        
        OnRemoved?.Try(v => v.Invoke(entry.Holder, entry.Behaviour));
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

    private IEnumerable<HeldBehaviour<THolder, TBehaviour>> GetReadyEntries()
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

    public void ForEach(Action<THolder, TBehaviour> onEach)
    {
        foreach (var componentsValue in GetReadyEntries())
        {
            componentsValue.Try(value => onEach.Invoke(value.Holder, value.Behaviour));
        }
    }

    public THolder? GetHolder(TBehaviour behaviour)
    {
        return _holderLookup.GetValueOrDefault(behaviour);
    }
}