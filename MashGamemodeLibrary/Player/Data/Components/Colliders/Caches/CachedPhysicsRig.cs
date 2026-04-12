using System.Collections;
using System.Collections.Immutable;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Data.Components.Colliders.Caches;
using MashGamemodeLibrary.Player.Spectating.data.Colliders;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;

public class CachedPhysicsRig : ICachedCollider, IDisableableCollider
{
    private static readonly int SpectatorLayer = BonelabLayers.NoRaycast;

    private static readonly Dictionary<string, int> PhysicsRigLayout = new()
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
        {
            "Knee", BonelabLayers.Feet
        },
        {
            "KneetoPelvis", BonelabLayers.Feet
        },
        // From second list (Deci variants):
        {
            "DeciHead", BonelabLayers.Deciverse
        },
        {
            "DeciChest", BonelabLayers.Deciverse
        },
        {
            "DeciShoulderLf", BonelabLayers.Deciverse
        },
        {
            "DeciElbowLf", BonelabLayers.Deciverse
        },
        {
            "DeciHandLf", BonelabLayers.Deciverse
        },
        {
            "DeciShoulderRt", BonelabLayers.Deciverse
        },
        {
            "DeciElbowRt", BonelabLayers.Deciverse
        },
        {
            "DeciHandRt", BonelabLayers.Deciverse
        },
        {
            "DeciSpine", BonelabLayers.Deciverse
        },
        {
            "DeciPelvis", BonelabLayers.Deciverse
        },
        {
            "DeciHipLf", BonelabLayers.Deciverse
        },
        {
            "DeciHipRt", BonelabLayers.Deciverse
        }
    };

    public ImmutableArray<CachedCollider> Colliders;

    static CachedPhysicsRig()
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

    public CachedPhysicsRig(PhysicsRig physicsRig)
    {
        PhysicsRig = physicsRig;

        Colliders = physicsRig
            .GetComponentsInChildren<Collider>()
            .Select(c => PhysicsRigLayout.TryGetValue(c.name, out var sourceLayer) ? new CachedCollider(c, sourceLayer) : null)
            .OfType<CachedCollider>()
            .ToImmutableArray();
    }

    public PhysicsRig PhysicsRig { get; init; }

    public IEnumerator<Collider> GetEnumerator()
    {
        return Colliders.Select(cachedCollider => cachedCollider.Collider).GetEnumerator();
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
    public bool IsColliding { get; private set; }

    // TODO ON THIS
    // - Layer changes on colliders
    // - Marrow Entity collecting
    public void SetColliding(bool isColliding)
    {
        IsColliding = isColliding;

        // Set avatar layers

        foreach (var collider in Colliders)
        {
            collider.SetLayer(isColliding ? null : SpectatorLayer);
        }

        // Resolve marrow bodies
        foreach (var cachedEntity in CachedColliderCache.CachedEntities)
        {
            // Don't set colliding with ourselves, that would be bad
            if (cachedEntity == this)
                continue;
            
            SetColliding(cachedEntity, isColliding);
        }
    }
    public void OnColliderCached(ICachedCollider cachedCollider)
    {
        if (IsColliding)
            return;

        SetColliding(cachedCollider, IsColliding);
    }
}