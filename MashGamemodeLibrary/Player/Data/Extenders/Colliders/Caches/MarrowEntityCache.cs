using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Extensions;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Scheduler;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public static class MarrowEntityCache
{
    private static readonly Dictionary<MarrowEntity, CachedMarrowEntity> _cache = new(new UnityComparer());
    private static readonly HashSet<CachedMarrowEntity> _awakeEntities = new();
    
    public static void OnMarrowEntityCreated(MarrowEntity entity)
    {
        // Ignore despawned entities
        if (entity.IsDespawned)
            return;
        
        if (entity.name == "PhysicsRig")
        {
            PhysicsRigCache.OnPhysicsRigCreated(entity);
            return;
        }
        
        // Avoid duplicates
        if (_cache.ContainsKey(entity))
            return;
        
        var cachedEntity = new CachedMarrowEntity(entity);
        
        // If the entity has no colliders, we don't need to cache it or track its awake state
        if (cachedEntity.Colliders.Length == 0)
            return;
        
        _cache.Add(entity, cachedEntity);
        
        if (entity.IsHibernating)
            return;
            
        _awakeEntities.Add(cachedEntity);
        MarrowEntityCollisionScheduler.ScheduleForIgnoringRigs(cachedEntity);
    }

    public static void OnMarrowEntityDestroyed(MarrowEntity entity)
    {
        if (entity.name == "PhysicsRig")
        {
            PhysicsRigCache.OnPhysicsRigDestroyed(entity);
            return;
        }

        _cache.Remove(entity, out var cachedEntity);
        if (cachedEntity == null)
            return;
        
        if (!_awakeEntities.Remove(cachedEntity))
            return;
        
        // Only called if the entity is awake, otherwise collisions would be reset regardless
        MarrowEntityCollisionScheduler.ScheduleDespawnReset(cachedEntity);
    }
    
    public static void OnMarrowEntityHibernate(MarrowEntity entity)
    {
        if (!_cache.TryGetValue(entity, out var cachedEntity))
            return;
        
        cachedEntity.IsHibernating = true;
        _awakeEntities.Add(cachedEntity);
        MarrowEntityCollisionScheduler.ScheduleForIgnoringRigs(cachedEntity);
    }
    
    public static void OnMarrowEntityWake(MarrowEntity entity)
    {
        if (!_cache.TryGetValue(entity, out var cachedEntity))
            return;
        
        cachedEntity.IsHibernating = false;
        _awakeEntities.Remove(cachedEntity);
        
        // TODO : Possible optimisation by only toggling for non-colliding rigs
        MarrowEntityCollisionScheduler.ScheduleDespawnReset(cachedEntity);
    }
    
    public static void Reset()
    {
        _cache.Clear();
        _awakeEntities.Clear();
    }
    
    public static IEnumerable<CachedMarrowEntity> GetAwake()
    {
        return _awakeEntities;
    }
}