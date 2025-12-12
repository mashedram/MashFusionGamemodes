using Il2CppSLZ.Marrow.Interaction;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Collision;

internal class ColliderSet
{
    private readonly HashSet<Collider> _colliders;
    public IEnumerable<Collider> Colliders => _colliders;

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