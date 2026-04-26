using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

/// <summary>
/// Hide the player, even if they aren't spectating.
/// </summary>
public class ForceHideRule : IPlayerRule
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

    public int GetHash()
    {
        return _isEnabled.GetHashCode();
    }
}