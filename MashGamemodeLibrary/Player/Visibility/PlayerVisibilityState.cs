using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Vision.Holster;
using MashGamemodeLibrary.Vision.Holster.Receivers;

namespace MashGamemodeLibrary.Vision;

internal class PlayerVisibilityState
{
    private readonly RenderSet _avatarRenderers = new();

    private readonly Dictionary<Handedness, RenderSet> _heldItems = new();
    private readonly Dictionary<string, bool> _hideOverwrites = new();
    private readonly Dictionary<InventoryHandReceiver, HolsterHider> _inventoryRenderers = new();

    private readonly NetworkPlayer _player;
    private readonly RenderSet _specialRenderers = new();

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

            _specialRenderers.Add(slotContainer.gameObject);
        }

        var hands = new[] { rigManager.physicsRig.leftHand, rigManager.physicsRig.rightHand };
        foreach (var hand in hands) PopulateHand(new GrabData(hand));
    }

    public void SetSpecialsHidden(bool hidden)
    {
        _isSpecialHidden = hidden;

        _specialRenderers.SetHidden(_isSpecialHidden);
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

        if (!_specialRenderers.SetHidden(_isHiddenInternal))
        {
            return false;
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
            inventoryRenderersValue.UpdateIf<InventoryAmmoReceiverHider>(IsHidden);
        }
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
        
        if (_lastAvatar != null && avatar == _lastAvatar && _isHiddenInternal == IsHidden)
            return;

        _lastAvatar = avatar;

        PopulateRenderers();
    }
    public void RefreshRenderers()
    {
        PopulateRenderers();
    }
}