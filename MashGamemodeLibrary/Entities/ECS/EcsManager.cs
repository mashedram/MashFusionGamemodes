using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Integration;
using MashGamemodeLibrary.Entities.ECS.Query;
using UnityEngine.SocialPlatforms;

namespace MashGamemodeLibrary.Entities.ECS;

public static class EcsManager
{
    // Internal caches

    private static readonly EcsBehaviourCache<IComponentReady> ComponentReadyCache = CreateBehaviorCache<IComponentReady>();
    private static readonly EcsBehaviourCache<IComponentPlayerReady> ComponentPlayerReadyCache = CreateBehaviorCache<IComponentPlayerReady>();
    
    private static readonly EcsBehaviourCache<IComponentUpdate> ComponentUpdateCache = CreateBehaviorCache<IComponentUpdate>();
    private static readonly EcsBehaviourCache<IComponentRemoved> ComponentRemovedCache = CreateBehaviorCache<IComponentRemoved>();

    static EcsManager()
    {
        ComponentReadyCache.OnAdded += (instance, component) =>
        {
            instance.HookOnReady(component.OnReady);
        };

        ComponentPlayerReadyCache.OnAdded += (instance, component) =>
        {
            instance.HookOnReady((networkEntity, marrowEntity) =>
            {
                if (!NetworkPlayerManager.TryGetPlayer((byte)networkEntity.ID, out var networkPlayer))
                    return;

                component.OnReady(networkPlayer, marrowEntity);
            });
        };

        ComponentRemovedCache.OnRemoved += (instance, component) =>
        {
            component.OnRemoved(instance.Index.EntityID.GetEntity());
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
    
    public static EcsBehaviourCache<T> CreateBehaviorCache<T>() where T : IBehaviour
    {
        return LocalEcsCache.CreateBehaviorCache<T>();
    }
    
    public static CachedQuery<T> CacheQuery<T>() where T : IComponent
    {
        return LocalEcsCache.CacheQuery<T>();
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