using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Audio;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.WindBuffetSFX;

public class WindBuffetSFXExtender : IPlayerExtender
{
    private GameObject? _windBuffetSfxObject;
    
    private NetworkPlayer? _player;
    
    private bool _enabled;
    private bool _isSpectating;

    private void Update()
    {
        // Don't affect the local player
        if (_player?.PlayerID is { IsMe: true})
            return;
        
        var shouldBeEnabled = _enabled && (!_isSpectating || SpectatorExtender.IsLocalPlayerSpectating());
        if (_windBuffetSfxObject != null && _windBuffetSfxObject.activeSelf != shouldBeEnabled)
            _windBuffetSfxObject.SetActive(shouldBeEnabled);
    }

    private void ConfigureSfx()
    {
        if (_windBuffetSfxObject == null || _player == null)
            return;
        
        if (_player.PlayerID.IsMe)
            return;
        
        // If not the local player, assign the right sound group
        var sfx = _windBuffetSfxObject.GetComponent<Il2CppSLZ.Marrow.WindBuffetSFX>();
        if (sfx == null)
            return;
        
        var src = sfx._buffetSrc;
        if (src == null)
            return;
        
        src.outputAudioMixerGroup = Audio3dManager.ambience;
        src.spatialBlend = 1f;
        src.spatialize = true;
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _player = networkPlayer;
        _windBuffetSfxObject = rigManager.transform.Find("VRControllerRig/TrackingSpace/Headset/WindBuffetSFX")?.gameObject;
        
        ConfigureSfx();
        Update();
    }

    public IEnumerable<Type> RuleTypes => new[] { typeof(WindBuffetSFXEnabled), typeof(PlayerSpectatingRule) };
    public void OnRuleChanged(PlayerData data)
    {
        _enabled = data.CheckRule<WindBuffetSFXEnabled>(p => p.IsEnabled);
        _isSpectating = data.CheckRule<PlayerSpectatingRule>(p => p.IsSpectating);
        
        Update();
    }

    public IEnumerable<Type> EventTypes => new[] { typeof(PlayerRuleChangedEvent) };
    public void OnEvent(IPlayerEvent playerEvent)
    {
        switch (playerEvent)
        {
            case PlayerRuleChangedEvent { Rule: PlayerSpectatingRule, Player: var playerID2 } when playerID2.Equals(PlayerIDManager.LocalID):
                Update();
                break;
        }
    }
}