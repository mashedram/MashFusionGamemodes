using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;

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

    public IEnumerable<Type> RuleTypes => new[]
    {
        typeof(PlayerSpectatingRule),
        typeof(SpectatorNightvisionRule)
    };
    public void OnRuleChanged(PlayerData data)
    {
        var isSpectating = data.CheckRule<PlayerSpectatingRule>(p => p.IsSpectating);
        var hasNightVision = data.CheckRule<SpectatorNightvisionRule>(p => p.IsEnabled);
        
        SetVisibility(!isSpectating);
        ToggleNightVision(hasNightVision && isSpectating);
    }

    public IEnumerable<Type> EventTypes => new[]
    {
        typeof(AvatarChangedEvent),
        typeof(PlayerRuleChangedEvent)
    };
    public void OnEvent(IPlayerEvent playerEvent)
    {
        switch (playerEvent)
        {
            // When the avatar changes, we need to update the visibility of the new avatar
            // If the spectator state of the local player changes, we need to update the visibility of all players
            case AvatarChangedEvent:
            case PlayerRuleChangedEvent { Rule: PlayerSpectatingRule, Player: var playerID } when playerID.Equals(PlayerIDManager.LocalID):
                SetVisibility(_isVisible);
                break;
        }

    }
}