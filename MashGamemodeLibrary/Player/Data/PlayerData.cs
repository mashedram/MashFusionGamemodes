using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.Player.Data.Components;
using MashGamemodeLibrary.Player.Data.Components.Visibility;
using MashGamemodeLibrary.Player.Data.Events;
using MashGamemodeLibrary.Player.Data.Events.Callers;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Extenders;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders;
using MashGamemodeLibrary.Player.Data.Extenders.LocalInteractions;
using MashGamemodeLibrary.Player.Data.Extenders.Visibility;
using MashGamemodeLibrary.Player.Data.Rules;
using MashGamemodeLibrary.Player.Data.Rules.Networking;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data;

internal record NetworkRulePacket(PlayerID PlayerID, ulong RuleHash)
{

}

[RequireStaticConstructor]
public class PlayerData : IEventReceiver
{
    // Caches
    private static readonly IBehaviourCache<IPlayerRuleChanged> PlayerRuleChangedCache = BehaviourManager.CreateCache<IPlayerRuleChanged>();
    
    // Registries
    private static readonly FactoryTypedRegistry<IPlayerExtender> ExtenderRegistry = new();
    private static readonly FactoryTypedRegistry<IPlayerRule> RuleRegistry = new();
    private static readonly FactoryTypedRegistry<IEventCaller> EventCallerRegistry = new();

    public static void Register<TMod>()
    {
        ExtenderRegistry.RegisterAll<TMod>();
        RuleRegistry.RegisterAll<TMod>();
        EventCallerRegistry.RegisterAll<TMod>();
    }

    // Instance Data

    public PlayerID PlayerID { get; init; }
    public NetworkPlayer? NetworkPlayer { get; private set; }

    private readonly Dictionary<Type, IPlayerExtender> _extenderCache = new();
    private readonly Dictionary<Type, IPlayerRuleInstance> _ruleInstanceCache = new();
    private readonly Dictionary<ulong, IPlayerRuleInstance> _ruleHashCache = new();
    private readonly Dictionary<Type, IEventCaller> _eventCallerCache = new();

    public IEnumerable<IPlayerExtender> Extenders => _extenderCache.Values;
    public IEnumerable<IPlayerRuleInstance> RuleInstances => _ruleInstanceCache.Values;
    public IEnumerable<IEventCaller> EventCallers => _eventCallerCache.Values;

    // Networking
    private static readonly NetworkRuleChangeEvent NetworkRuleChangeEvent = new("PlayerData.RuleChange");
    private static readonly NetworkRuleCatchupEvent NetworkRuleCatchupEvent = new("PlayerData.RuleCatchup");

    public PlayerData(PlayerID playerID)
    {
        PlayerID = playerID;

        // Extenders
        ExtenderRegistry.GetAll().ForEach(AddExtender);

        // Rules
        RuleRegistry.GetAllTypes().ForEach(AddRule);

        // Events
        EventCallerRegistry.GetAll().ForEach(AddEventCaller);
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

    private void AddRule(Type ruleType)
    {
        var instance = (IPlayerRuleInstance)Activator.CreateInstance(typeof(PlayerRuleInstance<>).MakeGenericType(ruleType), this)!;
        _ruleInstanceCache.Add(ruleType, instance);
        _ruleHashCache.Add(instance.Hash, instance);
    }

    private void AddEventCaller(IEventCaller eventCaller)
    {
        _eventCallerCache.Add(eventCaller.GetType(), eventCaller);
    }

    internal void NotifyRuleChanged(IPlayerRuleInstance ruleInstance)
    {
        var rule = ruleInstance.GetBaseRule();
        Extenders.ForEach(e => e.OnRuleChanged(rule));
        PlayerDataManager.CallEventOnAll(new OtherPlayerRuleChangedEvent(PlayerID, rule));
        if (NetworkPlayer != null)
            PlayerRuleChangedCache.ForEach(e => e.OnPlayerRuleChanged(NetworkPlayer, rule));

        Executor.RunIfHost(() =>
        {
            NetworkRuleChangeEvent.Send(PlayerID, ruleInstance);
        });
    }

    public void OnRigCreated(NetworkPlayer player, RigManager rigManager)
    {
        NetworkPlayer = player;
        
        Extenders.ForEach(e => e.OnPlayerChanged(player, rigManager));
        EventCallers.ForEach(e => e.OnEnable(this, player));
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

    public IPlayerRuleInstance? GetRuleByHash(ulong hash)
    {
        return _ruleHashCache.GetValueOrDefault(hash);

    }

    public PlayerRuleInstance<TRule> GetRuleInstance<TRule>() where TRule : class, IPlayerRule, new()
    {
        if (!_ruleInstanceCache.TryGetValue(typeof(TRule), out var ruleInstance))
            throw new KeyNotFoundException($"Rule of type {typeof(TRule)} not found for player {PlayerID}");

        return (ruleInstance as PlayerRuleInstance<TRule>)!;
    }

    public void Reset()
    {
        foreach (var playerRuleInstance in RuleInstances)
        {
            playerRuleInstance.Reset();
        }
    }

    public void SendCatchup(PlayerID playerID)
    {
        NetworkRuleCatchupEvent.SendCatchup(playerID, this);
    }

    public void ReceiveEvent(IPlayerEvent playerEvent)
    {
        Extenders.ForEach(e => e.OnEvent(playerEvent));
    }
}