using Clockhunt.Entities.Tags;
using Clockhunt.Nightmare;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.RPC;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging;
using UnityEngine;

namespace Clockhunt.Entities;

public static class ClockManager
{
    private static readonly string ClockBarcode = "SLZ.BONELAB.Content.Spawnable.AlarmClock";

    private static Spawnable GetSpawnable()
    {
        var reference = new SpawnableCrateReference(new Barcode(ClockBarcode));
        var spawnable = new Spawnable
        {
            crateRef = reference
        };

        AssetSpawner.Register(spawnable);
        return spawnable;
    }

    public static void SpawnEntityForPlayer(NetworkPlayer player)
    {
        var spawnable = GetSpawnable();
        var position = player.RigRefs.RightHand.transform.position;

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        {
            Position = position,
            Rotation = Quaternion.identity,
            Spawnable = spawnable,
            SpawnCallback = result =>
            {
                result.Entity.AddTag(new ClockMarker());
                result.Entity.AddTag(new EntityOwner(player));
                result.Entity.AddTag(new ObjectiveCollectable());
                result.Entity.AddTag(new ClockLight());
            }
        });
    }

    public static bool IsClockEntity(NetworkEntity entity)
    {
        return entity.HasTag<ClockMarker>();
    }

    public static int CountClockEntities()
    {
        return EntityTagManager.CountEntitiesWithTag<ClockMarker>();
    }

    public static HashSet<NetworkEntity> GetAllClockEntities()
    {
        return EntityTagManager.GetAllWithTag<ClockMarker>();
    }

    public static void RemoveUntilCount(int count)
    {
        var clocks = GetAllClockEntities();
        var toRemove = clocks.Count - count;
        if (toRemove <= 0) return;

        var survivorClocks = clocks
            .Where(e => e.TryGetTag<EntityOwner>(out var owner) && owner.NetworkPlayer != null &&
                        !NightmareManager.IsNightmare(owner.NetworkPlayer.PlayerID)).ToList();

        survivorClocks.Shuffle();

        foreach (var clock in survivorClocks.Take(toRemove))
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
            {
                DespawnEffect = true,
                EntityID = clock.ID
            });
            toRemove--;
            if (toRemove <= 0)
                return;
        }
    }

    public static void ClearClocks()
    {
        GameAssetSpawner.DespawnAll<ClockMarker>();
    }

    public static void AttemptCollection()
    {
    }
}