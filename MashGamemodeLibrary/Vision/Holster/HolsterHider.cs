using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Vision.Holster.Receivers;

namespace MashGamemodeLibrary.Vision.Holster;

internal class HolsterHider
{
    private readonly RenderSet _holsterSet;
    private readonly IReceiverHider _receiver;

    public HolsterHider(SlotContainer container, bool hidden)
    {
        _holsterSet = new RenderSet(container.art, hidden);
        if (container.inventorySlotReceiver)
        {
            _receiver = new InventorySlotReceiverHider(container.inventorySlotReceiver, hidden);
        }

        if (container.inventoryAmmoReceiver)
        {
            _receiver = new InventoryAmmoReceiverHider(container.inventoryAmmoReceiver, hidden);
        }
        
        throw new Exception("Invalid holster type, no receiver found!");
    }

    public HolsterHider(InventoryHandReceiver receiver, bool hidden)
    {
        if (receiver is InventorySlotReceiver slotReceiver)
        {
            _receiver = new InventorySlotReceiverHider(slotReceiver, hidden);
        }
        
        if (receiver is InventoryAmmoReceiver ammoReceiver)
        {
            _receiver = new InventoryAmmoReceiverHider(ammoReceiver, hidden);
        }
        
        throw new Exception("Invalid holster type, no receiver found!");
    }

    public void Update(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            _holsterSet.SetHidden(hidden.Value);
        }
        
        _receiver.Update(hidden);
    }
    
    public void SetHidden(bool hidden)
    {
        _holsterSet.SetHidden(hidden);
        _receiver.SetHidden(hidden);
    }
}