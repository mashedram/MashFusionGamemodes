using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Vision.Holster;

namespace MashGamemodeLibrary.Vision;

internal class PlayerVisibilityState
{
    private bool _isSpecialHidden;
    private Avatar? _lastAvatar;
    
    private readonly NetworkPlayer _player;
    private readonly Dictionary<string, bool> _hideOverwrites = new();
    
    private readonly RenderSet _avatarRenderers = new();
    private readonly Dictionary<string, HolsterHider> _inventoryRenderers = new();
    private readonly RenderSet _specialRenderers = new();

    private readonly Dictionary<Handedness, RenderSet> _heldItems = new();

    private bool _isHiddenInternal = false;

    public bool IsHidden => _hideOverwrites.Any(e => e.Value);
    
    public PlayerVisibilityState(NetworkPlayer player, bool specialHidden)
    {
        _player = player;
        _isSpecialHidden = specialHidden;
        PopulateRenderers();
    }

    private bool IsValid()
    {
        return _avatarRenderers.IsValid;
    }

    private void SetHeadUI(bool hidden)
    {
        
        _player.HeadUI.Visible = !hidden;
    }

    private void PopulateHand(GrabData grabData)
    {
        if (!grabData.IsHoldingItem(out var item)) return;

        var set = new RenderSet(item.GameObject, IsHidden);

        _heldItems[grabData.Hand.handedness] = set;
    }
    
    public void PopulateRenderers()
    {
        _avatarRenderers.Clear();
        _inventoryRenderers.Clear();
        _specialRenderers.Clear();
        
        if (!_player.HasRig)
        {
            _lastAvatar = null;
            _isHiddenInternal = false;
            return;
        }
        
        var rigManager = _player.RigRefs.RigManager;

        _isHiddenInternal = IsHidden;

        _avatarRenderers.Set(rigManager.avatar.gameObject, _isHiddenInternal);
        
        foreach (var slotContainer in rigManager.inventory.bodySlots)
        {
            if (!slotContainer || !slotContainer.gameObject)
                continue;
            _inventoryRenderers[slotContainer.name] = new HolsterHider(slotContainer, _isHiddenInternal);
        }
        
        // Check for dualAction compatibility
        var rightBelt = rigManager.physicsRig.m_pelvis.FindChild("BeltRt1");
        if (rightBelt != null)
        {
            // TODO: Check what exists on the actual object
            // _inventoryRenderers[rightBelt.name]
        }
        
        foreach (var slotContainer in rigManager.inventory.specialItems)
        {
            if (!slotContainer || !slotContainer.gameObject)
                continue;
            _specialRenderers.Add(slotContainer.gameObject);
        }

        var hands = new[] { rigManager.physicsRig.leftHand, rigManager.physicsRig.rightHand };
        foreach (var hand in hands)
        {
            PopulateHand(new GrabData(hand));
        }
    }
    
    public void SetSpecialsHidden(bool hidden)
    {
        _isSpecialHidden = hidden;
        
        _specialRenderers.SetHidden(_isSpecialHidden);
    }

    private void Refresh()
    {
        var hidden = IsHidden;

        if (!_player.HasRig)
        {
            _isHiddenInternal = false;
            return;
        }

        _isHiddenInternal = hidden;

        if (!IsValid())
        {
            PopulateRenderers();
            return;
        }
        
        _avatarRenderers.SetHidden(_isHiddenInternal);
        
        _inventoryRenderers.Values.ForEach(v => v.SetHidden(hidden));
        
        _specialRenderers.SetHidden(_isHiddenInternal);

        foreach (var rendererVisibility in _heldItems.Values.Select(heldItem => heldItem))
        {
            rendererVisibility.SetHidden(hidden);
        }

        SetHeadUI(hidden);
    }
    
    public void SetHidden(string key, bool hidden)
    {
        _hideOverwrites[key] = hidden;
        
        Refresh();
    }

    public void Reset()
    {
        _isSpecialHidden = false;

        _hideOverwrites.Clear();
        Refresh();
    }

    public void OnGrab(GrabData hand)
    {
        PopulateHand(hand);
    }

    public void OnDrop(GrabData hand)
    {
        if (!_heldItems.Remove(hand.Hand.handedness, out var renderers)) return;
        renderers.SetHidden(false);
    }
    
    public void OnHolster(InventoryHandReceiver slotReceiver)
    {
        var name = slotReceiver.transform.parent.name;

        if (_inventoryRenderers.TryGetValue(name, out var item))
        {
            item.Update(IsHidden);
            return;
        }
        
        _inventoryRenderers[name] = new HolsterHider(slotReceiver, IsHidden);
    }
    
    public void OnUnholster(InventorySlotReceiver slotReceiver)
    {
        var name = slotReceiver.transform.parent.name;
        
        if (!_inventoryRenderers.TryGetValue(name, out var hider)) 
            return;
        
        hider.Update(IsHidden);
    }

    public void Update()
    {
        if (!_player.HasRig)
        {
            _lastAvatar = null;
            return;
        }

        var rigManager = _player.RigRefs.RigManager;
        
        var avatar = rigManager.avatar;

        if (avatar == null)
        {
            _lastAvatar = null;
            return;
        }
            
        if (_lastAvatar != null && avatar == _lastAvatar && _isHiddenInternal == IsHidden || !IsValid())
            return;
        
        _lastAvatar = avatar;
        
        PopulateRenderers();
    }
}