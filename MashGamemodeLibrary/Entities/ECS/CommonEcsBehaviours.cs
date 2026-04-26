using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.ECS;

[RequireStaticConstructor]
public static class CommonEcsBehaviours
{
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IEntityAttached> EntityAttachedCache = BehaviourManager.CreateCache<NetworkEntityAssociation, IEntityAttached>();
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IPlayerAttached> PlayerAttachedCache = BehaviourManager.CreateCache<NetworkEntityAssociation, IPlayerAttached>();

    private static readonly IBehaviourCache<IUpdate> UpdateCache = BehaviourManager.CreateCache<IUpdate>();
    private static readonly IBehaviourCache<IRemoved> RemovedCache = BehaviourManager.CreateCache<IRemoved>();

    static CommonEcsBehaviours()
    {
        EntityAttachedCache.OnAdded += (instance, component) =>
        {
            if (!instance.NetworkID.TryGetEntity(out var entity))
                return;
            var extender = entity.GetExtender<IMarrowEntityExtender>();
            if (extender == null)
                return;
            
            component.OnReady(entity, extender.MarrowEntity);
        };

        PlayerAttachedCache.OnAdded += (association, component) =>
        {
            var id = association.NetworkID.ID;
            if (id > PlayerIDManager.MaxPlayerID)
                return;
            var playerId = (byte)id;
            if (NetworkPlayerManager.TryGetPlayer(playerId, out var player))
                return;

            component.OnReady(player);
        };

        RemovedCache.OnRemoved += (component) =>
        {
            component.OnRemoved();
        };
    }

    internal static void Update(float delta)
    {
        UpdateCache.ForEach(behaviour => behaviour.Update(delta));
    }
}