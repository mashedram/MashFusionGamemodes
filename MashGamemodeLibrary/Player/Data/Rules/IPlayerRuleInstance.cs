using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player.Data.Rules;

public interface IPlayerRuleInstance
{
    ulong Hash { get; }
    void Deserialize(NetReader reader, bool notify = true);
    IPlayerRule GetBaseRule();
    void Reset(bool notify = true);
}