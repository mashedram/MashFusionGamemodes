using LabFusion.Network;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Rules;

public class PlayerRuleInstance<TRule> : IPlayerRuleInstance where TRule : class, IPlayerRule, new()
{
    public delegate void ModifyRuleDelegate(TRule rule);
    private readonly PlayerData _playerData;

    // The rule of the host, used when we are the host and is sent over the network
    private TRule _localRule = new();

    // The rule to use when we are the client
    private readonly TRule _networkedRule = new();

    // The hash of the rule type, used for networking
    public ulong Hash { get; }

    public PlayerRuleInstance(PlayerData playerData)
    {
        _playerData = playerData;
        Hash = typeof(TRule).GetStableHash();
    }

    public void Modify(ModifyRuleDelegate modifier)
    {
        if (!NetworkInfo.IsHost)
        {
            InternalLogger.Error("Player Rules can't be edited on the client");
            return;
        }

        var hash = _localRule.GetHash();
        modifier.Try(m => m(_localRule));
        
        // Check if the modification actually changed anything
        if (hash != _localRule.GetHash())
            NotifyChange();
    }

    public void NotifyChange()
    {
        _playerData.NotifyRuleChanged(this);
    }

    // Accessors

    public TRule GetRule()
    {
        if (NetworkInfo.IsClient)
            return _networkedRule;

        return _localRule;
    }

    public IPlayerRule GetBaseRule()
    {
        return GetRule();
    }

    public void Reset(bool notifyChange = true)
    {
        _localRule = new TRule();
        if (notifyChange)
            NotifyChange();
    }

    public void Deserialize(NetReader reader, bool notifyChange = true)
    {
        _networkedRule.Serialize(reader);
        if (notifyChange)
            NotifyChange();
    }
}