using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Player.Data.Components;
using MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Events;
using MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
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
        if (playerEvent is AvatarChangedEvent)
            SetVisibility(_isVisible);
    }
}