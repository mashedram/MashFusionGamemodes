using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Rules;

public interface IPlayerRuleInstance
{
    ulong Hash { get; }
    void Deserialize(NetReader reader, bool notify = true);
    IPlayerRule GetBaseRule();
    void Reset(bool notify = true);
}