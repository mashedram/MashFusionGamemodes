using Clockhunt.Entities.Tags;
using Clockhunt.Nightmare;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.RPC;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.Tagging;
using UnityEngine;

namespace Clockhunt.Entities;

public static class ClockManager
{
    private static readonly string ClockBarcode = "SLZ.BONELAB.Content.Spawnable.AlarmClock";
    
    public static void SpawnEntityForPlayer(NetworkPlayer player)
    {
        var position = player.RigRefs.RightHand.transform.position;

        GameAssetSpawner.SpawnNetworkAsset(
            ClockBarcode,
            position,
            new ClockMarker(player),
            new ObjectiveCollectable(),
            new ClockLight()
        );
        // NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        // {
        //     Position = position,
        //     Rotation = Quaternion.identity,
        //     Spawnable = spawnable,
        //     SpawnCallback = result =>
        //     {
        //         result.Entity.AddTag(new ClockMarker());
        //         result.Entity.AddTag(new EntityOwner(player));
        //         result.Entity.AddTag(new ObjectiveCollectable());
        //         result.Entity.AddTag(new ClockLight());
        //     }
        // });
    }

    public static bool IsClockEntity(NetworkEntity entity)
    {
        return entity.GetComponent<ClockMarker>() != null;
    }

    public static int CountClockEntities()
    {
        return ClockMarker.Query.Count();
    }

    public static void RemoveUntilCount(int count)
    {
        var clocks = ClockMarker.Query.ToList();
        var toRemove = clocks.Count - count;
        if (toRemove <= 0) return;

        var survivorClocks = clocks
            .Where(e => e.Component.Owner != null &&
                        !NightmareManager.IsNightmare(e.Component.Owner.PlayerID)).ToList();

        survivorClocks.Shuffle();

        foreach (var clock in survivorClocks.Take(toRemove))
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
            {
                DespawnEffect = true,
                EntityID = clock.Instance.EntityId
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