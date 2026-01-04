using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.RPC;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities;

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
    private static readonly HashSet<Poolee> SpawnedGuns = new();

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

        var slottedItem = slot._slottedWeapon?.grip?._marrowEntity?._poolee;
        if (slottedItem != null)
        {
            slottedItem.Despawn();
        }

        if (Barcode == null)
            return;

        var spawnable = new Spawnable
        {
            crateRef = new SpawnableCrateReference(Barcode),
            policyData = null
        };

        var transform = slot.transform;
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        {
            Spawnable = spawnable,
            Position = transform.position,
            Rotation = transform.rotation,
            SpawnEffect = false,
            SpawnCallback = info =>
            {
                // Insert into known items
                // TODO: Add ownership here to return guns to their owners
                info.WaitOnMarrowEntity((networkEntity, marrowEntity) =>
                {
                    SpawnedGuns.Add(marrowEntity._poolee);
                    
                    var weaponSlotExtender = networkEntity.GetExtender<WeaponSlotExtender>();

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
                });
            }
        });
    }

    public static void ClearSpawned()
    {
        foreach (var entity in SpawnedGuns)
        {
            if (entity == null)
                continue;
            if (!entity.isActiveAndEnabled)
                continue;
            entity.Despawn();
        }
        SpawnedGuns.Clear();
    }
}