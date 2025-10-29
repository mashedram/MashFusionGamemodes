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

        FetchRenderers();
    }

    public bool SetHidden(bool hidden)
    {
        _isHidden = hidden;
        if (_art == null)
            return false;

        _art.SetActive(!hidden);
        return true;
    }

    public bool FetchRenderers(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            _isHidden = hidden.Value;
        }

        _art = _receiver.transform.FindChild("Holder").gameObject;
        _art.SetActive(_isHidden);

        return true;
    }
}