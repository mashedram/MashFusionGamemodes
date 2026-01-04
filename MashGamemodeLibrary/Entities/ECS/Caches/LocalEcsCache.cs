using System.Diagnostics.CodeAnalysis;
using Harmony;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Integration;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;
using UnityEngine.Rendering.Universal.LibTessDotNet;
using UnityEngine.SocialPlatforms;

namespace MashGamemodeLibrary.Entities.ECS.Caches;

internal class InheritanceCache
{
    private HashSet<Type> _baseTypes = new();
    private Dictionary<Type, HashSet<Type>> _dictionary = new();

    public void AddComponent(ComponentInstance componentInstance)
    {
        var t = componentInstance.ComponentType;
        
        if (_dictionary.ContainsKey(t))
            return;

        var baseComponents = t.GetInterfaces();
        foreach (var baseComponent in baseComponents)
        {
            if (!baseComponent.IsAssignableTo(typeof(IBehaviour)))
                continue;

            _baseTypes.Add(baseComponent);
            _dictionary
                .GetValueOrCreate(t, () => new HashSet<Type>())
                .Add(baseComponent);

        }
    }

    public IEnumerable<Type> GetBaseComponentTypes(ComponentInstance component)
    {
        return _dictionary.TryGetValue(component.Component.GetType(), out var set) ? set : Array.Empty<Type>();
    }
}

internal static class LocalEcsCache
{
    // Caches
    
    private static readonly Dictionary<EcsIndex, ComponentInstance> LocalComponents = new();

    private static readonly Dictionary<ushort, Dictionary<Type, ComponentInstance>> ComponentLookup = new();
    private static readonly Dictionary<Type, HashSet<ushort>> NetworkEntityLookup = new();
    
    private static readonly Dictionary<ushort, ECSExtender> EcsExtenders = new();

    private static readonly InheritanceCache InheritanceCache = new InheritanceCache();
    private static readonly Dictionary<Type, IEscTargetedCache> TargetedCaches = new();
    
    // Queries

    private static readonly Dictionary<Type, ICachedQuery> CachedQueries = new();
    
    // Networking
    
    internal static readonly FactoryTypedRegistry<IComponent> Registry = new();
    private static readonly SyncedDictionary<EcsIndex, IComponent> NetworkComponents =
        new SyncedDictionary<EcsIndex, IComponent>(
            "sync.ECS", 
            new NetSerializableEncoder<EcsIndex>(), 
            new DynamicInstanceEncoder<IComponent>(Registry)
        );
    
    static LocalEcsCache()
    {
        NetworkComponents.OnValueAdded += OnNetworkComponentAdded;
        NetworkComponents.OnValueRemoved += OnNetworkComponentRemoved;
    }
    
    private static void OnNetworkComponentAdded(EcsIndex key, IComponent component)
    {
        if (!NetworkInfo.HasServer || NetworkInfo.IsHost)
            return;
        
        Add(new ComponentInstance(key, component));
    }

    private static void OnNetworkComponentRemoved(EcsIndex key, IComponent oldValue)
    {   
        if (!NetworkInfo.HasServer ||NetworkInfo.IsHost)
            return;
        
        Remove(key);
    }
    
    // Cache Queries
    
    public static EcsBehaviourCache<T> CreateBehaviorCache<T>() where T : IBehaviour
    {
        var cache = new EcsBehaviourCache<T>();
        TargetedCaches.Add(typeof(T), cache);
        return cache;
    }

    public static CachedQuery<T> CacheQuery<T>() where T : IComponent
    {
        return (CachedQuery<T>)CachedQueries.GetValueOrCreate(typeof(T), () => new CachedQuery<T>());
    }
    
    // Methods

    private static IEnumerable<IEscTargetedCache> GetTargetedCaches(ComponentInstance componentInstance)
    {
        var baseTypes = InheritanceCache.GetBaseComponentTypes(componentInstance);
        return baseTypes.Select(baseType => TargetedCaches.GetValueOrDefault(baseType)).OfType<IEscTargetedCache>();
    }
    
