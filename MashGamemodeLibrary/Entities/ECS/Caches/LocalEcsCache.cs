using System.Diagnostics.CodeAnalysis;
using Harmony;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Helpers;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;
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

internal static class LocalEcsCache
{
    // Caches

    private static readonly Dictionary<EcsIndex, ComponentInstance> LocalComponents = new();

    private static readonly Dictionary<ushort, Dictionary<Type, ComponentInstance>> ComponentLookup = new();
    private static readonly Dictionary<Type, HashSet<ushort>> NetworkEntityLookup = new();

    // Networking

    internal static readonly FactoryTypedRegistry<IComponent> Registry = new();

    private static readonly SyncedDictionary<EcsIndex, IComponent> NetworkComponents =
        new(
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
        if (!NetworkInfo.HasServer || NetworkInfo.IsHost)
            return;

        Remove(key);
    }

    // Methods

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

        instance.Remove();

        ComponentLookup.GetValueOrDefault(index.EntityID.ID)?.Remove(instance.ComponentType);
        NetworkEntityLookup.GetValueOrDefault(instance.ComponentType)?.Remove(index.EntityID.ID);

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
        ComponentLookup.Clear();
        NetworkEntityLookup.Clear();

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