using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Patches;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MashGamemodeLibrary.Player.Spectating.data.Components.VisualOverlay;

public class LocalInteractionsExtender : IPlayerExtender
{
    private NetworkPlayer _player;
    private RigManager? _rigManager;
    
    private bool _hasInteractions;
    private GameObject? _overlayObject;
    
    public LocalInteractionsExtender(NetworkPlayer player)
    {
        _player = player;
    }

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
    
    private void SetInteractions(bool hasInteractions)
    {
        GetOverlayObject().SetActive(hasInteractions);
        
        if (!_hasInteractions && _rigManager != null)
            Loadout.Loadout.ClearPlayerLoadout(_rigManager);
        
        LocalControls.DisableInteraction = !hasInteractions;
        LocalControls.DisableInventory = !hasInteractions;
        LocalControls.DisableAmmoPouch = !hasInteractions;
        DevToolsPatches.CanSpawn = hasInteractions;
     
        // TODO: Disable grabbing
        // TODO: I kind off want to revamp this system first
        // PlayerGrabManager.SetOverwrite(GrabOverwriteKey, state ? null : _ => false);
    }
    
    public void OnRigChanged(RigManager? rigManager)
    {
        if (!_player.PlayerID.IsMe)
            return;
        
        SetInteractions(_hasInteractions);
    }
    
    public void OnRuleChanged(IPlayerRule rule)
    {
        if (!_player.PlayerID.IsMe)
            return;
        
        if (rule is not PlayerSpectatingRule spectatingRule)
            return;

        _hasInteractions = spectatingRule.IsSpectating;
        SetInteractions(_hasInteractions);
    }
}