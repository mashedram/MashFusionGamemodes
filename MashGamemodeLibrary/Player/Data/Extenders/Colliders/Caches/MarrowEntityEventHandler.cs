using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public static class MarrowEntityEventHandler
{
    public static bool ShouldColliderBeDynamic(Collider collider)
    {
        if (collider == null)
            return false;
        
        var go = collider.gameObject;
        if (go == null)
            return false;
        
        if (go.isStatic)
            return false;
        
        // Ignore already ignored colliders
        var layer = collider.gameObject.layer;
        // Anything on the not default layer, that is part of a marrow entity, is valid.
        if (layer != BonelabLayers.Default)
            return false;
        
        if ((CachedPhysicsRig.SpectatorIgnoredLayerMask & (1 << layer)) != 0)
            return false;
        
        var rb = collider.attachedRigidbody;
        if (rb == null)
            return false;
        
        if (rb is { freezeRotation: true, constraints: RigidbodyConstraints.FreezePosition })
            return false;
        
        if (rb.constraints == RigidbodyConstraints.FreezeAll)
            return false;

        return true;
    }
    
    public static void FixColliderLayer(Collider collider)
    {
        if (collider == null)
            return;

        if (!ShouldColliderBeDynamic(collider))
            return;
        
        collider.gameObject.layer = BonelabLayers.Dynamic;
    }

    public static void FixColliderLayers(ImpactProperties impactProperties)
    {
        if (impactProperties == null)
            return;

        foreach (var collider in impactProperties.GetComponents<Collider>())
        {
            FixColliderLayer(collider);
        }
    }

    public static void OnMarrowEntityCreated(MarrowEntity entity)
    {
        // Ignore despawned entities
        if (entity.IsDespawned)
            return;

        if (entity.name != "PhysicsRig") 
            return;
        
        PhysicsRigCache.OnPhysicsRigCreated(entity);
    }

    public static void OnMarrowEntityDestroyed(MarrowEntity entity)
    {
        if (entity.name != "PhysicsRig") 
            return;
        
        PhysicsRigCache.OnPhysicsRigDestroyed(entity);
    }
}