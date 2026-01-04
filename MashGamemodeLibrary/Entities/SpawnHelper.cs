using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.RPC;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities;

public delegate void OnMarrowEntitySpawned(NetworkEntity networkEntity, MarrowEntity entity);

public static class SpawnHelper
{
    public static void WaitOnMarrowEntity(this NetworkAssetSpawner.SpawnCallbackInfo callbackInfo, OnMarrowEntitySpawned callback)
    {
        if (callbackInfo.Entity == null)
        {
            InternalLogger.Debug($"Spawned entity with name: {callbackInfo.Spawned.name} is not synced");
            return;
        }
        WaitOnMarrowEntity(callbackInfo.Entity, callback);
    }
    public static void WaitOnMarrowEntity(this NetworkEntity networkEntity, OnMarrowEntitySpawned callback)
    {
        callback(networkEntity, GetMarrowEntity(networkEntity));
    }
    
    public static void WaitOnMarrowEntity(this NetworkEntityReference entityReference, OnMarrowEntitySpawned callback)
    {
        WaitOnMarrowEntity(entityReference.ID, callback);
    }
    
    public static void WaitOnMarrowEntity(ushort entityId, OnMarrowEntitySpawned callback)
    {
        NetworkEntityManager.HookEntityRegistered(entityId, networkEntity =>
        {
            callback.Invoke(networkEntity, GetMarrowEntity(networkEntity));
        });
    }

    private static MarrowEntity GetMarrowEntity(NetworkEntity networkEntity)
    { 
        return networkEntity.GetExtender<NetworkProp>()?.MarrowEntity ?? networkEntity.GetExtender<NetworkPlayer>().MarrowEntity;
    }
}