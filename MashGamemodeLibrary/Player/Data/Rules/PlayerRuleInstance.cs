using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Rules;

internal class RuleModifierCompare<TRule> : IComparer<PlayerRuleModifier<TRule>> where TRule : class, IPlayerRule, new()
{
    public int Compare(PlayerRuleModifier<TRule>? x, PlayerRuleModifier<TRule>? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (y is null) return 1;
        if (x is null) return -1;
        return x.Priority.CompareTo(y.Priority);
    }
}

public class PlayerRuleInstance<TRule> : IPlayerRuleInstance where TRule : class, IPlayerRule, new()
{
    private readonly PlayerData _playerData;
    private readonly TRule _defaultRule = new TRule();
    private readonly SortedSet<PlayerRuleModifier<TRule>> _modifiers = new(new RuleModifierCompare<TRule>());
    
    private TRule? _activeRule;
    public ulong Hash { get; private init;  }
    
    public PlayerRuleInstance(PlayerData playerData)
    {
        _playerData = playerData;
        Hash = typeof(TRule).GetStableHash();
    }

    public PlayerRuleModifier<TRule> GetModifier(RuleModifierPriority priority)
    {
        var modifier = new PlayerRuleModifier<TRule>(this, priority);
        _modifiers.Add(modifier);
        return modifier;
    }
    
    internal void DropModifier(PlayerRuleModifier<TRule> playerRuleModifier)
    {
        _modifiers.Remove(playerRuleModifier);
    }
    
    public void NotifyChange()
    {
        // Calculate the active rule
        var activeRule = _defaultRule;
        foreach (var playerRuleModifier in _modifiers)
        {
            if (!playerRuleModifier.IsEnabled())
                continue;

            activeRule = playerRuleModifier.Rule;
            break;
        }
        
        if (Equals(activeRule, _activeRule))
            return;
        
        _activeRule = activeRule;
        _playerData.NotifyRuleChanged(_activeRule);
    }
    
    // Accessors

    public TRule GetRule()
    {
        return _defaultRule;
    }
    
}