using Il2CppSLZ.Marrow;
using UnityEngine;

namespace MashGamemodeLibrary.Vision.Holster.Receivers;

internal class InventoryAmmoReceiverHider : IReceiverHider
{
    private readonly InventoryAmmoReceiver _receiver;
    private GameObject? _art;
    private bool _isHidden;

    public InventoryAmmoReceiverHider(InventoryAmmoReceiver receiver, bool hidden)
    {
        _receiver = receiver;
        _isHidden = hidden;

        Update();
    }

    public bool SetHidden(bool hidden)
    {
        _isHidden = hidden;
        if (_art == null)
            return false;

        _art.SetActive(!hidden);
        return true;
    }

    public bool Update(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            if (!SetHidden(hidden.Value))
                return false;
        }

        _art = _receiver.transform.FindChild("Holder").gameObject;

        return true;
    }
}