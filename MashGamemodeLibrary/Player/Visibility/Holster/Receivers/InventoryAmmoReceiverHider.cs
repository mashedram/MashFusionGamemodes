using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Vision.Holster;

namespace MashGamemodeLibrary.Player.Visibility.Holster.Receivers;

internal class InventoryAmmoReceiverHider : IReceiverHider
{
    private readonly InventoryAmmoReceiver _receiver;
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
        if (_receiver == null)
            return false;

        _receiver.gameObject.SetActive(!_isHidden);
        return true;
    }

    public bool FetchRenderers(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            _isHidden = hidden.Value;
        }

        _receiver.gameObject.SetActive(!_isHidden);

        return true;
    }

    public void Update()
    {
        if (!_isHidden)
            return;

        if (_receiver != null && !_receiver.isActiveAndEnabled)
            return;

        FetchRenderers(_isHidden);
    }
}