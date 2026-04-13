using System.Collections;
using System.Collections.Immutable;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Spectating.data.Colliders;

public class CachedMarrowBody : ICachedCollider
{
    public readonly ImmutableArray<CachedCollider> Colliders;

    public CachedMarrowBody(MarrowBody marrowBody)
    {
        MarrowBody = marrowBody;

        Colliders = marrowBody.Colliders
            .Where(c => c != null)
            .Where(c =>
            {
                var rb = c.attachedRigidbody;
                if (rb == null)
                    return true;

                if (rb is { freezeRotation: true, constraints: RigidbodyConstraints.FreezePosition })
                    return false;

                return rb.constraints != RigidbodyConstraints.FreezeAll;
            })
            .Select(collider => new CachedCollider(collider))
            .ToImmutableArray();
    }
    public MarrowBody MarrowBody { get; }

    public IEnumerator<Collider> GetEnumerator()
    {
        return Colliders.Select(collider => collider.Collider).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void SetColliding(Collider collider, bool isColliding)
    {
        foreach (var cachedCollider in Colliders)
        {
            cachedCollider.SetColliding(collider, isColliding);
        }
    }

    public void SetColliding(ICachedCollider other, bool isColliding)
    {
        foreach (var cachedCollider in Colliders)
        {
            cachedCollider.SetColliding(other, isColliding);
        }
    }
}