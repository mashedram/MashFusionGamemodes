using Il2CppSLZ.Marrow.Data;
using LabFusion.Marrow.Pool;
using LabFusion.RPC;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Declerations;
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


    public static void SpawnNetworkAsset(string barcode, Vector3 position, params IComponent[] components)
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
                    if (components.Length > 0)
                    {
                        MelonLogger.Error($"Failed to add tags to: {barcode}. Spawned crate is not synced.");
                    }
                    return;
                }
                
                // We're done, no extra behavior
                if (components.Length == 0)
                    return;
                
                Executor.RunIfHost(() =>
                {
                    result.WaitOnMarrowEntity((entity, _) =>
                    {
                        foreach (var component in components)
                        {
                            entity.AddComponent(component);
                        }
                    });
                });
            }
        });
    }

    public static void DespawnAll<T>() where T : IComponent
    {
        Executor.RunIfHost(() =>
        {
            foreach (var id in EcsManager.GetEntityIdsWithComponent<T>())
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