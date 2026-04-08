using System.Collections.Immutable;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Collision;

// TODO: Make these the actual values from the game

internal class PlayerColliderCache
{
    // No Raycast Layer
    private static readonly int SpectatorLayer = 2;
    private static readonly Dictionary<string, int> OriginalLayers = new()
    {
        {
            "Feet", BonelabLayers.Feet  
        },
        {
            "Foot (right)", BonelabLayers.NoCollide
        },
        {
            "Foot (left)", BonelabLayers.NoCollide
        },
        {
            "Head", BonelabLayers.Player
        },
        {
            "Neck", BonelabLayers.Player
        },
        {
            "Chest", BonelabLayers.Player
        },
        {
            "ShoulderLf", BonelabLayers.Player
        },
        {
            "ElbowLf", BonelabLayers.Player
        },
        {
            "Hand (left)", BonelabLayers.Player
        },
        {
            "l_fingers_col", BonelabLayers.Player
        },
        {
            "ShoulderRt", BonelabLayers.Player
        },
        {
            "ElbowRt", BonelabLayers.Player
        },
        {
            "Hand (right)", BonelabLayers.Player
        },
        {
            "r_fingers_col", BonelabLayers.Player
        },
        {
            "Spine", BonelabLayers.Player
        },
        {
            "Pelvis", BonelabLayers.Player
        },
        {
            "HipLf", BonelabLayers.NoCollide
        },
        {
            "KneeLf", BonelabLayers.NoCollide
        },
        {
            "HipRt", BonelabLayers.NoCollide
        },
        {
            "KneeRt", BonelabLayers.NoCollide
        },
        {
            "BreastLf", BonelabLayers.Deciverse
        },
        {
            "BreastRt", BonelabLayers.Deciverse
        },
        {
            "UpperarmLf", BonelabLayers.Deciverse
        },
        {
            "ForearmLf", BonelabLayers.Deciverse
        },
        {
            "SoftHandLf", BonelabLayers.Deciverse
        },
        {
            "UpperarmRt", BonelabLayers.Deciverse
        },
        {
            "ForearmRt", BonelabLayers.Deciverse
        },
        {
            "SoftHandRt", BonelabLayers.Deciverse
        },
        {
            "ButtLf", BonelabLayers.Deciverse
        },
        {
            "ButtRt", BonelabLayers.Deciverse
        },
        {
            "ThighLf", BonelabLayers.Deciverse
        },
        {
            "ThighRt", BonelabLayers.Deciverse
        },
        {
            "ItemReciever", BonelabLayers.Socket
        },
        {
            "InventoryAmmoReceiver", BonelabLayers.Socket
        },
        // From first list:
        { "Knee", BonelabLayers.Feet },
        { "KneetoPelvis", BonelabLayers.Feet },
        // From second list (Deci variants):
        { "DeciHead", BonelabLayers.Deciverse },
        { "DeciChest", BonelabLayers.Deciverse },
        { "DeciShoulderLf", BonelabLayers.Deciverse },
        { "DeciElbowLf", BonelabLayers.Deciverse },
        { "DeciHandLf", BonelabLayers.Deciverse },
        { "DeciShoulderRt", BonelabLayers.Deciverse },
        { "DeciElbowRt", BonelabLayers.Deciverse },
        { "DeciHandRt", BonelabLayers.Deciverse },
        { "DeciSpine", BonelabLayers.Deciverse },
        { "DeciPelvis", BonelabLayers.Deciverse },
        { "DeciHipLf", BonelabLayers.Deciverse },
        { "DeciHipRt", BonelabLayers.Deciverse },
    };

    static PlayerColliderCache()
    {
        // We only want terrain layers
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Default, false);
        
        // Ignore raycasts, so we don't have to worry about them when we set the rig
        
        
        // Ignore everything else
        Physics.IgnoreLayerCollision(SpectatorLayer, SpectatorLayer, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Fixture, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Player, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.NoCollide, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Dynamic, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.EnemyColliders, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Interactable, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Deciverse, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Socket, true);
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.PlayerAndNPC, true);
    }

    private readonly HashSet<ColliderSet> _groundPropColliders = new();

    private readonly HashSet<ColliderSet> _ignoredColliders = new();
    private readonly HashSet<PlayerColliderCache> _ignoredPlayers = new();
    private readonly Dictionary<GameObject, ColliderSet> _inventoryItemColliders = new();


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

        var colliders = _physicsRigColliders;

        if (colliding)
        {
            foreach (var collider in colliders)
            {
                if (OriginalLayers.TryGetValue(collider.gameObject.name, out var layer))
                    collider.gameObject.layer = layer;
            }
        }
        else
        {
            foreach (var collider in colliders)
            {
                var go = collider.gameObject;
                // We don't check if the collider is in the OriginalLayers, because we filter this earlier when we set the rig
                go.layer = SpectatorLayer;
            }
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

        var hands = new[]
        {
            _physicsRig.leftHand,
            _physicsRig.rightHand
        };
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
    
    // Debug
    
    public static bool IsLocalDisabled(Collider collider)
    {
        if (!OriginalLayers.ContainsKey(collider.name))
            return false;

        return true;
    }
}