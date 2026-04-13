using UnityEngine;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;

public interface ICachedCollider : IEnumerable<Collider>
{
    void SetColliding(Collider collider, bool isColliding);
    void SetColliding(ICachedCollider other, bool isColliding);
}