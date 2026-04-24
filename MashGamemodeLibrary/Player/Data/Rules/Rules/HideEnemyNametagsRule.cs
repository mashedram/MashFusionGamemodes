using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

/// <summary>
/// When true, hides all nametags of players on other teams
/// </summary>
public class HideEnemyNametagsRule : IPlayerRule
{
    private bool _isEnabled;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _isEnabled);
    }
}