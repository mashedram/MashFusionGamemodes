using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Components;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders;

public class PlayerCollisionsExtender : IPlayerExtender
{
    private bool _isColliding = true;
    private CachedPhysicsRig? _cachedPhysicsRig;
    
    private void SetColliding(bool isColliding)
    {
        _isColliding = isColliding;
        
        if (_cachedPhysicsRig == null)
            return;
        
        PhysicsRigCollisionEditScheduler.ScheduleEdit(_cachedPhysicsRig, isColliding);
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _cachedPhysicsRig = CachedMarrowEntities.GetCachedPhysicsRig(rigManager.physicsRig);
        SetColliding(_isColliding);
    }
    
    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is not PlayerSpectatingRule spectatingRule) return;
        
        _isColliding = !spectatingRule.IsSpectating;
        SetColliding(_isColliding);
    }
}