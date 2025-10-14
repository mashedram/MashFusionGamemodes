using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Entities;
using LabFusion.Network;
using MelonLoader;

namespace MashGamemodeLibrary.Loadout;

public class Loadout
{
    private static readonly List<SlotType> AllSlotTypes = Enum.GetValues(typeof(SlotType)).Cast<SlotType>().ToList();
    private static readonly SlotData DefaultSlotData = new(null, true);
    private readonly Dictionary<SlotType, SlotData> _slotAssigners = new();

    public Loadout()
    {
        foreach (SlotType slotType in Enum.GetValues(typeof(SlotType))) _slotAssigners[slotType] = new SlotData();
    }

    public Loadout SetSlotBarcode(SlotType slotType, Barcode? barcode, bool shouldOverwrite = true)
    {
        _slotAssigners[slotType] = new SlotData(barcode, shouldOverwrite);
        return this;
    }

    public Loadout ClearSlot(SlotType slotType)
    {
        _slotAssigners[slotType] = new SlotData(null, true);
        return this;
    }

    public Loadout IgnoreSlot(SlotType slotType)
    {
        _slotAssigners[slotType] = new SlotData();
        return this;
    }

    public Loadout IgnoreAllSlots()
    {
        foreach (var slotType in AllSlotTypes) IgnoreSlot(slotType);

        return this;
    }

    public void Assign(RigRefs rig, Action<NetworkEntity>? onAssign = null)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Msg("[Loadout] Cannot assign loadout on client!");
            return;
        }

        foreach (var slotType in AllSlotTypes)
        {
            var slotData = _slotAssigners.GetValueOrDefault(slotType, DefaultSlotData);
            slotData.AssignSlot(rig, slotType, onAssign);
        }
    }

    public static void ClearPlayerLoadout(RigRefs rig)
    {
        foreach (SlotType slotType in Enum.GetValues(typeof(SlotType))) DefaultSlotData.AssignSlot(rig, slotType, null);

        ClearHeadSlot(rig);
    }

    public static void ClearHeadSlot(RigRefs rig)
    {
        var headSlot = rig.RigManager.physicsRig.m_head.FindChild("HeadSlotContainer")
            ?.GetComponentInChildren<InventorySlotReceiver>();
        if (headSlot == null) return;

        headSlot.DespawnContents();
    }
}