using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Collision;

internal class PlayerColliderCache
{
    private static readonly HashSet<int> IncludedRemoteLayers = new()
    {
        24,
        8,
        9,
        16
    };
    private static readonly HashSet<int> IncludedLocalLayers = new()
    {
        8,
        9,
        16
    };

    private static readonly Dictionary<string, int> DefaultLayer = new();

    private readonly HashSet<ColliderSet> _ignoredColliders = new();
    private readonly HashSet<PlayerColliderCache> _ignoredPlayers = new();
    private readonly Dictionary<GameObject, ColliderSet> _itemColliders = new();
    private readonly HashSet<ColliderSet> _propColliders = new();
    
    
    private PhysicsRig? _physicsRig;
    private ColliderSet _physicsRigColliders = null!;

    public PlayerColliderCache(PhysicsRig physicsRig)
    {
        SetRig(physicsRig);
    }

    public void ClearPropColliders()
    {
        foreach (var propCollider in _propColliders)
        {
            _physicsRigColliders.SetColliding(propCollider, true);
        }
        _propColliders.Clear();
    }

    public void StopPropColliding(NetworkEntityReference reference)
    {
        if (!reference.TryGetEntity(out var networkEntity))
            return;

        var marrowEntity = networkEntity.GetExtender<IMarrowEntityExtender>();
        if (marrowEntity == null)
            return;

        var colliderSet = new ColliderSet(marrowEntity.MarrowEntity);
        
        _propColliders.Add(colliderSet);
        _physicsRigColliders.SetColliding(colliderSet, false);
    }

    private void StartItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Remove(otherColliders)) return;
        
        _physicsRigColliders.SetColliding(otherColliders, true);
        foreach (var ownItemColliders in _itemColliders.Values) ownItemColliders.SetColliding(otherColliders, true);
    }

    private void StopItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Add(otherColliders)) return;
        
        _physicsRigColliders.SetColliding(otherColliders, false);
        foreach (var ownItemColliders in _itemColliders.Values) ownItemColliders.SetColliding(otherColliders, false);
    }

    public void StartColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Remove(other)) return;
        
        other._ignoredPlayers.Remove(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, true);
        foreach (var otherColliders in other._itemColliders.Values) StartItemColliding(otherColliders);
    }

    public void StopColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Add(other)) return;
        
        other._ignoredPlayers.Add(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, false);
        foreach (var otherColliders in other._itemColliders.Values) StopItemColliding(otherColliders);
    }

    public bool IsCollidingWith(PlayerColliderCache other)
    {
        return _ignoredPlayers.Contains(other);
    }
    
    public void SetIgnoreRaycast(PlayerID target, bool colliding)
    {

        if (_physicsRig == null)
            return;
        
        if (colliding)
        {
            foreach (var collider in _physicsRig._collisionCollectors)
            {
                if (DefaultLayer.TryGetValue(collider.gameObject.name, out var layer))
                    collider.gameObject.layer = layer;
            }
            
            return;
        }
        
        foreach (var collider in _physicsRig._collisionCollectors)
        {
            var go = collider.gameObject;
            var cLayer = go.layer;
            
            if (target.IsMe)
            {
                if (!IncludedLocalLayers.Contains(cLayer))
                    continue;
            }
            else
            {
                if (!IncludedRemoteLayers.Contains(cLayer))
                    continue;
            }
            
            DefaultLayer.TryAdd(go.name, cLayer);

            // 2 Is ignore raycasts
            go.layer = 2;
        }
    }

    public void SetRig(PhysicsRig newRig)
    {
        if (_physicsRig == newRig) return;

        _physicsRig = newRig;
        _physicsRigColliders = new ColliderSet(newRig.gameObject);

        foreach (var itemColliders in _itemColliders.Values)
        foreach (var other in _ignoredPlayers)
            other.StartItemColliding(itemColliders);

        _itemColliders.Clear();

        var inventory = _physicsRig.gameObject.GetComponent<Inventory>();
        foreach (var inventoryBodySlot in inventory.bodySlots)
        {
            var slot = inventoryBodySlot._inventorySlot;
            if (slot == null) continue;
            var weapon = slot._weaponHost;
            if (weapon == null) continue;
            var gameObject = weapon.GetHostGameObject();
            if (gameObject == null) continue;
            AddItem(gameObject);
        }

        var hands = new[] { _physicsRig.leftHand, _physicsRig.rightHand };
        foreach (var hand in hands)
        {
            var attached = hand.m_CurrentAttachedGO;
            if (attached == null) return;

            AddItem(attached);
        }
    }

    public void AddItem(GameObject item)
    {
        if (_itemColliders.ContainsKey(item)) return;

        var set = new ColliderSet(item);
        _itemColliders[item] = set;

        foreach (var other in _ignoredPlayers) other.StopItemColliding(set);
    }

    public void RemoveItem(GameObject item)
    {
        if (!_itemColliders.Remove(item, out var colliderSet)) return;

        foreach (var other in _ignoredPlayers) other.StartItemColliding(colliderSet);
    }
}