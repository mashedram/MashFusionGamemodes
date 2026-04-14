using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public static class MarrowEntityEventHandler
{
    private static bool ShouldColliderBeDynamic(Collider collider)
    {
        if (collider == null)
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
    
    private static void FixColliderLayers(MarrowEntity entity)
    {
        if (entity == null)
            return;

        foreach (var entityBody in entity._bodies)
        {
            if (entityBody == null)
                continue;

            foreach (var collider in entityBody.Colliders)
            {
                if (!ShouldColliderBeDynamic(collider))
                    continue;
                
                collider.gameObject.layer = BonelabLayers.Dynamic;
            }
        }
    }
    
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

        // We need to fix the colliders on the entity for the spectator system to work properly
        FixColliderLayers(entity);
    }

    public static void OnMarrowEntityDestroyed(MarrowEntity entity)
    {
        if (entity.name != "PhysicsRig") 
            return;
        
        PhysicsRigCache.OnPhysicsRigDestroyed(entity);
    }
}