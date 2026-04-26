using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Association;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Behaviour.Cache;

public class AssociatedBehaviourCache<TAssociation, TBehaviour> : BehaviourCache<TBehaviour>, IAssociatedBehaviourCache<TAssociation, TBehaviour>
    where TBehaviour : IBehaviour
    where TAssociation : IEcsAssociation
{
    private readonly Dictionary<int, Dictionary<Type, HeldBehaviour<TBehaviour>>> _associationToBehaviourMap = new();
    
    public new event OnAssociatedAddedDelegate<TAssociation, TBehaviour>? OnAdded;
    public new event OnAssociatedRemovedDelegate<TAssociation, TBehaviour>? OnRemoved;
    
    protected override void OnAddedInternal(HeldBehaviour<TBehaviour> entry)
    {
        if (entry.Holder.Index.Association is not TAssociation association) 
            return;
        
        var hash = association.GetID();
        var behaviourMap = _associationToBehaviourMap.GetValueOrCreate(hash);
        behaviourMap.Add(entry.Behaviour.GetType(), entry);
        
        OnAdded?.Invoke(association, entry.Behaviour);
    }

    protected override void OnRemovedInternal(HeldBehaviour<TBehaviour> entry)
    {
        if (entry.Holder.Index.Association is not TAssociation association) 
            return;
        
        var behaviourMap = _associationToBehaviourMap.GetValueOrDefault(association.GetID());
        behaviourMap?.Remove(entry.Behaviour.GetType());
        
        OnRemoved?.Invoke(association, entry.Behaviour);
    }

    protected override void OnClear()
    {
        // Fire all the events before clearing the map
        foreach (var heldBehaviour in _associationToBehaviourMap.SelectMany(keyValuePair => keyValuePair.Value))
        {
            if (heldBehaviour.Value.Holder.Index.Association is not TAssociation association) 
                continue;
                
            OnRemoved?.Invoke(association, heldBehaviour.Value.Behaviour);
        }
        _associationToBehaviourMap.Clear();
    }

    public IEnumerable<TBehaviour> GetAll(int entityId)
    {
        if (!_associationToBehaviourMap.TryGetValue(entityId, out var behaviourSet))
            return Array.Empty<TBehaviour>();

        return behaviourSet
            .Values
            .Where(value => value.Holder.IsReady)
            .Select(value => value.Behaviour);
    }
    
    public bool Contains(int id)
    {
        return _associationToBehaviourMap.ContainsKey(id);
    }

    public void ForEach(int id, Action<TBehaviour> onEach)
    {
        foreach (var componentsValue in GetAll(id))
        {
            componentsValue.Try(onEach.Invoke);
        }
    }   
}