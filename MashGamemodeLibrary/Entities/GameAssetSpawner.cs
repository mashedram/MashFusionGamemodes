using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Player;
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

    public static void Despawn(NetworkEntity networkEntity)
    {
        if (networkEntity.ID <= PlayerIDManager.MaxPlayerID)
        {
            MelonLogger.Error($"Attempted to despawn a player entity: {networkEntity.ID}");
            return;
        }

        NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
        {
            EntityID = networkEntity.ID,
            DespawnEffect = true
        });
    }
    
    // Local
    public static void SpawnLocalAsset(string barcode, Vector3 position, Action<Poolee> callback)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, position, Quaternion.identity, callback);
    }
}