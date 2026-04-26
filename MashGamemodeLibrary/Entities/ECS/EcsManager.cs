using System.Runtime.CompilerServices;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS;

public static class EcsManager
{
    // Internal caches

    private static readonly IBehaviourCache<IEntityAttached> EntityAttachedCache = BehaviourManager.CreateCache<IEntityAttached>();
    private static readonly IBehaviourCache<IPlayerAttached> PlayerAttachedCache = BehaviourManager.CreateCache<IPlayerAttached>();

    private static readonly IBehaviourCache<IUpdate> UpdateCache = BehaviourManager.CreateCache<IUpdate>();
    private static readonly IBehaviourCache<IRemoved> RemovedCache = BehaviourManager.CreateCache<IRemoved>();

    static EcsManager()
    {
        EntityAttachedCache.OnAdded += (instance, component) =>
        {
            component.OnReady(instance.NetworkEntity, instance.MarrowEntity);
        };

        PlayerAttachedCache.OnAdded += (instance, component) =>
        {
            if (!NetworkPlayerManager.TryGetPlayer((byte)instance.NetworkEntity.ID, out var networkPlayer))
                return;

            component.OnReady(networkPlayer, instance.MarrowEntity);
        };

        RemovedCache.OnRemoved += (instance, component) =>
        {
            component.OnRemoved();
        };
    }

    internal static void Update(float delta)
    {
        UpdateCache.ForEach(behaviour => behaviour.Update(delta));
    }

    // Public methods

    public static void RegisterAll<T>()
    {
        LocalEcsCache.ComponentRegistry.RegisterAll<T>();

        // Ensure that all static constructors are run for the registered types, so if they have their own caches, these are also loaded
        foreach (var allType in LocalEcsCache.ComponentRegistry.GetAllTypes())
        {
            RuntimeHelpers.RunClassConstructor(allType.TypeHandle);
        }
    }

    public static void AddComponent(this NetworkEntity entity, IComponent component)
    {
        var index = EcsIndex.Create(entity, component);
        var instance = new ComponentInstance(index, component);

        LocalEcsCache.Add(instance);
    }

    public static void RemoveComponent<T>(this NetworkEntity entity) where T : IComponent
    {
        LocalEcsCache.Remove<T>(entity);
    }

    public static void ClearComponents(this NetworkEntity networkEntity)
    {
        LocalEcsCache.RemoveAll(networkEntity.ID);
    }

    public static T? GetComponent<T>(this NetworkEntity entity) where T : class, IComponent
    {
        return LocalEcsCache.GetComponent<T>(entity.ID);
    }

    public static T? GetComponent<T>(ushort entityId) where T : class, IComponent
    {
        return LocalEcsCache.GetComponent<T>(entityId);
    }

    public static T? GetComponent<T>(byte playerId) where T : class, IComponent
    {
        return LocalEcsCache.GetComponent<T>(playerId);
    }

    public static IEnumerable<ushort> GetEntityIdsWithComponent<T>() where T : IComponent
    {
        return LocalEcsCache.GetEntityIdsWithTag<T>();
    }
}