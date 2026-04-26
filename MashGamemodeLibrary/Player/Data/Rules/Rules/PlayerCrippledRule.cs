using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

/// <summary>
/// When true, remove any modded movement abilities from the player, such as the Spider-Man mods parkour
/// </summary>
public class PlayerCrippledRule : IPlayerRule
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
    
    public bool IsEqual(IPlayerRule playerRule)
    {
        if (playerRule is not PlayerCrippledRule other)
            return false;

        return _isEnabled == other._isEnabled;
    }

    public int GetHash()
    {
        return _isEnabled.GetHashCode();
    }
}