using Il2CppSLZ.Marrow;

namespace MashGamemodeLibrary.Vision.Holster.Receivers;

internal class InventoryAmmoReceiverHider : IReceiverHider
{
    private readonly InventoryAmmoReceiver _receiver;
    private readonly RenderSet _renderSet;
    
    public InventoryAmmoReceiverHider(InventoryAmmoReceiver receiver, bool hidden)
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

        var magazines = _receiver._magazineArts;
        if (magazines == null)
            return;
        if (magazines.Count == 0)
            return;
        
        foreach (var magazine in magazines)
        {
            if (magazine == null)
                continue;
            if (magazine.gameObject == null)
                continue;
            
            _renderSet.Add(magazine.gameObject);
        }
    }
}