using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Player.Collision;

// TODO: Make these the actual values from the game
internal static class BonelabLayers
{
    public const int Player = 8;
    public const int NoCollide = 9;
    public const int Deciball = 16;
    public const int Decaball = 17;
    public const int FootBall = 24;
}

internal class PlayerColliderCache
{
    private static readonly HashSet<int> IncludedRemoteLayers = new()
    {
        BonelabLayers.FootBall,
        BonelabLayers.Player,
        BonelabLayers.NoCollide
    };
    private static readonly HashSet<int> IncludedLocalLayers = new()
    {
        BonelabLayers.Player,
        BonelabLayers.NoCollide,
        BonelabLayers.Deciball
    };
    
    private static readonly Dictionary<string, int> OriginalLayers = new()
    {
        { "Foot (right)", BonelabLayers.NoCollide },
        { "Foot (left)", BonelabLayers.NoCollide },
        { "Head", BonelabLayers.Player },
        { "Neck", BonelabLayers.Player },
        { "Chest", BonelabLayers.Player },
        { "ShoulderLf", BonelabLayers.Player },
        { "ElbowLf", BonelabLayers.Player },
        { "Hand (left)", BonelabLayers.Player },
        { "l_fingers_col", BonelabLayers.Player },
        { "ShoulderRt", BonelabLayers.Player },
        { "ElbowRt", BonelabLayers.Player },
        { "Hand (right)", BonelabLayers.Player },
        { "r_fingers_col", BonelabLayers.Player },
        { "Spine", BonelabLayers.Player },
        { "Pelvis", BonelabLayers.Player },
        { "HipLf", BonelabLayers.NoCollide },
        { "KneeLf", BonelabLayers.NoCollide },
        { "HipRt", BonelabLayers.NoCollide },
        { "KneeRt", BonelabLayers.NoCollide },
        { "Knee", BonelabLayers.FootBall },
        { "KneetoPelvis", BonelabLayers.FootBall },
        { "Feet", BonelabLayers.FootBall },
        { "BreastLf", BonelabLayers.Decaball },
        { "BreastRt", BonelabLayers.Decaball },
        { "UpperarmLf", BonelabLayers.Decaball },
        { "ForearmLf", BonelabLayers.Decaball },
        { "SoftHandLf", BonelabLayers.Decaball },
        { "UpperarmRt", BonelabLayers.Decaball },
        { "ForearmRt", BonelabLayers.Decaball },
        { "SoftHandRt", BonelabLayers.Decaball },
        { "ButtLf", BonelabLayers.Decaball },
        { "ButtRt", BonelabLayers.Decaball },
        { "ThighLf", BonelabLayers.Decaball },
        { "ThighRt", BonelabLayers.Decaball },
    };
    
    private readonly HashSet<ColliderSet> _ignoredColliders = new();
    private readonly HashSet<PlayerColliderCache> _ignoredPlayers = new();
    private readonly Dictionary<GameObject, ColliderSet> _inventoryItemColliders = new();
    private readonly HashSet<ColliderSet> _groundPropColliders = new();
    
    
    private PhysicsRig? _physicsRig;
    private ColliderSet _physicsRigColliders = null!;

    public PlayerColliderCache(PhysicsRig physicsRig)
    {
        SetRig(physicsRig);
    }

    public void ClearPropColliders()
    {
        foreach (var propCollider in _groundPropColliders)
        {
            _physicsRigColliders.SetColliding(propCollider, true);
        }
        _groundPropColliders.Clear();
    }

    public void StopPropColliding(NetworkEntityReference reference)
    {
        if (!reference.TryGetEntity(out var networkEntity))
            return;

        var marrowEntity = networkEntity.GetExtender<IMarrowEntityExtender>();
        if (marrowEntity == null)
            return;

        var colliderSet = new ColliderSet(marrowEntity.MarrowEntity);
        
        _groundPropColliders.Add(colliderSet);
        _physicsRigColliders.SetColliding(colliderSet, false);
    }

    private void StartItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Remove(otherColliders)) return;
        
        _physicsRigColliders.SetColliding(otherColliders, true);
        foreach (var ownItemColliders in _inventoryItemColliders.Values) ownItemColliders.SetColliding(otherColliders, true);
    }

    private void StopItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Add(otherColliders)) return;
        
        _physicsRigColliders.SetColliding(otherColliders, false);
        foreach (var ownItemColliders in _inventoryItemColliders.Values) ownItemColliders.SetColliding(otherColliders, false);
    }

    public void StartColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Remove(other)) return;
        
        other._ignoredPlayers.Remove(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, true);
        foreach (var otherColliders in other._inventoryItemColliders.Values) StartItemColliding(otherColliders);
    }

    public void StopColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Add(other)) return;
        
        other._ignoredPlayers.Add(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, false);
        foreach (var otherColliders in other._inventoryItemColliders.Values) StopItemColliding(otherColliders);
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
            foreach (var collider in _physicsRigColliders)
            {
                if (OriginalLayers.TryGetValue(collider.gameObject.name, out var layer))
                    collider.gameObject.layer = layer;
            }
            
            return;
        }
        
        foreach (var collider in _physicsRigColliders)
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
            
            // We don't check if the collider is in the OriginalLayers, because we filter this earlier when we set the rig

            // 2 Is ignore raycasts
            go.layer = 2;
        }
    }

    public void SetRig(PhysicsRig newRig)
    {
        if (_physicsRig == newRig) return;

        _physicsRig = newRig;
        _physicsRigColliders = new ColliderSet(newRig, OriginalLayers.Keys);

        foreach (var itemColliders in _inventoryItemColliders.Values)
        foreach (var other in _ignoredPlayers)
            other.StartItemColliding(itemColliders);

        _inventoryItemColliders.Clear();

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
        if (_inventoryItemColliders.ContainsKey(item)) return;

        var set = new ColliderSet(item);
        _inventoryItemColliders[item] = set;

        foreach (var other in _ignoredPlayers) other.StopItemColliding(set);
    }

    public void RemoveItem(GameObject item)
    {
        if (!_inventoryItemColliders.Remove(item, out var colliderSet)) return;

        foreach (var other in _ignoredPlayers) other.StartItemColliding(colliderSet);
    }
}