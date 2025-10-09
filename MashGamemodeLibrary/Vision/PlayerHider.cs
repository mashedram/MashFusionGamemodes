using BoneLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Vision;

/// <summary>
/// For as much of a mess this code is, it does work!
/// </summary>

class RendererVisibility
{
    private bool _forceHide;
    private Renderer _renderer;
    
    public RendererVisibility(Renderer renderer)
    {
        if (!renderer)
            MelonLogger.Warning("Tried to create RendererVisibility with null renderer!");
        
        _renderer = renderer;
    }
    
    public void SetForceHide(bool hidden)
    {
        _forceHide = hidden;
        if (!_renderer)
            return;
        _renderer.enabled = !_forceHide;
    }
}

class MagazineHolster
{
    private bool _isHidden;
    private readonly HashSet<RendererVisibility> _renderers = new();
    private readonly InventoryAmmoReceiver _slotReceiver;
    
    public MagazineHolster(InventoryAmmoReceiver slotReceiver, bool isHidden)
    {
        _isHidden = isHidden;
        _slotReceiver = slotReceiver;
        PopulateRenderers();
    }
    
    public void PopulateRenderers()
    {
        _renderers.Clear();

        var magazines = _slotReceiver._magazineArts;
        if (magazines.Count == 0)
            return;
        
        foreach (var magazine in magazines)
        {
            if (magazine?.gameObject == null)
                continue;
            
            foreach (var renderer in magazine.gameObject.GetComponentsInChildren<Renderer>())
            {
                var visibility = new RendererVisibility(renderer);
                _renderers.Add(visibility);
                visibility.SetForceHide(_isHidden);
            }
        }
    }
    
    public void SetForceHide(bool hidden)
    {
        _isHidden = hidden;
        
        foreach (var renderer in _renderers)
        {
            renderer.SetForceHide(hidden);
        }
    }
}

class HolsteredItem
{
    private bool _isHidden;
    private readonly HashSet<RendererVisibility> _renderers = new();
    private readonly InventorySlotReceiver _slotReceiver;

    public HolsteredItem(InventorySlotReceiver slotReceiver, bool isHidden)
    {
        _isHidden = isHidden;
        _slotReceiver = slotReceiver;
        PopulateRenderers();
    }

    public bool Equals(InventorySlotReceiver other)
    {
        return _slotReceiver == other;
    }
    
    public void PopulateRenderers()
    {
        _renderers.Clear();
        
        if (!_slotReceiver._slottedWeapon)
            return;

        var gameObject = _slotReceiver._weaponHost.GetHostGameObject();
        if (!gameObject)
            return;
        
        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            var visibility = new RendererVisibility(renderer);
            _renderers.Add(visibility);
            visibility.SetForceHide(_isHidden);
        }
    }
    
    public void SetForceHide(bool hidden)
    {
        _isHidden = hidden;
        
        foreach (var renderer in _renderers)
        {
            renderer.SetForceHide(hidden);
        }
    }
}

internal class PlayerVisibilityState
{
    private bool _isSpecialHidden;
    private string? _lastAvatarBarcode;
    
    private readonly NetworkPlayer _player;
    private readonly Dictionary<string, bool> _hideOverwrites = new();
    
    private readonly HashSet<RendererVisibility> _avatarRenderers = new();
    private readonly HashSet<RendererVisibility> _inventoryRenderers = new();
    private readonly HashSet<RendererVisibility> _specialRenderers = new();
    
    private readonly Dictionary<string, HolsteredItem> _holsteredItems = new();
    private readonly Dictionary<string, MagazineHolster> _magazineHolsters = new();

    public bool IsHidden => _hideOverwrites.Any(e => e.Value);
    
    public PlayerVisibilityState(NetworkPlayer player, bool specialHidden)
    {
        _player = player;
        _isSpecialHidden = specialHidden;
        PopulateRenderers();
    }

    private void SetMute(bool muted)
    {
        var audioSource = _player.VoiceSource?.VoiceSource.AudioSource; 
        if (audioSource) 
        { 
            audioSource!.mute = muted;
        }
    }

    private void SetHeadUI(bool hidden)
    {
        
        _player.HeadUI.Visible = !hidden;
    }
    
    public void PopulateRenderers()
    {
        var rigManager = _player.RigRefs.RigManager;

        var avatarName = rigManager?.avatar?.name;
        if (avatarName == _lastAvatarBarcode)
            return;
        
        _lastAvatarBarcode = avatarName;
        
        _avatarRenderers.Clear();
        _inventoryRenderers.Clear();
        _specialRenderers.Clear();

        if (!_player.HasRig)
            return;
        
        foreach (var renderer in rigManager.avatar.GetComponentsInChildren<Renderer>())
        {
            var visibility = new RendererVisibility(renderer);
            _avatarRenderers.Add(visibility);
            visibility.SetForceHide(IsHidden);
        }
        
        foreach (var slotContainer in rigManager.inventory.bodySlots)
        {
            if (!slotContainer || !slotContainer.gameObject)
                continue;
            var renderers = slotContainer.gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var visibility = new RendererVisibility(renderer);
                _inventoryRenderers.Add(visibility);
                visibility.SetForceHide(IsHidden);
            }
        }
        
