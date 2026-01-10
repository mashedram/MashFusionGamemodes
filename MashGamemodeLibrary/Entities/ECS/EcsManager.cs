using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS;

public static class EcsManager
{
    // Internal caches

    private static readonly IBehaviourCache<IComponentReady> ComponentReadyCache = BehaviourManager.CreateCache<IComponentReady>();
    private static readonly IBehaviourCache<IComponentPlayerReady> ComponentPlayerReadyCache = BehaviourManager.CreateCache<IComponentPlayerReady>();
    
    private static readonly IBehaviourCache<IComponentUpdate> ComponentUpdateCache = BehaviourManager.CreateCache<IComponentUpdate>();
    private static readonly IBehaviourCache<IComponentRemoved> ComponentRemovedCache = BehaviourManager.CreateCache<IComponentRemoved>();

    static EcsManager()
    {
        ComponentReadyCache.OnAdded += (instance, component) =>
        {
            component.NetworkEntity = instance.NetworkEntity;
            component.MarrowEntity = instance.MarrowEntity;
            component.OnReady(instance.NetworkEntity, instance.MarrowEntity);
        };

        ComponentPlayerReadyCache.OnAdded += (instance, component) =>
        {
            if (!NetworkPlayerManager.TryGetPlayer((byte)instance.NetworkEntity.ID, out var networkPlayer))
                return;

            component.OnReady(networkPlayer, instance.MarrowEntity);
        };

        ComponentRemovedCache.OnRemoved += (instance, component) =>
        {
            component.OnRemoved(instance.NetworkEntity);
        };
    }

    internal static void Update(float delta)
    {
        ComponentUpdateCache.ForEach(behaviour => behaviour.Update(delta));
    }
    
    // Public methods
    
    public static void RegisterAll<T>()
    {
        LocalEcsCache.Registry.RegisterAll<T>();
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
        LocalEcsCache.Clear(networkEntity.ID);
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