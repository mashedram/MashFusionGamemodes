using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Player.Data.Rules.Rules;

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

    public IEnumerable<Type> RuleTypes => new[]
    {
        typeof(PlayerSpectatingRule)
    };
    public void OnRuleChanged(PlayerData data)
    {
        var isSpectating = data.CheckRule<PlayerSpectatingRule>(p => p.IsSpectating);
        _isColliding = !isSpectating;
        SetColliding(_isColliding);
    }

    public void OnEvent(IPlayerEvent playerEvent)
    {
        // No-Op
    }
}