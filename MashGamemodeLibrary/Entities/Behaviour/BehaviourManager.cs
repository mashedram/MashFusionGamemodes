using System.Diagnostics.CodeAnalysis;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.Behaviour;

public static class BehaviourManager
{
    private static readonly InheritanceCache InheritanceCache = new();
    private static readonly Dictionary<Type, IBehaviourCache> Caches = new();

    private static IEnumerable<IBehaviourCache> GetTargetedCaches(IBehaviour behaviour)
    {
        var baseTypes = InheritanceCache.GetBaseTypes(behaviour);
        return baseTypes.Select(baseType => Caches.GetValueOrDefault(baseType)).OfType<IBehaviourCache>();
    }

    public static IBehaviourCache<TBehaviour> CreateCache<TBehaviour>()
        where TBehaviour : IBehaviour
    {
        if (Caches.TryGetValue(typeof(TBehaviour), out var value))
            return (IBehaviourCache<TBehaviour>)value;

        var newValue = new BehaviourCache<TBehaviour>();
        Caches[typeof(TBehaviour)] = newValue;
        return newValue;
    }

    public static List<BehaviourMember> Add(IBehaviourHolder holder, object behaviour)
    {
        if (behaviour is not IBehaviour typedBehaviour)
            return new List<BehaviourMember>();

        InheritanceCache.AddComponent(typedBehaviour);

        return GetTargetedCaches(typedBehaviour)
            .Select(cache => cache.TryAdd(holder, typedBehaviour))
            .OfType<BehaviourMember>()
            .ToList();
    }

    internal static void Remove(BehaviourMember behaviour)
    {
        behaviour.Remove();
    }

    internal static void RemoveAll(IEnumerable<BehaviourMember> list)
    {
        foreach (var behaviourMember in list)
        {
            behaviourMember.Remove();
        }
    }
}