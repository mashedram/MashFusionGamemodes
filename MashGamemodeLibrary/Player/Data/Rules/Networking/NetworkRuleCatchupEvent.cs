using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Player.Data.Rules;
using MashGamemodeLibrary.Player.Data.Rules.Networking;

namespace MashGamemodeLibrary.Player.Data.Rules.Networking;

public record NetworkRuleCatchupEventArgs(PlayerID PlayerID, IEnumerable<IPlayerRuleInstance> RuleInstances);

public class NetworkRuleCatchupEvent : GenericRemoteEvent<NetworkRuleCatchupEventArgs>, ICatchup
{
    public NetworkRuleCatchupEvent(string name) : base(name, CommonNetworkRoutes.HostToRemote)
    {
    }

    protected override int? GetSize(NetworkRuleCatchupEventArgs data)
    {
        return null;
    }

    protected override void Write(NetWriter writer, NetworkRuleCatchupEventArgs data)
    {
        var rules = data.RuleInstances.ToArray();
        writer.Write(data.PlayerID);
        writer.Write(rules.Length);
        foreach (var ruleInstance in rules)
        {
            writer.Write(ruleInstance.Hash);
            ruleInstance.GetBaseRule().Serialize(writer);
        }
    }

    protected override void Read(byte smallId, NetReader reader)
    {
        var playerId = reader.ReadByte();
        var playerData = PlayerDataManager.GetPlayerData(playerId);
        if (playerData == null)
            return;

        var ruleCount = reader.ReadInt32();
        for (var i = 0; i < ruleCount; i++)
        {
            var ruleHash = reader.ReadUInt64();
            var ruleInstance = playerData.GetRuleByHash(ruleHash);
            ruleInstance?.Deserialize(reader);
        }
    }

    public void OnCatchup(PlayerID playerId)
    {
        PlayerDataManager.SendCatchup(playerId);
    }

    public void SendCatchup(PlayerID playerId, PlayerData playerData)
    {
        var data = new NetworkRuleCatchupEventArgs(playerData.PlayerID, playerData.RuleInstances);
        Relay(data, playerId);
    }
}