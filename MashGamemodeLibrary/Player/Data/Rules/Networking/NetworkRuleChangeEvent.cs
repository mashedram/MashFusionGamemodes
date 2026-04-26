using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.Player.Data.Rules.Networking;

internal record NetworkRuleChangePacket(PlayerID PlayerID, ulong RuleHash, IPlayerRule Rule);

internal class NetworkRuleChangeEvent : GenericRemoteEvent<NetworkRuleChangePacket>
{
    public NetworkRuleChangeEvent(string name) : base(name, CommonNetworkRoutes.HostToRemote)
    {
    }

    public void Send(PlayerID playerId, IPlayerRuleInstance ruleInstance)
    {
        var packet = new NetworkRuleChangePacket(playerId, ruleInstance.Hash, ruleInstance.GetBaseRule());
        Relay(packet);
    }

    protected override int? GetSize(NetworkRuleChangePacket data)
    {
        // Can't really predict this, sry
        return null;
    }

    protected override void Write(NetWriter writer, NetworkRuleChangePacket data)
    {
        writer.Write(data.PlayerID);
        writer.Write(data.RuleHash);
        data.Rule.Serialize(writer);
    }

    protected override void Read(byte smallId, NetReader reader)
    {
        var playerId = reader.ReadByte();
        var playerData = PlayerDataManager.GetPlayerData(playerId);
        if (playerData == null)
            return;

        var ruleHash = reader.ReadUInt64();
        var ruleInstance = playerData.GetRuleByHash(ruleHash);

        ruleInstance?.Deserialize(reader);
    }
}