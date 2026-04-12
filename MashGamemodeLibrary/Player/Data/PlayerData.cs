using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.Player.Data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Components;
using MashGamemodeLibrary.Player.Spectating.data.Components.Colliders;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Visibility;
using MashGamemodeLibrary.Player.Spectating.data.Components.VisualOverlay;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data;

internal record NetworkRulePacket(PlayerID PlayerID, ulong RuleHash)
{
    
}

public class PlayerData
{
    // Instance Data
    
    public NetworkPlayer Player { get; }
    public RigManager? RigManager { get; private set; }

    private readonly Dictionary<Type, IPlayerExtender> _extenderCache = new();
    private readonly Dictionary<Type, IPlayerRuleInstance> _ruleInstanceCache = new();
    private readonly Dictionary<ulong, IPlayerRuleInstance> _ruleHashCache = new();
    
    public IEnumerable<IPlayerExtender> Extenders => _extenderCache.Values;
    public IEnumerable<IPlayerRuleInstance> RuleInstances => _ruleInstanceCache.Values;
    
    // Networking
    private static readonly GenericRemoteEvent<NetworkRulePacket> RuleChangeEvent = new("MGL_Player_RuleChange", NetworkEventImportance.Unreliable, NetworkEventDirection.ServerToClients);
    
    public PlayerData(NetworkPlayer player)
    {
        Player = player;
        
        // TODO: Automatic registries for these parts using factoryregistries
        AddExtender(new PlayerVisibility(Player));
        AddExtender(new PlayerCollisionsExtender(Player));
        
        if (player.PlayerID.IsMe)
            AddExtender(new LocalInteractionsExtender(Player));
        
        // Rules
        AddRule<PlayerSpectatingRule>();
    }

    private void AddExtender(IPlayerExtender playerExtender)
    {
        _extenderCache.Add(playerExtender.GetType(), playerExtender);
    }

    private void AddRule<TRule>() where TRule : class, IPlayerRule, new()
    {
        var instance = new PlayerRuleInstance<TRule>(this);
        _ruleInstanceCache.Add(typeof(TRule), instance);
        _ruleHashCache.Add(instance.Hash, instance);
    }
    
    internal void NotifyRuleChanged(IPlayerRule rule)
    {
        Extenders.ForEach(e => e.OnRuleChanged(rule));
    }

    public void OnRigChanged(RigManager rigManager)
    {
        if (rigManager == RigManager)
            return;
        
        Extenders.ForEach(e => e.OnRigChanged(rigManager));
    }
    
    // Accessors
    
    public bool CheckRule<TRule>(Func<TRule, bool> predicate) where TRule : class, IPlayerRule, new()
    {
        if (!_ruleInstanceCache.TryGetValue(typeof(TRule), out var ruleInstance))
            return false;

        if (ruleInstance is not PlayerRuleInstance<TRule> typedRuleInstance)
            return false;
        
        return predicate(typedRuleInstance.GetRule());
    }

    public PlayerRuleModifier<TRule> CreateModifier<TRule>(RuleModifierPriority priority) where TRule : class, IPlayerRule, new()
    {
        if (!_ruleInstanceCache.TryGetValue(typeof(TRule), out var ruleInstance))
            throw new ArgumentException("Rule not registered");
        
        if (ruleInstance is not PlayerRuleInstance<TRule> typedRuleInstance)
            throw new Exception("This should not happen.");

        return typedRuleInstance.GetModifier(priority);
    }
}