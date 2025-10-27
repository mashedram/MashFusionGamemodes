using Il2CppSLZ.Marrow;

namespace MashGamemodeLibrary.Vision.Holster.Receivers;

internal class InventorySlotReceiverHider : IReceiverHider
{
    private readonly InventorySlotReceiver _receiver;
    private readonly RenderSet _renderSet;

    public InventorySlotReceiverHider(InventorySlotReceiver receiver, bool hidden)
    {
        _receiver = receiver;
        _renderSet = new RenderSet(hidden);

        Update();
    }

    public bool SetHidden(bool hidden)
    {
        return _renderSet.SetHidden(hidden);
    }

    public bool Update(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            if (!_renderSet.SetHidden(hidden.Value))
                return false;
        }

        _renderSet.Clear();

        if (!_receiver._slottedWeapon)
            return true;

        var gameObject = _receiver._weaponHost.GetHostGameObject();
        if (!gameObject)
            return true;

        return _renderSet.Set(gameObject);
    }
}