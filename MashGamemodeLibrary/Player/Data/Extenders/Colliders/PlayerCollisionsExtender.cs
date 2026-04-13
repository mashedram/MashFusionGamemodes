using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Components;
using MashGamemodeLibrary.Player.Data.Events;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders;

public class PlayerCollisionsExtender : IPlayerExtender
{
    private bool _isColliding = true;
    private PhysicsRig? _cachedPhysicsRig;
    
    private void SetColliding(bool isColliding)
    {
        _isColliding = isColliding;
        
        if (_cachedPhysicsRig == null)
            return;
        
        PhysicsRigCache.GetRig(_cachedPhysicsRig)?.SetColliding(_isColliding);
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _cachedPhysicsRig = rigManager.physicsRig;
        SetColliding(_isColliding);
    }
    
    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is not PlayerSpectatingRule spectatingRule) return;
        
        _isColliding = !spectatingRule.IsSpectating;
        SetColliding(_isColliding);
    }
    
    public void OnEvent(IPlayerEvent playerEvent)
    {
        // No-Op
    }
}