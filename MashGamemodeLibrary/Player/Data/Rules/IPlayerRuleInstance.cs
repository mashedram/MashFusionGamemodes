using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Rules;

public interface IPlayerRuleInstance
{
    ulong Hash { get; }
    void Deserialize(NetReader reader);
    IPlayerRule GetBaseRule();
}