using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Utilities;
using MashGamemodeLibrary.Util;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;

internal class SlotContainerHider
{
    private GameObject? _art;
    
    private static readonly string[] HolsterSlotNames = new[]
    {
        "prop_handGunHolster",
        "prop_pouch",
        "InventoryAmmoReceiver"
    };

    private static GameObject? GetArt(SlotContainer slotContainer)
    {
        if (slotContainer == null)
            return null;

        var transform = slotContainer.transform;
        foreach (var holsterSlotName in HolsterSlotNames)
        {
            var art = transform.Find(holsterSlotName)?.gameObject;
            if (art != null)
                return art;
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

    public void SetVisible(PlayerVisibility visibility)
    {
        _isVisible = visibility.VisibleForLocalPlayer;
        _holsterHiders.Values.ForEach(hider => hider.SetVisible(_isVisible));
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _holsterHiders.Clear();

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

    public void OnAvatarChanged(Avatar avatar)
    {
        // No-Op
    }
}