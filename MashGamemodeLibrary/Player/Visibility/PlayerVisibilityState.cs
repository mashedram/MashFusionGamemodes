using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Player.Visibility.Holster.Receivers;
using MashGamemodeLibrary.Vision;
using MashGamemodeLibrary.Vision.Holster;
using MashGamemodeLibrary.Vision.Holster.Receivers;

namespace MashGamemodeLibrary.Player.Visibility;

internal class PlayerVisibilityState
{
    private readonly RenderSet _avatarRenderers = new();

    private readonly Dictionary<Handedness, RenderSet> _heldItems = new();
    private readonly Dictionary<string, bool> _hideOverwrites = new();
    private readonly Dictionary<InventoryHandReceiver, HolsterHider> _inventoryRenderers = new();
    private readonly HashSet<SlotContainer> _slotContainers = new();
    
    private readonly NetworkPlayer _player;

    private bool _isHiddenInternal;
    private bool _isSpecialHidden;
    private Avatar? _lastAvatar;

    public PlayerVisibilityState(NetworkPlayer player, bool specialHidden)
    {
        _player = player;
        _isSpecialHidden = specialHidden;
        PopulateRenderers();
    }

    public bool IsHidden => _hideOverwrites.Any(e => e.Value);

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
        _slotContainers.Clear();

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

            InventoryHandReceiver receiver = slotContainer.inventorySlotReceiver != null
                ? slotContainer.inventorySlotReceiver
                : slotContainer.inventoryAmmoReceiver;
            if (receiver == null)
                continue;

            _inventoryRenderers[receiver] = new HolsterHider(slotContainer, _isHiddenInternal);
        }

        // Check for dualAction compatibility
        var rightBelt = rigManager.physicsRig.m_pelvis.FindChild("BeltRt1");
        if (rightBelt != null && rightBelt.TryGetComponent<SlotContainer>(out var rightBeltSlot))
        {
            var receiver = rightBeltSlot.inventoryAmmoReceiver;
            _inventoryRenderers[receiver] = new HolsterHider(rightBeltSlot, _isHiddenInternal);
        }

        foreach (var slotContainer in rigManager.inventory.specialItems)
        {
            if (!slotContainer || !slotContainer.gameObject)
                continue;

            _slotContainers.Add(slotContainer);
        }

        var hands = new[] { rigManager.physicsRig.leftHand, rigManager.physicsRig.rightHand };
        foreach (var hand in hands) PopulateHand(new GrabData(hand));
    }

    public void SetSpecialsHidden(bool hidden)
    {
        _isSpecialHidden = hidden;

        foreach (var slotContainer in _slotContainers)
        {
            slotContainer.gameObject.SetActive(!(_isHiddenInternal || _isSpecialHidden));
        }
    }

    private bool Refresh()
    {
        var hidden = IsHidden;

        if (!_player.HasRig)
        {
            _isHiddenInternal = false;
            return false;
        }

        _isHiddenInternal = hidden;

        if (!_avatarRenderers.SetHidden(_isHiddenInternal))
        {
            return false;
        }

        foreach (var inventoryRenderersValue in _inventoryRenderers.Values)
        {
            if (inventoryRenderersValue.SetHidden(hidden))
                continue;
            
            return false;
        }

        foreach (var slotContainer in _slotContainers)
        {
            if (slotContainer == null)
                return false;
            
            slotContainer.gameObject.SetActive(!(_isHiddenInternal || _isSpecialHidden));
        }

        foreach (var rendererVisibility in _heldItems.Values.Select(heldItem => heldItem))
            rendererVisibility.SetHidden(hidden);

        SetHeadUI(hidden);
        return true;
    }
    
    private void RefreshAndPopulate()
    {
        if (!Refresh())
            PopulateRenderers();
    }

    public void SetHidden(string key, bool hidden)
    {
        _hideOverwrites[key] = hidden;

        RefreshAndPopulate();
    }

    public void Reset()
    {
        _isSpecialHidden = false;

        _hideOverwrites.Clear();
        RefreshAndPopulate();
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
        if (_inventoryRenderers.TryGetValue(slotReceiver, out var item))
        {
            if (!item.FetchRenderers(IsHidden))
            {
                PopulateRenderers();
            }
            return;
        }

        _inventoryRenderers[slotReceiver] = new HolsterHider(slotReceiver, IsHidden);
    }

    public void OnUnholster(InventorySlotReceiver slotReceiver)
    {
        if (!_inventoryRenderers.TryGetValue(slotReceiver, out var hider))
            return;

        hider.FetchRenderers(IsHidden);
    }

    public void OnAmmoChange()
    {
        foreach (var inventoryRenderersValue in _inventoryRenderers.Values)
        {
            inventoryRenderersValue.FetchRenderersIf<InventoryAmmoReceiverHider>(IsHidden);
        }
    }
    
    public void Update()
    {
        if (!_player.HasRig)
        {
            _lastAvatar = null;
            return;
        }
        
        foreach (var holsterHider in _inventoryRenderers.Values)
        {
            holsterHider.Update();
        }

        var rigManager = _player.RigRefs.RigManager;

        var avatar = rigManager.avatar;

        if (avatar == null)
        {
            _lastAvatar = null;
            return;
        }
        
        if (_lastAvatar != null && avatar == _lastAvatar && _avatarRenderers.AllValid() && _isHiddenInternal == IsHidden)
            return;

        _lastAvatar = avatar;

        PopulateRenderers();
    }
    public void RefreshRenderers()
    {
        PopulateRenderers();
    }
}