        foreach (var slotContainer in rigManager.inventory.specialItems)
        {
            if (!slotContainer || !slotContainer.gameObject)
                continue;
            var renderers = slotContainer.gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var visibility = new RendererVisibility(renderer);
                _specialRenderers.Add(visibility);
                visibility.SetForceHide(IsHidden || _isSpecialHidden);
            }
        }
    }
    
    public void SetSpecialsHidden(bool hidden)
    {
        _isSpecialHidden = hidden;
        
        foreach (var renderer in _specialRenderers)
        {
            renderer.SetForceHide(hidden || IsHidden);
        }
    }

    private void Refresh()
    {
        var hidden = IsHidden;
        
        foreach (var renderer in _avatarRenderers)
        {
            renderer.SetForceHide(hidden);
        }
        
        foreach (var renderer in _inventoryRenderers)
        {
            renderer.SetForceHide(hidden);
        }
        
        foreach (var renderer in _specialRenderers)
        {
            renderer.SetForceHide(hidden || _isSpecialHidden);
        }
        
        foreach (var item in _holsteredItems.Values)
        {
            item.SetForceHide(hidden);
        }
        
        foreach (var holster in _magazineHolsters.Values)
        {
            holster.SetForceHide(hidden);
        }

        SetMute(hidden);
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
    
    public void OnHolster(InventorySlotReceiver slotReceiver)
    {
        var name = slotReceiver.transform.parent.name;

        if (_holsteredItems.TryGetValue(name, out var item) && item.Equals(slotReceiver))
        {
            item.PopulateRenderers();
            item.SetForceHide(IsHidden);
            return;
        }
        
        _holsteredItems[name] = new HolsteredItem(slotReceiver, IsHidden);
    }
    
    public void OnUnholster(InventorySlotReceiver slotReceiver)
    {
        var name = slotReceiver.transform.parent.name;
        
        if (!_holsteredItems.Remove(name, out var item))
            return;

        item.SetForceHide(false);
    }

    public void UpdateAmmoHolster(InventoryAmmoReceiver inventoryAmmoReceiver)
    {
        var name = inventoryAmmoReceiver.transform.parent.name;

        if (_magazineHolsters.TryGetValue(name, out var holster))
        {
            holster.PopulateRenderers();
            holster.SetForceHide(IsHidden);
            return;
        }
        
        _magazineHolsters[name] = new MagazineHolster(inventoryAmmoReceiver, IsHidden);
    }
}

public static class PlayerHider
{
    private static bool _hideSpecials;
    private static Dictionary<byte, PlayerVisibilityState> _playerStates = new();

    public static void Register()
    {
        Hooking.OnSwitchAvatarPostfix += OnAvatarChanged;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }
    
    private static IEnumerable<PlayerVisibilityState> GetOrCreateAllStates()
    {
        return NetworkPlayer.Players.Select(player => GetOrCreateState(player.PlayerID)).OfType<PlayerVisibilityState>();
    }
    
    private static PlayerVisibilityState? GetOrCreateState(byte playerId)
    {
        if (_playerStates.TryGetValue(playerId, out var state)) return state;

        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
        {
            MelonLogger.Error("Failed to update player visibility, could not get or create state. Is the player connected?");
            return null;
        }
            
        state = new PlayerVisibilityState(player, _hideSpecials);
        _playerStates[playerId] = state;

        return state;
    }
    
    private static void OnAvatarChanged(Avatar avatar)
    {
        var rigManager = avatar.GetComponentInParent<RigManager>();
        if (!rigManager)
            return;
        
        if (!NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
            return;
        
        var state = GetOrCreateState(player.PlayerID);

        state?.PopulateRenderers();
    }
    
    private static void OnPlayerLeft(PlayerID playerId)
    {
        _playerStates.Remove(playerId);
    }
    
    public static bool IsHidden(this PlayerID playerID)
    {
        return !_playerStates.TryGetValue(playerID, out var state) || state.IsHidden;
    }

    public static void SetHidden(this PlayerID playerID, string key, bool hidden)
    {
        var state = GetOrCreateState(playerID);

        state?.SetHidden(key, hidden);
    }

    public static void HideAllSpecials()
    {
        _hideSpecials = true;
        
        foreach (var state in GetOrCreateAllStates())
        {
            state.SetSpecialsHidden(true);
        }
    }
    
    public static void UnhideAll()
    {
        _hideSpecials = false;
        
        foreach (var state in _playerStates.Values)
        {
            state.Reset();
        }
    }

    internal static void OnHolster(InventorySlotReceiver receiver)
    {
        if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var networkEntity))
            return;

        var id = (byte)networkEntity.ID;
        GetOrCreateState(id)?.OnHolster(receiver);
    }
    
    internal static void OnUnholster(InventorySlotReceiver receiver)
    {
        if (!InventorySlotReceiverExtender.Cache.TryGet(receiver, out var networkEntity))
            return;

        var id = (byte)networkEntity.ID;
        GetOrCreateState(id)?.OnUnholster(receiver);
    }

    public static void UpdateAmmoHolster(InventoryAmmoReceiver inventoryAmmoReceiver)
    {
        if (!InventoryAmmoReceiverExtender.Cache.TryGet(inventoryAmmoReceiver, out var networkEntity))
            return;

        var id = (byte)networkEntity.ID;
        GetOrCreateState(id)?.UpdateAmmoHolster(inventoryAmmoReceiver);
    }
}