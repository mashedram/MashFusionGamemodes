using Il2CppSLZ.Marrow;
using LabFusion.Extensions;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts.Holster;

internal class SlotContainerHider
{
    private GameObject? _art;
    
    // For some reason SlotContainers are not always set up properly
    // So we have backup names
    private static readonly string[] OtherNames = {
        "prop_pouch",
        "InventoryAmmoReceiver/Holder"
    };
    
    ~SlotContainerHider()
    {
        if (_art != null)
            _art.SetActive(true);
    }
    
    private static GameObject? GetArt(SlotContainer slotContainer)
    {
        if (slotContainer.art != null)
        {
            return slotContainer.art;
        }

        foreach (var name in OtherNames)
        {
            var art = slotContainer.transform.Find(name);
            if (art == null) continue;

            return art.gameObject;
        }

        return null;
    }
    
    public void SetSlotContainer(SlotContainer slotContainer)
    {
        var newArt = GetArt(slotContainer);
        if (newArt == _art) 
            return;
        
        if (_art != null)
            _art.SetActive(true);
        
        _art = newArt;
    }
    
    public void SetVisible(bool isVisible)
    {
        if (_art == null) return;
        
        _art.SetActive(isVisible);
    }
}

public class PlayerHolsterVisibility : IPlayerVisibility
{
    private bool _isVisible = true;
    private readonly Dictionary<string, SlotContainerHider> _holsterHiders = new();
    
    public void SetVisible(bool isVisible)
    {
        _isVisible = isVisible;
        _holsterHiders.Values.ForEach(hider => hider.SetVisible(isVisible));
    }
    
    public void OnRigChanged(RigManager? rigManager)
    {
        if (rigManager == null)
        {
            _holsterHiders.Clear();
            return;
        }

        var slots = rigManager.physicsRig.GetComponentsInChildren<SlotContainer>();
        if (slots == null)
            return;
        
        foreach (var slotContainer in slots)
        {
            var hider = _holsterHiders.GetValueOrCreate(slotContainer.name, () => new SlotContainerHider());
            hider.SetSlotContainer(slotContainer);
            hider.SetVisible(_isVisible);
        }
    }
}