    public static void Add(ComponentInstance componentInstance)
    {
        var index = componentInstance.Index;
        // Checks

        if (componentInstance.PlayerOnly && index.EntityID.ID > PlayerIDManager.MaxPlayerID)
            throw new Exception($"Failed to add tag meant for players to prop ({componentInstance.Component.GetType().FullName})");
        
        // Logic
        if (!LocalComponents.TryAdd(index, componentInstance))
            return;

        ComponentLookup
            .GetValueOrCreate(index.EntityID.ID)
            .Add(componentInstance.Component.GetType(), componentInstance);

        NetworkEntityLookup
            .GetValueOrCreate(componentInstance.Component.GetType())
            .Add(index.EntityID.ID);

        InheritanceCache.AddComponent(componentInstance);
        EcsExtenders
            .GetValueOrCreate(index.EntityID.ID, () => new ECSExtender(index.EntityID))
            .AddComponent(componentInstance);
        
        GetTargetedCaches(componentInstance).ForEach(cache => cache.Add(index, componentInstance));

        if (CachedQueries.TryGetValue(componentInstance.ComponentType, out var cachedQuery))
        {
            cachedQuery.TryAdd(componentInstance);
        }
        
        // Networking
        if (!componentInstance.IsNetworked)
            return;
        if (!NetworkInfo.IsHost)
            return;
        
        Executor.RunIfHost(() =>
        {
            NetworkComponents[index] = componentInstance.Component;
        });
    }

    public static void Remove(EcsIndex index)
    {
        if (!LocalComponents.Remove(index, out var instance))
            return;

        ComponentLookup.GetValueOrDefault(index.EntityID.ID)?.Remove(instance.ComponentType);
        NetworkEntityLookup.GetValueOrDefault(instance.ComponentType)?.Remove(index.EntityID.ID);

        var extender = EcsExtenders[index.EntityID.ID];
        extender.RemoveComponent(index);
        if (extender.IsEmpty())
        {
            extender.Disconnect();
            EcsExtenders.Remove(index.EntityID.ID);
        }
        
        GetTargetedCaches(instance).ForEach(cache => cache.Remove(index));
        
        if (CachedQueries.TryGetValue(instance.ComponentType, out var cachedQuery))
        {
            cachedQuery.Remove(index);
        }
        
        if (!NetworkInfo.IsHost)
            return;

        // Always remove on the network to ensure it's not there by accident
        NetworkComponents.Remove(index);
    }

    public static void Remove<T>(NetworkEntity entity) where T : IComponent
    {
        if (!ComponentLookup.TryGetValue(entity.ID, out var cache))
            return;
        
        if (!cache.TryGetValue(typeof(T), out var instance))
            return;
        
        Remove(instance.Index);
    }

    public static void Clear(ushort entityId)
    {
        if (!ComponentLookup.TryGetValue(entityId, out var cache))
            return;

        foreach (var componentInstance in cache.Values)
        {
            Remove(componentInstance.Index);
        }
    }

    public static void Clear()
    {
        LocalComponents.Clear();
        EcsExtenders.Clear();
        ComponentLookup.Clear();
        NetworkEntityLookup.Clear();
        TargetedCaches.Values.ForEach(cache => cache.Clear());
        
        if (NetworkInfo.IsHost)
            NetworkComponents.Clear();
    }
    
    public static ComponentInstance? GetComponentInstance(EcsIndex index)
    {
        return LocalComponents.GetValueOrDefault(index);
    }
    
    public static T? GetComponent<T>(ushort entityId) where T : class, IComponent
    {
        if (!ComponentLookup.TryGetValue(entityId, out var cache))
            return null;

        return cache.GetValueOrDefault(typeof(T))?.Component as T;
    }

    public static IEnumerable<ushort> GetEntityIdsWithTag<T>() where T : IComponent
    {
        return NetworkEntityLookup.TryGetValue(typeof(T), out var set) ? set : Array.Empty<ushort>();
    }
}