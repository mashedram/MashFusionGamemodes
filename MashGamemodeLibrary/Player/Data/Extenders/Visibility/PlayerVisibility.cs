using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Components;
using MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Data.Extenders.Visibility.Parts;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility;

public class PlayerVisibility : IPlayerExtender
{
    public NetworkPlayer? Player { get; init; }
    private readonly IPlayerVisibility[] _playerVisibilities;

    public PlayerVisibility()
    {
        _playerVisibilities = new IPlayerVisibility[]
        {
            new PlayerAvatarVisibility(),
            new PlayerVoiceVisibility(),
            new PlayerNametagVisibility(),
            new PlayerHolsterVisibility(),
            new PlayerBodylogVisibility()
        };
    }

    private void SetVisibility(bool isVisible)
    {
        foreach (var playerVisibility in _playerVisibilities)
        {
            playerVisibility.SetVisible(isVisible);
        }
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        if (!networkPlayer.HasRig)
            return;
        
        foreach (var playerVisibility in _playerVisibilities)
        {
            playerVisibility.OnPlayerChanged(networkPlayer, rigManager);
        }
    }

    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is not PlayerSpectatingRule spectatingRule) return;
        SetVisibility(!spectatingRule.IsSpectating);
    }
}