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
    private bool _isVisible = true;
    private readonly IPlayerVisibility[] _playerVisibilities = {
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
        
        // If the local player is spectating, and this is not the local player, we want to keep them visible so they can see other spectators
        if (SpectatorExtender.IsLocalPlayerSpectating())
            return;
        
        foreach (var playerVisibility in _playerVisibilities)
        {
            playerVisibility.SetVisible(_isVisible);
        }
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
        if (rule is not PlayerSpectatingRule spectatingRule) return;
        SetVisibility(!spectatingRule.IsSpectating);
    }
    
    public void OnEvent(IPlayerEvent playerEvent)
    {
        switch (playerEvent)
        {
            case AvatarChangedEvent:
            case PlayerRuleChangedEvent { Rule: PlayerSpectatingRule }:
                SetVisibility(_isVisible);
                break;
        }

    }
}