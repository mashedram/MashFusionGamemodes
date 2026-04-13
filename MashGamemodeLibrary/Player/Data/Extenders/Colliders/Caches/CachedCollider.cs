using System.Collections;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Spectating.data.Colliders;

public class CachedCollider : ICachedCollider
{

    // Source Data
    public int SourceLayer;

    public CachedCollider(Collider collider) : this(collider, collider.gameObject.layer)
    {
    }

    public CachedCollider(Collider collider, int sourceLayer)
    {
        Collider = collider;
        SourceLayer = sourceLayer;
    }
    public Collider Collider { get; }

    public void SetColliding(Collider collider, bool isColliding)
    {
        if (collider == null)
            return;
        
        Physics.IgnoreCollision(Collider, collider, !isColliding);
    }

    public void SetColliding(ICachedCollider other, bool isColliding)
    {
        foreach (var collider in other)
        {
            SetColliding(collider, isColliding);
        }
    }

    public IEnumerator<Collider> GetEnumerator()
    {
        yield return Collider;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    ~CachedCollider()
    {
        if (Collider == null)
            return;

        Collider.gameObject.layer = SourceLayer;
    }

    public void SetLayer(int? layer)
    {
        Collider.gameObject.layer = layer ?? SourceLayer;
    }

    public void ResetLayer()
    {
        Collider.gameObject.layer = SourceLayer;
    }
    public void OnColliderCreated(ICachedCollider collider)
    {
        throw new NotImplementedException();
    }
}