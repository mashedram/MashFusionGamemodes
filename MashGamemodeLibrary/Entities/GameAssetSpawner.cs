using Il2CppSLZ.Marrow.Data;
using LabFusion.Marrow.Pool;
using LabFusion.RPC;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Execution;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Entities;

public static class GameAssetSpawner
{
    private static Spawnable GetSpawnable(string barcode)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);
        LocalAssetSpawner.Register(spawnable);
        return spawnable;
    }


    public static void SpawnNetworkAsset(string barcode, Vector3 position, params IEntityTag[] tags)
    {
        var spawnable = GetSpawnable(barcode);
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        {
            Spawnable = spawnable,
            Position = position,
            Rotation = Quaternion.identity,
            SpawnEffect = false,
            SpawnCallback = result =>
            {
                // Entity is not synced, skip attaching values
                if (result.Entity == null)
                {
                    if (tags.Length > 0)
                    {
                        MelonLogger.Error($"Failed to add tags to: {barcode}. Spawned crate is not synced.");
                    }
                    return;
                }
                
                // We're done, no extra behavior
                if (tags.Length == 0)
                    return;
                
                Executor.RunIfHost(() =>
                {
                    result.WaitOnMarrowEntity((entity, _) =>
                    {
                        foreach (var entityTag in tags)
                        {
                            entity.AddTag(entityTag);
                        }
                    });
                });
            }
        });
    }

    public static void DespawnAll<T>() where T : IEntityTag
    {
        Executor.RunIfHost(() =>
        {
            foreach (var id in EntityTagManager.GetAllIdsWithTag<T>())
            {
                NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
                {
                    EntityID = id,
                    DespawnEffect = true
                });
            }
        });
    }
}