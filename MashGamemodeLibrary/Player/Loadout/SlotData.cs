using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.Utilities;
using MelonLoader;
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
    private static HashSet<NetworkEntity> _spawnedGuns = new();
    
    public Barcode? Barcode;

    public SlotData()
    {
    }

    public SlotData(Barcode? barcode)
    {
        Barcode = barcode;
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

    public void AssignSlot(RigManager rig, SlotType slotType)
    {
        var slotName = GetSlotName(slotType);
        var slotBase = rig.inventory.bodySlots.FirstOrDefault(e => e.name.Equals(slotName));
        var slot = slotBase?.inventorySlotReceiver;
        if (ReferenceEquals(slot, null))
            return;

        if (slot._slottedWeapon != null && WeaponSlotExtender.Cache.TryGet(slot._slottedWeapon, out var entity))
        {
            slot.DropWeapon();
            PooleeUtilities.RequestDespawn(entity.ID, true);
        }
        
        if (Barcode == null)
            return;

        var spawnable = new Spawnable()
        {
            crateRef = new SpawnableCrateReference(Barcode),
            policyData = null,
        };

        var transform = slot.transform;
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnable,
            Position = transform.position,
            Rotation = transform.rotation,
            SpawnEffect = false,
            SpawnCallback = (info) =>
            {
                // Insert into known items
                _spawnedGuns.Add(info.Entity);
                
                var weaponSlotExtender = info.Entity.GetExtender<WeaponSlotExtender>();

                if (weaponSlotExtender == null)
                {
                    return;
                }

                var weaponSlot = weaponSlotExtender.Component;

                if (weaponSlot == null || weaponSlot.interactableHost == null)
                {
                    return;
                }

                slot.OnHandDrop(weaponSlot.interactableHost.TryCast<IGrippable>());
            },
        });
    }

    public static void ClearSpawned()
    {
        foreach (var entity in _spawnedGuns)
        {
            if (entity.IsDestroyed)
                continue;
            
            PooleeUtilities.RequestDespawn(entity.ID, true);
        }
        _spawnedGuns.Clear();
    }
}