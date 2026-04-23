using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Data.Events.Data;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders;

public class PlayerCollisionsExtender : IPlayerExtender
{
    private bool _isColliding = true;
    private NetworkPlayer? _networkPlayer;
    private CachedPhysicsRig? _cachedPhysicsRig;
    
    private CachedPhysicsRig? GetPhysicsRig()
    {
        if (_networkPlayer == null)
            return null;

        if (_cachedPhysicsRig is { IsValid: true })
            return _cachedPhysicsRig;

        if (!_networkPlayer.HasRig)
            return null;

        _cachedPhysicsRig = new CachedPhysicsRig(_networkPlayer.RigRefs.RigManager.physicsRig);
        return _cachedPhysicsRig;
    }

    private void SetColliding(bool isColliding)
    {
        _isColliding = isColliding;

        GetPhysicsRig()?.SetColliding(_isColliding);
    }

    public void OnPlayerChanged(NetworkPlayer networkPlayer, RigManager rigManager)
    {
        _networkPlayer = networkPlayer;
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