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
    
    public void SetHidden(bool hidden)
    {
        _renderSet.SetHidden(hidden);
    }

    public void Update(bool? hidden = null)
    {
        if (hidden.HasValue)
        {
            _renderSet.SetHidden(hidden.Value);
        }
        
        _renderSet.Clear();
        
        if (!_receiver._slottedWeapon)
            return;

        var gameObject = _receiver._weaponHost.GetHostGameObject();
        if (!gameObject)
            return;
        
        _renderSet.Set(gameObject);
    }
}