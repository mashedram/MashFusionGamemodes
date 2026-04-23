using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.Player.Data.Rules.Networking;

internal record NetworkRuleBulkChangePacket(PlayerID PlayerID, IEnumerable<IPlayerRuleInstance> Rules);

internal class NetworkRuleBulkChangeEvent : GenericRemoteEvent<NetworkRuleBulkChangePacket>, ICatchup
{
    public NetworkRuleBulkChangeEvent(string name) : base(name, CommonNetworkRoutes.HostToRemote)
    {
    }

    public void Send(PlayerData playerData)
    {
        var packet = new NetworkRuleBulkChangePacket(playerData.PlayerID, playerData.RuleInstances);
        Relay(packet);
    }

    public void SendTo(PlayerID playerId, PlayerData playerData)
    {
        var data = new NetworkRuleBulkChangePacket(playerData.PlayerID, playerData.RuleInstances);
        Relay(data, playerId);
    }

    
    protected override int? GetSize(NetworkRuleBulkChangePacket data)
    {
        // Can't really predict this, sry
        return null;
    }

    protected override void Write(NetWriter writer, NetworkRuleBulkChangePacket data)
    {
        var rules = data.Rules.ToArray();
        writer.Write(data.PlayerID);
        writer.Write(rules.Length);
        foreach (var instance in rules)        
        {
            writer.Write(instance.Hash);
            instance.GetBaseRule().Serialize(writer);
        }
    }

    protected override void Read(byte smallId, NetReader reader)
    {
        var playerId = reader.ReadByte();
        var playerData = PlayerDataManager.GetPlayerData(playerId);
        if (playerData == null)
            return;

        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var ruleHash = reader.ReadUInt64();
            var ruleInstance = playerData.GetRuleByHash(ruleHash);

            ruleInstance?.Deserialize(reader, false);
        }
        
        // At last, notify all rules have been changed
        playerData.NotifyAllRules();
    }
    
    public void OnCatchup(PlayerID playerId)
    {
        PlayerDataManager.SendCatchup(playerId);
    }
}