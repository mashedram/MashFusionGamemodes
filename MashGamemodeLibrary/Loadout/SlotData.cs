using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Player;
using LabFusion.RPC;
using UnityEngine;

namespace MashGamemodeLibrary.Loadout;

public enum SlotType
{
    LeftHolster,
    RightHolster,
    LeftBack,
    RightBack,
    Belt
}

public class SlotData
{
    public bool ShouldOverride = true;
    public Barcode? Barcode;

    public SlotData()
    {
        
    }
    
    public SlotData(Barcode? barcode, bool shouldOverride = false)
    {
        Barcode = barcode;
        ShouldOverride = shouldOverride;
    }
    
    public static string GetSlotName(SlotType type)
    {
        return type switch
        {
            SlotType.LeftHolster => "SideLf",
            SlotType.RightHolster => "SideRt",
            SlotType.LeftBack => "BackLf",
            SlotType.RightBack => "BackRt",
            SlotType.Belt => "BackCt",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static NetworkEntity? GetSlotNetworkEntity(InventorySlotReceiver slot)
    {
        var itemInSlot = slot._weaponHost?.GetGrip()?._marrowEntity;
        if (itemInSlot == null)
            return null;
        
        return IMarrowEntityExtender.Cache.TryGet(itemInSlot, out var entity) ? entity : null;
    }
    
    public void AssignSlot(RigRefs rig, SlotType slotType, Action<NetworkEntity>? callback)
    {
        var slotName = GetSlotName(slotType);
        var slotBase = rig.RigManager.inventory.bodySlots.FirstOrDefault(e => e.name.Equals(slotName));
        var slot = slotBase?._inventorySlot;
        if (ReferenceEquals(slot, null))
            return;

        var networkEntity = GetSlotNetworkEntity(slot);
        
        if (Barcode == null)
        {
            if (ShouldOverride && networkEntity != null)
            {
                NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
                {
                    EntityID = networkEntity.ID,
                    DespawnEffect = false
                });
            }
            
            return;
        }
        
        if (networkEntity != null && !ShouldOverride)
            return;

        var spawnPosition = rig.Head.transform.position + rig.Head.transform.forward * -0.5f;
        
        var spawnable = LocalAssetSpawner.CreateSpawnable(new SpawnableCrateReference(Barcode));
        if (spawnable == null)
            return;
        LocalAssetSpawner.Register(spawnable);
        
        if (networkEntity != null)
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
            {
                EntityID = networkEntity.ID,
                DespawnEffect = false
            });
        }
        
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        {
            Position = spawnPosition,
            Rotation = Quaternion.identity,
            Spawnable = spawnable,
            SpawnCallback = result =>
            {
                callback?.Invoke(result.Entity);
                if (!result.Spawned.TryGetComponent<InteractableHost>(out var interactableHost))
                    return;
                slot.InsertInSlot(interactableHost);
            },
            SpawnEffect = false
        });
    }
}