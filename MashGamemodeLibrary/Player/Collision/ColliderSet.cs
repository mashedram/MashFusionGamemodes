using System.Collections;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Collision;

internal class ColliderSet : IEnumerable<Collider>
{
    private readonly HashSet<Collider> _colliders;

    public ColliderSet(GameObject root)
    {
        _colliders = root.GetComponentsInChildren<Collider>().ToHashSet();
    }

    public ColliderSet(MarrowBody body)
    {
        _colliders = body._colliders.ToHashSet();
    }

    public ColliderSet(MarrowEntity entity)
    {
        _colliders = entity._bodies.SelectMany(body => body._colliders).ToHashSet();
    }

    public ColliderSet(PhysicsRig physicsRig, ICollection<string> filter)
    {
        _colliders = physicsRig.GetComponentsInChildren<Collider>().Where(v => filter.Contains(v.gameObject.name)).ToHashSet();
    }
    public IEnumerable<Collider> Colliders => _colliders;
    public IEnumerator<Collider> GetEnumerator()
    {
        return _colliders.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void SetColliding(ColliderSet other, bool colliding)
    {
        foreach (var collider1 in _colliders)
        foreach (var collider2 in other._colliders)
        {
            // Comes from the same source. One invalid all invalid
            if (collider1 == null || collider2 == null) return;

            Physics.IgnoreCollision(collider1, collider2, !colliding);
        }
    }
}