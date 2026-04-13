using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;

public class CachedCollider
{
    private readonly Collider _collider;
    private readonly int _sourceLayer;
    
    public CachedCollider(Collider collider) : this(collider, collider.gameObject.layer)
    {
    }
    
    public CachedCollider(Collider collider, int sourceLayer)
    {
        _collider = collider;
        _sourceLayer = sourceLayer;
    }
    
    public void SetLayer(int? layer)
    {
        if (_collider == null)
            return;
        
        _collider.gameObject.layer = layer ?? _sourceLayer;
    }
    
    public void SetColliding(Collider collider, bool isColliding)
    {
        if (collider == null || _collider == null)
            return;
        
        Physics.IgnoreCollision(_collider, collider, !isColliding);
    }
    
    public void SetColliding(CachedCollider other, bool isColliding)
    {
        SetColliding(other._collider, isColliding);
    }
}