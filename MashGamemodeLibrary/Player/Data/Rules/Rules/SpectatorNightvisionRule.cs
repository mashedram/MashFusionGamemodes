using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Rules.Rules;

/// <summary>
/// When true, remove any modded movement abilities from the player, such as the Spider-Man mods parkour
/// </summary>
public class SpectatorNightvisionRule : IPlayerRule
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