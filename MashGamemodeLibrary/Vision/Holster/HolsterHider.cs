using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Vision.Holster.Receivers;

namespace MashGamemodeLibrary.Vision.Holster;

internal class HolsterHider
{
    private readonly RenderSet _holsterSet;
    private readonly IReceiverHider _receiver;

    public HolsterHider(SlotContainer container, bool hidden)
    {
        if (container.inventorySlotReceiver)
        {
            var receiver = container.inventorySlotReceiver;
            var art =
                container.art ??
                container.transform.FindChild("prop_pouch")?.gameObject;
            
            _holsterSet = new RenderSet(art, hidden);
            _receiver = new InventorySlotReceiverHider(container.inventorySlotReceiver, hidden);
            return;
        }

        if (container.inventoryAmmoReceiver)
        {
            var receiver = container.inventoryAmmoReceiver;
            var art =
                receiver.transform.FindChild("Holder")?.gameObject;
            
            _holsterSet = new RenderSet(art, hidden);
            _receiver = new InventoryAmmoReceiverHider(receiver, hidden);
            return;
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