using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Data.Components;
using MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Events;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility;

public class PlayerVisibility : IPlayerExtender
{
    public NetworkPlayer? Player { get; set; }
    private bool _hasNightVision = false;
    private bool _nightVisionEnabled = false;
    private bool _isVisible = true;

    private readonly IPlayerVisibility[] _playerVisibilities =
    {
        new PlayerAvatarVisibility(),
        new PlayerVoiceVisibility(),
        new PlayerNametagVisibility(),
        new PlayerHolsterVisibility(),
        new PlayerBodylogVisibility(),
        new PlayerWindbuffetVisibility()
    };

    private void SetVisibility(bool isVisible)
    {
        _isVisible = isVisible;
        if (Player == null)
            return;

        if (Player.PlayerID.IsMe)
            return;

        // If the local player is spectating, nobody should be hidden
        var visibleForLocalPlayer = isVisible || SpectatorExtender.IsLocalPlayerSpectating();
        
        foreach (var playerVisibility in _playerVisibilities)
        {
            playerVisibility.SetVisible(visibleForLocalPlayer);
        }
    }
    
    private void ToggleNightVision(bool enabled)
    {
        _hasNightVision = enabled;
        // Only toggle when its from us
        if (Player?.PlayerID is not { IsMe: true })
            return;
        
        // Check if we aren't fucking with other things
        if (_hasNightVision == _nightVisionEnabled)
            return;

        var shouldBeEnabled = _hasNightVision && !_isVisible;
        if (shouldBeEnabled == _nightVisionEnabled)
            return;
        
        _nightVisionEnabled = shouldBeEnabled;
        NightVisionHelper.Enabled = shouldBeEnabled;
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        Player = networkPlayer;
        _playerVisibilities.ForEach(p =>
        {
            p.OnPlayerChanged(networkPlayer, rigManager);
            p.SetVisible(_isVisible);
        });
    }

    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is PlayerSpectatingRule spectatingRule)
        {
            SetVisibility(!spectatingRule.IsSpectating);
            ToggleNightVision(_hasNightVision);
        }
        if (rule is SpectatorNightvisionRule spectatorNightvisionRule)
        {
            _hasNightVision = spectatorNightvisionRule.IsEnabled;
            ToggleNightVision(_hasNightVision);
        }
    }

    public void OnEvent(IPlayerEvent playerEvent)
    {
        switch (playerEvent)
        {
            case AvatarChangedEvent:
            case OtherPlayerRuleChangedEvent { Rule: PlayerSpectatingRule }:
                SetVisibility(_isVisible);
                break;
        }

    }
}