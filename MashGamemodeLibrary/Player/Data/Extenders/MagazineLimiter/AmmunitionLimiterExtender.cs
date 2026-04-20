using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.MagazineLimiter;

public class AmmunitionLimiterExtender : IPlayerExtender
{
    private int? _ammunitionLimit;
    public int AmmunitionUsed { get; private set; }
    
    public void UseMagazine(int amount)
    {
        AmmunitionUsed += amount;
    }

    public bool CanUseMagazine()
    {
        if (_ammunitionLimit == null)
            return true;
        
        return AmmunitionUsed < _ammunitionLimit;
    }
    
    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        // No-Op
    }
    
    public IEnumerable<Type> RuleTypes => new[]
    {
        typeof(PlayerAmmunitionLimitRule)
    };
    public void OnRuleChanged(PlayerData data)
    {
        var ammunitionLimitRule = data.GetRule<PlayerAmmunitionLimitRule>();
        _ammunitionLimit = ammunitionLimitRule.AmmunitionLimit;
        AmmunitionUsed = 0;
    }
    
    public void OnEvent(IPlayerEvent playerEvent)
    {
        
    }
}