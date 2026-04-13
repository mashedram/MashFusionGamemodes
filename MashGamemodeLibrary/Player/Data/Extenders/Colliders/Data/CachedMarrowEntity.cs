using System.Collections.Immutable;
using Il2CppSLZ.Marrow.Interaction;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;

public class CachedMarrowEntity
{
    private static bool IsColliderValid(Collider collider)
    {
        if (collider == null)
            return false;
        
        // Ignore already ignored colliders
        var layer = collider.gameObject.layer;
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
    
    private static IEnumerable<Collider> GetColliders(MarrowEntity entity)
    {
        if (entity == null)
            yield break;

        foreach (var entityBody in entity._bodies)
        {
            if (entityBody == null)
                continue;

            foreach (var collider in entityBody.Colliders)
            {
                if (!IsColliderValid(collider))
                    continue;
                
                yield return collider;
            }
        }
    }
    
    public MarrowEntity MarrowEntity { get; }
    public ImmutableArray<CachedCollider> Colliders { get; }
    public bool IsHibernating { get; set; }
    
    public CachedMarrowEntity(MarrowEntity marrowEntity)
    {
        MarrowEntity = marrowEntity;
        Colliders = GetColliders(marrowEntity).Select(collider => new CachedCollider(collider)).ToImmutableArray();
        IsHibernating = marrowEntity.IsHibernating;
    }
    
    public void SetColliding(CachedPhysicsRig physicsRig, bool isColliding)
    {
        foreach (var physicsRigCollider in physicsRig.Colliders)
        {
            foreach (var collider in Colliders)
            {
                collider.SetColliding(physicsRigCollider, isColliding);
            }
        }
    }
}