using System.Collections.Immutable;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Scheduler;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;

public class CachedPhysicsRig
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
    public static readonly IReadOnlyList<int> SpectatorIgnoredLayers = new[]
    {
        BonelabLayers.Fixture,
        BonelabLayers.Player,
        BonelabLayers.NoCollide,
        BonelabLayers.Dynamic,
        BonelabLayers.EnemyColliders,
        BonelabLayers.Interactable,
        BonelabLayers.Deciverse,
        BonelabLayers.Socket,
        BonelabLayers.PlayerAndNPC
    };
    public static readonly int SpectatorIgnoredLayerMask = SpectatorIgnoredLayers.Aggregate(0, (mask, layer) => mask | (1 << layer));

    public ImmutableArray<CachedCollider> Colliders;

    static CachedPhysicsRig()
    {
        // We only want terrain layers
        Physics.IgnoreLayerCollision(SpectatorLayer, BonelabLayers.Default, false);

        // Ignore raycasts, so we don't have to worry about them when we set the rig


        // Ignore everything else
        Physics.IgnoreLayerCollision(SpectatorLayer, SpectatorLayer, true);
        foreach (var layer in SpectatorIgnoredLayers)
        {
            Physics.IgnoreLayerCollision(SpectatorLayer, layer, true);
        }
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
    
    public bool IsColliding { get; private set; }

    public void SetColliding(bool isColliding)
    {
        IsColliding = isColliding;

        // Set avatar layers

        foreach (var collider in Colliders)
        {
            collider.SetLayer(isColliding ? null : SpectatorLayer);
        }
        
        MarrowEntityCollisionScheduler.ScheduleRigCollisions(this, isColliding);
    }
}