using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Player.Spectating.data.Colliders;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public static class CachedMarrowEntities
{
    private static readonly Dictionary<PhysicsRig, CachedPhysicsRig> PhysicsRigCache = new(new UnityComparer());
    private static readonly Dictionary<MarrowEntity, ICachedCollider> ColliderCache = new(new UnityComparer());
    
    public static void OnInitializeMelon()
    {
    }
    
    private static CachedPhysicsRig OnPhysicsRigCreated(PhysicsRig physicsRig)
    {
        if (PhysicsRigCache.TryGetValue(physicsRig, out var rig))
            return rig;
        
        var cachedPhysicsRig = new CachedPhysicsRig(physicsRig);
        PhysicsRigCache.Add(physicsRig, cachedPhysicsRig);
        
        foreach (var cachedMarrowEntity in ColliderCache.Values)
        {
            cachedPhysicsRig.OnColliderCached(cachedMarrowEntity);
        }

        return cachedPhysicsRig;
    }
    
    private static void OnPhysicsRigDestroyed(PhysicsRig physicsRig)
    {
        PhysicsRigCache.Remove(physicsRig);
    }

    public static void OnMarrowEntityCreated(MarrowEntity marrowEntity)
    {
        if (marrowEntity.name == "PhysicsRig")
        {
            var physicsRig = marrowEntity.GetComponent<PhysicsRig>();
            if (physicsRig != null)
            {
                // We still want to add the rig
                ColliderCache.Add(marrowEntity, OnPhysicsRigCreated(physicsRig));
                return;
            }
        }
        
        if (ColliderCache.ContainsKey(marrowEntity))
            return;
        
        var cachedMarrowEntity = new CachedMarrowEntity(marrowEntity);
        ColliderCache.Add(marrowEntity, cachedMarrowEntity);
        foreach (var cachedRig in PhysicsRigCache.Values)
        {
            cachedRig.OnColliderCached(cachedMarrowEntity);
        }
    }
    
    public static void OnMarrowEntityDestroyed(MarrowEntity marrowEntity)
    {
        if (marrowEntity.name == "PhysicsRig")
        {
            var physicsRig = marrowEntity.GetComponent<PhysicsRig>();
            if (physicsRig != null)
            {
                // Don't return, we want to remove it from the collider cache too
                OnPhysicsRigDestroyed(physicsRig);
            }
        }

        ColliderCache.Remove(marrowEntity);
    }
    
    public static void OnMarrowEntityHibernated(MarrowEntity marrowEntity)
    {
        if (ColliderCache.TryGetValue(marrowEntity, out var cachedCollider) && cachedCollider is IHibernationEntity hibernationEntity)
        {
            hibernationEntity.IsHibernating = true;
        }
    }
    
    public static void OnMarrowEntityClearedHibernation(MarrowEntity marrowEntity)
    {
        if (ColliderCache.TryGetValue(marrowEntity, out var cachedCollider) && cachedCollider is IHibernationEntity hibernationEntity)
        {
            hibernationEntity.IsHibernating = false;
        }
    }
    
    // Accessors
    public static IEnumerable<ICachedCollider> CachedEntities => ColliderCache.Values;
    public static IEnumerator<ICachedCollider> CachedEntitiesEnumerator => ColliderCache.Values.GetEnumerator();

    public static CachedPhysicsRig? GetPlayerCollider(byte localSmallID)
    {
        if (!NetworkPlayerManager.TryGetPlayer(localSmallID, out var networkPlayer))
            return null;
        
        if (!networkPlayer.HasRig)
            return null;

        var physicsRig = networkPlayer.RigRefs.RigManager.physicsRig;
        return PhysicsRigCache.GetValueOrDefault(physicsRig);
    }
    
    public static CachedPhysicsRig? GetCachedPhysicsRig(PhysicsRig physicsRig)
    {
        return PhysicsRigCache.GetValueOrCreate(physicsRig, () => OnPhysicsRigCreated(physicsRig));
    }
}