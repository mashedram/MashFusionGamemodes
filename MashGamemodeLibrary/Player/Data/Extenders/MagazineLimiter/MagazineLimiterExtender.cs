using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.MagazineLimiter;

public class MagazineLimiterExtender : IPlayerExtender
{
    private int? _magazineLimit;
    public int MagazinesUsed { get; private set; }
    
    public void UseMagazine()
    {
        MagazinesUsed++;
    }

    public bool CanUseMagazine()
    {
        if (_magazineLimit == null)
            return true;
        
        return MagazinesUsed < _magazineLimit;
    }
    
    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        // No-Op
    }
    
    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is PlayerMagazineLimitRule magazineLimitRule)
            _magazineLimit = magazineLimitRule.MagazineLimit;
    }
    
    public void OnEvent(IPlayerEvent playerEvent)
    {
        
    }
}