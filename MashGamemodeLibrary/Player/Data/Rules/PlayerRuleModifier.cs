using MashGamemodeLibrary.Player.Data.Rules;

namespace MashGamemodeLibrary.Player.Spectating.data.Rules;

/// <summary>
/// TRule 
/// </summary>
/// <typeparam name="TRule"></typeparam>
public class PlayerRuleModifier<TRule> where TRule : class, IPlayerRule, new()
{
    public delegate void ModifierDelegate(TRule rule);
    public RuleModifierPriority Priority { get; }

    private readonly PlayerRuleInstance<TRule> _ruleInstance;
    public TRule Rule { get; }

    public PlayerRuleModifier(PlayerRuleInstance<TRule> ruleInstance, RuleModifierPriority priority)
    {
        _ruleInstance = ruleInstance;
        Rule = new TRule();
        Priority = priority;
    }

    ~PlayerRuleModifier()
    {
        _ruleInstance.DropModifier(this);
    }

    public bool IsEnabled()
    {
        return Rule.IsEnabled;
    }

    public void Modify(ModifierDelegate action)
    {
        action(Rule);
        _ruleInstance.NotifyChange();
    }
}