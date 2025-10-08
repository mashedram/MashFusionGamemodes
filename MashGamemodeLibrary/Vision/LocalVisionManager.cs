using BoneLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MelonLoader;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace MashGamemodeLibrary.Vision;

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

internal class PlayerVisibilityState
{
    private bool _isSpecialHidden;
    private string? _lastAvatarBarcode;
    private readonly NetworkPlayer _player;
    private readonly HashSet<RendererVisibility> _avatarRenderers = new();
    private readonly HashSet<RendererVisibility> _inventoryRenderers = new();
    private readonly HashSet<RendererVisibility> _specialRenderers = new();
    
    public bool IsForceHidden { get; private set; }
    
    public PlayerVisibilityState(NetworkPlayer player, bool specialHidden)
    {
        _player = player;
        _isSpecialHidden = specialHidden;
        PopulateRenderers();
    }
    
    public void PopulateRenderers()
    {
        var rigManager = _player.RigRefs.RigManager;

        if (rigManager.avatar.name == _lastAvatarBarcode)
            return;
        
        _lastAvatarBarcode = rigManager.avatarID;
        
        _avatarRenderers.Clear();
        _inventoryRenderers.Clear();
        _specialRenderers.Clear();

        if (!_player.HasRig)
            return;
        
        foreach (var renderer in rigManager.avatar.GetComponentsInChildren<Renderer>())
        {
            var visibility = new RendererVisibility(renderer);
            _avatarRenderers.Add(visibility);
            visibility.SetForceHide(IsForceHidden);
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
                visibility.SetForceHide(IsForceHidden);
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
                visibility.SetForceHide(IsForceHidden || _isSpecialHidden);
            }
        }
    }
    
    public void SetSpecialsHidden(bool hidden)
    {
        _isSpecialHidden = hidden;
        
        foreach (var renderer in _specialRenderers)
        {
            renderer.SetForceHide(hidden || IsForceHidden);
        }
    }
    
    public void SetForceHide(bool hidden)
    {
        IsForceHidden = hidden;
        
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
    }

    public void Reset()
    {
        _isSpecialHidden = false;

        SetForceHide(false);
    }
}

public static class LocalVisionManager
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
    
    private static PlayerVisibilityState? GetOrCreateState(PlayerID playerId)
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
        return !_playerStates.TryGetValue(playerID, out var state) || state.IsForceHidden;
    }

    public static void ForceHide(this PlayerID playerID, bool hidden)
    {
        var state = GetOrCreateState(playerID);

        state?.SetForceHide(hidden);
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
}