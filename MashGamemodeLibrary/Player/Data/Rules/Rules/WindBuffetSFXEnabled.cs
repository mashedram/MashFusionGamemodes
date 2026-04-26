using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

/// <summary>
/// Hides the wind buffet SFX when not the local player
/// </summary>
public class WindBuffetSFXEnabled : IPlayerRule
{
    // Default to true
    private bool _isEnabled = true;

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