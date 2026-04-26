using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules;

/// <summary>
/// Player Rules are modifiable by gamemodes, and ensure that gamemodes exit correctly and cannot interact with each other weirdly.
/// </summary>
public interface IPlayerRule : INetSerializable
{
    /// <summary>
    /// Whether this rule is currently enabled. If false, the rule will not be applied to the player, and will not be ticked.
    /// </summary>
    bool IsEnabled { get; }
    
    int GetHash();
}