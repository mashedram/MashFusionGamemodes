using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Patches;
using MashGamemodeLibrary.Player.Data.Events;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MashGamemodeLibrary.Player.Data.Extenders.LocalInteractions;

public class LocalInteractionsExtender : IPlayerExtender
{
    private NetworkPlayer? _player;
    
    private bool _areInteractionsEnabled = true;
    private GameObject? _overlayObject;

    private GameObject GetOverlayObject()
    {
        if (_overlayObject != null) return _overlayObject;

        _overlayObject = new GameObject("SpectatorEffect");

        var volume = _overlayObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10;
        volume.weight = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = profile;

        var colorAdjustments = profile.Add<ColorAdjustments>(true);
        colorAdjustments.saturation.value = -100f;

        return _overlayObject;
    }
    
    private void SetInteractions(bool areInteractionsEnabled)
    {
        _areInteractionsEnabled = areInteractionsEnabled;

        if (_player?.PlayerID == null)
            return;
        // Local Only extender
        if (!_player.PlayerID.IsMe)
            return;
        if (!_player.HasRig)
            return;

        GetOverlayObject().SetActive(!_areInteractionsEnabled);
        
        var rigManager = _player.RigRefs.RigManager;
        if (!_areInteractionsEnabled)
            Loadout.Loadout.ClearPlayerLoadout(rigManager);
        
        LocalControls.DisableInteraction = !_areInteractionsEnabled;
        LocalControls.DisableInventory = !_areInteractionsEnabled;
        LocalControls.DisableAmmoPouch = !_areInteractionsEnabled;
        DevToolsPatches.CanSpawn = _areInteractionsEnabled;
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        if (!networkPlayer.PlayerID.IsMe)
            return;
        
        _player = networkPlayer;
        SetInteractions(_areInteractionsEnabled);
    }

    public void OnRuleChanged(IPlayerRule rule)
    {
        if (_player == null)
            return;
        
        if (!_player.PlayerID.IsMe)
            return;
        
        if (rule is not PlayerSpectatingRule spectatingRule)
            return;

        SetInteractions(!spectatingRule.IsSpectating);
    }
    
    public void OnEvent(IPlayerEvent playerEvent)
    {
        // No-Op
    }
}