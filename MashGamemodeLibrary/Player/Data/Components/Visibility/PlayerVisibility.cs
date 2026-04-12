using System.Collections.Immutable;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Spectating.data.Components;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility.Parts.Holster;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;

public class PlayerVisibility : IPlayerExtender
{
    public NetworkPlayer Player { get; init; }
    private readonly IPlayerVisibility[] _playerVisibilities;

    public PlayerVisibility(NetworkPlayer player)
    {
        Player = player;
        
        _playerVisibilities = new IPlayerVisibility[]
        {
            new PlayerAvatarVisibility(player),
            new PlayerVoiceVisibility(player),
            new PlayerNametagVisibility(player),
            new PlayerHolsterVisibility()
        };
    }

    private void SetVisibility(bool isVisible)
    {
        foreach (var playerVisibility in _playerVisibilities)
        {
            playerVisibility.SetVisible(isVisible);
        }
    }

    public void OnRigChanged(RigManager? rigManager)
    {
        
    }
    
    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is not PlayerSpectatingRule spectatingRule) return;
        SetVisibility(!spectatingRule.IsSpectating);
    }
}