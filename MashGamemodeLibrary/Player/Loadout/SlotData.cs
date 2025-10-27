using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.RPC;
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
    public Barcode? Barcode;
    public bool ShouldOverride = true;

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

    public void AssignSlot(RigManager rig, SlotType slotType, Action<Poolee>? callback)
    {
        var slotName = GetSlotName(slotType);
        var slotBase = rig.inventory.bodySlots.FirstOrDefault(e => e.name.Equals(slotName));
        var slot = slotBase?.inventorySlotReceiver;
        if (ReferenceEquals(slot, null))
            return;

        if (ShouldOverride)
            slot.DespawnContents();
        
        if (Barcode == null)
            return;

        var task = slot.SpawnInSlotAsync(Barcode);
        if (callback == null)
            return;

        var awaiter = task.GetAwaiter();
        awaiter.OnCompleted((Il2CppSystem.Action)(() => callback.Invoke(slot._slottedWeapon.GetComponentInParent<Poolee>())));
    }
}