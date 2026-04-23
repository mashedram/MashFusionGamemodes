using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public static class MarrowEntityEventHandler
{
    public static void FixColliderLayer(Collider collider)
    {
        if (collider == null)
            return;

        var go = collider.gameObject;
        if (go == null)
            return;

        if (go.isStatic)
            return;

        // Ignore already ignored colliders
        var layer = collider.gameObject.layer;
        // Anything on the not default layer, that is part of a marrow entity, is valid.
        if (layer != BonelabLayers.Default)
            return;

        if ((CachedPhysicsRig.SpectatorIgnoredLayerMask & 1 << layer) != 0)
            return;

        var rb = collider.attachedRigidbody;
        if (rb == null)
            return;

        if (rb is { freezeRotation: true, constraints: RigidbodyConstraints.FreezePosition })
            return;

        if (rb.constraints == RigidbodyConstraints.FreezeAll)
            return;

        collider.gameObject.layer = BonelabLayers.Dynamic;
    }

    public static void FixColliderLayers(Transform transform, int maxRecursion = 0)
    {
        if (transform == null)
            return;
        
        if (!transform.TryGetComponent<Collider>(out var collider))
            return;
        
        FixColliderLayer(collider);
        
        if (maxRecursion <= 0)
            return;

        // While this may seem like a lot of iteration, it only really gets called during level loading and adds less than 1ms in total on most maps.
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            
            if (child == null)
                continue;

            FixColliderLayers(child, maxRecursion - 1);
        }
    }

    private static void FixColliderLayers(MarrowEntity entity)
    {
        if (entity == null)
            return;

        if (entity._bodies == null)
            return;

        foreach (var entityBody in entity._bodies)
        {
            if (entityBody == null)
                continue;

            if (entityBody._colliders == null)
                continue;

            foreach (var collider in entityBody.Colliders)
            {
                FixColliderLayer(collider);
            }
        }
    }

    public static void OnMarrowEntityCreated(MarrowEntity entity)
    {
        if (entity == null)
            return;

        if (entity.gameObject == null)
            return;

        // Ignore despawned entities
        if (entity.IsDespawned)
            return;

        if (entity.name == "PhysicsRig")
        {
            return;
        }

        FixColliderLayers(entity);
    }

    public static void OnMarrowEntityDestroyed(MarrowEntity entity)
    {
        if (entity == null)
            return;

        if (entity.name != "PhysicsRig")
            return;

        PhysicsRigCache.OnPhysicsRigDestroyed(entity);
    }
}