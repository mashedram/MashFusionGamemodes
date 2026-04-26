using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

public class PlayerSpectatingRule : IPlayerRule
{
    private bool _isSpectating;

    public bool IsSpectating
    {
        get => _isSpectating;
        set => _isSpectating = value;
    }

    /// <summary>
    /// This way spectating gets forced by the highest rule
    /// </summary>
    public bool IsEnabled => _isSpectating;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _isSpectating);
    }

    public int GetHash()
    {
        return _isSpectating.GetHashCode();
    }
}