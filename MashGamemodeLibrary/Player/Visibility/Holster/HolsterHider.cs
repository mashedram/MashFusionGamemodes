using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Visibility.Holster.Receivers;
using MashGamemodeLibrary.Vision.Holster.Receivers;
using UnityEngine;

namespace MashGamemodeLibrary.Vision.Holster;

internal class HolsterHider
{
    private readonly RenderSet? _holsterSet;
    private readonly IReceiverHider? _receiver;

    public HolsterHider(SlotContainer container, bool hidden)
    {
        if (container.inventorySlotReceiver)
        {
            var receiver = container.inventorySlotReceiver;
            var art = container.art;

            if (art == null)
            {
                var pouch = container.transform.FindChild("prop_pouch");
                if (pouch != null) art = pouch.gameObject;
            }

            _holsterSet = new RenderSet(art, hidden);
            _receiver = new InventorySlotReceiverHider(receiver, hidden);
            return;
        }

        if (container.inventoryAmmoReceiver)
        {
            var receiver = container.inventoryAmmoReceiver;
            GameObject? art = null;
            var holder = receiver.transform.FindChild("Holder");
            if (holder != null) art = holder.gameObject;

            _holsterSet = new RenderSet(art, hidden);
            _receiver = new InventoryAmmoReceiverHider(receiver, hidden);
            return;
        }

        _holsterSet = new RenderSet(container.gameObject, hidden);
    }

    public HolsterHider(InventoryHandReceiver receiver, bool hidden)
    {
        if (_receiver == null)
            return;

        _receiver = receiver switch
        {
            InventorySlotReceiver slotReceiver => new InventorySlotReceiverHider(slotReceiver, hidden),
            InventoryAmmoReceiver ammoReceiver => new InventoryAmmoReceiverHider(ammoReceiver, hidden),
            _ => _receiver
        };

    }

    public bool FetchRenderers(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            if (_holsterSet != null && !_holsterSet.SetHidden(hidden.Value))
                return false;
        }

        if (_receiver == null)
            return true;

        return _receiver.FetchRenderers(hidden);
    }

    public bool FetchRenderersIf<T>(bool? hidden = null) where T : IReceiverHider
    {
        if (_receiver == null || _receiver.GetType() != typeof(T))
            return true;

        return FetchRenderers(hidden);
    }

    public void Update()
    {
        _receiver?.Update();
    }

    public bool SetHidden(bool hidden)
    {
        if (_holsterSet != null && !_holsterSet.SetHidden(hidden))
            return false;
        
        return _receiver == null || _receiver.SetHidden(hidden);
    }
}