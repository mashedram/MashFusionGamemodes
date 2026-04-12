using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;

namespace MashGamemodeLibrary.Player.Spectating.data.Colliders;

public interface IDisableableCollider
{
    bool IsColliding { get; }
    void SetColliding(bool isColliding);
    void OnColliderCached(ICachedCollider cachedCollider);
}