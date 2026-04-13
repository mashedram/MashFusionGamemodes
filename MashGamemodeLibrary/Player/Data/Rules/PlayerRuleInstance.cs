using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Rules;

public class PlayerRuleInstance<TRule> : IPlayerRuleInstance where TRule : class, IPlayerRule, new()
{
    public delegate void ModifyRuleDelegate(TRule rule);
    private readonly PlayerData _playerData;
    
    // The rule of the host, used when we are the host and is sent over the network
    private TRule _localRule = new TRule();
    // The rule to use when we are the client
    private TRule _networkedRule = new TRule();
    
    // The hash of the rule type, used for networking
    public ulong Hash { get; }

    public PlayerRuleInstance(PlayerData playerData)
    {
        _playerData = playerData;
        Hash = typeof(TRule).GetStableHash();
    }
    
    public void Modify(ModifyRuleDelegate modifier)
    {
        if (NetworkInfo.IsClient)
            return;
        
        modifier(_localRule);
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
    
    public void Reset()
    {
        _localRule = new TRule();
        NotifyChange();
    }

    public void Deserialize(NetReader reader)
    {
        _networkedRule.Serialize(reader);
        NotifyChange();
    }
}