using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Data.Components.Colliders.Caches;
using MashGamemodeLibrary.Player.Spectating.data.Colliders;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;
using MashGamemodeLibrary.Player.Spectating.data.Rules;
using MashGamemodeLibrary.Player.Spectating.data.Rules.Rules;

namespace MashGamemodeLibrary.Player.Spectating.data.Components.Colliders;

public class PlayerCollisionsExtender : IPlayerExtender
{
    private NetworkPlayer _player;
    private CachedPhysicsRig? _cachedPhysicsRig;
    
    public PlayerCollisionsExtender(NetworkPlayer player)
    {
        _player = player;
    }

    public void OnRigChanged(RigManager? rigManager)
    {
    }
    
    public void OnRuleChanged(IPlayerRule rule)
    {
        if (rule is not PlayerSpectatingRule spectatingRule) return;
        CachedColliderCache.GetPlayerCollider(_player.PlayerID)?.SetColliding(!spectatingRule.IsSpectating);
    }
}